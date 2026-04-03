using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class WordlePuzzleUI : MonoBehaviour
{
    [SerializeField] private GameObject puzzlePanel;
    [SerializeField] private Transform gridParent;
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private TMP_Text messageText;

    [SerializeField] private Color emptyColor = new Color(0.15f, 0.15f, 0.15f);
    [SerializeField] private Color wrongColor = new Color(0.23f, 0.23f, 0.25f);
    [SerializeField] private Color misplacedColor = new Color(0.71f, 0.63f, 0.21f);
    [SerializeField] private Color correctColor = new Color(0.32f, 0.55f, 0.30f);
    [SerializeField] private Color textColor = Color.white;

    private WordlePuzzle puzzle;
    private TMP_Text[,] tileTexts;
    private Image[,] tileImages;
    private string currentInput = "";
    private bool isActive;
    private bool shouldPause;

    public event Action<bool> OnPuzzleComplete;

    public void Open(bool pause = true)
    {
        puzzle = new WordlePuzzle();
        currentInput = "";
        isActive = true;
        shouldPause = pause;

        BuildGrid();
        SetMessage("");

        puzzlePanel.SetActive(true);
        if (shouldPause) PauseController.SetPause(true);
    }

    public void Open(string specificWord, bool pause = true)
    {
        puzzle = new WordlePuzzle(specificWord);
        currentInput = "";
        isActive = true;
        shouldPause = pause;

        BuildGrid();
        SetMessage("");

        puzzlePanel.SetActive(true);
        if (shouldPause) PauseController.SetPause(true);
    }

    public void Close()
    {
        isActive = false;
        puzzlePanel.SetActive(false);
        if (shouldPause) PauseController.SetPause(false);
    }

    private void Update()
    {
        if (!isActive || puzzle == null || puzzle.IsGameOver) return;

        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        for (Key key = Key.A; key <= Key.Z; key++)
        {
            if (kb[key].wasPressedThisFrame && currentInput.Length < WordlePuzzle.WordLength)
            {
                currentInput += (char)('A' + (key - Key.A));
                UpdateCurrentRow();
                return;
            }
        }

        if (kb.backspaceKey.wasPressedThisFrame && currentInput.Length > 0)
        {
            currentInput = currentInput.Substring(0, currentInput.Length - 1);
            UpdateCurrentRow();
        }

        if (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame)
        {
            TrySubmitGuess();
        }
    }

    private void BuildGrid()
    {
        foreach (Transform child in gridParent)
            Destroy(child.gameObject);

        tileTexts = new TMP_Text[WordlePuzzle.MaxAttempts, WordlePuzzle.WordLength];
        tileImages = new Image[WordlePuzzle.MaxAttempts, WordlePuzzle.WordLength];

        for (int row = 0; row < WordlePuzzle.MaxAttempts; row++)
        {
            for (int col = 0; col < WordlePuzzle.WordLength; col++)
            {
                GameObject tile = Instantiate(tilePrefab, gridParent);
                tile.SetActive(true);
                tileImages[row, col] = tile.GetComponent<Image>();
                tileTexts[row, col] = tile.GetComponentInChildren<TMP_Text>();
                tileImages[row, col].color = emptyColor;
                tileTexts[row, col].text = "";
                tileTexts[row, col].color = textColor;
            }
        }
    }

    private void UpdateCurrentRow()
    {
        int row = puzzle.CurrentAttempt;
        if (row >= WordlePuzzle.MaxAttempts) return;

        for (int col = 0; col < WordlePuzzle.WordLength; col++)
        {
            tileTexts[row, col].text = col < currentInput.Length
                ? currentInput[col].ToString()
                : "";
            tileImages[row, col].color = emptyColor;
        }
    }

    private void TrySubmitGuess()
    {
        if (currentInput.Length != WordlePuzzle.WordLength)
        {
            SetMessage("Not enough letters");
            return;
        }

        if (!puzzle.IsValidGuess(currentInput))
        {
            SetMessage("Not a valid word");
            return;
        }

        SetMessage("");
        int row = puzzle.CurrentAttempt;
        LetterState[] result = puzzle.SubmitGuess(currentInput);
        currentInput = "";
        StartCoroutine(RevealRow(row, result));
    }

    private IEnumerator RevealRow(int row, LetterState[] states)
    {
        isActive = false;

        for (int col = 0; col < WordlePuzzle.WordLength; col++)
        {
            tileImages[row, col].color = states[col] switch
            {
                LetterState.Correct => correctColor,
                LetterState.Misplaced => misplacedColor,
                LetterState.Wrong => wrongColor,
                _ => emptyColor
            };
            yield return new WaitForSecondsRealtime(0.2f);
        }

        if (puzzle.IsSolved)
        {
            SetMessage("You solved it!");
            yield return new WaitForSecondsRealtime(1.5f);
            OnPuzzleComplete?.Invoke(true);
            Close();
        }
        else if (puzzle.IsFailed)
        {
            SetMessage($"The word was: {puzzle.TargetWord}");
            yield return new WaitForSecondsRealtime(2f);
            OnPuzzleComplete?.Invoke(false);
            Close();
        }
        else
        {
            isActive = true;
        }
    }

    private void SetMessage(string msg)
    {
        if (messageText != null)
            messageText.text = msg;
    }
}
