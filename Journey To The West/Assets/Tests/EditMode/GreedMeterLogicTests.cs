using NUnit.Framework;

[TestFixture]
public class GreedMeterLogicTests
{
    // --- Tier calculation tests ---

    [TestCase(0, GreedTier.None)]
    [TestCase(50, GreedTier.None)]
    [TestCase(299, GreedTier.None)]
    [TestCase(300, GreedTier.Tier1)]
    [TestCase(500, GreedTier.Tier1)]
    [TestCase(599, GreedTier.Tier1)]
    [TestCase(600, GreedTier.Tier2)]
    [TestCase(899, GreedTier.Tier2)]
    [TestCase(900, GreedTier.Tier3)]
    [TestCase(1199, GreedTier.Tier3)]
    [TestCase(1200, GreedTier.Tier4)]
    [TestCase(2000, GreedTier.Tier4)]
    public void CalculateTier_ReturnsCorrectTier(int gold, GreedTier expected)
    {
        Assert.AreEqual(expected, GreedMeterLogic.CalculateTier(gold));
    }

    // --- Bonus damage tests ---

    [Test]
    public void GetBonusDamage_Tier3OrHigher_Returns20()
    {
        Assert.AreEqual(20f, GreedMeterLogic.GetBonusDamage(GreedTier.Tier3));
        Assert.AreEqual(20f, GreedMeterLogic.GetBonusDamage(GreedTier.Tier4));
    }

    [Test]
    public void GetBonusDamage_BelowTier3_Returns0()
    {
        Assert.AreEqual(0f, GreedMeterLogic.GetBonusDamage(GreedTier.None));
        Assert.AreEqual(0f, GreedMeterLogic.GetBonusDamage(GreedTier.Tier1));
        Assert.AreEqual(0f, GreedMeterLogic.GetBonusDamage(GreedTier.Tier2));
    }

    // --- Bonus speed tests ---

    [Test]
    public void GetBonusSpeed_ReturnsCorrectCumulative()
    {
        Assert.AreEqual(0f, GreedMeterLogic.GetBonusSpeed(GreedTier.None));
        Assert.AreEqual(0f, GreedMeterLogic.GetBonusSpeed(GreedTier.Tier1));
        Assert.AreEqual(15f, GreedMeterLogic.GetBonusSpeed(GreedTier.Tier2));
        Assert.AreEqual(25f, GreedMeterLogic.GetBonusSpeed(GreedTier.Tier3));
        Assert.AreEqual(35f, GreedMeterLogic.GetBonusSpeed(GreedTier.Tier4));
    }

    // --- Bonus HP tests ---

    [Test]
    public void GetBonusHP_ReturnsCorrectCumulative()
    {
        Assert.AreEqual(0f, GreedMeterLogic.GetBonusHP(GreedTier.None));
        Assert.AreEqual(0f, GreedMeterLogic.GetBonusHP(GreedTier.Tier1));
        Assert.AreEqual(20f, GreedMeterLogic.GetBonusHP(GreedTier.Tier2));
        Assert.AreEqual(20f, GreedMeterLogic.GetBonusHP(GreedTier.Tier3));
        Assert.AreEqual(30f, GreedMeterLogic.GetBonusHP(GreedTier.Tier4));
    }

    // --- Shield count tests ---

    [Test]
    public void GetShieldCount_ReturnsCorrectCumulative()
    {
        Assert.AreEqual(0, GreedMeterLogic.GetShieldCount(GreedTier.None));
        Assert.AreEqual(1, GreedMeterLogic.GetShieldCount(GreedTier.Tier1));
        Assert.AreEqual(1, GreedMeterLogic.GetShieldCount(GreedTier.Tier2));
        Assert.AreEqual(2, GreedMeterLogic.GetShieldCount(GreedTier.Tier3));
        Assert.AreEqual(3, GreedMeterLogic.GetShieldCount(GreedTier.Tier4));
    }

    // --- ApplyStyleModifier tests ---

    [Test]
    public void ApplyStyleModifier_ZeroBase_ReturnsZero()
    {
        Assert.AreEqual(0, GreedMeterLogic.ApplyStyleModifier(0, 1.5f));
    }

    [Test]
    public void ApplyStyleModifier_NegativeBase_ReturnsZero()
    {
        Assert.AreEqual(0, GreedMeterLogic.ApplyStyleModifier(-10, 1.5f));
        Assert.AreEqual(0, GreedMeterLogic.ApplyStyleModifier(-1, 2.0f));
    }

    [Test]
    public void ApplyStyleModifier_PositiveBase_ReturnsModified()
    {
        Assert.AreEqual(110, GreedMeterLogic.ApplyStyleModifier(100, 1.1f));
        Assert.AreEqual(100, GreedMeterLogic.ApplyStyleModifier(100, 1.0f));
        Assert.AreEqual(12, GreedMeterLogic.ApplyStyleModifier(10, 1.15f));
        Assert.AreEqual(60, GreedMeterLogic.ApplyStyleModifier(50, 1.2f));
    }

    // --- GetTierThresholds tests ---

    [Test]
    public void GetTierThresholds_ReturnsAscendingOrder()
    {
        int[] thresholds = GreedMeterLogic.GetTierThresholds();
        Assert.AreEqual(new[] { 300, 600, 900, 1200 }, thresholds);
    }

    // --- GetMaxThreshold tests ---

    [Test]
    public void GetMaxThreshold_Returns1200()
    {
        Assert.AreEqual(1200, GreedMeterLogic.GetMaxThreshold());
    }

    // --- GetBuffText tests ---

    [Test]
    public void GetBuffText_None_ShowsAllZeroes()
    {
        Assert.AreEqual("+0 DMG  +0 SPD  +0 HP  +0 SHD", GreedMeterLogic.GetBuffText(GreedTier.None));
    }

    [Test]
    public void GetBuffText_Tier1_ShowsShieldOnly()
    {
        Assert.AreEqual("+0 DMG  +0 SPD  +0 HP  +1 SHD", GreedMeterLogic.GetBuffText(GreedTier.Tier1));
    }

    [Test]
    public void GetBuffText_Tier2_ShowsHPAndSpeed()
    {
        Assert.AreEqual("+0 DMG  +15 SPD  +20 HP  +1 SHD", GreedMeterLogic.GetBuffText(GreedTier.Tier2));
    }

    [Test]
    public void GetBuffText_Tier3_ShowsDamageSpeedShields()
    {
        Assert.AreEqual("+20 DMG  +25 SPD  +20 HP  +2 SHD", GreedMeterLogic.GetBuffText(GreedTier.Tier3));
    }

    [Test]
    public void GetBuffText_Tier4_ShowsAll()
    {
        Assert.AreEqual("+20 DMG  +35 SPD  +30 HP  +3 SHD", GreedMeterLogic.GetBuffText(GreedTier.Tier4));
    }
}
