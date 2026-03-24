using UnityEngine;
using System.Threading.Tasks;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance;
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] float fadeDuration = 0.5f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    async Task Fade(float personTransparency)
    {
        float start = canvasGroup.alpha, t = 0;
        while(t < fadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, personTransparency, t / fadeDuration);
            await Task.Yield();
        }
        canvasGroup.alpha = personTransparency;
    }

    public async Task FadeOut()
    {
        await Fade(1);
    }

    public async Task FadeIn()
    {
        await Fade(0);
    }
}