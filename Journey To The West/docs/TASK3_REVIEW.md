# Task 3 Review

This document summarizes the current Task 3 state of the repository based on:

- Task 3 ticket names visible in Git history
- current code on `main`
- branch-only Task 3 work for the shuriken skill and hustle style systems

Note: the assignment PDF was available locally, but this environment did not have a working PDF text extractor. This write-up is therefore anchored to the repo's Task 3 tickets and implemented code rather than a direct quote-by-quote PDF comparison.

## High-Level Status

### Present on `main`

- `T3-04` Dropped gold pickup
- `T3-05` Player combat, HP, melee attack, death
- `T3-06` Player inventory
- `T3-07` Quest manager and quest log UI
- `T3-09` HUD manager and HUD updates
- `T3-10` NPC base, Village Elder NPC, Level 1 package data
- merchant shop waypoint / map transition scene work

### Implemented, but not merged into `main`

- `T3-11` Shuriken skill branch: `codex/T3-11-shuriken-skill`
- `T3-12` Hustle style branch: `codex/T3-12-hustle-style`
- duplicate push branch for hustle style: `hustlefeat`

## Review Findings

### 1. Task 3 is not fully represented on `main`

The biggest delivery risk is that the Task 3 systems added after `T3-10` are not on `main`. The shuriken skill and hustle style work both exist in separate branches, but the primary integration branch still stops short of those systems.

Impact:

- if Task 3 grading or demo expectations assume the default branch contains the final deliverable, the repo appears incomplete
- reviewers checking only `main` will miss the skill and hustle-style work entirely

Relevant branches:

- `codex/T3-11-shuriken-skill`
- `codex/T3-12-hustle-style`
- `hustlefeat`

### 2. Hustle style testing is not obvious in the shipped scene setup

The hustle style implementation includes a debug selector script, but practical validation in `Main` is still fragile because the gameplay loop does not expose the modifiers clearly. In testing, pressing `1`, `2`, or `3` in `Main` did not produce visible behavior without extra scene-level verification.

Impact:

- the code may exist, but a TA or teammate can conclude it does not work because there is no obvious scene feedback
- the branch lacks a reliable "demo path" for style selection

Current implementation paths:

- [`Assets/Scripts/Dev/HustleStyleDebug.cs`](C:\Users\camer\Downloads\journey-to-the-we-t\Journey To The West\Assets\Scripts\Dev\HustleStyleDebug.cs)
- [`Assets/Scripts/Player/HustleStyleManager.cs`](C:\Users\camer\Downloads\journey-to-the-we-t\Journey To The West\Assets\Scripts\Player\HustleStyleManager.cs)
- [`Assets/Resources/HustleStyles`](C:\Users\camer\Downloads\journey-to-the-we-t\Journey To The West\Assets\Resources\HustleStyles)

### 3. Local compile verification is currently blocked by stale generated project files

A `dotnet build` check fails because the generated Unity `.csproj` still references shuriken source files on a branch where those files do not exist. That is a tooling issue rather than direct evidence of a code error, but it means command-line verification is not trustworthy until Unity regenerates project files.

Impact:

- command-line compile status cannot be used as a clean gate right now
- the repo still needs an in-Unity validation pass after branch switching

## Implemented Systems by Area

### Combat and Gold Loop

- [`Assets/Scripts/Player/PlayerCombat.cs`](C:\Users\camer\Downloads\journey-to-the-we-t\Journey To The West\Assets\Scripts\Player\PlayerCombat.cs)
  - HP tracking
  - melee attack hitbox activation
  - death and respawn
- [`Assets/Scripts/Player/GreedMeter.cs`](C:\Users\camer\Downloads\journey-to-the-we-t\Journey To The West\Assets\Scripts\Player\GreedMeter.cs)
  - gold total
  - greed tiers
  - combat and NPC gold source hooks on the hustle-style branch
- [`Assets/Scripts/Global/DroppedGold.cs`](C:\Users\camer\Downloads\journey-to-the-we-t\Journey To The West\Assets\Scripts\Global\DroppedGold.cs)
  - world pickup behavior

### Inventory, Quests, and NPCs

- [`Assets/Scripts/Player/PlayerInventory.cs`](C:\Users\camer\Downloads\journey-to-the-we-t\Journey To The West\Assets\Scripts\Player\PlayerInventory.cs)
- [`Assets/Scripts/Quest/QuestManager.cs`](C:\Users\camer\Downloads\journey-to-the-we-t\Journey To The West\Assets\Scripts\Quest\QuestManager.cs)
- [`Assets/Scripts/Interfaces/NPCBase.cs`](C:\Users\camer\Downloads\journey-to-the-we-t\Journey To The West\Assets\Scripts\Interfaces\NPCBase.cs)
- [`Assets/Scripts/NPC/VillageElderNPC.cs`](C:\Users\camer\Downloads\journey-to-the-we-t\Journey To The West\Assets\Scripts\NPC\VillageElderNPC.cs)

### UI

- [`Assets/Scripts/UI/HUDManager.cs`](C:\Users\camer\Downloads\journey-to-the-we-t\Journey To The West\Assets\Scripts\UI\HUDManager.cs)
- [`Assets/Scripts/UI/QuestLogUI.cs`](C:\Users\camer\Downloads\journey-to-the-we-t\Journey To The West\Assets\Scripts\UI\QuestLogUI.cs)

### Branch-Only Task 3 Additions

#### T3-11 Shuriken Skill

Branch:

- `codex/T3-11-shuriken-skill`

Key files:

- [`Assets/Scripts/Skills/ShurikenBarrageSkill.cs`](C:\Users\camer\Downloads\journey-to-the-we-t\Journey To The West\Assets\Scripts\Skills\ShurikenBarrageSkill.cs)
- [`Assets/Scripts/Skills/ShurikenProjectile.cs`](C:\Users\camer\Downloads\journey-to-the-we-t\Journey To The West\Assets\Scripts\Skills\ShurikenProjectile.cs)
- [`Assets/Data/ShurikenBarrageSkill.asset`](C:\Users\camer\Downloads\journey-to-the-we-t\Journey To The West\Assets\Data\ShurikenBarrageSkill.asset)
- [`Assets/Prefabs/ShurikenProjectile.prefab`](C:\Users\camer\Downloads\journey-to-the-we-t\Journey To The West\Assets\Prefabs\ShurikenProjectile.prefab)

What it adds:

- abstract skill data pattern
- data-driven shuriken barrage skill
- projectile prefab and hit logic

#### T3-12 Hustle Style

Branches:

- `codex/T3-12-hustle-style`
- `hustlefeat`

Key files:

- [`Assets/Scripts/Player/HustleStyleData.cs`](C:\Users\camer\Downloads\journey-to-the-we-t\Journey To The West\Assets\Scripts\Player\HustleStyleData.cs)
- [`Assets/Scripts/Player/HustleStyleManager.cs`](C:\Users\camer\Downloads\journey-to-the-we-t\Journey To The West\Assets\Scripts\Player\HustleStyleManager.cs)
- [`Assets/Scripts/Dev/HustleStyleDebug.cs`](C:\Users\camer\Downloads\journey-to-the-we-t\Journey To The West\Assets\Scripts\Dev\HustleStyleDebug.cs)
- [`Assets/Resources/HustleStyles`](C:\Users\camer\Downloads\journey-to-the-we-t\Journey To The West\Assets\Resources\HustleStyles)

What it adds:

- style data assets
- runtime style manager
- HP modifier application
- source-typed gold modifier hooks

## Recommended Next Steps

1. Decide which branch is the Task 3 submission branch.
2. Merge or cherry-pick the `T3-11` and `T3-12` work into that branch if those tickets are required for submission.
3. Open the project in Unity and force regeneration of project files before using command-line compile results.
4. Create a reliable demo path in `Main` or `testing` for the hustle-style branch so the style system has visible feedback during review.
5. Add a short root-level README or submission note pointing reviewers to the correct branch if `main` is intentionally incomplete.
