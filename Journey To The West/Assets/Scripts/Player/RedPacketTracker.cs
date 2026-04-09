using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class RedPacketTracker : MonoBehaviour
{
    public static RedPacketTracker Instance { get; private set; }

    [SerializeField] private QuestData[] dojoQuests;
    [SerializeField] private string[] mapNames;
    [SerializeField] private string[] mapSceneNames;

    public UnityEvent onRedPacketCollected;

    private HashSet<string> collectedScenes = new HashSet<string>();

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

    private bool subscribed;

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void Update()
    {
        if (!subscribed)
            TrySubscribe();
    }

    private void TrySubscribe()
    {
        if (subscribed) return;
        if (QuestManager.Instance == null) return;

        QuestManager.Instance.onQuestCompleted.AddListener(OnQuestCompleted);
        subscribed = true;
    }

    private void OnDisable()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.onQuestCompleted.RemoveListener(OnQuestCompleted);
        }
        subscribed = false;
    }

    private void OnQuestCompleted(QuestData quest)
    {
        if (Array.IndexOf(dojoQuests, quest) >= 0)
        {
            Collect("quest:" + quest.questName);
        }
    }

    public void Collect(string sceneName)
    {
        if (collectedScenes.Add(sceneName))
            onRedPacketCollected?.Invoke();
    }

    public int GetCount()
    {
        return collectedScenes.Count;
    }

    public bool IsCollected(string sceneName)
    {
        return collectedScenes.Contains(sceneName);
    }

    public string[] GetMapNames()
    {
        return mapNames;
    }

    public string[] GetMapSceneNames()
    {
        return mapSceneNames;
    }
}
