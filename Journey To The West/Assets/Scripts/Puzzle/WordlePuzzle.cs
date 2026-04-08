using System.Collections.Generic;
using UnityEngine;

public enum LetterState
{
    Empty,
    Wrong,
    Misplaced,
    Correct
}

public class WordlePuzzle
{
    public const int WordLength = 5;
    public const int MaxAttempts = 6;

    private string targetWord;
    private List<string> guesses = new List<string>();
    private bool isSolved;
    private bool isFailed;

    public string TargetWord => targetWord;
    public IReadOnlyList<string> Guesses => guesses;
    public int CurrentAttempt => guesses.Count;
    public bool IsSolved => isSolved;
    public bool IsFailed => isFailed;
    public bool IsGameOver => isSolved || isFailed;

    public WordlePuzzle()
    {
        var words = WordleWordList.AllWords;
        targetWord = words[Random.Range(0, words.Length)];
        Debug.Log($"[Wordle] Answer: {targetWord}");
    }

    public WordlePuzzle(string word)
    {
        targetWord = word.ToUpper();
    }

    public bool IsValidGuess(string guess)
    {
        if (guess.Length != WordLength) return false;
        return WordleWordList.ValidGuesses.Contains(guess.ToUpper());
    }

    public LetterState[] SubmitGuess(string guess)
    {
        guess = guess.ToUpper();
        if (guess.Length != WordLength) return null;

        guesses.Add(guess);
        LetterState[] result = EvaluateGuess(guess);

        if (guess == targetWord)
            isSolved = true;
        else if (guesses.Count >= MaxAttempts)
            isFailed = true;

        return result;
    }

    public LetterState[] EvaluateGuess(string guess)
    {
        LetterState[] result = new LetterState[WordLength];
        bool[] targetUsed = new bool[WordLength];
        bool[] guessUsed = new bool[WordLength];

        for (int i = 0; i < WordLength; i++)
        {
            if (guess[i] == targetWord[i])
            {
                result[i] = LetterState.Correct;
                targetUsed[i] = true;
                guessUsed[i] = true;
            }
        }

        for (int i = 0; i < WordLength; i++)
        {
            if (guessUsed[i]) continue;

            for (int j = 0; j < WordLength; j++)
            {
                if (targetUsed[j]) continue;

                if (guess[i] == targetWord[j])
                {
                    result[i] = LetterState.Misplaced;
                    targetUsed[j] = true;
                    break;
                }
            }

            if (result[i] == LetterState.Empty)
                result[i] = LetterState.Wrong;
        }

        return result;
    }
}
