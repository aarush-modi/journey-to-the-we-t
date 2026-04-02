using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Drop this on an empty GameObject in a scene to test the Wordle puzzle.
/// It builds all required UI at runtime — no manual setup needed.
/// Press Space to open the puzzle. Type letters, Backspace to delete, Enter to submit.
/// The target word is logged to the console for easy testing.
/// </summary>
public class WordlePuzzleTest : MonoBehaviour
{
    private WordlePuzzleUI wordleUI;

    private void Start()
    {
        // Build the entire UI from scratch
        GameObject canvasObj = new GameObject("WordleTestCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObj.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        // --- Puzzle Panel (centered, dark background) ---
        GameObject panel = CreateUIElement("PuzzlePanel", canvasObj.transform);
        Image panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(0.1f, 0.1f, 0.12f, 0.95f);
        RectTransform panelRT = panel.GetComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0.5f, 0.5f);
        panelRT.anchorMax = new Vector2(0.5f, 0.5f);
        panelRT.sizeDelta = new Vector2(440, 580);
        panelRT.anchoredPosition = Vector2.zero;

        // --- Title ---
        GameObject titleObj = CreateUIElement("Title", panel.transform);
        TMP_Text titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "WORDLE";
        titleText.fontSize = 36;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = Color.white;
        titleText.fontStyle = FontStyles.Bold;
        RectTransform titleRT = titleObj.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0, 1);
        titleRT.anchorMax = new Vector2(1, 1);
        titleRT.pivot = new Vector2(0.5f, 1);
        titleRT.sizeDelta = new Vector2(0, 50);
        titleRT.anchoredPosition = new Vector2(0, -10);

        // --- Grid Parent (GridLayoutGroup) ---
        GameObject gridObj = CreateUIElement("Grid", panel.transform);
        GridLayoutGroup grid = gridObj.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(62, 62);
        grid.spacing = new Vector2(6, 6);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 5;
        grid.childAlignment = TextAnchor.UpperCenter;
        RectTransform gridRT = gridObj.GetComponent<RectTransform>();
        gridRT.anchorMin = new Vector2(0.5f, 1);
        gridRT.anchorMax = new Vector2(0.5f, 1);
        gridRT.pivot = new Vector2(0.5f, 1);
        gridRT.sizeDelta = new Vector2(340, 414);
        gridRT.anchoredPosition = new Vector2(0, -65);

        // --- Message Text ---
        GameObject msgObj = CreateUIElement("Message", panel.transform);
        TMP_Text msgText = msgObj.AddComponent<TextMeshProUGUI>();
        msgText.text = "";
        msgText.fontSize = 22;
        msgText.alignment = TextAlignmentOptions.Center;
        msgText.color = Color.white;
        RectTransform msgRT = msgObj.GetComponent<RectTransform>();
        msgRT.anchorMin = new Vector2(0, 0);
        msgRT.anchorMax = new Vector2(1, 0);
        msgRT.pivot = new Vector2(0.5f, 0);
        msgRT.sizeDelta = new Vector2(0, 50);
        msgRT.anchoredPosition = new Vector2(0, 10);

        // --- Tile Prefab (hidden template, not under canvas so it won't render) ---
        GameObject tilePrefab = new GameObject("TilePrefab");
        Image tileImg = tilePrefab.AddComponent<Image>();
        tileImg.color = new Color(0.15f, 0.15f, 0.15f);

        GameObject tileTextObj = new GameObject("Letter");
        tileTextObj.transform.SetParent(tilePrefab.transform, false);
        TMP_Text tileText = tileTextObj.AddComponent<TextMeshProUGUI>();
        tileText.fontSize = 32;
        tileText.fontStyle = FontStyles.Bold;
        tileText.alignment = TextAlignmentOptions.Center;
        tileText.color = Color.white;
        RectTransform tileTextRT = tileTextObj.GetComponent<RectTransform>();
        tileTextRT.anchorMin = Vector2.zero;
        tileTextRT.anchorMax = Vector2.one;
        tileTextRT.sizeDelta = Vector2.zero;

        // --- Instruction text at bottom of screen ---
        GameObject instructObj = CreateUIElement("Instructions", canvasObj.transform);
        TMP_Text instructText = instructObj.AddComponent<TextMeshProUGUI>();
        instructText.text = "Press SPACE to open puzzle  |  Type A-Z  |  BACKSPACE to delete  |  ENTER to submit";
        instructText.fontSize = 20;
        instructText.alignment = TextAlignmentOptions.Center;
        instructText.color = new Color(1, 1, 1, 0.6f);
        RectTransform instructRT = instructObj.GetComponent<RectTransform>();
        instructRT.anchorMin = new Vector2(0, 0);
        instructRT.anchorMax = new Vector2(1, 0);
        instructRT.pivot = new Vector2(0.5f, 0);
        instructRT.sizeDelta = new Vector2(0, 40);
        instructRT.anchoredPosition = new Vector2(0, 10);

        // --- Attach WordlePuzzleUI and wire references ---
        wordleUI = canvasObj.AddComponent<WordlePuzzleUI>();

        // Use reflection to set serialized fields since we're building at runtime
        SetPrivateField(wordleUI, "puzzlePanel", panel);
        SetPrivateField(wordleUI, "gridParent", gridObj.transform);
        SetPrivateField(wordleUI, "tilePrefab", tilePrefab);
        SetPrivateField(wordleUI, "messageText", msgText);

        wordleUI.OnPuzzleComplete += OnResult;

        // Start hidden
        panel.SetActive(false);

        Debug.Log("[WordlePuzzleTest] Ready! Press SPACE to open the Wordle puzzle.");
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            wordleUI.Open(pause: false);
            Debug.Log($"[WordlePuzzleTest] Puzzle opened. Target word logged below (for testing):");
            // Log the target word so testers can verify
            var puzzle = GetPrivateField<WordlePuzzle>(wordleUI, "puzzle");
            if (puzzle != null)
                Debug.Log($"[WordlePuzzleTest] TARGET: {puzzle.TargetWord}");
        }
    }

    private void OnResult(bool solved)
    {
        if (solved)
            Debug.Log("[WordlePuzzleTest] Player SOLVED the puzzle!");
        else
            Debug.Log("[WordlePuzzleTest] Player FAILED the puzzle.");

        Debug.Log("[WordlePuzzleTest] Press SPACE to play again.");
    }

    private GameObject CreateUIElement(string name, Transform parent)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        return obj;
    }

    private void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
            field.SetValue(target, value);
        else
            Debug.LogWarning($"[WordlePuzzleTest] Could not find field: {fieldName}");
    }

    private T GetPrivateField<T>(object target, string fieldName) where T : class
    {
        var field = target.GetType().GetField(fieldName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return field?.GetValue(target) as T;
    }
}
