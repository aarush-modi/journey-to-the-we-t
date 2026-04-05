using System.Collections;
using UnityEngine;

public class ModiGuard : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHP = 20f;
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float contactDamage = 5f;
    [SerializeField] private float contactDamageCooldown = 0.5f;
    [SerializeField] private float flashDuration = 0.1f;
    [SerializeField] private Color flashColor = Color.red;

    private static readonly int IsChasingHash = Animator.StringToHash("IsChasing");
    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int GuardIdleStateHash = Animator.StringToHash("GuardIdle");
    private static readonly int GuardWalkFrontStateHash = Animator.StringToHash("GuardWalk");
    private static readonly int GuardWalkBackStateHash = Animator.StringToHash("GuardWalkBack");
    private static readonly int GuardWalkLeftStateHash = Animator.StringToHash("GuardWalkLeft");
    private static readonly int GuardWalkRightStateHash = Animator.StringToHash("GuardWalkRight");
    private static readonly int GuardAttackStateHash = Animator.StringToHash("GuardAttack");

    private Animator guardAnimator;
    private Rigidbody2D guardBody;
    private SpriteRenderer spriteRenderer;
    private Transform playerTarget;
    private float currentHP;
    private float nextDamageTime;
    private int currentMoveStateHash;
    private bool isChasing;
    private bool isDead;

    public static void AlertAllGuards()
    {
        foreach (Transform sceneTransform in FindObjectsOfType<Transform>(true))
        {
            if (!sceneTransform.name.StartsWith("Guard"))
            {
                continue;
            }

            ModiGuard guard = sceneTransform.GetComponent<ModiGuard>();
            if (guard == null)
            {
                guard = sceneTransform.gameObject.AddComponent<ModiGuard>();
            }

            guard.BeginChase();
        }
    }

    private void Awake()
    {
        guardAnimator = GetComponent<Animator>();
        guardBody = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentHP = maxHP;
        currentMoveStateHash = GuardIdleStateHash;

        if (guardBody == null)
        {
            guardBody = gameObject.AddComponent<Rigidbody2D>();
        }

        guardBody.bodyType = RigidbodyType2D.Dynamic;
        guardBody.gravityScale = 0f;
        guardBody.freezeRotation = true;
        guardBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        Collider2D guardCollider = GetComponent<Collider2D>();
        if (guardCollider == null)
        {
            guardCollider = gameObject.AddComponent<BoxCollider2D>();
        }

        guardCollider.isTrigger = false;
    }

    public void BeginChase()
    {
        if (isDead)
        {
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTarget = player.transform;
            isChasing = true;
            if (guardAnimator != null)
            {
                guardAnimator.SetBool(IsChasingHash, true);
                PlayMoveState(GuardWalkFrontStateHash);
            }
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead)
        {
            return;
        }

        currentHP -= amount;

        if (spriteRenderer != null)
        {
            StopAllCoroutines();
            StartCoroutine(HurtFlash());
        }

        if (currentHP <= 0f)
        {
            Die();
        }
    }

    public void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        isChasing = false;
        if (guardBody != null)
        {
            guardBody.linearVelocity = Vector2.zero;
        }

        if (guardAnimator != null)
        {
            guardAnimator.SetBool(IsChasingHash, false);
            PlayMoveState(GuardIdleStateHash);
        }
        var deathEffect = GetComponent<EnemyDeathEffect>();
        if (deathEffect != null)
            deathEffect.PlayDeath();
        else
            gameObject.SetActive(false);
    }

    public bool IsDead() => isDead;

    private void FixedUpdate()
    {
        if (!isChasing || isDead || playerTarget == null)
        {
            if (guardBody != null)
            {
                guardBody.linearVelocity = Vector2.zero;
            }

            PlayMoveState(GuardIdleStateHash);
            return;
        }

        Vector2 chaseDirection = ((Vector2)playerTarget.position - guardBody.position).normalized;
        guardBody.linearVelocity = chaseDirection * moveSpeed;
        PlayMoveState(GetMoveStateHash(chaseDirection));
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryDamagePlayer(collision.collider);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryDamagePlayer(collision.collider);
    }

    private void TryDamagePlayer(Collider2D other)
    {
        if (!isChasing || isDead || other == null || !other.CompareTag("Player"))
        {
            return;
        }

        if (Time.time < nextDamageTime)
        {
            return;
        }

        if (other.TryGetComponent(out PlayerCombat playerCombat))
        {
            if (guardAnimator != null)
            {
                guardAnimator.SetTrigger(AttackHash);
            }

            playerCombat.TakeDamage(contactDamage);
            nextDamageTime = Time.time + contactDamageCooldown;
        }
    }

    private int GetMoveStateHash(Vector2 chaseDirection)
    {
        if (Mathf.Abs(chaseDirection.x) > Mathf.Abs(chaseDirection.y))
        {
            return chaseDirection.x < 0f ? GuardWalkLeftStateHash : GuardWalkRightStateHash;
        }

        return chaseDirection.y > 0f ? GuardWalkBackStateHash : GuardWalkFrontStateHash;
    }

    private void PlayMoveState(int nextStateHash)
    {
        if (guardAnimator == null || currentMoveStateHash == nextStateHash)
        {
            return;
        }

        AnimatorStateInfo stateInfo = guardAnimator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.shortNameHash == GuardAttackStateHash && stateInfo.normalizedTime < 1f)
        {
            return;
        }

        currentMoveStateHash = nextStateHash;
        guardAnimator.Play(nextStateHash);
    }

    private IEnumerator HurtFlash()
    {
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(flashDuration);

        if (!isDead && spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }
}
