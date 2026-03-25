using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionDetector : MonoBehaviour
{
    private IInteractable interactableInRange;

    private void Update()
    {
        if (interactableInRange == null) return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            interactableInRange.Interact(transform.parent.gameObject);

            if (!interactableInRange.CanInteract())
            {
                interactableInRange.ShowInteractionIcon(false);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IInteractable interactable) && interactable.CanInteract())
        {
            interactableInRange = interactable;
            interactable.ShowInteractionIcon(true);
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
}
