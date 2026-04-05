using UnityEngine;

public abstract class SkillData : ScriptableObject
{
    public string skillName;
    [TextArea] public string description;
    public Sprite icon;
    public float cooldown;
    public int goldCost;
    public GameObject skillPrefab;

    public abstract void Activate(PlayerController user);
}