using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MerchantShopController : MonoBehaviour
{
    public static MerchantShopController Instance;

    [Header("UI References")]
    [SerializeField] private GameObject shopMenuUI;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private Transform shopInventoryGrid;
    [SerializeField] private GameObject shopSlotPrefab;
    [SerializeField] private TMP_Text playerMoneyText;

    [Header("Shop Stock")]
    [SerializeField] private List<SkillData> skillsForSale = new List<SkillData>();

    [Header("References")]
    [SerializeField] private NPCBase npcBase;

    private GreedMeter playerGreed;
    private InventoryController playerInventory;
    // Tracks which skills have been purchased so they don't reappear
    private HashSet<SkillData> purchasedSkills = new HashSet<SkillData>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (npcBase == null)
            npcBase = GetComponent<NPCBase>();

        if (shopMenuUI != null)
            shopMenuUI.SetActive(false);
    }

    private void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerGreed = player.GetComponent<GreedMeter>();

            if (playerGreed != null)
                playerGreed.OnGoldChanged.AddListener(OnPlayerGoldChanged);
        }

        // Find InventoryController on GameController
        GameObject gameController = GameObject.FindWithTag("GameController");
        if (gameController != null)
            playerInventory = gameController.GetComponent<InventoryController>();

        if (playerGreed == null)
            Debug.LogWarning("MerchantShopController: No GreedMeter found on Player.");
    }

    public void OpenShop()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (shopMenuUI != null)
            shopMenuUI.SetActive(true);

        RefreshShopDisplay();
        UpdateMoneyDisplay();
    }

    public void CloseShop()
    {
        if (shopMenuUI != null)
            shopMenuUI.SetActive(false);

        PauseController.SetPause(false);
    }

    private void RefreshShopDisplay()
    {
        // Clear existing slots
        foreach (Transform child in shopInventoryGrid)
            Destroy(child.gameObject);

        foreach (SkillData skill in skillsForSale)
        {
            // Skip already purchased skills
            if (purchasedSkills.Contains(skill)) continue;

            GameObject slotObj = Instantiate(shopSlotPrefab, shopInventoryGrid);
            ShopSlot slot = slotObj.GetComponent<ShopSlot>();
            if (slot != null)
                slot.SetSkill(skill, this);
        }
    }

    public void TryBuySkill(SkillData skill, ShopSlot slot)
    {
        if (playerGreed == null || playerInventory == null) return;

        if (playerGreed.GetCurrentGold() < skill.goldCost)
        {
            Debug.Log("Not enough gold to buy: " + skill.skillName);
            return;
        }

        if (skill.skillPrefab == null)
        {
            Debug.LogWarning("SkillData has no prefab assigned: " + skill.skillName);
            return;
        }

        // Find first empty inventory slot
        InventorySlot emptySlot = playerInventory.GetFirstEmptySlot();
        if (emptySlot == null)
        {
            Debug.Log("No empty inventory slots available.");
            return;
        }

        // Deduct gold
        playerGreed.RemoveGold(skill.goldCost);

        // Instantiate skill into inventory slot
        GameObject skillInstance = Instantiate(skill.skillPrefab, emptySlot.transform);
        skillInstance.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        emptySlot.currentItem = skillInstance;

        // Mark as purchased and remove from shop
        purchasedSkills.Add(skill);
        Destroy(slot.gameObject);

        // Refresh affordability on remaining slots
        RefreshAllSlotAffordability();
    }

    private void OnPlayerGoldChanged(int newAmount)
    {
        UpdateMoneyDisplay();
        RefreshAllSlotAffordability();
    }

    private void UpdateMoneyDisplay()
    {
        if (playerMoneyText != null && playerGreed != null)
            playerMoneyText.text = playerGreed.GetCurrentGold().ToString() + "g";
    }

    private void RefreshAllSlotAffordability()
    {
        foreach (Transform child in shopInventoryGrid)
        {
            ShopSlot slot = child.GetComponent<ShopSlot>();
            if (slot != null) slot.RefreshAffordability();
        }
    }

    private void OnDestroy()
    {
        if (playerGreed != null)
            playerGreed.OnGoldChanged.RemoveListener(OnPlayerGoldChanged);
    }
}