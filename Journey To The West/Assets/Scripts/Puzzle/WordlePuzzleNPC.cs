using UnityEngine;

public class WordlePuzzleNPC : NPCBase
{
    [Header("Dialogue")]
    [SerializeField] private NPCDialogue introDialogue;
    [SerializeField] private NPCDialogue solvedDialogue;
    [SerializeField] private NPCDialogue failedDialogue;

    [Header("Puzzle")]
    [SerializeField] private WordlePuzzleUI wordlePuzzleUI;

    [Header("Rewards")]
    [SerializeField] private int goldReward = 50;

    private bool hasSolved;
    private bool awaitingPuzzle;

    protected override bool ShouldUnpauseOnDialogueEnd()
    {
        return !awaitingPuzzle;
    }

    public override void Interact(GameObject player)
    {
        if (hasSolved)
        {
            PlayDialogue(solvedDialogue);
        }
        else
        {
            awaitingPuzzle = true;
            OnDialogueComplete.AddListener(OnIntroDialogueComplete);
            PlayDialogue(introDialogue);
        }
    }

    private void OnIntroDialogueComplete()
    {
        OnDialogueComplete.RemoveListener(OnIntroDialogueComplete);
        awaitingPuzzle = false;

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

            // Award gold
            GreedMeter greedMeter = FindObjectOfType<GreedMeter>();
            if (greedMeter != null)
                greedMeter.AddGold(goldReward);
        }
        else
        {
            // Player can try again on next interaction
            if (failedDialogue != null)
            {
                PlayDialogue(failedDialogue);
            }
        }
    }
}
