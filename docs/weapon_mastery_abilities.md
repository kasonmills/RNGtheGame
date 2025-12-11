# Weapon Mastery Abilities

## Overview
Weapon Mastery abilities are passive abilities that provide significant combat bonuses when wielding a specific weapon type. Each weapon type in the game has its own dedicated mastery ability, rewarding players for specializing in their chosen weapon.

## Design Philosophy

### Why Weapon Masteries?

1. **Weapon Specialization**: Encourages players to commit to a weapon type and master it
2. **Build Diversity**: Creates distinct playstyles based on weapon choice
3. **Progression Rewards**: Provides meaningful power increases for dedicated weapon users
4. **Strategic Choice**: Players must choose between mastery and other passive abilities

### Conditional Activation

Unlike other passive abilities that are always active, Weapon Masteries **only provide bonuses when wielding the correct weapon type**. This creates interesting gameplay decisions:

- Switch weapons, lose mastery bonuses (but gain versatility)
- Stay with one weapon, maintain powerful bonuses (but lose flexibility)
- Balance between mastery power and tactical weapon swapping

## Available Weapon Masteries

All weapon masteries follow the same scaling formula:

| Level | Accuracy Bonus | Damage Bonus | Crit Chance Bonus |
|-------|----------------|--------------|-------------------|
| 1     | +1%            | +1%          | +1%               |
| 10    | +4%            | +7%          | +3%               |
| 25    | +10%           | +17%         | +7%               |
| 50    | +20%           | +35%         | +13%              |
| 75    | +30%           | +53%         | +19%              |
| 100   | +40%           | +70%         | +25%              |

### 1. Sword Mastery
- **Rarity**: Rare
- **Weapon Type**: Sword
- **Description**: "Your mastery of swordsmanship grants you enhanced accuracy, damage, and critical strike potential when wielding a sword."

### 2. Axe Mastery
- **Rarity**: Rare
- **Weapon Type**: Axe
- **Description**: "Your expertise with axes grants you enhanced accuracy, damage, and critical strike potential when wielding an axe."

### 3. Mace Mastery
- **Rarity**: Rare
- **Weapon Type**: Mace
- **Description**: "Your proficiency with maces grants you enhanced accuracy, damage, and critical strike potential when wielding a mace."

### 4. Dagger Mastery
- **Rarity**: Rare
- **Weapon Type**: Dagger
- **Description**: "Your skill with daggers grants you enhanced accuracy, damage, and critical strike potential when wielding a dagger."

### 5. Spear Mastery
- **Rarity**: Rare
- **Weapon Type**: Spear
- **Description**: "Your extensive training with spears grants you enhanced accuracy, damage, and critical strike potential when wielding a spear."

### 6. Staff Mastery
- **Rarity**: Rare
- **Weapon Type**: Staff
- **Description**: "Your training with staves grants you enhanced accuracy, damage, and critical strike potential when wielding a staff."

### 7. Bow Mastery
- **Rarity**: Rare
- **Weapon Type**: Bow
- **Description**: "Your archery expertise grants you enhanced accuracy, damage, and critical strike potential when wielding a bow."

### 8. Crossbow Mastery
- **Rarity**: Rare
- **Weapon Type**: Crossbow
- **Description**: "Your expertise with crossbows grants you enhanced accuracy, damage, and critical strike potential when wielding a crossbow."

### 9. Wand Mastery
- **Rarity**: Rare
- **Weapon Type**: Wand
- **Description**: "Your proficiency with wands grants you enhanced accuracy, damage, and critical strike potential when wielding a wand."

## Scaling Breakdown

### Accuracy Scaling
**Formula**: 1% at Level 1 → 40% at Level 100 (linear)

- **Purpose**: Makes attacks more reliable as mastery increases
- **Impact**: At max level, a 70% base accuracy weapon becomes 98% accuracy
- **Example**:
  - Level 1: 75% base accuracy → 75.75% with mastery (+1%)
  - Level 50: 75% base accuracy → 90% with mastery (+20%)
  - Level 100: 75% base accuracy → 105% with mastery (+40%, capped at 100%)

### Damage Scaling
**Formula**: 1% at Level 1 → 70% at Level 100 (linear)

- **Purpose**: Primary power increase for weapon specialists
- **Impact**: Significant DPS boost at high levels
- **Example**:
  - Level 1: 20 base damage → 20.2 damage (+1%)
  - Level 50: 20 base damage → 27 damage (+35%)
  - Level 100: 20 base damage → 34 damage (+70%)

### Critical Chance Scaling
**Formula**: 1% at Level 1 → 25% at Level 100 (linear)

- **Purpose**: Rewards mastery with increased crit frequency
- **Impact**: Flat bonus added to weapon's base crit chance
- **Example**:
  - Level 1: 5% weapon crit → 6% total (+1%)
  - Level 50: 5% weapon crit → 18% total (+13%)
  - Level 100: 5% weapon crit → 30% total (+25%)

## Leveling Requirements

Weapon Masteries use **Rare rarity** XP scaling:

| Level Range | Approximate XP Required |
|-------------|-------------------------|
| 1 → 10      | ~3,000 XP               |
| 1 → 25      | ~10,000 XP              |
| 1 → 50      | ~30,000 XP              |
| 1 → 75      | ~68,000 XP              |
| 1 → 100     | **~105,000 XP**         |

**Base XP**: 300 XP at Level 1
**Formula**: 300 * (1.035^(Level-1))

This is 3x slower than Common abilities, representing a significant commitment to weapon specialization that matches the powerful three-stat bonuses provided.

## Comparison with Other Passive Abilities

### vs. Attack-Focused Abilities

| Ability | Max Damage Bonus | Conditions | Rarity |
|---------|------------------|------------|--------|
| **Weapon Mastery** | +70% | Must use specific weapon | Rare |
| Attack Boost (passive) | +10% | Always active | Common |
| Rallying Cry | N/A | Buffs companions, not player | Rare |

### vs. Accuracy Abilities

| Ability | Max Accuracy Bonus | Conditions | Rarity |
|---------|-------------------|------------|--------|
| **Weapon Mastery** | +40% | Must use specific weapon | Rare |
| Precision Training | +30% | Applies to player & allies | Epic |
| Critical Strike | Small bonus | When ability is equipped | Common |

### vs. Critical Hit Abilities

| Ability | Crit Chance | Crit Multiplier | Rarity |
|---------|-------------|-----------------|--------|
| **Weapon Mastery** | +25% | Default (1.5x) | Rare |
| Executioner | -10% to +10% | 1.1x to 3.0x | Legendary |
| Critical Strike (active) | Large temporary boost | Default (1.5x) | Common |

## Integration with Combat System

### Accuracy Calculation
When player has a weapon mastery active and is wielding the correct weapon:

```
Base Accuracy (from weapon)
  + Weapon Mastery Accuracy Bonus (percentage-based)
  = Final Accuracy
```

Example with Level 100 Sword Mastery:
```
70% (Iron Sword accuracy)
  + 28% (40% bonus of 70% base)
  = 98% Final Accuracy
```

### Damage Calculation
Weapon mastery damage applies **multiplicatively** in the damage formula:

```
Base Damage (weapon roll + level bonus)
  × Weapon Mastery Damage Multiplier (1.0 to 1.7)
  × Other multipliers (Attack Boost, etc.)
  = Final Damage (before defense)
```

Example with Level 100 Dagger Mastery:
```
15 base damage
  × 1.70 (Dagger Mastery +70%)
  = 25.5 damage
  × 1.3 (Attack Boost active)
  = 33 damage (before enemy defense)
```

### Critical Hit Calculation
Weapon mastery crit chance is **additive** with weapon base crit:

```
Base Crit Chance (from weapon)
  + Weapon Mastery Crit Bonus (flat percentage)
  + Other bonuses (Critical Strike effect, etc.)
  = Final Crit Chance (capped 0-100%)
```

Example with Level 100 Bow Mastery:
```
8% (Longbow crit chance)
  + 25% (Bow Mastery bonus)
  + 15% (Critical Strike ability active)
  = 48% Final Crit Chance
```

## Strategic Considerations

### When to Choose Weapon Mastery

**Good Choice If**:
- You've found a powerful weapon you want to commit to
- You have multiple copies of the same weapon type at different levels
- Your build focuses on direct combat damage
- You prefer specialization over versatility

**Bad Choice If**:
- You like to frequently switch weapons for tactical advantage
- You haven't found a good weapon yet
- You prefer support/utility abilities
- You want to keep options open for different weapon types

### Combining with Other Abilities

**Synergies**:
- **Executioner** + Weapon Mastery: Extreme crit damage build (Mastery provides +25% crit chance to offset Executioner's penalty)
- **Attack Boost** + Weapon Mastery: Stack damage multipliers for burst damage
- **Critical Strike** + Weapon Mastery: Maximize crit frequency with temporary boosts

**Anti-Synergies**:
- **Precision Training**: Overlaps with mastery's accuracy bonus (wastes potential)
- **Rallying Cry**: Buffs companions instead of player (no damage synergy)
- **Evasion/Iron Will**: Defensive abilities that don't complement offensive mastery

## Progression Examples

### Early Game (Levels 1-25)

**Level 10 Sword Mastery**:
- Accuracy: +4%
- Damage: +7%
- Crit Chance: +3%

**Impact**: Noticeable but not game-changing. Acts as a small power boost while training.

### Mid Game (Levels 26-75)

**Level 50 Spear Mastery**:
- Accuracy: +20%
- Damage: +35%
- Crit Chance: +13%

**Impact**: Significant power spike. 35% more damage makes weapon specialization clearly superior to generalist builds.

### Late Game (Levels 76-100)

**Level 100 Axe Mastery**:
- Accuracy: +40%
- Damage: +70%
- Crit Chance: +25%

**Impact**: Weapon mastery becomes a dominant force. Nearly doubles base damage, makes attacks almost always hit, and crits occur frequently. Player becomes a true weapon master.

## Example Build: The Sword Saint

**Core Concept**: Maximum sword damage with critical strikes

**Ability**: Sword Mastery (Level 100)
- +40% accuracy (near-perfect hits)
- +70% damage (massive DPS)
- +25% crit chance (frequent crits)

**Weapon**: Legendary Longsword +100
- High base damage
- Good crit chance
- Synergizes with mastery bonuses

**Playstyle**:
- Commit to swords permanently
- Upgrade one sword to max level
- Rely on consistent, powerful attacks
- Become unstoppable with chosen weapon

**Strengths**:
- Highest possible sword damage in game
- Reliable hits (near 100% accuracy)
- Frequent critical hits
- Clear power fantasy

**Weaknesses**:
- Locked into swords (no weapon flexibility)
- Vulnerable if weapon is lost/broken
- No utility or support options
- 105,000 XP investment required

## Frequently Asked Questions

### Q: Can I use multiple weapon masteries?
**A**: No. You can only have one ability selected at a time, including weapon masteries.

### Q: What happens if I switch weapons?
**A**: The mastery bonuses only apply when wielding the correct weapon type. Switch to a different weapon type and you lose all mastery bonuses (but the ability stays equipped).

### Q: Which weapon mastery is best?
**A**: They all provide identical bonuses. Choose based on which weapon type you prefer or have access to.

### Q: Do mastery bonuses apply to unarmed combat?
**A**: No. You must be wielding the specific weapon type for bonuses to apply.

### Q: Can I change my mastery later?
**A**: Yes, you can switch to a different ability (including a different mastery) at any time. Your old mastery keeps its level and XP.

### Q: Is weapon mastery better than Executioner?
**A**: Different purposes. Mastery provides reliable damage/accuracy/crit, while Executioner trades consistency for extreme crit damage potential. Mastery is Rare (105k XP to max), Executioner is Legendary (175k XP to max).

### Q: Do mastery bonuses stack with weapon level bonuses?
**A**: Yes! A level 100 weapon with level 100 mastery is incredibly powerful.

## Implementation Details

### Code Integration

**Files Created**:
- `spear_mastery_ability.cs`
- `sword_mastery_ability.cs`
- `axe_mastery_ability.cs`
- `mace_mastery_ability.cs`
- `dagger_mastery_ability.cs`
- `staff_mastery_ability.cs`
- `bow_mastery_ability.cs`
- `crossbow_mastery_ability.cs`
- `wand_mastery_ability.cs`

**Files Modified**:
- `damage_calculator.cs` - Added mastery bonus application to accuracy, damage, and crit calculations
- `player.cs` - Added all masteries to ability selection and save/load system

**Key Methods**:
- `GetAccuracyBonus()` - Returns accuracy percentage bonus (1-40%)
- `GetAttackDamageBonus()` - Returns damage percentage bonus (1-70%)
- `GetCritChanceBonus()` - Returns crit chance bonus (1-25%)
- `IsWieldingCorrectWeapon()` - Checks if player has correct weapon type equipped
- `GetAccuracyMultiplier()` - Returns accuracy as decimal (0.01-0.40)
- `GetDamageMultiplier()` - Returns damage multiplier (1.01-1.70)

### Balance Considerations

**Why Rare Rarity?**
- Common would be too easy to max (35k XP) for such powerful bonuses
- Uncommon (70k XP) would still be too accessible for three powerful stat bonuses
- Rare (105k XP) represents a significant commitment matching the triple-bonus power
- Still more accessible than Epic (140k) or Legendary (175k)

**Why These Specific Scaling Values?**
- Accuracy: 40% maximum ensures high-level accuracy weapons can approach 100% hit rate
- Damage: 70% maximum provides significant power without being overwhelming
- Crit: 25% maximum allows builds to reach ~50% crit with weapon + mastery + effects

**Why Weapon-Specific?**
- Encourages specialization and build commitment
- Creates interesting trade-offs vs. weapon flexibility
- Rewards finding/upgrading a single powerful weapon
- Prevents mastery from being universally optimal choice

## Summary

✅ 9 weapon masteries implemented (one per weapon type)
✅ Identical scaling for all masteries (fair and balanced)
✅ Conditional activation (only works with correct weapon)
✅ Three-pronged bonuses (accuracy, damage, crit chance)
✅ Rare rarity (105,000 XP to max)
✅ Integrated into damage calculation system
✅ Available in ability selection
✅ Save/load support
✅ Milestone notifications every 10 levels
✅ Complete documentation

Weapon Mastery abilities provide a powerful specialization option for players who want to commit to a specific weapon type. The substantial bonuses (+40% accuracy, +70% damage, +25% crit at max level) reward dedication while the conditional activation creates meaningful strategic choices between specialization and versatility.