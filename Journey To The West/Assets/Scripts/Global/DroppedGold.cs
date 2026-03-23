using UnityEngine;


// Gold drop when the player dies
// Implements ICollectible so pickup systems can interact with it generically
public class DroppedGold : MonoBehaviour, ICollectible
{
    [SerializeField] private int goldAmount;
    [SerializeField] private float despawnTime = 20f;

    private float timer;

    private void Start()
    {
        timer = despawnTime;
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            Destroy(gameObject);
        }
    }

    //Adds gold to the GreedMeter and destroys this object
    public void Collect(GameObject collector)
    {
        GreedMeter greedMeter = collector.GetComponent<GreedMeter>();
        if (greedMeter == null) return;

        greedMeter.AddGold(goldAmount);
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Collect(other.gameObject);
    }

    public void SetGoldAmount(int amount)
    {
        goldAmount = amount;
    }
}
