using UnityEngine;
using Unity.Cinemachine;

public class MapTransitions : MonoBehaviour
{
    [SerializeField] PolygonCollider2D mapBoundary;
    [SerializeField] Direction direction;
    [SerializeField] Transform teleportTargetPosition;
    [SerializeField] float additionalPosition = 2f;
    [SerializeField] NickelNoumanNPC lockedTeleportNpc;

    CinemachineConfiner2D confiner;
    CinemachineCamera vcam;
    private bool hasPlayedRedPacketEscapeWarning;

    enum Direction { Up, Down, Left, Right, Teleport }

    private void Awake()
    {
        confiner = FindObjectOfType<CinemachineConfiner2D>();
        vcam = FindObjectOfType<CinemachineCamera>();

        if (direction == Direction.Teleport
            && lockedTeleportNpc == null
            && (gameObject.name == "1+" || (teleportTargetPosition != null && teleportTargetPosition.name == "2-")))
        {
            lockedTeleportNpc = FindObjectOfType<NickelNoumanNPC>();
            Debug.Log($"[{name}] Auto-linked lockedTeleportNpc: {(lockedTeleportNpc != null ? lockedTeleportNpc.name : "null")}", this);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
        {
            return;
        }

        if (direction == Direction.Teleport
            && lockedTeleportNpc != null
            && !lockedTeleportNpc.IsTeleporterUnlocked)
        {
            Debug.Log($"[{name}] Teleporter locked. Triggering {lockedTeleportNpc.name} dialogue.", this);
            lockedTeleportNpc.PlayLockedTeleporterEmote();
            lockedTeleportNpc.Interact(collision.gameObject);
            return;
        }

        if (direction == Direction.Teleport
            && teleportTargetPosition != null
            && teleportTargetPosition.name == "2-"
            && lockedTeleportNpc != null
            && KingModiBlackjackNPC.HasRedPacket
            && !hasPlayedRedPacketEscapeWarning)
        {
            hasPlayedRedPacketEscapeWarning = true;
            Debug.Log($"[{name}] Triggering NickelNouman escape warning before teleport to 2-.", this);
            lockedTeleportNpc.PlayRedPacketEscapeWarning(() => FadeTransition(collision.gameObject));
            return;
        }

        if (direction == Direction.Teleport)
        {
            Debug.Log($"[{name}] Teleporter unlocked or ungated. Moving player to {teleportTargetPosition?.name}.", this);
        }

        FadeTransition(collision.gameObject);
    }

    async void FadeTransition(GameObject player)
    {
        if (ScreenFader.Instance != null)
        {
            await ScreenFader.Instance.FadeOut();
        }

        if (confiner != null && mapBoundary != null)
        {
            confiner.BoundingShape2D = mapBoundary;
            confiner.InvalidateBoundingShapeCache();
        }

        UpdatePlayerPosition(player);

        if (direction == Direction.Teleport && vcam != null && teleportTargetPosition != null)
        {
            vcam.ForceCameraPosition(
                teleportTargetPosition.position + new Vector3(0, 0, -10f),
                Quaternion.identity
            );
        }

        if (ScreenFader.Instance != null)
        {
            await ScreenFader.Instance.FadeIn();
        }
    }

    private void UpdatePlayerPosition(GameObject player)
    {
        if (direction == Direction.Teleport)
        {
            if (teleportTargetPosition == null)
            {
                Debug.LogWarning($"{name} is set to Teleport but has no target position assigned.", this);
                return;
            }

            player.transform.position = teleportTargetPosition.position;
            return;
        }

        Vector2 newPosition = player.transform.position;
        switch (direction)
        {
            case Direction.Up:    newPosition.y += additionalPosition; break;
            case Direction.Down:  newPosition.y -= additionalPosition; break;
            case Direction.Left:  newPosition.x -= additionalPosition; break;
            case Direction.Right: newPosition.x += additionalPosition; break;
        }
        player.transform.position = newPosition;
    }
}
