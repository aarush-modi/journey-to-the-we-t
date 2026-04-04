using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Level 5 only. The dead courier narrative scene object.
/// Shows a story message when the player enters the trigger zone and spawns
/// scattered gold coins around the body on Start.
/// </summary>
public class DeadCourier : MonoBehaviour
{
    [Header("Narrative")]
    [SerializeField] private GameObject narrativePanel;
    [SerializeField] private TMP_Text narrativeText;
    [SerializeField] [TextArea(4, 8)] private string narrativeMessage =
        "A courier.\n\nHe didn't make it.\n\nCoins litter the dirt around him — the package torn open, contents scattered.\n\nJin pockets the gold. He doesn't think about what it means.";
    [SerializeField] private float displayDuration = 5f;

    [Header("Scattered Gold")]
    [SerializeField] private GameObject droppedGoldPrefab;
    [SerializeField] private int totalGold = 80;
    [SerializeField] private int goldPileCount = 5;
    [SerializeField] private float scatterRadius = 1.5f;

    private bool triggered;

    private void Start()
    {
        SpawnScatteredGold();
        if (narrativePanel != null) narrativePanel.SetActive(false);
    }

    private void SpawnScatteredGold()
    {
        if (droppedGoldPrefab == null || totalGold <= 0) return;

        int perPile = totalGold / goldPileCount;
        int remainder = totalGold % goldPileCount;

        for (int i = 0; i < goldPileCount; i++)
        {
            Vector2 offset = Random.insideUnitCircle * scatterRadius;
            Vector3 pos = transform.position + new Vector3(offset.x, offset.y, 0f);
            GameObject drop = Instantiate(droppedGoldPrefab, pos, Quaternion.identity);
            DroppedGold dg = drop.GetComponent<DroppedGold>();
            if (dg != null) dg.SetGoldAmount(perPile + (i == 0 ? remainder : 0));
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered || !other.CompareTag("Player")) return;
        triggered = true;
        StartCoroutine(ShowNarrative());
    }

    private IEnumerator ShowNarrative()
    {
        if (narrativePanel == null) yield break;
        if (narrativeText != null) narrativeText.text = narrativeMessage;
        narrativePanel.SetActive(true);
        yield return new WaitForSeconds(displayDuration);
        narrativePanel.SetActive(false);
    }
}
