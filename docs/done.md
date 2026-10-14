# RNGtheGame - Done

What is finished. The other half of `to_do_list.md`: that file is what is still to do, this file is what is already done.

**How this file works**
- When an entry in `to_do_list.md` is completed, it is **moved** here - cut from that file, pasted into this one - with the date it was moved.
- "Done" means built **and** working in the game. Code that is written but not connected to the game stays in `to_do_list.md`.
- If follow-up work on a finished item is still planned, the entry says where in `to_do_list.md` that work is logged.
- Newest entries go at the top of their section.
- Entries keep the number they had in `to_do_list.md`, so older notes that say "item 7" still make sense.

---

## Core Gameplay

### 2. RevivePotion System
- **Moved here**: 2026-10-13
- **File**: `game_logic/items/item_database.cs`, `consumable.cs`, `combat_manager.cs`
- **Status**: ✅ FULLY COMPLETED
- **Description**: Complete revival potion system with logic and items
- **Features**:
  - ✅ UseRevivePotion() method with level-scaled revival (1 HP at Lv1, full HP at Lv10)
  - ✅ Combat manager integration with target selection for dead allies
  - ✅ Revival speed penalty (-2 to -4 speed on revival)
  - ✅ Clears negative effects on revival
  - ✅ Three revival potion tiers:
    - Minor Revival Potion (Lv1-4): 10%-40% HP restoration (Uncommon)
    - Revival Potion (Lv5-8): 50%-80% HP restoration (Rare)
    - Greater Revival Potion (Lv9-10): 90%-100% HP restoration (Epic)
- **Follow-up still in `to_do_list.md`**: a fight ends the moment the player falls, so a revive potion can never be used on the player, only on companions (open question under `game_logic/combat/` in the code review); the revive message prints to the console instead of the combat log (under `game_logic/items/`).

### 4. Enemy Defense Application
- **Moved here**: 2026-10-13
- **File**: `game_logic/combat/damage_calculator.cs:87-317`
- **Status**: ✅ COMPLETED
- **Description**: Enemy defense reduces incoming damage using flat reduction system
- **Implementation**:
  - ✅ ApplyEnemyDefenseReduction() method exists (lines 303-317)
  - ✅ Called in CalculatePlayerAttackDamage() at line 87
  - ✅ Works as flat damage reduction (never below 1 damage minimum)
  - ✅ All enemy types have Defense values defined
  - ✅ Fully functional and integrated into combat flow
- **Follow-up still in `to_do_list.md`**: this covers the player attacking an enemy. Companion attacks still ignore defense, and the damage rules are to be made the same for any attacker against any target (to-dos 4 and 5 under `game_logic/combat/` in the code review).

## Systems & Features

### 7. Turn Order with Speed Stats
- **Moved here**: 2026-10-13
- **File**: `game_logic/combat/combat_manager.cs:168`
- **Status**: ✅ COMPLETED
- **Description**: Dynamic turn order based on speed stats with action-based modifiers
- **Current**:
  - ✅ Speed-based turn order (highest speed acts first)
  - ✅ Action modifiers (Attack: -1 to -3, Defend: +1 to +3, Ability/Item: -1 to +1)
  - ✅ Revival speed penalty (revived entities get -2 to -4 speed)
  - ✅ Modifiers reset each round
  - ✅ Turn order displayed at start of each round
- **Follow-up still in `to_do_list.md`**: the 8-turn-slot round (4 party, 4 opponents) decided 2026-10-12 builds on this - to-do 9 under `game_logic/combat/` in the code review.

## GUI & Game Engine

### 3. Get Godot
- **Moved here**: 2026-10-13
- **Status**: ✅ Done (installed, Godot 4.7.2 mono build)
- **Phase**: GUI Development
