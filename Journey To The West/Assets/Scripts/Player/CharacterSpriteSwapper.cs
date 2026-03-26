using System.Collections.Generic;
using UnityEngine;

public class CharacterSpriteSwapper : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Dictionary<Sprite, Sprite> swapMap;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void BuildSwapMap(HustleStyleData fromStyle, HustleStyleData toStyle)
    {
        if (fromStyle == null || toStyle == null || fromStyle == toStyle)
        {
            swapMap = null;
            return;
        }

        swapMap = new Dictionary<Sprite, Sprite>();
        AddMappings(fromStyle.idleSprites, toStyle.idleSprites);
        AddMappings(fromStyle.walkSprites, toStyle.walkSprites);
        AddMappings(fromStyle.attackSprites, toStyle.attackSprites);
    }

    public void ClearSwapMap()
    {
        swapMap = null;
    }

    private void AddMappings(Sprite[] from, Sprite[] to)
    {
        if (from == null || to == null) return;
        int count = Mathf.Min(from.Length, to.Length);
        for (int i = 0; i < count; i++)
        {
            if (from[i] != null && to[i] != null)
            {
                swapMap[from[i]] = to[i];
            }
        }
    }

    private void LateUpdate()
    {
        if (swapMap == null || spriteRenderer == null) return;
        if (swapMap.TryGetValue(spriteRenderer.sprite, out Sprite replacement))
        {
            spriteRenderer.sprite = replacement;
        }
    }
}
