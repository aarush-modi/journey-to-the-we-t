using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class AnimatedVFX : MonoBehaviour
{
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float frameDuration = 0.05f;
    [Tooltip("When true, each frame spawns as a new layer instead of replacing the previous one")]
    [SerializeField] private bool layered;

    private SpriteRenderer spriteRenderer;
    private int currentFrame;
    private float timer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (frames.Length > 0)
            spriteRenderer.sprite = frames[0];
    }

    private void Update()
    {
        if (frames == null || frames.Length == 0)
        {
            Destroy(gameObject);
            return;
        }

        timer += Time.deltaTime;
        if (timer >= frameDuration)
        {
            timer -= frameDuration;
            currentFrame++;
            if (currentFrame >= frames.Length)
            {
                Destroy(gameObject);
                return;
            }

            if (layered)
            {
                GameObject layer = new GameObject("Frame" + currentFrame);
                layer.transform.SetParent(transform, false);
                SpriteRenderer sr = layer.AddComponent<SpriteRenderer>();
                sr.sprite = frames[currentFrame];
                sr.sortingLayerID = spriteRenderer.sortingLayerID;
                sr.sortingOrder = spriteRenderer.sortingOrder + currentFrame;
            }
            else
            {
                spriteRenderer.sprite = frames[currentFrame];
            }
        }
    }
}
