using UnityEngine;

/// <summary>
/// Place one per room with a BoxCollider2D trigger covering the room.
/// When the player enters, rebakes the A* grid to cover this room.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class PathfindingZone : MonoBehaviour
{
    [SerializeField] private Vector2 gridOrigin;
    [SerializeField] private Vector2Int gridSize = new Vector2Int(30, 30);

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (Pathfinding2D.Instance == null) return;

        Pathfinding2D.Instance.SetBounds(gridOrigin, gridSize);
        Pathfinding2D.Instance.BakeGrid();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 0.5f, 1f, 0.15f);
        Vector2 center = gridOrigin + new Vector2(gridSize.x * 0.5f, gridSize.y * 0.5f);
        Vector2 size = new Vector2(gridSize.x, gridSize.y);
        Gizmos.DrawCube(center, size);
        Gizmos.color = new Color(0f, 0.5f, 1f, 0.5f);
        Gizmos.DrawWireCube(center, size);
    }
}
