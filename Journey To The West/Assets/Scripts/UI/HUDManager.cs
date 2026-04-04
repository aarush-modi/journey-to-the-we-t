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

    [Header("Skill Slot")]
    [SerializeField] private Image skillIcon;
    [SerializeField] private Image skillCooldownOverlay;

    private Coroutine cooldownCoroutine;

    private void OnEnable()
    {
        BindPlayerReferences();

        if (playerCombat != null)
        {
            playerCombat.OnHPChanged.AddListener(UpdateHP);
            playerCombat.OnSkillActivated.AddListener(StartSkillCooldown);
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
            playerCombat.OnSkillActivated.RemoveListener(StartSkillCooldown);
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

            SkillData skill = playerCombat.GetEquippedSkill();
            if (skillIcon != null)
            {
                skillIcon.sprite = skill != null ? skill.icon : null;
                skillIcon.enabled = skill != null && skill.icon != null;
            }

            if (skillCooldownOverlay != null)
            {
                skillCooldownOverlay.fillAmount = 0f;
            }
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

    private void StartSkillCooldown(float cooldownDuration)
    {
        if (skillCooldownOverlay == null) return;

        if (cooldownCoroutine != null)
            StopCoroutine(cooldownCoroutine);

        cooldownCoroutine = StartCoroutine(AnimateCooldown(cooldownDuration));
    }

    private IEnumerator AnimateCooldown(float duration)
    {
        float elapsed = 0f;
        skillCooldownOverlay.fillAmount = 1f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            skillCooldownOverlay.fillAmount = 1f - (elapsed / duration);
            yield return null;
        }

        skillCooldownOverlay.fillAmount = 0f;
        cooldownCoroutine = null;
    }
}
