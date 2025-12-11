# Ability XP Scaling by Rarity

## Overview
Ability XP requirements now scale based on rarity, creating meaningful progression differences between Common and Legendary abilities. More powerful abilities require significantly more experience to level up, rewarding player dedication and creating strategic choices.

## Formula

**Base Formula**: `baseXP * (1.035^(Level-1))`

**Base XP by Rarity**:
- **Common**: 100 XP
- **Uncommon**: 200 XP
- **Rare**: 300 XP
- **Epic**: 400 XP
- **Legendary**: 500 XP

## XP Requirements Comparison

### Key Levels

| Level | Common | Uncommon | Rare | Epic | Legendary |
|-------|--------|----------|------|------|-----------|
| 1→2   | 100    | 200      | 300  | 400  | 500       |
| 10→11 | 135    | 270      | 405  | 540  | 675       |
| 25→26 | 211    | 422      | 633  | 844  | 1,055     |
| 50→51 | 400    | 800      | 1,200| 1,600| 2,000     |
| 75→76 | 759    | 1,518    | 2,277| 3,036| 3,795     |
| 99→100| 1,430  | 2,860    | 4,290| 5,720| 7,150     |

### Total XP to Reach Level 100

**Approximate Total XP Required** (sum of all level-ups):

| Rarity | Total XP | vs Common |
|--------|----------|-----------|
| Common | ~35,000 XP | 1.0x |
| Uncommon | ~70,000 XP | 2.0x |
| Rare | ~105,000 XP | 3.0x |
| Epic | ~140,000 XP | 4.0x |
| Legendary | ~175,000 XP | 5.0x |

*(Exact calculations may vary slightly due to rounding)*

## Current Ability Rarities

### Common Abilities
- **Attack Boost** (Active) - Temporary damage increase
- **Healing Ability** (Active) - Self-healing
- **Defense Boost** (Active) - Temporary defense increase
- **Critical Strike** (Active) - Temporary crit chance boost

**XP Progression**: Fastest leveling (100 base XP)

### Rare Abilities
- **Leadership** (Passive) - Increases max party size
- **Rallying Cry** (Passive) - Increases companion attack damage

**XP Progression**: Moderate leveling (300 base XP, 3x slower than Common)

### Epic Abilities
- **Evasion** (Passive) - Chance to dodge damage
- **Precision Training** (Passive) - Increases ally accuracy
- **Swift Tactics** (Passive) - Increases companion speed
- **Iron Will** (Passive) - Chance to cleanse status effects

**XP Progression**: Slow leveling (400 base XP, 4x slower than Common)

### Legendary Abilities
- **Executioner** (Passive) - Modifies critical hit mechanics

**XP Progression**: Slowest leveling (500 base XP, 5x slower than Common)

## Design Philosophy

### Why Scale by Rarity?

1. **Meaningful Choices**: Players must choose between:
   - **Common** abilities that level quickly but cap lower
   - **Legendary** abilities that take dedication but become extremely powerful

2. **Investment Commitment**: Higher rarity = higher commitment
   - Legendary abilities require ~5x more XP than Common
   - Players feel the weight of choosing a Legendary passive
   - Reaching max level on Legendary feels more rewarding

3. **Balance Through Time**: Power is balanced by time
   - Executioner is incredibly strong at L100 (3x crit damage)
   - But it takes 175,000 XP to get there (vs 35,000 for Common)
   - Natural progression gate prevents early game abuse

4. **Progression Curve**: Smooth difficulty increase
   - Early levels (1-25): Achievable for all rarities
   - Mid levels (26-75): Epic/Legendary start to feel slow
   - Late levels (76-100): Legendary becomes a true grind

### XP Sources

Abilities gain XP from:
- **Combat Usage**: Active abilities gain XP when used
- **Passive Experience**: All abilities can gain XP from quest rewards
- **Shared Experience**: Party XP can contribute to ability leveling

This means:
- **Common Active** abilities level very quickly (used often + low requirement)
- **Legendary Passive** abilities level very slowly (no combat XP + high requirement)

## Examples

### Common Active Ability (Attack Boost)

**Level 1**: Requires 100 XP
- Use in 10 combats (~10 XP per use) = Level 2

**Level 50**: Requires 400 XP
- Moderate but achievable

**Level 100**: Total ~35,000 XP
- Achievable in normal playthrough

### Legendary Passive Ability (Executioner)

**Level 1**: Requires 500 XP
- 5x longer than Common to reach Level 2
- Significant initial investment

**Level 50**: Requires 2,000 XP
- 5x longer than Common
- Major grind to reach break-even point (Level 51)

**Level 100**: Total ~175,000 XP
- Endgame goal
- True dedication required
- Worthy of "Legendary" status

## Impact on Ability Choice

### For New Players

**Recommendation**: Consider Common or Uncommon abilities
- Level quickly
- See progress immediately
- Lower commitment
- Good for learning

### For Experienced Players

**Recommendation**: Epic or Legendary abilities
- Higher payoff at max level
- Long-term investment
- Powerful endgame builds
- Prestige factor

### For Specific Builds

**Party Leader** (Multiple Rare/Epic abilities):
- Leadership (Rare): ~105,000 XP
- Rallying Cry (Rare): ~105,000 XP
- Swift Tactics (Epic): ~140,000 XP
- Precision Training (Epic): ~140,000 XP

**Solo Powerhouse** (Legendary focus):
- Executioner (Legendary): ~175,000 XP
- Highest XP requirement
- Highest solo power at max level
- Risk/reward personified

## Visual Comparison

### XP Required to Reach Level 50

```
Common:      [████████████████████] ~10,000 XP
Uncommon:    [████████████████████████████████████████] ~20,000 XP
Rare:        [████████████████████████████████████████████████████████████] ~30,000 XP
Epic:        [████████████████████████████████████████████████████████████████████████████████] ~40,000 XP
Legendary:   [████████████████████████████████████████████████████████████████████████████████████████████████████] ~50,000 XP
```

### Time Investment (Rough Estimates)

Assuming average player gains 1,000 XP per hour of play:

| Rarity | Hours to L100 |
|--------|---------------|
| Common | ~35 hours |
| Uncommon | ~70 hours |
| Rare | ~105 hours |
| Epic | ~140 hours |
| Legendary | ~175 hours |

**Legendary abilities literally require 5x the playtime investment!**

## Summary

✅ XP scaling now properly reflects ability power
✅ Common abilities level quickly (100 base XP)
✅ Legendary abilities require massive investment (500 base XP, 5x slower)
✅ Uses same exponential formula as player leveling (1.035 multiplier)
✅ Creates meaningful strategic choices for players
✅ Rewards dedication to high-rarity abilities
✅ Balances power through time investment

This system ensures that when a player finally maxes out a Legendary ability like Executioner, they truly feel like they've earned its incredible power. The 3x critical damage isn't just handed out - it's the reward for 175,000 XP of dedication.