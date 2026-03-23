using UnityEngine;

public enum ItemType
{
    Consumable,
    QuestItem,
    Equipment,
    Misc
}

[CreateAssetMenu(fileName = "NewItem", menuName = "Scriptable Objects/Item Data")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public string description;
    public Sprite icon;
    public ItemType itemType;
}
