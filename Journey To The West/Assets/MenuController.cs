using UnityEngine;
using UnityEngine.InputSystem; // Added the new input namespace

public class MenuController : MonoBehaviour
{
    public GameObject menuCanvas;
    
    void Start()
    {
        menuCanvas.SetActive(false);
    }

    void Update()
    {
        // Using the New Input System to check the Tab key
        if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
        {
            menuCanvas.SetActive(!menuCanvas.activeSelf);
        }
    }
}
