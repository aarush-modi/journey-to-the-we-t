# Red Packet Tracker

## Overview

The red packet tracker monitors red packet collection across maps and displays a counter ("n/6") on the PlayerPage in the player menu. Red packets can be awarded via dojo quest completion or directly by NPC scripts (e.g. the Village Elder grants the first red packet when the player accepts the intro dialogue).

## Scripts

| Script | Location | Purpose |
|--------|----------|---------|
| `RedPacketTracker` | `Scripts/Player/RedPacketTracker.cs` | Singleton that tracks which maps' dojo quests are completed |
| `PlayerMenuUI` | `Scripts/UI/PlayerMenuUI.cs` | Displays red packet count on PlayerPage (modified, not new) |

## RedPacketTracker

Singleton that persists across scenes (`DontDestroyOnLoad`). Exposes a public `Collect(string sceneName)` method that any script can call to award a red packet. Also listens to `QuestManager.onQuestCompleted` and auto-collects when a dojo quest completes.

### Configuration Fields

| Field | Type | Purpose |
|-------|------|---------|
| `dojoQuests` | `QuestData[]` | Quests that award red packets (wire in Inspector) |
| `mapNames` | `string[]` | Display names for the 6 maps |
| `mapSceneNames` | `string[]` | Corresponding Unity scene names (must match scene files exactly) |

### Key Methods

| Method | Purpose |
|--------|---------|
| `Collect(string sceneName)` | Records a red packet for the given scene name. Fires `onRedPacketCollected` if not already collected |
| `GetCount()` | Returns number of collected red packets |
| `IsCollected(string sceneName)` | Returns true if the red packet for that scene was earned |
| `GetMapNames()` | Returns the display name array |
| `GetMapSceneNames()` | Returns the scene name array |

### Events

| Event | When it fires |
|-------|---------------|
| `onRedPacketCollected` | After `Collect()` records a new scene (not fired if scene was already collected) |

## PlayerMenuUI Counter

`PlayerMenuUI` was extended with two fields under the "Red Packets" header:
- `redPacketLabelText` — a TMP text you set in the Inspector (code never touches it)
- `redPacketCountText` — code updates this to "n/6" via `RedPacketTracker.Instance.GetCount()`

Updates live via the `onRedPacketCollected` event.

## Data Flow

### Path 1: Dojo Quest Completion

```
DojoMasterNPC completes quest
    |
    v
QuestManager.Instance.CompleteQuest(questData)
    |
    v
onQuestCompleted event fires
    |
    v
RedPacketTracker.OnQuestCompleted(quest)
    |-- checks if quest is in dojoQuests array
    |-- calls Collect(SceneManager.GetActiveScene().name)
    |
    v
Collect(sceneName)
    |-- collectedScenes.Add(sceneName)  (HashSet; returns false if duplicate)
    |-- if new: fires onRedPacketCollected
    |
    v
PlayerMenuUI.UpdateRedPacketCount()  -->  "3/6"
```

### Path 2: Village Elder Intro (First Red Packet)

```
VillageElderNPC.Interact() → plays introDialogue
    |
    v
Player chooses "accepted"
    |
    v
VillageElderNPC.OnIntroComplete()
    |-- hasGivenPackage = true
    |-- RedPacketTracker.Instance.Collect(SceneManager.GetActiveScene().name)
    |-- gives package, starts quest
    |
    v
Collect(sceneName)
    |-- collectedScenes.Add(sceneName)
    |-- if new: fires onRedPacketCollected
    |
    v
PlayerMenuUI.UpdateRedPacketCount()  -->  "1/6"
```

> **Note:** Both paths converge on the same `Collect()` method. The `HashSet` guarantees a scene is never double-counted even if both paths fire for the same scene.

## Scene Setup

The `RedPacketTracker` GameObject lives in the **HUD.unity** scene. The counter is inside the menu canvas:

```
UI (Canvas)
  Menu
    Pages Container
      PlayerPage
        RedPacketLabelText (TMP)  -->  set your label in Inspector (e.g. "RED PACKETS:")
        RedPacketCountText (TMP)  -->  wired to PlayerMenuUI.redPacketCountText
```

## Granting a Red Packet (Simple Path)

If you just need an NPC or trigger to grant a red packet directly — no quest, no enemies — call `Collect()`:

```csharp
RedPacketTracker.Instance.Collect(SceneManager.GetActiveScene().name);
```

This is what `VillageElderNPC` does to give the player their first red packet on dialogue accept. The scene name is the key — each scene can only have one red packet, and `Collect()` is safe to call multiple times (the `HashSet` deduplicates).

## Setting Up a Red Packet Quest in Your Town (Full Dojo Path)

Use this when the red packet is earned by completing a combat quest (kill all enemies, talk to NPC, receive reward). This is the standard pattern used by every dojo.

### Step 1: Create the Quest Data Asset

1. In the Project window, navigate to `Assets > Data > Quests`
2. Right-click in the folder and choose **Create > Scriptable Objects > Quest Data**
3. Rename it to something descriptive (e.g. `DefeatDojoEnemies_SnowyTown`)
4. Click on it and fill in the Inspector:
   - **Quest Name**: the name shown in the quest log (e.g. "Defeat the Dojo Enemies")
   - **Description**: what the player needs to do (e.g. "Defeat all enemies in the Snowy Town dojo")
   - **Quest Type**: `Side`
   - **Is Completed**: leave unchecked

### Step 2: Create the Package Reward

1. In the Project window, navigate to `Assets > Data > Quests`
2. Right-click and choose **Create > Scriptable Objects > Package Data**
3. Rename it (e.g. `SnowyTownDojoPackage`)
4. Fill in:
   - **Package Name**: e.g. "Snowy Town Red Packet"
   - **Seal Description**: e.g. "A red packet from the Snowy Town dojo master"

### Step 3: Create the Dialogue Assets

You need 3 dialogue assets for the dojo master. Navigate to `Assets > Data > Dialogue`.

1. **Intro Dialogue** — Right-click > **Create > Scriptable Objects > NPC Dialogue**
   - Rename to e.g. `DojoMasterIntro_SnowyTown`
   - This is what the dojo master says when the player first talks to them
   - Include a choice with outcome `"accepted"` so the quest can start (look at `DojoMasterIntroDialogue 2.asset` in the same folder as a reference)

2. **Reminder Dialogue** — create another NPC Dialogue asset
   - Rename to e.g. `DojoMasterReminder_SnowyTown`
   - This plays when the player talks to the dojo master while the quest is in progress

3. **Complete Dialogue** — create another NPC Dialogue asset
   - Rename to e.g. `DojoMasterComplete_SnowyTown`
   - This plays when the player returns after defeating all enemies

### Step 4: Set Up the Enemies

You need two GameObjects: one that **groups the enemies** and one that **tracks whether the objective is done**. They're separate because the dojo master reads the tracker to know when to offer the reward.

Target hierarchy in your scene:

```
Scene Root
  DojoEnemyGroup          [EnemyGroupCondition] ──references──> DojoObjectiveTracker
    Enemy1                 (any prefab with IDamageable)
    Enemy2
    Enemy3
  DojoObjectiveTracker     [ObjectiveTracker]
```

1. Create an empty GameObject named **DojoEnemyGroup**
2. Add your enemy GameObjects as **children** of DojoEnemyGroup (they must implement `IDamageable`)
3. Select **DojoEnemyGroup** and click **Add Component** > **EnemyGroupCondition**
4. Create a sibling empty GameObject named **DojoObjectiveTracker**
5. Select it and click **Add Component** > **ObjectiveTracker**
6. Select **DojoEnemyGroup** again, drag **DojoObjectiveTracker** into the EnemyGroupCondition **Tracker** slot

`EnemyGroupCondition` polls its child `IDamageable` components each frame. When all are dead, it calls `ObjectiveTracker.SetComplete()`. The dojo master NPC checks this tracker to know when to show the completion dialogue.

### Step 5: Set Up the Dojo Master NPC

1. In your scene, create a GameObject for the dojo master (sprite + colliders)
   - See [npc-system.md](npc-system.md#unity-editor-setup-step-by-step) for full NPC setup steps including the interaction icon
2. Select the NPC and click **Add Component**, add **DojoMasterNPC**
3. In the Inspector, fill in the **DojoMasterNPC** fields:

| Section | Field | What to drag in |
|---------|-------|----------------|
| Dialogue | **Intro Dialogue** | Your intro dialogue asset |
| Dialogue | **Reminder Dialogue** | Your reminder dialogue asset |
| Dialogue | **Complete Dialogue** | Your complete dialogue asset |
| Quest | **Quest To Start** | Your QuestData asset from Step 1 |
| Quest | **Package To Give** | Your PackageData asset from Step 2 |
| Quest | **Objective Tracker** | The DojoObjectiveTracker GameObject from Step 4 |

4. Also wire the standard NPCBase fields (see the NPC Identity, Interaction, Dialogue UI, and Dialogue Choices sections — these are shared UI elements from the HUD)

> **Note: The dojo master automatically becomes untalkable after the quest is completed.** The `DojoMasterNPC` script handles this for you — once the player receives the package, `CanInteract()` returns false and the interaction icon is hidden. You do not need to set this up manually. Just make sure you have an **Interaction Icon** child GameObject wired into the **Interaction Icon** slot (see [npc-system.md](npc-system.md#adding-the-interaction-icon-above-the-npcs-head) for how to create one).

### Step 6: Register Your Quest with the RedPacketTracker

This is the step that makes your quest count toward the red packet total. **You need to switch scenes** — the RedPacketTracker lives in the HUD scene, not your town scene.

1. Open **HUD.unity** (this is where the RedPacketTracker GameObject lives)
2. In the Hierarchy, find the **RedPacketTracker** GameObject
3. Select it and look at the Inspector
4. Expand the **Dojo Quests** array
5. Increase the **Size** by 1
6. Drag your QuestData asset (from Step 1) into the new empty slot
7. Make sure your map's display name and scene name are in the **Map Names** and **Map Scene Names** arrays at the correct index (check the table in the Configuration Fields section above)
8. **Ctrl+S** to save

### Step 7: Test

1. Enter Play Mode (Ctrl+P)
2. Walk to the dojo master and press **E** — accept the quest
3. Kill all the enemies in the dojo group
4. Walk back to the dojo master and press **E** — he should give you the package and the red packet counter in the menu should go up by 1
5. Try talking to the dojo master again — he should be untalkable (icon hidden, no interaction)
6. Open the menu (Tab) and check the PlayerPage — the counter should show `1/6`

### Checklist

- [ ] QuestData asset created and filled in
- [ ] PackageData asset created and filled in
- [ ] 3 dialogue assets created (intro with `"accepted"` outcome, reminder, complete)
- [ ] Enemies set up as children of a group with EnemyGroupCondition
- [ ] ObjectiveTracker added and wired to EnemyGroupCondition
- [ ] DojoMasterNPC component added with all fields wired
- [ ] NPCBase fields wired (dialogue UI, interaction icon, NPC identity)
- [ ] QuestData added to RedPacketTracker's `dojoQuests` array
- [ ] Map name and scene name added to RedPacketTracker arrays

## Limitations

- No save/load persistence — red packet progress resets on app restart (matches current `QuestManager` behavior)
- When a save system is added, `collectedScenes` should be serialized alongside quest state
