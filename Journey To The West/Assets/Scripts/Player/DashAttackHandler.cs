using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashAttackHandler : MonoBehaviour
{
    private enum DashState { Idle, Dashing }

    [SerializeField] private CursorReticle cursorReticle;
    [SerializeField] private TrailRenderer dashTrail;
    [SerializeField] private GameObject[] slashVFXPrefabs;

    [Header("Ghost Trail")]
    [SerializeField] private GameObject dashGhostPrefab;
    [SerializeField] private float ghostSpawnInterval = 0.03f;
    [SerializeField] private float ghostFadeDuration = 0.2f;
    [SerializeField] private Color ghostColor = new Color(1f, 1f, 1f, 0.5f);

    [Header("Wall Collision")]
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float wallStopOffset = 0.2f;

    private Rigidbody2D rb;
    private Animator animator;
    private GreedMeter greedMeter;
    private PlayerCombat playerCombat;

    private DashState currentState = DashState.Idle;
    private DashAttackSkill currentConfig;
    private Vector2 dashDestination;
    private List<IDamageable> snapshotTargets;
    private bool snapshotHadTargets;
    private float ghostSpawnTimer;
    private SpriteRenderer playerSprite;
    private float dashSnappedX;
    private float dashSnappedY;
    private Vector2 lastDashPos;
    private bool dashFirstTick;

    private float dashElapsed;
    private float dashMaxDuration;
    private int stuckFrameCount;

    private Coroutine shieldFlashRoutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        greedMeter = GetComponent<GreedMeter>();
        playerCombat = GetComponent<PlayerCombat>();
        playerSprite = GetComponent<SpriteRenderer>();
    }

    public bool IsDashing => currentState == DashState.Dashing;

    public bool IsLocked()
    {
        return currentState != DashState.Idle;
    }

    public Vector2 GetDashDirection()
    {
        return new Vector2(dashSnappedX, dashSnappedY);
    }

    public void SetReticleActive(bool active, DashAttackSkill config = null)
    {
        if (cursorReticle != null)
        {
            cursorReticle.gameObject.SetActive(active);
            if (active && config != null)
            {
                cursorReticle.SetDetectionRadius(config.detectionRadius);
                cursorReticle.SetConstraint(config.dashRange, transform);
            }
        }
    }

    public void ExecuteDash(DashAttackSkill config)
    {
        if (currentState != DashState.Idle) return;
        if (cursorReticle == null) return;

        // Snapshot targeting at activation time
        snapshotHadTargets = cursorReticle.HasTarget;
        snapshotTargets = cursorReticle.GetTargets();
        dashDestination = cursorReticle.GetWorldPosition();

        // Clamp destination if a wall is in the way
        Vector2 toTarget = dashDestination - rb.position;
        ContactFilter2D wallFilter = new ContactFilter2D();
        wallFilter.SetLayerMask(wallLayer);
        wallFilter.useLayerMask = true;
        wallFilter.useTriggers = false;
        RaycastHit2D[] hits = new RaycastHit2D[1];
        int hitCount = rb.Cast(toTarget.normalized, wallFilter, hits, toTarget.magnitude);
        if (hitCount > 0)
        {
            dashDestination = rb.position + toTarget.normalized * Mathf.Max(0f, hits[0].distance - wallStopOffset);
        }

        // Calculate max duration for guaranteed arrival
        float distance = Vector2.Distance(rb.position, dashDestination);
        dashMaxDuration = Mathf.Max(0.15f, (distance / config.dashSpeed) * 1.5f);
        dashElapsed = 0f;
        stuckFrameCount = 0;

        currentConfig = config;

        currentState = DashState.Dashing;

        // Face dash direction snapped to 8-way sectors and play attack animation
        Vector2 dashDir = (dashDestination - rb.position).normalized;
        float angle = Mathf.Atan2(dashDir.y, dashDir.x);
        float snapped = Mathf.Round(angle / (Mathf.PI / 4f)) * (Mathf.PI / 4f);
        dashSnappedX = Mathf.Round(Mathf.Cos(snapped));
        dashSnappedY = Mathf.Round(Mathf.Sin(snapped));

        animator.SetFloat("LastInputX", dashSnappedX);
        animator.SetFloat("LastInputY", dashSnappedY);
        animator.SetTrigger("Attack");
        animator.SetBool("isWalking", false);

        // Enable trail
        if (dashTrail != null)
            dashTrail.emitting = true;

        lastDashPos = rb.position;
        dashFirstTick = true;
        ghostSpawnTimer = 0f;
    }

    private void FixedUpdate()
    {
        if (currentState == DashState.Dashing)
            HandleDashing();
    }

    private void HandleDashing()
    {
        // Re-apply snapped direction every tick to prevent other scripts from overwriting
        animator.SetFloat("LastInputX", dashSnappedX);
        animator.SetFloat("LastInputY", dashSnappedY);
        animator.SetBool("isWalking", false);

        ghostSpawnTimer -= Time.fixedDeltaTime;
        if (ghostSpawnTimer <= 0f)
        {
            SpawnGhost();
            ghostSpawnTimer = ghostSpawnInterval;
        }

        dashElapsed += Time.fixedDeltaTime;

        rb.MovePosition(Vector2.MoveTowards(rb.position, dashDestination, currentConfig.dashSpeed * Time.fixedDeltaTime));

        // --- Arrival check (takes priority over stuck) ---
        if (Vector2.Distance(rb.position, dashDestination) < 0.1f)
        {
            if (dashTrail != null)
                dashTrail.emitting = false;
            HandleArrival();
            return;
        }

        // --- Stuck / timeout detection ---
        bool isStuck = false;
        if (!dashFirstTick && Vector2.Distance(rb.position, lastDashPos) < 0.01f)
        {
            stuckFrameCount++;
            isStuck = stuckFrameCount >= 2;
        }
        else
        {
            stuckFrameCount = 0;
        }
        dashFirstTick = false;
        lastDashPos = rb.position;

        bool timedOut = dashElapsed > dashMaxDuration;

        if (isStuck || timedOut)
        {
            if (snapshotHadTargets)
            {
                ForceArrival();
                return;
            }
            else
            {
                if (dashTrail != null)
                    dashTrail.emitting = false;
                CameraShakeManager.Instance?.Shake(0.15f);
                if (playerCombat != null)
                    playerCombat.SetSkillCooldown(currentConfig.whiffCooldown);
                EndDash();
                return;
            }
        }
    }

    private void HandleArrival()
    {
        if (snapshotHadTargets)
        {
            float damage = currentConfig.baseDashDamage * (greedMeter != null ? greedMeter.GetDamageMultiplier() : 1f);

            bool hitShield = false;
            foreach (IDamageable target in snapshotTargets)
            {
                if (target is MonoBehaviour mb && mb != null && mb.gameObject.activeInHierarchy)
                {
                    EnemyShield shield = mb.GetComponent<EnemyShield>();
                    bool hadShields = shield != null && shield.HasShields();
                    target.TakeDamage(damage);
                    if (hadShields)
                    {
                        hitShield = true;
                    }
                }
            }

            if (slashVFXPrefabs != null && slashVFXPrefabs.Length > 0)
            {
                GameObject prefab = slashVFXPrefabs[Random.Range(0, slashVFXPrefabs.Length)];
                if (prefab != null)
                {
                    float angle = Mathf.Atan2(dashSnappedY, dashSnappedX) * Mathf.Rad2Deg;
                    Instantiate(prefab, transform.position, Quaternion.Euler(0f, 0f, angle));
                }
            }

            CameraShakeManager.Instance?.Shake(0.5f);

            if (hitShield)
            {
                // Knockback: push player away from dash destination
                Vector2 knockbackDir = (rb.position - dashDestination).normalized;
                if (knockbackDir.sqrMagnitude < 0.01f)
                    knockbackDir = -new Vector2(dashSnappedX, dashSnappedY);

                float knockbackDist = 2f;
                ContactFilter2D wallFilter = new ContactFilter2D();
                wallFilter.SetLayerMask(wallLayer);
                wallFilter.useLayerMask = true;
                wallFilter.useTriggers = false;
                RaycastHit2D[] hits = new RaycastHit2D[1];
                int hitCount = rb.Cast(knockbackDir, wallFilter, hits, knockbackDist);
                if (hitCount > 0)
                    knockbackDist = Mathf.Max(0f, hits[0].distance - 0.1f);

                rb.MovePosition(rb.position + knockbackDir * knockbackDist);

                if (playerSprite != null)
                    StartShieldFlash();

                if (playerCombat != null)
                    playerCombat.ResetSkillCooldown();
            }
            else
            {
                // Check if all targets were killed — allow chain dash
                bool allKilled = true;
                foreach (IDamageable target in snapshotTargets)
                {
                    if (target is MonoBehaviour mb && mb != null && mb.gameObject.activeInHierarchy && !target.IsDead())
                    {
                        allKilled = false;
                        break;
                    }
                }

                if (allKilled && playerCombat != null)
                {
                    playerCombat.ResetSkillCooldown();
                }
            }

            StartCoroutine(HitStop());
        }
        else
        {
            CameraShakeManager.Instance?.Shake(0.15f);
            if (playerCombat != null)
                playerCombat.SetSkillCooldown(currentConfig.whiffCooldown);
        }

        EndDash();
    }

    private void ForceArrival()
    {
        rb.position = dashDestination;
        if (dashTrail != null)
            dashTrail.emitting = false;
        HandleArrival();
    }

    private void EndDash()
    {
        currentState = DashState.Idle;
        currentConfig = null;
        snapshotTargets = null;
    }

    private void SpawnGhost()
    {
        if (dashGhostPrefab == null || playerSprite == null) return;

        GameObject ghost = Instantiate(dashGhostPrefab, transform.position, Quaternion.identity);
        DashGhost dg = ghost.GetComponent<DashGhost>();
        if (dg != null)
        {
            dg.Initialize(
                playerSprite.sprite,
                transform.position,
                transform.localScale,
                playerSprite.flipX,
                ghostColor,
                ghostFadeDuration
            );
        }
    }

    private void StartShieldFlash()
    {
        if (shieldFlashRoutine != null)
            StopCoroutine(shieldFlashRoutine);
        shieldFlashRoutine = StartCoroutine(ShieldHitFlash());
    }

    private IEnumerator ShieldHitFlash()
    {
        playerSprite.color = Color.cyan;
        yield return new WaitForSecondsRealtime(0.15f);
        if (playerSprite != null)
            playerSprite.color = Color.white;
        shieldFlashRoutine = null;
    }

    private IEnumerator HitStop()
    {
        float savedAnimSpeed = animator.speed;
        Vector2 savedVelocity = rb.linearVelocity;
        animator.speed = 0f;
        rb.linearVelocity = Vector2.zero;

        yield return new WaitForSecondsRealtime(0.05f);

        if (animator != null) animator.speed = savedAnimSpeed;
        if (rb != null) rb.linearVelocity = savedVelocity;
    }
}
