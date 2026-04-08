/// <summary>
/// Pure C# logic extracted from GreedMeter for testability.
/// </summary>
public static class GreedMeterLogic
{
    public static GreedTier CalculateTier(int gold)
    {
        if (gold >= 1200) return GreedTier.Tier4;
        if (gold >= 900) return GreedTier.Tier3;
        if (gold >= 600) return GreedTier.Tier2;
        if (gold >= 300) return GreedTier.Tier1;
        return GreedTier.None;
    }

    public static float GetBonusDamage(GreedTier tier)
    {
        float bonus = 0f;
        if (tier >= GreedTier.Tier3) bonus += 20f;
        return bonus;
    }

    public static float GetBonusSpeed(GreedTier tier)
    {
        float bonus = 0f;
        if (tier >= GreedTier.Tier2) bonus += 2f;
        if (tier >= GreedTier.Tier3) bonus += 2f;
        if (tier >= GreedTier.Tier4) bonus += 2f;
        return bonus;
    }

    public static float GetBonusHP(GreedTier tier)
    {
        float bonus = 0f;
        if (tier >= GreedTier.Tier2) bonus += 20f;
        if (tier >= GreedTier.Tier4) bonus += 10f;
        return bonus;
    }

    public static int GetShieldCount(GreedTier tier)
    {
        int count = 0;
        if (tier >= GreedTier.Tier1) count += 1;
        if (tier >= GreedTier.Tier3) count += 1;
        if (tier >= GreedTier.Tier4) count += 1;
        return count;
    }

    public static int ApplyStyleModifier(int baseAmount, float modifier)
    {
        if (baseAmount <= 0) return 0;
        int result = UnityEngine.Mathf.RoundToInt(baseAmount * modifier);
        return UnityEngine.Mathf.Max(0, result);
    }

    public static int[] GetTierThresholds() => new[] { 300, 600, 900, 1200 };

    public static int GetMaxThreshold() => 1200;

    public static string GetBuffText(GreedTier tier)
    {
        float dmg = GetBonusDamage(tier);
        float spd = GetBonusSpeed(tier);
        float hp = GetBonusHP(tier);
        int shd = GetShieldCount(tier);
        return $"+{dmg:0} DMG  +{spd:0} SPD  +{hp:0} HP  +{shd} SHD";
    }
}
