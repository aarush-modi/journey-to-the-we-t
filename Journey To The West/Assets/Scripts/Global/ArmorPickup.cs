using UnityEngine;

/// <summary>
/// A world-space armor item the player walks over to equip.
/// Used by the Level 5 boss to drop the Gold-Trimmed Robe.
/// Implements ICollectible so it works with the existing pickup system.
/// </summary>
public class ArmorPickup : MonoBehaviour, ICollectible
{
    [SerializeField] private ArmorData armorData;

    public void Collect(GameObject collector)
    {
        PlayerCombat combat = collector.GetComponent<PlayerCombat>();
        if (combat == null) return;
        combat.EquipArmor(armorData);
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            Collect(other.gameObject);
    }
}
