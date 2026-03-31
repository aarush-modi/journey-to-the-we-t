using UnityEngine;

public class Skill : MonoBehaviour
{
    public int ID;
    public string skillName;
    public SkillData data;
    
    public virtual void UseSkill()
        {
            Debug.Log("Using skill: " + skillName);
        }
}
