using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerCombat playerCombat;
    [SerializeField] private GreedMeter greedMeter;

    [Header("HP Bar")]
    [SerializeField] private Slider hpBar;
    [SerializeField] private TextMeshProUGUI hpText;

    [Header("Greed Meter")]
    [SerializeField] private Slider greedSlider;
    [SerializeField] private Image greedFill;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private Color tierNoneColor = Color.gray;
    [SerializeField] private Color tier1Color = Color.yellow;
    [SerializeField] private Color tier2Color = new Color(1f, 0.5f, 0f); // orange
    [SerializeField] private Color tier3Color = Color.red;

    private void OnEnable()
    {
        if (playerCombat != null)
        {
            playerCombat.OnHPChanged.AddListener(UpdateHP);
        }

        if (greedMeter != null)
        {
            greedMeter.OnGoldChanged.AddListener(UpdateGold);
            greedMeter.OnTierChanged.AddListener(UpdateGreedTier);
        }
    }

    private void OnDisable()
    {
        if (playerCombat != null)
        {
            playerCombat.OnHPChanged.RemoveListener(UpdateHP);
        }

        if (greedMeter != null)
        {
            greedMeter.OnGoldChanged.RemoveListener(UpdateGold);
            greedMeter.OnTierChanged.RemoveListener(UpdateGreedTier);
        }
    }

    private void Start()
    {
        if (playerCombat != null)
        {
            hpBar.minValue = 0f;
            hpBar.maxValue = playerCombat.GetMaxHP();
            hpBar.value = playerCombat.GetCurrentHP();
        }

        if (greedMeter != null)
        {
            greedSlider.minValue = 0f;
            greedSlider.maxValue = 600f;
            UpdateGold(greedMeter.GetCurrentGold());
            UpdateGreedTier(greedMeter.GetCurrentTier());
        }
    }

    private void UpdateHP(float current, float max)
    {
        hpBar.maxValue = max;
        hpBar.value = current;
        if (hpText != null)
            hpText.text = $"{current:0}";
    }

    private void UpdateGold(int gold)
    {
        greedSlider.value = gold;
        goldText.text = gold.ToString();
    }

    private void UpdateGreedTier(GreedTier tier)
    {
        if (greedFill == null) return;

        greedFill.color = tier switch
        {
            GreedTier.Tier1 => tier1Color,
            GreedTier.Tier2 => tier2Color,
            GreedTier.Tier3 => tier3Color,
            _ => tierNoneColor
        };
    }
}
