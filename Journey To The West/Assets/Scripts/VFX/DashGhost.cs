using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class DashGhost : MonoBehaviour
{
    public void Initialize(Sprite sprite, Vector3 position, Vector3 scale, bool flipX, Color startColor, float fadeDuration)
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = startColor;
        sr.flipX = flipX;
        sr.sortingLayerName = "Player";
        sr.sortingOrder = -1;

        transform.position = position;
        transform.localScale = scale;

        StartCoroutine(FadeOut(sr, startColor, fadeDuration));
    }

    private IEnumerator FadeOut(SpriteRenderer sr, Color startColor, float fadeDuration)
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startColor.a, 0f, elapsed / fadeDuration);
            sr.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        Destroy(gameObject);
    }
}
