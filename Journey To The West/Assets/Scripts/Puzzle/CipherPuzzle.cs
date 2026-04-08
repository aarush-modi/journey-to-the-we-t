using System.Collections.Generic;
using UnityEngine;

public class CipherPuzzle
{
    public string EncryptedText { get; private set; }
    public string Answer { get; private set; }
    public Dictionary<char, char> FullKey { get; private set; }
    public Dictionary<char, char> RevealedKey { get; private set; }
    public int Round { get; private set; }

    private static readonly string[][] roundPhrases = new string[][]
    {
        new string[] { "OPEN THE GATE", "ENTER THE KEEP", "CROSS THE MOAT", "LOWER THE BRIDGE" },
        new string[] { "THE KING AWAITS", "GUARD THE THRONE", "SEIZE THE CROWN", "LIGHT THE BEACON" },
        new string[] { "ONLY THE WORTHY SHALL PASS", "DARKNESS FALLS AT MIDNIGHT", "STRENGTH LIES IN WISDOM", "THE DRAGON SLEEPS BELOW" }
    };

    public CipherPuzzle(int round)
    {
        Round = Mathf.Clamp(round, 0, 2);
        string[] phrases = roundPhrases[Round];
        Answer = phrases[Random.Range(0, phrases.Length)];
        GenerateCipher();
    }

    private void GenerateCipher()
    {
        FullKey = new Dictionary<char, char>();

        if (Round == 0)
        {
            int shift = Random.Range(3, 8);
            for (char c = 'A'; c <= 'Z'; c++)
                FullKey[c] = (char)('A' + (c - 'A' + shift) % 26);
        }
        else
        {
            List<char> alphabet = new List<char>();
            for (char c = 'A'; c <= 'Z'; c++)
                alphabet.Add(c);

            for (int i = alphabet.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (alphabet[i], alphabet[j]) = (alphabet[j], alphabet[i]);
            }

            for (int i = 0; i < 26; i++)
                FullKey[(char)('A' + i)] = alphabet[i];
        }

        char[] encrypted = new char[Answer.Length];
        for (int i = 0; i < Answer.Length; i++)
        {
            char c = Answer[i];
            if (c >= 'A' && c <= 'Z')
                encrypted[i] = FullKey[c];
            else
                encrypted[i] = c;
        }
        EncryptedText = new string(encrypted);

        RevealedKey = new Dictionary<char, char>();
        HashSet<char> usedLetters = new HashSet<char>();
        foreach (char c in Answer)
        {
            if (c >= 'A' && c <= 'Z')
                usedLetters.Add(c);
        }

        List<char> letterList = new List<char>(usedLetters);
        int revealCount;
        switch (Round)
        {
            case 0: revealCount = Mathf.CeilToInt(letterList.Count * 0.7f); break;
            case 1: revealCount = Mathf.CeilToInt(letterList.Count * 0.4f); break;
            default: revealCount = Mathf.CeilToInt(letterList.Count * 0.2f); break;
        }

        for (int i = letterList.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (letterList[i], letterList[j]) = (letterList[j], letterList[i]);
        }

        for (int i = 0; i < revealCount && i < letterList.Count; i++)
        {
            char plain = letterList[i];
            RevealedKey[plain] = FullKey[plain];
        }
    }

    public bool CheckAnswer(string guess)
    {
        return guess.ToUpper().Trim() == Answer;
    }
}
