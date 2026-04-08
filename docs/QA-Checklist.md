# Journey To The West -- Manual QA Checklist

Use this checklist before each release to verify that every feature works end to end.
Test in Play mode from a clean scene load. Mark each item as you go.

---

## 1. Scene Loading and Transitions

### Scene Boot
- [ ] **Main** scene loads without errors in the console
- [ ] **SnowyTown** scene loads without errors in the console
- [ ] **MerchantTown** scene loads without errors in the console
- [ ] **Abandoned Outpost** scene loads without errors in the console
- [ ] **FishingPort** scene loads without errors in the console
- [ ] **Level 3** scene loads without errors in the console
- [ ] **Level 5 (t3-12b)** scene loads without errors in the console
- [ ] **HUD** scene loads without errors in the console

### SceneTeleporter Transitions
- [ ] Walking into a SceneTeleporter trigger fades the screen out
- [ ] The target scene loads after the fade completes
- [ ] Player spawns at the correct SpawnPoint (matching `targetSpawnId`)
- [ ] Player movement is locked during the fade transition (no sliding through)
- [ ] Main to SnowyTown teleporter works in both directions
- [ ] Main to MerchantTown teleporter works (if present)
- [ ] Returning from SnowyTown to Main places player at the correct spawn point

### MapTransitions (Intra-Scene Zones)
- [ ] Directional transitions (Up/Down/Left/Right) shift the player by the correct offset
- [ ] Cinemachine confiner updates to the new map boundary polygon after transition
- [ ] Teleport-type transitions move the player to the exact target position
- [ ] Screen fades out and back in during every MapTransition
- [ ] Camera snaps to the new position after a Teleport transition (no slow pan)

### Locked Teleporter (Nickel Nouman Gate)
- [ ] Teleporter "1+" is blocked when Nickel Nouman's riddle has not been answered
- [ ] Nickel Nouman shows the locked-teleporter emote sprite when player tries a locked teleporter
- [ ] Answering the riddle correctly (moneygrubber) unlocks the teleporter permanently for the session
- [ ] Answering incorrectly keeps the teleporter locked
- [ ] Killing Nickel Nouman unlocks the teleporter
- [ ] Killing all Modi Guards also unlocks the teleporter (mercy dialogue)

### Red Packet Escape Warning
- [ ] After winning the red packet from King Modi, using teleporter "1+" toward "2-" triggers Nickel's escape warning dialogue
- [ ] The warning dialogue only plays once per session
- [ ] After dismissing the warning, the fade transition proceeds normally

### ScreenFader
- [ ] Fade out reaches full black (alpha 1)
- [ ] Fade in returns to fully transparent (alpha 0)
- [ ] ScreenFader survives scene transitions (DontDestroyOnLoad)
- [ ] No duplicate ScreenFader instances after multiple scene loads

---

## 2. NPC Dialogue System

### Dialogue Panel UI
- [ ] DialoguePanel activates when any NPC interaction begins
- [ ] NPC name displays in the NameText field
- [ ] NPC portrait displays in the DialoguePortrait image
- [ ] Dialogue text types out letter by letter at the configured typing speed
- [ ] Pressing E during typing completes the line instantly
- [ ] ContinuePrompt appears after a line finishes typing
- [ ] Pressing E after a line finishes advances to the next line
- [ ] DialoguePanel hides when dialogue ends

### Dialogue Choices
- [ ] Choice buttons appear in ChoiceContainer at the correct dialogue index
- [ ] Each button shows the correct label with a number prefix (e.g., "[1] Accept")
- [ ] Clicking a choice button navigates to the correct dialogue line
- [ ] Number keys (1-9) also select the corresponding choice
- [ ] Choices are cleared after selection before the next line displays

### GenericNPC
- [ ] First interaction plays the intro dialogue
- [ ] Subsequent interactions play the reminder dialogue
- [ ] OnDialogueComplete fires after the intro finishes

### VillageElderNPC
- [ ] First interaction plays intro dialogue with accept/decline choice
- [ ] Choosing "accepted" outcome gives the player the package item
- [ ] Choosing "accepted" starts the configured quest via QuestManager
- [ ] Declining does not give the package or start the quest
- [ ] After accepting, subsequent interactions play the reminder dialogue

### DojoMasterNPC
- [ ] First interaction plays intro dialogue with accept/decline choice
- [ ] Accepting starts the quest via QuestManager
- [ ] While quest is active and objectives incomplete, plays reminder dialogue
- [ ] After objectives are complete (ObjectiveTracker.IsComplete), plays complete dialogue
- [ ] Complete dialogue gives the player the reward package
- [ ] Complete dialogue marks the quest as completed in QuestManager

### GhostGirlNPC
- [ ] First interaction plays intro dialogue
- [ ] After intro completes, gives the player a package
- [ ] Completes the configured quest
- [ ] Starts the next configured quest
- [ ] Screen fades out, GhostGirl disappears, screen fades back in
- [ ] Subsequent interactions are blocked (hasInteracted = true)

### StatueNPC (Hustle Style Selection)
- [ ] First interaction plays the blessing dialogue
- [ ] After dialogue ends, Hustle Style selection UI opens
- [ ] Game remains paused while selection UI is open
- [ ] Selecting a card applies the chosen Hustle Style
- [ ] Subsequent interactions play the reminder dialogue (no second selection)

### DeadCourier
- [ ] Interaction plays the courier dialogue
- [ ] After dialogue ends, gold piles scatter around the courier's position
- [ ] Gold piles can be collected by walking over them
- [ ] Second interaction is blocked (CanInteract returns false)

### RockResetNPC
- [ ] First interaction plays the intro dialogue
- [ ] Subsequent interactions play the reset dialogue
- [ ] After reset dialogue ends, screen fades out, all rocks return to original positions, screen fades in
- [ ] Rocks that were pushed are correctly reset to starting positions

### NickelNoumanNPC
- [ ] Interaction plays the riddle dialogue when guards are alive
- [ ] Correct answer ("moneygrubber") unlocks the teleporter for the session
- [ ] Wrong answer keeps teleporter locked and shows the wrong-answer response line
- [ ] When no living guards remain, shows mercy dialogue and auto-unlocks teleporter
- [ ] Nickel can be killed (TakeDamage / Die) and triggers death dialogue
- [ ] Death dialogue mentions whether guards are alive or not
- [ ] After death dialogue, Nickel's GameObject is deactivated
- [ ] Debug key (9) toggles teleporter unlock state

### KingModiBlackjackNPC
- [ ] Interaction shows the "overwhelming surge of greed" intro
- [ ] "Wait for Modi to notice you" button begins Modi's multi-line intro
- [ ] After intro, gamble choice appears (Yes / No)
- [ ] Choosing "No" takes all player gold and shows leave button
- [ ] Choosing "Yes" starts the blackjack round (see Blackjack section)
- [ ] With zero gold, Modi shows the "no money" dismiss dialogue
- [ ] After winning, subsequent interactions show post-win dialogue and deduct 5 gold
- [ ] Modi can be killed, triggering death sequence and loot dialogue
- [ ] Killing Modi grants the red packet and Modi's fortune (1000 gold)
- [ ] Killing Modi alerts all Modi Guards

### RoamingMerchantNPC (Dice Gambling)
- [ ] Approaching the merchant shows the emote icon above them
- [ ] Interaction shows a random quip with "Let's gamble" / "Ignore" choices
- [ ] Choosing "Ignore" closes dialogue normally
- [ ] Choosing "Let's gamble" deducts the bet (50 gold) and rolls dice
- [ ] Dice roll lines show: ante, player roll, house roll, outcome
- [ ] Win: player receives 2x bet back (net +bet)
- [ ] Loss: player loses the bet
- [ ] Push: player gets bet returned (net 0)
- [ ] Boxcars (player 6+6): player receives 3x bet back
- [ ] Snake eyes (player 1+1): player loses 2x bet
- [ ] House boxcars (6+6): player loses 2x bet
- [ ] After round, "Gamble again" / "Leave" choices appear
- [ ] "Leave" shows final gold summary with merchant reaction
- [ ] Going broke mid-session shows the "broke" dialogue and ends gambling
- [ ] Merchant can be killed; drops gold, shows death dialogue, then deactivates
- [ ] With zero gold, merchant dismisses player ("Get away from here, peon.")

### Dialogue State Management
- [ ] Only one dialogue panel is active at a time
- [ ] Game pauses (Time.timeScale = 0) when dialogue opens
- [ ] Game unpauses when dialogue closes
- [ ] Combat is disabled on the player while dialogue is active
- [ ] Combat re-enables after dialogue closes
- [ ] Walking away from an NPC during dialogue does not break state

---

## 3. Merchant Shop

### Opening and Closing
- [ ] MerchantNPC dialogue leads to a shop-open choice
- [ ] Choosing the shop option closes the dialogue panel and opens the shop UI
- [ ] Game remains paused while shop UI is open
- [ ] Close button on shop UI hides the panel and unpauses the game

### Display
- [ ] Shop inventory grid populates with all configured skills for sale
- [ ] Each ShopSlot shows the skill icon, name, and gold cost
- [ ] Player's current gold is displayed and updates in real time

### Purchasing
- [ ] Clicking a skill with enough gold deducts the cost from GreedMeter
- [ ] Purchased skill appears in the first empty inventory slot
- [ ] Purchased skill is removed from the shop grid (does not reappear)
- [ ] Remaining shop slots refresh affordability styling after a purchase
- [ ] Gold display updates immediately after purchase

### Rejection
- [ ] Clicking a skill with insufficient gold does not deduct any gold
- [ ] A "Not enough gold" message appears in the console (or future UI feedback)
- [ ] Slot remains in the shop and is visually marked as unaffordable

### Inventory Full
- [ ] If all inventory slots are occupied, purchase is blocked
- [ ] A "No empty inventory slots" message appears in the console
- [ ] Gold is not deducted

---

## 4. Quest System

### Quest Start
- [ ] QuestManager.StartQuest adds the quest to the active list
- [ ] onQuestStarted event fires and QuestLogUI adds the entry
- [ ] Starting the same quest twice does nothing (no duplicate)
- [ ] Starting a completed quest does nothing

### Quest Log UI
- [ ] Opening the menu (Tab) shows the quest log panel
- [ ] Active quests display with correct name and description
- [ ] Quest entry prefab instantiates under the content parent
- [ ] Closing and reopening the menu preserves quest state

### Quest Completion
- [ ] QuestManager.CompleteQuest removes the quest from active and adds to completed
- [ ] onQuestCompleted event fires and QuestLogUI removes the entry
- [ ] Quest's isCompleted flag is set to true
- [ ] Completing an inactive quest does nothing

### Quest NPCs Integration
- [ ] VillageElderNPC starts a quest when player accepts the intro
- [ ] DojoMasterNPC starts a quest on accept, completes it after objectives met
- [ ] GhostGirlNPC completes one quest and starts another on first interaction

---

## 5. Combat

### Player Melee Attack
- [ ] Left-click triggers the attack animation
- [ ] Attack respects the cooldown timer (no spam faster than attackCooldown)
- [ ] Attack is blocked while game is paused
- [ ] Attack is blocked while player is dead
- [ ] Attack direction matches movement direction (blend tree)
- [ ] MeleeHitbox deals damage to enemies with IDamageable

### Player Taking Damage
- [ ] Enemy contact calls PlayerCombat.TakeDamage
- [ ] HP decreases by the correct amount
- [ ] Hurt flash (red tint) plays briefly on the player sprite
- [ ] Armor reduces incoming damage by ArmorData.damageReduction
- [ ] Damage is blocked during a dash (DashAttackHandler.IsDashing)
- [ ] HP bar updates in the HUD immediately

### Player Death and Respawn
- [ ] Player dies when HP reaches 0
- [ ] On death, gold drops as a DroppedGold pickup at the player's position
- [ ] GreedMeter is set to 0 after dropping gold
- [ ] Player respawns at the last checkpoint after a 0.5s delay
- [ ] HP is restored to full on respawn
- [ ] isDead flag resets so player can act again

### Player Death in Level 5
- [ ] Level5Manager detects player death and triggers scene restart
- [ ] Death overlay fades in before reload (if configured)
- [ ] Scene reloads fully (fresh enemy state, player at start)

### Enemy Melee (EnemyController)
- [ ] Enemy spawns with HP from EnemyData.maxHP
- [ ] Enemy sprite is set from EnemyData.sprite
- [ ] Enemy takes damage from player melee hits
- [ ] Hurt flash plays on enemy when damaged
- [ ] EnemyShield absorbs one hit before enemy takes damage (if present)
- [ ] Enemy dies when HP reaches 0
- [ ] On death, enemy drops gold (DroppedGold) based on baseGoldDrop and HustleStyle modifier
- [ ] EnemyDeathEffect plays (if present), otherwise GameObject deactivates

### Ranged Enemy
- [ ] Patrols between waypoints when unaware
- [ ] Transitions to Chase state when StealthDetector becomes Suspicious or Alerted
- [ ] Uses A* pathfinding to navigate toward the player
- [ ] Enters Combat state at engagement range and strafes
- [ ] Fires projectiles at the player after aim pause
- [ ] Retreats when player gets too close (minSafeDistance)
- [ ] Returns to Search then Patrol when player escapes line of sight
- [ ] Drops gold on death

### Stealth Guard (Level 5)
- [ ] Patrols waypoints at patrol speed
- [ ] Investigates last-seen position when suspicious
- [ ] Chases player when alerted
- [ ] Alerts nearby guards within radius (no alert through walls)
- [ ] Contact damage applies on collision with player
- [ ] Drops gold on death

### Modi Guards
- [ ] Guards start inactive (not chasing)
- [ ] AlertAllGuards causes all guards to begin chasing the player
- [ ] Guards deal contact damage on collision with player
- [ ] Guards have directional walk and attack animations
- [ ] Guard death plays EnemyDeathEffect or deactivates the GameObject
- [ ] HasLivingGuards returns false when all guards are dead

### Level 5 Boss
- [ ] Boss is inactive until player enters the intro trigger radius
- [ ] Intro cutscene pauses the game and types dialogue lines
- [ ] Pressing E advances through intro dialogue
- [ ] After intro, boss activates and chases the player
- [ ] Boss deals contact damage on collision
- [ ] Boss performs lunge attack when within lunge range
- [ ] Boss takes damage and plays hit animation with flash
- [ ] Boss drops ArmorPickup prefab on death
- [ ] Boss deactivates on death

### Pickpocket Thief
- [ ] Thief chases the player with forced forward movement at start
- [ ] Thief contact deals touch damage and steals all player gold
- [ ] Thief flees along scripted route, then flees player dynamically
- [ ] Catching the thief (second contact) restores 600 gold and shows cornered dialogue
- [ ] Interaction with cornered thief opens post-catch dialogue
- [ ] Threaten path leads to "Who sent you?" choice and sprint lesson
- [ ] "Ignore him" path closes dialogue without sprint lesson
- [ ] Sprint lesson enables shift-to-sprint on the player
- [ ] After sprint lesson, thief can be killed
- [ ] Thief cannot be killed before sprint lesson (hits ignored)
- [ ] Killing thief shows death dialogue, then deactivates

---

## 6. Blackjack Minigame (King Modi)

### Round Setup
- [ ] A new round deals 2 cards to player and 2 to dealer
- [ ] Player cards are visible with their total displayed
- [ ] Dealer's second card is hidden ("?") during the player's turn
- [ ] Opening blackjack (21 on deal) is detected for player, dealer, or both

### Player Actions
- [ ] "Hit" button draws a card and updates the hand display
- [ ] "Stand" button causes the dealer to draw to 17 and resolves the round
- [ ] Buttons are disabled after the round ends

### Outcomes
- [ ] Player bust (over 21) results in DealerWin with "You busted" message
- [ ] Dealer bust results in PlayerWin with "Dealer busted" message
- [ ] Higher player total results in PlayerWin
- [ ] Higher dealer total results in DealerWin
- [ ] Equal totals result in Push
- [ ] Both opening blackjacks result in Push

### Rewards and Penalties
- [ ] Winning grants the red packet and sets player gold to Modi's fortune (1000)
- [ ] Winning alerts all Modi Guards
- [ ] Losing deducts 100 gold from the player
- [ ] Push offers "Play Again" / "Leave" choices (no gold change)
- [ ] Loss dialogue offers "Yes" (play again) / "No" (leave) choices

### Post-Game
- [ ] After winning, dialogue transitions through win lines then closes
- [ ] After losing, dialogue shows loss reason, then Modi's taunt, then choice
- [ ] Dealer's full hand is revealed after the round ends

---

## 7. Inventory and Skills

### Inventory Panel
- [ ] Inventory panel creates the correct number of slots (slotCount)
- [ ] Starting skill prefabs populate into the first N slots
- [ ] Empty slots have no currentItem
- [ ] Skills purchased from the shop appear in the first empty slot

### Skill Drag and Drop
- [ ] Skills can be dragged from inventory slots
- [ ] Skills can be dropped into hotbar slots
- [ ] Skills can be rearranged within the hotbar
- [ ] Dragging respects CanvasGroup alpha for visual feedback

### Hotbar
- [ ] Hotbar creates 10 slots (keys 1-0)
- [ ] Starting skills populate into hotbar slots at launch
- [ ] Pressing a number key (1-9, 0) selects the skill in that slot
- [ ] Selected slot shows an active/highlight visual
- [ ] Pressing the same key again deselects the skill (toggle off)
- [ ] Selecting a new slot deactivates the previous slot highlight
- [ ] Selected skill is equipped on PlayerCombat via EquipSkill

### Skill Activation
- [ ] Right-click activates the equipped skill
- [ ] Skill activation starts the cooldown timer
- [ ] Cooldown overlay appears on the active hotbar slot
- [ ] Skill cannot be activated again until cooldown expires
- [ ] Cooldown reset (chain dash) clears the overlay immediately

### Dash Attack Skill
- [ ] Equipping a DashAttackSkill shows the cursor reticle
- [ ] Activating performs a dash toward the reticle direction
- [ ] Dash deals damage to enemies in the path
- [ ] Successful hit resets cooldown (chain dash ready)
- [ ] Whiff applies the longer whiffCooldown
- [ ] Player is invulnerable during dash frames
- [ ] Unequipping a DashAttackSkill hides the reticle

### Shuriken Barrage Skill
- [ ] Activating spawns the correct number of projectiles (projectileCount)
- [ ] Projectiles spread in a fan pattern (spreadAngle)
- [ ] Projectiles travel in the player's facing direction
- [ ] Each projectile deals the configured damage on hit
- [ ] Projectiles are destroyed after hitting or traveling max distance

### Rock Push Skill
- [ ] With RockPushSkill in hotbar, colliding with a rock pushes it in the player's direction
- [ ] Without RockPushSkill in hotbar, rocks cannot be pushed
- [ ] Pushed rock slides to the next grid-aligned position
- [ ] Player stops and snaps to grid after rock collision

---

## 8. Greed Meter and Gold

### Gold Pickup
- [ ] Walking over a DroppedGold trigger adds gold to GreedMeter
- [ ] DroppedGold object is destroyed after collection
- [ ] DroppedGold despawns after 20 seconds if not collected
- [ ] AddGold ignores amounts of 0 or less

### Gold Display
- [ ] HUD gold text updates when gold changes
- [ ] Greed slider value updates when gold changes
- [ ] Shop money text updates when gold changes during shopping

### Tier Thresholds
- [ ] 0-299 gold: GreedTier.None (gray fill)
- [ ] 300-599 gold: GreedTier.Tier1 (yellow fill)
- [ ] 600-899 gold: GreedTier.Tier2 (orange fill)
- [ ] 900-1199 gold: GreedTier.Tier3 (red fill)
- [ ] 1200+ gold: GreedTier.Tier4 (purple fill)
- [ ] OnTierChanged event fires when crossing a threshold in either direction

### Greed Bonuses (Flat)
- [ ] Tier1+: shield count +1 (verify via GetShieldCount)
- [ ] Tier2+: speed bonus +2, HP bonus +20 (verify via GetBonusSpeed, GetBonusHP)
- [ ] Tier3+: damage bonus +20, additional shield +1 (verify via GetBonusDamage, GetShieldCount)
- [ ] Tier4+: speed bonus +6 total, HP bonus +30 total, shield count 3 total

### Hustle Style Modifiers
- [ ] Combat gold modifier (HustleStyleManager) applies to enemy gold drops
- [ ] NPC gold modifier applies to AddNPCGold calls
- [ ] Shop price modifier is accessible (for future shop price scaling)
- [ ] Bonus gold from Hustle Style is granted once when style is applied
- [ ] Max HP modifier from Hustle Style adjusts effective max HP

### RemoveGold
- [ ] RemoveGold clamps gold to 0 (never negative)
- [ ] OnGoldChanged fires after removal
- [ ] Tier recalculates after gold removal

---

## 9. HUD and UI

### HP Bar
- [ ] HP bar slider initializes to full at game start
- [ ] HP bar decreases when player takes damage
- [ ] HP bar increases when player heals
- [ ] HP text shows current HP as a whole number
- [ ] HP bar max value updates when maxHP modifier changes

### Greed Meter Display
- [ ] Greed slider initializes to starting gold value
- [ ] Greed fill color matches the current tier color
- [ ] Gold text shows the current gold amount as a number

### Menu (Tab)
- [ ] Pressing Tab opens the menu canvas
- [ ] Game pauses when menu opens
- [ ] Pressing Tab again closes the menu canvas
- [ ] Game unpauses when menu closes
- [ ] Inventory panel is visible within the menu
- [ ] Quest log is visible within the menu

### Player Menu UI (Hustle Style)
- [ ] Before choosing a style, label shows "NONE"
- [ ] After choosing a style, label shows the style name in uppercase
- [ ] Style portrait image updates to the chosen style's sprite

### Hustle Style Selection UI
- [ ] Selection cards display for each available Hustle Style
- [ ] Each card shows the style name, description, and sprite
- [ ] Clicking a card selects the style and closes the UI
- [ ] Game unpauses after selection

### Pause System
- [ ] PauseController.SetPause(true) sets Time.timeScale to 0
- [ ] PauseController.SetPause(false) restores Time.timeScale
- [ ] Nested pauses (menu + dialogue) maintain correct depth counting
- [ ] Unpausing with depth > 1 does not restore timeScale prematurely
- [ ] Combat inputs are blocked while paused
- [ ] Hotbar keys are blocked while paused

---

## 10. Player Movement and Physics

### Basic Movement
- [ ] WASD moves the player in four/eight directions
- [ ] Movement speed matches the configured moveSpeed
- [ ] Idle animation plays when not moving
- [ ] Walk animation plays when moving
- [ ] Facing direction updates correctly for all directions

### Sprint
- [ ] Sprint is not available until taught by the pickpocket thief
- [ ] After learning sprint, holding Shift increases speed by sprintSpeedMultiplier
- [ ] Releasing Shift returns to normal speed

### Ice Physics
- [ ] Entering an ice-tagged trigger locks the player into a cardinal slide direction
- [ ] Player cannot change direction while sliding on ice
- [ ] Colliding with a wall on ice stops the player and snaps to grid
- [ ] Exiting ice restores normal movement

### Input Lock
- [ ] SetMovementLocked(true) stops player movement and blocks input
- [ ] SetMovementLocked(false) restores movement
- [ ] SetForcedForwardMovement(true) moves the player upward at reduced speed
- [ ] SetForcedForwardMovement(false) returns control to the player

### Interaction
- [ ] InteractionDetector shows the interaction icon when near an IInteractable
- [ ] Pressing E triggers Interact on the nearest IInteractable
- [ ] Walking away from an NPC hides the interaction icon
- [ ] If a dialogue is active, E continues dialogue instead of interacting with a different NPC
- [ ] CanInteract() returning false prevents interaction icon from showing

---

## 11. Edge Cases and Regression

### Zero Gold
- [ ] Attempting to buy a shop item with 0 gold is rejected gracefully
- [ ] Attempting to gamble with King Modi with 0 gold shows the "no money" dialogue
- [ ] Attempting to gamble with Roaming Merchant with 0 gold shows dismissal
- [ ] RemoveGold when already at 0 does not go negative
- [ ] Player death with 0 gold does not spawn a DroppedGold pickup (gold amount 0)

### Full Inventory
- [ ] Buying a skill when all inventory slots are full is blocked
- [ ] Gold is not deducted on a blocked purchase

### Double Interaction
- [ ] Pressing E rapidly on an NPC does not open two dialogue panels
- [ ] Pressing E during typing completes the line (does not skip two lines)
- [ ] Interacting with an NPC while already in dialogue advances the current dialogue

### Pause During Combat
- [ ] Opening menu (Tab) during combat freezes all enemies and projectiles
- [ ] Closing menu resumes combat exactly where it left off
- [ ] Opening dialogue during combat disables player combat component
- [ ] Closing dialogue re-enables player combat component

### Scene Transition During Dialogue
- [ ] If a scene transition triggers while dialogue is open, no null reference errors occur
- [ ] Dialogue state resets cleanly in the new scene

### Persistent Objects
- [ ] Player prefab survives scene transitions (PersistantPlayer / DontDestroyOnLoad)
- [ ] Camera survives scene transitions (PersistentCamera)
- [ ] QuestManager singleton survives scene transitions
- [ ] HustleStyleManager singleton survives scene transitions
- [ ] ScreenFader singleton survives scene transitions
- [ ] No duplicate persistent objects after multiple scene transitions

### Hustle Style Edge Cases
- [ ] Hustle Style can only be chosen once per playthrough (hasChosenStyle guard)
- [ ] Scene load after choosing a style re-applies effects to the player
- [ ] Character sprite swap map rebuilds correctly on scene load

### Enemy Edge Cases
- [ ] Killing an enemy that is already dead does not double-drop gold
- [ ] EnemyShield blocks the first hit, then subsequent hits deal damage
- [ ] Ranged enemy projectiles are destroyed on collision (not lingering)
- [ ] Modi Guard death effect plays or guard deactivates (no stuck invisible guard)

### Luck System
- [ ] Luck component ShouldNegateDamage returns true at the configured percentage
- [ ] Luck of 0% never negates damage
- [ ] Luck of 100% always negates damage

---

## 12. Audio and Visual Polish

### Animations
- [ ] Player idle, walk, and attack animations play without visual glitches
- [ ] Enemy directional animations (walk, idle, attack) match movement direction
- [ ] Boss sprite animation cycles through frames at the configured frame rate
- [ ] Pickpocket thief directional run animations match flee direction
- [ ] Roaming Merchant spritesheet animation plays correctly (if animateWithSpritesheet)

### VFX
- [ ] Hurt flash (red tint) appears on player and enemies when damaged
- [ ] Dash ghost trail plays during DashAttack
- [ ] HitVFX plays on successful melee strikes
- [ ] Enemy death effect plays (EnemyDeathEffect / AnimatedVFX)
- [ ] Camera shake triggers on appropriate events (CameraShakeManager)

### Screen Fade
- [ ] Fade transitions are smooth (no frame skips or teleport flicker)
- [ ] Fade duration matches the configured fadeDuration (0.5s default)
