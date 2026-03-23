// ============================================================
// TEMPORARY TEST FILE — DO NOT COMMIT
// To revert: delete this file and remove the NobleDialogueTest
// component from the Noble game object. Nothing else was changed.
// ============================================================

using System.Collections;
using UnityEngine;

/// <summary>
/// Attach to the Noble game object alongside the NPC component.
/// On Awake this script auto-configures everything the Noble needs:
///   • A Kinematic Rigidbody2D (required for OnTriggerEnter2D to fire reliably)
///   • A CapsuleCollider2D (physics blocker so the player cannot walk through)
///   • A CircleCollider2D set as trigger (approach detection zone)
/// Each is only added if a suitable collider of that kind is not already present.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class NobleDialogueTest : MonoBehaviour
{
    [Tooltip("Radius of the auto-added trigger detection zone (world units).")]
    public float detectionRadius = 1.5f;

    [Tooltip("Size of the auto-added physics blocker (world units). X = width, Y = height.")]
    public Vector2 bodyColliderSize = new Vector2(0.5f, 1f);

    private NPC _npc;
    private bool _running;

    void Awake()
    {
        _npc = GetComponent<NPC>();
        ConfigureRigidbody();
        EnsureColliders();
    }

    void ConfigureRigidbody()
    {
        var rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
    }

    void EnsureColliders()
    {
        bool hasPhysics = false;
        bool hasTrigger = false;

        foreach (var col in GetComponents<Collider2D>())
        {
            if (col.isTrigger) hasTrigger = true;
            else hasPhysics = true;
        }

        if (!hasPhysics)
        {
            var body = gameObject.AddComponent<CapsuleCollider2D>();
            body.size = bodyColliderSize;
            body.isTrigger = false;
            Debug.Log("[NobleDialogueTest] Added physics CapsuleCollider2D. Resize via bodyColliderSize in the Inspector to fit the Noble sprite.");
        }

        if (!hasTrigger)
        {
            var zone = gameObject.AddComponent<CircleCollider2D>();
            zone.radius = detectionRadius;
            zone.isTrigger = true;
            Debug.Log("[NobleDialogueTest] Added trigger CircleCollider2D. Resize via detectionRadius in the Inspector.");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (_running) return;

        if (!other.CompareTag("Player"))
        {
            Debug.LogWarning($"[NobleDialogueTest] Trigger hit by '{other.gameObject.name}' but it is not tagged 'Player'. Make sure the player GameObject has the 'Player' tag.");
            return;
        }

        if (_npc == null || _npc.dialogue == null)
        {
            Debug.LogWarning("[NobleDialogueTest] NPC component or its dialogue asset is not assigned.");
            return;
        }

        _running = true;
        StartCoroutine(AutoPlayDialogue());
    }

    IEnumerator AutoPlayDialogue()
    {
        var d = _npc.dialogue;

        _npc.nameText.text = d.npcName;
        if (_npc.npcPortraitImage != null && d.npcSprite != null)
            _npc.npcPortraitImage.sprite = d.npcSprite;

        _npc.dialoguePanel.SetActive(true);
        PauseController.SetPause(true);

        for (int i = 0; i < d.dialogue.Length; i++)
        {
            _npc.dialogueText.text = "";
            foreach (char c in d.dialogue[i])
            {
                _npc.dialogueText.text += c;
                yield return new WaitForSecondsRealtime(d.typingSpeed);
            }

            yield return new WaitForSecondsRealtime(d.autoProgressDelay);
        }

        _npc.dialogueText.text = "";
        _npc.dialoguePanel.SetActive(false);
        PauseController.SetPause(false);
        _running = false;
    }
}
