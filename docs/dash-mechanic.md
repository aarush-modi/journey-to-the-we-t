# Dash Mechanic

## Overview

The dash is a combat-movement skill that propels the player toward a target location, damaging enemies in the detection radius upon arrival. It features 8-way directional snapping, a cursor reticle targeting system, ghost afterimage trails, and a chain-dash reset mechanic that rewards clean kills.

## Input

| Platform | Binding |
|----------|---------|
| Mouse & Keyboard | Right Mouse Button |
| Gamepad | Right Shoulder |

The dash is bound to the **Skill** action in the Unity Input System (`InputSystem_Actions.inputactions`). It fires through `PlayerCombat.OnSkill()` and activates when the equipped skill is a `DashAttackSkill`.

## Core Parameters

| Parameter | Default | Description |
|-----------|---------|-------------|
| `dashSpeed` | 25 | Movement speed during dash (units/sec) |
| `dashRange` | 7 | Maximum dash distance (units) |
| `baseDashDamage` | 20 | Base damage dealt on arrival |
| `detectionRadius` | 1.5 | Radius for detecting enemies at the cursor |
| `whiffCooldown` | 1.5s | Cooldown applied on miss or wall collision |
| `cooldown` | (ScriptableObject) | Cooldown applied on a successful hit |

Damage is multiplied by the GreedMeter system: `finalDamage = baseDashDamage * greedMultiplier`.

## Execution Flow

1. Player presses the Skill input.
2. `PlayerCombat` checks cooldown, then calls `DashAttackSkill.Activate()`.
3. `DashAttackHandler.ExecuteDash()` snapshots all enemies in the detection radius at the cursor position.
4. The player's facing direction snaps to one of 8 cardinal directions based on the cursor angle.
5. The `Attack` animation trigger fires and the dash state begins.
6. Each `FixedUpdate` during the dash:
   - A ghost afterimage is spawned every 0.03s.
   - The player moves toward the destination via `Rigidbody2D.MovePosition()` using `Vector2.MoveTowards()`.
   - A raycast checks for walls ahead; if one is hit, the dash stops early.
   - Stuck detection triggers a whiff if the player hasn't moved for 2+ frames.
7. On arrival (distance < 0.1 units):
   - All snapshot targets take damage.
   - A random slash VFX prefab spawns at the player's position, rotated to match the dash direction.
   - Camera shake (0.5 magnitude) and hit stop (0.05s time freeze) trigger.
8. Cooldown is applied based on the outcome (see below).

## Cooldown & Chain Dash

| Outcome | Cooldown Applied |
|---------|-----------------|
| Hit target(s), some survive | Normal skill cooldown |
| Hit target(s), **all killed** | **0s** (immediate reset) |
| Whiff (no targets hit) | 1.5s |
| Wall collision stops dash | 1.5s |

**Chain dash** is the key mechanic: if every enemy in the detection radius dies from the dash, the cooldown resets instantly, allowing the player to dash again without waiting. This is tracked via the `chainDashReady` flag in `PlayerCombat`.

## Targeting System (Cursor Reticle)

The `CursorReticle` component provides real-time feedback:

- A circle indicator follows the cursor, constrained to `dashRange` distance from the player.
- The circle changes color based on whether enemies are detected in the radius.
- Range constraint lines show the maximum dash distance.
- Target detection uses `Physics2D.OverlapCircleAll()`.
- The reticle toggles on/off when equipping or unequipping the DashAttackSkill.

Targeting is **snapshot-based**: enemies are locked in at activation and cannot be retargeted mid-dash.

## Visual Effects

### Ghost Afterimage Trail
- **Prefab**: `Dash.prefab` (SpriteRenderer)
- **Spawn rate**: Every 0.03s during the dash
- **Color**: White at 50% alpha `(1, 1, 1, 0.5)`
- **Fade duration**: 0.2s (smooth fade to transparent)
- **Sorting**: "Player" layer, order -1 (renders behind the player)
- **Script**: `DashGhost.cs` handles the fade-out lifecycle

### Trail Renderer
- Attached to the `DashAttackHandler`
- Enabled at dash start, disabled on arrival or whiff

### Slash VFX
- An array of slash prefabs (`slashVFXPrefabs[]`) allows random variation
- Spawned at the player's position on a successful hit
- Rotated to match the dash direction via `atan2`

## Hit Feedback

| Effect | Value |
|--------|-------|
| Camera shake (hit) | 0.5 magnitude |
| Camera shake (whiff) | 0.15 magnitude |
| Hit stop | 0.05s at `Time.timeScale = 0` |

## Wall Collision

A raycast fires in the dash direction each frame. If a wall is detected:
- The dash stops at `wallStopOffset` (0.2 units) before the wall.
- The whiff cooldown (1.5s) is applied.
- No damage is dealt.

## Direction Snapping

The dash direction is snapped to 8-way cardinal directions (N, NE, E, SE, S, SW, W, NW) regardless of the exact cursor angle. This keeps animations clean and predictable. The animator's `LastInputX` and `LastInputY` float parameters are set accordingly and re-applied each frame during the dash to prevent other scripts from overwriting them.

## Notes

- **No invincibility frames**: The player can take damage while dashing.
- **No stamina/resource cost**: The dash is gated by cooldown only.
- **Action locking**: While dashing, normal movement and attacks are locked. The `chainDashReady` flag bypasses this lock for consecutive dashes.

## Files

| File | Role |
|------|------|
| `Scripts/Player/DashAttackHandler.cs` | Core dash execution, state machine, ghost spawning, wall detection |
| `Scripts/Skills/DashAttackSkill.cs` | ScriptableObject with dash configuration values |
| `Scripts/Player/PlayerCombat.cs` | Input handling, cooldown management, action lock integration |
| `Scripts/Player/PlayerController.cs` | Movement integration, action lock checks |
| `Scripts/Player/CursorReticle.cs` | Targeting UI, detection radius visualization |
| `Scripts/VFX/DashGhost.cs` | Afterimage ghost fade effect |
| `Prefabs/Dash.prefab` | Ghost sprite prefab |
| `Prefabs/SlashVFX1.prefab` | Slash effect prefab |
| `Animations/Dash.anim` | Dash animation clip |
| `Animations/DashSprite_0.controller` | Animator controller for dash sprites |
