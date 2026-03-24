using UnityEngine;
using Unity.Cinemachine;

public class MapTransitions : MonoBehaviour
{
    [SerializeField] PolygonCollider2D mapBoundary;
    [SerializeField] Direction direction;
    [SerializeField] Transform teleportTargetPosition;

    CinemachineConfiner2D confiner;
    CinemachineCamera vcam; // or CinemachineVirtualCamera if on older Cinemachine

    enum Direction { Up, Down, Left, Right, Teleport }

    private void Awake()
    {
        confiner = FindObjectOfType<CinemachineConfiner2D>();
        vcam = FindObjectOfType<CinemachineCamera>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            confiner.BoundingShape2D = mapBoundary;
            confiner.InvalidateBoundingShapeCache();

            UpdatePlayerPosition(collision.gameObject);

            if (direction == Direction.Teleport)
            {
                vcam.ForceCameraPosition(
                    teleportTargetPosition.position + new Vector3(0, 0, -10f),
                    Quaternion.identity
                );
            }
        }
    }

    private void UpdatePlayerPosition(GameObject player)
    {
        if (direction == Direction.Teleport)
        {
            player.transform.position = teleportTargetPosition.position;
            return;
        }

        Vector2 newPos = player.transform.position;
        switch (direction)
        {
            case Direction.Up:    newPos.y += 2; break;
            case Direction.Down:  newPos.y -= 2; break;
            case Direction.Left:  newPos.x -= 2; break;
            case Direction.Right: newPos.x += 2; break;
        }
        player.transform.position = newPos;
    }
}