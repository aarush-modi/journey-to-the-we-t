using UnityEngine;
using UnityEngine.InputSystem;

public class HUDTest : MonoBehaviour
{
    private PlayerCombat playerCombat;
    private GreedMeter greedMeter;

    private void Awake()
    {
        playerCombat = GetComponent<PlayerCombat>();
        greedMeter = GetComponent<GreedMeter>();
    }

    private void Update()
    {
        if (Keyboard.current.jKey.wasPressedThisFrame)
        {
            playerCombat.TakeDamage(10f);
            Debug.Log("HUDTest: took 10 damage");
        }

        if (Keyboard.current.kKey.wasPressedThisFrame)
        {
            greedMeter.AddGold(100);
            Debug.Log("HUDTest: added 100 gold");
        }
    }
}
