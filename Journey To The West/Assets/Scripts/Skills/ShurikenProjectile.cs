using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class ShurikenProjectile : MonoBehaviour
{
    [SerializeField] private float lifetime = 2f;

    private Rigidbody2D rb;
    private GameObject owner;
    private float damage;
    private float lifetimeTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        Collider2D triggerCollider = GetComponent<Collider2D>();
        triggerCollider.isTrigger = true;
    }

    private void OnEnable()
    {
        lifetimeTimer = lifetime;
    }

    private void Update()
    {
        lifetimeTimer -= Time.deltaTime;
        if (lifetimeTimer <= 0f)
        {
            Destroy(gameObject);
        }
    }

    public void Initialize(GameObject projectileOwner, Vector2 direction, float speed, float projectileDamage)
    {
        owner = projectileOwner;
        damage = projectileDamage;
        rb.linearVelocity = direction.normalized * speed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (owner != null && other.transform.root.gameObject == owner)
        {
            return;
        }

        IDamageable damageable = other.GetComponentInParent<IDamageable>();
        if (damageable == null)
        {
            return;
        }

        damageable.TakeDamage(damage);
        Destroy(gameObject);
    }
}
