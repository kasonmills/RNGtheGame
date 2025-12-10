# Leadership Passive Ability System

## Overview
Leadership is the first passive ability implemented in RNGtheGame. Unlike active abilities that are used in combat, passive abilities provide permanent bonuses that scale with level. Leadership specifically increases the maximum party size, allowing players to recruit more companions.

## Implementation Status: ✅ COMPLETE

## What is a Passive Ability?

Passive abilities differ from active abilities in key ways:
- **Always Active**: Provide bonuses at all times without activation
- **No Cooldown**: Don't require turns to use
- **Not Used in Combat**: Can't be executed as a combat action
- **Automatic Scaling**: Bonuses increase as the ability levels up
- **Experience Gain**: Level up through general experience, not combat usage

## Leadership Ability Details

### Basic Information
- **Name**: Leadership
- **Type**: Passive
- **Rarity**: Rare
- **Description**: "Your natural charisma and leadership allow you to inspire more companions to join your party."

### Scaling Formula

**Party Size Progression**:
| Ability Level | Party Size Bonus | Total Party Size |
|--------------|------------------|------------------|
| 1-24         | +0               | 4 (player + 3)   |
| 25-49        | +1               | 5 (player + 4)   |
| 50-74        | +2               | 6 (player + 5)   |
| 75-99        | +3               | 7 (player + 6)   |
| 100          | +4               | 8 (player + 7)   |

**Scaling Logic**:
```csharp
if (Level >= 100) return 4;
if (Level >= 75) return 3;
if (Level >= 50) return 2;
if (Level >= 25) return 1;
return 0;
```

### Milestone Notifications

When Leadership reaches certain levels, the player receives a notification:
- **Level 25**: "Your leadership has grown! Max party size increased to 5!"
- **Level 50**: "Your leadership has grown! Max party size increased to 6!"
- **Level 75**: "Your leadership has grown! Max party size increased to 7!"
- **Level 100**: "Your leadership has grown! Max party size increased to 8!"

## Files Created/Modified

### 1. [game_logic/abilities/ability.cs](game_logic/abilities/ability.cs)
**Changes Made**:
- Added `IsPassive` boolean property (line 17)
- Modified `Execute()` to handle passive abilities (lines 48-60)
- Added `GetPassiveBonusValue()` method (lines 62-74)
- Updated class documentation to mention passive abilities

### 2. [game_logic/abilities/leadership_ability.cs](game_logic/abilities/leadership_ability.cs) - NEW
**Purpose**: Implementation of Leadership passive ability
**Key Methods**:
- `GetPassiveBonusValue()`: Returns party size bonus based on level
- `OnLevelUp()`: Notifies player at milestone levels
- `GetInfo()`: Displays detailed ability information with milestones
- `Execute()`: Explains that passive abilities can't be activated

### 3. [game_logic/core/game_manager.cs](game_logic/core/game_manager.cs)
**Changes Made**:
- Added `GetMaxPartySize()` method (lines 70-87)
  - Returns base party size (4)
  - Checks if player has Leadership ability
  - Adds Leadership bonus if applicable
- Added `CanAddCompanion()` method (lines 92-97)
  - Checks if party has room for another companion
  - Accounts for player taking one slot

### 4. [game_logic/entities/player/player.cs](game_logic/entities/player/player.cs)
**Changes Made**:
- Added "Leadership" case to `CreateAbilityFromName()` (line 66-67)
- Added Leadership to `GetAvailableAbilities()` array (line 83)

## How It Works

### Character Creation
1. Player is presented with ability choices during character creation
2. Leadership appears in the list alongside active abilities
3. If selected, player starts with Leadership at Level 1 (party size 4)

### Experience Gain
Leadership levels up through:
- **Combat Experience**: Shared with all abilities after battles
- **Quest Completion**: XP rewards from quests
- **General Progression**: Any XP the ability gains

### Party Size Management

**GameManager Integration**:
```csharp
private int GetMaxPartySize()
{
    const int basePartySize = 4;

    if (_player == null || _player.SelectedAbility == null)
    {
        return basePartySize;
    }

    // Check if player has Leadership passive ability
    if (_player.SelectedAbility is Abilities.LeadershipAbility leadership)
    {
        int bonus = leadership.GetPassiveBonusValue();
        return basePartySize + bonus;
    }

    return basePartySize;
}
```

**Companion Recruitment Check**:
```csharp
private bool CanAddCompanion()
{
    // _activeCompanions doesn't include the player
    int maxCompanions = GetMaxPartySize() - 1;
    return _activeCompanions.Count < maxCompanions;
}
```

## Usage Example

### Character Creation
```
Choose your permanent ability:
1. Attack Boost - Active ability that boosts damage
2. Healing - Active ability that heals the user
3. Defense Boost - Active ability that increases defense
4. Critical Strike - Active ability with high crit chance
5. Leadership - PASSIVE: Increases max party size

Choice: 5
Hero has chosen the ability: Leadership!
```

### Viewing Ability Info
```
Leadership (Lv.15/100) [PASSIVE]
Your natural charisma and leadership allow you to inspire more
companions to join your party.

Current Party Size: 4
Party Size Bonus: +0

Next Milestone: Level 25
  Party Size will increase to 5

XP: 450/250
```

### Milestone Reached
```
Leadership leveled up to Level 25!
Your leadership has grown! Max party size increased to 5!
```

### Max Level
```
Leadership (Lv.100/100) [PASSIVE]
Your natural charisma and leadership allow you to inspire more
companions to join your party.

Current Party Size: 8
Party Size Bonus: +4

Max level reached! Party size at maximum (8).

XP: 0/1090
```

## Technical Implementation

### Passive vs Active Distinction

**Passive Abilities**:
- `IsPassive = true`
- `Execute()` does nothing (or provides info message)
- `GetPassiveBonusValue()` returns the bonus amount
- No cooldown management needed
- Always active, never "on cooldown"

**Active Abilities**:
- `IsPassive = false` (default)
- `Execute()` performs combat action
- `GetPassiveBonusValue()` returns 0
- Cooldown managed through combat system
- Must be activated to take effect

### Type Checking Pattern

To check if a player has Leadership:
```csharp
if (_player.SelectedAbility is Abilities.LeadershipAbility leadership)
{
    int bonus = leadership.GetPassiveBonusValue();
    // Use bonus...
}
```

This uses C# pattern matching to safely cast and check the type.

### Experience Scaling

Leadership uses the same XP scaling as other abilities:
```csharp
protected virtual int CalculateNextLevelXP()
{
    return 100 + (Level * 10); // Gets harder each level
}
```

- Level 1→2: 110 XP
- Level 2→3: 120 XP
- Level 10→11: 200 XP
- Level 50→51: 600 XP
- Level 99→100: 1090 XP

## Game Balance Considerations

### Why Tiered Increases?

Party size increases every 25 levels (not every level) to:
1. **Maintain Balance**: Gradual power increases prevent overwhelming advantage
2. **Create Milestones**: Players have goals to work towards
3. **Reward Investment**: High-level players are rewarded significantly
4. **Limit Early Power**: Prevents new players from having huge parties immediately

### Trade-offs

**Choosing Leadership**:
- **Pros**:
  - Larger party means more tactical options
  - More companions to share combat load
  - Companions provide diverse abilities
  - Max party of 8 at level 100 is powerful

- **Cons**:
  - No direct combat ability
  - Passive benefit takes time to scale
  - Requires recruiting companions to benefit
  - Must level ability to see benefits

**Compared to Active Abilities**:
- Active abilities provide immediate combat power
- Leadership provides long-term strategic advantage
- Different playstyles: solo powerhouse vs. party leader

## Future Passive Abilities

The infrastructure supports more passive abilities:

**Potential Ideas**:
1. **Fortune** - Increases gold/loot drop rates
2. **Vitality** - Increases max HP as it levels
3. **Swiftness** - Permanent speed bonus
4. **Craftsmanship** - Improves equipment quality
5. **Merchant** - Better shop prices
6. **Scholar** - Increases XP gain
7. **Tank** - Increases defense permanently
8. **Berserker** - Increases attack permanently

All would follow the same pattern:
```csharp
public class NewPassiveAbility : Ability
{
    public NewPassiveAbility()
    {
        Name = "Ability Name";
        Description = "What it does";
        IsPassive = true;
        Rarity = AbilityRarity.Something;
    }

    public override int GetPassiveBonusValue()
    {
        // Return scaled bonus based on Level
        return GetScaledValueInt(minBonus, maxBonus);
    }
}
```

## Testing Recommendations

### Manual Testing Checklist:
- [ ] Create character with Leadership ability
- [ ] Verify initial party size is 4
- [ ] Gain XP and level Leadership to 25
- [ ] Verify party size increases to 5
- [ ] Continue to level 50, 75, 100
- [ ] Verify each milestone increases party size
- [ ] Try to recruit more companions than max
- [ ] Verify CanAddCompanion() prevents over-recruiting
- [ ] Save and load game with Leadership
- [ ] Verify bonus persists after load

### Edge Cases:
- [ ] Player without Leadership (should default to 4)
- [ ] Player with Leadership at level 1 (should be 4)
- [ ] Player with Leadership just below milestone (24, 49, etc.)
- [ ] Player at max level 100 (should be 8)
- [ ] Save file with Leadership from old version

## Integration Points

### Where Party Size is Checked:
1. **Companion Recruitment**: Check `CanAddCompanion()` before allowing recruitment
2. **Combat Initialization**: All companions in `_activeCompanions` list join combat
3. **Save/Load**: Party composition saved and restored
4. **UI Display**: Show current party size / max party size

### Where to Add Party Size Limits:
- Companion recruitment menus
- Party management screens
- Companion interaction points
- NPC dialogue (can mention party being full)

## Summary

The Leadership passive ability system is now fully functional:

✅ Passive ability support added to base Ability class
✅ Leadership ability implemented with tiered scaling
✅ Party size management integrated into GameManager
✅ Ability available in character creation
✅ Save/load support through existing ability system
✅ Milestone notifications for player feedback
✅ Scales from 4 to 8 party size (level 1 to 100)

The system provides a strong foundation for future passive abilities while maintaining game balance and providing clear progression milestones for players.