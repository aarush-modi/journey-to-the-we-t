# Dialogue System

## Overview

The dialogue system handles NPC conversations with typing effects, auto-progression, branching choices, and outcome-based logic. Dialogue content is stored in **ScriptableObjects** (`NPCDialogue`) and the runtime logic lives in **NPCBase**.

## Scripts

| Script | Location | Purpose |
|--------|----------|---------|
| `NPCDialogue` | `Scripts/Interfaces/NPCDialogue.cs` | ScriptableObject that holds all dialogue data |
| `DialogueChoice` | `Scripts/Interfaces/NPCDialogue.cs` | Data class for branching choices (same file) |
| `NPCBase` | `Scripts/Interfaces/NPCBase.cs` | Abstract base class with all dialogue runtime logic |
| `GenericNPC` | `Scripts/Interfaces/GenericNPC.cs` | Simple NPC that plays a single dialogue |
| `VillageElderNPC` | `Scripts/NPC/VillageElderNPC.cs` | NPC with quest logic that checks dialogue outcomes |

## NPCDialogue Fields

| Field | Type | Purpose |
|-------|------|---------|
| `npcName` | `string` | Display name shown in dialogue UI |
| `npcSprite` | `Sprite` | Portrait shown in dialogue UI |
| `dialogue` | `string[]` | Array of dialogue lines, referenced by index |
| `dialogueTime` | `float[]` | (unused currently) |
| `typingSpeed` | `float` | Seconds between each character typed (default 0.05) |
| `voiceSounds` | `AudioClip` | Sound played during typing |
| `voucePitch` | `float` | Pitch of voice sound |
| `autoProgressLines` | `bool[]` | If true for an index, that line auto-advances after a delay |
| `autoProgressDelay` | `float` | Seconds to wait before auto-advancing (default 1.5) |
| `endDialogueOutcomes` | `string[]` | Non-empty string = end dialogue here. The string is the outcome tag (e.g. `"accepted"`, `"declined"`) |
| `nextLineOverride` | `int[]` | Override which line to go to next. `-1` = normal (go to next index). `0+` = jump to that index |
| `choices` | `DialogueChoice[]` | Branching choice points |

## DialogueChoice Fields

| Field | Type | Purpose |
|-------|------|---------|
| `dialogueIndex` | `int` | Which dialogue line triggers this choice |
| `choices` | `string[]` | Button labels the player sees (e.g. `["Yes", "No"]`) |
| `nextDialogueIndexes` | `int[]` | Where each choice jumps to. Must match `choices` length. Element 0 in `choices` maps to element 0 here. |

## How Dialogue Flows

```
Player presses E
    |
    v
Is dialogue active? --NO--> StartDialogue() --> Type first line
    |
   YES
    |
    v
Waiting for choice? --YES--> Do nothing (player must click a button)
    |
   NO
    |
    v
Currently typing? --YES--> Skip animation, show full line
    |
   NO (line fully displayed, player pressed E to advance)
    |
    v
Check endDialogueOutcomes[currentIndex]
    |-- Non-empty string --> EndDialogue() (outcome saved to lastDialogueOutcome)
    |
    v
Check choices[] for matching dialogueIndex
    |-- Found --> Display choice buttons, wait for click
    |
    v
Check nextLineOverride[currentIndex]
    |-- >= 0 --> Jump to that index
    |-- -1 or missing --> Go to next index (currentIndex + 1)
    |
    v
More lines? --YES--> Type next line
    |
   NO --> EndDialogue()
```

## Branching Dialogue Example

Here's how the Village Elder's dialogue is set up:

```
Dialogue Lines:
  0: "Ah, young traveler. I have been waiting..."
  1: "Thanks! I knew you'd help"
  2: "Oh. Okay."
  3: "There is a red packet that must be delivered..."
  4: "Take this and deliver it safely."

Choices (1 entry):
  Element 0:
    dialogueIndex: 0        --> choice appears after line 0
    choices: ["Yes", "No"]
    nextDialogueIndexes: [1, 2]  --> Yes->line 1, No->line 2

End Dialogue Outcomes (size 5):
  0: ""           (not an end point, has choices)
  1: ""           (not an end point, continues)
  2: "declined"   (ends here)
  3: ""           (not an end point, continues)
  4: "accepted"   (ends here)

Next Line Override (size 5):
  0: -1    (doesn't matter, choices handle it)
  1:  3    (skip line 2, jump to line 3)
  2: -1    (doesn't matter, ends here)
  3: -1    (normal, go to line 4)
  4: -1    (doesn't matter, ends here)
```

**Yes path:** 0 --> 1 --> 3 --> 4 --> end (outcome: `"accepted"`)

**No path:** 0 --> 2 --> end (outcome: `"declined"`)

## Using Outcomes in NPC Scripts

After dialogue ends, `lastDialogueOutcome` contains the outcome string. Subclasses of NPCBase can check this in their `OnDialogueComplete` handler:

```csharp
private void OnIntroComplete()
{
    OnDialogueComplete.RemoveListener(OnIntroComplete);

    if (lastDialogueOutcome != "accepted") return;

    // Player said yes - give package, start quest, etc.
}
```

This means the NPC logic doesn't depend on line numbers, only on the outcome tag you set in the ScriptableObject.

## Unity Setup

### Choice UI (one-time setup)

1. Inside your **Dialogue Panel**, create a child **UI > Panel** named `ChoicesPanel`
2. Set its Rect Transform anchor to **Middle Center**, set width/height
3. Set Image alpha to `0` (invisible background)
4. Add a **Vertical Layout Group** component, untick Child Force Expand
5. Create a **UI > Button - TextMeshPro** inside ChoicesPanel, style it, then drag it to your Prefabs folder to make it a prefab
6. Delete the button from the hierarchy (the code spawns them dynamically)

### Per-NPC setup

On each NPC's Inspector, under **Dialogue Choices**:
- **Choice Container**: drag in `ChoicesPanel`
- **Choice Button Prefab**: drag in the button prefab

### Creating a new branching dialogue

1. Create a new NPCDialogue ScriptableObject (right-click > Create > Scriptable Objects > NPC Dialogue)
2. Fill in `dialogue` lines
3. Add entries to `choices` for each branching point
4. Set `endDialogueOutcomes` on lines where conversation should end
5. Set `nextLineOverride` on lines that need to skip over other branches (use `-1` for normal flow)
