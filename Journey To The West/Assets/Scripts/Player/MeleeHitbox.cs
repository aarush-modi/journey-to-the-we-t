using UnityEngine;

public class MeleeHitbox : MonoBehaviour
{
    private PlayerCombat combat;

    private void Awake()
    {
        combat = GetComponentInParent<PlayerCombat>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject == combat.gameObject) return;

        IDamageable target = other.GetComponent<IDamageable>();
        if (target != null)
        {
            target.TakeDamage(combat.GetAttackDamage());
        }
    }
}
