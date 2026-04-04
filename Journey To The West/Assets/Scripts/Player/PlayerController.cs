using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float tileSize = 1f;

    private Rigidbody2D rb;
    private Animator animator;
    private PlayerCombat combat;
    private Vector2 moveInput;
    private Vector2 lastFacingDirection = Vector2.down;

    private bool isOnIce = false;
    private Vector2 iceVelocity = Vector2.zero;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        combat = GetComponent<PlayerCombat>();
    }

    private void FixedUpdate()
    {
        if (combat != null && combat.IsAttacking())
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (isOnIce)
        {
            rb.linearVelocity = iceVelocity;
        }
        else
        {
            rb.linearVelocity = moveInput * moveSpeed;
        }
    }

    private void Update()
    {
        // Unfreeze if somehow left frozen
        if (isOnIce
            && iceVelocity.sqrMagnitude < 0.01f
            && rb.constraints == RigidbodyConstraints2D.FreezeAll)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ice"))
        {
            isOnIce = true;
            iceVelocity = rb.linearVelocity;

            if (iceVelocity.sqrMagnitude < 0.01f)
                iceVelocity = lastFacingDirection * moveSpeed;

            // No diagonals
            if (Mathf.Abs(iceVelocity.x) >= Mathf.Abs(iceVelocity.y))
                iceVelocity = new Vector2(iceVelocity.x > 0 ? moveSpeed : -moveSpeed, 0f);
            else
                iceVelocity = new Vector2(0f, iceVelocity.y > 0 ? moveSpeed : -moveSpeed);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Ice"))
        {
            isOnIce = false;
            iceVelocity = Vector2.zero;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Rock"))
        {
            RockController rock = collision.gameObject.GetComponent<RockController>();
            if (rock != null)
            {
                Vector2 pushDir = (collision.transform.position - transform.position).normalized;
                pushDir = SnapToCardinal(pushDir);

                bool pushed = rock.TryPush(pushDir);

                // Whether the push succeeded or not, stop the player
                moveInput = Vector2.zero;
                rb.linearVelocity = Vector2.zero;
                rb.constraints = RigidbodyConstraints2D.FreezeAll;
                SnapToGridBeforeObstacle(pushDir);
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;

                // If on ice and the rock was blocked, treat it like a wall collision
                if (isOnIce && !pushed)
                {
                    iceVelocity = Vector2.zero;
                }
            }
            return;
        }

        // Existing ice wall collision
        if (!isOnIce) return;

        Vector2 slideDir = iceVelocity.normalized;
        bool headOn = false;

        foreach (ContactPoint2D contact in collision.contacts)
        {
            float dot = Vector2.Dot(contact.normal, slideDir);
            if (dot < -0.5f)
            {
                headOn = true;
                break;
            }
        }

        if (!headOn) return;

        Vector2 capturedSlideDir = iceVelocity;
        iceVelocity = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        SnapToGridBeforeObstacle(capturedSlideDir);
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }
    private void SnapToGridBeforeObstacle(Vector2 slideDir)
    {
        Collider2D col = GetComponent<Collider2D>();
        Vector2 colliderOffset = col != null ? col.offset : Vector2.zero;
        Vector2 pos = rb.position;

        // Cell centers in the grid
        if (Mathf.Abs(slideDir.x) > Mathf.Abs(slideDir.y))
        {
            // Sliding horizontally

            // Perpendicular axis (Y): snap to nearest cell center
            float worldCenterY = pos.y + colliderOffset.y;
            pos.y = (Mathf.Floor(worldCenterY / tileSize) * tileSize + tileSize * 0.5f) - colliderOffset.y;

            // Slide axis (X): land on the cell just before the obstacle
            float worldCenterX = pos.x + colliderOffset.x;
            if (slideDir.x > 0)
                // Moving right: floor puts us in the cell we're currently in
                pos.x = (Mathf.Floor(worldCenterX / tileSize) * tileSize + tileSize * 0.5f) - colliderOffset.x;
            else
                // Moving left: ceil puts us in the cell we're currently in
                pos.x = (Mathf.Ceil(worldCenterX / tileSize) * tileSize - tileSize * 0.5f) - colliderOffset.x;
        }
        else
        {
            // Sliding vertically

            // Perpendicular axis (X): snap to nearest cell center
            float worldCenterX = pos.x + colliderOffset.x;
            pos.x = (Mathf.Floor(worldCenterX / tileSize) * tileSize + tileSize * 0.5f) - colliderOffset.x;

            // Slide axis (Y): land on the cell just before the obstacle
            float worldCenterY = pos.y + colliderOffset.y;
            if (slideDir.y > 0)
                pos.y = (Mathf.Floor(worldCenterY / tileSize) * tileSize + tileSize * 0.5f) - colliderOffset.y;
            else
                pos.y = (Mathf.Ceil(worldCenterY / tileSize) * tileSize - tileSize * 0.5f) - colliderOffset.y;
        }

        rb.MovePosition(pos);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            moveInput = context.ReadValue<Vector2>();
            if (moveInput.sqrMagnitude > 0f)
                lastFacingDirection = moveInput.normalized;

            if (isOnIce && iceVelocity.sqrMagnitude < 0.01f)
            {
                Vector2 snapped = moveInput;
                if (Mathf.Abs(snapped.x) >= Mathf.Abs(snapped.y))
                    snapped = new Vector2(snapped.x > 0 ? 1f : -1f, 0f);
                else
                    snapped = new Vector2(0f, snapped.y > 0 ? 1f : -1f);

                iceVelocity = snapped * moveSpeed;
            }

            animator.SetBool("isWalking", true);
            animator.SetFloat("InputX", moveInput.x);
            animator.SetFloat("InputY", moveInput.y);
        }

        if (context.canceled)
        {
            animator.SetFloat("LastInputX", moveInput.x);
            animator.SetFloat("LastInputY", moveInput.y);
            moveInput = Vector2.zero;
            animator.SetBool("isWalking", false);
        }
    }

    public void Respawn(Vector3 position)
    {
        transform.position = position;
        iceVelocity = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    public Vector2 GetFacingDirection()
    {
        return moveInput.sqrMagnitude > 0f ? moveInput.normalized : lastFacingDirection;
    }

    private Vector2 SnapToCardinal(Vector2 dir)
    {
        if (Mathf.Abs(dir.x) >= Mathf.Abs(dir.y))
            return new Vector2(Mathf.Sign(dir.x), 0f);
        else
            return new Vector2(0f, Mathf.Sign(dir.y));
    }
}