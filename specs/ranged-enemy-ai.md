# Plan: Ranged Enemy AI System

## Task Description
Implement a complete enemy AI system for a ranged enemy that patrols, detects the player, chases with proper pathfinding (navigating around corners), and engages in ranged combat with kiting behavior (strafe, retreat, shoot-move-shoot). The system must handle line-of-sight detection, chase persistence (not giving up instantly when the player breaks LOS), and obstacle-aware navigation.

## Objective
When complete, a ranged enemy will:
1. Patrol between waypoints when idle
2. Detect the player via line-of-sight (reusing StealthDetector)
3. Chase the player using A* pathfinding that navigates around walls and corners
4. Engage in ranged combat: maintain preferred distance, strafe left/right, retreat when player closes in, firing projectiles in a shoot-move-shoot rhythm
5. Search the player's last known position before returning to patrol when LOS is lost
6. Feel challenging and dynamic to play against alongside the player's dash mechanic

## Problem Statement
Enemies are currently "stale" - they either stand still or do simple direct-line chases that get stuck on walls. There is no pathfinding, no ranged combat, and no intelligent engagement behavior. The player's new dash mechanic needs enemies that create pressure and require skillful play.

## Solution Approach

### Industry Standard: Finite State Machine + Grid A* + Steering Behaviors

The proven approach for 2D action game enemy AI is a **hybrid system**:

1. **FSM (Finite State Machine)** for high-level decision making - the project already uses this pattern (enum + switch in StealthGuard, ModiGuard, PickpocketThief). We keep it consistent.

2. **Grid-based A* pathfinding** for navigation - this is the standard for 2D tile/grid games. It handles navigating around walls, through doorways, and around corners. We build a walkability grid by sampling Physics2D colliders, then run A* to find paths.

3. **Steering behaviors** for combat movement - once in weapon range, the enemy uses reactive movement: maintain preferred distance, strafe perpendicular to the player, retreat if too close. This creates the dynamic kiting feel.

4. **StealthDetector reuse** for awareness - already built, handles LOS raycasts, FOV cones, meter-based detection with gradual drain (provides the "don't give up instantly" behavior via `drainDelay` + `timeToCalm`).

### State Machine Design

```
                    ┌──────────┐
                    │  PATROL  │◄─── timeout ───┐
                    └────┬─────┘                │
                         │ detect player        │
                         ▼                      │
                    ┌──────────┐           ┌────┴─────┐
        ┌──────────│  CHASE   │──lost LOS─►│  SEARCH  │
        │          └────┬─────┘           └──────────┘
        │               │ in weapon range       ▲
        │               ▼                       │
        │          ┌──────────┐                 │
        │          │  COMBAT  │──lost LOS long──┘
        │          └────┬─────┘
        │               │ player too close
        │               ▼
        │          ┌──────────┐
        └──────────│ RETREAT  │──safe distance──► COMBAT
                   └──────────┘
        
  Any state + take damage → force alert → CHASE
```

**States:**
- **Patrol**: Walk waypoints at patrol speed. StealthDetector handles detection.
- **Chase**: A* pathfind toward player. Continuously update path. Transition to Combat when in weapon range.
- **Combat**: Maintain preferred distance (e.g. 6-8 units). Alternate between shooting (brief pause to fire) and repositioning (strafe perpendicular or retreat). Uses raycasts for obstacle avoidance during strafing.
- **Retreat**: Triggered when player closes to minimum safe distance. Move away from player with obstacle avoidance. Return to Combat when at preferred distance.
- **Search**: Go to last-known player position via A*. Look around briefly (rotate/pause). Return to Patrol after timeout.

### Pathfinding Design

**Grid-based A* approach:**
- A singleton `Pathfinding2D` manager generates a grid of walkable/blocked nodes by sampling the world with `Physics2D.OverlapBox` against obstacle layers
- Grid cell size matches the game's tile size (1 unit based on PlayerController grid snapping)
- Path requests return a list of `Vector2` waypoints
- Paths are smoothed by removing redundant collinear points
- Grid can be rebaked on demand (or baked once at scene start for static maps)
- Enemies request new paths periodically (every 0.3-0.5s) to handle player movement

**Why A* over simpler approaches:**
- Raycast steering (like PickpocketThief) gets stuck in concave obstacles and L-shaped corridors
- Direct chase (like StealthGuard/ModiGuard) walks into walls at corners
- NavMesh is designed for 3D and requires awkward workarounds for 2D
- Grid A* is lightweight, no external dependencies, perfect for tile-based 2D maps

### Combat Behavior Design

**Shoot-Move-Shoot Rhythm:**
```
COMBAT state loop:
  1. Pick strafe direction (left or right of player, check for walls)
  2. Move to strafe position for moveDuration (0.5-1s)
  3. Stop, aim at player, fire projectile
  4. Brief shoot cooldown (0.3-0.5s)
  5. Repeat with possibly new strafe direction
  
  If player distance < minSafeDistance → switch to RETREAT
  If player distance > maxEngageDistance → switch to CHASE
  If lost LOS for > lostSightTimeout → switch to SEARCH
```

**Retreat behavior:**
- Move directly away from player
- Use raycast probes for obstacle avoidance (proven pattern from PickpocketThief)
- Once at preferred distance, resume Combat

### Projectile Design

Adapt the existing `ShurikenProjectile` pattern for enemy use:
- New `EnemyProjectile` component (or make ShurikenProjectile generic by checking tags)
- Fires toward player's position (with optional slight leading)
- Damages player via `IDamageable.TakeDamage()`
- Visual: uses the user's existing projectile sprites
- Destroyed on hit or after lifetime expires

## Relevant Files

### Existing Files (reference/modify)
- `Journey To The West/Assets/Scripts/Enemy/StealthDetector.cs` - Reuse as-is for player detection. Already handles LOS, FOV, meter-based awareness with drain delay. Attach to ranged enemy.
- `Journey To The West/Assets/Scripts/Enemy/StealthGuard.cs` - Reference for patrol waypoint pattern, hurt flash, gold drop, death handling. The ranged enemy follows this structure.
- `Journey To The West/Assets/Scripts/Enemy/EnemyController.cs` - Reference for basic enemy patterns (EnemyData usage, death effects).
- `Journey To The West/Assets/Scripts/Enemy/EnemyData.cs` - Extend with ranged-specific fields OR create separate RangedEnemyData ScriptableObject.
- `Journey To The West/Assets/Scripts/Skills/ShurikenProjectile.cs` - Reference for projectile pattern (Initialize, velocity, trigger detection, IDamageable).
- `Journey To The West/Assets/Scripts/Interfaces/IDamageable.cs` - Interface the ranged enemy must implement.
- `Journey To The West/Assets/Scripts/Enemy/PickpocketThief.cs` - Reference for raycast-based obstacle avoidance (7-probe pattern). Useful for combat strafing.
- `Journey To The West/Assets/Scripts/Player/PlayerCombat.cs` - Target for enemy projectile damage. Verify TakeDamage works from enemy projectiles.
- `Journey To The West/Assets/Scripts/Enemy/EnemyDeathEffect.cs` - Reuse for ranged enemy death VFX.
- `Journey To The West/Assets/Scripts/VFX/HitVFX.cs` - Reference for hit feedback effects.

### New Files
- `Journey To The West/Assets/Scripts/Enemy/AI/Pathfinding2D.cs` - Singleton A* pathfinding manager. Builds walkability grid from Physics2D, runs A* algorithm, returns smoothed paths.
- `Journey To The West/Assets/Scripts/Enemy/AI/PathNode.cs` - Grid node class for A* (position, walkable, gCost, hCost, parent).
- `Journey To The West/Assets/Scripts/Enemy/RangedEnemy.cs` - Main ranged enemy controller with FSM (Patrol/Chase/Combat/Retreat/Search states), pathfinding integration, and combat logic.
- `Journey To The West/Assets/Scripts/Enemy/EnemyProjectile.cs` - Enemy-fired projectile. Similar to ShurikenProjectile but targets player. Initialize with direction, speed, damage.

## Implementation Phases

### Phase 1: Foundation - Pathfinding System
Build the A* pathfinding infrastructure that the ranged enemy (and all future enemies) will use.

**Pathfinding2D.cs** - Singleton manager:
```csharp
// Core API:
public class Pathfinding2D : MonoBehaviour
{
    public static Pathfinding2D Instance { get; private set; }
    
    [SerializeField] private LayerMask obstacleLayers;
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private Vector2Int gridSize = new Vector2Int(50, 50);
    [SerializeField] private Vector2 gridOrigin; // bottom-left corner
    
    public List<Vector2> FindPath(Vector2 start, Vector2 end);
    public void BakeGrid(); // Call on scene load
    public bool IsWalkable(Vector2 worldPos);
}
```

**PathNode.cs** - Lightweight node:
```csharp
public class PathNode
{
    public Vector2Int GridPosition;
    public Vector2 WorldPosition;
    public bool Walkable;
    public int GCost, HCost;
    public PathNode Parent;
    public int FCost => GCost + HCost;
}
```

**A* Algorithm Details:**
- Grid baking: iterate cells, `Physics2D.OverlapBox(cellCenter, cellSize, 0, obstacleLayers)` to check walkability
- 8-directional neighbors (diagonal movement allowed, with diagonal cost = 14, cardinal = 10)
- Diagonal movement blocked if either adjacent cardinal cell is blocked (prevents corner cutting)
- Manhattan or Octile distance heuristic
- Path smoothing: remove waypoints where direct LOS exists between non-adjacent points
- Cache grid on scene load; no need for runtime rebaking on static maps

### Phase 2: Core Implementation - Ranged Enemy
Build the ranged enemy controller with full FSM and combat behavior.

**RangedEnemy.cs** - Main controller:
```csharp
[RequireComponent(typeof(StealthDetector))]
[RequireComponent(typeof(Rigidbody2D))]
public class RangedEnemy : MonoBehaviour, IDamageable
{
    private enum EnemyState { Patrol, Chase, Combat, Retreat, Search }
    
    [Header("Stats")]
    float maxHP, contactDamage, contactCooldown;
    
    [Header("Movement")]
    float patrolSpeed, chaseSpeed, combatMoveSpeed, retreatSpeed;
    
    [Header("Patrol")]
    Transform[] waypoints; float waypointReachDist, waypointPauseDuration;
    
    [Header("Combat - Ranges")]
    float preferredDistance = 7f;    // ideal shooting distance
    float minSafeDistance = 3f;      // too close, retreat
    float maxEngageDistance = 12f;   // too far, chase
    
    [Header("Combat - Shooting")]
    GameObject projectilePrefab;
    float projectileSpeed, projectileDamage;
    float shootCooldown = 1.2f;     // time between shots
    float aimPauseDuration = 0.3f;  // pause before firing
    
    [Header("Combat - Movement")]
    float strafeDuration = 0.8f;    // how long to strafe before next shot
    float strafeChangeChance = 0.3f;// chance to flip strafe direction
    
    [Header("Search")]
    float searchDuration = 4f;      // how long to search at last known pos
    float lostSightChaseTime = 2f;  // keep chasing after losing LOS
    
    [Header("Pathfinding")]
    float pathUpdateInterval = 0.4f;// how often to recalculate path
    
    [Header("Gold / Death")]
    GameObject droppedGoldPrefab; int baseGoldDrop;
    
    [Header("Hurt Feedback")]
    float flashDuration; Color flashColor;
}
```

**Combat Strafing Logic:**
```csharp
// In Combat state:
Vector2 toPlayer = (playerPos - myPos).normalized;
Vector2 strafeDir = Vector2.Perpendicular(toPlayer) * strafeSign; // +1 or -1

// Raycast check: if wall in strafe direction, flip
if (Physics2D.Raycast(myPos, strafeDir, 2f, obstacleLayers))
    strafeSign *= -1;

// Also drift to preferred distance
float distToPlayer = Vector2.Distance(myPos, playerPos);
Vector2 distanceAdjust = Vector2.zero;
if (distToPlayer < preferredDistance - 1f)
    distanceAdjust = -toPlayer * 0.5f; // drift away
else if (distToPlayer > preferredDistance + 1f)
    distanceAdjust = toPlayer * 0.5f;  // drift closer

rb.linearVelocity = (strafeDir + distanceAdjust).normalized * combatMoveSpeed;
```

**Shoot-Move Cycle:**
```csharp
// Timer-based alternation:
// shootTimer counts down → when 0, fire projectile, reset to shootCooldown
// During aimPauseDuration before shot, enemy slows/stops to "aim"
// Between shots, enemy strafes freely
```

**EnemyProjectile.cs:**
```csharp
// Nearly identical to ShurikenProjectile but:
// - Targets "Player" tag specifically
// - Can use different sprite/visual
// - Same Initialize(owner, direction, speed, damage) API
// - Same trigger-based hit detection via IDamageable
```

### Phase 3: Integration & Polish
- Set up Animator controller for ranged enemy (4-directional movement + shoot animation)
- Wire up projectile prefab with enemy's projectile sprite
- Create RangedEnemy prefab with all components (RangedEnemy, StealthDetector, Rigidbody2D, Animator, Collider2D)
- Place in scene with patrol waypoints
- Tune values: detection ranges, combat distances, shoot cooldowns, movement speeds
- Add Gizmo debug visualization (patrol path, engagement ranges, current path)
- Test against dash mechanic to ensure challenging but fair gameplay

## Team Orchestration

- You operate as the team lead and orchestrate the team to execute the plan.
- You're responsible for deploying the right team members with the right context to execute the plan.
- IMPORTANT: You NEVER operate directly on the codebase. You use `Task` and `Task*` tools to deploy team members to do the building, validating, testing, deploying, and other tasks.
  - This is critical. Your job is to act as a high level director of the team, not a builder.
  - Your role is to validate all work is going well and make sure the team is on track to complete the plan.
  - You'll orchestrate this by using the Task* Tools to manage coordination between the team members.
  - Communication is paramount. You'll use the Task* Tools to communicate with the team members and ensure they're on track to complete the plan.
- Take note of the session id of each team member. This is how you'll reference them.

### Team Members

- Builder
  - Name: pathfinding-builder
  - Role: Implement the A* pathfinding system (Pathfinding2D.cs, PathNode.cs)
  - Agent Type: general-purpose
  - Resume: true

- Builder
  - Name: ranged-enemy-builder
  - Role: Implement RangedEnemy.cs controller with full FSM and combat logic
  - Agent Type: general-purpose
  - Resume: true

- Builder
  - Name: projectile-builder
  - Role: Implement EnemyProjectile.cs and wire up projectile prefab setup
  - Agent Type: general-purpose
  - Resume: true

- Validator
  - Name: validator
  - Role: Validate all scripts compile, patterns are consistent, and integration is correct
  - Agent Type: validator
  - Resume: false

## Step by Step Tasks

- IMPORTANT: Execute every step in order, top to bottom. Each task maps directly to a `TaskCreate` call.
- Before you start, run `TaskCreate` to create the initial task list that all team members can see and execute.

### 1. Create Feature Branch
- **Task ID**: create-branch
- **Depends On**: none
- **Assigned To**: pathfinding-builder
- **Agent Type**: general-purpose
- **Parallel**: false
- Create and switch to branch `feature/ranged-enemy-ai` from `main`

### 2. Implement A* Pathfinding System
- **Task ID**: pathfinding-system
- **Depends On**: create-branch
- **Assigned To**: pathfinding-builder
- **Agent Type**: general-purpose
- **Parallel**: false
- Create `Journey To The West/Assets/Scripts/Enemy/AI/` directory
- Create `PathNode.cs` with grid position, world position, walkable flag, G/H/F costs, parent reference
- Create `Pathfinding2D.cs` singleton MonoBehaviour:
  - Serialized fields: `obstacleLayers` (LayerMask), `cellSize` (float, default 1), `gridSize` (Vector2Int), `gridOrigin` (Vector2)
  - `BakeGrid()`: iterate all cells, use `Physics2D.OverlapBox` to determine walkability, store in 2D array
  - `FindPath(Vector2 start, Vector2 end)`: standard A* with open/closed lists, 8-directional neighbors, diagonal blocked if adjacent cardinals blocked, returns `List<Vector2>` of world positions
  - `IsWalkable(Vector2 worldPos)`: point query
  - Path smoothing: post-process to remove redundant collinear waypoints using LOS raycasts between non-adjacent points
  - `Awake()` sets singleton instance, calls `BakeGrid()`
  - Add `OnDrawGizmosSelected()` to visualize grid (walkable=green, blocked=red)
- Follow existing code conventions: `using UnityEngine;`, `[SerializeField]` for inspector fields, `Rigidbody2D` velocity-based movement patterns
- Use `Physics2D` for all spatial queries (consistent with rest of project)

### 3. Implement Enemy Projectile
- **Task ID**: enemy-projectile
- **Depends On**: create-branch
- **Assigned To**: projectile-builder
- **Agent Type**: general-purpose
- **Parallel**: true (can run alongside pathfinding)
- Create `Journey To The West/Assets/Scripts/Enemy/EnemyProjectile.cs`
- Model after `ShurikenProjectile.cs` (same file at `Scripts/Skills/ShurikenProjectile.cs`):
  - `[RequireComponent(typeof(Collider2D))]` and `[RequireComponent(typeof(Rigidbody2D))]`
  - `Initialize(GameObject owner, Vector2 direction, float speed, float damage)` - sets velocity and stores damage
  - `OnTriggerEnter2D` - skip owner, find `IDamageable` on collider, call `TakeDamage`, destroy self
  - Lifetime timer (default 3s) - destroy if no hit
  - Also destroy on hitting obstacle layer (check if collider is NOT on "Player" tag and NOT damageable - it's a wall)
- Key difference from ShurikenProjectile: this one is fired BY enemies AT the player, so the owner check skips the enemy that fired it

### 4. Implement Ranged Enemy Controller
- **Task ID**: ranged-enemy-controller
- **Depends On**: pathfinding-system, enemy-projectile
- **Assigned To**: ranged-enemy-builder
- **Agent Type**: general-purpose
- **Parallel**: false
- Create `Journey To The West/Assets/Scripts/Enemy/RangedEnemy.cs`
- Implement `IDamageable` interface (TakeDamage, Die, IsDead)
- `[RequireComponent(typeof(StealthDetector))]` and `[RequireComponent(typeof(Rigidbody2D))]`
- **FSM with enum** `EnemyState { Patrol, Chase, Combat, Retreat, Search }`:

  **Patrol State:**
  - Reuse waypoint pattern from StealthGuard (Transform[] waypoints, waypointIndex, WaypointPause coroutine)
  - Move at patrolSpeed between waypoints
  - StealthDetector.OnStateChanged triggers transition: Suspicious→Chase, Alerted→Chase
  
  **Chase State:**
  - Request A* path to player position via `Pathfinding2D.Instance.FindPath()`
  - Follow path waypoints using Rigidbody2D velocity at chaseSpeed
  - Re-request path every `pathUpdateInterval` (0.4s)
  - Continuously update `lastSeenPosition` while player is visible
  - Transition to Combat when `distance < maxEngageDistance` AND has LOS to player
  - Transition to Search if StealthDetector goes Unaware (lost LOS long enough)
  - On taking damage: `detector.ForceAlert()` and stay in Chase
  
  **Combat State:**
  - **Shoot-Move cycle** using timers:
    - `strafeTimer` counts down from `strafeDuration` (0.8s)
    - During strafe: move perpendicular to player direction, also adjust distance toward `preferredDistance`
    - When strafeTimer expires: brief aim pause (`aimPauseDuration` 0.3s), then fire projectile
    - Reset strafeTimer, possibly flip strafe direction (30% chance)
  - **Strafe movement:**
    - `Vector2 toPlayer = (playerPos - myPos).normalized`
    - `Vector2 strafeDir = Vector2.Perpendicular(toPlayer) * strafeSign`
    - Raycast in strafe direction; if wall detected, flip `strafeSign`
    - Blend with distance-maintaining vector
  - **Shooting:**
    - Instantiate `EnemyProjectile` prefab at enemy position
    - Direction = toward player current position
    - Call `projectile.Initialize(gameObject, dirToPlayer, projectileSpeed, projectileDamage)`
  - Transition to Retreat if `distance < minSafeDistance`
  - Transition to Chase if `distance > maxEngageDistance` or lost LOS
  - Track `lostSightTimer`: if no LOS for `lostSightChaseTime` (2s), transition to Search
  
  **Retreat State:**
  - Move directly away from player at retreatSpeed
  - Use raycast obstacle avoidance (3-5 probes) to avoid backing into walls - reference PickpocketThief's probe pattern
  - Transition back to Combat when `distance >= preferredDistance`
  - Still fires if shootCooldown is ready (shoot while retreating)
  
  **Search State:**
  - A* pathfind to `lastSeenPosition`
  - On arrival, pause for `searchDuration` (4s)
  - If StealthDetector re-detects player during search → Chase
  - After timeout → Patrol (find nearest waypoint to resume)

- **Damage/Death** (copy pattern from StealthGuard):
  - `TakeDamage`: decrement HP, HurtFlash coroutine, `detector.ForceAlert()`, die if HP <= 0
  - `Die`: set isDead, zero velocity, DropGold, SetActive(false) or trigger EnemyDeathEffect
  - `DropGold`: same pattern as StealthGuard with HustleStyle modifier

- **Contact Damage** (same as StealthGuard):
  - `OnCollisionEnter2D`/`OnCollisionStay2D` → damage player on cooldown

- **Animator Integration:**
  - Set animator parameters based on movement direction (same pattern as ModiGuard)
  - Trigger shoot animation when firing
  - 4-directional movement animations

- **Gizmo Debug:**
  - Draw patrol waypoints (cyan, like StealthGuard)
  - Draw engagement ranges (preferredDistance=yellow, minSafeDistance=red, maxEngageDistance=blue)
  - Draw current A* path (green line)

### 5. Validate All Scripts
- **Task ID**: validate-all
- **Depends On**: ranged-enemy-controller
- **Assigned To**: validator
- **Agent Type**: validator
- **Parallel**: false
- Verify all new .cs files have correct syntax and will compile
- Verify `RangedEnemy` implements all `IDamageable` methods correctly
- Verify `Pathfinding2D` singleton pattern matches project conventions
- Verify `EnemyProjectile` follows same patterns as `ShurikenProjectile`
- Verify no circular dependencies between new scripts
- Verify all `[SerializeField]` fields have sensible defaults
- Verify `StealthDetector` integration (event subscription/unsubscription in Start/OnDestroy)
- Check that existing files are not broken by changes
- Verify code style consistency with existing Enemy scripts (naming, structure, comments)

## Acceptance Criteria
- [ ] `Pathfinding2D.cs` and `PathNode.cs` exist and implement grid-based A* pathfinding
- [ ] `Pathfinding2D.FindPath()` returns a valid path around obstacles
- [ ] `EnemyProjectile.cs` exists and damages the player on hit via `IDamageable`
- [ ] `RangedEnemy.cs` exists with 5 states: Patrol, Chase, Combat, Retreat, Search
- [ ] Patrol: enemy moves between waypoints when unaware
- [ ] Chase: enemy pathfinds around corners to reach player using A*
- [ ] Combat: enemy maintains distance, strafes, and fires projectiles in shoot-move-shoot pattern
- [ ] Retreat: enemy backs away when player gets too close
- [ ] Search: enemy investigates last known position before returning to patrol
- [ ] Enemy uses StealthDetector for LOS-based detection with gradual awareness
- [ ] Enemy doesn't instantly give up when player breaks line of sight (drain delay + search state)
- [ ] Enemy implements IDamageable (TakeDamage, Die, IsDead)
- [ ] Enemy drops gold on death with HustleStyle modifier
- [ ] All scripts compile without errors
- [ ] Code follows existing project conventions (Physics2D, Rigidbody2D velocity, SerializeField, etc.)

## Validation Commands
Execute these commands to validate the task is complete:

```bash
# Verify all new files exist
ls "Journey To The West/Assets/Scripts/Enemy/AI/Pathfinding2D.cs"
ls "Journey To The West/Assets/Scripts/Enemy/AI/PathNode.cs"
ls "Journey To The West/Assets/Scripts/Enemy/RangedEnemy.cs"
ls "Journey To The West/Assets/Scripts/Enemy/EnemyProjectile.cs"

# Check for compilation errors (basic syntax check)
grep -c "class Pathfinding2D" "Journey To The West/Assets/Scripts/Enemy/AI/Pathfinding2D.cs"
grep -c "class PathNode" "Journey To The West/Assets/Scripts/Enemy/AI/PathNode.cs"
grep -c "class RangedEnemy" "Journey To The West/Assets/Scripts/Enemy/RangedEnemy.cs"
grep -c "class EnemyProjectile" "Journey To The West/Assets/Scripts/Enemy/EnemyProjectile.cs"

# Verify IDamageable implementation
grep "IDamageable" "Journey To The West/Assets/Scripts/Enemy/RangedEnemy.cs"
grep "TakeDamage" "Journey To The West/Assets/Scripts/Enemy/RangedEnemy.cs"
grep "Die()" "Journey To The West/Assets/Scripts/Enemy/RangedEnemy.cs"
grep "IsDead()" "Journey To The West/Assets/Scripts/Enemy/RangedEnemy.cs"

# Verify StealthDetector integration
grep "StealthDetector" "Journey To The West/Assets/Scripts/Enemy/RangedEnemy.cs"

# Verify pathfinding integration
grep "Pathfinding2D" "Journey To The West/Assets/Scripts/Enemy/RangedEnemy.cs"

# Verify all 5 states exist
grep -c "Patrol\|Chase\|Combat\|Retreat\|Search" "Journey To The West/Assets/Scripts/Enemy/RangedEnemy.cs"
```

## Notes
- **No external dependencies**: Everything is implemented with Unity's built-in APIs (Physics2D, Rigidbody2D). No asset store packages needed.
- **Pathfinding grid must be baked**: The Pathfinding2D singleton bakes on Awake(). For the current static maps this is sufficient. If maps become dynamic, add a `RebakeGrid()` call.
- **StealthDetector tuning**: The existing drainDelay (0.5s) and timeToCalm (4s) provide ~4.5s of chase persistence after losing LOS. Combined with the Search state, this creates natural "don't give up at corners" behavior. These values can be tuned per-enemy via the inspector.
- **Animator setup is manual**: The plan creates the script with animator parameter hooks, but the actual Animator Controller and animation clips must be configured in the Unity Editor using the user's existing enemy sprites and attack animations.
- **Prefab assembly is manual**: After scripts are written, the user needs to create the RangedEnemy prefab in Unity Editor: add RangedEnemy + StealthDetector + Rigidbody2D + Collider2D + Animator + SpriteRenderer, assign the projectile prefab, set waypoints in scene.
- **Balance values are starting points**: All combat distances, cooldowns, and speeds are configurable via `[SerializeField]` and should be tuned through playtesting against the dash mechanic.
- **Future extensibility**: The Pathfinding2D system is reusable for any future enemy types. The FSM pattern can be copied/adapted for melee enemies, boss enemies, etc.
