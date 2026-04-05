using System.Collections.Generic;
using UnityEngine;

public class MeleeHitbox : MonoBehaviour
{
    private PlayerCombat combat;
    private HashSet<Collider2D> hitThisSwing = new HashSet<Collider2D>();

    private void Awake()
    {
        combat = GetComponentInParent<PlayerCombat>();
    }

    // Called by WeaponDisplay each time an attack starts
    public void PrepareForAttack()
    {
        hitThisSwing.Clear();

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
        if (hitThisSwing.Contains(other)) return;

        IDamageable target = other.GetComponent<IDamageable>();
        if (target != null)
        {
            hitThisSwing.Add(other);
            target.TakeDamage(combat.GetAttackDamage());
        }
    }
}
