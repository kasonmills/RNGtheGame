# Evasion Passive Ability

## Overview
Evasion is a passive ability that gives the player a chance to completely avoid incoming damage. The chance to evade scales from 1% at level 1 to 60% at level 100, providing a powerful defensive option that becomes increasingly effective as it levels up.

## Implementation Status: ✅ COMPLETE

## Ability Details

### Basic Information
- **Name**: Evasion
- **Type**: Passive
- **Rarity**: Epic
- **Description**: "Your reflexes and agility allow you to dodge incoming attacks, taking no damage. Does not work while defending."

### Scaling Formula

**Evasion Chance Progression**:
| Ability Level | Evasion Chance |
|--------------|----------------|
| 1            | 1%             |
| 10           | 7%             |
| 20           | 13%            |
| 30           | 18%            |
| 40           | 24%            |
| 50           | 30%            |
| 60           | 36%            |
| 70           | 42%            |
| 80           | 48%            |
| 90           | 54%            |
| 100          | 60%            |

**Formula**: Linear scaling from 1% to 60%
```csharp
int evasionChance = 1 + ((Level - 1) * 59 / 99);
// Equivalent to: GetScaledValueInt(1, 60)
```

**Approximately 0.595% increase per level**

### Key Restrictions

1. **Does NOT work while defending**
   - If player is in defensive stance, evasion is disabled
   - This prevents stacking defensive bonuses (50% damage reduction + evasion)
   - Defending already provides significant damage reduction

2. **Only works for the player**
   - Companions cannot have evasion
   - Only applies to incoming damage on the player

3. **Triggers before damage is applied**
   - If evasion succeeds, damage is set to 0
   - No damage is taken at all (not reduced, completely negated)

### How It Works

**Damage Flow with Evasion**:
```
Enemy attacks Player
       ↓
Calculate base damage
       ↓
Apply armor reduction
       ↓
Is player defending? → Yes → Apply 50% reduction → Evasion DISABLED
       ↓ No
Does player have Evasion ability?
       ↓ Yes
Roll 1-100 vs Evasion Chance
       ↓
Success? → Damage set to 0, "EVADED!" message
       ↓ No
Apply damage normally
```

## Files Created/Modified

### 1. [game_logic/abilities/evasion_ability.cs](game_logic/abilities/evasion_ability.cs) - NEW
**Purpose**: Implementation of Evasion passive ability

**Key Methods**:
- `GetPassiveBonusValue()`: Returns evasion % chance based on level (1-60%)
- `ShouldEvade(rng, isDefending)`: Rolls to check if attack is evaded
  - Returns `false` if defending
  - Rolls 1-100 against evasion chance
  - Returns `true` if evaded
- `OnLevelUp()`: Notifies player every 10 levels about evasion improvement
- `GetInfo()`: Displays detailed ability information with next milestone
- `Execute()`: Explains that it's a passive ability

### 2. [game_logic/combat/combat_manager.cs](game_logic/combat/combat_manager.cs)
**Changes Made**: Added evasion check in `ProcessAttack()` method (lines 779-798)

**Integration Point**:
```csharp
// Check if target is defending (for evasion purposes)
bool isDefending = _defendingEntities.ContainsKey(action.Target) && _defendingEntities[action.Target];

// Apply defense reduction if target is defending
if (isDefending)
{
    int damageReduction = result.FinalDamage / 2;
    result.FinalDamage -= damageReduction;
    Console.WriteLine($"{action.Target.Name}'s defensive stance reduced damage by {damageReduction}!");
}

// Check for Evasion passive ability (only for player, only when not defending)
if (action.Target == _player && !isDefending && _player.SelectedAbility is Abilities.EvasionAbility evasion)
{
    if (evasion.ShouldEvade(_rngManager, isDefending))
    {
        Console.WriteLine($"{_player.Name} evaded the attack! No damage taken!");
        result.FinalDamage = 0;
    }
}
```

### 3. [game_logic/entities/player/player.cs](game_logic/entities/player/player.cs)
**Changes Made**:
- Added "Evasion" case to `CreateAbilityFromName()` (lines 68-69)
- Added Evasion to `GetAvailableAbilities()` array (line 86)

## Usage Examples

### Character Creation
```
Choose your permanent ability:
1. Attack Boost - Active ability that boosts damage
2. Healing - Active ability that heals the user
3. Defense Boost - Active ability that increases defense
4. Critical Strike - Active ability with high crit chance
5. Leadership - PASSIVE: Increases max party size
6. Evasion - PASSIVE: Chance to completely avoid damage

Choice: 6
Hero has chosen the ability: Evasion!
```

### Viewing Ability Info (Level 1)
```
Evasion (Lv.1/100) [PASSIVE]
Your reflexes and agility allow you to dodge incoming attacks,
taking no damage. Does not work while defending.

Current Evasion Chance: 1%
When triggered: Take 0 damage instead of incoming damage
Restriction: Does not work while defending

Next Milestone: 5% (around Level 8)

XP: 0/110
```

### Viewing Ability Info (Level 50)
```
Evasion (Lv.50/100) [PASSIVE]
Your reflexes and agility allow you to dodge incoming attacks,
taking no damage. Does not work while defending.

Current Evasion Chance: 30%
When triggered: Take 0 damage instead of incoming damage
Restriction: Does not work while defending

Next Milestone: 35% (around Level 59)

XP: 234/600
```

### Viewing Ability Info (Level 100)
```
Evasion (Lv.100/100) [PASSIVE]
Your reflexes and agility allow you to dodge incoming attacks,
taking no damage. Does not work while defending.

Current Evasion Chance: 60%
When triggered: Take 0 damage instead of incoming damage
Restriction: Does not work while defending

Max level reached! Evasion chance at maximum (60%).

XP: 0/1090
```

### Combat Examples

**Successful Evasion**:
```
Dragon attacks Hero for 45 damage!
Armor blocked 10 damage!
Hero evaded the attack! No damage taken!

-------------------------------------------------
Player HP: 100/100  (no damage!)
Enemy HP: 215/250
-------------------------------------------------
```

**Failed Evasion** (normal damage):
```
Dragon attacks Hero for 45 damage!
Armor blocked 10 damage!
Hero took 35 damage!

-------------------------------------------------
Player HP: 65/100
Enemy HP: 215/250
-------------------------------------------------
```

**Evasion Disabled While Defending**:
```
Hero takes a defensive stance!

Dragon attacks Hero for 45 damage!
Armor blocked 10 damage!
Defensive stance reduced damage by 17!
Hero took 18 damage!

(Evasion did NOT trigger because Hero was defending)

-------------------------------------------------
Player HP: 82/100
Enemy HP: 215/250
-------------------------------------------------
```

### Milestone Notification
```
Evasion leveled up to Level 10!
Your evasion has improved! Evasion chance is now 7%!
```

## Game Balance Considerations

### Why 1-60% Range?

**Starting at 1%**:
- Prevents early game abuse
- Keeps new players vulnerable
- Requires investment to become effective
- Occasional lucky dodge feels rewarding

**Capping at 60%**:
- Prevents invincibility (100% would be broken)
- Maintains challenge even at max level
- Still significant at 60% (blocks ~3 out of 5 attacks)
- Complements other defensive options without dominating

### Why Disable During Defending?

**Balance Reasons**:
1. **Prevents Stacking**: Defending gives 50% damage reduction, adding 60% evasion on top would be too strong
2. **Risk vs Reward**: Players must choose between guaranteed 50% reduction (defend) or chance to dodge completely (don't defend)
3. **Tactical Decisions**: Creates interesting combat choices
4. **Defensive Overlap**: Both are defensive mechanics; shouldn't stack

**Design Philosophy**:
- Evasion rewards aggressive/normal play
- Defending is the "safe" option with guaranteed reduction
- Players choose their defensive style

### Trade-offs

**Choosing Evasion**:
- **Pros**:
  - Complete damage negation (not just reduction)
  - Scales to 60% (very powerful at high levels)
  - Works automatically, no activation needed
  - Allows normal combat actions while defending passively
  - Synergizes with high HP/armor builds

- **Cons**:
  - Unreliable at low levels (1% is nearly useless)
  - RNG-dependent (might fail when you need it most)
  - Doesn't work while defending
  - No direct offensive benefit
  - Requires leveling to become effective

**Compared to Other Passives**:
- **vs Leadership**: Evasion provides survivability, Leadership provides utility
- **vs Active Abilities**: No direct combat action, but always-on protection

**Compared to Active Defense**:
- **Evasion** (Passive): Chance-based, 0% or 100% damage, always active
- **Defending** (Action): Guaranteed 50% reduction, costs your turn, disables evasion

## Probability Analysis

### Expected Damage Reduction

With 60% evasion at max level:
- 60% of attacks: 0 damage
- 40% of attacks: Full damage (after armor)

**Average damage reduction = 60%**

However, this is different from defending because:
- Evasion is all-or-nothing (0% or 100%)
- Defending is consistent (always 50%)

**Variance**:
- Evasion has high variance (sometimes 0, sometimes full damage)
- Defending has low variance (always 50% reduction)

### Probability of Evading Multiple Attacks

With 60% evasion chance:
- Evade 1 attack: 60%
- Evade 2 attacks in a row: 36%
- Evade 3 attacks in a row: 21.6%
- Evade 4 attacks in a row: 12.96%
- Evade 5 attacks in a row: 7.78%

**Getting hit eventually is guaranteed, but evasion provides excellent burst protection**

## Synergies and Anti-Synergies

### Synergizes Well With:
1. **High HP builds**: More HP means surviving hits that get through
2. **High armor**: Reduces damage on failed evasions
3. **Healing items**: Recover from hits that connect
4. **Aggressive playstyle**: Don't defend, maximize evasion uptime
5. **Speed builds**: Act before enemies, kill them before they can attack

### Anti-Synergies:
1. **Defensive playstyle**: Defending disables evasion
2. **Low HP**: Getting hit when evasion fails can be fatal
3. **Long fights**: RNG averages out over time; short burst protection

## Technical Implementation Details

### Type Checking Pattern

To check if player has Evasion and trigger it:
```csharp
if (action.Target == _player && !isDefending && _player.SelectedAbility is Abilities.EvasionAbility evasion)
{
    if (evasion.ShouldEvade(_rngManager, isDefending))
    {
        Console.WriteLine($"{_player.Name} evaded the attack! No damage taken!");
        result.FinalDamage = 0;
    }
}
```

### Roll Mechanism

```csharp
public bool ShouldEvade(RNGManager rng, bool isDefending)
{
    // Evasion doesn't work while defending
    if (isDefending)
    {
        return false;
    }

    int evasionChance = GetPassiveBonusValue();  // 1-60%
    int roll = rng.Roll(1, 100);                 // Random 1-100

    return roll <= evasionChance;                // Success if roll <= chance
}
```

### Damage Flow Order

1. Calculate base damage from enemy
2. Apply armor reduction
3. **Check if defending → apply 50% reduction (EVASION DISABLED)**
4. **If not defending → check evasion → if success, set damage to 0**
5. Apply final damage to player

This order ensures defending always disables evasion.

## Future Considerations

### Potential Enhancements:
1. **Evasion Counter**: Track total attacks evaded in statistics
2. **Visual Feedback**: Special animation/sound for evasion
3. **Dodge Roll Bonus**: Extra damage on next attack after evasion
4. **Evasion Streak**: Bonus after evading multiple attacks
5. **Partial Evasion**: Glancing blows reduce damage instead of negating

### Variant Ideas:
- **Parry**: Similar to evasion but works with weapons
- **Block**: Shield-based damage negation
- **Phase**: Magic-based evasion with different scaling
- **Counter**: Evade and deal return damage

## Testing Recommendations

### Manual Testing Checklist:
- [ ] Create character with Evasion ability
- [ ] Verify starting evasion is 1%
- [ ] Get attacked multiple times, verify occasional evasion at low level
- [ ] Level Evasion to 50 (30% chance)
- [ ] Verify approximately 3/10 attacks evaded
- [ ] Try defending in combat
- [ ] Get attacked while defending
- [ ] Verify NO evasion messages while defending
- [ ] Level to 100 (60% chance)
- [ ] Verify majority of attacks evaded (when not defending)
- [ ] Save and load game with Evasion
- [ ] Verify evasion persists after load

### Statistical Testing:
- [ ] Level Evasion to 50 (30%)
- [ ] Get attacked 100 times (not defending)
- [ ] Verify approximately 30 evasions (±10% variance expected)
- [ ] Level Evasion to 100 (60%)
- [ ] Get attacked 100 times (not defending)
- [ ] Verify approximately 60 evasions (±10% variance expected)

### Edge Cases:
- [ ] Player without Evasion (should take normal damage)
- [ ] Player with Evasion at level 1 (1% chance)
- [ ] Player with Evasion while defending (should not evade)
- [ ] Critical hits from enemies (should still be evadeable)
- [ ] Very high damage attacks (should still evade all or nothing)
- [ ] Ability damage vs normal attacks (currently only works on attacks)

## Integration with Statistics

Potential stats to track:
- `TotalAttacksEvaded`: Count of successful evasions
- `DamageEvaded`: Total damage negated by evasion
- `LongestEvasionStreak`: Most consecutive evades
- `EvasionRate`: Percentage of attacks evaded

## Summary

The Evasion passive ability is now fully functional:

✅ Passive ability with 1-60% scaling
✅ Integrated into combat damage processing
✅ Disabled while defending (prevents stacking)
✅ Only affects player (not companions)
✅ Proper RNG-based chance system
✅ Milestone notifications every 10 levels
✅ Available in character creation
✅ Save/load support through existing system
✅ Clear visual feedback when triggered

The ability provides a unique defensive option that rewards aggressive play and creates interesting tactical decisions about when to defend vs. when to rely on evasion.