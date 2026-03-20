using UnityEngine;
using UnityEngine.InputSystem;

public class HustleStyleDebug : MonoBehaviour
{
    private HustleStyleData scammerStyle;
    private HustleStyleData bruteStyle;
    private HustleStyleData hagglerStyle;

    private void Awake()
    {
        scammerStyle = Resources.Load<HustleStyleData>("HustleStyles/Scammer");
        bruteStyle = Resources.Load<HustleStyleData>("HustleStyles/Brute");
        hagglerStyle = Resources.Load<HustleStyleData>("HustleStyles/Haggler");
    }

    private void Update()
    {
        if (Keyboard.current == null || HustleStyleManager.Instance == null)
        {
            return;
        }

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            HustleStyleManager.Instance.ApplyStyle(scammerStyle);
        }
        else if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            HustleStyleManager.Instance.ApplyStyle(bruteStyle);
        }
        else if (Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            HustleStyleManager.Instance.ApplyStyle(hagglerStyle);
        }
    }
}
