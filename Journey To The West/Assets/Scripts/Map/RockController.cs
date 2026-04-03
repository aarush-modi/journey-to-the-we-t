using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class RockController : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private float tileSize = 1f;
    [SerializeField] private float pushSpeed = 4f;
    [SerializeField] private LayerMask obstacleLayer;

    private Rigidbody2D rb;
    private bool isMoving = false;
    private Vector2 targetPosition;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        SnapToGrid();
    }

    private void FixedUpdate()
    {
        if (!isMoving) return;

        Vector2 newPos = Vector2.MoveTowards(rb.position, targetPosition, pushSpeed * Time.fixedDeltaTime);
        rb.MovePosition(newPos);

        if (Vector2.Distance(rb.position, targetPosition) < 0.01f)
        {
            rb.MovePosition(targetPosition);
            rb.linearVelocity = Vector2.zero;
            isMoving = false;
        }
    }

    public bool TryPush(Vector2 direction)
    {
        if (isMoving) return false;

        direction = SnapToCardinal(direction);
        Vector2 destination = rb.position + direction * tileSize;

        if (!IsClear(destination)) return false;

        targetPosition = destination;
        isMoving = true;
        return true;
    }

    private bool IsClear(Vector2 position)
    {
        Collider2D hit = Physics2D.OverlapCircle(position, tileSize * 0.4f, obstacleLayer);
        return hit == null;
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
        Collider2D col = GetComponent<Collider2D>();
        Vector2 offset = col != null ? col.offset : Vector2.zero;

        float x = (Mathf.Round((rb.position.x + offset.x) / tileSize) * tileSize) - offset.x;
        float y = (Mathf.Round((rb.position.y + offset.y) / tileSize) * tileSize) - offset.y;
        rb.position = new Vector2(x, y);
    }
}