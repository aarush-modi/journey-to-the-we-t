using System.Collections;
using UnityEngine;

/// <summary>
/// Level 5 boss enemy. Chases the player and periodically lunges.
/// On death drops an ArmorPickup prefab (Gold-Trimmed Robe).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Level5Boss : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    [SerializeField] private float maxHP = 200f;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float contactDamage = 20f;
    [SerializeField] private float contactCooldown = 0.5f;

    [Header("Lunge Attack")]
    [SerializeField] private float lungeRange = 5f;
    [SerializeField] private float lungeSpeed = 7f;
    [SerializeField] private float lungeDuration = 0.4f;
    [SerializeField] private float lungeCooldown = 3.5f;

    [Header("Boss Drop")]
    [SerializeField] private GameObject armorPickupPrefab;

    [Header("Hurt Feedback")]
    [SerializeField] private float flashDuration = 0.15f;
    [SerializeField] private Color flashColor = Color.red;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Transform playerTarget;

    private float currentHP;
    private bool isDead;
    private float nextDamageTime;
    private float nextLungeTime;
    private bool isLunging;

    private void Awake()
    {
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
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTarget = player.transform;
    }

    private void FixedUpdate()
    {
        if (isDead || playerTarget == null || isLunging) return;

        Vector2 dir = ((Vector2)playerTarget.position - rb.position).normalized;
        rb.linearVelocity = dir * moveSpeed;

        float dist = Vector2.Distance(rb.position, playerTarget.position);
        if (dist <= lungeRange && Time.time >= nextLungeTime)
            StartCoroutine(Lunge(dir));
    }

    private IEnumerator Lunge(Vector2 dir)
    {
        isLunging = true;
        nextLungeTime = Time.time + lungeCooldown;
        rb.linearVelocity = dir * lungeSpeed;
        yield return new WaitForSeconds(lungeDuration);
        isLunging = false;
    }

    private void OnCollisionEnter2D(Collision2D collision) => TryDamagePlayer(collision.collider);
    private void OnCollisionStay2D(Collision2D collision)  => TryDamagePlayer(collision.collider);

    private void TryDamagePlayer(Collider2D other)
    {
        if (isDead || other == null || !other.CompareTag("Player")) return;
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
        if (currentHP <= 0f) Die();
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        rb.linearVelocity = Vector2.zero;

        if (armorPickupPrefab != null)
            Instantiate(armorPickupPrefab, transform.position, Quaternion.identity);

        gameObject.SetActive(false);
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
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, lungeRange);
    }
}
