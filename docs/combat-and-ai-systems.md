# Combat & AI Systems

Changes since the ranged enemy AI merge.

## Dash Attack

`DashAttackHandler` drives the player's dash skill. Data lives in `DashAttackSkill` (ScriptableObject).

Flow: player activates skill -> `CursorReticle` snapshots targets -> player dashes to destination -> `HandleArrival()` deals damage or whiffs.

Key behaviors:
- **Shield interaction**: hitting a shielded enemy triggers knockback (wall-aware), cyan flash, and cooldown reset so the player can dash again immediately
- **Stuck/timeout**: dash has a max duration (1.5x expected travel time). If stuck with targets, force-teleports to destination. If stuck without targets, whiffs with shorter cooldown.
- **Chain dash**: if all targets die from the dash hit, skill cooldown resets
- **Hit stop**: freezes animator + rigidbody for 0.05s (not `Time.timeScale`)
- **Invulnerability**: `PlayerCombat.TakeDamage()` is blocked during dash
- **Dead target filtering**: `CursorReticle` skips `IsDead()` targets
- **Melee suppression**: `MeleeHitbox` skips hits while `IsDashing` is true

## Enemy Shields

`EnemyShield` absorbs hits before HP damage. Both `EnemyController` and `RangedEnemy` check for it in `TakeDamage()`.

- `shieldCount`: hits absorbed before breaking
- Visual icons auto-spawn and center above the enemy
- Dash knockback only triggers when shields are present

## Enemy Boundaries

`EnemyBoundary` is a trigger collider that constrains enemy movement. Enemies find their boundary at startup via `EnemyBoundary.FindContaining(position)`.

`RangedEnemy` uses it to clamp strafe, retreat, and pathfinding destinations.

## Room Containment (Dojo)

Enemies stay contained per-room via:

1. **LOS detection**: `StealthDetector.CanSeePlayer()` raycasts against `obstacleLayers`. Walls and DoorBlockers block it.
2. **DoorBlocker**: invisible collider at doorways on the `DoorBlocker` layer. Player walks through (collision disabled in Physics2D matrix), enemies can't see through.
3. **EnemyBoundary**: constrains enemy movement within room bounds.
4. **Alert LOS**: `StealthGuard.AlertNearbyGuards()` linecasts before alerting, preventing chain-alerts through walls.
5. **Projectile destruction**: `EnemyProjectile` destroys on contact with `obstacleLayers`.

### Inspector setup
- `StealthDetector.obstacleLayers`: Obstacle + DoorBlocker
- `RangedEnemy.obstacleLayers`: Obstacle + DoorBlocker
- `EnemyProjectile` prefab `obstacleLayers`: Obstacle + DoorBlocker
- Physics2D collision matrix: Player / DoorBlocker unchecked
- Player GameObject layer: Player

## Quest / Objective Tracking

`QuestManager` is a singleton that tracks active and completed quests via `QuestData` ScriptableObjects.

`ObjectiveTracker` + `EnemyGroupCondition` bridge combat to quests:
- `EnemyGroupCondition` polls its child `IDamageable` components each frame
- When all children are dead, it calls `ObjectiveTracker.SetComplete()`
- NPCs (e.g. `DojoMasterNPC`) reference the `ObjectiveTracker` to gate dialogue/rewards

### Dojo quest flow
1. Player talks to DojoMaster -> intro dialogue -> quest starts via `QuestManager.StartQuest()`
2. Player clears all enemies under `DojoEnemies` -> `EnemyGroupCondition` sets `ObjectiveTracker.IsComplete = true`
3. Player returns to DojoMaster -> complete dialogue -> package reward + `QuestManager.CompleteQuest()`

## Projectile Fix

`EnemyProjectile.OnTriggerEnter2D()` uses `IsChildOf()` instead of `transform.root` to skip owner. Fixes projectiles passing through owner children.
