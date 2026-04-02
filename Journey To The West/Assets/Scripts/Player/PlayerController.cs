using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Animator animator;
    private PlayerCombat combat;
    private Vector2 moveInput;
    private Vector2 lastFacingDirection = Vector2.down;

    // --- Ice state ---
    private bool isOnIce = false;
    private Vector2 iceVelocity = Vector2.zero; // locked-in slide direction

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
            // Keep sliding at the locked velocity — collisions will stop the player naturally
            rb.linearVelocity = iceVelocity;
        }
        else
        {
            rb.linearVelocity = moveInput * moveSpeed;
        }
    }

    // Called when the player enters a trigger tagged "Ice"
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ice"))
        {
            isOnIce = true;
            // Lock in whatever velocity the player had when they stepped on ice
            iceVelocity = rb.linearVelocity;

            // If the player was standing still, give them a small nudge in their facing direction
            if (iceVelocity.sqrMagnitude < 0.01f)
                iceVelocity = lastFacingDirection * moveSpeed;

            // Prevent diagonal sliding
            if (Mathf.Abs(iceVelocity.x) >= Mathf.Abs(iceVelocity.y))
                iceVelocity = new Vector2(iceVelocity.x, 0f);
            else
                iceVelocity = new Vector2(0f, iceVelocity.y);
        }
    }

    // Called when the player exits the ice trigger
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Ice"))
        {
            isOnIce = false;
            iceVelocity = Vector2.zero;
        }
    }

    // Stop sliding when hitting a wall
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isOnIce)
        {
            iceVelocity = Vector2.zero;
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            moveInput = context.ReadValue<Vector2>();
            if (moveInput.sqrMagnitude > 0f)
                lastFacingDirection = moveInput.normalized;

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
    }

    public Vector2 GetFacingDirection()
    {
        return moveInput.sqrMagnitude > 0f ? moveInput.normalized : lastFacingDirection;
    }
}