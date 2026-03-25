using UnityEngine;

[CreateAssetMenu(fileName = "NewArmor", menuName = "Scriptable Objects/Armor Data")]
public class ArmorData : ScriptableObject
{
    public string armorName;
    public float damageReduction;
    public Sprite armorSprite;
}
