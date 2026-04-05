using System.Collections;
using UnityEngine;

public class ChaseEnemyController : MonoBehaviour, IDamageable
{
    [SerializeField] private EnemyData enemyData;
    [SerializeField] private GameObject droppedGoldPrefab;
    [SerializeField] private float detectRange = 100f;
    [SerializeField] private float attackRange = 5f;
    [SerializeField] private float attackCooldown = 2f;

    [Header("Animation Clip Names")]
    [SerializeField] private string idleAnim = "Idle";
    [SerializeField] private string moveAnim = "Move";
    [SerializeField] private string attackAnim = "Attack";
    [SerializeField] private string hurtAnim = "Take Hit";
    [SerializeField] private string deathAnim = "Death";

    private float currentHP;
    private bool isDead;
    private bool canAttack = true;
    private bool isAttacking;
    private bool isHurt;
    private string currentAnim = "";
    private Transform player;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        currentHP = enemyData.maxHP;
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        PlayAnim(idleAnim);
    }

    private void PlayAnim(string animName)
    {
        if (currentAnim == animName) return;
        PlayAnimForce(animName);
    }

    private void PlayAnimForce(string animName)
    {
        currentAnim = animName;
        int hash = Animator.StringToHash(animName);
        bool found = animator.HasState(0, hash);
        if (!found)
        {
            hash = Animator.StringToHash("Base Layer." + animName);
            found = animator.HasState(0, hash);
        }
        if (found)
            animator.Play(hash);
        else
            Debug.LogWarning($"[ChaseEnemy] State NOT found: '{animName}' on controller: {animator.runtimeAnimatorController.name}");
    }

    private void Update()
    {
        if (isDead || isAttacking || isHurt || player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= attackRange)
        {
            FlipTowards((player.position - transform.position).x);
            if (canAttack)
                StartCoroutine(Attack());
            else
                PlayAnim(idleAnim);
        }
        else if (dist <= detectRange)
        {
            Vector2 dir = (player.position - transform.position).normalized;
            transform.position += (Vector3)(dir * enemyData.moveSpeed * Time.deltaTime);
            FlipTowards(dir.x);
            PlayAnim(moveAnim);
        }
        else
        {
            PlayAnim(idleAnim);
        }
    }

    private void FlipTowards(float dirX)
    {
        if (dirX > 0)
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else if (dirX < 0)
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    private IEnumerator Attack()
    {
        canAttack = false;
        isAttacking = true;
        rb.linearVelocity = Vector2.zero;

        currentAnim = attackAnim;
        PlayAnimForce(attackAnim);

        yield return new WaitForSeconds(0.5f);

        if (!isDead && player != null)
        {
            float dist = Vector2.Distance(transform.position, player.position);
            IDamageable target = player.GetComponent<IDamageable>()
                ?? player.GetComponentInChildren<IDamageable>()
                ?? player.GetComponentInParent<IDamageable>();
            if (dist <= attackRange && target != null)
            {
                target.TakeDamage(enemyData.damage);
            }
        }

        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
        canAttack = true;
        currentAnim = "";
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHP -= amount;
        StartCoroutine(PlayHurt());

        if (currentHP <= 0f)
        {
            currentHP = 0f;
            Die();
        }
    }

    private IEnumerator PlayHurt()
    {
        isHurt = true;
        rb.linearVelocity = Vector2.zero;
        currentAnim = hurtAnim;
        PlayAnimForce(hurtAnim);
        yield return new WaitForSeconds(0.3f);
        isHurt = false;
        currentAnim = "";
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        isAttacking = false;
        isHurt = false;
        rb.linearVelocity = Vector2.zero;

        StopAllCoroutines();
        currentAnim = deathAnim;
        PlayAnimForce(deathAnim);
        DropGold();
        StartCoroutine(DeathCleanup());
    }

    private IEnumerator DeathCleanup()
    {
        yield return new WaitForSeconds(1f);
        gameObject.SetActive(false);
    }

    private void DropGold()
    {
        if (droppedGoldPrefab == null) return;

        float modifier = 1f;
        if (HustleStyleManager.Instance != null)
            modifier = HustleStyleManager.Instance.GetCombatGoldModifier();

        int finalGold = Mathf.RoundToInt(enemyData.baseGoldDrop * modifier);

        if (finalGold > 0)
        {
            GameObject drop = Instantiate(droppedGoldPrefab, transform.position, Quaternion.identity);
            drop.GetComponent<DroppedGold>().SetGoldAmount(finalGold);
        }
    }
}
