using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Ranged enemy controller with a full finite state machine.
/// Patrols waypoints when unaware, chases via A* pathfinding when alerted,
/// strafes and shoots at preferred distance, retreats when too close,
/// and searches last-seen position when the player escapes line of sight.
/// </summary>
[RequireComponent(typeof(StealthDetector))]
[RequireComponent(typeof(Rigidbody2D))]
public class RangedEnemy : MonoBehaviour, IDamageable
{
    private enum EnemyState { Patrol, Chase, Combat, Retreat, Search }

    private static readonly int IdleDownHash = Animator.StringToHash("NinjaRedIdleDown");
    private static readonly int IdleLeftHash = Animator.StringToHash("NinjaRedIdleLeft");
    private static readonly int IdleRightHash = Animator.StringToHash("NinjaRedIdleRight");
    private static readonly int IdleUpHash = Animator.StringToHash("NinjaRedIdleUp");

    private static readonly int WalkDownHash = Animator.StringToHash("NinjaRedWalkDown");
    private static readonly int WalkLeftHash = Animator.StringToHash("NinjaRedWalkLeft");
    private static readonly int WalkRightHash = Animator.StringToHash("NinjaRedWalkRight");
    private static readonly int WalkUpHash = Animator.StringToHash("NinjaRedWalkUp");

    private static readonly int AttackDownHash = Animator.StringToHash("NinjaRedAttackDown");
    private static readonly int AttackLeftHash = Animator.StringToHash("NinjaRedAttackLeft");
    private static readonly int AttackRightHash = Animator.StringToHash("NinjaRedAttackRight");
    private static readonly int AttackUpHash = Animator.StringToHash("NinjaRedAttackUp");

    [Header("Data")]
    [SerializeField] private RangedEnemyData enemyData;

    [Header("References")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private GameObject droppedGoldPrefab;
    [SerializeField] private LayerMask obstacleLayers;

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

    private float strafeTimer;
    private int strafeSign = 1;

    private float shootTimer;
    private bool isAiming;
    private float aimTimer;

    private float lostSightTimer;
    private float searchTimer;

    private readonly Vector2[] avoidanceProbeDirections = new Vector2[7];

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

        currentHP = enemyData.maxHP;
    }

    private void Start()
    {
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
                // Don't immediately return to patrol — let lostSightTimer handle transitions
                break;
        }
    }

    private void SwitchState(EnemyState next)
    {
        currentState = next;
        lostSightTimer = 0f;
        pathUpdateTimer = 0f;
        isAiming = false;

        switch (next)
        {
            case EnemyState.Patrol:
                isWaitingAtWaypoint = false;
                break;
            case EnemyState.Chase:
                currentPath = null;
                pathIndex = 0;
                break;
            case EnemyState.Combat:
                strafeTimer = enemyData.strafeDuration;
                shootTimer = enemyData.shootCooldown;
                break;
            case EnemyState.Retreat:
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
        if (isDead) return;

        switch (currentState)
        {
            case EnemyState.Patrol:  DoPatrol();  break;
            case EnemyState.Chase:   DoChase();   break;
            case EnemyState.Combat:  DoCombat();  break;
            case EnemyState.Retreat: DoRetreat(); break;
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

        float distToPlayer = Vector2.Distance(rb.position, playerTarget.position);

        // Transition to Combat when close enough and have LOS
        if (distToPlayer < enemyData.maxEngageDistance && HasLineOfSight())
        {
            SwitchState(EnemyState.Combat);
            return;
        }

        // A* pathfinding to player
        pathUpdateTimer -= Time.fixedDeltaTime;
        if (pathUpdateTimer <= 0f)
        {
            pathUpdateTimer = enemyData.pathUpdateInterval;
            RequestPath(playerTarget.position);
        }

        FollowPath(enemyData.chaseSpeed);
        PlayMoveAnimation(rb.linearVelocity);
    }

    // ── Combat ──────────────────────────────────────────────────────────

    private void DoCombat()
    {
        if (playerTarget == null) return;

        float distToPlayer = Vector2.Distance(rb.position, playerTarget.position);

        // Transition: too close → Retreat
        if (distToPlayer < enemyData.minSafeDistance)
        {
            SwitchState(EnemyState.Retreat);
            return;
        }

        // Transition: too far → Chase
        if (distToPlayer > enemyData.maxEngageDistance)
        {
            SwitchState(EnemyState.Chase);
            return;
        }

        // Track lost sight timer
        if (!HasLineOfSight())
        {
            lostSightTimer += Time.fixedDeltaTime;
            if (lostSightTimer >= enemyData.lostSightChaseTime)
            {
                lastSeenPosition = playerTarget.position;
                SwitchState(EnemyState.Search);
                return;
            }
        }
        else
        {
            lostSightTimer = 0f;
            lastSeenPosition = playerTarget.position;
        }

        // Decrement shoot timer
        shootTimer -= Time.fixedDeltaTime;

        // Shoot-move cycle
        if (isAiming)
        {
            // Stopped to aim
            rb.linearVelocity = Vector2.zero;
            PlayMoveAnimation(Vector2.zero);
            aimTimer -= Time.fixedDeltaTime;

            if (aimTimer <= 0f)
            {
                PlayAttackAnimation();
                FireProjectile();
                isAiming = false;
                strafeTimer = enemyData.strafeDuration;
                shootTimer = enemyData.shootCooldown;

                if (Random.value < enemyData.strafeChangeChance)
                    strafeSign *= -1;
            }

            return;
        }

        // Strafe movement
        strafeTimer -= Time.fixedDeltaTime;

        if (strafeTimer <= 0f && shootTimer <= 0f)
        {
            // Begin aiming
            isAiming = true;
            aimTimer = enemyData.aimPauseDuration;
            rb.linearVelocity = Vector2.zero;
            PlayMoveAnimation(Vector2.zero);
            return;
        }

        // Calculate strafe + distance adjustment velocity
        Vector2 toPlayer = ((Vector2)playerTarget.position - rb.position).normalized;
        Vector2 strafeDir = Vector2.Perpendicular(toPlayer) * strafeSign;

        // Check for wall in strafe direction and flip if blocked
        if (Physics2D.Raycast(rb.position, strafeDir, 2f, obstacleLayers))
            strafeSign *= -1;

        Vector2 distanceAdjust = Vector2.zero;
        if (distToPlayer < enemyData.preferredDistance - 1f)
            distanceAdjust = -toPlayer * 0.5f;
        else if (distToPlayer > enemyData.preferredDistance + 1f)
            distanceAdjust = toPlayer * 0.5f;

        rb.linearVelocity = (strafeDir + distanceAdjust).normalized * enemyData.combatMoveSpeed;
        PlayMoveAnimation(rb.linearVelocity);
    }

    // ── Retreat ─────────────────────────────────────────────────────────

    private void DoRetreat()
    {
        if (playerTarget == null) return;

        float distToPlayer = Vector2.Distance(rb.position, playerTarget.position);

        // Transition back to Combat when at preferred distance
        if (distToPlayer >= enemyData.preferredDistance)
        {
            SwitchState(EnemyState.Combat);
            return;
        }

        // Move away from player with obstacle avoidance
        Vector2 awayFromPlayer = (rb.position - (Vector2)playerTarget.position).normalized;
        Vector2 retreatVelocity = GetObstacleAwareVelocity(awayFromPlayer, enemyData.retreatSpeed);
        rb.linearVelocity = retreatVelocity;
        PlayMoveAnimation(retreatVelocity);

        // Still fire while retreating if shoot timer is ready
        shootTimer -= Time.fixedDeltaTime;
        if (shootTimer <= 0f && HasLineOfSight())
        {
            PlayAttackAnimation();
            FireProjectile();
            shootTimer = enemyData.shootCooldown;
        }
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

    // ── Pathfinding helpers ─────────────────────────────────────────────

    private void RequestPath(Vector2 destination)
    {
        if (Pathfinding2D.Instance == null) return;

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
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 target = currentPath[pathIndex];
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

    // ── Obstacle avoidance (7-probe pattern) ────────────────────────────

    private Vector2 GetObstacleAwareVelocity(Vector2 preferredDirection, float speed)
    {
        if (preferredDirection.sqrMagnitude < 0.0001f)
            return Vector2.zero;

        Vector2 normalizedDirection = preferredDirection.normalized;
        avoidanceProbeDirections[0] = normalizedDirection;
        avoidanceProbeDirections[1] = RotateDirection(normalizedDirection, 35f);
        avoidanceProbeDirections[2] = RotateDirection(normalizedDirection, -35f);
        avoidanceProbeDirections[3] = RotateDirection(normalizedDirection, 70f);
        avoidanceProbeDirections[4] = RotateDirection(normalizedDirection, -70f);
        avoidanceProbeDirections[5] = RotateDirection(normalizedDirection, 110f);
        avoidanceProbeDirections[6] = RotateDirection(normalizedDirection, -110f);

        foreach (Vector2 candidateDirection in avoidanceProbeDirections)
        {
            if (!HasBlockingObstacle(candidateDirection))
                return candidateDirection * speed;
        }

        // All probes blocked; move in preferred direction as fallback
        return normalizedDirection * speed;
    }

    private bool HasBlockingObstacle(Vector2 direction)
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(
            rb.position,
            enemyData.obstacleProbeRadius,
            direction,
            enemyData.obstacleProbeDistance
        );

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null
                || hit.collider.isTrigger
                || hit.collider.attachedRigidbody == rb
                || hit.collider.CompareTag("Player")
                || (playerTarget != null && hit.collider.transform.IsChildOf(playerTarget)))
            {
                continue;
            }

            return true;
        }

        return false;
    }

    private static Vector2 RotateDirection(Vector2 direction, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        float sin = Mathf.Sin(radians);
        float cos = Mathf.Cos(radians);

        return new Vector2(
            (direction.x * cos) - (direction.y * sin),
            (direction.x * sin) + (direction.y * cos)
        ).normalized;
    }

    // ── Shooting ────────────────────────────────────────────────────────

    private void FireProjectile()
    {
        if (projectilePrefab == null || playerTarget == null) return;

        Vector2 dirToPlayer = ((Vector2)playerTarget.position - rb.position).normalized;
        lastFacingDirection = dirToPlayer;
        PlayAttackAnimation();
        GameObject proj = Instantiate(projectilePrefab, (Vector3)rb.position, Quaternion.identity);
        proj.GetComponent<EnemyProjectile>().Initialize(gameObject, dirToPlayer, enemyData.projectileSpeed, enemyData.projectileDamage);
    }

    // ── Line of sight ───────────────────────────────────────────────────

    private bool HasLineOfSight()
    {
        if (playerTarget == null) return false;

        RaycastHit2D hit = Physics2D.Linecast(rb.position, playerTarget.position, obstacleLayers);
        return hit.collider == null;
    }

    // ── Patrol resume helper ────────────────────────────────────────────

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

    // ── Contact damage ──────────────────────────────────────────────────

    private void OnCollisionEnter2D(Collision2D collision) => TryDamagePlayer(collision.collider);
    private void OnCollisionStay2D(Collision2D collision)  => TryDamagePlayer(collision.collider);

    private void TryDamagePlayer(Collider2D other)
    {
        if (isDead || other == null || !other.CompareTag("Player")) return;
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

    // ── Gold drop ───────────────────────────────────────────────────────

    private void DropGold()
    {
        if (droppedGoldPrefab == null || enemyData.baseGoldDrop <= 0) return;
        float modifier = HustleStyleManager.Instance?.GetCombatGoldModifier() ?? 1f;
        int finalGold = Mathf.RoundToInt(enemyData.baseGoldDrop * modifier);
        if (finalGold <= 0) return;
        GameObject drop = Instantiate(droppedGoldPrefab, transform.position, Quaternion.identity);
        drop.GetComponent<DroppedGold>().SetGoldAmount(finalGold);
    }

    // ── Hurt flash ──────────────────────────────────────────────────────

    private IEnumerator HurtFlash()
    {
        if (spriteRenderer == null) yield break;
        Color original = spriteRenderer.color;
        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        if (!isDead && spriteRenderer != null)
            spriteRenderer.color = original;
    }

    // ── Animation helpers ────────────────────────────────────────────────

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

    private void PlayAttackAnimation()
    {
        if (animator == null) return;
        int attackHash = GetAttackHash(lastFacingDirection);
        animator.Play(attackHash);
        currentAnimHash = attackHash;
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

    private int GetAttackHash(Vector2 direction)
    {
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            return direction.x < 0f ? AttackLeftHash : AttackRightHash;
        return direction.y > 0f ? AttackUpHash : AttackDownHash;
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

        // Engagement ranges
        if (enemyData != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, enemyData.preferredDistance);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, enemyData.minSafeDistance);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, enemyData.maxEngageDistance);
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
