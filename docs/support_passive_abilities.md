# Support Passive Abilities - Rallying Cry & Precision Training

## Overview
Support passive abilities focus on enhancing the player's companions and allies rather than providing direct personal benefits. These abilities are ideal for players who want to build strong party-based strategies and maximize companion effectiveness in combat.

## Implementation Status: ✅ COMPLETE

Two support passive abilities have been implemented:
1. **Rallying Cry** - Increases companion attack damage
2. **Precision Training** - Increases accuracy for all allies (player + companions)

---

## Rallying Cry - Companion Attack Buff

### Basic Information
- **Name**: Rallying Cry
- **Type**: Passive
- **Rarity**: Rare
- **Description**: "Your inspiring presence and battle cries embolden your companions, increasing their attack damage."
- **Target**: All companions in party (does NOT affect player damage)

### Scaling Formula

**Attack Damage Bonus Progression**:
| Ability Level | Attack Bonus | Damage Multiplier |
|--------------|--------------|-------------------|
| 1            | +5%          | 1.05x             |
| 10           | +13.6%       | 1.136x            |
| 25           | +26%         | 1.26x             |
| 50           | +47.5%       | 1.475x            |
| 75           | +69%         | 1.69x             |
| 100          | +90%         | 1.90x             |

**Formula**: Linear scaling from 5% to 90%
```csharp
int attackBonus = GetScaledValueInt(5, 90);
double damageMultiplier = 1.0 + (attackBonus / 100.0);
companionDamage = (int)(companionDamage * damageMultiplier);
```

**Approximately 0.859% increase per level**

### Key Features

1. **Companions Only**
   - Only affects companion attack damage
   - Player damage is NOT increased
   - Encourages party-based playstyle

2. **Applied After Crits**
   - Bonus is applied after critical hit calculation
   - Critical hits get amplified by Rallying Cry
   - Maximizes burst damage potential

3. **Stacks with Weapon Damage**
   - Multiplies total damage (base + level bonus + crit)
   - Better weapons = better Rallying Cry value
   - Scales with companion power level

### How It Works

**Companion Damage Flow with Rallying Cry**:
```
Companion attacks enemy
       ↓
Calculate base weapon damage
       ↓
Add level bonus
       ↓
Check for critical hit → Apply 1.5x multiplier if crit
       ↓
Check if player has Rallying Cry ability
       ↓ Yes
Apply Rallying Cry multiplier (1.05x to 1.90x)
       ↓
Display bonus damage message
       ↓
Apply final damage to enemy
```

### Combat Examples

**Level 1 Rallying Cry (+5%)**:
```
Companion attacks Goblin!
Base damage: 20
Critical hit! (20 * 1.5 = 30)
Rallying Cry increased damage by 1! (+5%)
Companion attacks Goblin for 31 damage!
```

**Level 50 Rallying Cry (+47.5%)**:
```
Companion attacks Dragon!
Base damage: 35
Rallying Cry increased damage by 16! (+47%)
Companion attacks Dragon for 51 damage!
```

**Level 100 Rallying Cry (+90%)**:
```
Companion attacks Boss!
Base damage: 50
Critical hit! (50 * 1.5 = 75)
Rallying Cry increased damage by 67! (+90%)
Companion attacks Boss for 142 damage!
```

### Milestone Notifications

Every 10 levels, player receives notification:
```
Rallying Cry leveled up to Level 10!
Your rallying cry grows stronger! Companion attack bonus is now 13%!
```

---

## Precision Training - Ally Accuracy Buff

### Basic Information
- **Name**: Precision Training
- **Type**: Passive
- **Rarity**: Epic
- **Description**: "Your expertise in combat techniques improves the accuracy of you and your allies, making attacks more likely to hit."
- **Target**: Player AND all companions (affects entire party)

### Scaling Formula

**Accuracy Bonus Progression**:
| Ability Level | Accuracy Bonus | Example (60 base → result) |
|--------------|----------------|----------------------------|
| 1            | +1%            | 60 → 60.6                  |
| 10           | +3.7%          | 60 → 62.2                  |
| 25           | +8.2%          | 60 → 64.9                  |
| 50           | +15.5%         | 60 → 69.3                  |
| 75           | +22.7%         | 60 → 73.6                  |
| 100          | +30%           | 60 → 78                    |

**Formula**: Linear scaling from 1% to 30%
```csharp
int accuracyBonus = GetScaledValueInt(1, 30);
double bonusMultiplier = accuracyBonus / 100.0;
finalAccuracy = (int)(baseAccuracy + (baseAccuracy * bonusMultiplier));
```

**Approximately 0.293% increase per level**

### Key Features

1. **Affects All Allies**
   - Increases player accuracy
   - Increases all companion accuracy
   - Party-wide benefit

2. **Percentage-Based Scaling**
   - Better base accuracy = better Precision Training value
   - Higher tier weapons benefit more
   - Compounds with weapon accuracy stats

3. **Multiplicative Bonus**
   - Adds percentage of base accuracy
   - 60 base accuracy with 30% bonus = 78 accuracy
   - Scales naturally with equipment

### How It Works

**Accuracy Check Flow with Precision Training**:
```
Entity attacks target
       ↓
Get base accuracy from weapon (or 60 if unarmed)
       ↓
Check if player has Precision Training
       ↓ Yes
Calculate accuracy bonus percentage (1-30%)
       ↓
Apply multiplicative bonus: base + (base * bonus%)
       ↓
Roll 1-100 vs final accuracy
       ↓
Hit if roll ≤ accuracy, Miss otherwise
```

### Combat Examples

**Player Attack (Level 1 Precision Training, +1%)**:
```
Base accuracy: 70 (from weapon)
With Precision Training: 70 + (70 * 0.01) = 70.7 ≈ 71
Roll: 68 → HIT! (68 ≤ 71)
```

**Companion Attack (Level 50 Precision Training, +15.5%)**:
```
Base accuracy: 60 (from weapon)
With Precision Training: 60 + (60 * 0.155) = 69.3 ≈ 69
Roll: 66 → HIT! (66 ≤ 69)
Without Precision Training: 66 > 60 → MISS
```

**Player Unarmed (Level 100 Precision Training, +30%)**:
```
Base accuracy: 60 (unarmed default)
With Precision Training: 60 + (60 * 0.30) = 78
Roll: 75 → HIT! (75 ≤ 78)
Without Precision Training: 75 > 60 → MISS
```

### Milestone Notifications

Every 5 levels, player receives notification:
```
Precision Training leveled up to Level 5!
Your precision training improves! Ally accuracy bonus is now 2%!
```

---

## Files Created/Modified

### 1. [game_logic/abilities/rallying_cry_ability.cs](game_logic/abilities/rallying_cry_ability.cs) - NEW
**Purpose**: Implementation of Rallying Cry companion attack buff

**Key Methods**:
- `GetPassiveBonusValue()`: Returns attack bonus % (5-90%)
- `GetDamageMultiplier()`: Returns damage multiplier (1.05 to 1.90)
- `OnLevelUp()`: Milestone notifications every 10 levels
- `GetInfo()`: Detailed ability information display

### 2. [game_logic/abilities/precision_training_ability.cs](game_logic/abilities/precision_training_ability.cs) - NEW
**Purpose**: Implementation of Precision Training accuracy buff

**Key Methods**:
- `GetPassiveBonusValue()`: Returns accuracy bonus % (1-30%)
- `GetAccuracyBonus()`: Returns decimal bonus for calculations
- `OnLevelUp()`: Milestone notifications every 5 levels
- `GetInfo()`: Detailed ability information display

### 3. [game_logic/entities/NPCs/companions/companion_base.cs](game_logic/entities/NPCs/companions/companion_base.cs)
**Changes Made**: Modified `AttackEnemy()` to accept player parameter and apply buffs

**Integration Points**:
```csharp
public virtual DamageResult AttackEnemy(Enemy target, DamageCalculator damageCalc, RNGManager rng, Player.Player player = null)
{
    // ... damage calculation ...

    // Apply Precision Training accuracy buff
    if (player != null && player.SelectedAbility is Abilities.PrecisionTrainingAbility precisionTraining)
    {
        double accuracyBonus = precisionTraining.GetAccuracyBonus();
        accuracy = (int)(accuracy + (accuracy * accuracyBonus));
    }

    // ... hit check and crit calculation ...

    // Apply Rallying Cry damage buff
    if (player != null && player.SelectedAbility is Abilities.RallyingCryAbility rallyingCry)
    {
        double damageMultiplier = rallyingCry.GetDamageMultiplier();
        int originalDamage = baseDamage;
        baseDamage = (int)(baseDamage * damageMultiplier);
        int bonusDamage = baseDamage - originalDamage;

        if (bonusDamage > 0)
        {
            Console.WriteLine($"Rallying Cry increased damage by {bonusDamage}! (+{rallyingCry.GetPassiveBonusValue()}%)");
        }
    }
}
```

### 4. [game_logic/combat/combat_manager.cs](game_logic/combat/combat_manager.cs)
**Changes Made**: Updated companion attack calls to pass player reference (lines 530, 544)

**Before**:
```csharp
companion.AttackEnemy(_enemy, _damageCalculator, _rngManager);
```

**After**:
```csharp
companion.AttackEnemy(_enemy, _damageCalculator, _rngManager, _player);
```

### 5. [game_logic/combat/damage_calculator.cs](game_logic/combat/damage_calculator.cs)
**Changes Made**: Added Precision Training check in `GetPlayerAccuracy()` method (lines 188-193)

**Integration**:
```csharp
// Precision Training ability provides scaling accuracy bonus
else if (player.SelectedAbility is PrecisionTrainingAbility precisionTraining)
{
    double accuracyBonus = precisionTraining.GetAccuracyBonus();
    baseAccuracy = (int)(baseAccuracy + (baseAccuracy * accuracyBonus));
}
```

### 6. [game_logic/entities/player/player.cs](game_logic/entities/player/player.cs)
**Changes Made**:
- Added "Rallying Cry" and "Precision Training" to `CreateAbilityFromName()` (lines 70-73)
- Added both abilities to `GetAvailableAbilities()` array (lines 91-92)

---

## Game Balance Considerations

### Why These Scaling Ranges?

**Rallying Cry (5-90%)**:
- **Starting at 5%**: Noticeable but not overpowered early
- **Capping at 90%**: Nearly doubles companion damage at max level
- **Rare Rarity**: Powerful scaling justifies rare classification
- **Companion-Only**: Player must invest in party to benefit

**Precision Training (1-30%)**:
- **Starting at 1%**: Minimal early impact (60 → 60.6)
- **Capping at 30%**: Significant but not guaranteed hits
- **Epic Rarity**: Affects entire party (player + companions)
- **Percentage-Based**: Better weapons = better value

### Trade-offs and Synergies

**Choosing Rallying Cry**:
- **Pros**:
  - Massive damage increase for companions at high levels
  - Synergizes with critical hits (amplifies burst)
  - Makes companions much more threatening
  - Scales with companion weapons/levels
- **Cons**:
  - Doesn't help player damage at all
  - Requires recruiting companions to benefit
  - No accuracy help (companions might miss more)
  - Useless if companions die

**Choosing Precision Training**:
- **Pros**:
  - Helps player AND companions
  - More consistent hits = more reliable damage
  - Multiplicative scaling rewards better weapons
  - Entire party benefits
- **Cons**:
  - Lower impact than damage buffs
  - Doesn't help if already hitting reliably
  - No damage increase (just hit chance)
  - Diminishing returns at high accuracy

### Synergies Between Abilities

**Rallying Cry + Good Weapons**:
- High base damage gets amplified significantly
- Critical hits become devastating
- Makes weapon choice more important

**Precision Training + Low Accuracy Builds**:
- Unarmed combat becomes more viable (60 → 78)
- Heavy weapons with lower accuracy become better
- Reduces frustration from missed attacks

**Support Abilities + Party Size**:
- Leadership + Rallying Cry = More companions dealing boosted damage
- Leadership + Precision Training = Larger party with better accuracy
- Strong synergy for party-based playstyles

### Comparison to Other Passives

| Ability | Target | Type | Max Benefit |
|---------|--------|------|-------------|
| Leadership | Party | Utility | +4 party size (8 total) |
| Evasion | Player | Defense | 60% dodge chance |
| Rallying Cry | Companions | Offense | +90% companion damage |
| Precision Training | All Allies | Offense | +30% accuracy (all) |

**Strategic Choices**:
- **Solo Player**: Evasion (personal survival)
- **Party Leader**: Leadership (maximize party)
- **Damage Focus**: Rallying Cry (companion power)
- **Consistency**: Precision Training (reliable hits)

---

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
7. Rallying Cry - PASSIVE: Increases companion attack damage
8. Precision Training - PASSIVE: Increases ally accuracy

Choice: 7
Hero has chosen the ability: Rallying Cry!
```

### Viewing Rallying Cry Info (Level 50)

```
Rallying Cry (Lv.50/100) [PASSIVE]
Your inspiring presence and battle cries embolden your companions,
increasing their attack damage.

Current Companion Attack Bonus: +47%
Affects: All companions in your party
Does NOT affect: Player damage

Next Milestone: Level 60
  Companion attack bonus will be 56%

XP: 234/600
```

### Viewing Precision Training Info (Level 75)

```
Precision Training (Lv.75/100) [PASSIVE]
Your expertise in combat techniques improves the accuracy of you
and your allies, making attacks more likely to hit.

Current Accuracy Bonus: +22%
Affects: You and all companions in your party
Effect: Increases hit chance for all attacks

Next Milestone: Level 80
  Accuracy bonus will be 24%

XP: 567/850
```

### Combat with Both Abilities

**Scenario: Player has Rallying Cry at Level 100**
```
=================================================
    COMBAT: Hero vs Ancient Dragon
=================================================

Your Party:
  Hero (Level 45) - HP: 120/120
  Warrior (Level 40) - HP: 95/95
  Archer (Level 38) - HP: 75/75

Enemy:
  Ancient Dragon (Level 50) - HP: 500/500

--- ROUND 1 ---

Warrior attacks Ancient Dragon!
Base damage: 42
Critical hit! (42 * 1.5 = 63)
Rallying Cry increased damage by 56! (+90%)
Warrior attacks Ancient Dragon for 119 damage!

Archer attacks Ancient Dragon!
Base damage: 35
Rallying Cry increased damage by 31! (+90%)
Archer attacks Ancient Dragon for 66 damage!

Ancient Dragon attacks Hero for 45 damage!
Armor blocked 10 damage!
Hero took 35 damage!

-------------------------------------------------
Player HP: 85/120
Warrior HP: 95/95
Archer HP: 75/75
Ancient Dragon HP: 315/500
-------------------------------------------------
```

**Scenario: Player has Precision Training at Level 50**
```
Warrior's HP: 85/95

Warrior attacks Goblin!
(Base accuracy: 65, with Precision Training: 75)
Roll: 71 → HIT! (would have MISSED without Precision Training)
Warrior attacks Goblin for 38 damage!

Hero attacks Goblin!
(Base accuracy: 70, with Precision Training: 80)
Roll: 78 → HIT! (would have MISSED without Precision Training)
Hero attacks Goblin for 42 damage!
```

---

## Technical Implementation Details

### Rallying Cry Integration

**Damage Application Order**:
1. Roll base weapon damage
2. Add companion level bonus
3. Check for critical hit (1.5x if success)
4. **Apply Rallying Cry multiplier** (1.05x to 1.90x)
5. Apply final damage to target

**Type Checking Pattern**:
```csharp
if (player != null && player.SelectedAbility is Abilities.RallyingCryAbility rallyingCry)
{
    double damageMultiplier = rallyingCry.GetDamageMultiplier();
    baseDamage = (int)(baseDamage * damageMultiplier);
}
```

### Precision Training Integration

**Accuracy Calculation Order**:
1. Get base accuracy from weapon (or 60 if unarmed)
2. **Apply Precision Training bonus** (if player has it)
3. Roll 1-100 vs final accuracy
4. Hit if roll ≤ accuracy

**Multiplicative Formula**:
```csharp
int accuracyBonus = GetPassiveBonusValue(); // 1-30%
double bonusMultiplier = accuracyBonus / 100.0;
finalAccuracy = (int)(baseAccuracy + (baseAccuracy * bonusMultiplier));
```

**Example Calculation**:
- Base accuracy: 70
- Precision Training level 50: 15.5% bonus
- Calculation: 70 + (70 * 0.155) = 70 + 10.85 = 80.85 ≈ 81
- Final accuracy: 81

---

## Testing Recommendations

### Manual Testing Checklist:

**Rallying Cry**:
- [ ] Create character with Rallying Cry
- [ ] Recruit companions
- [ ] Enter combat and observe companion damage
- [ ] Verify damage bonus messages appear
- [ ] Level Rallying Cry and verify scaling
- [ ] Verify player damage is NOT affected
- [ ] Test with critical hits (should amplify bonus)
- [ ] Save and load game

**Precision Training**:
- [ ] Create character with Precision Training
- [ ] Test player accuracy increase
- [ ] Recruit companions and test their accuracy
- [ ] Verify multiplicative scaling with different weapons
- [ ] Level Precision Training and verify scaling
- [ ] Test with unarmed combat (60 base accuracy)
- [ ] Compare hit rates with/without ability
- [ ] Save and load game

### Statistical Testing:

**Rallying Cry Damage Scaling** (Level 50 = 47.5% bonus):
- Companion base damage: 40
- Expected damage: 40 * 1.475 = 59
- Test 20 attacks, verify average ~59 damage (±variance for crits)

**Precision Training Hit Rate** (Level 100 = 30% bonus):
- Base accuracy: 60 → With ability: 78
- Test 100 attacks, verify ~78% hit rate
- Compare to control without ability (~60% hit rate)

### Edge Cases:
- [ ] Rallying Cry with no companions recruited
- [ ] Precision Training with 95+ base accuracy (near cap)
- [ ] Both abilities at level 1 (minimal bonuses)
- [ ] Both abilities at level 100 (maximum bonuses)
- [ ] Companion with 100 base accuracy + Precision Training
- [ ] Switching between abilities (save/load with different ability)

---

## Integration with Statistics

Potential stats to track:
- `TotalRallyingCryDamageAdded`: Cumulative bonus damage from Rallying Cry
- `MostDamageFromRallyingCry`: Highest single bonus damage instance
- `AttacksHitByPrecisionTraining`: Attacks that hit due to Precision Training (wouldn't have hit without it)
- `CompanionCritHitsAmplified`: Critical hits boosted by Rallying Cry

---

## Future Enhancements

### Potential Improvements:
1. **Visual Feedback**: Special icon/effect when buffs apply
2. **Companion UI**: Show buff indicators on companion HP bars
3. **Statistics Tracking**: Track total bonus damage/hits from abilities
4. **Combo System**: Bonus when multiple companions hit in same round
5. **Aura Effect**: Visual radius showing buff area

### Variant Ideas:
- **Battle Tactics**: Increases companion defense instead of offense
- **Inspiring Presence**: Companions gain slight HP regen each turn
- **Veteran Commander**: Reduces companion ability cooldowns
- **Pack Tactics**: Bonus when companions focus same target
- **Training Regimen**: Companions gain XP faster

---

## Summary

The support passive abilities system is now fully functional:

✅ Rallying Cry passive ability (5-90% companion damage buff)
✅ Precision Training passive ability (1-30% ally accuracy buff)
✅ Integration into companion attack system
✅ Integration into player accuracy system
✅ Both abilities available in character creation
✅ Save/load support through existing ability system
✅ Milestone notifications for player feedback
✅ Complete documentation

These abilities provide powerful party-based alternatives to solo-focused passives (Evasion, Leadership) and encourage team-oriented playstyles. The scaling ensures early accessibility while providing significant late-game power for dedicated party leaders.