using UnityEngine;

public interface IInteractable
{
    string GetPromptText();
    bool CanInteract();
    void Interact(GameObject player);
    void ShowInteractionIcon(bool show);
}
