using System.Collections;
using UnityEngine;

public class NickelNoumanNPC : NPCBase
{
    private const int CorrectAnswerLineIndex = 7;

    [Header("Dialogue")]
    [SerializeField] private NPCDialogue dialogue;

    public bool HasSolvedRiddle { get; private set; }

    public override void Interact(GameObject player)
    {
        PlayDialogue(dialogue);
    }

    protected override void ChooseOption(int nextIndex)
    {
        if (nextIndex == CorrectAnswerLineIndex)
            HasSolvedRiddle = true;
        base.ChooseOption(nextIndex);
    }
}
