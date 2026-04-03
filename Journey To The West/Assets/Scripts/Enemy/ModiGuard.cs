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

    private Animator guardAnimator;
    private SpriteRenderer spriteRenderer;
    private Transform playerTarget;
    private float currentHP;
    private float nextDamageTime;
    private bool isChasing;
    private bool isDead;

    private void Awake()
    {
        guardAnimator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentHP = maxHP;

        Rigidbody2D guardBody = GetComponent<Rigidbody2D>();
        if (guardBody == null)
        {
            guardBody = gameObject.AddComponent<Rigidbody2D>();
        }

        guardBody.bodyType = RigidbodyType2D.Kinematic;
        guardBody.gravityScale = 0f;
        guardBody.freezeRotation = true;

        Collider2D guardCollider = GetComponent<Collider2D>();
        if (guardCollider == null)
        {
            guardCollider = gameObject.AddComponent<BoxCollider2D>();
        }

        guardCollider.isTrigger = true;
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
        if (guardAnimator != null)
        {
            guardAnimator.SetBool(IsChasingHash, false);
        }
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!isChasing || isDead || playerTarget == null)
        {
            return;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            playerTarget.position,
            moveSpeed * Time.deltaTime
        );
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDamagePlayer(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryDamagePlayer(other);
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
