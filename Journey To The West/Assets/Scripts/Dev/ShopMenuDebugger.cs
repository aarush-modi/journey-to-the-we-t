using UnityEngine;

public class ShopMenuDebugger : MonoBehaviour
{
    void OnDisable()
    {
        Debug.Log("ShopMenu was disabled! Stack trace: " + System.Environment.StackTrace);
    }
}