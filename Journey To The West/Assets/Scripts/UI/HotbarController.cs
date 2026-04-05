using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HotbarController : MonoBehaviour
{
    public GameObject hotbarPanel;
    public GameObject slotPrefab;
    public int slotCount = 10; //1-0 on the keyboard

    [Header("Starting Skills")]
    [SerializeField] private SkillData[] startingSkills;

    private Key[] hotbarKeys;
    private PlayerCombat playerCombat;
    private int activeSlotIndex = -1;

    private void Awake()
    {
        // Hotbar keys based on slot count
        hotbarKeys = new Key[slotCount];
        for(int i = 0; i < slotCount; i++)
        {
            hotbarKeys[i] = i < 9 ? (Key)((int)Key.Digit1 + i) : Key.Digit0;
        }
    }

    void Start()
    {
        for (int i = 0; i < slotCount; i++)
        {
            Instantiate(slotPrefab, hotbarPanel.transform);
        }
        playerCombat = GameObject.FindWithTag("Player")?.GetComponent<PlayerCombat>();

        // Populate starting skills into hotbar slots
        if (startingSkills != null)
        {
            for (int i = 0; i < startingSkills.Length && i < slotCount; i++)
            {
                if (startingSkills[i] != null)
                    AddSkillToSlot(i, startingSkills[i]);
            }
        }

        // Subscribe to PlayerCombat events
        if (playerCombat != null)
        {
            playerCombat.OnSkillActivated.AddListener(HandleSkillActivated);
            playerCombat.OnSkillCooldownReset.AddListener(HandleSkillCooldownReset);
        }

    }

    private void AddSkillToSlot(int slotIndex, SkillData skillData)
    {
        InventorySlot slot = hotbarPanel.transform.GetChild(slotIndex).GetComponent<InventorySlot>();
        if (slot == null || slot.currentItem != null) return;

        GameObject item = new GameObject(skillData.skillName);
        item.transform.SetParent(slot.transform, false);

        // Add Image with skill icon
        Image image = item.AddComponent<Image>();
        image.sprite = skillData.icon;
        image.raycastTarget = true;
        image.preserveAspect = true;

        // Fill the slot
        RectTransform rt = item.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;

        // Add Skill component
        Skill skill = item.AddComponent<Skill>();
        skill.data = skillData;

        // Add CanvasGroup + drag handler so it can be rearranged
        item.AddComponent<CanvasGroup>();
        item.AddComponent<ItemDragHandler>();

        slot.currentItem = item;
    }

    void Update()
    {
        if (PauseController.IsGamePaused) return;

        // Check for key presses
        for (int i = 0; i < slotCount; i++)
        {
            if (Keyboard.current[hotbarKeys[i]].wasPressedThisFrame)
            {
                UseItemInSlot(i);
            }
        }
    }

    void UseItemInSlot(int index)
    {
        InventorySlot slot = hotbarPanel.transform.GetChild(index).GetComponent<InventorySlot>();
        if (slot.currentItem != null)
        {
            Skill skill = slot.currentItem.GetComponent<Skill>();
            if (skill != null && skill.data != null)
            {
                // Deactivate the previous slot highlight
                if (activeSlotIndex >= 0)
                {
                    InventorySlot prevSlot = hotbarPanel.transform.GetChild(activeSlotIndex).GetComponent<InventorySlot>();
                    if (prevSlot != null)
                    {
                        prevSlot.SetActive(false);
                    }
                }

                // Activate the new slot
                activeSlotIndex = index;
                slot.SetActive(true);

                if (playerCombat != null)
                {
                    playerCombat.EquipSkill(skill.data);
                }
            }
        }
        else
        {
            Debug.LogWarning($"No Skill component found on item in hotbar slot {index}");
        }
    }

    private InventorySlot GetActiveSlot()
    {
        if (activeSlotIndex < 0 || activeSlotIndex >= hotbarPanel.transform.childCount)
            return null;
        return hotbarPanel.transform.GetChild(activeSlotIndex).GetComponent<InventorySlot>();
    }

    private void HandleSkillActivated(float cooldown)
    {
        InventorySlot slot = GetActiveSlot();
        if (slot != null)
        {
            slot.StartCooldown(cooldown);
        }
    }

    private void HandleSkillCooldownReset()
    {
        InventorySlot slot = GetActiveSlot();
        Debug.Log($"[Hotbar] HandleSkillCooldownReset — activeSlotIndex={activeSlotIndex}, slot={slot}");
        if (slot != null)
        {
            slot.ResetCooldown();
        }
    }

    private void OnDestroy()
    {
        if (playerCombat != null)
        {
            playerCombat.OnSkillActivated.RemoveListener(HandleSkillActivated);
            playerCombat.OnSkillCooldownReset.RemoveListener(HandleSkillCooldownReset);
        }
    }
}
