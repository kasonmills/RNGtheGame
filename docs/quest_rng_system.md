# Quest RNG System

## Overview
Job Board quests now feature randomized requirements and rewards, making each save file unique. Boss quests remain fixed to ensure consistent progression.

## Implementation Date
2025-12-05

## RNG Features

### 1. Randomized Requirements

Job Board quests have variable requirements determined at save file creation:

| Quest Type | Base Requirement | Variance | Range |
|------------|-----------------|----------|-------|
| Enemy Kill (Tier 1) | 10 enemies | ±6 | 4-16 |
| Enemy Kill (Tier 2) | 25 enemies | ±6 | 19-31 |
| Enemy Kill (Tier 3) | 50 enemies | ±6 | 44-56 |
| Gold Collection (Tier 1) | 500 gold | ±150 | 350-650 |
| Gold Collection (Tier 2) | 1000 gold | ±150 | 850-1150 |
| Gold Collection (Tier 3) | 2500 gold | ±300 | 2200-2800 |
| Weapon Upgrade (Tier 1) | Level 5 | ±2 | 3-7 |
| Weapon Upgrade (Tier 2) | Level 10 | ±3 | 7-13 |
| Equipment (Tier 1) | Level 3 | ±1 | 2-4 |
| Equipment (Tier 2) | Level 5 | ±2 | 3-7 |
| Win Streak Challenge | 5 battles | ±2 | 3-7 |

### 2. Randomized Rewards

All quest rewards have percentage-based variation:

| Quest Type | Reward Variation |
|------------|-----------------|
| Boss Quests | Fixed (no RNG) |
| Level Quests | ±20% |
| All Job Board Quests | ±30% |

**Example:**
- Enemy Kill Quest (Tier 1): Base 75 gold, 40 XP
- With ±30% variation: 52-97 gold, 28-52 XP

### 3. Rewards Scale with Difficulty

When a quest has higher requirements due to RNG, it automatically has proportionally higher rewards, and vice versa. The variation is applied independently to both requirements and rewards, creating natural difficulty/reward scaling.

## Technical Implementation

### Quest Generation (game_manager.cs)

```csharp
// Apply RNG to requirements
private int ApplyRequirementVariation(int baseValue, int maxVariance)
{
    int variation = _rngManager.Roll(-maxVariance, maxVariance);
    return Math.Max(1, baseValue + variation);
}

// Apply RNG to rewards (percentage-based)
private int ApplyRewardVariation(int baseValue, double variancePercent)
{
    int maxVariance = (int)(baseValue * variancePercent);
    int variation = _rngManager.Roll(-maxVariance, maxVariance);
    return Math.Max(1, baseValue + variation);
}
```

### Quest Persistence

Quests are generated **once** per save file and persist through save/load:

**On New Game:**
1. InitializeQuests() is called
2. RNG determines requirements and rewards for each quest
3. Quests are created with those specific values
4. When saved, full quest data (requirements + rewards) is serialized

**On Load Game:**
1. Quest data is read from save file
2. ReconstructQuests() rebuilds quests with original values
3. No new RNG - quests remain identical to when save was created

### Save System (save_data.cs)

SerializedQuest now includes:
- `QuestName` - Quest title
- `Description` - Quest description
- `GoldReward` - Exact gold reward amount
- `XPReward` - Exact XP reward amount
- `Objectives` - List including `RequiredProgress` (the requirement)
- `QuestType` - Quest type for reconstruction

### Quest Reconstruction (save_manager.cs)

The `ReconstructQuests()` method rebuilds quests from saved data:
- Extracts requirements from saved objectives
- Extracts rewards from saved reward values
- Creates new quest instances with exact original values
- Restores progress and state

## Quest Types with RNG

### Fixed (No RNG)
- **Boss Defeat Quests**: Always 100 gold + 50 XP
  - Ensures consistent progression through boss challenges
  - All 14 champion boss quests are identical across save files

- **Final Boss Quest**: No rewards (game ends)
  - Fixed requirements: 10 keys, enter gate, defeat boss

### Randomized Requirements Only
- **Level Quests**: Fixed milestones (5, 10, 15, 20, 25)
  - Requirements don't vary (can't change level milestones)
  - Rewards vary ±20%

### Fully Randomized
- **Enemy Kill Quests**: Requirements ±6, Rewards ±30%
- **Gold Collection Quests**: Requirements ±150-300, Rewards ±30%
- **Weapon Upgrade Quests**: Requirements ±2-3 levels, Rewards ±30%
- **Equipment Quests**: Requirements ±1-2 levels, Rewards ±30%
- **Challenge Quests**:
  - Most are fixed (1 completion)
  - Win Streak: Requirements ±2 battles, Rewards ±30%

## Examples

### Example Save File A

**Enemy Kill Quest (Tier 1)**
- Requirement: 7 enemies (rolled 10 - 3)
- Reward: 84 gold (rolled 75 + 9), 46 XP (rolled 40 + 6)

**Gold Collection Quest (Tier 1)**
- Requirement: 580 gold (rolled 500 + 80)
- Reward: 88 gold (rolled 100 - 12), 42 XP (rolled 50 - 8)

**Win Streak Challenge**
- Requirement: 6 battles (rolled 5 + 1)
- Reward: 205 gold (rolled 250 - 45), 108 XP (rolled 125 - 17)

### Example Save File B

**Enemy Kill Quest (Tier 1)**
- Requirement: 14 enemies (rolled 10 + 4)
- Reward: 68 gold (rolled 75 - 7), 35 XP (rolled 40 - 5)

**Gold Collection Quest (Tier 1)**
- Requirement: 425 gold (rolled 500 - 75)
- Reward: 112 gold (rolled 100 + 12), 58 XP (rolled 50 + 8)

**Win Streak Challenge**
- Requirement: 4 battles (rolled 5 - 1)
- Reward: 272 gold (rolled 250 + 22), 139 XP (rolled 125 + 14)

## Benefits

### Replayability
- Each new game has unique quest objectives
- No two save files have identical job board quests
- Encourages multiple playthroughs

### Natural Difficulty Scaling
- Some saves have easier quests (lower requirements, potentially higher rewards)
- Some saves have harder quests (higher requirements, potentially lower rewards)
- Creates variety in player experience

### Maintained Balance
- Variation ranges are carefully tuned
- Minimum values ensure quests are never trivial
- Boss progression remains consistent across all saves

### Quest Descriptions Update Automatically
- Enemy kill quests show actual requirement in objective
- Win streak challenge description updates with RNG value
- Example: "Win 6 battles in a row" instead of fixed "Win 5 battles"

## Testing the System

To test RNG quest generation:

1. **Create New Save File**
   - Start new game
   - Visit Job Board
   - Note quest requirements and rewards
   - Write down a few examples

2. **Save and Load**
   - Save the game
   - Exit and reload
   - Visit Job Board again
   - Verify quests have SAME requirements/rewards

3. **Create Second Save File**
   - Start another new game
   - Visit Job Board
   - Compare to first save
   - Quests should have DIFFERENT values

4. **Verify Boss Quests**
   - Visit Quest Giver
   - Boss quests should always be 100 gold + 50 XP
   - Should be identical across all save files

## Code Locations

- **Quest Generation**: [game_manager.cs](../game_logic/core/game_manager.cs) lines 867-1038
  - `InitializeQuests()` - Generates quests with RNG
  - `ApplyRequirementVariation()` - Adds variance to requirements
  - `ApplyRewardVariation()` - Adds variance to rewards

- **Quest Serialization**: [save_manager.cs](../game_logic/data/save_manager.cs) lines 583-757
  - `SerializeQuests()` - Saves complete quest data
  - `ReconstructQuests()` - Rebuilds quests from save data

- **Save Data Structure**: [save_data.cs](../game_logic/data/save_data.cs) lines 48-71
  - `SerializedQuest` - Contains all quest details for persistence

## Future Enhancements

Potential additions:
- Daily/weekly quests with fresh RNG each day/week
- Quest difficulty tiers (easy/normal/hard) with different RNG ranges
- Quest reroll system (pay gold to regenerate a quest)
- Seasonal quests with unique RNG patterns
- Quest chains where later quests scale based on earlier completion

## Summary

The quest RNG system adds variety and replayability to the game while maintaining balance and consistency where needed. Boss progression remains predictable, while Job Board quests provide unique challenges in each playthrough. Quest data persists perfectly through save/load, ensuring a consistent experience within each save file while creating variety across different saves.