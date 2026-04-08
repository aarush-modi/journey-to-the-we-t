using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Melee enemy controller with a finite state machine.
/// Patrols waypoints when unaware, chases via A* pathfinding when alerted to deal contact damage,
/// and searches last-seen position when the player escapes line of sight.
/// </summary>
[RequireComponent(typeof(StealthDetector))]
[RequireComponent(typeof(Rigidbody2D))]
public class MeleeEnemy : MonoBehaviour, IDamageable
{
    private enum EnemyState { Patrol, Chase, Search }

    private static readonly int IdleDownHash = Animator.StringToHash("NinjaRedIdleDown");
    private static readonly int IdleLeftHash = Animator.StringToHash("NinjaRedIdleLeft");
    private static readonly int IdleRightHash = Animator.StringToHash("NinjaRedIdleRight");
    private static readonly int IdleUpHash = Animator.StringToHash("NinjaRedIdleUp");

    private static readonly int WalkDownHash = Animator.StringToHash("NinjaRedWalkDown");
    private static readonly int WalkLeftHash = Animator.StringToHash("NinjaRedWalkLeft");
    private static readonly int WalkRightHash = Animator.StringToHash("NinjaRedWalkRight");
    private static readonly int WalkUpHash = Animator.StringToHash("NinjaRedWalkUp");

    [Header("Data")]
    [SerializeField] private MeleeEnemyData enemyData;

    [Header("References")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private GameObject droppedGoldPrefab;

    [Header("Hurt Feedback")]
    [SerializeField] private float flashDuration = 0.1f;
    [SerializeField] private Color flashColor = Color.red;

    private StealthDetector detector;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private int currentAnimHash;
    private Vector2 lastFacingDirection = Vector2.down;

    private EnemyState currentState = EnemyState.Patrol;
    private float currentHP;
    private bool isDead;
    private float nextDamageTime;

    private int waypointIndex;
    private bool isWaitingAtWaypoint;

    private Transform playerTarget;
    private Vector2 lastSeenPosition;

    private List<Vector2> currentPath;
    private int pathIndex;
    private float pathUpdateTimer;

    private float lostSightTimer;
    private float searchTimer;

    private Collider2D movementBounds;

    private void Awake()
    {
        detector = GetComponent<StealthDetector>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        currentHP = enemyData != null ? enemyData.maxHP : 40f;
    }

    private void Start()
    {
        if (movementBounds == null)
        {
            EnemyBoundary boundary = EnemyBoundary.FindContaining(rb.position);
            if (boundary != null)
                movementBounds = boundary.BoundsCollider;
        }

        detector.OnStateChanged += HandleDetectionStateChanged;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTarget = player.transform;
    }

    private void OnDestroy()
    {
        if (detector != null)
            detector.OnStateChanged -= HandleDetectionStateChanged;
    }

    private void HandleDetectionStateChanged(DetectionState state)
    {
        switch (state)
        {
            case DetectionState.Suspicious:
            case DetectionState.Alerted:
                if (playerTarget != null) lastSeenPosition = playerTarget.position;
                if (currentState == EnemyState.Patrol || currentState == EnemyState.Search)
                    SwitchState(EnemyState.Chase);
                break;

            case DetectionState.Unaware:
                break;
        }
    }

    private void SwitchState(EnemyState next)
    {
        currentState = next;
        lostSightTimer = 0f;
        pathUpdateTimer = 0f;

        switch (next)
        {
            case EnemyState.Patrol:
                isWaitingAtWaypoint = false;
                break;
            case EnemyState.Chase:
                currentPath = null;
                pathIndex = 0;
                break;
            case EnemyState.Search:
                searchTimer = enemyData.searchDuration;
                currentPath = null;
                pathIndex = 0;
                break;
        }
    }

    private void FixedUpdate()
    {
        if (isDead || enemyData == null) return;

        switch (currentState)
        {
            case EnemyState.Patrol:  DoPatrol();  break;
            case EnemyState.Chase:   DoChase();   break;
            case EnemyState.Search:  DoSearch();  break;
        }
    }

    // ── Patrol ──────────────────────────────────────────────────────────

    private void DoPatrol()
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            rb.linearVelocity = Vector2.zero;
            PlayMoveAnimation(Vector2.zero);
            return;
        }

        if (isWaitingAtWaypoint) return;

        Vector2 target = waypoints[waypointIndex].position;
        Vector2 toTarget = target - rb.position;

        if (toTarget.magnitude <= enemyData.waypointReachDist)
        {
            rb.linearVelocity = Vector2.zero;
            PlayMoveAnimation(Vector2.zero);
            waypointIndex = (waypointIndex + 1) % waypoints.Length;
            StartCoroutine(WaypointPause());
        }
        else
        {
            rb.linearVelocity = toTarget.normalized * enemyData.patrolSpeed;
            PlayMoveAnimation(toTarget);
        }
    }

    private IEnumerator WaypointPause()
    {
        isWaitingAtWaypoint = true;
        yield return new WaitForSeconds(enemyData.waypointPauseDuration);
        isWaitingAtWaypoint = false;
    }

    // ── Chase ───────────────────────────────────────────────────────────

    private void DoChase()
    {
        if (playerTarget == null) return;

        // Update last-seen position while player is visible
        if (detector.State == DetectionState.Alerted)
        {
            lastSeenPosition = playerTarget.position;
            lostSightTimer = 0f;
        }
        else
        {
            lostSightTimer += Time.fixedDeltaTime;
            if (lostSightTimer >= enemyData.lostSightChaseTime)
            {
                SwitchState(EnemyState.Search);
                return;
            }
        }

        // Continually A* pathfind to player to naturally collide with them
        pathUpdateTimer -= Time.fixedDeltaTime;
        if (pathUpdateTimer <= 0f)
        {
            pathUpdateTimer = enemyData.pathUpdateInterval;
            RequestPath(playerTarget.position);
        }

        FollowPath(enemyData.chaseSpeed);
        PlayMoveAnimation(rb.linearVelocity);
    }

    // ── Search ──────────────────────────────────────────────────────────

    private void DoSearch()
    {
        // Re-detection transitions
        if (detector.State == DetectionState.Suspicious || detector.State == DetectionState.Alerted)
        {
            SwitchState(EnemyState.Chase);
            return;
        }

        // A* pathfind to last-seen position
        pathUpdateTimer -= Time.fixedDeltaTime;
        if (pathUpdateTimer <= 0f)
        {
            pathUpdateTimer = enemyData.pathUpdateInterval;
            RequestPath(lastSeenPosition);
        }

        float distToLastSeen = Vector2.Distance(rb.position, lastSeenPosition);

        if (distToLastSeen <= enemyData.waypointReachDist)
        {
            // Arrived at last-seen position; wait and look around
            rb.linearVelocity = Vector2.zero;
            PlayMoveAnimation(Vector2.zero);
            searchTimer -= Time.fixedDeltaTime;

            if (searchTimer <= 0f)
            {
                // Give up and return to patrol at nearest waypoint
                ResumePatrolAtNearestWaypoint();
                SwitchState(EnemyState.Patrol);
            }
        }
        else
        {
            FollowPath(enemyData.chaseSpeed);
            PlayMoveAnimation(rb.linearVelocity);
        }
    }

    // ── Pathfinding Helpers ─────────────────────────────────────────────

    private Vector2 ClampToBounds(Vector2 position)
    {
        if (movementBounds == null) return position;
        return movementBounds.ClosestPoint(position);
    }

    private void RequestPath(Vector2 destination)
    {
        if (Pathfinding2D.Instance == null) return;

        destination = ClampToBounds(destination);
        List<Vector2> path = Pathfinding2D.Instance.FindPath(rb.position, destination);
        if (path != null && path.Count > 0)
        {
            currentPath = path;
            pathIndex = 0;
        }
    }

    private void FollowPath(float speed)
    {
        if (currentPath == null || pathIndex >= currentPath.Count)
        {
            if (currentState == EnemyState.Chase && playerTarget != null)
            {
                // Fallback to direct movement if pathfinding is exhausted or missing
                Vector2 directToTarget = (Vector2)playerTarget.position - rb.position;
                rb.linearVelocity = directToTarget.normalized * speed;
                return;
            }

            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 target = currentPath[pathIndex];

        // Skip waypoints outside bounds
        if (movementBounds != null && !movementBounds.OverlapPoint(target))
        {
            target = ClampToBounds(target);
        }

        Vector2 toTarget = target - rb.position;

        if (toTarget.magnitude <= enemyData.waypointReachDist)
        {
            pathIndex++;
            if (pathIndex >= currentPath.Count)
            {
                rb.linearVelocity = Vector2.zero;
                return;
            }
            toTarget = currentPath[pathIndex] - rb.position;
        }

        rb.linearVelocity = toTarget.normalized * speed;
    }

    private void ResumePatrolAtNearestWaypoint()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        float closestDist = float.MaxValue;
        int closestIndex = 0;

        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;
            float dist = Vector2.Distance(rb.position, waypoints[i].position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closestIndex = i;
            }
        }

        waypointIndex = closestIndex;
    }

    // ── Contact Damage ──────────────────────────────────────────────────

    private void OnCollisionEnter2D(Collision2D collision) => TryDamagePlayer(collision.collider);
    private void OnCollisionStay2D(Collision2D collision)  => TryDamagePlayer(collision.collider);

    private void TryDamagePlayer(Collider2D other)
    {
        if (isDead || other == null || !other.CompareTag("Player") || enemyData == null) return;
        if (Time.time < nextDamageTime) return;

        if (other.TryGetComponent(out PlayerCombat pc))
        {
            pc.TakeDamage(enemyData.contactDamage);
            nextDamageTime = Time.time + enemyData.contactCooldown;
        }
    }

    // ── IDamageable ─────────────────────────────────────────────────────

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        EnemyShield shield = GetComponent<EnemyShield>();
        if (shield != null && shield.TryAbsorbHit())
        {
            detector.ForceAlert();
            return;
        }

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

        var deathEffect = GetComponent<EnemyDeathEffect>();
        if (deathEffect != null)
            deathEffect.PlayDeath();
        else
            gameObject.SetActive(false);
    }

    public bool IsDead() => isDead;

    // ── Gold Drop ───────────────────────────────────────────────────────

    private void DropGold()
    {
        if (droppedGoldPrefab == null || enemyData == null || enemyData.baseGoldDrop <= 0) return;
        float modifier = HustleStyleManager.Instance?.GetCombatGoldModifier() ?? 1f;
        int finalGold = Mathf.RoundToInt(enemyData.baseGoldDrop * modifier);
        if (finalGold <= 0) return;
        GameObject drop = Instantiate(droppedGoldPrefab, transform.position, Quaternion.identity);
        drop.GetComponent<DroppedGold>().SetGoldAmount(finalGold);
    }

    // ── Hurt Flash ──────────────────────────────────────────────────────

    private IEnumerator HurtFlash()
    {
        if (spriteRenderer == null) yield break;
        Color original = spriteRenderer.color;
        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        if (!isDead && spriteRenderer != null)
            spriteRenderer.color = original;
    }

    // ── Animation Helpers ────────────────────────────────────────────────

    private void PlayMoveAnimation(Vector2 velocity)
    {
        if (animator == null) return;

        if (velocity.sqrMagnitude < 0.0001f)
        {
            // Play idle in last facing direction
            int idleHash = GetIdleHash(lastFacingDirection);
            if (idleHash != currentAnimHash)
            {
                animator.Play(idleHash);
                currentAnimHash = idleHash;
            }
            return;
        }

        lastFacingDirection = velocity;
        int walkHash = GetWalkHash(velocity);
        if (walkHash != currentAnimHash)
        {
            animator.Play(walkHash);
            currentAnimHash = walkHash;
        }
    }

    private int GetWalkHash(Vector2 direction)
    {
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            return direction.x < 0f ? WalkLeftHash : WalkRightHash;
        return direction.y > 0f ? WalkUpHash : WalkDownHash;
    }

    private int GetIdleHash(Vector2 direction)
    {
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            return direction.x < 0f ? IdleLeftHash : IdleRightHash;
        return direction.y > 0f ? IdleUpHash : IdleDownHash;
    }

    // ── Gizmos ──────────────────────────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        // Patrol waypoints
        if (waypoints != null)
        {
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

        // Current A* path
        if (currentPath != null && currentPath.Count > 1)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < currentPath.Count - 1; i++)
            {
                Gizmos.DrawLine(currentPath[i], currentPath[i + 1]);
            }
        }
    }
}
