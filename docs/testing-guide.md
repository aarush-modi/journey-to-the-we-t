# Testing Guide — Journey to the West

This document describes every automated test in the project: what it tests, why it matters, and how to run it.

---

## How to Run Tests

1. Open the project in **Unity Editor**
2. Go to **Window > General > Test Runner**
3. You'll see two tabs: **EditMode** and **PlayMode**
4. Click **Run All** in each tab

**Important for Play Mode tests:** The scenes being tested must be in your Build Settings list. Go to **File > Build Settings** and make sure these scenes are in **Scenes In Build**:
- Main
- SnowyTown
- FishingPort
- MerchantTown

---

## Test Overview

| Category | File | Test Count | Runs In |
|---|---|---|---|
| Blackjack Logic | `BlackjackRoundTests.cs` | 14 | Edit Mode |
| Greed Meter Math | `GreedMeterLogicTests.cs` | 16 (+12 parameterized) | Edit Mode |
| Data Integrity | `ScriptableObjectValidationTests.cs` | 4 | Edit Mode |
| Scene Loading | `SceneLoadTests.cs` | 6 | Play Mode |
| **Total** | | **40** | |

---

## Edit Mode Tests

Edit Mode tests run instantly in the Editor without loading any scene. They test pure C# logic.

### BlackjackRoundTests.cs

**Location:** `Assets/Tests/EditMode/BlackjackRoundTests.cs`
**What it tests:** The `BlackjackRound` class — the complete Blackjack minigame logic (cards, dealing, hit/stand, scoring, outcomes).

Since Blackjack involves randomness (shuffled decks), these tests use retry loops to find the specific game state they need, then assert on it. This is intentional — we're testing behavioral invariants, not exact card values.

| Test Method | What It Verifies |
|---|---|
| `NewRound_DealsCards` | After creating a new round, both player and dealer have cards (totals > 0) |
| `Hit_AddsCardToPlayerHand` | Calling `Hit()` changes the player's hand total or ends the game |
| `Hit_WhenGameOver_DoesNothing` | Once the game is over, calling `Hit()` has no effect on total or outcome |
| `Hit_PlayerBusts_OutcomeIsDealerWin` | When the player hits until total > 21, the outcome is `DealerWin` |
| `Stand_DealerDrawsToSeventeenOrMore` | After `Stand()`, the dealer's total is always >= 17 (standard Blackjack rule) |
| `Stand_PlayerHigher_PlayerWins` | When player total > dealer total (no bust), outcome is `PlayerWin` |
| `Stand_DealerHigher_DealerWins` | When dealer total > player total (no bust), outcome is `DealerWin` |
| `Stand_EqualTotals_IsPush` | When both totals are equal, outcome is `Push` (draw) |
| `Stand_DealerBusts_PlayerWins` | When dealer draws past 21, outcome is `PlayerWin` |
| `ResetRound_ClearsGameState` | After `ResetRound()`, `IsGameOver` is false, `Outcome` is `None`, and both hands have cards |
| `GetPlayerTotal_AceCountsCorrectly` | With only 2 cards dealt, the total never exceeds 21 (Aces adjust from 11 to 1) |
| `RenderRoundState_ContainsPlayerTotal` | The text output of the game state includes the player's total and the "You:" label |
| `IsGameOver_FalseOnNewRound` | A fresh round (without opening Blackjack) has `IsGameOver == false` |
| `Outcome_NoneOnNewRound` | A fresh round (without opening Blackjack) has `Outcome == None` |

---

### GreedMeterLogicTests.cs

**Location:** `Assets/Tests/EditMode/GreedMeterLogicTests.cs`
**What it tests:** The `GreedMeterLogic` static class — pure math for gold tiers, flat stat bonuses, shields, buff text, and style modifiers.

This logic was extracted from the `GreedMeter` MonoBehaviour into a standalone static class (`GreedMeterLogic.cs`) specifically so it could be unit tested without needing a running Unity scene.

| Test Method | What It Verifies |
|---|---|
| `CalculateTier_ReturnsCorrectTier` | **12 parameterized cases** testing every tier boundary: 0/50/299 = `None`, 300/500/599 = `Tier1`, 600/899 = `Tier2`, 900/1199 = `Tier3`, 1200/2000 = `Tier4` |
| `GetBonusDamage_Tier3OrHigher_Returns20` | Tier3 and Tier4 get +20 flat damage bonus |
| `GetBonusDamage_BelowTier3_Returns0` | None, Tier1, Tier2 = no damage bonus (+0) |
| `GetBonusSpeed_ReturnsCorrectCumulative` | None=+0, Tier1=+0, Tier2=+15, Tier3=+25, Tier4=+35 |
| `GetBonusHP_ReturnsCorrectCumulative` | None=+0, Tier1=+0, Tier2=+20, Tier3=+20, Tier4=+30 |
| `GetShieldCount_ReturnsCorrectCumulative` | None=0, Tier1=1, Tier2=1, Tier3=2, Tier4=3 |
| `ApplyStyleModifier_ZeroBase_ReturnsZero` | 0 gold base amount always returns 0 regardless of modifier |
| `ApplyStyleModifier_NegativeBase_ReturnsZero` | Negative base amounts are clamped to 0 |
| `ApplyStyleModifier_PositiveBase_ReturnsModified` | 100 * 1.1 = 110, 100 * 1.0 = 100, 10 * 1.15 = 12 (rounded), 50 * 1.2 = 60 |
| `GetTierThresholds_ReturnsAscendingOrder` | Returns [300, 600, 900, 1200] |
| `GetMaxThreshold_Returns1200` | Max threshold is 1200 |
| `GetBuffText_None_ShowsAllZeroes` | None tier: "+0 DMG  +0 SPD  +0 HP  +0 SHD" |
| `GetBuffText_Tier1_ShowsShieldOnly` | Tier1: "+0 DMG  +0 SPD  +0 HP  +1 SHD" |
| `GetBuffText_Tier2_ShowsHPAndSpeed` | Tier2: "+0 DMG  +15 SPD  +20 HP  +1 SHD" |
| `GetBuffText_Tier3_ShowsDamageSpeedShields` | Tier3: "+20 DMG  +25 SPD  +20 HP  +2 SHD" |
| `GetBuffText_Tier4_ShowsAll` | Tier4: "+20 DMG  +35 SPD  +30 HP  +3 SHD" |

**Tier threshold reference:**

| Gold Range | Tier | Damage | Speed | HP | Shield |
|---|---|---|---|---|---|
| 0 – 299 | None | +0 | +0 | +0 | 0 |
| 300 – 599 | Tier1 | +0 | +0 | +0 | 1 |
| 600 – 899 | Tier2 | +0 | +15 | +20 | 1 |
| 900 – 1199 | Tier3 | +20 | +25 | +20 | 2 |
| 1200+ | Tier4 | +20 | +35 | +30 | 3 |

---

### ScriptableObjectValidationTests.cs

**Location:** `Assets/Tests/EditMode/ScriptableObjectValidationTests.cs`
**What it tests:** Every ScriptableObject asset in the project has valid, non-empty data. Catches common mistakes like forgetting to fill in a name field or leaving a prefab reference null.

These tests use `AssetDatabase.FindAssets()` to scan every asset of a given type, so they automatically cover new assets as you add them.

| Test Method | What It Verifies |
|---|---|
| `AllEnemyData_HaveValidStats` | Every `EnemyData` asset has: `maxHP > 0`, `baseGoldDrop >= 0`, `enemyName` not empty |
| `AllSkillData_HaveValidConfig` | Every `SkillData` asset has: `skillName` not empty, `goldCost >= 0`, `skillPrefab` not null |
| `AllQuestData_HaveValidNames` | Every `QuestData` asset has: `questName` not empty |
| `AllItemData_HaveValidNames` | Every `ItemData` asset has: `itemName` not empty |

---

## Play Mode Tests

Play Mode tests launch a real Unity scene and run inside the game loop. They're slower but can test things that require GameObjects, MonoBehaviours, and scene loading.

### SceneLoadTests.cs

**Location:** `Assets/Tests/PlayMode/SceneLoadTests.cs`
**What it tests:** Core scenes load without crashing and critical GameObjects exist after loading.

Each test loads a scene, waits 2 frames (one for the load, one for `Start()` calls), then checks for errors.

| Test Method | What It Verifies |
|---|---|
| `MainScene_LoadsWithoutErrors` | Main scene loads with no error-level log messages |
| `SnowyTown_LoadsWithoutErrors` | SnowyTown scene loads with no error-level log messages |
| `FishingPort_LoadsWithoutErrors` | FishingPort scene loads with no error-level log messages |
| `MerchantTown_LoadsWithoutErrors` | MerchantTown scene loads with no error-level log messages |
| `MainScene_PlayerExists` | After loading Main, a GameObject tagged "Player" exists |
| `MainScene_QuestManagerInitialized` | After loading Main, `QuestManager.Instance` is not null |

---

## Project Structure

```
Journey To The West/Assets/
├── Scripts/
│   ├── GameScripts.asmdef          <- assembly def so tests can reference game code
│   └── Player/
│       ├── GreedMeter.cs           <- delegates to GreedMeterLogic
│       └── GreedMeterLogic.cs      <- extracted pure logic (testable)
└── Tests/
    ├── EditMode/
    │   ├── EditMode.asmdef         <- assembly def (Editor-only platform)
    │   ├── BlackjackRoundTests.cs  <- 14 tests
    │   ├── GreedMeterLogicTests.cs <- 11 tests + 10 parameterized
    │   └── ScriptableObjectValidationTests.cs <- 4 tests
    └── PlayMode/
        ├── PlayMode.asmdef         <- assembly def (all platforms)
        └── SceneLoadTests.cs       <- 6 tests
```

---

## What These Tests Do NOT Cover

These automated tests cover logic and data integrity. The following areas require **manual QA playtesting** (see `QA-Checklist.md` at the project root):

- Visual correctness (sprites, animations, VFX)
- Game feel (movement responsiveness, camera behavior)
- UI layout and interaction flow
- NPC dialogue content and branching
- Full combat encounters and boss fights
- Scene transition timing and fade effects
- Audio
