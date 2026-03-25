using UnityEngine;

public class GenericNPC : NPCBase
{
    [Header("Dialogue")]
    [SerializeField] private NPCDialogue dialogue;

    public override void Interact(GameObject player)
    {
        PlayDialogue(dialogue);
    }
}
