using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class RockController : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private float tileSize = 1f;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float slideSpeed = 5f;

    private Rigidbody2D rb;
    private Collider2D col;
    private bool isPushed = false;
    private bool isSliding = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.freezeRotation = true;
        SnapToGrid();
    }

    public bool TryPush(Vector2 direction)
    {
        if (isPushed || isSliding) return false;

        direction = SnapToCardinal(direction);
        Vector2 destination = rb.position + direction * tileSize;

        if (!IsClear(destination)) return false;

        // Move one tile
        rb.position = destination;
        transform.position = new Vector3(destination.x, destination.y, transform.position.z);

        isPushed = true;
        Invoke(nameof(ResetPush), 0.2f);

        // Check if the tile we just moved to is ice
        if (IsOnIce(destination))
            StartCoroutine(SlideOnIce(direction));

        return true;
    }

    private IEnumerator SlideOnIce(Vector2 direction)
    {
        isSliding = true;

        while (true)
        {
            Vector2 current = rb.position;
            Vector2 next = current + direction * tileSize;

            // Stop if the next cell is blocked
            if (!IsClear(next))
            {
                isSliding = false;
                yield break;
            }

            // Animate the slide across one tile
            yield return StartCoroutine(SlideTo(next));

            // Stop if the tile we just landed on is not ice
            if (!IsOnIce(rb.position))
            {
                isSliding = false;
                yield break;
            }
        }
    }

    private IEnumerator SlideTo(Vector2 target)
    {
        // Snap target to grid center before we even start moving
        target = SnapPositionToGrid(target);

        Vector2 start = rb.position;
        float distance = tileSize;
        float elapsed = 0f;
        float duration = distance / slideSpeed;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            Vector2 next = Vector2.Lerp(start, target, elapsed / duration);
            rb.MovePosition(next);
            yield return null;
        }

        rb.MovePosition(target);
        transform.position = new Vector3(target.x, target.y, transform.position.z);
    }

    private Vector2 SnapPositionToGrid(Vector2 position)
    {
        Vector2 offset = col != null ? col.offset : Vector2.zero;
        float worldX = position.x + offset.x;
        float worldY = position.y + offset.y;

        float x = (Mathf.Floor(worldX / tileSize) * tileSize + tileSize * 0.5f) - offset.x;
        float y = (Mathf.Floor(worldY / tileSize) * tileSize + tileSize * 0.5f) - offset.y;

        return new Vector2(x, y);
    }

    private bool IsOnIce(Vector2 position)
    {
        // Sample a small point at the rock's position to check for an ice tile trigger
        Collider2D[] hits = Physics2D.OverlapCircleAll(position, tileSize * 0.3f);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Ice") && hit.isTrigger)
                return true;
        }
        return false;
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
        float worldX = rb.position.x + offset.x;
        float worldY = rb.position.y + offset.y;

        float x = (Mathf.Floor(worldX / tileSize) * tileSize + tileSize * 0.5f) - offset.x;
        float y = (Mathf.Floor(worldY / tileSize) * tileSize + tileSize * 0.5f) - offset.y;

        rb.position = new Vector2(x, y);
        transform.position = new Vector3(x, y, transform.position.z);
    }

    public bool IsSliding() => isSliding;
}