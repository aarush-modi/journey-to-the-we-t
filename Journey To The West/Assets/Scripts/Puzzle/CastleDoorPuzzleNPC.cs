using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class CastleDoorPuzzleNPC : NPCBase
{
    [SerializeField] private NPCDialogue introDialogue;
    [SerializeField] private NPCDialogue solvedDialogue;
    [SerializeField] private NPCDialogue failedDialogue;
    [SerializeField] private string castleSceneName = "Castle";

    private CipherPuzzleUI cipherUI;
    private bool hasSolved;
    private bool awaitingPuzzle;

    protected override void Start()
    {
        if (dialoguePanel == null)
            BuildDialogueUI();

        BuildCipherUI();
        base.Start();
    }

    private void BuildDialogueUI()
    {
        GameObject canvasObj = new GameObject("CastleDoorDialogueCanvas");
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

    private void BuildCipherUI()
    {
        Color panelColor = new Color(0.13f, 0.11f, 0.10f, 0.95f);
        Color borderColor = new Color(0.45f, 0.30f, 0.15f);
        Color goldColor = new Color(0.93f, 0.82f, 0.55f);
        Color textWhite = new Color(0.95f, 0.93f, 0.88f);

        GameObject canvasObj = new GameObject("CipherPuzzleCanvas");
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
        borderRT.sizeDelta = new Vector2(700, 500);

        GameObject panel = MakeRect("CipherPanel", border.transform);
        panel.AddComponent<Image>().color = panelColor;
        RectTransform panelRT = panel.GetComponent<RectTransform>();
        panelRT.anchorMin = Vector2.zero;
        panelRT.anchorMax = Vector2.one;
        panelRT.offsetMin = new Vector2(6, 6);
        panelRT.offsetMax = new Vector2(-6, -6);

        GameObject titleObj = MakeRect("Title", panel.transform);
        TMP_Text title = titleObj.AddComponent<TextMeshProUGUI>();
        title.text = "CIPHER DECODE";
        title.fontSize = 30;
        title.fontStyle = FontStyles.Bold;
        title.alignment = TextAlignmentOptions.Center;
        title.color = goldColor;
        RectTransform titleRT = titleObj.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0, 1);
        titleRT.anchorMax = new Vector2(0.75f, 1);
        titleRT.pivot = new Vector2(0.5f, 1);
        titleRT.sizeDelta = new Vector2(0, 40);
        titleRT.anchoredPosition = new Vector2(0, -10);

        GameObject timerObj = MakeRect("Timer", panel.transform);
        TMP_Text timer = timerObj.AddComponent<TextMeshProUGUI>();
        timer.text = "60";
        timer.fontSize = 36;
        timer.fontStyle = FontStyles.Bold;
        timer.alignment = TextAlignmentOptions.Center;
        timer.color = goldColor;
        RectTransform timerRT = timerObj.GetComponent<RectTransform>();
        timerRT.anchorMin = new Vector2(0.75f, 1);
        timerRT.anchorMax = new Vector2(1, 1);
        timerRT.pivot = new Vector2(0.5f, 1);
        timerRT.sizeDelta = new Vector2(0, 40);
        timerRT.anchoredPosition = new Vector2(0, -10);

        GameObject encLabel = MakeRect("EncLabel", panel.transform);
        TMP_Text encLbl = encLabel.AddComponent<TextMeshProUGUI>();
        encLbl.text = "ENCRYPTED:";
        encLbl.fontSize = 16;
        encLbl.color = new Color(0.6f, 0.55f, 0.45f);
        encLbl.alignment = TextAlignmentOptions.Left;
        RectTransform encLblRT = encLabel.GetComponent<RectTransform>();
        encLblRT.anchorMin = new Vector2(0, 1);
        encLblRT.anchorMax = new Vector2(1, 1);
        encLblRT.pivot = new Vector2(0, 1);
        encLblRT.sizeDelta = new Vector2(-40, 25);
        encLblRT.anchoredPosition = new Vector2(20, -55);

        GameObject encObj = MakeRect("Encrypted", panel.transform);
        TMP_Text encrypted = encObj.AddComponent<TextMeshProUGUI>();
        encrypted.fontSize = 28;
        encrypted.fontStyle = FontStyles.Bold;
        encrypted.alignment = TextAlignmentOptions.Center;
        encrypted.color = new Color(0.85f, 0.45f, 0.35f);
        RectTransform encRT = encObj.GetComponent<RectTransform>();
        encRT.anchorMin = new Vector2(0, 1);
        encRT.anchorMax = new Vector2(1, 1);
        encRT.pivot = new Vector2(0.5f, 1);
        encRT.sizeDelta = new Vector2(-40, 45);
        encRT.anchoredPosition = new Vector2(0, -80);

        GameObject keyObj = MakeRect("Key", panel.transform);
        TMP_Text key = keyObj.AddComponent<TextMeshProUGUI>();
        key.fontSize = 18;
        key.alignment = TextAlignmentOptions.Center;
        key.color = textWhite;
        key.enableWordWrapping = true;
        RectTransform keyRT = keyObj.GetComponent<RectTransform>();
        keyRT.anchorMin = new Vector2(0, 1);
        keyRT.anchorMax = new Vector2(1, 1);
        keyRT.pivot = new Vector2(0.5f, 1);
        keyRT.sizeDelta = new Vector2(-40, 80);
        keyRT.anchoredPosition = new Vector2(0, -135);

        GameObject inputLabel = MakeRect("InputLabel", panel.transform);
        TMP_Text inLbl = inputLabel.AddComponent<TextMeshProUGUI>();
        inLbl.text = "YOUR ANSWER:";
        inLbl.fontSize = 16;
        inLbl.color = new Color(0.6f, 0.55f, 0.45f);
        inLbl.alignment = TextAlignmentOptions.Left;
        RectTransform inLblRT = inputLabel.GetComponent<RectTransform>();
        inLblRT.anchorMin = new Vector2(0, 1);
        inLblRT.anchorMax = new Vector2(1, 1);
        inLblRT.pivot = new Vector2(0, 1);
        inLblRT.sizeDelta = new Vector2(-40, 25);
        inLblRT.anchoredPosition = new Vector2(20, -225);

        GameObject inputBg = MakeRect("InputBG", panel.transform);
        inputBg.AddComponent<Image>().color = new Color(0.08f, 0.07f, 0.06f);
        Outline inputOutline = inputBg.AddComponent<Outline>();
        inputOutline.effectColor = borderColor;
        inputOutline.effectDistance = new Vector2(2, -2);
        RectTransform inputBgRT = inputBg.GetComponent<RectTransform>();
        inputBgRT.anchorMin = new Vector2(0, 1);
        inputBgRT.anchorMax = new Vector2(1, 1);
        inputBgRT.pivot = new Vector2(0.5f, 1);
        inputBgRT.sizeDelta = new Vector2(-40, 50);
        inputBgRT.anchoredPosition = new Vector2(0, -250);

        GameObject inputObj = MakeRect("Input", inputBg.transform);
        TMP_Text input = inputObj.AddComponent<TextMeshProUGUI>();
        input.text = "_";
        input.fontSize = 26;
        input.fontStyle = FontStyles.Bold;
        input.alignment = TextAlignmentOptions.Center;
        input.color = Color.white;
        RectTransform inputRT = inputObj.GetComponent<RectTransform>();
        inputRT.anchorMin = Vector2.zero;
        inputRT.anchorMax = Vector2.one;
        inputRT.sizeDelta = Vector2.zero;

        GameObject msgObj = MakeRect("Message", panel.transform);
        TMP_Text msg = msgObj.AddComponent<TextMeshProUGUI>();
        msg.fontSize = 20;
        msg.alignment = TextAlignmentOptions.Center;
        msg.color = goldColor;
        RectTransform msgRT = msgObj.GetComponent<RectTransform>();
        msgRT.anchorMin = new Vector2(0, 0);
        msgRT.anchorMax = new Vector2(1, 0);
        msgRT.pivot = new Vector2(0.5f, 0);
        msgRT.sizeDelta = new Vector2(0, 40);
        msgRT.anchoredPosition = new Vector2(0, 10);

        cipherUI = canvasObj.AddComponent<CipherPuzzleUI>();
        SetField(cipherUI, "puzzlePanel", border);
        SetField(cipherUI, "titleText", title);
        SetField(cipherUI, "encryptedText", encrypted);
        SetField(cipherUI, "keyText", key);
        SetField(cipherUI, "inputText", input);
        SetField(cipherUI, "timerText", timer);
        SetField(cipherUI, "messageText", msg);

        border.SetActive(false);
    }

    protected override bool ShouldUnpauseOnDialogueEnd()
    {
        return !awaitingPuzzle;
    }

    public override void Interact(GameObject player)
    {
        if (hasSolved)
        {
            PlayDialogue(solvedDialogue);
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

        if (cipherUI != null)
        {
            cipherUI.OnPuzzleComplete += OnPuzzleResult;
            cipherUI.Open();
        }
    }

    private async void OnPuzzleResult(bool solved)
    {
        cipherUI.OnPuzzleComplete -= OnPuzzleResult;

        if (solved)
        {
            hasSolved = true;
            PlayDialogue(solvedDialogue);

            await System.Threading.Tasks.Task.Delay(2000);

            if (ScreenFader.Instance != null)
                await ScreenFader.Instance.FadeOut();

            SceneManager.LoadScene(castleSceneName);
        }
        else
        {
            if (failedDialogue != null)
                PlayDialogue(failedDialogue);
        }
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
}
