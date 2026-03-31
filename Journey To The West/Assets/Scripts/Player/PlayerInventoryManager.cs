using System.Collections.Generic;
using UnityEngine;

public class PlayerInventoryManager : MonoBehaviour
{
    public static PlayerInventoryManager Instance { get; private set; }

    private List<SkillData> ownedSkills = new List<SkillData>();
    public System.Action OnInventoryChanged;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public bool OwnsSkill(SkillData data) => ownedSkills.Contains(data);

    public void AddSkill(SkillData data)
    {
        if (OwnsSkill(data)) return;
        ownedSkills.Add(data);
        OnInventoryChanged?.Invoke();
    }

    public List<SkillData> GetOwnedSkills() => ownedSkills;
}