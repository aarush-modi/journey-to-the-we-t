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
| `VillageElderNPC` | `Scripts/NPC/VillageElderNPC.cs` | Quest-giving NPC with state tracking; grants first red packet |
| `DojoMasterNPC` | `Scripts/NPC/DojoMasterNPC.cs` | Dojo quest NPC with intro/reminder/complete dialogue and objective tracking |
| `MerchantNPC` | `Scripts/NPC/MerchantNPC.cs` | Shop-opening NPC that triggers `MerchantShopController` after dialogue |
| `MerchantShopController` | `Scripts/NPC/MerchantShopController.cs` | Skill shop UI controller; sells `SkillData` items for gold |
| `RoamingMerchantNPC` | `Scripts/NPC/RoamingMerchantNPC.cs` | Wandering merchant with a dice-gambling mini-game |
| `NickelNoumanNPC` | `Scripts/NPC/NickelNoumanNPC.cs` | Riddle NPC that gates a teleporter; killable with death sequence and guard alert |
| `KingModiBlackjackNPC` | `Scripts/Interfaces/KingModiBlackjackNPC.cs` | Blackjack mini-game NPC that awards a red packet and Modi fortune on win |
| `GhostGirlNPC` | `Scripts/NPC/GhostGirlNPC.cs` | Quest NPC with intro/reminder dialogue and package reward |
| `StatueNPC` | `Scripts/NPC/StatueNPC.cs` | Blessing statue that opens a hustle-style card selection UI |
| `RockResetNPC` | `Scripts/NPC/RockResetNPC.cs` | NPC that resets rock puzzle state via dialogue |
| `DeadCourier` | `Scripts/NPC/DeadCourier.cs` | Dead courier NPC that scatters gold pickups on interaction |

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

Simplest NPC implementation. Plays an intro dialogue on first interaction and a reminder dialogue on subsequent interactions.

```csharp
public override void Interact(GameObject player)
{
    if (!hasSpokenBefore)
    {
        OnDialogueComplete.AddListener(OnIntroComplete);
        PlayDialogue(introDialogue);
    }
    else
    {
        PlayDialogue(reminderDialogue);
    }
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
    |                          hasGivenPackage = true
    |                          RedPacketTracker.Instance.Collect(currentScene)
    |                          give package    do nothing
    |                          start quest     (replay intro next time)
    |
hasGivenPackage == true
    |
    v
Interact → play reminderDialogue
```

When the player accepts the intro dialogue, `OnIntroComplete()` sets `hasGivenPackage = true`, then calls `RedPacketTracker.Instance.Collect()` with the current scene name to grant the first red packet, gives the package, and starts the quest.

### Inspector Fields

| Field | Purpose |
|-------|---------|
| `introDialogue` | NPCDialogue for first interaction |
| `reminderDialogue` | NPCDialogue for repeat interactions |
| `packageToGive` | PackageData to add to player inventory |
| `questToStart` | QuestData to start via QuestManager |

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

## Unity Editor Setup (Step by Step)

### Setting up the NPC GameObject

1. In your scene Hierarchy, right-click and choose **2D Object > Sprite** — rename it to your NPC's name (e.g. "DojoMaster")
2. Assign a sprite to the **Sprite Renderer** in the Inspector
3. With the NPC selected, click **Add Component** and add your NPC script (e.g. `GenericNPC`, `DojoMasterNPC`, etc.)
4. Click **Add Component** again, add a **Box Collider 2D**
   - Check **Is Trigger** — this is the interaction range the player walks into
   - Adjust the **Size** to be larger than the sprite (e.g. Size X: 3, Y: 3) so the player can trigger it from nearby
5. Set the **Tag** to `Untagged` (NPCs don't need a special tag — the player's `InteractionDetector` finds them via the `IInteractable` interface)

### Adding the Interaction Icon Above the NPC's Head

This is the icon that appears when the player is close enough to interact (e.g. an "E" prompt or exclamation mark).

1. Right-click your NPC in the Hierarchy and choose **Create Empty** — rename it to **InteractionIcon**
2. Select **InteractionIcon** and set its position above the NPC's head:
   - In the Inspector, set **Pos X**: `0`, **Pos Y**: `1.5` (adjust to taste — higher values move it further up), **Pos Z**: `0`
3. With **InteractionIcon** still selected, click **Add Component** and add a **Sprite Renderer**
4. Drag your icon sprite (e.g. an "E" key icon, exclamation mark, or speech bubble) into the **Sprite** field on the Sprite Renderer
5. Optionally adjust the **Order in Layer** to make sure it renders in front of the NPC (e.g. set to `10`)
6. Select the NPC parent GameObject, find the NPC script component in the Inspector, and drag **InteractionIcon** from the Hierarchy into the **Interaction Icon** slot under the **Interaction** header

The icon will auto-hide on start and appear/disappear when the player enters/exits the trigger range.

### Wiring the Dialogue UI

Every NPC that uses dialogue needs references to the shared dialogue UI elements. These typically live in the HUD scene.

1. Select your NPC GameObject
2. In the NPC script component, find the **Dialogue UI** section
3. Drag these from the Hierarchy into the matching slots:
   - **Dialogue Panel** — the panel GameObject that contains the dialogue box (find it under the HUD canvas)
   - **Dialogue Text** — the TMP_Text inside the dialogue panel for the dialogue content
   - **Name Text** — the TMP_Text for the NPC's name
   - **NPC Portrait Image** — the Image component for the NPC's face portrait
4. Under **Dialogue Choices**, drag:
   - **Choice Container** — the Transform that holds choice buttons (usually a vertical layout inside the dialogue panel)
   - **Choice Button Prefab** — the prefab for choice buttons (check `Assets/Prefabs` for an existing one)
5. Under **NPC Identity**, fill in:
   - **NPC Name** — the name displayed during dialogue
   - **Face Sprite** — the portrait sprite shown during dialogue

### Testing Your NPC

1. Enter **Play Mode** (Ctrl+P)
2. Walk the player near the NPC — the interaction icon should appear above their head
3. Press **E** — dialogue should start, game should pause
4. Press **E** again to advance lines, or click choice buttons if the dialogue has branching
5. When dialogue ends, the icon should hide if `CanInteract()` returns false, or stay visible if the NPC can be talked to again
