using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    private List<QuestData> activeQuests = new List<QuestData>();
    private List<QuestData> completedQuests = new List<QuestData>();

    // TODO: Remove startingQuests once NPCs call StartQuest() directly
    [Header("Starting Quests (for testing)")]
    [SerializeField] private QuestData[] startingQuests;

    public UnityEvent<QuestData> onQuestStarted;
    public UnityEvent<QuestData> onQuestCompleted;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        foreach (var quest in startingQuests)
        {
            StartQuest(quest);
        }
    }

    public void StartQuest(QuestData quest)
    {
        if (activeQuests.Contains(quest) || completedQuests.Contains(quest)) return;

        activeQuests.Add(quest);
        onQuestStarted?.Invoke(quest);
    }

    public void CompleteQuest(QuestData quest)
    {
        if (!activeQuests.Contains(quest)) return;

        activeQuests.Remove(quest);
        quest.isCompleted = true;
        completedQuests.Add(quest);
        onQuestCompleted?.Invoke(quest);
    }

    public bool IsQuestActive(QuestData quest)
    {
        return activeQuests.Contains(quest);
    }

    public bool IsQuestCompleted(QuestData quest)
    {
        return completedQuests.Contains(quest);
    }

    public QuestData GetActiveQuestByName(string questName)
    {
        return activeQuests.FirstOrDefault(q => q.questName == questName);
    }

    public IReadOnlyList<QuestData> GetActiveQuests()
    {
        return activeQuests;
    }

    public IReadOnlyList<QuestData> GetCompletedQuests()
    {
        return completedQuests;
    }
}
