using UnityEngine;
using UnityEngine.Events;

public class GreedMeter : MonoBehaviour
{
    [SerializeField] private int currentGold;

    private GreedTier currentTier = GreedTier.None;

    public UnityEvent<int> OnGoldChanged;
    public UnityEvent<GreedTier> OnTierChanged;

    public void AddGold(int amount)
    {
        if (amount <= 0) return;
        currentGold += amount;
        RecalculateTier();
        OnGoldChanged?.Invoke(currentGold);
    }

    public void AddCombatGold(int baseAmount)
    {
        AddGold(ApplyStyleModifier(baseAmount, HustleStyleManager.Instance?.GetCombatGoldModifier() ?? 1f));
    }

    public void AddNPCGold(int baseAmount)
    {
        AddGold(ApplyStyleModifier(baseAmount, HustleStyleManager.Instance?.GetNPCGoldModifier() ?? 1f));
    }

    public void RemoveGold(int amount)
    {
        if (amount <= 0) return;
        currentGold = Mathf.Max(0, currentGold - amount);
        RecalculateTier();
        OnGoldChanged?.Invoke(currentGold);
    }

    public int GetCurrentGold() => currentGold;

    public GreedTier GetCurrentTier() => currentTier;

    public float GetBonusDamage() => GreedMeterLogic.GetBonusDamage(currentTier);

    public float GetDamageMultiplier() => GreedMeterLogic.GetDamageMultiplier(currentTier);

    public float GetBonusSpeed() => GreedMeterLogic.GetBonusSpeed(currentTier);

    public float GetBonusHP() => GreedMeterLogic.GetBonusHP(currentTier);

    public int GetShieldCount() => GreedMeterLogic.GetShieldCount(currentTier);

    private void RecalculateTier()
    {
        GreedTier newTier = GreedMeterLogic.CalculateTier(currentGold);

        if (newTier != currentTier)
        {
            currentTier = newTier;
            OnTierChanged?.Invoke(currentTier);
        }
    }

    private static int ApplyStyleModifier(int baseAmount, float modifier)
    {
        return GreedMeterLogic.ApplyStyleModifier(baseAmount, modifier);
    }
}

public enum GreedTier
{
    None = 0,
    Tier1 = 1,
    Tier2 = 2,
    Tier3 = 3,
    Tier4 = 4
}
