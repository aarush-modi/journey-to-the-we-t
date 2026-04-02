using UnityEngine;

public class BridgePuzzleNPC : NPCBase
{
    [Header("Dialogue")]
    [SerializeField] private NPCDialogue introDialogue;       // "Solve my puzzle to cross. Costs 25 gold."
    [SerializeField] private NPCDialogue noGoldDialogue;      // "You don't have enough gold."
    [SerializeField] private NPCDialogue solvedDialogue;      // "Well done, the bridge is yours."
    [SerializeField] private NPCDialogue failedDialogue;      // "Wrong! Come back and try again."

    [Header("Puzzle")]
    [SerializeField] private WordlePuzzleUI wordlePuzzleUI;
    [SerializeField] private int goldCost = 25;

    [Header("Bridge")]
    [SerializeField] private GameObject bridgeBlocker;  // collider/object blocking the bridge

    private bool hasSolved;
    private bool awaitingPuzzle;
    private GreedMeter playerGreedMeter;

    protected override void Start()
    {
        base.Start();
        if (bridgeBlocker != null)
            bridgeBlocker.SetActive(true);
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

        // Take the gold
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

            // Open the bridge
            if (bridgeBlocker != null)
                bridgeBlocker.SetActive(false);

            if (solvedDialogue != null)
                PlayDialogue(solvedDialogue);
        }
        else
        {
            // Player already lost the gold, can try again next time
            if (failedDialogue != null)
                PlayDialogue(failedDialogue);
        }
    }
}
