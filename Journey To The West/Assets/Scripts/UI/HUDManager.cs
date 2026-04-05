using System.Collections;
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
        BindPlayerReferences();

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
        BindPlayerReferences();

        // Initialize all HUD elements to current state
        if (playerCombat != null)
        {
            hpBar.minValue = 0f;
            hpBar.maxValue = playerCombat.GetMaxHP();
            UpdateHP(playerCombat.GetCurrentHP(), playerCombat.GetMaxHP());

        }

        if (greedMeter != null)
        {
            greedSlider.minValue = 0f;
            greedSlider.maxValue = 600f;
            UpdateGold(greedMeter.GetCurrentGold());
            UpdateGreedTier(greedMeter.GetCurrentTier());
        }
    }

    private void BindPlayerReferences()
    {
        if (playerCombat != null && greedMeter != null)
        {
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            return;
        }

        if (playerCombat == null)
        {
            playerCombat = player.GetComponent<PlayerCombat>();
        }

        if (greedMeter == null)
        {
            greedMeter = player.GetComponent<GreedMeter>();
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
        Debug.Log($"UpdateGreedTier called: tier={tier}");
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
