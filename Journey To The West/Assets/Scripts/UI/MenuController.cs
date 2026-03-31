using UnityEngine;
using UnityEngine.InputSystem;

public class MenuController : MonoBehaviour
{
    public GameObject menuCanvas;

    void Awake()
    {
        menuCanvas.SetActive(false);
    }

    // Reads Tab key directly instead of going through Player Input events
    // because Unity 6's singleton input actions asset doesn't reliably
    // expand action maps into the Player Input event list.
    void Update()
    {
        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            bool opening = !menuCanvas.activeSelf;
            menuCanvas.SetActive(opening);
            PauseController.SetPause(opening);
        }
    }
}
