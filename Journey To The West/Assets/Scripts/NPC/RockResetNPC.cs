using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class RockResetNPC : NPCBase
{
    [Header("Dialogue")]
    [SerializeField] private NPCDialogue introDialogue;
    [SerializeField] private NPCDialogue resetDialogue;

    private bool hasHadIntro = false;

    private Dictionary<Rigidbody2D, Vector2> originalPositions = new();

    private void Start()
    {
        RockController[] allRocks = FindObjectsByType<RockController>(FindObjectsSortMode.None);
        foreach (RockController rock in allRocks)
        {
            Rigidbody2D rb = rock.GetComponent<Rigidbody2D>();
            if (rb != null)
                originalPositions[rb] = rb.position;
        }
    }

    public override void Interact(GameObject player)
    {
        if (!hasHadIntro)
        {
            OnDialogueComplete.AddListener(OnIntroComplete);
            PlayDialogue(introDialogue);
        }
        else
        {
            OnDialogueComplete.AddListener(OnResetDialogueComplete);
            PlayDialogue(resetDialogue);
        }
    }

    private void OnIntroComplete()
    {
        OnDialogueComplete.RemoveListener(OnIntroComplete);
        hasHadIntro = true;
    }

    private void OnResetDialogueComplete()
    {
        OnDialogueComplete.RemoveListener(OnResetDialogueComplete);
        _ = DoResetSequence();
    }

    private async Task DoResetSequence()
    {
        if (ScreenFader.Instance != null)
            await ScreenFader.Instance.FadeOut();

        ResetAllRocks();

        if (ScreenFader.Instance != null)
            await ScreenFader.Instance.FadeIn();
    }

    private void ResetAllRocks()
    {
        foreach (var kvp in originalPositions)
        {
            Rigidbody2D rb = kvp.Key;
            Vector2 origin = kvp.Value;

            if (rb == null) continue;

            rb.linearVelocity = Vector2.zero;
            rb.MovePosition(origin);
            rb.transform.position = new Vector3(origin.x, origin.y, rb.transform.position.z);

            RockController rock = rb.GetComponent<RockController>();
            if (rock != null)
            {
                rock.StopAllCoroutines();
                rock.ResetState();
            }
        }
    }
}