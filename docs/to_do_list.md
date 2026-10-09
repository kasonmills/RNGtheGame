# RNGtheGame - TODO List

## Legend
- ✅ = Completed
- 🚧 = In Progress / Partially Complete
- ❌ = Not Started
- 🔴 = High Priority
- 🟡 = Medium Priority
- 🟢 = Low Priority
- 🔵 = Future/GUI Phase
- 💼 = Business/Legal

---

## ⭐ ACTIVE FOCUS: STORY-DRIVEN FEATURE ROADMAP (added 2026-09-28)
_This is the living plan for turning the current codebase into what the story needs. It supersedes/extends item 5 (Boss Key Progression System) and item 10 (Replace Procedural Map) below - see those for original context, both are cross-referenced from here._

**How this is organized:**
- **Bucket A** - story/creative decisions still needed from the designer. This is the actual bottleneck right now - most "code work" below is a mechanical consequence of these, not independent work.
- **Bucket B** - confirmed features, sorted into tiers by dependency (foundational first), not by size. Tackling Tier 0 first collapses the most downstream uncertainty.

### Bucket A: Open story decisions
1. ✅ **Resolved (2026-10-05)** Character-creation Gender selection has no stat/mechanical difference between genders - purely cosmetic/narrative.
2. Librarian-Monk boss - unique mechanic not yet designed.
3. Giant House Centipede boss - unique mechanic not yet designed.
4. Cyclops Brothers boss - unique mechanic not yet designed.
5. Demon Prince boss - mechanic loosely Chaos-magic themed, not finalized.
6. Princess (final boss) - whether her mechanic is "randomly use one of the other bosses' mechanics" is not committed yet.
7. Slime Dragon fight - does defeating just the main dragon end the fight, or must every spawned dragling be cleared too?
8. ✅ **Resolved (2026-10-05)** Army interaction section - an optional, non-story area: an army camp preparing to go to war. The player can talk to the soldiers (dialogue only), or talk to one of the commanders, who offers to have his soldiers "test your strength" - the entry point into the endless combat mode (see Tier 4). Still open: the exact reward structure (gold and/or a prize, scaled by how long the player lasted and/or how many enemies they defeated).
9. Day/night cycle - what actually drives the clock (real time, in-game turns/actions, travel count, etc.)?
10. Which of the existing companions (Warrior/Mage/Ranger/Rogue/Healer) survive as-is, get adjusted, or get fully replaced? (Warrior → Captain of the Royal Guard is the one confirmed swap so far.)
11. Boss difficulty scaling - now that boss order is fixed, what should the difficulty curve look like? The current automatic scaling (15% per boss defeated, 50% per repeat fight) was designed for the old any-order key system (see item 5 below).
12. **Level cap and XP system review (decided 2026-10-08: cap drops from 100 to 50; XP rework deliberately deferred)** - the game is now about 8 bosses with roughly 5 combat encounters between each, not the 10+ encounters and 15 bosses the levelling was built for, so level 100 is out of reach in a normal playthrough. Plan: halve the level cap to 50 for abilities, weapons and armor. Do NOT rework XP yet - wait until more of the game is defined (map routes, encounter counts, boss difficulty curve in #11) so it can be scaled against real numbers. Not started in code. Things to cover when this is picked up:
    - Where the cap lives: `Ability.MaxLevel` (`game_logic/abilities/ability.cs`), `Weapon.MaxLevel`, `Armor.MaxLevel`, plus the enemy abilities that set `MaxLevel = 100` or divide by 99 (`rage.cs`, `poison_attack.cs`). Decide whether player/enemy levels follow the same cap.
    - Hard-coded level thresholds that assume 100: ability cooldown reductions at levels 25 and 75, milestone levels 10/25/50/75/100 in `leveling_system.cs`, Leadership's 25/50/75/100 (post-MVP).
    - Ability effects scale linearly from level 1 to `MaxLevel`, so changing the cap alone keeps the same top-end values but doubles the gain per level - confirm that is wanted.
    - XP curve: currently base XP by rarity (100/200/300/400/500) x 1.035 per level. Set a target first (what level should a typical player reach by the final boss?) and build the curve backwards from the expected number of fights.
    - XP sources: only active abilities gain XP today (per use in combat). Passive abilities, weapons and armor are never awarded XP anywhere in the code, so they cannot level at all - the "Equipment Leveling System" below is complete except for this.
    - Whether rarity should still slow levelling (Legendary currently needs 5x the XP of Common).
13. **Player class system - OUT OF THE MVP, post-MVP idea (decided 2026-10-09)** - the idea is kept, but it is not part of the MVP. The code has five classes (Warrior, Rogue, Mage, Ranger, Paladin), each with stat bonuses and armor bonuses/penalties (`game_logic/entities/player/player_class.cs`, described in `docs/player_class_system.md`). **To do for the MVP:** take the class system out of play without deleting it, the same way Leadership was handled - no class choice at character creation, and no class bonuses or armor penalties applied. Today every `Player` is created with a class (default Warrior), `player.cs` applies it, and the class is written to and read from the save file (`save_data.cs`, `save_manager.cs`), so those are the places to check. Knock-on question for the MVP: the six armor types exist mainly to serve the classes - decide what armor types mean without classes. **Post-MVP:** revisit the design alongside companions (#10) and ability/level balance (#12). Keep `player_class.cs` and `docs/player_class_system.md` as the reference for that.
14. **Armor types and the size of the weapon and armor lists - may be reduced (noted 2026-10-09)** - as the MVP scope shrinks, the six armor types and the large item catalog (207 weapons across 9 weapon types, 180 armor sets across 6 armor types in `game_logic/items/item_database.cs`) may need cutting down. The adjusted numbers are not known yet - they depend on the reduced scope (8 boss fights, smaller map, level cap 50, no classes in the MVP). Settle the numbers before doing other work on the catalog: fixing which items random loot can reach, creating the missing Legendary armor, and the nine weapon masteries (one per weapon type) all depend on them.

### Bucket B: Confirmed work, dependency-ordered

**Tier 0 - foundational, unblocks the most downstream work:**
- ✅ **Done (2026-09-30)** Remove the Champion Key/Final Gate system, replace with order-based progression (defeating boss N-1 unlocks boss N). `BossManager` now has `IsBossUnlocked`/`GetNextBoss`/`GetFinalBoss`/`IsFinalBoss` derived from registration order instead of keys/randomness.
- ✅ **Done (2026-09-30)** "Champion" → "Boss" terminology cleanup across the boss/quest system (`boss_manager.cs`, `boss_enemy.cs`, `boss_definitions.cs`, `boss_encounter.cs`, `final_boss_quest.cs`, `boss_defeat_quest.cs`, `quest_giver.cs`).
- ⏳ **Still pending, separate pass** Fixed/hand-drawn map (replacing the placeholder `GenerateFixedMap()`, see item 10 below) - blocks *placing* any new boss/NPC in the actual explorable world (Elder of the Elven Village, every new boss's encounter location). Each boss's combat mechanic can still be designed/built/tested independently of map placement. Needs real location design first (village/route names, where each boss lives) - deliberately not started yet.

**Tier 1 - companion system consolidation (do as one pass, not piecemeal - they all touch the same roster):**
- Decide the final companion roster (Bucket A #10).
- Build Fluke (luck companion) - needs a new team-wide buff/debuff concept ("luck"), since today's effect system (`AbilityEffect`) is single-target only.
- Swap Warrior → Captain of the Royal Guard (pending Bucket A #10 confirmation).
- Build the companion→boss transition plumbing (a leave-party trigger + a bridge from a `CompanionBase` instance to a matching `BossEnemy` instance) - needed for the Captain's late-game turn.
- General companion recruitment UI (beyond the one scripted tutorial method) - already deferred; now clearly needed since Fluke and any roster changes need a real recruit flow, not just the tutorial's one-off `RecruitStarterCompanion()`.

**Tier 2 - new bosses, in confirmed story order (corrected 2026-09-30, supersedes the earlier tentative order below this list used to describe):**
- #1 Skarn the Eagle Bear - ✅ done (tutorial fight).
- #2 Librarian-Monk (mechanic TBD)
- #3 Giant House Centipede (mechanic TBD)
- #4/#5 Cyclops Brothers (mechanic TBD) - already fits the multi-enemy combat engine (two bosses, one fight) with no new architecture needed for that part.
- #6 Demon Prince (Chaos-magic themed, TBD)
- #7 Slime Dragon - reactive "spawns draglings whenever hit" mechanic. Needs `IBossMechanic` to grow a new on-damage/reactive hook (today it only supports "decide my own turn"), and `CombatManager` to support enemies joining a fight already in progress - a real engine change, bigger than a typical new-boss addition.
- #8 Captain of the Royal Guard - signature is very high crit chance + crit multiplier; may need crit multiplier to become per-enemy configurable (unconfirmed whether it's currently a fixed global value - check `damage_calculator.cs`).
- #9 Princess (final boss) - build last, once the others' mechanics exist, in case she ends up borrowing from them (Bucket A #6). Tier 0's order-based progression is already in place, so this boss needs no key/gate work - just registering her last in `BossDefinitions.GetAllBosses()`.

  **Important (confirmed 2026-09-30):** the Princess being the final boss/antagonist is a twist that must stay hidden until the very end - she spends most of the game as a quest-giver sending the player on story quests. When building her: (1) don't add any flavor text elsewhere that foreshadows her before the reveal; (2) she needs both a quest-giver presence (early/mid-game) and a `BossEnemy` instance (late-game) - same shape as the Captain of the Royal Guard's companion→boss transition in Tier 1, but for a quest-giver→boss transition instead; (3) `GameStartup.InitializeQuests()` currently calls `finalQuest.Discover()` unconditionally at game start for whoever the final boss is - **this will spoil her identity immediately** once she's boss #9 unless that's changed to only discover the final quest once she's actually unlocked (see the `SPOILER RISK` comment left in `game_logic/core/game_startup.cs`).

**Tier 3 - large parallel systems (big, but don't block or get blocked by the boss/companion work above):**
- Magic/spellcasting + component system - foundational for player build diversity, but nothing else on this list strictly requires it to exist first, so it can proceed in parallel. Fits well as a `SpellAbility : Ability` subtype (reusing the existing ability/combat pipeline) with components as a new `ItemCategory` that slots naturally into the already-unified loot table system.

**Tier 4 - smaller/independent additions (can slot in anytime):**
- Character-creation Gender selection - cosmetic/narrative only, no stat differences (Bucket A #1, resolved).
- Elder of the Elven Village (NPC) - blocked on the fixed map existing (needs an Elven Village location to live at).
- Army camp (optional, non-story area) + endless "test your strength" mode (scoped 2026-10-05, Bucket A #8 - these were two separate items, now one feature since the camp is how the player reaches the endless mode):
  - Camp: soldiers camping and preparing for war, each with dialogue. Placing it in the world is blocked on the fixed map, same as the Elder.
  - Commander NPC: talking to him offers the challenge, which throws the player into back-to-back combat encounters with no healing between fights, until they fall.
  - Reward on exit: gold and/or a prize based on how long the player lasted and/or how many enemies they defeated (exact structure still open).
  - The combat loop itself is mostly additive and can be built/tested before the camp exists. "No healing between fights" already falls out for free from how `Player.Health` persists across separate fights today. Needs deciding at build time: what losing here means (it shouldn't be a normal game over, since the point is to last as long as possible).
- Day/night cycle affecting enemy spawns - needs its own clock mechanism (Bucket A #9); may interact with the already-flagged-broken difficulty-scaling gap (see the "Difficulty selection has zero mechanical effect" item under Code Cleanup below).
- `Rest()` → inn-based, location-gated mechanic (unrelated to the story list above, but already an agreed-on change - carried over so it doesn't get lost).

**Deferred - post-MVP (not needed to ship the MVP; revisit once there's time for features beyond it):**
- NG+ ghost mechanic + secret ghost-only boss (deferred 2026-10-05, was Tier 3) - fun, but not required for the MVP. Notes kept for when it's revisited: needs New Game+ save tracking, a death-flow branch point (offer the ghost choice instead of an immediate game over), and a new "weapon enchantment" concept that doesn't exist yet. Enemies don't deal damage via `Weapon` objects today (just flat `MinDamage`/`MaxDamage` stats), so "only enchanted weapons can hurt a ghost" needs a new tag-based system, not a literal weapon check.

---

## HIGH PRIORITY - Core Gameplay

### 🔴 1. Equipment Leveling System
- ✅ Weapon/Armor XP tracking
- ✅ Two-stage level up (XP + Blacksmith payment)
- ✅ Blacksmith NPC implementation
- ✅ Player-choice upgrade paths (3 options per level)
- ✅ Weapon-specific upgrade ranges (data-driven)
- ✅ All 9 weapon types defined with unique scaling
- ✅ Safety checks (min damage can't exceed max)
- ✅ Save system preserves custom-upgraded weapons
- **Status**: 🚧 Built, NOT working in the game (status corrected 2026-10-09 - previously "FULLY COMPLETED")
- **Not done**:
  - ❌ Weapons and armor are never awarded XP - nothing in the game calls their XP methods, so gear never becomes ready for an upgrade
  - ❌ The Blacksmith is never created or placed anywhere in the game, so the payment/upgrade-choice step cannot be reached
  - ❌ Level cap and XP curve are under review (Bucket A #12)

### 🔴 2. RevivePotion System
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

### 🔴 3. Shop Keeper Functionality
- **File**: `game_logic/entities/NPCs/shop_keeper.cs`
- **Status**: 🚧 Built, NOT reachable in the game (status corrected 2026-10-09 - previously "FULLY COMPLETED")
- **Not done**:
  - ❌ No shop is ever created or placed anywhere in the game - the `ShopKeeper` class is complete but there is no instance for the player to visit (needs shops on the map/towns and a Godot UI)
  - ❌ Shop visits, purchases and sales are not recorded in statistics
- **Description**: Shop system with dynamic pricing (the features below exist in the class)
- **Features**:
  - ✅ Full buy/sell menu system with item browsing
  - ✅ Tier-based shops (1-5) with better prices at higher tiers
  - ✅ Dynamic 3-layer price variability system:
    - **Layer 1**: Shop personality (0.85-1.15x, permanent per shop)
      - Generous shops have cheaper buy prices, pay more when buying from player
      - Greedy shops have expensive buy prices, pay less when buying from player
    - **Layer 2**: Market conditions (0.95-1.05x, changes per restock)
      - Buyer's market vs Seller's market
      - Affects both buy and sell prices
    - **Layer 3**: Per-item variation (±3% for shop items, ±2% for player sales)
      - Tiny haggling/negotiation variation per transaction
  - ✅ Personality-based dialogue (generous/fair/greedy merchants)
  - ✅ RestockShop() generates level-appropriate inventory
  - ✅ Can't sell quest items or equipped gear (with warnings)
  - ✅ Full shop info display explaining pricing system
  - ✅ Dual restock system (prevents shop scumming):
    - **Time-based**: 25±(1-5) minutes since last restock
    - **Combat-based**: 4±(1-2) combat encounters since last restock
    - Whichever condition is met first triggers restock
    - Requirements re-randomized after each restock
    - Initial stock provided at shop creation
  - ✅ Restock tracking display shows time and combat remaining
  - ✅ Combat integration complete:
    - Victory counts as 1 encounter, resets flee counter
    - First flee counts as 1 encounter (no penalty)
    - Consecutive flees add +5 min and +1 encounter to requirements
    - Prevents shop scumming via flee spam

### 🔴 4. Enemy Defense Application
- **File**: `game_logic/combat/damage_calculator.cs:87-317`
- **Status**: ✅ COMPLETED
- **Description**: Enemy defense reduces incoming damage using flat reduction system
- **Implementation**:
  - ✅ ApplyEnemyDefenseReduction() method exists (lines 303-317)
  - ✅ Called in CalculatePlayerAttackDamage() at line 87
  - ✅ Works as flat damage reduction (never below 1 damage minimum)
  - ✅ All enemy types have Defense values defined
  - ✅ Fully functional and integrated into combat flow

---

## MEDIUM PRIORITY - Systems & Features

### 🔴 5. Boss Progression System - ORDER-BASED (key/gate system removed 2026-09-30)
> **See the "STORY-DRIVEN FEATURE ROADMAP" section above for the current plan** - the confirmed 9-boss roster (with mechanics) lives there as Tier 2 work.
- **Files**: `game_logic/entities/enemies/bosses/boss_enemy.cs`, `game_logic/entities/enemies/bosses/boss_manager.cs`, `game_logic/entities/enemies/bosses/boss_definitions.cs`, `game_logic/entities/enemies/bosses/boss_mechanic.cs`, `game_logic/world/boss_encounter.cs`
- **Status**: 🚧 Framework complete, roster being built one boss at a time
- **How it works now**: Bosses unlock in a fixed order (defeating boss N-1 unlocks boss N), derived from registration order in `BossDefinitions`. There are no champion keys, no final gate, and no random final boss. The original 15-boss roster is archived in `docs/boss_ideas_archive.md` for inspiration only.
- **Still in the code from the old any-order design** (see "Still needed" below):
  - Progression scaling: every boss is 15% stronger per unique boss already defeated (`BossManager.STRENGTH_SCALING_PER_BOSS`)
  - Repeat penalty: a boss is 50% stronger per previous defeat of that same boss (`BossManager.REPEAT_PENALTY_PER_DEFEAT`), multiplied with the progression scaling
  - Reward scaling: gold/XP +5% per unique boss defeated and +10% per repeat (`BossEnemy.ApplyStrengthScaling`)
  - Boss progress, defeats and repeat counts are saved/loaded
- **Still needed**:
  - **Redesign boss difficulty scaling for the fixed order** - the 15%-per-boss and repeat-penalty rules were built for fighting bosses in any order, with key farming to discourage. With a set order, each boss's difficulty can be authored directly, so decide whether any automatic scaling (and any repeat penalty) is still wanted (Bucket A #11)
  - Build the remaining bosses in the roster (Tier 2 above)
  - Tie boss encounters to the fixed map's spawn locations (depends on the map work below)

### 🟡 6. Quest Event System
- **Files**: `game_logic/quests/`, `game_logic/entities/npcs/quest_giver.cs`, `game_logic/menus/job_board.cs`
- **Status**: 🚧 Built, NOT working in the game (status corrected 2026-10-09 - previously "FULLY COMPLETED")
- **Not done** (details in "Quests, statistics and settings are built but not connected" under Code Cleanup):
  - ❌ Quest progress is never updated - no game event (kill, gold, level up, boss defeat, weapon upgrade, equipment change) reaches the quests
  - ❌ Quest Giver and Job Board cannot be opened (waiting for the Godot UI)
  - ❌ Quest list is out of date: it was written for 14 champion bosses and the longer game; boss quests now follow the current roster, and the Job Board set needs resizing for the current scope
  - ❌ Final boss quest is revealed at game start, which would spoil the ending (see the note in `game_startup.cs`)
- **Description**: Quest system with two quest hubs and RNG variety
- **Features built** (the code exists; ✅ here means written, not working in play):
  - ✅ Quest base classes (Quest, QuestObjective, QuestReward)
  - ✅ QuestManager for centralized tracking and active quest management
  - ✅ 8 quest types:
    - BossDefeatQuest (14 quests - one per champion boss)
    - FinalBossQuest (1 quest - requires acceptance before completion)
    - LevelQuest (5 quests - reach level milestones)
    - EnemyKillQuest (3 quests - defeat X enemies with RNG variance)
    - GoldCollectionQuest (3 quests - earn total gold with RNG variance)
    - WeaponUpgradeQuest (2 quests - upgrade weapon to level X with RNG variance)
    - EquipmentQuest (2 quests - equip full gear set with RNG variance)
    - ChallengeQuest (4 quests - flawless victory, crit master, survivor, win streak)
  - ✅ **Quest Giver NPC** for boss-related quests (fixed rewards: 100g + 50 XP)
  - ✅ **Job Board menu** for all other quests (randomized requirements & rewards)
  - ✅ Quest states: NotDiscovered → Available → Accepted → Completed → Claimed
  - ✅ **Retroactive completion**: Progress tracks even if quest not accepted (except final boss)
  - ✅ **Active quest tracking**: Focus on one quest with ★ marker
  - ✅ Quest log display with progress tracking
  - ✅ **RNG quest generation** (once per save file):
    - Job board quest requirements randomized (±1-6 variance)
    - Job board quest rewards randomized (±20-30% variance)
    - Rewards scale with difficulty naturally
  - ✅ **Complete save/load support**:
    - Quest data serialized with requirements and rewards
    - ReconstructQuests() rebuilds quests with original RNG values
    - No re-randomization on load - quests stay consistent per save
  - ❌ GameManager menu access (the old console menu options 8, 9, 10 are commented out)

### 🟡 7. Turn Order with Speed Stats
- **File**: `game_logic/combat/combat_manager.cs:168`
- **Status**: ✅ COMPLETED
- **Description**: Dynamic turn order based on speed stats with action-based modifiers
- **Current**:
  - ✅ Speed-based turn order (highest speed acts first)
  - ✅ Action modifiers (Attack: -1 to -3, Defend: +1 to +3, Ability/Item: -1 to +1)
  - ✅ Revival speed penalty (revived entities get -2 to -4 speed)
  - ✅ Modifiers reset each round
  - ✅ Turn order displayed at start of each round

### 🟡 8. Settings Menu
- **File**: `game_logic/systems/game_settings.cs`, `game_logic/menus/settings_menu.cs`
- **Status**: 🚧 Built, mostly NOT connected (status corrected 2026-10-09 - previously "FULLY COMPLETED")
- **Not done**:
  - ❌ Only the two RNG settings are read by the game. Display, auto-save, confirmation, accessibility and audio settings are stored and saved but change nothing
  - ❌ Difficulty selection has no effect on gameplay (see "Difficulty selection has zero mechanical effect" under Code Cleanup)
  - ❌ The settings menu is console-only and needs a Godot UI
- **Description**: Settings system with multiple categories
- **Features built** (✅ here means the setting exists and is saved, not that it does anything):
  - ✅ Display settings (turn order, combat log detail, damage calculations, enemy stats)
  - ✅ Gameplay settings (auto-save, confirmations)
  - ✅ RNG settings (algorithm selection, statistics tracking)
  - ✅ Accessibility settings (colored text, emojis, text speed)
  - ✅ Audio settings (placeholder for GUI version)
  - ✅ Difficulty selection (Normal/Hard/Difficult/Unfair - immutable after save creation)
  - ✅ Full save/load support
  - ✅ Reset to defaults option
  - ✅ Accessible from pause menu

### 🟡 9. Statistics/Records Page
- **Files**: `game_logic/systems/statistics_tracker.cs`, `game_logic/menus/statistics_menu.cs`
- **Status**: 🚧 Built, NOT connected (status corrected 2026-10-09 - previously "FULLY COMPLETED")
- **Not done**:
  - ❌ Almost nothing is recorded. The only calls are at save time (play time, current gold, current level, save count). Battles, damage, kills, bosses, purchases, upgrades, item and ability use, quests and NPC visits are never recorded, so the pages below show zeros
  - ❌ The statistics menu is console-only and needs a Godot UI
- **Description**: Statistics tracking system for gameplay metrics
- **Features built** (✅ here means the tracker can store and display it, not that the game records it):
  - ✅ Combat statistics (battles, damage, kills, bosses, streaks)
  - ✅ Economic statistics (gold flow, purchases, sales)
  - ✅ Equipment statistics (upgrades, levels, weapon usage)
  - ✅ Item usage statistics (consumables, abilities)
  - ✅ Exploration statistics (shops, NPCs, quests)
  - ✅ Progression statistics (level, XP)
  - ✅ Achievement statistics (flawless victories, close calls, perfect crits)
  - ✅ 8 categorized viewing menus
  - ✅ Summary overview page
  - ✅ Calculated statistics (win rate, averages, favorites)
  - ✅ Full save/load support with dictionaries
  - ✅ Accessible from main menu (option 11)

### 🟡 9. Companion/Party System Integration
- **Status**: 🚧 Mostly complete (entry corrected 2026-10-09 - it previously said companions were not in combat)
- **Description**: Companions fight alongside the player
- **Current**:
  - ✅ Companion base classes exist
  - ✅ Companion abilities implemented
  - ✅ Party management (`PartyManager`, owned by `GameManager`; fixed party of 4 for the MVP)
  - ✅ Companions in combat: own turns in the speed-based turn order, AI picks attack / ability / defend by AI style (Aggressive, Defensive, Balanced, Supportive, Strategic - `CompanionBase.DecideCombatAction`), enemies can target them
  - ✅ Companion XP from combat (every living companion gets the full fight XP)
  - ❌ Recruiting: only the tutorial's starter companion is ever recruited (`GameManager.RecruitStarterCompanion`) - no way to gain the others yet
  - ❌ Never play-tested end to end (companion death, revive mid-combat, boss fights with companions, all companions down)
- **Depends on**: Bucket A #10 (which companions survive into the story)

---

## CODE CLEANUP / REFACTORING
_Tracking spots found while going through the codebase to build a better understanding of it and remove what isn't necessary._

### 🟡 Finish the docs/ folder cleanup (started 2026-10-08)
- **Status**: 🚧 Nearly done - boss docs (6 files down to 1), ability docs (8 files down to `abilities.md`), item docs (3 removed) and the six "implementation complete" write-ups (removed; unfinished work moved into this file) are done
- **Goal**: Get `docs/` down to a small set of files that reflect the current scope. For each group: read it against the current code, keep what is still true, merge or delete the rest.
- **Still to review**:
  - `player_class_system.md` - kept as it is, as the reference for a post-MVP feature (Bucket A #13). It describes code that is not part of the MVP; add a note at the top of the file saying so.

### 🟡 Folder-by-folder code review (started 2026-10-09)
_Big-picture pass over every folder to see where the code is at before the Godot frontend. Findings are logged here only - no code is changed during the review. Fixing/wiring happens in later sessions._

**Progress**: ✅ `systems/` · ✅ `world/` · ✅ `data/` · ✅ `progression/` · ✅ `quests/` · ⬜ `menus/` · ⬜ `items/` · ⬜ `abilities/` · ⬜ `entities/` · ⬜ `combat/` · ⬜ `core/` · ⬜ `godot_integration/` and root files

#### `game_logic/systems/`
- `RNG_manager.cs` - in use everywhere, healthy. **Decided 2026-10-09: the built-in system RNG algorithm is enough for the MVP.** Post-MVP idea: a system that uses several RNG algorithms at once (see Low Priority #9). Notes for then:
  - The two placeholder algorithms (`MersenneTwisterAlgorithm`, `XorshiftAlgorithm`) are empty shells that are never registered and throw "not implemented". Leave them as markers for the post-MVP work; they are harmless as long as they stay unregistered.
  - The manager is built to run one algorithm at a time and switch between them. Running several at once would need a design change, not just filling in the placeholders.
  - The settings menu offers an RNG algorithm choice, but for the MVP there is only one option - hide that setting in the Godot settings panel until there is a real choice.
- `game_settings.cs` - in use, but most settings are not read by anything (see the "built but not connected" item below). **Decided 2026-10-09 - which settings carry into the Godot settings panel:**
  - Drop `ColoredText` and `UseEmojis` - they only controlled terminal output (also remove them from the save data, `Clone()`, `ResetToDefaults()`, the console settings menu and `GameSettingsTests`).
  - Keep `TextSpeed` (for dialogue/story text that types out).
  - Keep the display toggles (turn order, detailed combat log, damage calculations, enemy stat block) as HUD toggles, the confirmations (flee, item use) as popups, and the audio settings.
  - `AutoSave` (and its interval): keep the setting in the code but **hide it in the MVP settings panel** - there is no auto-saving in the MVP (see `save_manager.cs` under `game_logic/data/` below).
  - None of the kept settings is read by anything yet - each still needs wiring.
- `statistics_tracker.cs` - **staying in the MVP.** Intended behaviour: every statistic updates the moment the thing happens, so the stats page is always accurate. Actual: about 30 recording methods exist, only 4 are ever called, all inside `SaveGame()`. Needs wiring into combat, shops, blacksmith, player, items, abilities and quests. Suggested approach: have the systems raise events (as `CombatManager` already does for Godot) and let the tracker subscribe - the same mechanism would also drive quest progress.
- **Godot hook-ups for settings and statistics are missing** (the two classes themselves are fine for Godot: plain data, no console code):
  - `GameManager` keeps both as private fields with no public access, so a Godot scene has no way to get at them.
  - There are no `OpenSettings()`/`CloseSettings()` or `OpenStatistics()`/`CloseStatistics()` methods like the ones that exist for inventory, character stats and pause. `GameState` has a `Settings` state that nothing uses and no state for the statistics page. (`OpenStats()` is the character sheet, not the statistics page.)
  - The only screens for them are `menus/settings_menu.cs` and `menus/statistics_menu.cs`, which are console menus (typed input, printed text) and cannot be used in Godot - each needs a Godot panel that reads/writes the same data.
  - Neither class announces changes. A statistics panel can simply read the numbers when opened, but settings need a "setting changed" signal so things like volume apply immediately.
  - Settings live inside the save file, so there are no settings before a game is loaded (e.g. volume on the title screen). Decide whether some settings should be global rather than per save.
- `difficulty_scaler.cs` - **KEEP (decided 2026-10-09).** This is the intended difficulty system. It is not connected to the game yet (only `tests/DifficultyScalerTests.cs` calls it), and that is expected for now: tuning it needs the Godot frontend and full playthroughs/simulations, so it will see little use until closer to the MVP. Before it can be tuned it still has to be wired in:
  - Nothing creates a `DifficultyScaler` - `GameManager`/`CombatManager` need one, and enemy health, damage, XP, gold and loot need to pass through it.
  - Its difficulty names (Easy/Normal/Hard/Nightmare) do not match the ones the player picks at character creation (Normal/Hard/Difficult/Unfair in `game_settings.cs`) - the two need to become one list.
  - Once it is the real system, the unused multiplier methods in `GameSettings` and the XP difficulty enum in `leveling_system.cs` become the duplicates to remove (see "Difficulty selection has zero mechanical effect" below).
- `event_system.cs` - **NEEDS AN IN-DEPTH REVIEW in a later session - do not delete yet (decided 2026-10-09).** Not used by anything today, tests included (552 lines). What it contains now: random world events (random encounter, merchant visit, treasure find, weather) with trigger chances, rarity, level limits, cooldowns and durations.
  - **Original intent**: a helper to `GameManager` for random events and spawning - e.g. laying out a city, placing quest givers around the map.
  - **What changed**: much of that is no longer random. Towns and areas will have set locations for the quest giver, blacksmith, magic shop and so on.
  - **Question for the review**: is this another case of a mechanic built twice with only one copy in use (compare `GameManager.Explore`/loot events, `OverworldEnemySpawn`, `MapNode.AvailableEvents`)? Or should it become the one central handler for every non-combat event, with event code that is currently scattered across separate files moved into it? Combat keeps its own event handling in `CombatManager`.
  - **Possible use (idea, 2026-10-09)**: the day/night cycle could be run from here. It fits what the file already does - events with durations that start, expire and can be checked as "currently active". Ties in with Bucket A #9 (what drives the clock).
  - **Outcome to reach**: either remove it, or reshape it into that central non-combat event handler.

#### `game_logic/world/`
- **Intended design (stated 2026-10-09)**: `map_manager.cs` owns the map, and every other file in this folder is a helper to it, so the game can always tell (and later display) which area the player is in and where things are relative to the player's position. Two levels of map node:
  - **Overworld nodes** - a city, a boss encounter, or a route/area between cities and other locations.
  - **Smaller nodes inside a city or camp** - the spawn point of the inn, a shop, the blacksmith and possibly quest givers. These are handled by, or connected to, `city.cs`.
- **Gap against that design**: only the overworld level exists. A town today is one `MapNode` with text labels in `AvailableEvents` ("Shop", "Rest"); nothing models the places inside it. This is the same gap that leaves the shop keeper, blacksmith and quest giver with nowhere to be placed (items 1, 3 and 6).
- `map_manager.cs`, `map_node.cs` - in use. The map itself is still the placeholder (`GenerateFixedMap()`: 10 made-up nodes such as "Haven Village", "The Tyrant's Lair"), not the story's routes and towns - already tracked as Low Priority #10 / Tier work. `LoadMap()` is a stub that just regenerates the map; `GenerateMapFromSeed()` is left over from the procedural map and is never called; the map seed is still written into every save file but nothing reads it back to rebuild a map.
- `boss_encounter.cs` - in use. `DisplayBossSelectionMenu()` and `GetAvailableBosses()` are leftovers from the removed Champion menu and are no longer called.
- `overworld_enemy_spawn.cs` - in use, logic only, waiting on Godot scenes.
- `map_node.cs` location types (`LocationType`: Town, Forest, Cave, Ruins, Mountain, Crossroads, BossRoom, TreasureRoom, RestSite) come from the old node-crawl design. Revisit them when the routes-and-towns map is built; `GameManager.GetEncounterSpawnRange()` depends on them.
- `city.cs` - **KEEP (decided 2026-10-09)**. Today it is a 16-line stub (name, population, location type) that nothing uses. It is the planned home for the in-town nodes described above and needs building out and connecting to `MapManager`/`MapNode`.
- `dungeon.cs` - **DELETED 2026-10-09**. Dungeons are out of scope, including for future versions. Nothing referenced it.
- `README.md` project tree is out of date: it lists a `world/location_type.cs` that does not exist (the enum lives in `map_node.cs`) and has not been checked against the other folders.

#### `game_logic/data/`
- `save_data.cs` - in use, healthy.
- `save_manager.cs` - in use.
  - **Save slots and auto-save - decided 2026-10-09:** the MVP has **one save slot and no auto-saving**. Post-MVP plan: multiple save slots (likely 3) and maybe auto-save. The game already saves to one fixed slot ("save1"), which matches the MVP. `AutoSave`, `QuickSave`, `LoadAutoSave`, `LoadQuickSave`, `DeleteSave`, `GetAllSaveFiles`, `GetSaveFileInfo` and `SaveExists` are only called by tests - leave them in place as the base for the post-MVP slots, and do not wire them for the MVP.
  - **Duplicate code - TO DO, delete it (decided 2026-10-09):** `LoadSettingsFromSaveData`, `SaveStatisticsToSaveData` and `LoadStatisticsFromSaveData` exist twice, once in `SaveManager` and an identical copy pasted into `QuestSerializationHelper` (about 190 lines). Despite the class name, the copied code is about settings and statistics, not quests. Only the `SaveManager` copies are called; remove the copies.
  - **Quests and saving - intended behaviour (stated 2026-10-09):** quest progress must update the moment something changes (example: quest is "get a weapon to level 10" and the weapon goes from 7 to 8 - the quest tracker updates right then). The save manager's only job is to read the current quest state from the quest manager when a save is called. The save side already works that way (`SaveGame` calls `QuestSerializationHelper.SerializeQuests(questManager)`); the missing half is the live updating, tracked in "Quests, statistics and settings are built but not connected to the game" below. When that is wired, check that no quest progress is calculated at save time.
- `game_data.cs` - **DELETED 2026-10-09** (362 lines). `GameDatabase` was an older, second item/enemy/companion/ability database, unused even by tests and superseded by `ItemDatabase` and the real factories. Its entries in `RNGTheGame.csproj` and the README tree were removed too.
- `constants.cs` - **DELETED 2026-10-09** (11 lines). Nothing used it; the same numbers are defined where they are needed elsewhere in the code, and a separate constants file is not wanted.

#### `game_logic/progression/`
- `loot_table.cs` - in use. This is the live loot system for enemy drops, chests and shop stock. **To do (2026-10-09): find out and fix why random loot only reaches part of the item database instead of every item** - tracked in "Random loot only draws from a fraction of the item catalog" below.
- `leveling_system.cs` - **not used by the game** (346 lines, only its test file `tests/LevelingSystemTests.cs` calls it). It was meant to be the single place for XP curves, item upgrade costs, enemy stat scaling by level, XP penalties for under-levelled enemies and the XP difficulty multiplier. Instead the player, abilities, weapons, armor and enemies each carry their own separate formulas. **KEEP (decided 2026-10-09) - it will likely stay but needs a significant rework**, to be done as part of the XP review (Bucket A #12), not before. Its current numbers disagree with the live ones (for example ability XP: base 50 growing 20% per level here, against base 100-500 growing 3.5% per level in `ability.cs`), and its `XpDifficultyLevel` is one of the three difficulty definitions that need unifying (see `difficulty_scaler.cs`).
- `quest_system.cs` - **not used by anything** (576 lines). A complete second quest system; the game uses `game_logic/quests/`. **Do NOT delete - idea under consideration (2026-10-09), see next point.**
- **Main story quests vs side quests (idea, 2026-10-09)**: the game needs a clear distinction between the two. Proposed split: rework `progression/quest_system.cs` into the **main quest tracker**, and make `game_logic/quests/` the **side quest** system, with every quest explicitly designated as one category or the other.
  - Where the code is today: `game_logic/quests/` (the live system) has no main/side designation at all. `quest_system.cs` already has a `QuestType` enum that includes `Main` and `Side`, plus objective types the live system lacks (talk to NPC, reach location, escort, use item).
  - To settle when this is planned: what a main quest is in this game (the boss order and story beats?), how the two trackers share one quest log and one save format, and how the final boss quest is handled without revealing the twist (see the spoiler risk in item 6).
  - Compare the two systems properly when the review reaches `game_logic/quests/`.

#### `game_logic/quests/` (first pass 2026-10-09 - 12 files, about 1,200 lines)
- `quest.cs`, `quest_objective.cs`, `quest_reward.cs` - the base pieces (a quest, one objective with a progress count, a reward of gold/XP/items). In use. Quest states: NotDiscovered -> Available -> Accepted -> Completed -> Claimed.
- `quest_manager.cs` - in use: created at game start, saved and loaded. Holds every quest, the one "active" (tracked) quest, and accept/complete/claim. `DisplayQuestLog()` and `DisplayActiveQuestTracker()` are console screens that need Godot versions. `SetQuestState()` uses reflection to set a property that is already public - tidy up.
- Eight quest types, all created at game start in `game_startup.cs` (`InitializeQuests`): `boss_defeat_quest.cs` (one per boss), `final_boss_quest.cs` (1), `level_quest.cs` (levels 5/10/15/20/25), `enemy_kill_quest.cs` (3), `gold_collection_quest.cs` (3), `weapon_upgrade_quest.cs` (2), `equipment_quest.cs` (2), `challenge_quest.cs` (4).
- **Already logged**: no quest ever progresses, and the Quest Giver and Job Board cannot be opened (see the item below); the final boss quest is revealed at game start (item 6).
- **New - quest rewards are never given to the player.** Claiming a quest marks it Claimed and prints the reward, but nothing adds the gold, XP or items to the player. Both `quest_giver.cs` and `job_board.cs` carry the comment "Actual reward application (gold, XP) happens in GameManager", and `GameManager` has no such code.
- **New - item rewards are not saved.** `QuestReward` can hold items, but the save file only stores a quest's gold and XP reward. No quest uses item rewards today.
- **New - every quest is handed out at game start.** All quests are created and made available immediately, and their random requirement/reward values are rolled once per save. Nothing ties a quest to a place, an NPC or a point in the story, and there is no quest order or prerequisite.
- **New - nothing would feed the challenge quests even after wiring.** Flawless victory, 5 critical hits in one battle, win under 10% health and win streak need per-battle counts that combat does not report. `ChallengeType.MultiKill` and `PerfectBlock` are defined but no quest uses them.
- **New - quests that depend on unfinished systems**: the weapon upgrade quests need gear that can level and a reachable blacksmith (item 1); the level quests stop at 25 and need checking against the level cap change (Bucket A #12); the gold quest only counts gold correctly if every source of gold reports to it.
- **New - story text is out of date**: quest descriptions and the final quest's completion text talk about "the greatest warrior in the realm" / "champion" framing from the old design, and `FinalBossQuest` prints its ending straight to the console.
- **Comparison with `progression/quest_system.cs` (for the main quest / side quest idea above)**:
  - What only `quest_system.cs` has: a main/side quest type; prerequisites (quests that must be finished first); a minimum level to accept; a named quest giver and a named turn-in NPC per quest; optional objectives; repeatable quests; abandoning a quest; a cap on active quests; rewards that unlock an area, a companion or an ability; one general progress method (`UpdateQuestProgress(type, target, amount)`); and `TurnInQuest`, which actually takes the player as input to hand over the reward.
  - What only `game_logic/quests/` has: save/load support, randomised requirements and rewards, the "discovered" step, progress that counts before a quest is accepted, the tracked active quest, the challenge quests, and tests (`tests/QuestManagerTests.cs`, 36 tests).
  - Reading: the unused system is the better fit for story quests (order, NPCs, unlocks); the live one is the better fit for job-board style side quests. That supports the proposed split, but the two have different quest, objective and reward classes with the same names, so they cannot simply run side by side - they need a shared base or one common quest log.

- **TO DOs decided 2026-10-09 for the quest system** (all ❌ Not Started; the work happens in later sessions):
  1. **Give quest rewards to the player.** Claiming a quest must add its gold to the player's gold, its XP to the player's experience and its items to the player's inventory. Wire this through `GameManager` so it works the same from the Quest Giver, the Job Board and any Godot screen.
  2. **Wire the whole quest system to `GameManager`** so quests progress and complete during play (the event wiring described in the item below).
  3. **Job board contents vary by city or town.** Build a system that decides which side quests appear on which town's job board, replacing "every quest available everywhere from the start". Depends on towns existing as real places (`game_logic/world/` - `city.cs`).
  4. **Main story quests unlock in story order.** The next main quest stays hidden until the previous one has been beaten. This also fixes the final boss quest being visible from the start (spoiler risk in item 6). Goes with the main quest / side quest split logged under `game_logic/progression/`.
  5. **Challenge quests read from statistics.** Wire the challenge quests to the statistics tracker so they track and update properly. Statistics must therefore record per-battle facts as they happen (damage taken, critical hits, blocks, enemies defeated in the fight, win streak) - today it records almost nothing (see `statistics_tracker.cs` under `game_logic/systems/`).
  6. **Flesh out quests so they read real game information.** Connect the quest system to the inventory and to the save/statistics data that already track the game's state - weapon levels, armor levels, ability levels, gold, player level and so on - so a quest checks the real value instead of keeping its own separate count.
  7. **Rework the quest text and prepare it for Godot.** Rewrite names, descriptions and completion text for the current story (drop the "greatest warrior in the realm" framing), and move text that is printed straight to the console (`DisplayQuestInfo`, `DisplayQuestLog`, `DisplayActiveQuestTracker`, `QuestReward.DisplayRewards`, the ending text in `FinalBossQuest.OnCompleted`) to data a Godot screen can show.
  8. **Save data for quests and rewards.** Item rewards must be saved (today only a quest's gold and XP reward are), and the inventory must save properly once reward items land in it. The save must record each quest as completed, in progress, not started or **locked**. Today the save stores each quest's state and objective progress, but there is no "locked" state - `NotDiscovered` is the closest and nothing sets a quest back to it. Add the locked state together with to-do 4.

#### `game_logic/menus/` (first pass 2026-10-09 - 3 files, about 1,250 lines)
- All three files are **console screens**: they print text and wait for typed input (433 console calls between them), so none can run in Godot. Each has one entry method, and the only calls to those are in the commented-out console menu in `game_manager.cs` - **nothing reaches any of them today**. No tests.
- `job_board.cs` (378 lines) - side quest screen: view available quests, accept, view active, claim rewards, set the tracked quest. Shows every quest except boss and final boss quests. Carries two gaps already logged under `game_logic/quests/`: rewards are displayed but never given, and the board is the same everywhere (not per town).
- `settings_menu.cs` (504 lines) - edits `GameSettings` in five groups: display, gameplay, RNG, accessibility, audio (labelled "Placeholder"), plus view difficulty and reset to defaults. Its accessibility group is Colored Text, Use Emojis and Text Speed - the first two are being dropped. It offers the RNG algorithm choice and the auto-save interval, both to be hidden for the MVP.
- `statistics_menu.cs` (364 lines) - shows the statistics in eight pages: combat, economic, equipment, item usage, exploration, progression, achievements, summary. Almost every number would read zero today because statistics are not recorded during play.
- **Value of these files**: the screen logic cannot be reused, but each one is a ready-made list of what its Godot screen has to show and let the player do (menu structure, groupings, labels). Use them as the specification when planning the Godot frontend, then remove them once the Godot screens exist.
- **STILL TO DECIDE - the session of 2026-10-09 stopped here, pick the review up at this point**: keep the three files until the Godot screens exist (suggested), or comment out / delete them now. `menus/` has been presented but not discussed; the folders after it have not been looked at.
- **Other console screens live outside this folder** (quest giver, shop keeper, blacksmith, quest log, boss screens and more) - to be listed as each folder is reviewed.

### 🔴 Quests, statistics and settings are built but not connected to the game (found 2026-10-09)
- **Status**: ❌ Not Started
- **Description**: Items 6, 8 and 9 above were marked FULLY COMPLETED until 2026-10-09. The classes, menus and save/load are all there, but the game never feeds them:
  - **Quests never progress.** Nothing outside `game_logic/quests/` calls the progress methods (`OnEnemyDefeated`, `OnGoldEarned`, `UpdateLevel`, `OnBossDefeated`, `OnFinalBossDefeated`, `OnWeaponUpgraded`, `CheckEquipment`, `UpdateChallengeProgress`, `QuestManager.CheckQuestCompletion`), and git history shows they never had callers. Killing enemies, earning gold, levelling up or beating a boss does not move any quest forward. They need to be called from combat (`combat_manager.cs`), the player (gold, level, equipment), the blacksmith and the boss encounter.
  - **Quest Giver and Job Board cannot be opened.** Their calls sit in the commented-out console menu in `game_manager.cs`, waiting for the Godot UI.
  - **Statistics record almost nothing.** The only calls are at save time (play time, current gold, current level, save count). Battles, damage, kills, shop visits, purchases, upgrades, item and ability use, quest and NPC interactions are never recorded. The tracker has the methods; they need calling from `combat_manager.cs`, `shop_keeper.cs`, `blacksmith.cs`, `player.cs`, `consumable.cs`, `ability.cs`, `quest_manager.cs`, `quest_giver.cs` and `job_board.cs`.
  - **Most settings do nothing.** Only the RNG algorithm and RNG statistics-tracking settings are read (`game_startup.cs`). The display, auto-save, confirmation, accessibility and audio settings are stored and saved but nothing reads them. Difficulty is covered by its own item below.
- **Quest content to revisit for the current scope**: boss quests are generated one per boss from the roster (fine), but the Job Board set (5 level quests, 3 kill, 3 gold, 2 weapon upgrade, 2 equipment, 4 challenge) and its requirement numbers were sized for the longer game - e.g. "upgrade a weapon to level 10" while gear cannot currently level at all (Bucket A #12).

### 🟡 Unit tests still missing (found 2026-10-09)
- **Status**: ❌ Not Started
- **Description**: `tests/` has 11 test files (RNG, leveling, difficulty scaler, damage calculator, turn manager, combat manager, save manager, player, statistics tracker, game settings, quest manager). Planned but never written:
  - `BossManagerTests` (now for the order-based unlock, not keys)
  - `ShopKeeperTests` (pricing, restock, buy/sell)
  - Weapon/armor upgrade tests (XP, blacksmith payment, upgrade choices)
  - `SaveManagerTests` updates for settings, statistics, quest reconstruction and boss progress
  - Cross-system integration tests
  - A CI pipeline to run the tests (there is no `.github/workflows/`)
- **Also**: `CombatManagerTests.cs` is known-broken since the combat engine rework and needs rewriting against the new event-driven `CombatManager`.

### 🔴 Random loot only draws from a fraction of the item catalog (found 2026-10-09)
- **Goal (2026-10-09)**: random loot should be able to draw from the whole item database. Find out why it does not, then fix it.
- **Files**: `game_logic/items/item_database.cs` (`GetAllWeapons`, `GetAllArmor`, `GetRandomWeapon`, `GetRandomArmor`), `game_logic/progression/loot_table.cs`
- **Status**: ❌ Not Started
- **Description**: `GetWeapon()` defines 207 weapons and `GetArmor()` defines 180 armor sets, but random loot picks from the `GetAllWeapons()` / `GetAllArmor()` lists, and those were never brought up to date:
  - `GetAllWeapons()` still lists only the original 19 weapons, so the other 188 (every Epic and Legendary weapon included) can never come from a random drop.
  - `GetAllArmor()` lists 150 armor sets and leaves out all 30 Epic ones.
- **Also open**: the armor plan called for 30 Legendary armor sets (5 per armor type) that were never created - there is no Legendary armor in the game. Decide whether they are still wanted at the reduced scope.
- **Check first**: the catalog itself may be reduced for the MVP (Bucket A #14). Settle those numbers before bringing the lists up to date, so the work is not done for items that get cut.

### 🟢 Ignore stray generated files
- **Status**: ❌ Not Started
- **Description**: `coding_sessions.json` at the repo root (written when `tools/git_time_tracker.py` is run from the root instead of from `tools/`) and `tools/__pycache__/` show up as untracked files. Add both to `.gitignore` and delete the stray root copy.

### 🟢 Leadership ability disabled for the MVP (2026-10-08)
- **Files**: `game_logic/entities/player/player.cs`, `game_logic/abilities/leadership_ability.cs`, `game_logic/entities/NPCs/companions/party_manager.cs`
- **Status**: ⏸ Parked until after the MVP
- **Description**: Leadership is commented out of `Player.GetAvailableAbilities()` and `Player.CreateAbilityFromName()`. The ability class and the party-size check in `PartyManager` are left in place, unused. To bring it back, uncomment those two spots. Not yet compiled or tested since the change.

### 🟢 Investigate progression/quest_system.cs and progression/loot_table.cs
- **Files**: `game_logic/progression/quest_system.cs`, `game_logic/progression/loot_table.cs`
- **Status**: ❌ Not Started
- **Description**: Both files only contain a couple of enums (`QuestType`/`QuestStatus` in quest_system.cs, `LootSourceType` in loot_table.cs) in the `GameLogic.Progression` namespace. Check whether these duplicate/overlap with enums already defined in `game_logic/quests/` (which has its own `quest.cs`, `quest_manager.cs`, etc.) and whether they should be merged or removed.
- **Correction (2026-10-09)**: neither file is "just enums". `loot_table.cs` is the live loot system (`LootTable`, `LootGenerator`, `LootTableTemplates`) and should stay. `quest_system.cs` is a complete second quest system (576 lines: `Quest`, `QuestObjective`, `QuestReward`, `QuestSystem`, `QuestTemplates`) that nothing else references - the game uses `game_logic/quests/` instead - so it is the candidate for removal. **Update 2026-10-09: not being removed for now** - it may become the main story quest tracker (see `game_logic/progression/` in the folder-by-folder code review).
- **Context**: Bosses were just moved out of `progression/` into `game_logic/entities/enemies/bosses/` (namespace `GameLogic.Entities.Enemies.Bosses`) since `BossManager`/`BossDefinitions` were content/entity-related, not progression logic. `quest_system.cs` and `loot_table.cs` look like they might be similar leftover misplacement.

### 🔴 Difficulty selection has zero mechanical effect on gameplay
- **Files**: `game_logic/systems/game_settings.cs`, `game_logic/systems/difficulty_scaler.cs`, `game_logic/progression/leveling_system.cs`, `game_logic/menus/settings_menu.cs`
- **Status**: ❌ Not Started
- **Description**: Found while renaming the difficulty tiers (2026-09-10). There are actually THREE separate, disconnected "difficulty" concepts in the codebase:
  1. `GameLogic.Systems.DifficultyLevel` (game_settings.cs) - the one actually chosen at new-game creation (`GameManager.SelectDifficulty`), now named Normal/Hard/Difficult/Unfair. `GameSettings.GetDifficultyMultiplier()`/`GetRewardMultiplier()` exist but are **only ever read by the settings menu display** - never applied to actual enemy stats or rewards anywhere in combat/loot code.
  2. `GameLogic.Progression.XpDifficultyLevel` (leveling_system.cs) - **renamed 2026-09-10** from the confusing same-named `DifficultyLevel` to disambiguate from #1. Intentionally distinct from the main difficulty enum - it's meant to scale XP gain specifically (harder difficulty = modestly more XP, not a 1:1 stat multiplier), now using values Normal=0.8x/Hard=1.0x/Difficult=1.25x/Unfair=1.5x (adopted from `DifficultyScaler.ScaleEnemyXP`'s curve). `LevelingSystem.GetDifficultyMultiplier()` is still only ever called from unit tests, not live game code - **still needs to be wired into the actual XP-award path** (likely `combat_manager.cs`, wherever `enemy.XPValue` is granted to the player) using `_gameSettings.Difficulty` mapped to the corresponding `XpDifficultyLevel` tier.
  3. `GameLogic.Systems.GameDifficulty` (difficulty_scaler.cs) - yet another distinct enum (Easy/Normal/Hard/Nightmare) feeding the whole `DifficultyScaler` class (health/damage/XP/gold scaling, elite/boss scaling, loot quality) - a fully-built system that is **never instantiated anywhere outside its own test file**. `GameManager` has no `DifficultyScaler` field at all.
  - **Impact**: Picking Normal/Hard/Difficult/Unfair at character creation currently does nothing to actual enemy difficulty - it's cosmetic only. `DifficultyScaler` looks like the intended real implementation but was never wired in.
  - **Next step**: Decide whether to wire `DifficultyScaler` into `GameManager`/`CombatManager` (and delete the redundant `Progression.DifficultyLevel` + `GameSettings` multiplier methods), or remove `DifficultyScaler` if the flat `GameSettings` multipliers are meant to be the real mechanism (in which case those need to actually be applied in combat/loot code).

---

## LOW PRIORITY - Polish & Optional Features

### 🟢 9. Alternative RNG Algorithms
- **File**: `game_logic/systems/RNG_manager.cs:75-92`
- **Status**: ❌ Placeholder Only
- **Description**: Mersenne Twister and Xorshift RNG algorithms have interfaces but throw NotImplementedException
- **Impact**: Minimal - System.Random works fine for current needs
- **Note**: Optional enhancement for players who want specific RNG behaviors

### 🔴 10. Replace Procedural Map with Fixed Story Map
> **See the "STORY-DRIVEN FEATURE ROADMAP" section above (2026-09-28) for the current plan** - this is Tier 0 foundational work there, blocking where new bosses/NPCs get placed in the world.
- **File**: `game_logic/world/map_manager.cs`
- **Status**: ❌ Not Started (scope change, 2026-09-11)
- **Description**: Scope reduced - the map is no longer randomized. It needs to become a fixed, story-driven map with specific spawn locations (tied to the new 8-boss roster above), replacing `GenerateNewMap()`/`GenerateMapFromSeed()`'s current procedural "Slay the Spire"-style linear generation.
- **Impact**: This changes how saves store map state (`MapSeed` regeneration won't make sense for a fixed map) and how boss encounters are triggered (map location instead of free-select Champion menu).
- **Not started yet** - needs design input (how many locations, what's at each, story beats) before implementation.

### 🟢 11. Player Execute Method
- **File**: `game_logic/entities/player/player.cs:186`
- **Status**: 🚧 Placeholder
- **Description**: Abstract Execute() method from Entity has no real logic for player
- **Impact**: Minimal - method isn't actively used in current gameplay loop

---

## CONTENT CREATION

### 12. Create More Abilities
- **Status**: 🚧 Ongoing
- **Current**:
  - ✅ 4 Player abilities (Attack Boost, Critical Strike, Defense Boost, Healing)
  - ✅ 2 Enemy abilities (Poison Attack, Rage)
  - ✅ 5 Companion abilities (one per companion type)
- **Needed**: More variety for players and enemies
- **System**: ✅ Ability framework is complete and scalable

### 13. Update Logic for New Abilities
- **Status**: 🚧 Ongoing
- **Description**: As new abilities are created, ensure they integrate properly with:
  - Combat system
  - Effect system
  - Status effect display
  - AI decision-making for enemies

### 14. Expand Loot Tables
- **Status**: 🚧 Good Progress
- **Current**:
  - ✅ Item database exists with weapons, armor, consumables
  - ✅ Basic loot drop system in enemies
  - ✅ 19 weapons across 9 weapon types:
    - Swords (3): Rusty, Iron, Steel
    - Axes (2): Rusty Axe, Battle Axe
    - Maces (2): Blunt Mace, Spiked Mace
    - Spears (2): Hunting Spear, War Spear
    - Staves (2): Wooden Staff, Arcane Staff
    - Bows (2): Hunting Bow, Longbow
    - Crossbows (2): Light Crossbow, Heavy Crossbow
    - Wands (2): Apprentice Wand, Arcane Wand
    - Daggers (2): Rusty Dagger, Assassin's Blade
  - ✅ 9 armor pieces across slots/types
  - ✅ 12 consumables (health potions, food, elixirs, bombs, antidotes, revival potions)
- **Still Needed**:
  - More high-tier weapons (Epic/Legendary)
  - More armor variety
  - Set items (future)

---

## SAVE SYSTEM ENHANCEMENTS

### 15. Save Data for Selections
- **Status**: 🚧 Mostly Complete
- **Current**:
  - ✅ Player stats, inventory, equipped items saved
  - ✅ Ability level/XP saved
  - ✅ Map position saved
  - ✅ Weapon/Armor XP progress and custom stats
  - ✅ Weapon upgrade choices preserved across saves
  - ✅ ReadyForUpgrade state restoration
  - ❌ Quest progress (when quest system implemented)
  - ❌ Companion data (when companions integrated)
  - ❌ World state (defeated enemies, looted chests, etc.)

---

## FUTURE PHASE - GUI & Game Engine

### 🔴 Plan the Godot frontend file structure and file contents (added 2026-10-09)
- **Status**: ❌ Not Started
- **Phase**: Planning - do this BEFORE any Godot implementation, so the thinking is finished separately from the building
- **Goal**: a written plan of every folder and file the Godot frontend will have, and what each file contains and is responsible for. Implementation then follows the plan instead of inventing structure as it goes.
- **Output**: one planning document in `docs/` (for example `docs/godot_frontend_plan.md`)
- **What the plan needs to cover**:
  - **Folder layout** of the Godot project: scenes, scripts, UI, and art/audio assets (sprites, tiles, map images, icons, fonts, music, sound effects). Where the Godot project sits relative to `game_logic/`, and how the C# game logic is included in it.
  - **Asset storage**: `game_logic/` holds code only. Images such as a world-map picture belong in a Godot assets folder, not in `game_logic/world/`. No assets folder and no image or scene files exist in the repo yet.
  - **Scene list**: one entry per screen or area - main menu, character creation/tutorial, overworld areas (routes, towns, camps), combat, boss fights, shop, blacksmith, quest giver/job board, inventory, character sheet, settings, statistics, pause, save/load, game over, ending.
  - **For each file**: its purpose, which `game_logic` classes it talks to, which `GameManager` Open/Close methods and events (`StateChanged`, `CombatMessage`, `CombatEnded`) it uses, and what the player sees and can do on it.
  - **Map display**: how `MapManager`/`MapNode` data (including each node's `PositionX`/`PositionY`) is drawn, whether each area is its own scene, and whether the MVP needs a world-map overview screen. Depends on the two-level node design logged under `game_logic/world/` in the code review.
  - **Gaps the plan will expose in `game_logic`**: hook-ups the frontend needs that do not exist yet (settings and statistics access is already logged in the code review; expect more).
- **Starting point**: `godot_integration/` has 11 placeholder files (`scripts/` - combat scene, enemy, map and player controllers; `UI/` - UI manager, HUD and menus). All are empty except an 11-line `map_controller.cs`, and the folder is excluded from the build. The plan decides whether to keep, rename or replace them.
- **Best done after**: the folder-by-folder code review (so the plan is based on what the code really offers) and the Bucket A decisions that change which screens exist.

### 🔵 3. Get Godot
- **Status**: ❌ Not Started
- **Phase**: GUI Development

### 🔵 4. Figure Out Sprites
- **Status**: ❌ Not Started
- **Phase**: GUI Development
- **Needed**:
  - Character sprites
  - Enemy sprites
  - Item icons
  - Environment tiles
  - UI elements

### 🔵 5. Learn Godot
- **Status**: ❌ Not Started
- **Phase**: GUI Development

### 🔵 6. Learn Game Engines
- **Status**: ❌ Not Started
- **Phase**: GUI Development
- **Goal**: Understand engine architecture for potential custom engine

### 🔵 9. Make Game Maps
- **Status**: 🚧 Partially Complete
- **Current**: ✅ Procedural map generation functional
- **Future**: Visual map design in Godot

---

## BUSINESS & LEGAL

### 💼 2. Write Game Story
- **Status**: ❌ Not Started
- **Description**: Create the narrative/lore for the game world
- **Needed**:
  - Main storyline
  - Character backstories
  - World lore
  - Quest narratives

### 💼 7. Marketing Strategy
- **Status**: ❌ Not Started
- **Phase**: Pre-Launch
- **Needed**:
  - Target audience research
  - Social media presence
  - Trailer/screenshots
  - Press kit
  - Community building

### 💼 8. Steam Publishing
- **Status**: ❌ Not Started
- **Phase**: Launch
- **Needed**:
  - Steam Direct account
  - Store page setup
  - Build preparation
  - Achievement integration

### 💼 10. Copyright Registration
- **Status**: ❌ Not Started
- **Phase**: Pre-Launch
- **Note**: Research if needed for indie game

### 💼 11. Create LLC
- **Status**: ❌ Not Started
- **Phase**: Pre-Launch
- **Note**: Consult lawyer/accountant about business structure

---

## SUMMARY BY STATUS

_Corrected 2026-10-09 after checking each item against the code. "Built, not connected" means the classes exist but the game never uses them - these are NOT done._

**Completed and working in combat**: 4
- Turn order with speed-based initiative
- RevivePotion system with three tiers and combat integration
- Enemy defense application (flat damage reduction)
- Weapon/Armor save system with custom stats

**Built, not connected** (do not skip these): 5
- Equipment leveling - gear never gains XP, Blacksmith never placed in the game (item 1)
- ShopKeeper - no shop exists in the game to visit (item 3)
- Quest system - quest progress is never updated (item 6)
- Settings - most settings do nothing, difficulty has no effect (item 8)
- Statistics - almost nothing is recorded (item 9)

**In progress**: 3
- Boss progression - order-based framework done, roster and difficulty scaling open (item 5)
- Companion integration - in combat, but only the tutorial companion can be recruited
- Save system enhancements (mostly done)

**Removed**: the Boss Key Progression System (15 bosses, keys, final gate) was replaced by order-based unlocking on 2026-09-30

**Low Priority**: 3
- Alternative RNG algorithms
- Custom map loading
- Player Execute method

**Ongoing**: 2
- Create more abilities
- Expand loot tables

**Future/GUI Phase**: 5
**Business/Legal**: 5

---

## NOTES
- Core game logic is solid and functional
- Combat system works excellently with speed-based turn order
- Equipment and ability classes are written, but levelling is not working in play: gear and passive abilities never gain XP (Bucket A #12)
- Weapon upgrade system provides strategic player choice (9 weapon types with unique scaling) - not reachable until the Blacksmith is placed in the game
- Save system robustly preserves all custom-upgraded equipment
- Main gaps: systems that are built but not connected to the game (shops, blacksmith, quests, statistics, settings) - see SUMMARY BY STATUS
- The console menus are stubbed out; these systems need Godot UI and map placement to become reachable

## SESSION HISTORY (2025-12-05) - what was written, NOT current status

> **Read this as history only.** These entries record code that was written in that session. Several of the systems are not connected to the game, and the boss key system has since been removed. For current status use the numbered items and SUMMARY BY STATUS above.

### Boss Progression System (key/gate version - REMOVED 2026-09-30)
- ✅ Full combat integration with boss scaling and key drops
- ✅ BossManager integrated into GameManager
- ✅ Boss encounter system with warnings for repeat fights
- ✅ Final gate key consumption (10 keys required)
- ✅ Final gate unlock bug fixed (prevents save/load exploit)
- ✅ Dual scaling system:
  - 15% stronger per unique boss defeated
  - 50% stronger per repeat defeat of same boss
- ✅ Diminishing returns on key drops (100% → 50% → 25% → 12.5%...)
- ✅ Save system preserves boss progression, defeats, and repeat counts
- ✅ Boss combat verified - uses same combat system as regular enemies

### Quest System (code written - not connected, see item 6)
- ✅ Quest base architecture (Quest, QuestObjective, QuestReward, QuestManager)
- ✅ 8 quest types with 34+ total quests:
  - 14 Boss defeat quests
  - 1 Final boss quest (special: requires acceptance)
  - 5 Level progression quests
  - 3 Enemy kill quests
  - 3 Gold collection quests
  - 2 Weapon upgrade quests
  - 2 Equipment quests
  - 4 Challenge quests
- ✅ **Quest Giver NPC**: "Veteran Ranger" for all boss quests
- ✅ **Job Board Menu**: Browse, accept, track, and claim all other quests
- ✅ **RNG Quest Generation**:
  - Job board quests randomized once per save file
  - Requirements: ±1-6 variance depending on quest type
  - Rewards: ±20-30% variance for gold and XP
  - Boss quests remain fixed (100g + 50 XP) for consistency
- ✅ **Quest Persistence**:
  - Complete quest data saved (requirements, rewards, progress, state)
  - ReconstructQuests() rebuilds with original RNG values on load
  - Each save file has unique quest challenges
- ✅ Retroactive completion (progress tracks before acceptance)
- ✅ Active quest tracking with ★ marker
- ✅ Quest log with detailed progress display
- ✅ Menu integration (Options 8, 9, 10 in main game loop)

### Settings Menu System (code written - mostly not connected, see item 8)
- ✅ **GameSettings Class**: Complete settings data structure
- ✅ **Settings Categories**:
  - Display settings (turn order, combat log, damage calculations, enemy stats)
  - Gameplay settings (auto-save, confirmations)
  - RNG settings (algorithm selection, statistics tracking)
  - Accessibility settings (colored text, emojis, text speed)
  - Audio settings (placeholder for GUI)
  - Difficulty settings (Normal/Hard/Difficult/Unfair)
- ✅ **Difficulty System**:
  - 4 difficulty levels affecting enemy stats (75%-200%) and rewards (80%-150%)
  - Immutable after save file creation (prevents exploitation)
  - Selected at new game creation before character name
- ✅ **SettingsMenu**: Interactive menu with 6 submenus
- ✅ **Full Save/Load Support**: All settings persist in save file
- ✅ **Integration**: Accessible from pause menu (option 4)
- ✅ **Reset to Defaults**: Option to reset all adjustable settings

### Statistics Tracking System (code written - not connected, see item 9)
- ✅ **StatisticsTracker Class**: Comprehensive tracking system
- ✅ **Statistics Categories**:
  - Combat (battles, damage, kills, bosses, win streaks)
  - Economic (gold flow, purchases, sales)
  - Equipment (upgrades, levels, weapon usage)
  - Item usage (consumables, abilities)
  - Exploration (shops, NPCs, quests)
  - Progression (level, XP)
  - Achievements (flawless victories, close calls, perfect crits)
  - Miscellaneous (play time, sessions, version)
- ✅ **StatisticsMenu**: 8 categorized viewing pages + summary overview
- ✅ **Calculated Stats**: Win rate, averages, favorites, highlights
- ✅ **Dictionary Tracking**: Top kills by enemy, weapons used, consumables used
- ✅ **Full Save/Load Support**: All statistics persist in save file
- ✅ **Integration**: Accessible from main menu (option 11)
- ✅ **Ready for Game System Integration**: All tracking methods implemented

### Previous Session Completions
- ✅ Weapon upgrade system with player choice at each level (9 weapon types)
- ✅ RevivePotion system with three tiers
- ✅ ShopKeeper system with 3-layer dynamic pricing
- ✅ Dual restock system (time + combat based)