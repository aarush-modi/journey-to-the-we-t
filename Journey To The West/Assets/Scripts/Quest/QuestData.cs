using UnityEngine;

public enum QuestType
{
    Main,
    Side
}

[CreateAssetMenu(fileName = "NewQuest", menuName = "Scriptable Objects/Quest Data")]
public class QuestData : ScriptableObject
{
    public string questName;
    [TextArea(2, 4)]
    public string description;
    public QuestType questType;
    public bool isCompleted;
}
