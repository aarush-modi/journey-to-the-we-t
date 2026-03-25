# Game Architecture

## Overview

Journey to the West is a 2D action game built in Unity using the new Input System, Cinemachine for camera, and TextMeshPro for UI text. The codebase has 31 scripts organized into 7 systems.

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

## All Scripts (31 total)

### Player (`Scripts/Player/`) — 7 scripts

| Script | Purpose |
|--------|---------|
| `PlayerController` | Movement via Rigidbody2D, animation states |
| `PlayerCombat` | HP, attacks, skills, death/respawn, checkpoints. Implements `IDamageable` |
| `GreedMeter` | Gold tracking with tiered stat bonuses |
| `PlayerInventory` | Items and packages with change events |
| `InteractionDetector` | Detects `IInteractable` objects, handles E key |
| `MeleeHitbox` | Delivers damage to `IDamageable` targets on collision |
| `PauseController` | Static utility for nested pause management |

### Player Data (`Scripts/Player/`) — 4 ScriptableObjects

| Script | Purpose |
|--------|---------|
| `ItemData` | Item definitions (name, type, icon, description) |
| `PackageData` | Quest package definitions (name, seal description) |
| `ArmorData` | Armor stats (name, damage reduction, sprite) |
| `SkillData` | Skill stats (name, damage, cooldown, icon) |

### NPC & Dialogue (`Scripts/Interfaces/`, `Scripts/NPC/`) — 5 scripts

| Script | Purpose |
|--------|---------|
| `NPCBase` | Abstract base for all NPCs. Dialogue UI, typing, choices, branching |
| `GenericNPC` | Simple NPC with one dialogue |
| `VillageElderNPC` | Quest giver with state tracking and dialogue outcomes |
| `NPCDialogue` | ScriptableObject for dialogue data + `DialogueChoice` class |
| `NobleDialogueTest` | Temporary test script (auto-setup colliders) |

### Quest (`Scripts/Quest/`) — 2 scripts

| Script | Purpose |
|--------|---------|
| `QuestManager` | Singleton managing active/completed quests. Persists across scenes |
| `QuestData` | ScriptableObject for quest definitions |

### UI (`Scripts/UI/`) — 5 scripts

| Script | Purpose |
|--------|---------|
| `HUDManager` | HP bar, gold display, greed tier icon, skill cooldown |
| `MenuController` | Tab key toggles menu canvas + pause |
| `QuestLogUI` | Displays active quests from QuestManager events |
| `QuestEntryUI` | Individual quest entry component |
| `TabController` | Tab switching for menu pages |

### Map & Scenes (`Scripts/Map/`) — 2 scripts

| Script | Purpose |
|--------|---------|
| `MapTransitions` | Trigger-based scene transitions with fade and camera bounds |
| `ScreenFader` | Singleton for fade in/out using CanvasGroup |

### Global (`Scripts/Global/`) — 1 script

| Script | Purpose |
|--------|---------|
| `DroppedGold` | Gold pickup dropped on player death. Implements `ICollectible` |

### Interfaces (`Scripts/Interfaces/`) — 3 interfaces

| Interface | Implemented by |
|-----------|---------------|
| `IInteractable` | `NPCBase` (and all NPC subclasses) |
| `IDamageable` | `PlayerCombat` |
| `ICollectible` | `DroppedGold` |

### Debug (`Scripts/Dev/`) — 2 scripts

| Script | Purpose |
|--------|---------|
| `HUDTest` | J key = take damage, K key = add gold |
| `GreedMeterDebug` | G key = +100 gold, H key = -150 gold |

## Singletons

| Singleton | Persists Across Scenes | Purpose |
|-----------|----------------------|---------|
| `QuestManager` | Yes | Quest state |
| `ScreenFader` | No | Fade transitions |
| `PauseController` | Static class | Pause state with depth tracking |

## Pause System

`PauseController` uses a depth counter for nested pauses:

- `SetPause(true)` → increments depth, sets `Time.timeScale = 0`
- `SetPause(false)` → decrements depth, only restores `timeScale` when depth reaches 0

**Used by:** `NPCBase` (during dialogue), `MenuController` (menu open), `MapTransitions` (during transitions)

## Event Connections

| Source | Event | Listener | Action |
|--------|-------|----------|--------|
| `PlayerCombat` | `OnHPChanged` | `HUDManager` | Update HP bar |
| `GreedMeter` | `OnGoldChanged` | `HUDManager` | Update gold display |
| `GreedMeter` | `OnTierChanged` | `HUDManager` | Update tier icon color |
| `QuestManager` | `onQuestStarted` | `QuestLogUI` | Add quest entry |
| `QuestManager` | `onQuestCompleted` | `QuestLogUI` | Remove quest entry |
| `PlayerInventory` | `onInventoryChanged` | (available) | Not yet connected to UI |
| `NPCBase` | `OnDialogueComplete` | NPC subclasses | Post-dialogue logic |

## Data Assets

| Asset | Location | Type |
|-------|----------|------|
| `ElderIntroDialogue` | `Assets/Data/Dialogue/` | NPCDialogue |
| `ElderReminderDialogue` | `Assets/Data/Dialogue/` | NPCDialogue |
| `OldManDialogue` | `Assets/NPC/Noble/` | NPCDialogue |
| `DeliverRedPackets` | `Assets/Data/Quests/` | QuestData |
| `Level1Package` | `Assets/Data/Quests/` | PackageData |

## Detailed Documentation

- [Dialogue System](dialogue-system.md) — branching dialogue, choices, outcomes
- [Quest System](quest-system.md) — quest manager, quest data, quest log UI
- [NPC System](npc-system.md) — interaction detection, NPC base, creating new NPCs
- [Inventory System](inventory-system.md) — items, packages, equipment, gold
