using System.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class MerchantTownTeleporter3AGate : MonoBehaviour
{
    [SerializeField] private PolygonCollider2D mapBoundary;
    [SerializeField] private Transform teleportTargetPosition;
    [SerializeField] private NickelNoumanNPC nickel;

    private BoxCollider2D gateCollider;
    private CinemachineConfiner2D confiner;
    private CinemachineCamera vcam;
    private bool isTransitioning;

    private void Awake()
    {
        gateCollider = GetComponent<BoxCollider2D>();
        confiner = FindObjectOfType<CinemachineConfiner2D>();
        vcam = FindObjectOfType<CinemachineCamera>();

        if (nickel == null)
            nickel = FindObjectOfType<NickelNoumanNPC>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        TryHandleCollision(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        TryHandleCollision(collision);
    }

    private void TryHandleCollision(Collider2D collision)
    {
        GameObject player = ResolvePlayer(collision);
        if (player == null || isTransitioning)
            return;

        if (!CanUseTeleport())
        {
            if (nickel != null && !nickel.IsDialogueOpen)
            {
                nickel.Interact(player);
            }
            return;
        }

        if (nickel != null && nickel.IsDialogueOpen)
            return;

        _ = TeleportPlayerAsync(player);
    }

    private bool CanUseTeleport()
    {
        return nickel != null && nickel.HasSolvedRiddle;
    }

    private GameObject ResolvePlayer(Collider2D collision)
    {
        if (collision == null)
            return null;

        if (collision.CompareTag("Player"))
            return collision.gameObject;

        if (collision.attachedRigidbody != null && collision.attachedRigidbody.CompareTag("Player"))
            return collision.attachedRigidbody.gameObject;

        PlayerController controller = collision.GetComponentInParent<PlayerController>();
        if (controller != null)
            return controller.gameObject;

        PlayerCombat combat = collision.GetComponentInParent<PlayerCombat>();
        if (combat != null)
        {
            return combat.gameObject;
        }

        return null;
    }

    private async Task TeleportPlayerAsync(GameObject player)
    {
        if (player == null || teleportTargetPosition == null || isTransitioning)
            return;

        isTransitioning = true;

        if (ScreenFader.Instance != null)
            await ScreenFader.Instance.FadeOut();

        if (confiner != null && mapBoundary != null)
        {
            confiner.BoundingShape2D = mapBoundary;
            confiner.InvalidateBoundingShapeCache();
        }

        player.transform.position = teleportTargetPosition.position;

        if (vcam != null)
        {
            vcam.ForceCameraPosition(
                teleportTargetPosition.position + new Vector3(0f, 0f, -10f),
                Quaternion.identity
            );
        }

        if (ScreenFader.Instance != null)
            await ScreenFader.Instance.FadeIn();

        isTransitioning = false;
    }
}
