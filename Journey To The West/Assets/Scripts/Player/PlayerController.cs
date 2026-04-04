using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeedMultiplier = 2f;
    [SerializeField] private float forcedMoveSpeedMultiplier = 0.5f;
    // [SerializeField] public HustleStyleData hustleStyle; // null for now, T3-12 fills this but we skipped it for task 3

    private Rigidbody2D rb;
    private Animator animator;
    private PlayerCombat combat;
    private Vector2 moveInput;
    private Vector2 lastFacingDirection = Vector2.down;
    private bool canSprint;
    private bool isInputLocked;
    private bool isForcedMoving;

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
            animator.SetBool("isWalking", false);
            return;
        }

        if (isInputLocked)
        {
            if (!isForcedMoving)
            {
                rb.linearVelocity = Vector2.zero;
                animator.SetBool("isWalking", false);
                return;
            }

            Vector2 forcedDirection = Vector2.up;
            rb.linearVelocity = forcedDirection * moveSpeed * forcedMoveSpeedMultiplier;
            animator.SetBool("isWalking", true);
            animator.SetFloat("InputX", forcedDirection.x);
            animator.SetFloat("InputY", forcedDirection.y);
            return;
        }

        float activeMoveSpeed = moveSpeed;
        if (canSprint
            && Keyboard.current != null
            && (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed))
        {
            activeMoveSpeed *= sprintSpeedMultiplier;
        }

        rb.linearVelocity = moveInput * activeMoveSpeed;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (isInputLocked)
        {
            return;
        }

        if (context.performed)
        {
            moveInput = context.ReadValue<Vector2>();
            if (moveInput.sqrMagnitude > 0f)
            {
                lastFacingDirection = moveInput.normalized;
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
    }

    public Vector2 GetFacingDirection()
    {
        if (moveInput.sqrMagnitude > 0f)
        {
            return moveInput.normalized;
        }

        return lastFacingDirection;
    }

    public void ApplySprintLesson(float sprintMultiplier)
    {
        if (sprintMultiplier > 1f)
        {
            sprintSpeedMultiplier = sprintMultiplier;
            canSprint = true;
        }
    }

    public void SetMovementLocked(bool locked)
    {
        isInputLocked = locked;
        isForcedMoving = false;

        if (locked)
        {
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("isWalking", false);
            return;
        }

        animator.SetBool("isWalking", moveInput.sqrMagnitude > 0f);
    }

    public void SetForcedForwardMovement(bool enabled)
    {
        isInputLocked = enabled;
        isForcedMoving = enabled;

        if (!enabled)
        {
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("isWalking", moveInput.sqrMagnitude > 0f);
        }
    }
}
