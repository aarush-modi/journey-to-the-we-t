# Quest System

## Overview

The quest system tracks active and completed quests using a singleton manager with event-driven UI updates. Quests are defined as ScriptableObjects and triggered by NPC interactions.

## Scripts

| Script | Location | Purpose |
|--------|----------|---------|
| `QuestManager` | `Scripts/Quest/QuestManager.cs` | Singleton that manages all quest state |
| `QuestData` | `Scripts/Quest/QuestData.cs` | ScriptableObject defining a quest |
| `QuestLogUI` | `Scripts/UI/QuestLogUI.cs` | Displays active quests in the UI |
| `QuestEntryUI` | `Scripts/UI/QuestEntryUI.cs` | Individual quest entry display |

## QuestData Fields

| Field | Type | Purpose |
|-------|------|---------|
| `questName` | `string` | Display name of the quest |
| `description` | `string` | Quest objective description |
| `questType` | `enum` | `Main` or `Side` |
| `isCompleted` | `bool` | Completion flag |

Create new quests via: Right-click > Create > Scriptable Objects > Quest Data

## QuestManager

Singleton that persists across scenes (`DontDestroyOnLoad`).

### Key Methods

| Method | Purpose |
|--------|---------|
| `StartQuest(QuestData quest)` | Adds quest to active list (prevents duplicates) |
| `CompleteQuest(QuestData quest)` | Moves quest from active to completed |
| `IsQuestActive(QuestData quest)` | Returns true if quest is currently active |
| `IsQuestCompleted(QuestData quest)` | Returns true if quest has been completed |
| `GetActiveQuestByName(string name)` | Finds an active quest by name |
| `GetActiveQuests()` | Returns read-only list of active quests |
| `GetCompletedQuests()` | Returns read-only list of completed quests |

### Events

| Event | When it fires |
|-------|---------------|
| `onQuestStarted` | After a quest is added to the active list |
| `onQuestCompleted` | After a quest is moved to completed |

## Quest Log UI

`QuestLogUI` subscribes to QuestManager events and dynamically creates/removes `QuestEntryUI` prefabs.

- On enable: subscribes to events, refreshes all entries
- On disable: unsubscribes from events
- `AddEntry(QuestData)`: instantiates a QuestEntry prefab
- `RemoveEntry(QuestData)`: destroys the entry GameObject

## Data Flow

```
NPC interaction (e.g. VillageElderNPC)
    |
    v
QuestManager.Instance.StartQuest(questData)
    |
    v
Quest added to activeQuests list
    |
    v
onQuestStarted event fires
    |
    v
QuestLogUI.OnQuestStarted() → creates QuestEntryUI prefab
```

## Quest Data Assets

| Asset | Location | Description |
|-------|----------|-------------|
| `DeliverRedPackets` | `Assets/Data/Quests/DeliverRedPackets.asset` | Main quest: deliver six red packets to the king |
| `Level1Package` | `Assets/Data/Quests/Level1Package.asset` | Package: Elder's Red Packet |

## How Quests Are Started

Currently, quests are started by NPCs after dialogue completes. Example from `VillageElderNPC`:

1. Player talks to elder and accepts (dialogue outcome = `"accepted"`)
2. `OnIntroComplete()` fires
3. Package added to player inventory
4. `QuestManager.Instance.StartQuest(questToStart)` called
5. Quest appears in the quest log UI

## Adding a New Quest

1. Create a `QuestData` ScriptableObject (Right-click > Create > Scriptable Objects > Quest Data)
2. Fill in name, description, and type
3. Reference it from whichever NPC or trigger should start it
4. Call `QuestManager.Instance.StartQuest(yourQuest)` at the appropriate time
5. Call `QuestManager.Instance.CompleteQuest(yourQuest)` when the objective is met
