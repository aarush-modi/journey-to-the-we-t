using UnityEngine;
using UnityEngine.InputSystem;

public class GreedMeterDebug : MonoBehaviour
{
    private GreedMeter greedMeter;

    void Start()
    {
        greedMeter = GetComponent<GreedMeter>();
        greedMeter.OnGoldChanged.AddListener(gold => Debug.Log($"Gold: {gold}"));
        greedMeter.OnTierChanged.AddListener(tier => Debug.Log($"Tier changed: {tier}"));
    }

    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.gKey.wasPressedThisFrame)
        {
            greedMeter.AddGold(100);
            Debug.Log($"Tier: {greedMeter.GetCurrentTier()} | DMG: +{greedMeter.GetBonusDamage()} | SPD: +{greedMeter.GetBonusSpeed()} | HP: +{greedMeter.GetBonusHP()} | SHD: {greedMeter.GetShieldCount()}");
        }
        if (keyboard.hKey.wasPressedThisFrame)
        {
            greedMeter.RemoveGold(150);
            Debug.Log($"Tier: {greedMeter.GetCurrentTier()} | DMG: +{greedMeter.GetBonusDamage()} | SPD: +{greedMeter.GetBonusSpeed()} | HP: +{greedMeter.GetBonusHP()} | SHD: {greedMeter.GetShieldCount()}");
        }
    }
}
