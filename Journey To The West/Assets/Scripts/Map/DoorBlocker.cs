using UnityEngine;

/// <summary>
/// Place on an empty GameObject at each doorway to block enemy line of sight
/// without blocking player movement. Requires a BoxCollider2D.
/// Set the GameObject's layer to "DoorBlocker" and configure the Physics2D
/// collision matrix so Player does not collide with DoorBlocker.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class DoorBlocker : MonoBehaviour
{
    private void Reset()
    {
        // Auto-configure collider when component is first added
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        col.isTrigger = false;
        col.size = new Vector2(1f, 1f);
    }

    private void OnDrawGizmos()
    {
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col == null) return;

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.4f);
        Vector3 center = transform.position + (Vector3)col.offset;
        Vector3 size = new Vector3(col.size.x * transform.lossyScale.x, col.size.y * transform.lossyScale.y, 0.1f);
        Gizmos.DrawCube(center, size);
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.8f);
        Gizmos.DrawWireCube(center, size);
    }
}
