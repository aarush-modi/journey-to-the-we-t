using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BridgePuzzleNPC : NPCBase
{
    [SerializeField] private NPCDialogue introDialogue;
    [SerializeField] private NPCDialogue noGoldDialogue;
    [SerializeField] private NPCDialogue solvedDialogue;
    [SerializeField] private NPCDialogue failedDialogue;
    [SerializeField] private WordlePuzzleUISetup puzzleSetup;
    [SerializeField] private int goldCost = 25;
    [SerializeField] private GameObject bridgeBlocker;

    private WordlePuzzleUI wordlePuzzleUI;
    private bool hasSolved;
    private bool awaitingPuzzle;
    private GreedMeter playerGreedMeter;

    protected override void Start()
    {
        if (dialoguePanel == null)
            BuildDialogueUI();

        base.Start();

        if (bridgeBlocker != null)
            bridgeBlocker.SetActive(true);
        if (puzzleSetup != null)
            wordlePuzzleUI = puzzleSetup.PuzzleUI;
    }

    private void BuildDialogueUI()
    {
        GameObject canvasObj = new GameObject("BridgeDialogueCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 150;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject panel = MakeRect("DialoguePanel", canvasObj.transform);
        panel.AddComponent<Image>().color = new Color(0.1f, 0.08f, 0.07f, 0.92f);
        Outline outline = panel.AddComponent<Outline>();
        outline.effectColor = new Color(0.35f, 0.28f, 0.20f);
        outline.effectDistance = new Vector2(3, -3);
        RectTransform panelRT = panel.GetComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0.5f, 0);
        panelRT.anchorMax = new Vector2(0.5f, 0);
        panelRT.pivot = new Vector2(0.5f, 0.5f);
        panelRT.sizeDelta = new Vector2(600, 150);
        panelRT.anchoredPosition = new Vector2(0, 100);

        GameObject portraitObj = MakeRect("Portrait", panel.transform);
        Image portrait = portraitObj.AddComponent<Image>();
        portrait.color = Color.white;
        portrait.preserveAspect = true;
        RectTransform portraitRT = portraitObj.GetComponent<RectTransform>();
        portraitRT.anchorMin = new Vector2(0, 0.5f);
        portraitRT.anchorMax = new Vector2(0, 0.5f);
        portraitRT.pivot = new Vector2(0, 0.5f);
        portraitRT.sizeDelta = new Vector2(100, 100);
        portraitRT.anchoredPosition = new Vector2(15, 0);

        GameObject nameObj = MakeRect("NPCName", panel.transform);
        TMP_Text nText = nameObj.AddComponent<TextMeshProUGUI>();
        nText.fontSize = 22;
        nText.fontStyle = FontStyles.Bold;
        nText.color = new Color(0.93f, 0.82f, 0.55f);
        nText.alignment = TextAlignmentOptions.TopLeft;
        RectTransform nameRT = nameObj.GetComponent<RectTransform>();
        nameRT.anchorMin = new Vector2(0, 1);
        nameRT.anchorMax = new Vector2(1, 1);
        nameRT.pivot = new Vector2(0, 1);
        nameRT.sizeDelta = new Vector2(-140, 30);
        nameRT.anchoredPosition = new Vector2(130, -10);

        GameObject textObj = MakeRect("DialogueText", panel.transform);
        TMP_Text dText = textObj.AddComponent<TextMeshProUGUI>();
        dText.fontSize = 18;
        dText.color = new Color(0.92f, 0.90f, 0.85f);
        dText.alignment = TextAlignmentOptions.TopLeft;
        dText.enableWordWrapping = true;
        RectTransform textRT = textObj.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = new Vector2(130, 15);
        textRT.offsetMax = new Vector2(-15, -40);

        GameObject choiceObj = MakeRect("Choices", panel.transform);
        VerticalLayoutGroup layout = choiceObj.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 5;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        RectTransform choiceRT = choiceObj.GetComponent<RectTransform>();
        choiceRT.anchorMin = new Vector2(0.5f, 0);
        choiceRT.anchorMax = new Vector2(0.5f, 0);
        choiceRT.pivot = new Vector2(0.5f, 1);
        choiceRT.sizeDelta = new Vector2(400, 100);
        choiceRT.anchoredPosition = new Vector2(0, -5);

        dialoguePanel = panel;
        dialogueText = dText;
        nameText = nText;
        npcPortraitImage = portrait;
        choiceContainer = choiceObj.transform;
    }

    private GameObject MakeRect(string name, Transform parent)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        return obj;
    }

    protected override bool ShouldUnpauseOnDialogueEnd()
    {
        return !awaitingPuzzle;
    }

    public override void Interact(GameObject player)
    {
        if (playerGreedMeter == null)
            playerGreedMeter = player.GetComponent<GreedMeter>();

        if (hasSolved)
        {
            PlayDialogue(solvedDialogue);
            return;
        }

        if (playerGreedMeter != null && playerGreedMeter.GetCurrentGold() < goldCost)
        {
            PlayDialogue(noGoldDialogue);
            return;
        }

        awaitingPuzzle = true;
        OnDialogueComplete.AddListener(OnIntroComplete);
        PlayDialogue(introDialogue);
    }

    private void OnIntroComplete()
    {
        OnDialogueComplete.RemoveListener(OnIntroComplete);
        awaitingPuzzle = false;

        if (playerGreedMeter != null)
            playerGreedMeter.RemoveGold(goldCost);

        if (wordlePuzzleUI != null)
        {
            wordlePuzzleUI.OnPuzzleComplete += OnPuzzleResult;
            wordlePuzzleUI.Open();
        }
        else
        {
            PauseController.SetPause(false);
        }
    }

    private void OnPuzzleResult(bool solved)
    {
        wordlePuzzleUI.OnPuzzleComplete -= OnPuzzleResult;

        if (solved)
        {
            hasSolved = true;
            if (bridgeBlocker != null)
                bridgeBlocker.SetActive(false);
            if (solvedDialogue != null)
                PlayDialogue(solvedDialogue);
        }
        else
        {
            if (failedDialogue != null)
                PlayDialogue(failedDialogue);
        }
    }
}
