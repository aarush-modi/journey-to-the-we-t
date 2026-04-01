using System.Collections.Generic;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    public GameObject inventoryPanel;
    public GameObject slotPrefab;
    public int slotCount;
    public GameObject[] skillPrefabs;

    private List<InventorySlot> slots = new List<InventorySlot>();

    void Start()
    {
        for (int i = 0; i < slotCount; i++)
        {
            InventorySlot slot = Instantiate(slotPrefab, inventoryPanel.transform)
                                    .GetComponent<InventorySlot>();
            slots.Add(slot);

            if (i < skillPrefabs.Length && skillPrefabs[i] != null)
            {
                GameObject item = Instantiate(skillPrefabs[i], slot.transform);
                item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                slot.currentItem = item;
            }
        }
    }

    public InventorySlot GetFirstEmptySlot()
    {
        foreach (InventorySlot slot in slots)
        {
            if (slot.currentItem == null)
                return slot;
        }
        return null;
    }
}