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

    public void RemoveGold(int amount)
    {
        if (amount <= 0) return;
        currentGold = Mathf.Max(0, currentGold - amount);
        RecalculateTier();
        OnGoldChanged?.Invoke(currentGold);
    }

    public int GetCurrentGold() => currentGold;

    public GreedTier GetCurrentTier() => currentTier;

    public float GetDamageMultiplier() => currentTier >= GreedTier.Tier1 ? 1.1f : 1.0f;

    public float GetSpeedMultiplier() => currentTier >= GreedTier.Tier2 ? 1.15f : 1.0f;

    public float GetHPMultiplier() => currentTier >= GreedTier.Tier3 ? 1.2f : 1.0f;

    public bool IsDesperate() => currentGold < 50;

    private void RecalculateTier()
    {
        GreedTier newTier;

        if (currentGold >= 600)
            newTier = GreedTier.Tier3;
        else if (currentGold >= 300)
            newTier = GreedTier.Tier2;
        else if (currentGold >= 100)
            newTier = GreedTier.Tier1;
        else
            newTier = GreedTier.None;

        if (newTier != currentTier)
        {
            currentTier = newTier;
            OnTierChanged?.Invoke(currentTier);
        }
    }
}

public enum GreedTier
{
    None = 0,
    Tier1 = 1,
    Tier2 = 2,
    Tier3 = 3
}
