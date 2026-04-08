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
        borderRT.sizeDelta = new Vector2(460, 720);

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

        GameObject kbObj = MakeRect("Keyboard", panel.transform);
        GridLayoutGroup kbGrid = kbObj.AddComponent<GridLayoutGroup>();
        kbGrid.cellSize = new Vector2(30, 36);
        kbGrid.spacing = new Vector2(4, 4);
        kbGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        kbGrid.constraintCount = 9;
        kbGrid.childAlignment = TextAnchor.UpperCenter;
        RectTransform kbRT = kbObj.GetComponent<RectTransform>();
        kbRT.anchorMin = new Vector2(0.5f, 0);
        kbRT.anchorMax = new Vector2(0.5f, 0);
        kbRT.pivot = new Vector2(0.5f, 0);
        kbRT.sizeDelta = new Vector2(330, 120);
        kbRT.anchoredPosition = new Vector2(0, 50);

        GameObject keyPrefab = new GameObject("KeyPrefab");
        keyPrefab.AddComponent<Image>().color = new Color(0.35f, 0.32f, 0.28f);
        GameObject keyTextObj = new GameObject("Letter");
        keyTextObj.transform.SetParent(keyPrefab.transform, false);
        TMP_Text keyText = keyTextObj.AddComponent<TextMeshProUGUI>();
        keyText.fontSize = 18;
        keyText.fontStyle = FontStyles.Bold;
        keyText.alignment = TextAlignmentOptions.Center;
        keyText.color = textWhite;
        RectTransform keyTextRT = keyTextObj.GetComponent<RectTransform>();
        keyTextRT.anchorMin = Vector2.zero;
        keyTextRT.anchorMax = Vector2.one;
        keyTextRT.sizeDelta = Vector2.zero;

        wordlePuzzleUI = canvasObj.AddComponent<WordlePuzzleUI>();
        SetField(wordlePuzzleUI, "puzzlePanel", border);
        SetField(wordlePuzzleUI, "gridParent", gridObj.transform);
        SetField(wordlePuzzleUI, "keyboardParent", kbObj.transform);
        SetField(wordlePuzzleUI, "tilePrefab", tilePrefab);
        SetField(wordlePuzzleUI, "keyPrefab", keyPrefab);
        SetField(wordlePuzzleUI, "messageText", msgText);
        SetField(wordlePuzzleUI, "emptyColor", emptyTile);
        SetField(wordlePuzzleUI, "wrongColor", new Color(0.28f, 0.26f, 0.24f));
        SetField(wordlePuzzleUI, "misplacedColor", new Color(0.78f, 0.65f, 0.20f));
        SetField(wordlePuzzleUI, "correctColor", new Color(0.25f, 0.52f, 0.22f));
        SetField(wordlePuzzleUI, "textColor", textWhite);
        SetField(wordlePuzzleUI, "keyDefaultColor", new Color(0.35f, 0.32f, 0.28f));

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
