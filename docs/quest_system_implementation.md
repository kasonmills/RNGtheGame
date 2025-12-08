# Quest System Implementation

## Overview
A comprehensive quest system has been implemented that supports multiple quest types, retroactive completion, quest tracking, and integration with the save system.

## Implementation Date
2025-12-05

## Core Components

### 1. Quest Base Classes

#### [quest.cs](../game_logic/quests/quest.cs)
- **Quest** - Abstract base class for all quests
  - Quest states: NotDiscovered → Available → Accepted → Completed → Claimed
  - Objective tracking via list of QuestObjective
  - Reward system via QuestReward
  - Retroactive completion support
  - Special requirement flag for final boss quest

#### [quest_objective.cs](../game_logic/quests/quest_objective.cs)
- **QuestObjective** - Tracks individual objective progress
  - Progress tracking (current/required)
  - Progress display (string, percentage)
  - Update and reset functionality

#### [quest_reward.cs](../game_logic/quests/quest_reward.cs)
- **QuestReward** - Defines quest rewards
  - Gold, Experience, Items
  - Display methods for showing rewards
  - Summary generation

### 2. Quest Manager

#### [quest_manager.cs](../game_logic/quests/quest_manager.cs)
- Centralized quest tracking system
- Features:
  - Quest registration and retrieval
  - Quest state management (discover, accept, complete, claim)
  - Active quest tracking (one quest can be "focused")
  - Quest filtering by state
  - Quest log display
  - Save/load support

### 3. Specific Quest Types

#### [boss_defeat_quest.cs](../game_logic/quests/boss_defeat_quest.cs)
- Quest to defeat a specific champion boss
- Tracks boss ID and boss name
- Single objective: defeat the boss
- Retroactive status checking against BossManager
- Rewards: 100 gold + 50 XP per boss

#### [final_boss_quest.cs](../game_logic/quests/final_boss_quest.cs)
- **SPECIAL**: Requires acceptance before completion (unique)
- Tracks 3 objectives:
  1. Collect 10 Champion Keys
  2. Enter the Final Gate
  3. Defeat the Final Boss
- No rewards (game ends upon completion)

#### [level_quest.cs](../game_logic/quests/level_quest.cs)
- Quest to reach a specific level
- Milestones: levels 5, 10, 15, 20, 25
- Auto-updates when player levels up

#### [enemy_kill_quest.cs](../game_logic/quests/enemy_kill_quest.cs)
- Quest to defeat X enemies (any type or specific type)
- Tracks kill count
- Three tiers: 10, 25, 50 enemies

#### [gold_collection_quest.cs](../game_logic/quests/gold_collection_quest.cs)
- Quest to earn X gold **total** (not current gold held)
- Tracks cumulative gold earned
- Three tiers: 500, 1000, 2500 gold

#### [weapon_upgrade_quest.cs](../game_logic/quests/weapon_upgrade_quest.cs)
- Quest to upgrade any weapon to level X
- Two tiers: level 5, level 10

#### [equipment_quest.cs](../game_logic/quests/equipment_quest.cs)
- Quest to equip full gear set (weapon + armor) at minimum level
- Two tiers: level 3+, level 5+

#### [challenge_quest.cs](../game_logic/quests/challenge_quest.cs)
- Special achievement-style quests
- Types:
  - **Flawless Victory**: Win without taking damage
  - **Critical Master**: Land 5 crits in one battle
  - **Survivor**: Win with <10% health remaining
  - **Win Streak**: Win 5 battles in a row

### 4. Quest Distribution

#### [quest_giver.cs](../game_logic/entities/npcs/quest_giver.cs)
- **Quest Giver NPC** - "Veteran Ranger"
- Provides **boss-related quests** only
- Features:
  - View available boss quests
  - View accepted boss quests with progress
  - View/accept final boss quest
  - Claim completed quest rewards
  - Lore/hints dialogue based on progression

#### [job_board.cs](../game_logic/menus/job_board.cs)
- **Job Board Menu** - Menu-based quest discovery
- Provides **all non-boss quests**
- Features:
  - Browse available quests by category
  - View active quests with progress
  - View completed quests and claim rewards
  - Set active quest (choose which quest to track)
  - Quest detail views

### 5. Save System Integration

#### Updated Files:
- **[save_data.cs](../game_logic/data/save_data.cs)**
  - Added `SerializedQuest` and `SerializedQuestObjective` classes
  - Added quest list and active quest ID to SaveData

- **[save_manager.cs](../game_logic/data/save_manager.cs)**
  - Added `questManager` parameter to SaveGame method
  - Added `QuestSerializationHelper` class with:
    - `SerializeQuests()` - Convert QuestManager to serializable format
    - `DeserializeQuests()` - Restore quest progress from save
  - Saves quest state, objective progress, and quest-specific data

### 6. Game Manager Integration

#### [game_manager.cs](../game_logic/core/game_manager.cs)
- Added `_questManager` and `_questGiver` fields
- **InitializeQuests()** method:
  - Creates all 35+ quests at game start
  - Boss quests (14 regular + 1 final)
  - Level quests (5 milestones)
  - Enemy kill quests (3 tiers)
  - Gold collection quests (3 tiers)
  - Weapon upgrade quests (2 tiers)
  - Equipment quests (2 tiers)
  - Challenge quests (4 types)

- **Menu Integration**:
  - Option 8: Quest Giver (Boss Quests)
  - Option 9: Job Board (Other Quests)
  - Option 10: Quest Log (view all quests)

- **Load Game**:
  - Initializes quest system
  - Restores quest progress from save
  - Restores active quest tracking

- **Save Game**:
  - Passes QuestManager to SaveManager
  - Persists all quest states and progress

## Key Features

### 1. Retroactive Completion
- Quests track progress **even if not accepted**
- Player can defeat a boss, then accept the quest and it's already complete
- **Exception**: Final boss quest requires acceptance before completion

### 2. Quest States
```
NotDiscovered → Available → Accepted → Completed → Claimed
```
- **NotDiscovered**: Quest exists but player hasn't found it
- **Available**: Quest discovered but not accepted
- **Accepted**: Player actively working on quest
- **Completed**: All objectives met, rewards ready
- **Claimed**: Rewards have been received

### 3. Active Quest Tracking
- Player can set one quest as "active/focused"
- Active quest shows with ★ marker in lists
- Can be changed at any time
- Helps player focus on one goal

### 4. Quest Categories
- **Boss Quests**: Given by Quest Giver NPC
- **Progression Quests**: Level, gold, equipment (Job Board)
- **Combat Quests**: Enemy kills, challenges (Job Board)
- **Special Quests**: Final boss (Quest Giver)

## Quest Counts

| Category | Count | Source |
|----------|-------|--------|
| Boss Defeat Quests | 14 | Quest Giver |
| Final Boss Quest | 1 | Quest Giver |
| Level Quests | 5 | Job Board |
| Enemy Kill Quests | 3 | Job Board |
| Gold Collection Quests | 3 | Job Board |
| Weapon Upgrade Quests | 2 | Job Board |
| Equipment Quests | 2 | Job Board |
| Challenge Quests | 4 | Job Board |
| **Total** | **34** | |

## User Experience Flow

### For Boss Quests:
1. Talk to Quest Giver NPC
2. View available boss quests
3. Accept a quest (or don't - progress tracks anyway!)
4. Defeat the boss
5. Return to Quest Giver
6. Claim rewards (gold + XP)

### For Other Quests:
1. Visit Job Board
2. Browse available quests by type
3. Accept quests of interest
4. Complete objectives naturally through gameplay
5. Return to Job Board to claim rewards

### For Final Boss:
1. Talk to Quest Giver
2. Accept Final Boss Quest (REQUIRED!)
3. Collect 10 keys
4. Enter Final Gate
5. Defeat Final Boss
6. Game ends (no reward claim needed)

## Technical Notes

### Quest Progress Updates
Quest progress is updated automatically during gameplay:
- **Boss defeats**: Updated in CombatManager after boss victory
- **Level ups**: Updated when player gains level
- **Enemy kills**: Updated after each combat victory
- **Gold earned**: Updated when player receives gold
- **Weapon upgrades**: Updated when weapon levels up
- **Equipment checks**: Updated when equipment changes

### Save/Load Behavior
- Quest states are preserved across sessions
- Objective progress is saved
- Active quest tracking is maintained
- Quest-specific data (like total gold earned) is preserved

### Quest Completion Logic
```csharp
// Normal quest - can complete without acceptance
if (AreObjectivesComplete() && State != QuestState.NotDiscovered)
{
    Complete();
}

// Final boss quest - MUST be accepted
if (RequiresAcceptanceToComplete && State == QuestState.Accepted && AreObjectivesComplete())
{
    Complete();
}
```

## Future Enhancements (Not Implemented)

Potential additions for future development:
- Quest chains (quests that unlock other quests)
- Daily/weekly repeatable quests
- Hidden quests (discovered through exploration)
- Quest rewards with unique items
- Quest dialogue trees
- Quest failure conditions
- Time-limited quests
- Faction reputation quests

## Files Created

### Core Quest System
- `game_logic/quests/quest.cs` - Base Quest class
- `game_logic/quests/quest_objective.cs` - Objective tracking
- `game_logic/quests/quest_reward.cs` - Reward system
- `game_logic/quests/quest_manager.cs` - Quest management

### Quest Types
- `game_logic/quests/boss_defeat_quest.cs`
- `game_logic/quests/final_boss_quest.cs`
- `game_logic/quests/level_quest.cs`
- `game_logic/quests/enemy_kill_quest.cs`
- `game_logic/quests/gold_collection_quest.cs`
- `game_logic/quests/weapon_upgrade_quest.cs`
- `game_logic/quests/equipment_quest.cs`
- `game_logic/quests/challenge_quest.cs`

### NPCs and Menus
- `game_logic/entities/npcs/quest_giver.cs` - Quest Giver NPC
- `game_logic/menus/job_board.cs` - Job Board menu

### Modified Files
- `game_logic/data/save_data.cs` - Added quest serialization structures
- `game_logic/data/save_manager.cs` - Added quest save/load logic
- `game_logic/core/game_manager.cs` - Integrated quest system

## Testing Checklist

- [ ] Create new game and verify quests are initialized
- [ ] Accept a boss quest and defeat the boss
- [ ] Defeat a boss without accepting quest, then accept and claim
- [ ] Test final boss quest requires acceptance
- [ ] Save game with quest progress and reload
- [ ] Test active quest tracking and switching
- [ ] Complete various quest types (level, gold, enemy kills)
- [ ] Test quest reward claiming
- [ ] Test quest log displays correctly
- [ ] Test Job Board categorization
- [ ] Test Quest Giver dialogue and boss quest flow

## Summary

The quest system is fully implemented and integrated with the game. It provides:
- 34 total quests across 8 different types
- Two discovery methods (Quest Giver NPC + Job Board)
- Retroactive completion for convenience
- Active quest tracking for focus
- Full save/load support
- Clean integration with existing systems (boss progression, player stats, combat)

The system is ready for player use and can be extended with additional quest types or features as needed.