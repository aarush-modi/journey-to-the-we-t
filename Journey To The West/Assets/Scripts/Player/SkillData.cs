using UnityEngine;

[CreateAssetMenu(fileName = "NewSkill", menuName = "Scriptable Objects/Skill Data")]
public class SkillData : ScriptableObject
{
    public string skillName;
    public float damage;
    public float cooldown;
    public Sprite icon;
}
