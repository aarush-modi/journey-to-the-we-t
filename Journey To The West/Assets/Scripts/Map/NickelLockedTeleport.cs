using System.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;

public class NickelLockedTeleport : MonoBehaviour
{
    [SerializeField] private PolygonCollider2D mapBoundary;
    [SerializeField] private Transform teleportTargetPosition;
    [SerializeField] private NickelNoumanNPC nickel;

    private CinemachineConfiner2D confiner;
    private CinemachineCamera vcam;
    private GameObject pendingPlayer;
    private bool isTransitioning;
    private bool playerInside;

    private void Awake()
    {
        confiner = FindObjectOfType<CinemachineConfiner2D>();
        vcam = FindObjectOfType<CinemachineCamera>();
    }

    private void Update()
    {
        if (!playerInside)
            pendingPlayer = null;

        if (pendingPlayer == null || nickel == null) return;
        if (!nickel.HasSolvedRiddle || nickel.IsDialogueOpen || isTransitioning) return;

        _ = TeleportPlayerAsync(pendingPlayer);
        pendingPlayer = null;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;
        if (isTransitioning) return;
        playerInside = true;

        if (nickel != null && !nickel.HasSolvedRiddle)
        {
            pendingPlayer = collision.gameObject;
            nickel.Interact(collision.gameObject);
            return;
        }

        _ = TeleportPlayerAsync(collision.gameObject);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;
        playerInside = false;
    }

    private async Task TeleportPlayerAsync(GameObject player)
    {
        if (isTransitioning) return;
        if (teleportTargetPosition == null) return;

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
