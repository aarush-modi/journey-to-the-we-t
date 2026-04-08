# Game Architecture

## Overview

Journey to the West is a 2D action game built in Unity using the new Input System, Cinemachine for camera, and TextMeshPro for UI text. The codebase has 103 scripts organized into 14 systems.

## System Map

```
                        ┌─────────────────┐
                        │   Input System   │
                        └────────┬────────┘
              ┌──────────┬───────┼────────┬──────────┐
              v          v       v        v          v
        ┌──────────┐ ┌────────┐ ┌───┐ ┌────────┐ ┌─────────────────┐
        │ Movement │ │ Combat │ │ E │ │  Tab   │ │  Skill/Attack   │
        │(Player   │ │(Player │ │key│ │(Menu   │ │  (PlayerCombat) │
        │Controller│ │Combat) │ │   │ │Control)│ │                 │
        └──────────┘ └───┬────┘ │   │ └───┬────┘ └────────┬────────┘
                         │      │   │     │               │
                         v      v   │     v               v
                   ┌──────────────┐ │ ┌────────┐   ┌───────────┐
                   │  IDamageable │ │ │ Pause  │   │ MeleeHit  │
                   │  (HP/Death)  │ │ │Control │   │   box     │
                   └──────┬───────┘ │ └────────┘   └───────────┘
                          │         │
                     ┌────┘         v
                     v      ┌──────────────────┐
              ┌────────────┐│InteractionDetector│
              │DroppedGold ││                  │
              │(ICollect)  │└────────┬─────────┘
              └─────┬──────┘        │
                    v               v
              ┌──────────┐  ┌──────────────┐
              │GreedMeter│  │ IInteractable│
              └──────────┘  │  (NPCBase)   │
                            └──────┬───────┘
                         ┌─────────┼─────────┐
                         v         v         v
                   ┌──────────┐┌────────┐┌──────────┐
                   │GenericNPC││Village ││  Future  │
                   │          ││ElderNPC││  NPCs    │
                   └──────────┘└───┬────┘└──────────┘
                                   │
                            ┌──────┼──────┐
                            v      v      v
                      ┌────────┐┌──────┐┌─────────┐
                      │Player  ││Quest ││Dialogue │
                      │Inven   ││Manag ││Outcomes │
                      │tory    ││er    ││         │
                      └────────┘└──┬───┘└─────────┘
                                   │
                                   v
                            ┌────────────┐
                            │QuestLogUI  │
                            └────────────┘
```

## All Scripts (103 total)

### Player (`Scripts/Player/`) — 19 scripts

| Script | Purpose |
|--------|---------|
| `PlayerController` | Movement via Rigidbody2D, animation states, ice physics, sprint, greed bonus speed |
| `PlayerCombat` | HP, attacks, skills, death/respawn, checkpoints, iframes. Implements `IDamageable` |
| `GreedMeter` | Gold tracking with tiered stat bonuses. Fires `OnGoldChanged` and `OnTierChanged` |
| `GreedMeterLogic` | Pure C# static class with tier calculation and bonus math (extracted for testability) |
| `PlayerInventory` | Items and packages with change events |
| `InteractionDetector` | Detects `IInteractable` objects, handles E key |
| `MeleeHitbox` | Delivers damage to `IDamageable` targets on collision |
| `PauseController` | Static utility for nested pause management |
| `PersistentPlayer` | Singleton, persists across scenes. Finds `SpawnPoint` by ID on scene load, locks input during transitions, applies per-scene startup stats |
| `PersistentCamera` | Singleton with `Instance`, persists across scenes. Cinemachine tracking + confiner refresh. Has public `SnapToPlayer(Vector3)` that refreshes confiner bounds and forces camera position |
| `PersistentObject` | Generic DontDestroyOnLoad helper keyed by GameObject name |
| `RedPacketTracker` | Singleton, tracks red packet collection per-scene. Has `Collect(string sceneName)` and listens to quest completions |
| `PlayerShield` | Shield system with hit absorption. Replenishes on tier change. Fires `OnShieldChanged` |
| `DashAttackHandler` | Dash attack execution with ghost trail, wall collision, hit-stop, and chain-dash resets |
| `CursorReticle` | Dash targeting reticle that follows mouse, detects `IDamageable` in radius, draws indicator lines |
| `HustleStyleManager` | Singleton, persists across scenes. Manages active hustle style, applies gold/HP modifiers and sprite swaps |
| `CharacterSpriteSwapper` | Swaps player sprites at runtime based on hustle style |
| `InventoryController` | Creates and manages inventory slot grid for the skill inventory panel |
| `WeaponDisplay` | Renders equipped weapon sprite in the player's hand with directional positioning |

### Player Data (`Scripts/Player/`) — 4 ScriptableObjects

| Script | Purpose |
|--------|---------|
| `ItemData` | Item definitions (name, type, icon, description) |
| `PackageData` | Quest package definitions (name, seal description) |
| `ArmorData` | Armor stats (name, damage reduction, sprite) |
| `HustleStyleData` | Hustle style definitions (name, sprite swaps, gold/HP modifiers, bonus gold) |

### NPC & Dialogue (`Scripts/Interfaces/`, `Scripts/NPC/`) — 16 scripts

| Script | Purpose |
|--------|---------|
| `NPCBase` | Abstract base for all NPCs. Dialogue UI, typing, choices, branching |
| `GenericNPC` | NPC with intro and reminder dialogues |
| `VillageElderNPC` | Quest giver with state tracking and dialogue outcomes |
| `DojoMasterNPC` | Dojo quest giver: starts combat quest, gives package on completion |
| `NickelNoumanNPC` | Riddle NPC that gates teleporter access. Implements `IDamageable`, killable |
| `RoamingMerchantNPC` | Wandering merchant with dice-gambling mini-game. Implements `IDamageable`, drops gold on death |
| `MerchantNPC` | Shop merchant NPC that opens `MerchantShopController` after dialogue |
| `MerchantShopController` | Skill shop UI controller: buy skills with gold, deducts from `GreedMeter` |
| `KingModiBlackjackNPC` | Blackjack mini-game NPC. Implements `IDamageable`, grants red packet and fortune gold |
| `DeadCourier` | One-shot NPC that scatters gold piles after dialogue |
| `GhostGirlNPC` | Quest NPC that gives package, completes/starts quests, then fades away |
| `RockResetNPC` | NPC that resets all `RockController` positions via fade transition |
| `StatueNPC` | Blessing statue NPC that opens `HustleStyleSelectionUI` for style selection |
| `NickelRetreatMover` | Helper that slides Nickel Nouman to a retreat position |
| `NPCDialogue` | ScriptableObject for dialogue data + `DialogueChoice` class |
| `NobleDialogueTest` | Temporary test script (auto-setup colliders) |

### Quest (`Scripts/Quest/`) — 2 scripts

| Script | Purpose |
|--------|---------|
| `QuestManager` | Singleton managing active/completed quests. Persists across scenes |
| `QuestData` | ScriptableObject for quest definitions |

### Enemy (`Scripts/Enemy/`) — 15 scripts

| Script | Purpose |
|--------|---------|
| `EnemyController` | Basic melee enemy with `EnemyData`-driven stats, gold drops, flash feedback. Implements `IDamageable` |
| `RangedEnemy` | Ranged enemy with full FSM (patrol, chase, combat, retreat, search), A* pathfinding, projectile attacks. Implements `IDamageable` |
| `StealthGuard` | Patrol guard with stealth detection, waypoint patrol, investigation, and chase states. Implements `IDamageable` |
| `Level5Boss` | Boss with lunge attacks, sprite animation, intro dialogue, armor drop on death. Implements `IDamageable` |
| `ModiGuard` | Palace guard that chases on alert, contact damage, coordinates with `NickelNoumanNPC`. Implements `IDamageable` |
| `PickpocketThief` | Thief that chases player, steals gold, flees along scripted route. Implements `IDamageable` |
| `EnemyShield` | Shield hit absorption for enemies with visual shield icons |
| `EnemyBoundary` | Trigger collider that constrains enemy movement within bounds |
| `EnemyDeathEffect` | Death flash and particle effect sequence |
| `EnemyProjectile` | Spinning projectile with lifetime, obstacle collision, and damage delivery |
| `EnemyGroupCondition` | Monitors child `IDamageable` objects and marks `ObjectiveTracker` complete when all dead |
| `ObjectiveTracker` | Simple boolean completion flag used by `EnemyGroupCondition` and `DojoMasterNPC` |
| `StealthDetector` | Detection meter component: tracks player awareness (unaware/suspicious/alerted) via line-of-sight |
| `EnemyData` | ScriptableObject for basic enemy stats (name, HP, damage, speed, gold drop, sprite) |
| `RangedEnemyData` | ScriptableObject for ranged enemy stats (ranges, projectile, patrol, pathfinding) |

### Enemy AI (`Scripts/Enemy/AI/`) — 3 scripts

| Script | Purpose |
|--------|---------|
| `Pathfinding2D` | Singleton A* grid pathfinding on a 2D obstacle grid |
| `PathfindingZone` | Trigger zone that rebakes the A* grid when the player enters a room |
| `PathNode` | A* node data class (grid position, world position, walkable, G/H/F costs) |

### UI (`Scripts/UI/`) — 13 scripts

| Script | Purpose |
|--------|---------|
| `HUDManager` | HP bar with `current/max` text, gold display, greed tier color (5 tiers including purple), greed buff text |
| `MenuController` | Tab key toggles menu canvas + pause |
| `QuestLogUI` | Displays active quests from QuestManager events |
| `QuestEntryUI` | Individual quest entry component |
| `TabController` | Tab switching for menu pages |
| `PlayerMenuUI` | Player menu showing hustle style info and red packet count. Listens to `HustleStyleManager` and `RedPacketTracker` |
| `DamageVignette` | Singleton. Red screen flash on damage, pulsing vignette at low HP |
| `HotbarController` | Number-key hotbar for skills. Spawns slots, tracks active skill, triggers `PlayerCombat` |
| `InventorySlot` | Single inventory/hotbar slot with cooldown overlay and active highlight |
| `ShopSlot` | Shop UI slot displaying skill icon, name, price, and buy button |
| `GreedMeterMarkers` | Draws tier threshold markers on the greed meter slider |
| `HustleStyleCard` | UI card component for hustle style selection (icon, name, stats, description) |
| `HustleStyleSelectionUI` | Full-screen hustle style picker opened by `StatueNPC` |

### Skills (`Scripts/Skills/`) — 8 scripts

| Script | Purpose |
|--------|---------|
| `SkillData` | Base ScriptableObject for skill definitions (name, description, icon, disabledIcon, cooldown, gold cost, skill prefab) |
| `Skill` | MonoBehaviour wrapper on skill prefab instances. Calls `SkillData.Activate()` |
| `DashAttackSkill` | `SkillData` subclass for dash attack parameters (speed, damage, range, whiff cooldown) |
| `ShurikenBarrageSkill` | `SkillData` subclass that fires a fan of shuriken projectiles |
| `ShurikenProjectile` | Shuriken projectile with lifetime, collision, and damage |
| `RockPushSkill` | Passive `Skill` subclass (no hotbar activation, enables rock pushing) |
| `RockPushSkillData` | `SkillData` subclass for rock push (no-op activation) |
| `ItemDragHandler` | Drag-and-drop handler for skill icons between inventory and hotbar |

### Map & Scenes (`Scripts/Map/`) — 7 scripts

| Script | Purpose |
|--------|---------|
| `MapTransitions` | Trigger-based scene transitions with fade and camera bounds |
| `ScreenFader` | Singleton for fade in/out using CanvasGroup. Persists across scenes |
| `SceneTeleporter` | Trigger-based cross-scene teleportation with fade, sets `PersistentPlayer.PendingSpawnId` |
| `SpawnPoint` | Spawn point marker with string `spawnId`, placed in scenes for `PersistentPlayer` to find |
| `DoorBlocker` | Blocks enemy line-of-sight at doorways without blocking player movement |
| `ReplenishZone` | Trigger zone that restores player HP and shields to full |
| `RockController` | Pushable rock with grid snapping, cardinal movement, and ice sliding |

### Global (`Scripts/Global/`) — 2 scripts

| Script | Purpose |
|--------|---------|
| `DroppedGold` | Gold pickup dropped on player death or by enemies. Implements `ICollectible` |
| `ArmorPickup` | World-space armor pickup with optional post-collect dialogue. Implements `ICollectible` |

### Camera (`Scripts/Camera/`) — 1 script

| Script | Purpose |
|--------|---------|
| `CameraShakeManager` | Singleton wrapper for `CinemachineImpulseSource`. Called by dash hits and whiffs |

### Level (`Scripts/Level/`) — 1 script

| Script | Purpose |
|--------|---------|
| `Level5Manager` | Level 5 only. Watches for player death and reloads the scene instead of checkpoint respawn |

### Luck (`Scripts/Luck/`) — 1 script

| Script | Purpose |
|--------|---------|
| `Luck` | Percentage-based luck stat. `ShouldNegateDamage()` rolls against `luckPercent` |

### VFX (`Scripts/VFX/`) — 3 scripts

| Script | Purpose |
|--------|---------|
| `AnimatedVFX` | Frame-by-frame sprite animation with optional layered mode |
| `DashGhost` | Fading sprite ghost spawned during dash attack |
| `HitVFX` | Self-destroying hit effect with configurable lifetime |

### Interfaces (`Scripts/Interfaces/`) — 3 interfaces

| Interface | Implemented by |
|-----------|---------------|
| `IInteractable` | `NPCBase` (and all NPC subclasses) |
| `IDamageable` | `PlayerCombat`, `EnemyController`, `RangedEnemy`, `StealthGuard`, `Level5Boss`, `ModiGuard`, `PickpocketThief`, `NickelNoumanNPC`, `RoamingMerchantNPC`, `KingModiBlackjackNPC` |
| `ICollectible` | `DroppedGold`, `ArmorPickup` |

### Blackjack (`Scripts/Interfaces/`) — 1 script

| Script | Purpose |
|--------|---------|
| `BlackjackRound` | Pure C# blackjack game logic (deck, deal, hit, stand, outcome) |

### Debug (`Scripts/Dev/`) — 4 scripts

| Script | Purpose |
|--------|---------|
| `HUDTest` | J key = take damage, K key = add gold |
| `GreedMeterDebug` | G key = +100 gold, H key = -150 gold |
| `DummyScript` | Empty placeholder script |
| `ShopMenuDebugger` | Logs stack trace when shop menu is disabled |

## Singletons

| Singleton | Persists Across Scenes | Purpose |
|-----------|----------------------|---------|
| `QuestManager` | Yes | Quest state |
| `ScreenFader` | Yes | Fade transitions |
| `PersistentPlayer` | Yes | Player persistence across scenes, spawn point resolution |
| `PersistentCamera` | Yes | Camera tracking and confiner management |
| `RedPacketTracker` | Yes | Red packet collection state |
| `HustleStyleManager` | Yes | Hustle style selection, gold/HP modifiers, sprite swaps |
| `DamageVignette` | Yes | Screen damage flash and low-HP pulse |
| `MerchantShopController` | No | Skill shop state for current scene |
| `CameraShakeManager` | No | Camera impulse shake |
| `EnemyBoundary` | No | Enemy movement bounds for current scene |
| `Pathfinding2D` | No | A* grid pathfinding for current scene |
| `PauseController` | Static class | Pause state with depth tracking |

## Pause System

`PauseController` uses a depth counter for nested pauses:

- `SetPause(true)` → increments depth, sets `Time.timeScale = 0`
- `SetPause(false)` → decrements depth, only restores `timeScale` when depth reaches 0

**Used by:** `NPCBase` (during dialogue), `MenuController` (menu open), `MapTransitions` (during transitions), `MerchantShopController` (shop open), `NickelNoumanNPC` (warning/death dialogues), `RoamingMerchantNPC` (death dialogue)

## Event Connections

| Source | Event | Listener | Action |
|--------|-------|----------|--------|
| `PlayerCombat` | `OnHPChanged` | `HUDManager` | Update HP bar and `current/max` text |
| `PlayerCombat` | `OnSkillActivated` | `HotbarController` | Start cooldown overlay on active slot |
| `PlayerCombat` | `OnSkillCooldownReset` | `HotbarController` | Clear cooldown overlay |
| `GreedMeter` | `OnGoldChanged` | `HUDManager` | Update gold display |
| `GreedMeter` | `OnTierChanged` | `HUDManager` | Update tier icon color and buff text |
| `GreedMeter` | `OnTierChanged` | `PlayerCombat` | Update bonus HP and effective max HP |
| `GreedMeter` | `OnTierChanged` | `PlayerController` | Update bonus speed |
| `GreedMeter` | `OnTierChanged` | `PlayerShield` | Replenish shields to new tier count |
| `GreedMeter` | `OnGoldChanged` | `MerchantShopController` | Update money display and slot affordability |
| `QuestManager` | `onQuestStarted` | `QuestLogUI` | Add quest entry |
| `QuestManager` | `onQuestCompleted` | `QuestLogUI` | Remove quest entry |
| `QuestManager` | `onQuestCompleted` | `RedPacketTracker` | Record red packet collection |
| `RedPacketTracker` | `onRedPacketCollected` | `PlayerMenuUI` | Update red packet counter |
| `PlayerShield` | `OnShieldChanged` | `HUDManager` | Update shield display |
| `HustleStyleManager` | `OnStyleSelected` | `PlayerMenuUI` | Update hustle style display |
| `PlayerInventory` | `onInventoryChanged` | (available) | Not yet connected to UI |
| `NPCBase` | `OnDialogueComplete` | NPC subclasses | Post-dialogue logic |

## Data Assets

| Asset | Location | Type |
|-------|----------|------|
| `ElderIntroDialogue` | `Assets/Data/Dialogue/` | NPCDialogue |
| `ElderReminderDialogue` | `Assets/Data/Dialogue/` | NPCDialogue |
| `DojoMasterIntroDialogue 2` | `Assets/Data/Dialogue/` | NPCDialogue |
| `DojoMasterReminderDialogue` | `Assets/Data/Dialogue/` | NPCDialogue |
| `DojoMasterCompleteDialogue` | `Assets/Data/Dialogue/` | NPCDialogue |
| `NickelNoumanDialogue` | `Assets/Data/Dialogue/` | NPCDialogue |
| `KingModiDialogue` | `Assets/Data/Dialogue/` | NPCDialogue |
| `MerchantDialogue` | `Assets/Data/Dialogue/` | NPCDialogue |
| `GhostGirlDialogue` | `Assets/Data/Dialogue/` | NPCDialogue |
| `DeadCourierDialogue` | `Assets/Data/Dialogue/` | NPCDialogue |
| `BlessingDialogue` | `Assets/Data/Dialogue/` | NPCDialogue |
| `BlessingReminderDialogue` | `Assets/Data/Dialogue/` | NPCDialogue |
| `BlueSamuraiDialogue` | `Assets/Data/Dialogue/` | NPCDialogue |
| `CaveManDialogue` | `Assets/Data/Dialogue/` | NPCDialogue |
| `CaveWomanDialogue` | `Assets/Data/Dialogue/` | NPCDialogue |
| `ColdAndLostGuy` | `Assets/Data/Dialogue/` | NPCDialogue |
| `IceManDialogue` | `Assets/Data/Dialogue/` | NPCDialogue |
| `InspectorDialogue` | `Assets/Data/Dialogue/` | NPCDialogue |
| `MountainClimberDialogue` | `Assets/Data/Dialogue/` | NPCDialogue |
| `MountainGuyDialogue` | `Assets/Data/Dialogue/` | NPCDialogue |
| `ResetRocks1Dialogue` | `Assets/Data/Dialogue/` | NPCDialogue |
| `ResetRocks2Dialogue` | `Assets/Data/Dialogue/` | NPCDialogue |
| `ResetRocks3Dialogue` | `Assets/Data/Dialogue/` | NPCDialogue |
| `SnowVillager1` | `Assets/Data/Dialogue/` | NPCDialogue |
| `SnowVillager2` | `Assets/Data/Dialogue/` | NPCDialogue |
| `SnowVillager3` | `Assets/Data/Dialogue/` | NPCDialogue |
| `StarterVillager1` | `Assets/Data/Dialogue/` | NPCDialogue |
| `OldManDialogue` | `Assets/NPC/Noble/` | NPCDialogue |
| `DeliverRedPackets` | `Assets/Data/Quests/` | QuestData |
| `DefeatDojoEnemies` | `Assets/Data/Quests/` | QuestData |
| `GetToTheSnowyTown` | `Assets/Data/Quests/` | QuestData |
| `GetToTheFishingPort` | `Assets/Data/Quests/` | QuestData |
| `StarterPackage` | `Assets/Data/Quests/` | PackageData |
| `DojoMasterPackage` | `Assets/Data/Quests/` | PackageData |
| `Level2Package` | `Assets/Data/Quests/` | PackageData |
| `NinjaRed` | `Assets/Data/Enemies/` | RangedEnemyData |
| `StealthDetection` | `Assets/Data/Enemies/` | StealthDetectorData |
| `StealthGuard` | `Assets/Data/Enemies/` | EnemyData |
| `DashAttackSkill` | `Assets/Data/Skills/` | DashAttackSkill |
| `RockPushSkill` | `Assets/Data/Skills/` | RockPushSkillData |
| `ShurikenBarrageSkill` | `Assets/Data/Skills/` | ShurikenBarrageSkill |
| `GoldTrimmedRobe` | `Assets/Data/Armor/` | ArmorData |

## Detailed Documentation

- [Dialogue System](dialogue-system.md) — branching dialogue, choices, outcomes
- [Quest System](quest-system.md) — quest manager, quest data, quest log UI
- [NPC System](npc-system.md) — interaction detection, NPC base, creating new NPCs
- [Inventory System](inventory-system.md) — items, packages, equipment, gold
- [Combat System](combat-system.md) — player combat, enemy AI, damage, shields
- [Dash Mechanic](dash-mechanic.md) — dash attack, cursor reticle, chain dashes
- [Red Packet Tracker](red-packet-tracker.md) — red packet collection, quest integration
