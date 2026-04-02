using System.Collections.Generic;
using UnityEngine;

public enum LetterState
{
    Empty,
    Wrong,      // gray  - letter not in word
    Misplaced,  // yellow - letter in word but wrong position
    Correct     // green  - letter in correct position
}

public class WordlePuzzle
{
    public const int WordLength = 5;
    public const int MaxAttempts = 6;

    private string targetWord;
    private List<string> guesses = new List<string>();
    private bool isSolved;
    private bool isFailed;

    private static readonly string[] wordList = new string[]
    {
        "FLAME", "STONE", "PEARL", "CRANE", "SWORD",
        "CLOUD", "TIGER", "RIVER", "MOUNT", "DREAM",
        "STAFF", "GHOST", "QUEEN", "FEAST", "HEART",
        "MONKS", "DEMON", "STORM", "SPINE", "GUARD",
        "ROYAL", "MAGIC", "SCALE", "BRAVE", "TOWER",
        "LIGHT", "SHADE", "EARTH", "PLANT", "BLADE",
        "NIGHT", "FROST", "QUEST", "HORSE", "RIDGE",
        "CHARM", "SAINT", "LOTUS", "GRAIN", "REIGN",
        "POWER", "ROBES", "DRINK", "FORGE", "TRAIL",
        "BEAST", "CREST", "TEETH", "BLOOD", "HONOR"
    };

    // Larger set for validating guesses (includes target words + common 5-letter words)
    private static readonly HashSet<string> validGuesses = new HashSet<string>(wordList)
    {
        "ABOUT", "ABOVE", "AFTER", "AGAIN", "ANGRY",
        "APPLE", "BEGIN", "BLACK", "BOARD", "BRING",
        "BUILD", "CARRY", "CATCH", "CAUSE", "CHAIR",
        "CHEAP", "CHECK", "CHIEF", "CHILD", "CHINA",
        "CLASS", "CLEAN", "CLEAR", "CLIMB", "CLOSE",
        "COLOR", "COULD", "COUNT", "COVER", "CROSS",
        "DAILY", "DANCE", "DEATH", "DIRTY", "DOUBT",
        "DOZEN", "DRAFT", "DRAWN", "DRESS", "DRIVE",
        "EIGHT", "EMPTY", "ENEMY", "ENJOY", "ENTER",
        "EQUAL", "ERROR", "EVENT", "EVERY", "EXACT",
        "EXTRA", "FAITH", "FALSE", "FAULT", "FIELD",
        "FIGHT", "FINAL", "FIRST", "FLESH", "FLOAT",
        "FLOOR", "FOCUS", "FORCE", "FOUND", "FRAME",
        "FRESH", "FRONT", "FRUIT", "GIVEN", "GLASS",
        "GOING", "GRACE", "GRAND", "GRANT", "GRASS",
        "GREAT", "GREEN", "GROUP", "GROWN", "GUESS",
        "GUIDE", "HAPPY", "HAVEN", "HEAVY", "HELLO",
        "HOUSE", "HUMAN", "IDEAL", "IMAGE", "INDEX",
        "INNER", "INPUT", "ISSUE", "JOINT", "JUDGE",
        "JUICE", "KNIFE", "KNOCK", "KNOWN", "LABOR",
        "LARGE", "LATER", "LAUGH", "LAYER", "LEARN",
        "LEAVE", "LEGAL", "LEVEL", "LIMIT", "LIVED",
        "LOCAL", "LOOSE", "LOVER", "LOWER", "LUCKY",
        "LUNCH", "MAJOR", "MAKER", "MATCH", "MAYBE",
        "MAYOR", "MEANT", "MEDIA", "METAL", "MIGHT",
        "MINOR", "MINUS", "MODEL", "MONEY", "MONTH",
        "MORAL", "MOTOR", "MOUSE", "MOUTH", "MOVIE",
        "MUSIC", "NERVE", "NEVER", "NOISE", "NORTH",
        "NOTED", "NOVEL", "NURSE", "OCEAN", "OFFER",
        "ORDER", "OTHER", "OUGHT", "OUTER", "OWNER",
        "PAINT", "PANEL", "PAPER", "PARTY", "PATCH",
        "PAUSE", "PEACE", "PHASE", "PHONE", "PHOTO",
        "PIANO", "PIECE", "PILOT", "PITCH", "PIZZA",
        "PLACE", "PLAIN", "PLANE", "PLATE", "PLAZA",
        "POINT", "POUND", "PRESS", "PRICE", "PRIDE",
        "PRIME", "PRINT", "PRIOR", "PRIZE", "PROOF",
        "PROUD", "PROVE", "QUEEN", "QUICK", "QUIET",
        "QUITE", "QUOTE", "RADIO", "RAISE", "RANGE",
        "RAPID", "RATIO", "REACH", "READY", "REALM",
        "RIGHT", "RISEN", "RISKY", "ROBOT", "ROGER",
        "ROMAN", "ROUGH", "ROUND", "ROUTE", "RURAL",
        "SALAD", "SAUCE", "SAVED", "SCENE", "SCOPE",
        "SCORE", "SENSE", "SERVE", "SETUP", "SEVEN",
        "SHALL", "SHAME", "SHAPE", "SHARE", "SHARP",
        "SHELL", "SHIFT", "SHINE", "SHIRT", "SHOCK",
        "SHOOT", "SHORT", "SHOUT", "SIGHT", "SINCE",
        "SIXTY", "SIZED", "SKILL", "SLEEP", "SLIDE",
        "SMALL", "SMART", "SMELL", "SMILE", "SMOKE",
        "SOLID", "SOLVE", "SORRY", "SOUTH", "SPACE",
        "SPARE", "SPEAK", "SPEED", "SPEND", "SPENT",
        "SPLIT", "SPORT", "SPRAY", "SQUAD", "STACK",
        "STAGE", "STAKE", "STAND", "START", "STATE",
        "STEAL", "STEAM", "STEEL", "STEEP", "STICK",
        "STILL", "STOCK", "STORE", "STRIP", "STUCK",
        "STUDY", "STUFF", "STYLE", "SUGAR", "SUITE",
        "SUPER", "SURGE", "SWEET", "SWING", "TABLE",
        "TAKEN", "TASTE", "TEACH", "THANK", "THEME",
        "THERE", "THICK", "THING", "THINK", "THIRD",
        "THOSE", "THREE", "THROW", "TIGHT", "TIMES",
        "TIRED", "TITLE", "TODAY", "TOKEN", "TOTAL",
        "TOUCH", "TOUGH", "TRACE", "TRACK", "TRADE",
        "TRAIN", "TRAIT", "TRASH", "TREAT", "TREND",
        "TRIAL", "TRICK", "TRIED", "TRUCK", "TRULY",
        "TRUST", "TRUTH", "TWICE", "TWIST", "ULTRA",
        "UNCLE", "UNDER", "UNION", "UNITE", "UNITY",
        "UNTIL", "UPPER", "UPSET", "URBAN", "USAGE",
        "USUAL", "VALID", "VALUE", "VIDEO", "VIRUS",
        "VISIT", "VITAL", "VOCAL", "VOICE", "VOTER",
        "WASTE", "WATCH", "WATER", "WHEEL", "WHERE",
        "WHICH", "WHILE", "WHITE", "WHOLE", "WHOSE",
        "WIDER", "WOMAN", "WORLD", "WORRY", "WORSE",
        "WORST", "WORTH", "WOULD", "WOUND", "WRITE",
        "WRONG", "WROTE", "YOUNG", "YOUTH"
    };

    public string TargetWord => targetWord;
    public IReadOnlyList<string> Guesses => guesses;
    public int CurrentAttempt => guesses.Count;
    public bool IsSolved => isSolved;
    public bool IsFailed => isFailed;
    public bool IsGameOver => isSolved || isFailed;

    public WordlePuzzle()
    {
        targetWord = wordList[Random.Range(0, wordList.Length)];
    }

    public WordlePuzzle(string word)
    {
        targetWord = word.ToUpper();
    }

    public bool IsValidGuess(string guess)
    {
        if (guess.Length != WordLength) return false;
        return validGuesses.Contains(guess.ToUpper());
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

        // First pass: mark correct letters
        for (int i = 0; i < WordLength; i++)
        {
            if (guess[i] == targetWord[i])
            {
                result[i] = LetterState.Correct;
                targetUsed[i] = true;
                guessUsed[i] = true;
            }
        }

        // Second pass: mark misplaced letters
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
