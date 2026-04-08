using System.Collections;
using UnityEngine;

/// <summary>
/// Level 5 patrol guard. Wanders waypoints when unaware, reacts with graduated
/// suspicion (cautious → watchful → investigating), and chases the player when
/// fully alerted. Requires a StealthDetector on the same GameObject.
/// </summary>
[RequireComponent(typeof(StealthDetector))]
[RequireComponent(typeof(Rigidbody2D))]
public class StealthGuard : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    [SerializeField] private float maxHP = 30f;
    [SerializeField] private float patrolSpeed = 1.5f;
    [SerializeField] private float cautiousSpeedMultiplier = 0.7f;
    [SerializeField] private float chaseSpeed = 3f;
    [SerializeField] private float contactDamage = 10f;
    [SerializeField] private float contactCooldown = 0.5f;

    [Header("Patrol Waypoints")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float waypointReachDist = 0.3f;
    [SerializeField] private float waypointPauseDuration = 1f;

    [Header("Investigation")]
    [SerializeField] private float investigateDuration = 3f;

    [Header("Alert Radius")]
    [SerializeField] private float alertNearbyRadius = 8f;

    [Header("Gold Drop")]
    [SerializeField] private GameObject droppedGoldPrefab;
    [SerializeField] private int baseGoldDrop = 15;

    [Header("Hurt Feedback")]
    [SerializeField] private float flashDuration = 0.1f;
    [SerializeField] private Color flashColor = Color.red;

    private enum GuardBehavior { Patrolling, CautiousPatrol, Watching, Investigating, Chasing }

    private StealthDetector detector;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private GuardBehavior behavior = GuardBehavior.Patrolling;
    private float currentHP;
    private bool isDead;
    private float nextDamageTime;

    private int waypointIndex;
    private bool isWaitingAtWaypoint;

    private Vector2 lastSeenPosition;
    private float investigateTimer;

    private Transform playerTarget;

    private void Awake()
    {
        detector = GetComponent<StealthDetector>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        currentHP = maxHP;
    }

    private void Start()
    {
        detector.OnStateChanged += HandleDetectionStateChanged;
        detector.OnSuspicionLevelChanged += HandleSuspicionLevelChanged;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTarget = player.transform;
    }

    private void OnDestroy()
    {
        if (detector != null)
        {
            detector.OnStateChanged -= HandleDetectionStateChanged;
            detector.OnSuspicionLevelChanged -= HandleSuspicionLevelChanged;
        }
    }

    private void HandleDetectionStateChanged(DetectionState state)
    {
        switch (state)
        {
            case DetectionState.Alerted:
                if (playerTarget != null) lastSeenPosition = playerTarget.position;
                SwitchBehavior(GuardBehavior.Chasing);
                AlertNearbyGuards();
                break;

            case DetectionState.Unaware:
                SwitchBehavior(GuardBehavior.Patrolling);
                break;
        }
    }

    private void HandleSuspicionLevelChanged(SuspicionLevel level)
    {
        if (detector.State == DetectionState.Alerted) return;

        if (playerTarget != null)
            lastSeenPosition = playerTarget.position;

        switch (level)
        {
            case SuspicionLevel.None:
                SwitchBehavior(GuardBehavior.Patrolling);
                break;
            case SuspicionLevel.Low:
                SwitchBehavior(GuardBehavior.CautiousPatrol);
                break;
            case SuspicionLevel.Medium:
                SwitchBehavior(GuardBehavior.Watching);
                break;
            case SuspicionLevel.High:
                SwitchBehavior(GuardBehavior.Investigating);
                break;
        }
    }

    private void SwitchBehavior(GuardBehavior next)
    {
        behavior = next;
        investigateTimer = investigateDuration;
        isWaitingAtWaypoint = false;
    }

    private void FixedUpdate()
    {
        if (isDead) return;

        switch (behavior)
        {
            case GuardBehavior.Patrolling:     DoPatrol(patrolSpeed);                                break;
            case GuardBehavior.CautiousPatrol: DoPatrol(patrolSpeed * cautiousSpeedMultiplier);       break;
            case GuardBehavior.Watching:       DoWatch();                                             break;
            case GuardBehavior.Investigating:  DoInvestigate();                                       break;
            case GuardBehavior.Chasing:        DoChase();                                             break;
        }

        FlipSprite();
    }

    // ── Behaviors ─────────────────────────────────────────────────────────

    private void DoPatrol(float speed)
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (isWaitingAtWaypoint) return;

        Vector2 target = waypoints[waypointIndex].position;
        Vector2 toTarget = target - rb.position;

        if (toTarget.magnitude <= waypointReachDist)
        {
            rb.linearVelocity = Vector2.zero;
            waypointIndex = (waypointIndex + 1) % waypoints.Length;
            StartCoroutine(WaypointPause());
        }
        else
        {
            rb.linearVelocity = toTarget.normalized * speed;
        }
    }

    private IEnumerator WaypointPause()
    {
        isWaitingAtWaypoint = true;
        yield return new WaitForSeconds(waypointPauseDuration);
        isWaitingAtWaypoint = false;
    }

    private void DoWatch()
    {
        rb.linearVelocity = Vector2.zero;

        if (playerTarget != null)
            lastSeenPosition = playerTarget.position;
    }

    private void DoInvestigate()
    {
        Vector2 toLastSeen = lastSeenPosition - rb.position;

        if (toLastSeen.magnitude > waypointReachDist)
        {
            rb.linearVelocity = toLastSeen.normalized * patrolSpeed;
            return;
        }

        rb.linearVelocity = Vector2.zero;
        investigateTimer -= Time.fixedDeltaTime;

        if (investigateTimer <= 0f && detector.State != DetectionState.Alerted)
            SwitchBehavior(GuardBehavior.Patrolling);
    }

    private void DoChase()
    {
        if (playerTarget == null) return;
        Vector2 dir = ((Vector2)playerTarget.position - rb.position).normalized;
        rb.linearVelocity = dir * chaseSpeed;
        lastSeenPosition = playerTarget.position;
    }

    private void FlipSprite()
    {
        if (spriteRenderer == null) return;
        if (rb.linearVelocity.x > 0.05f) spriteRenderer.flipX = false;
        else if (rb.linearVelocity.x < -0.05f) spriteRenderer.flipX = true;
    }

    // ── Alerting ──────────────────────────────────────────────────────────

    private void AlertNearbyGuards()
    {
        Collider2D[] nearby = Physics2D.OverlapCircleAll(transform.position, alertNearbyRadius);
        foreach (Collider2D col in nearby)
        {
            if (col.gameObject == gameObject) continue;
            StealthDetector nearbyDetector = col.GetComponent<StealthDetector>();
            if (nearbyDetector == null) continue;

            RaycastHit2D hit = Physics2D.Linecast(transform.position, col.transform.position, detector.ObstacleLayers);
            if (hit.collider != null) continue;

            nearbyDetector.ForceAlert();
        }
    }

    // ── Collision / Damage ────────────────────────────────────────────────

    private void OnCollisionEnter2D(Collision2D collision) => TryDamagePlayer(collision.collider);
    private void OnCollisionStay2D(Collision2D collision)  => TryDamagePlayer(collision.collider);

    private void TryDamagePlayer(Collider2D other)
    {
        if (isDead || other == null || !other.CompareTag("Player")) return;
        if (behavior != GuardBehavior.Chasing) return;
        if (Time.time < nextDamageTime) return;

        if (other.TryGetComponent(out PlayerCombat pc))
        {
            pc.TakeDamage(contactDamage);
            nextDamageTime = Time.time + contactCooldown;
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;
        currentHP -= amount;
        StartCoroutine(HurtFlash());
        detector.ForceAlert();
        if (currentHP <= 0f) Die();
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        DropGold();
        gameObject.SetActive(false);
    }

    public bool IsDead() => isDead;

    private void DropGold()
    {
        if (droppedGoldPrefab == null || baseGoldDrop <= 0) return;
        float modifier = HustleStyleManager.Instance?.GetCombatGoldModifier() ?? 1f;
        int finalGold = Mathf.RoundToInt(baseGoldDrop * modifier);
        if (finalGold <= 0) return;
        GameObject drop = Instantiate(droppedGoldPrefab, transform.position, Quaternion.identity);
        drop.GetComponent<DroppedGold>().SetGoldAmount(finalGold);
    }

    private IEnumerator HurtFlash()
    {
        if (spriteRenderer == null) yield break;
        Color original = spriteRenderer.color;
        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        if (!isDead && spriteRenderer != null)
            spriteRenderer.color = original;
    }

    private void OnDrawGizmosSelected()
    {
        if (waypoints == null) return;
        Gizmos.color = Color.cyan;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;
            Gizmos.DrawSphere(waypoints[i].position, 0.2f);
            int next = (i + 1) % waypoints.Length;
            if (waypoints[next] != null)
                Gizmos.DrawLine(waypoints[i].position, waypoints[next].position);
        }
    }
}
