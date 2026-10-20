# RNGtheGame - Done

What is finished. The other half of `to_do_list.md`: that file is what is still to do, this file is what is already done.

**How this file works**
- When an entry in `to_do_list.md` is completed, it is **moved** here - cut from that file, pasted into this one - with the date it was moved.
- "Done" means built **and** working in the game. Code that is written but not connected to the game, or not yet built and tested, stays in `to_do_list.md`.
- Also moved here (rule widened 2026-10-15): anything **resolved with nothing left to do** - a settled decision or question, a deleted file, a finished cleanup.
- If follow-up work on a finished item is still planned, the entry says where in `to_do_list.md` that work is logged.
- Newest entries go at the top of their section.
- Entries keep the number they had in `to_do_list.md`, so older notes that say "item 7" still make sense.

---

## Decisions resolved

### Bucket A #1. Gender selection
- **Moved here**: 2026-10-15
- ✅ **Resolved (2026-10-05)** Character-creation Gender selection has no stat/mechanical difference between genders - purely cosmetic/narrative.
- **Follow-up still in `to_do_list.md`**: building the selection itself (Tier 4).

### Bucket A #6. Princess (final boss) mechanic
- **Moved here**: 2026-10-15
- ✅ **Resolved (2026-10-14)** Princess (final boss) - her unique mechanic is decided, and it replaces the earlier "randomly use one of the other bosses' mechanics" idea.
- **Follow-up still in `to_do_list.md`**: the full design, its open points and the build work are under Tier 2 #9.

### Fight continues while a party member stands (`combat/` review)
- **Moved here**: 2026-10-15
- ✅ **Decided 2026-10-14**: when the player falls and companions are still standing, the fight continues. Lost = the whole party is down.
- **Follow-up still in `to_do_list.md`**: written in the "Combat code pass" under `game_logic/combat/`, not built or tested yet.

### Flee chance replaces the flat 30% (`combat/` review)
- **Moved here**: 2026-10-15
- ✅ **Decided 2026-10-14**: the flat 30% flee chance for everything is replaced by a chance per enemy type, with no fleeing from bosses.
- **Follow-up still in `to_do_list.md`**: written in the "Combat code pass" under `game_logic/combat/`, not built or tested yet.

## Code Cleanup

### Order-based boss progression (Tier 0)
- **Moved here**: 2026-10-15
- ✅ **Done (2026-09-30)** Remove the Champion Key/Final Gate system, replace with order-based progression (defeating boss N-1 unlocks boss N). `BossManager` now has `IsBossUnlocked`/`GetNextBoss`/`GetFinalBoss`/`IsFinalBoss` derived from registration order instead of keys/randomness.
- ✅ **Done (2026-09-30)** "Champion" → "Boss" terminology cleanup across the boss/quest system (`boss_manager.cs`, `boss_enemy.cs`, `boss_definitions.cs`, `boss_encounter.cs`, `final_boss_quest.cs`, `boss_defeat_quest.cs`, `quest_giver.cs`).

### Deleted files (code review, 2026-10-09)
- **Moved here**: 2026-10-15
- `world/dungeon.cs` - **DELETED 2026-10-09**. Dungeons are out of scope, including for future versions. Nothing referenced it.
- `data/game_data.cs` - **DELETED 2026-10-09** (362 lines). `GameDatabase` was an older, second item/enemy/companion/ability database, unused even by tests and superseded by `ItemDatabase` and the real factories. Its entries in `RNGTheGame.csproj` and the README tree were removed too.
- `data/constants.cs` - **DELETED 2026-10-09** (11 lines). Nothing used it; the same numbers are defined where they are needed elsewhere in the code, and a separate constants file is not wanted.

### README project tree
- **Moved here**: 2026-10-15
- ✅ The README's "Project Structure" tree listed a `world/location_type.cs` that does not exist and had not been checked against the other folders. Brought up to date 2026-10-13 and kept current since.
- **Follow-up still in `to_do_list.md`**: the rest of the README (features, status, test count) still overstates what works (root files review).

## GUI & Game Engine

### 3. Get Godot
- **Moved here**: 2026-10-13
- **Status**: ✅ Done (installed, Godot 4.7.2 mono build)
- **Phase**: GUI Development

---

## Session History (2025-12-05) - what was written, NOT current status
- **Moved here**: 2026-10-15

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
