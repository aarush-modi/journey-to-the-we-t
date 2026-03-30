using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    public GameObject inventoryPanel;
    public GameObject slotPrefab;
    public int slotCount;
    public GameObject[] skillPrefabs;

    void Start()
    {
        for(int i = 0; i < slotCount; i++)
        {
            InventorySlot slot = Instantiate(slotPrefab, inventoryPanel.transform).GetComponent<InventorySlot>();
            if(i < skillPrefabs.Length)
            {
                GameObject item = Instantiate(skillPrefabs[i], slot.transform);
                item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                slot.currentItem = item;
            }
        }
    }
}
