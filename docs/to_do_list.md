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
1. Does character-creation Gender selection affect any mechanics/stats, or is it purely cosmetic/narrative (pronouns, visual model)?
2. Librarian-Monk boss - unique mechanic not yet designed.
3. Giant House Centipede boss - unique mechanic not yet designed.
4. Cyclops Brothers boss - unique mechanic not yet designed.
5. Demon Prince boss - mechanic loosely Chaos-magic themed, not finalized.
6. Princess (final boss) - whether her mechanic is "randomly use one of the other bosses' mechanics" is not committed yet.
7. Slime Dragon fight - does defeating just the main dragon end the fight, or must every spawned dragling be cleared too?
8. Army interaction section - what does "interact with the army" actually consist of (dialogue? a questline? recruiting soldiers?)?
9. Day/night cycle - what actually drives the clock (real time, in-game turns/actions, travel count, etc.)?
10. Which of the existing companions (Warrior/Mage/Ranger/Rogue/Healer) survive as-is, get adjusted, or get fully replaced? (Warrior → Captain of the Royal Guard is the one confirmed swap so far.)

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
- NG+ ghost mechanic + secret ghost-only boss - needs New Game+ save tracking, a death-flow branch point (offer the ghost choice instead of an immediate game over), and a new "weapon enchantment" concept that doesn't exist yet. Note: enemies don't deal damage via `Weapon` objects today (just flat `MinDamage`/`MaxDamage` stats), so "only enchanted weapons can hurt a ghost" needs a new tag-based system, not a literal weapon check.
- Magic/spellcasting + component system - foundational for player build diversity, but nothing else on this list strictly requires it to exist first, so it can proceed in parallel. Fits well as a `SpellAbility : Ability` subtype (reusing the existing ability/combat pipeline) with components as a new `ItemCategory` that slots naturally into the already-unified loot table system.

**Tier 4 - smaller/independent additions (can slot in anytime):**
- Character-creation Gender selection.
- Elder of the Elven Village (NPC) - blocked on the fixed map existing (needs an Elven Village location to live at).
- Army interaction section - needs scoping first (Bucket A #8).
- Endless/gauntlet survival mode - mostly additive, doesn't depend on the other items here. "No healing between waves" already falls out for free from how `Player.Health` persists across separate fights today.
- Day/night cycle affecting enemy spawns - needs its own clock mechanism (Bucket A #9); may interact with the already-flagged-broken difficulty-scaling gap (see the "Difficulty selection has zero mechanical effect" item under Code Cleanup below).
- `Rest()` → inn-based, location-gated mechanic (unrelated to the story list above, but already an agreed-on change - carried over so it doesn't get lost).

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
- **Status**: FULLY COMPLETED!

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
- **Status**: ✅ FULLY COMPLETED
- **Description**: Complete shop system with dynamic pricing
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

### 🔴 5. Boss Key Progression System - SCOPE REDUCTION IN PROGRESS (2026-09-11)
> **See the "STORY-DRIVEN FEATURE ROADMAP" section above (2026-09-28) for the current plan** - the key/gate removal and the confirmed 9-boss roster (with mechanics) now live there as Tier 0/Tier 2 work.
- **Files**: `game_logic/entities/enemies/bosses/boss_enemy.cs`, `game_logic/entities/enemies/bosses/boss_manager.cs`, `game_logic/entities/enemies/bosses/boss_definitions.cs`, `game_logic/world/boss_encounter.cs`
- **Status**: 🚧 Framework complete, roster emptied pending redesign
- **Scope change**: Reduced from 15 bosses to 8, tied to a fixed story map with specific spawn locations (procedural map generation is being replaced - see item below). The original 15-boss roster was archived to `docs/boss_ideas_archive.md` for inspiration; `BossDefinitions.GetAllChampionBosses()` now returns an empty list.
- **Still needed** (not done yet, deliberately deferred):
  - Design and add the new 8-boss roster to `BossDefinitions`
  - Update `BossManager.TOTAL_BOSSES` (currently still 15) and `KEYS_REQUIRED` (currently still 10) to match the new 8-boss scale
  - Fix stale UI text in `GameManager.StartNewGame()` ("Defeat 10 of the 15 Champions...") and `BossDefinitions.GetBossListSummary()` ("Defeat any 10...") once the new numbers are decided
  - Tie boss encounters to the new fixed map's spawn locations instead of the Champion Challenges menu's free-select list (depends on the map work below)
- **Previously completed work below still applies to the framework** (key drops, strength scaling, save/load) - only the content (which bosses, how many) is being redone:
- **Description**: Complete boss progression system using champion keys to unlock final gate
- **Phase 1 Complete** (✅ Framework):
  - ✅ BossEnemy class with unique mechanics (12 mechanic types)
  - ✅ BossManager for tracking and strength scaling
  - ✅ 15 unique champion bosses defined
  - ✅ 15 unique champion keys as QuestItems
  - ✅ Dual strength scaling system:
    - 15% stronger per unique boss defeated (progression scaling)
    - 50% stronger per repeat defeat of same boss (anti-farming)
  - ✅ Final gate unlock logic (need 10 of 15 keys)
  - ✅ Random final boss selection per save file
- **Phase 2 Complete** (✅ Integration):
  - ✅ Combat manager integration (boss defeats tracked, key drops working)
  - ✅ BossManager integrated with GameManager
  - ✅ Game initialization complete (bosses registered, final boss selected)
  - ✅ Save system integration (boss progress, defeats, repeat counts persisted)
  - ✅ Boss encounter system with player warnings for repeat fights
  - ✅ Final gate encounter with key consumption
  - ✅ Champion menu for boss selection and status display
  - ✅ Final gate unlock bug fixed (keys consumed = gate unlocked, prevents save/load exploit)
- **Documentation**: See `docs/boss_system_phase2_plan.md`, `docs/boss_final_gate_fix.md`, `docs/boss_combat_verification.md`

### 🟡 6. Quest Event System
- **Files**: `game_logic/quests/`, `game_logic/entities/npcs/quest_giver.cs`, `game_logic/menus/job_board.cs`
- **Status**: ✅ FULLY COMPLETED
- **Description**: Complete quest system with 34+ quests, two quest hubs, and RNG variety
- **Features Implemented**:
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
  - ✅ Full integration with GameManager (menu options 8, 9, 10)
- **Documentation**: See `docs/quest_system_implementation.md`, `docs/quest_rng_system.md`

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
- **Status**: ✅ FULLY COMPLETED
- **Description**: Comprehensive settings system with multiple categories
- **Features Implemented**:
  - ✅ Display settings (turn order, combat log detail, damage calculations, enemy stats)
  - ✅ Gameplay settings (auto-save, confirmations)
  - ✅ RNG settings (algorithm selection, statistics tracking)
  - ✅ Accessibility settings (colored text, emojis, text speed)
  - ✅ Audio settings (placeholder for GUI version)
  - ✅ Difficulty selection (Normal/Hard/Difficult/Unfair - immutable after save creation)
  - ✅ Full save/load support
  - ✅ Reset to defaults option
  - ✅ Accessible from pause menu
- **Documentation**: See `docs/settings_system_implementation.md`

### 🟡 9. Statistics/Records Page
- **Files**: `game_logic/systems/statistics_tracker.cs`, `game_logic/menus/statistics_menu.cs`
- **Status**: ✅ FULLY COMPLETED
- **Description**: Comprehensive statistics tracking system for all gameplay metrics
- **Features Implemented**:
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
- **Ready for Integration**: Tracking methods ready, need to be called from game systems
- **Documentation**: See `docs/statistics_system_implementation.md`

### 🟡 9. Companion/Party System Integration
- **Status**: 🚧 Partially Complete
- **Description**: Companions exist but aren't fully integrated into Player class or combat
- **Current**:
  - ✅ Companion base classes exist
  - ✅ Companion abilities implemented
  - ❌ Party management in Player class
  - ❌ Companions in combat alongside player
  - ❌ Companion XP/level up from combat
- **Impact**: Companions can't be used in actual gameplay

---

## CODE CLEANUP / REFACTORING
_Tracking spots found while going through the codebase to build a better understanding of it and remove what isn't necessary._

### 🟢 Investigate progression/quest_system.cs and progression/loot_table.cs
- **Files**: `game_logic/progression/quest_system.cs`, `game_logic/progression/loot_table.cs`
- **Status**: ❌ Not Started
- **Description**: Both files only contain a couple of enums (`QuestType`/`QuestStatus` in quest_system.cs, `LootSourceType` in loot_table.cs) in the `GameLogic.Progression` namespace. Check whether these duplicate/overlap with enums already defined in `game_logic/quests/` (which has its own `quest.cs`, `quest_manager.cs`, etc.) and whether they should be merged or removed.
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

**Completed**: 11
- Equipment leveling system with player-choice upgrade paths
- Turn order with speed-based initiative
- Weapon/Armor save system with custom stats
- RevivePotion system with three tiers and combat integration
- ShopKeeper system with 3-layer dynamic pricing and restock system
- Enemy defense application (flat damage reduction)
- **Boss Key Progression System (Phases 1 & 2) - 15 bosses, dual scaling, final gate**
- **Quest Event System - 34+ quests with RNG variety and save/load support**
- **Settings Menu System - Comprehensive settings with difficulty selection**
- **Statistics Tracking System - Full gameplay metrics tracking and viewing**

**High Priority** (Next to tackle): 0
- All high priority items completed!

**Medium Priority**: 2
- Companion integration
- Save system enhancements (mostly done)

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
- Equipment and ability systems are complete with deep customization
- Weapon upgrade system provides strategic player choice (9 weapon types with unique scaling)
- Save system robustly preserves all custom-upgraded equipment
- Main gaps are in NPC interactions (shops, quests) and system polish
- GUI development is separate future phase after console version is complete

## RECENT COMPLETIONS (Latest Session - 2025-12-05)

### Boss Progression System (Phase 2 Complete)
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

### Quest System (Complete Implementation)
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

### Settings Menu System (Complete Implementation)
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

### Statistics Tracking System (Complete Implementation)
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