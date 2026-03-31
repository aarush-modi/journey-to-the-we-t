using UnityEngine;
using UnityEngine.InputSystem;

public class HotbarController : MonoBehaviour
{
    public GameObject hotbarPanel;
    public GameObject slotPrefab;
    public int slotCount = 10; //1-0 on the keyboard

    private SkillDictionary skillDictionary;

    private Key[] hotbarKeys;

    private void Awake()
    {
        skillDictionary = FindObjectOfType<SkillDictionary>();

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
    }

    void Update()
    {
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
            if (skill != null)
            {
                skill.UseSkill();
            }
        }
        else
            {
                Debug.LogWarning($"No Skill component found on item in hotbar slot {index}");
            }
    }
}