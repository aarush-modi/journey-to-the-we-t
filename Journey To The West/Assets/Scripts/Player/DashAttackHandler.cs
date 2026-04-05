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

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        greedMeter = GetComponent<GreedMeter>();
        playerCombat = GetComponent<PlayerCombat>();
        playerSprite = GetComponent<SpriteRenderer>();
    }

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
        RaycastHit2D[] hits = new RaycastHit2D[1];
        int hitCount = rb.Cast(toTarget.normalized, wallFilter, hits, toTarget.magnitude);
        if (hitCount > 0)
        {
            dashDestination = rb.position + toTarget.normalized * Mathf.Max(0f, hits[0].distance - wallStopOffset);
        }

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

        rb.MovePosition(Vector2.MoveTowards(rb.position, dashDestination, currentConfig.dashSpeed * Time.fixedDeltaTime));

        // Stuck detection: if we haven't moved, end dash early (skip first tick — MovePosition is deferred on dynamic bodies)
        if (!dashFirstTick && Vector2.Distance(rb.position, lastDashPos) < 0.01f)
        {
            // Wall blocked us — end dash as a whiff
            if (dashTrail != null)
                dashTrail.emitting = false;
            CameraShakeManager.Instance?.Shake(0.15f);
            if (playerCombat != null)
                playerCombat.SetSkillCooldown(currentConfig.whiffCooldown);
            currentState = DashState.Idle;
            currentConfig = null;
            snapshotTargets = null;
            return;
        }
        dashFirstTick = false;
        lastDashPos = rb.position;

        if (Vector2.Distance(rb.position, dashDestination) < 0.1f)
        {
            // Arrived at destination
            if (dashTrail != null)
                dashTrail.emitting = false;

            if (snapshotHadTargets)
            {
                float damage = currentConfig.baseDashDamage * (greedMeter != null ? greedMeter.GetDamageMultiplier() : 1f);

                foreach (IDamageable target in snapshotTargets)
                {
                    if (target is MonoBehaviour mb && mb != null && mb.gameObject.activeInHierarchy)
                        target.TakeDamage(damage);
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

                StartCoroutine(HitStop());
            }
            else
            {
                CameraShakeManager.Instance?.Shake(0.15f);
                if (playerCombat != null)
                    playerCombat.SetSkillCooldown(currentConfig.whiffCooldown);
            }

            currentState = DashState.Idle;
            currentConfig = null;
            snapshotTargets = null;
        }
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

    private IEnumerator HitStop()
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(0.05f);
        Time.timeScale = 1f;
    }
}
