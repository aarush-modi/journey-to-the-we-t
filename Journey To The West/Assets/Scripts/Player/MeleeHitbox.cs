using UnityEngine;

public class MeleeHitbox : MonoBehaviour
{
    private PlayerCombat combat;

    private void Awake()
    {
        combat = GetComponentInParent<PlayerCombat>();
    }

    private void OnEnable()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col == null) return;

        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = true;
        filter.NoFilter();

        Collider2D[] results = new Collider2D[10];
        int count = col.Overlap(filter, results);

        for (int i = 0; i < count; i++)
        {
            HitTarget(results[i]);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HitTarget(other);
    }

    private void HitTarget(Collider2D other)
    {
        if (other.gameObject == combat.gameObject) return;

        IDamageable target = other.GetComponent<IDamageable>();
        if (target != null)
        {
            target.TakeDamage(combat.GetAttackDamage());
        }
    }
}
