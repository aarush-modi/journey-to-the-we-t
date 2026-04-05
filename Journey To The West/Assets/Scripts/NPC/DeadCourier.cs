using UnityEngine;

public class DeadCourier : NPCBase
{
    [Header("Courier Dialogue")]
    [SerializeField] private NPCDialogue courierDialogue;

    [Header("Scattered Gold")]
    [SerializeField] private GameObject droppedGoldPrefab;
    [SerializeField] private int totalGold = 80;
    [SerializeField] private int goldPileCount = 5;
    [SerializeField] private float scatterRadius = 1.5f;

    private bool triggered;

    public override bool CanInteract() => !triggered;

    public override void Interact(GameObject player)
    {
        triggered = true;
        OnDialogueComplete.AddListener(SpawnScatteredGold);
        PlayDialogue(courierDialogue);
    }

    private void SpawnScatteredGold()
    {
        OnDialogueComplete.RemoveListener(SpawnScatteredGold);

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
}
