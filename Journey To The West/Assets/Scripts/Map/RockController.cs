using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class RockController : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private float tileSize = 1f;
    [SerializeField] private LayerMask obstacleLayer;

    private Rigidbody2D rb;
    private Collider2D col;
    private bool isPushed = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.interpolation = RigidbodyInterpolation2D.None;
        rb.freezeRotation = true;
        SnapToGrid();
    }

    public bool TryPush(Vector2 direction)
    {
        if (isPushed) return false;

        direction = SnapToCardinal(direction);
        Vector2 destination = rb.position + direction * tileSize;

        if (!IsClear(destination))
        {
            Debug.Log("Blocked! Cannot push rock to: " + destination);
            return false;
        }

        rb.position = destination;
        transform.position = new Vector3(destination.x, destination.y, transform.position.z);

        isPushed = true;
        Invoke(nameof(ResetPush), 0.2f);
        return true;
    }

    private void ResetPush()
    {
        isPushed = false;
    }

    private bool IsClear(Vector2 position)
    {
        Vector2 checkSize = new Vector2(tileSize * 0.8f, tileSize * 0.8f);
        
        Collider2D[] hits = Physics2D.OverlapBoxAll(position, checkSize, 0f, obstacleLayer);

        foreach (Collider2D hit in hits)
        {
            if (hit == col) continue;
            Debug.Log("Obstacle found: " + hit.gameObject.name +
                    " | Layer: " + LayerMask.LayerToName(hit.gameObject.layer));
            return false;
        }

        return true;
    }

    private Vector2 SnapToCardinal(Vector2 dir)
    {
        if (Mathf.Abs(dir.x) >= Mathf.Abs(dir.y))
            return new Vector2(Mathf.Sign(dir.x), 0f);
        else
            return new Vector2(0f, Mathf.Sign(dir.y));
    }

    private void SnapToGrid()
    {
        Vector2 offset = col != null ? col.offset : Vector2.zero;

        // Mirror PlayerController's SnapToGridBeforeObstacle logic exactly:
        // Floor to tile, then add 0.5 to land on tile CENTER not tile edge
        float worldX = rb.position.x + offset.x;
        float worldY = rb.position.y + offset.y;

        float x = (Mathf.Floor(worldX / tileSize) * tileSize + tileSize * 0.5f) - offset.x;
        float y = (Mathf.Floor(worldY / tileSize) * tileSize + tileSize * 0.5f) - offset.y;

        rb.position = new Vector2(x, y);
        transform.position = new Vector3(x, y, transform.position.z);
    }
}