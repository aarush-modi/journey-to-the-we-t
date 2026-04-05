using UnityEngine;

public class Skill : MonoBehaviour
{
    public SkillData data;

    public string SkillName => data != null ? data.skillName : "Unknown";
    public int GoldCost => data != null ? data.goldCost : 0;

    public virtual void UseSkill()
    {
        Debug.Log("Using skill: " + SkillName);
        if (data != null)
            data.Activate(GetComponentInParent<PlayerController>());
    }
}