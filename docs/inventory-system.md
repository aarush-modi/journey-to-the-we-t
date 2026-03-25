# Inventory System

## Overview

The inventory system manages two types of collectibles: **items** (general-purpose) and **packages** (quest-related). Equipment (armor, skills) is handled separately by `PlayerCombat`. Gold is tracked by `GreedMeter`.

## Scripts

| Script | Location | Purpose |
|--------|----------|---------|
| `PlayerInventory` | `Scripts/Player/PlayerInventory.cs` | Core inventory management |
| `ItemData` | `Scripts/Player/ItemData.cs` | ScriptableObject for items |
| `PackageData` | `Scripts/Player/PackageData.cs` | ScriptableObject for quest packages |
| `ArmorData` | `Scripts/Player/ArmorData.cs` | ScriptableObject for armor |
| `SkillData` | `Scripts/Player/SkillData.cs` | ScriptableObject for skills |
| `PlayerCombat` | `Scripts/Player/PlayerCombat.cs` | Handles equipment (armor + skill) |
| `GreedMeter` | `Scripts/Player/GreedMeter.cs` | Gold and greed tier tracking |
| `DroppedGold` | `Scripts/Global/DroppedGold.cs` | Collectible gold pickup |
| `ICollectible` | `Scripts/Interfaces/ICollectible.cs` | Interface for pickupable objects |

## PlayerInventory

Attached to the player GameObject. Maintains two separate lists.

### Methods

| Method | Purpose |
|--------|---------|
| `AddItem(ItemData item)` | Add an item to inventory |
| `RemoveItem(ItemData item)` | Remove an item from inventory |
| `HasItem(string name)` | Check if item exists by name |
| `GetItems()` | Returns read-only list of items |
| `AddPackage(PackageData package)` | Add a quest package |
| `RemovePackage(PackageData package)` | Remove a quest package |
| `GetPackageCount()` | Returns total number of packages |
| `GetPackages()` | Returns read-only list of packages |

### Events

| Event | When it fires |
|-------|---------------|
| `onInventoryChanged` | Any time an item or package is added/removed |

## Data Types

### ItemData

| Field | Type | Purpose |
|-------|------|---------|
| `itemName` | `string` | Display name |
| `description` | `string` | Item description |
| `icon` | `Sprite` | UI icon |
| `itemType` | `enum` | `Consumable`, `QuestItem`, `Equipment`, `Misc` |

Create via: Right-click > Create > Scriptable Objects > Item Data

### PackageData

| Field | Type | Purpose |
|-------|------|---------|
| `packageName` | `string` | Package name |
| `sealDescription` | `string` | Description of the seal |

Create via: Right-click > Create > Scriptable Objects > Package Data

## Equipment System

Equipment is managed by `PlayerCombat`, not `PlayerInventory`.

### ArmorData

| Field | Type | Purpose |
|-------|------|---------|
| `armorName` | `string` | Armor name |
| `damageReduction` | `float` | Damage reduction value |
| `armorSprite` | `Sprite` | Visual sprite applied to player |

### SkillData

| Field | Type | Purpose |
|-------|------|---------|
| `skillName` | `string` | Skill name |
| `damage` | `float` | Damage dealt |
| `cooldown` | `float` | Cooldown in seconds |
| `icon` | `Sprite` | UI icon for HUD |

### PlayerCombat Equipment Methods

| Method | Purpose |
|--------|---------|
| `EquipSkill(SkillData skill)` | Set the active skill |
| `EquipArmor(ArmorData armor)` | Set armor and update player sprite |

## Gold and Greed System

`GreedMeter` tracks gold separately from inventory.

### Greed Tiers

| Tier | Gold Required | Bonus |
|------|--------------|-------|
| None | 0-99 | No bonuses |
| Tier 1 | 100-299 | 1.1x damage |
| Tier 2 | 300-599 | 1.15x speed |
| Tier 3 | 600+ | 1.2x HP |

### Events

| Event | When it fires |
|-------|---------------|
| `onGoldChanged` | Gold amount changes |
| `onTierChanged` | Greed tier changes |

### DroppedGold

Implements `ICollectible`. Spawned when the player dies. Auto-despawns after 20 seconds. On pickup, calls `greedMeter.AddGold(goldAmount)`.

## How Systems Connect

```
VillageElderNPC
    |
    v
playerInventory.AddPackage(packageData)
    |
    v
onInventoryChanged fires
```

```
Player dies (PlayerCombat)
    |
    v
DroppedGold spawned at death position
    |
    v
Player walks over it → ICollectible.Collect()
    |
    v
greedMeter.AddGold()
    |
    v
onGoldChanged fires → HUDManager updates display
```
