using UnityEditor;
using UnityEngine;
using System.Linq;

public static class HustleStyleSpriteSetup
{
    private static readonly (string styleName, string characterFolder)[] StyleMap =
    {
        ("Default", "Jin"),
        ("Brute", "Brute"),
        ("Scammer", "Scammer"),
        ("Haggler", "Haggler")
    };

    [MenuItem("Tools/Setup Hustle Style Sprites")]
    public static void SetupAllStyles()
    {
        int successCount = 0;

        foreach (var (styleName, folder) in StyleMap)
        {
            string assetPath = $"Assets/Resources/HustleStyles/{styleName}.asset";
            var style = AssetDatabase.LoadAssetAtPath<HustleStyleData>(assetPath);
            if (style == null)
            {
                Debug.LogWarning($"[SpriteSetup] Could not find HustleStyleData at {assetPath}");
                continue;
            }

            string basePath = $"Assets/Sprites/{folder}/SeparateAnim";

            style.idleSprites = LoadSortedSprites($"{basePath}/Idle.png");
            style.walkSprites = LoadSortedSprites($"{basePath}/Walk.png");
            style.attackSprites = LoadSortedSprites($"{basePath}/Attack.png");

            EditorUtility.SetDirty(style);
            successCount++;

            Debug.Log($"[SpriteSetup] {styleName} ({folder}): " +
                      $"{style.idleSprites.Length} idle, " +
                      $"{style.walkSprites.Length} walk, " +
                      $"{style.attackSprites.Length} attack sprites");
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[SpriteSetup] Done! Set up {successCount}/{StyleMap.Length} styles. Check Console for details.");
    }

    private static Sprite[] LoadSortedSprites(string assetPath)
    {
        var allAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
        if (allAssets == null || allAssets.Length == 0)
        {
            Debug.LogWarning($"[SpriteSetup] No assets found at {assetPath}. Is the sprite sheet sliced?");
            return new Sprite[0];
        }

        return allAssets
            .OfType<Sprite>()
            .OrderBy(s => GetSpriteIndex(s.name))
            .ToArray();
    }

    private static int GetSpriteIndex(string name)
    {
        int lastUnderscore = name.LastIndexOf('_');
        if (lastUnderscore >= 0 && int.TryParse(name.Substring(lastUnderscore + 1), out int index))
        {
            return index;
        }
        return 0;
    }
}
