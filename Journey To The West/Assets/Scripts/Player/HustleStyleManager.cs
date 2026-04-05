using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class HustleStyleManager : MonoBehaviour
{
    private const string DefaultStylePath = "HustleStyles/Default";

    public static HustleStyleManager Instance { get; private set; }

    public UnityEvent<HustleStyleData> OnStyleSelected = new();

    private HustleStyleData currentStyle;
    private HustleStyleData defaultStyle;
    private bool hasChosenStyle;
    private bool grantedCurrentBonus;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance != null)
        {
            return;
        }

        GameObject managerObject = new GameObject(nameof(HustleStyleManager));
        managerObject.AddComponent<HustleStyleManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;

        defaultStyle = Resources.Load<HustleStyleData>(DefaultStylePath);
        if (defaultStyle == null)
        {
            Debug.LogError("HustleStyleManager could not load the default style asset.");
            return;
        }

        currentStyle = defaultStyle;
        ApplyCurrentStyleEffectsToActivePlayer(grantBonusGold: false);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyCurrentStyleEffectsToActivePlayer(grantBonusGold: false);
    }

    public void ApplyStyle(HustleStyleData style)
    {
        if (style == null)
        {
            return;
        }

        if (hasChosenStyle)
        {
            return;
        }

        currentStyle = style;
        hasChosenStyle = currentStyle != defaultStyle;
        grantedCurrentBonus = false;

        ApplyCurrentStyleEffectsToActivePlayer(grantBonusGold: true);

        if (hasChosenStyle)
        {
            OnStyleSelected?.Invoke(currentStyle);
        }
    }

    public HustleStyleData GetCurrentStyle()
    {
        return currentStyle != null ? currentStyle : defaultStyle;
    }

    public bool HasChosenStyle()
    {
        return hasChosenStyle;
    }

    public float GetCombatGoldModifier()
    {
        var style = GetCurrentStyle();
        return style != null ? style.combatGoldModifier : 1f;
    }

    public float GetNPCGoldModifier()
    {
        var style = GetCurrentStyle();
        return style != null ? style.npcGoldModifier : 1f;
    }

    public float GetShopPriceModifier()
    {
        var style = GetCurrentStyle();
        return style != null ? style.shopPriceModifier : 1f;
    }

    public void RefreshStyleEffects()
    {
        ApplyCurrentStyleEffectsToActivePlayer(grantBonusGold: false);
    }

    private void ApplyCurrentStyleEffectsToActivePlayer(bool grantBonusGold)
    {
        HustleStyleData style = GetCurrentStyle();
        if (style == null)
        {
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            return;
        }

        GreedMeter greedMeter = player.GetComponent<GreedMeter>();
        bool shouldGrantBonus = !grantedCurrentBonus && style.bonusGold > 0 && greedMeter != null &&
            (grantBonusGold || hasChosenStyle);

        if (shouldGrantBonus)
        {
            greedMeter.AddGold(style.bonusGold);
            grantedCurrentBonus = true;
        }

        PlayerCombat playerCombat = player.GetComponent<PlayerCombat>();
        if (playerCombat != null)
        {
            playerCombat.ApplyMaxHPModifier(style.maxHPModifier);
        }

        CharacterSpriteSwapper swapper = player.GetComponent<CharacterSpriteSwapper>();
        if (swapper == null)
        {
            swapper = player.AddComponent<CharacterSpriteSwapper>();
        }
        swapper.BuildSwapMap(defaultStyle, style);
    }
}
