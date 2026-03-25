using UnityEngine;
using UnityEngine.InputSystem;

public class MenuController : MonoBehaviour
{
    public GameObject menuCanvas;

    void Start()
    {
        menuCanvas.SetActive(false);
    }

    public void OnMenu(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            bool opening = !menuCanvas.activeSelf;
            menuCanvas.SetActive(opening);
            PauseController.SetPause(opening);
        }
    }
}
