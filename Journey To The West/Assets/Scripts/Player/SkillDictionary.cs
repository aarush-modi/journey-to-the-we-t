using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillDictionary : MonoBehaviour
{
    public List<Skill> skillPrefabs;
    private Dictionary<int, GameObject> skillDictionary;

    private void Awake()
    {
        skillDictionary = new Dictionary<int, GameObject>();

        for (int i = 0; i < skillPrefabs.Count; i++)
        {
            if (skillPrefabs[i] != null)
            {
                int id = i + 1;
                skillDictionary[id] = skillPrefabs[i].gameObject;
                skillPrefabs[i].ID = id;
            }
        }
    }

    public GameObject GetSkillPrefab(int skillID)
    {
        skillDictionary.TryGetValue(skillID, out GameObject prefab);
        if(prefab == null)
        {
            Debug.LogWarning("Skill with ID " + skillID + " not found in dictionary");
        }
        return prefab;
    }
}