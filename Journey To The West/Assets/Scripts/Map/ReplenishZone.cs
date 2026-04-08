using UnityEngine;

public class ReplenishZone : MonoBehaviour
{
    [Tooltip("Restore player HP to full when entering this zone")]
    [SerializeField] private bool replenishHP = true;

    [Tooltip("Restore player shields to full when entering this zone")]
    [SerializeField] private bool replenishShields = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (replenishHP)
        {
            PlayerCombat combat = other.GetComponent<PlayerCombat>();
            if (combat != null)
                combat.Heal(combat.GetMaxHP());
        }

        if (replenishShields)
        {
            PlayerShield shield = other.GetComponent<PlayerShield>();
            if (shield != null)
                shield.ReplenishShields();
        }
    }
}
