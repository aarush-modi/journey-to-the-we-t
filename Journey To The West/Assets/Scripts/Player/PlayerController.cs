using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    // [SerializeField] public HustleStyleData hustleStyle; // null for now, T3-12 fills this but we skipped it for task 3

    private Rigidbody2D rb;
    private Animator animator;
    private PlayerCombat combat;
    private Vector2 moveInput;

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
        rb.linearVelocity = moveInput * moveSpeed;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            moveInput = context.ReadValue<Vector2>();
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
}