using UnityEngine;

public class HealerNPC : NPCBase
{
    [Header("Dialogue")]
    [SerializeField] private NPCDialogue healedDialogue;
    [SerializeField] private NPCDialogue fullHealthDialogue;

    private PlayerCombat currentPlayerCombat;

    public override void Interact(GameObject player)
    {
        currentPlayerCombat = player.GetComponent<PlayerCombat>();
        
        if (currentPlayerCombat != null)
        {
            // Check if player's current HP is less than their max HP
            if (currentPlayerCombat.GetCurrentHP() < currentPlayerCombat.GetMaxHP())
            {
                // Wait for the dialogue to finish before healing and fading
                OnDialogueComplete.AddListener(OnHealDialogueComplete);
                PlayDialogue(healedDialogue);
            }
            else
            {
                // Play the dialogue where they say the player is already at full health
                PlayDialogue(fullHealthDialogue);
            }
        }
    }

    private async void OnHealDialogueComplete()
    {
        // Immediately remove listener so it doesn't trigger on future unbound dialogues
        OnDialogueComplete.RemoveListener(OnHealDialogueComplete);

        if (currentPlayerCombat == null) return;

        // Fade out
        if (ScreenFader.Instance != null)
        {
            await ScreenFader.Instance.FadeOut();
        }

        // Heal them fully when the screen is black
        float amountToHeal = currentPlayerCombat.GetMaxHP() - currentPlayerCombat.GetCurrentHP();
        currentPlayerCombat.Heal(amountToHeal);

        // Fade in
        if (ScreenFader.Instance != null)
        {
            await ScreenFader.Instance.FadeIn();
        }
    }
}
