using UnityEngine;

[CreateAssetMenu(fileName = "NewHustleStyle", menuName = "Scriptable Objects/Hustle Style Data")]
public class HustleStyleData : ScriptableObject
{
    public string styleName;
    [TextArea] public string description;
    public Sprite sprite;
    public float combatGoldModifier = 1f;
    public float npcGoldModifier = 1f;
    public float shopPriceModifier = 1f;
    public int bonusGold = 0;
    public float maxHPModifier = 1f;
}
