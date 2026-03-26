# NPC and Interaction System

## Overview

The NPC system uses an interface-based design where any GameObject can be interactable. The player detects interactables via a trigger collider, and NPCs share common functionality through an abstract base class.

For dialogue-specific details (branching, choices, outcomes), see [dialogue-system.md](dialogue-system.md).

## Scripts

| Script | Location | Purpose |
|--------|----------|---------|
| `IInteractable` | `Scripts/Interfaces/IInteractable.cs` | Interface for any interactable object |
| `InteractionDetector` | `Scripts/Player/InteractionDetector.cs` | Player-side detection and input handling |
| `NPCBase` | `Scripts/Interfaces/NPCBase.cs` | Abstract base class for all NPCs |
| `GenericNPC` | `Scripts/Interfaces/GenericNPC.cs` | Simple NPC with one dialogue |
| `VillageElderNPC` | `Scripts/NPC/VillageElderNPC.cs` | Quest-giving NPC with state tracking |
| `StatueNPC` | `Scripts/NPC/StatueNPC.cs` | Shrine that grants a one-time hustle style |

## IInteractable Interface

Any object implementing this interface can be interacted with by the player.

| Method | Purpose |
|--------|---------|
| `GetPromptText()` | Text shown to player (e.g. "Talk to Elder") |
| `CanInteract()` | Whether the object can currently be interacted with |
| `Interact(GameObject player)` | Called when the player interacts |
| `ShowInteractionIcon(bool show)` | Show/hide the interaction prompt icon |

## InteractionDetector

Attached as a child of the player GameObject. Uses a 2D trigger collider.

### How It Works

```
Player enters NPC trigger range
    |
    v
OnTriggerEnter2D: CanInteract()? → store reference, show icon
    |
    v
Player presses E
    |
    v
Update: call Interact(player) → if !CanInteract() after, hide icon
    |
    v
Player leaves range
    |
    v
OnTriggerExit2D: hide icon, clear reference
```

- The player reference is passed as `transform.parent.gameObject` (detector is on a child object)
- E key input uses the new Input System (`Keyboard.current.eKey`)
- `Interact()` is called even during active dialogue (this is how pressing E advances lines)
- `CanInteract()` is checked after interaction to decide if the icon should hide

## NPCBase

Abstract base class implementing `IInteractable`. Provides:

- **NPC identity**: name, face sprite
- **Interaction icon**: shown/hidden based on player proximity
- **Dialogue system**: typing effect, auto-progress, branching choices, outcomes
- **Pause integration**: pauses the game during dialogue

### Serialized Fields (Inspector)

| Section | Field | Purpose |
|---------|-------|---------|
| NPC Identity | `npcName` | Display name |
| NPC Identity | `faceSprite` | Portrait sprite |
| Interaction | `interactionIcon` | Icon GameObject shown when in range |
| Dialogue UI | `dialoguePanel` | The dialogue panel GameObject |
| Dialogue UI | `dialogueText` | TMP_Text for dialogue content |
| Dialogue UI | `nameText` | TMP_Text for NPC name |
| Dialogue UI | `npcPortraitImage` | Image for NPC portrait |
| Dialogue Choices | `choiceContainer` | Transform parent for choice buttons |
| Dialogue Choices | `choiceButtonPrefab` | Prefab for choice buttons |

### Key Properties

| Property | Type | Purpose |
|----------|------|---------|
| `isDialogueActive` | `bool` | Whether dialogue is currently showing |
| `lastDialogueOutcome` | `string` | Outcome tag from the last ended dialogue |

Subclasses must override `Interact(GameObject player)`.

## GenericNPC

Simplest NPC implementation. Plays a single assigned dialogue.

```csharp
public override void Interact(GameObject player)
{
    PlayDialogue(dialogue);
}
```

Use this for townspeople, signs, or any NPC with straightforward dialogue.

## VillageElderNPC

Stateful NPC that gives packages and starts quests.

### State Machine

```
hasGivenPackage == false
    |
    v
Interact → play introDialogue → choices shown
    |                                |
    |                          "accepted"  "declined"
    |                                |          |
    |                          give package    do nothing
    |                          start quest     (replay intro next time)
    |                          hasGivenPackage = true
    |
hasGivenPackage == true
    |
    v
Interact → play reminderDialogue
```

### Inspector Fields

| Field | Purpose |
|-------|---------|
| `introDialogue` | NPCDialogue for first interaction |
| `reminderDialogue` | NPCDialogue for repeat interactions |
| `packageToGive` | PackageData to add to player inventory |
| `questToStart` | QuestData to start via QuestManager |

## StatueNPC

Shrine NPC that lets the player choose a hustle style (one-time only).

```
Player interacts → blessing dialogue plays → dialogue ends
    → HustleStyleSelectionUI opens → player picks a style → confirms
    → HustleStyleManager.ApplyStyle() → hasBlessed = true (blocks re-selection)
```

## Creating a New NPC

### Simple NPC (no special logic)
1. Create a GameObject with a sprite and colliders
2. Add the `GenericNPC` component
3. Create an `NPCDialogue` ScriptableObject with your dialogue
4. Drag references into the Inspector (dialogue, UI elements, interaction icon)

### Custom NPC (with special behavior)
1. Create a new script extending `NPCBase`
2. Override `Interact(GameObject player)`
3. Use `PlayDialogue()` to start conversations
4. Use `OnDialogueComplete` event and `lastDialogueOutcome` for post-dialogue logic
