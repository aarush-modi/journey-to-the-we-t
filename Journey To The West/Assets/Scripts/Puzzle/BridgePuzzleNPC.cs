using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BridgePuzzleNPC : NPCBase
{
    [SerializeField] private NPCDialogue introDialogue;
    [SerializeField] private NPCDialogue noGoldDialogue;
    [SerializeField] private NPCDialogue solvedDialogue;
    [SerializeField] private NPCDialogue failedDialogue;
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

        BuildWordleUI();
        base.Start();

        if (bridgeBlocker != null)
            bridgeBlocker.SetActive(true);
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

    private void BuildWordleUI()
    {
        Color emptyTile = new Color(0.22f, 0.20f, 0.18f);
        Color textWhite = new Color(0.95f, 0.93f, 0.88f);
        Color borderColor = new Color(0.35f, 0.28f, 0.20f);
        Color titleColor = new Color(0.93f, 0.82f, 0.55f);

        GameObject canvasObj = new GameObject("WordlePuzzleCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject border = MakeRect("Border", canvasObj.transform);
        border.AddComponent<Image>().color = borderColor;
        RectTransform borderRT = border.GetComponent<RectTransform>();
        borderRT.anchorMin = new Vector2(0.5f, 0.5f);
        borderRT.anchorMax = new Vector2(0.5f, 0.5f);
        borderRT.sizeDelta = new Vector2(460, 610);

        GameObject panel = MakeRect("PuzzlePanel", border.transform);
        panel.AddComponent<Image>().color = new Color(0.13f, 0.11f, 0.10f, 0.95f);
        RectTransform panelRT = panel.GetComponent<RectTransform>();
        panelRT.anchorMin = Vector2.zero;
        panelRT.anchorMax = Vector2.one;
        panelRT.offsetMin = new Vector2(6, 6);
        panelRT.offsetMax = new Vector2(-6, -6);

        GameObject titleObj = MakeRect("Title", panel.transform);
        TMP_Text titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "W O R D L E";
        titleText.fontSize = 34;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = titleColor;
        titleText.fontStyle = FontStyles.Bold;
        RectTransform titleRT = titleObj.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0, 1);
        titleRT.anchorMax = new Vector2(1, 1);
        titleRT.pivot = new Vector2(0.5f, 1);
        titleRT.sizeDelta = new Vector2(0, 50);
        titleRT.anchoredPosition = new Vector2(0, -12);

        GameObject gridObj = MakeRect("Grid", panel.transform);
        GridLayoutGroup grid = gridObj.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(66, 66);
        grid.spacing = new Vector2(8, 8);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 5;
        grid.childAlignment = TextAnchor.UpperCenter;
        RectTransform gridRT = gridObj.GetComponent<RectTransform>();
        gridRT.anchorMin = new Vector2(0.5f, 1);
        gridRT.anchorMax = new Vector2(0.5f, 1);
        gridRT.pivot = new Vector2(0.5f, 1);
        gridRT.sizeDelta = new Vector2(370, 440);
        gridRT.anchoredPosition = new Vector2(0, -72);

        GameObject msgObj = MakeRect("Message", panel.transform);
        TMP_Text msgText = msgObj.AddComponent<TextMeshProUGUI>();
        msgText.fontSize = 22;
        msgText.alignment = TextAlignmentOptions.Center;
        msgText.color = titleColor;
        RectTransform msgRT = msgObj.GetComponent<RectTransform>();
        msgRT.anchorMin = new Vector2(0, 0);
        msgRT.anchorMax = new Vector2(1, 0);
        msgRT.pivot = new Vector2(0.5f, 0);
        msgRT.sizeDelta = new Vector2(0, 45);
        msgRT.anchoredPosition = new Vector2(0, 8);

        GameObject tilePrefab = new GameObject("WordleTilePrefab");
        tilePrefab.AddComponent<Image>().color = emptyTile;
        Outline tileOutline = tilePrefab.AddComponent<Outline>();
        tileOutline.effectColor = new Color(0.4f, 0.35f, 0.28f, 0.6f);
        tileOutline.effectDistance = new Vector2(2, -2);

        GameObject tileTextObj = new GameObject("Letter");
        tileTextObj.transform.SetParent(tilePrefab.transform, false);
        TMP_Text tileText = tileTextObj.AddComponent<TextMeshProUGUI>();
        tileText.fontSize = 34;
        tileText.fontStyle = FontStyles.Bold;
        tileText.alignment = TextAlignmentOptions.Center;
        tileText.color = textWhite;
        RectTransform tileTextRT = tileTextObj.GetComponent<RectTransform>();
        tileTextRT.anchorMin = Vector2.zero;
        tileTextRT.anchorMax = Vector2.one;
        tileTextRT.sizeDelta = Vector2.zero;

        wordlePuzzleUI = canvasObj.AddComponent<WordlePuzzleUI>();
        SetField(wordlePuzzleUI, "puzzlePanel", border);
        SetField(wordlePuzzleUI, "gridParent", gridObj.transform);
        SetField(wordlePuzzleUI, "tilePrefab", tilePrefab);
        SetField(wordlePuzzleUI, "messageText", msgText);
        SetField(wordlePuzzleUI, "emptyColor", emptyTile);
        SetField(wordlePuzzleUI, "wrongColor", new Color(0.28f, 0.26f, 0.24f));
        SetField(wordlePuzzleUI, "misplacedColor", new Color(0.78f, 0.65f, 0.20f));
        SetField(wordlePuzzleUI, "correctColor", new Color(0.25f, 0.52f, 0.22f));
        SetField(wordlePuzzleUI, "textColor", textWhite);

        border.SetActive(false);
    }

    private void SetField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
            field.SetValue(target, value);
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
        PauseController.SetPause(false);

        if (playerGreedMeter != null)
            playerGreedMeter.RemoveGold(goldCost);

        if (wordlePuzzleUI != null)
        {
            wordlePuzzleUI.OnPuzzleComplete += OnPuzzleResult;
            wordlePuzzleUI.Open();
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
