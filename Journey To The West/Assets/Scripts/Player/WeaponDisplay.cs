using System.Collections;
using UnityEngine;

public class WeaponDisplay : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerCombat playerCombat;

    [Header("Position")]
    [SerializeField] private float handForwardOffset = 0.2f;
    [SerializeField] private float handSideOffset = 0f;
    [SerializeField] private float stabExtension = 0.25f;

    [Header("Rotation")]
    [SerializeField] private float baseRotationOffset = 270f;

    [Header("Down Diagonal Adjustment")]
    [SerializeField] private float downDiagonalForwardBoost = 0.1f; // extra push along dir for DownLeft/DownRight

    [Header("Timing")]
    [SerializeField] private float stabOutTime = 0.08f;
    [SerializeField] private float stabReturnTime = 0.18f;

    // Direction order: Up, UpRight, Right, DownRight, Down, DownLeft, Left, UpLeft
    private static readonly Vector2[] EightDirs =
    {
        Vector2.up,
        new Vector2( 1,  1).normalized,
        Vector2.right,
        new Vector2( 1, -1).normalized,
        Vector2.down,
        new Vector2(-1, -1).normalized,
        Vector2.left,
        new Vector2(-1,  1).normalized,
    };

    // +1 = right-hand side of facing dir, -1 = left-hand side
    // Tuned so the sword starts from the correct side for each direction
    private static readonly int[] SideSigns = { -1, 1, 1, -1, 1, 1, -1, -1 };
    //                                           Up  UR  R  DR   D  DL   L  UL

    private SpriteRenderer sr;
    private Collider2D hitbox;
    private MeleeHitbox meleeHitbox;
    private bool wasAttacking;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        hitbox = GetComponent<Collider2D>();
        meleeHitbox = GetComponent<MeleeHitbox>();
        sr.enabled = false;
        if (hitbox != null) hitbox.enabled = false;
    }

    private void LateUpdate()
    {
        bool isAttacking = playerCombat.IsAttacking();

        if (isAttacking && !wasAttacking)
        {
            StopAllCoroutines();
            StartCoroutine(StabRoutine(playerController.GetFacingDirection()));
        }

        wasAttacking = isAttacking;
    }

    private int GetClosestDir(Vector2 input)
    {
        int best = 0;
        float bestDot = -Mathf.Infinity;
        for (int i = 0; i < EightDirs.Length; i++)
        {
            float dot = Vector2.Dot(input.normalized, EightDirs[i]);
            if (dot > bestDot) { bestDot = dot; best = i; }
        }
        return best;
    }

    private IEnumerator StabRoutine(Vector2 rawDirection)
    {
        int dirIdx = GetClosestDir(rawDirection);
        Vector2 direction = EightDirs[dirIdx];

        bool flipX = direction.x < 0f;
        Vector2 perp = new Vector2(direction.y, -direction.x);
        float effectiveSide = handSideOffset * SideSigns[dirIdx];
        Vector2 origin = direction * handForwardOffset + perp * effectiveSide;

        // Shift DownRight (3) and DownLeft (5) further along their diagonal
        if (dirIdx == 3 || dirIdx == 5)
            origin += direction * downDiagonalForwardBoost;

        sr.flipX = flipX;
        sr.enabled = true;
        if (hitbox != null) hitbox.enabled = true;
        meleeHitbox?.PrepareForAttack();

        float angle = flipX
            ? -(Mathf.Atan2(direction.y, -direction.x) * Mathf.Rad2Deg + baseRotationOffset)
            :   Mathf.Atan2(direction.y,  direction.x) * Mathf.Rad2Deg + baseRotationOffset;
        transform.localEulerAngles = new Vector3(0f, 0f, angle);

        yield return LerpPosition(direction, origin, 0f,           stabExtension, stabOutTime);
        yield return LerpPosition(direction, origin, stabExtension, 0f,           stabReturnTime);

        yield return new WaitUntil(() => !playerCombat.IsAttacking());
        sr.enabled = false;
        if (hitbox != null) hitbox.enabled = false;
    }

    private IEnumerator LerpPosition(Vector2 direction, Vector2 origin, float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float dist = Mathf.Lerp(from, to, Mathf.Clamp01(t / duration));
            transform.localPosition = new Vector3(origin.x + direction.x * dist, origin.y + direction.y * dist, 0f);
            yield return null;
        }
        transform.localPosition = new Vector3(origin.x + direction.x * to, origin.y + direction.y * to, 0f);
    }
}
