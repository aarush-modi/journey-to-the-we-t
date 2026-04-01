using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopSlot : MonoBehaviour
{
    [Header("UI References")]
    public Image iconImage;
    public TMP_Text nameText;
    public TMP_Text priceText;
    public Button buyButton;

    private SkillData skillData;
    private MerchantShopController shopController;

    public void SetSkill(SkillData data, MerchantShopController controller)
    {
        skillData = data;
        shopController = controller;

        if (iconImage != null) iconImage.sprite = data.icon;
        if (nameText != null) nameText.text = data.skillName;
        if (priceText != null) priceText.text = data.goldCost.ToString() + "g";

        if (buyButton != null)
            buyButton.onClick.AddListener(OnBuyClicked);

        RefreshAffordability();
    }

    public void RefreshAffordability()
    {
        if (buyButton == null) return;

        GreedMeter greed = GameObject.FindWithTag("Player")?.GetComponent<GreedMeter>();
        bool canAfford = greed != null && greed.GetCurrentGold() >= skillData.goldCost;

        buyButton.interactable = canAfford;

        // Grey out the icon and text if can't afford
        Color affordable = canAfford ? Color.white : new Color(0.4f, 0.4f, 0.4f, 1f);
        if (iconImage != null) iconImage.color = affordable;
        if (nameText != null) nameText.color = affordable;
        if (priceText != null) priceText.color = affordable;
    }

    private void OnBuyClicked()
    {
        if (shopController != null && skillData != null)
            shopController.TryBuySkill(skillData, this);
    }
}