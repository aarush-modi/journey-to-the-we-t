using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestLogUI : MonoBehaviour
{
    [SerializeField] private GameObject questEntryPrefab;
    [SerializeField] private Transform contentParent;

    private Dictionary<QuestData, GameObject> entries = new Dictionary<QuestData, GameObject>();

    private void OnEnable()
    {
        RefreshAll();
        StartCoroutine(RebuildLayoutNextFrame());

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.onQuestStarted.AddListener(OnQuestStarted);
            QuestManager.Instance.onQuestCompleted.AddListener(OnQuestCompleted);
        }
    }

    private IEnumerator RebuildLayoutNextFrame()
    {
        yield return null;
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent as RectTransform);
    }

    private void OnDisable()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.onQuestStarted.RemoveListener(OnQuestStarted);
            QuestManager.Instance.onQuestCompleted.RemoveListener(OnQuestCompleted);
        }
    }

    private void RefreshAll()
    {
        foreach (var entry in entries.Values)
        {
            Destroy(entry);
        }
        entries.Clear();

        if (QuestManager.Instance == null) return;

        foreach (var quest in QuestManager.Instance.GetActiveQuests())
        {
            AddEntry(quest);
        }
    }

    private void AddEntry(QuestData quest)
    {
        if (entries.ContainsKey(quest)) return;

        GameObject entry = Instantiate(questEntryPrefab, contentParent);
        entry.GetComponent<QuestEntryUI>().Setup(quest);
        entries[quest] = entry;
    }

    private void RemoveEntry(QuestData quest)
    {
        if (!entries.TryGetValue(quest, out GameObject entry)) return;

        Destroy(entry);
        entries.Remove(quest);
    }

    private void OnQuestStarted(QuestData quest)
    {
        AddEntry(quest);
    }

    private void OnQuestCompleted(QuestData quest)
    {
        RemoveEntry(quest);
    }
}
