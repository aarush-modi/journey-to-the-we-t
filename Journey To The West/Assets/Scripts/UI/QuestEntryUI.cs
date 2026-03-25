using TMPro;
using UnityEngine;

public class QuestEntryUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI questNameText;
    [SerializeField] private TextMeshProUGUI objectiveText;

    public void Setup(QuestData quest)
    {
        questNameText.text = quest.questName;
        objectiveText.text = quest.description;
    }
}
