using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using System.Linq;

public static class RangedEnemyAnimSetup
{
    private static readonly string[] Directions = { "Down", "Left", "Right", "Up" };

    [MenuItem("Tools/Setup Ranged Enemy Animations")]
    public static void SetupAnimations()
    {
        // 1. Load sprites
        Sprite[] idleSprites = LoadSortedSprites("Assets/Sprites/NinjaRed/SeparateAnim/Idle.png");
        Sprite[] walkSprites = LoadSortedSprites("Assets/Sprites/NinjaRed/SeparateAnim/Walk.png");
        Sprite[] attackSprites = LoadSortedSprites("Assets/Sprites/NinjaRed/SeparateAnim/Attack.png");

        if (idleSprites.Length == 0 || walkSprites.Length == 0 || attackSprites.Length == 0)
        {
            Debug.LogError("[RangedEnemyAnimSetup] Missing sprites. Ensure all sprite sheets are sliced.");
            return;
        }

        // 2. Create animation clips
        // Walk: 4 frames per direction
        for (int dir = 0; dir < 4; dir++)
        {
            Sprite[] frames = walkSprites.Skip(dir * 4).Take(4).ToArray();
            CreateWalkClip($"NinjaRedWalk{Directions[dir]}", frames);
        }

        // Idle: 1 frame per direction
        for (int dir = 0; dir < 4; dir++)
        {
            CreateIdleClip($"NinjaRedIdle{Directions[dir]}", idleSprites[dir]);
        }

        // Attack: 1 frame per direction, not looping
        for (int dir = 0; dir < 4; dir++)
        {
            CreateAttackClip($"NinjaRedAttack{Directions[dir]}", attackSprites[dir]);
        }

        // 3. Create AnimatorController
        CreateAnimatorController();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[RangedEnemyAnimSetup] Done! Created 12 animation clips and NinjaRed.controller");
    }

    private static void CreateWalkClip(string name, Sprite[] frames)
    {
        AnimationClip clip = new AnimationClip();
        clip.frameRate = 60;

        // Build sprite keyframes
        ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[frames.Length + 1];
        for (int i = 0; i < frames.Length; i++)
        {
            keyframes[i] = new ObjectReferenceKeyframe
            {
                time = i * 0.25f,
                value = frames[i]
            };
        }
        // Duplicate last frame for clean loop
        keyframes[frames.Length] = new ObjectReferenceKeyframe
        {
            time = 1f,
            value = frames[frames.Length - 1]
        };

        EditorCurveBinding binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
        AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

        // Set looping
        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        AssetDatabase.CreateAsset(clip, $"Assets/Animations/{name}.anim");
    }

    private static void CreateIdleClip(string name, Sprite frame)
    {
        AnimationClip clip = new AnimationClip();
        clip.frameRate = 60;

        ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[]
        {
            new ObjectReferenceKeyframe { time = 0f, value = frame },
            new ObjectReferenceKeyframe { time = 0.25f, value = frame }
        };

        EditorCurveBinding binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
        AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        AssetDatabase.CreateAsset(clip, $"Assets/Animations/{name}.anim");
    }

    private static void CreateAttackClip(string name, Sprite frame)
    {
        AnimationClip clip = new AnimationClip();
        clip.frameRate = 60;

        ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[]
        {
            new ObjectReferenceKeyframe { time = 0f, value = frame },
            new ObjectReferenceKeyframe { time = 0.41666666f, value = frame }
        };

        EditorCurveBinding binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
        AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = false;
        settings.stopTime = 0.43333334f;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        AssetDatabase.CreateAsset(clip, $"Assets/Animations/{name}.anim");
    }

    private static void CreateAnimatorController()
    {
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(
            "Assets/Animations/NinjaRed.controller");
        AnimatorStateMachine rootStateMachine = controller.layers[0].stateMachine;

        // Add all states
        string[] prefixes = { "Idle", "Walk", "Attack" };
        AnimatorState defaultState = null;

        foreach (string prefix in prefixes)
        {
            foreach (string dir in Directions)
            {
                string stateName = $"NinjaRed{prefix}{dir}";
                string clipPath = $"Assets/Animations/{stateName}.anim";
                AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);

                AnimatorState state = rootStateMachine.AddState(stateName);
                state.motion = clip;

                if (stateName == "NinjaRedIdleDown")
                    defaultState = state;
            }
        }

        if (defaultState != null)
            rootStateMachine.defaultState = defaultState;

        EditorUtility.SetDirty(controller);
    }

    private static Sprite[] LoadSortedSprites(string assetPath)
    {
        var allAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
        if (allAssets == null || allAssets.Length == 0)
        {
            Debug.LogWarning($"[RangedEnemyAnimSetup] No assets found at {assetPath}. Is the sprite sheet sliced?");
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
