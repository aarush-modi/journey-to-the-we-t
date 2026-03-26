using UnityEngine;

[CreateAssetMenu(fileName = "NewHustleStyle", menuName = "Scriptable Objects/Hustle Style Data")]
public class HustleStyleData : ScriptableObject
{
    public string styleName;
    [TextArea] public string description;
    public Sprite sprite;

    [Header("Character Sprites (auto-populated by Tools > Setup Hustle Style Sprites)")]
    public Sprite[] idleSprites;
    public Sprite[] walkSprites;
    public Sprite[] attackSprites;

    public float combatGoldModifier = 1f;
    public float npcGoldModifier = 1f;
    public float shopPriceModifier = 1f;
    public int bonusGold = 0;
    public float maxHPModifier = 1f;
}
