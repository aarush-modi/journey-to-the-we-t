using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class CipherPuzzleUI : MonoBehaviour
{
    [SerializeField] private GameObject puzzlePanel;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text encryptedText;
    [SerializeField] private TMP_Text keyText;
    [SerializeField] private TMP_Text inputText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text messageText;

    [SerializeField] private float[] roundTimers = new float[] { 60f, 45f, 30f };

    private CipherPuzzle puzzle;
    private string currentInput = "";
    private int currentRound;
    private float timeRemaining;
    private bool isActive;

    public event Action<bool> OnPuzzleComplete;

    public void Open()
    {
        currentRound = 0;
        isActive = true;
        puzzlePanel.SetActive(true);
        PauseController.SetPause(true);
        StartRound();
    }

    public void Close()
    {
        isActive = false;
        puzzlePanel.SetActive(false);
        PauseController.SetPause(false);
    }

    private void StartRound()
    {
        puzzle = new CipherPuzzle(currentRound);
        currentInput = "";
        timeRemaining = roundTimers[currentRound];

        titleText.text = $"ROUND {currentRound + 1} / 3";
        encryptedText.text = puzzle.EncryptedText;
        UpdateKeyDisplay();
        UpdateInputDisplay();
        SetMessage("");
    }

    private void Update()
    {
        if (!isActive) return;

        timeRemaining -= Time.unscaledDeltaTime;
        timerText.text = Mathf.CeilToInt(timeRemaining).ToString();

        if (timeRemaining <= 10f)
            timerText.color = Color.red;
        else
            timerText.color = new Color(0.93f, 0.82f, 0.55f);

        if (timeRemaining <= 0f)
        {
            SetMessage($"Time's up! Answer: {puzzle.Answer}");
            isActive = false;
            Invoke(nameof(FailPuzzle), 2f);
            return;
        }

        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        for (Key key = Key.A; key <= Key.Z; key++)
        {
            if (kb[key].wasPressedThisFrame)
            {
                currentInput += (char)('A' + (key - Key.A));
                UpdateInputDisplay();
                return;
            }
        }

        if (kb.spaceKey.wasPressedThisFrame)
        {
            currentInput += " ";
            UpdateInputDisplay();
        }

        if (kb.backspaceKey.wasPressedThisFrame && currentInput.Length > 0)
        {
            currentInput = currentInput.Substring(0, currentInput.Length - 1);
            UpdateInputDisplay();
        }

        if (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame)
        {
            TrySubmit();
        }
    }

    private void UpdateKeyDisplay()
    {
        string display = "CIPHER KEY:\n";
        List<string> entries = new List<string>();
        foreach (var pair in puzzle.RevealedKey)
            entries.Add($"{pair.Key}={pair.Value}");
        display += string.Join("  ", entries);

        if (currentRound == 0)
            display += "\n(Hint: Each letter is shifted by the same amount)";

        keyText.text = display;
    }

    private void UpdateInputDisplay()
    {
        inputText.text = currentInput.Length > 0 ? currentInput : "_";
    }

    private void TrySubmit()
    {
        if (currentInput.Trim().Length == 0)
        {
            SetMessage("Type your answer first");
            return;
        }

        if (puzzle.CheckAnswer(currentInput))
        {
            currentRound++;
            if (currentRound >= 3)
            {
                SetMessage("All ciphers decoded!");
                isActive = false;
                Invoke(nameof(SucceedPuzzle), 1.5f);
            }
            else
            {
                SetMessage("Correct! Next round...");
                Invoke(nameof(StartRound), 1.5f);
            }
        }
        else
        {
            SetMessage("Wrong! Try again.");
            currentInput = "";
            UpdateInputDisplay();
        }
    }

    private void SucceedPuzzle()
    {
        OnPuzzleComplete?.Invoke(true);
        Close();
    }

    private void FailPuzzle()
    {
        OnPuzzleComplete?.Invoke(false);
        Close();
    }

    private void SetMessage(string msg)
    {
        if (messageText != null)
            messageText.text = msg;
    }
}
