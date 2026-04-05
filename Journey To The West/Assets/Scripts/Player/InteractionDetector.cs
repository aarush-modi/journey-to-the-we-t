using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionDetector : MonoBehaviour
{
    private IInteractable interactableInRange;

    private void Update()
    {
        IInteractable interactable = NPCBase.CurrentDialogueNpc != null
            ? NPCBase.CurrentDialogueNpc
            : interactableInRange;

        if (interactable == null) return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            interactable.Interact(transform.parent.gameObject);

            if (interactable == interactableInRange && !interactableInRange.CanInteract())
            {
                interactableInRange.ShowInteractionIcon(false);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        TrySetInteractableInRange(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (interactableInRange == null)
        {
            TrySetInteractableInRange(collision);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IInteractable interactable) && interactable == interactableInRange)
        {
            interactable.ShowInteractionIcon(false);
            interactableInRange = null;
        }
    }

    private void TrySetInteractableInRange(Collider2D collision)
    {
        if (collision.TryGetComponent(out IInteractable interactable) && interactable.CanInteract())
        {
            interactableInRange = interactable;
            interactable.ShowInteractionIcon(true);
        }
    }
}
