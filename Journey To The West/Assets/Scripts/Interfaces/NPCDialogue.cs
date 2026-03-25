using UnityEngine;

[System.Serializable]
public class DialogueChoice
{
    public int dialogueIndex;
    public string[] choices;
    public int[] nextDialogueIndexes;
}

[CreateAssetMenu(fileName = "NewNPCDialogue", menuName = "Scriptable Objects/NPC Dialogue")]
public class NPCDialogue : ScriptableObject
{
    public string npcName;
    public Sprite npcSprite;
    public string[] dialogue;
    public float[] dialogueTime;
    public float typingSpeed = 0.05f;
    public AudioClip voiceSounds;
    public float voucePitch = 1.0f;
    public bool[] autoProgressLines;
    public float autoProgressDelay = 1.5f;
    public string[] endDialogueOutcomes;
    public int[] nextLineOverride;
    public DialogueChoice[] choices;

}
