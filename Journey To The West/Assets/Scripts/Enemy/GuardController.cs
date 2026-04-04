using System.Collections;
using UnityEngine;

public class GuardController : MonoBehaviour, IDamageable
{
    [SerializeField] private EnemyData enemyData;
    [SerializeField] private GameObject droppedGoldPrefab;
    [SerializeField] private float detectRange = 5f;
    [SerializeField] private float attackRange = 1.2f;
    [SerializeField] private float attackCooldown = 1.5f;

    private float currentHP;
    private bool isDead;
    private bool canAttack = true;
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
        if (animator != null)
        {
            animator.SetBool("Grounded", true);
            animator.SetFloat("AirSpeed", 0f);
        }
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    private void FixedUpdate()
    {
        if (isDead || player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= attackRange)
        {
            rb.linearVelocity = Vector2.zero;
            if (animator != null)
                animator.SetInteger("AnimState", 1);
            if (canAttack)
                StartCoroutine(Attack());
        }
        else if (dist <= detectRange)
        {
            Vector2 dir = (player.position - transform.position).normalized;
            rb.linearVelocity = dir * enemyData.moveSpeed;
            FlipTowards(dir.x);
            if (animator != null)
                animator.SetInteger("AnimState", 2);
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            if (animator != null)
                animator.SetInteger("AnimState", 0);
        }
    }

    private void FlipTowards(float dirX)
    {
        if (dirX > 0)
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else if (dirX < 0)
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    private IEnumerator Attack()
    {
        canAttack = false;
        rb.linearVelocity = Vector2.zero;

        if (animator != null)
            animator.SetTrigger("Attack");

        yield return new WaitForSeconds(0.4f);

        if (!isDead && player != null)
        {
            float dist = Vector2.Distance(transform.position, player.position);
            if (dist <= attackRange)
            {
                IDamageable target = player.GetComponent<IDamageable>();
                if (target != null)
                    target.TakeDamage(enemyData.damage);
            }
        }

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHP -= amount;

        if (animator != null)
            animator.SetTrigger("Hurt");

        if (currentHP <= 0f)
        {
            currentHP = 0f;
            Die();
        }
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        rb.linearVelocity = Vector2.zero;

        if (animator != null)
            animator.SetTrigger("Death");

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
