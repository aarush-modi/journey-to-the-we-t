using UnityEngine;
using TMPro;

public class SkillTooltip : MonoBehaviour
{
    public static SkillTooltip Instance { get; private set; }

    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text skillNameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text costText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        panel.SetActive(false);
    }

    public void Show(SkillData data)
    {
        skillNameText.text = data.skillName;
        descriptionText.text = data.description;
        costText.text = data.goldCost > 0 ? $"Cost: {data.goldCost}g" : "";
        panel.SetActive(true);
    }

    public void Hide()
    {
        panel.SetActive(false);
    }

    private void Update()
    {
        // Tooltip follows mouse
        if (panel.activeSelf)
            transform.position = Input.mousePosition + new Vector3(15f, -15f, 0f);
    }
}