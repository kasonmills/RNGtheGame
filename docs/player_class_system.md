# Player Class System

## Overview
Players choose a class at character creation that defines their playstyle, stat bonuses, and armor compatibility. This choice is permanent and significantly impacts gameplay.

## Class Archetypes

### 1. Warrior (Heavy Armor Specialist)
**Fantasy**: Tank/Frontline fighter who excels in heavy armor and melee combat.

**Base Bonuses**:
- +20% Max Health
- +5% Defense
- +0% Speed (neutral)

**Armor Proficiencies**:
- **Plate**: No penalty ✅
- **Chainmail**: No penalty ✅
- **Leather**: -10% Defense (too light, doesn't maximize defensive potential)
- **Cloth**: -15% Defense (provides minimal protection)
- **Robe**: -20% Defense (designed for mages, not warriors)
- **Shield**: +10% Defense bonus (trained to use shields effectively)

**Playstyle**: High survivability, can wear the heaviest armor without penalties. Best for players who want to tank damage.

---

### 2. Rogue (Light Armor Specialist)
**Fantasy**: Fast, agile combatant who relies on speed and evasion.

**Base Bonuses**:
- +0% Max Health (neutral)
- +0% Defense (neutral)
- +30% Speed

**Armor Proficiencies**:
- **Plate**: -50% Speed (way too heavy, severely hinders mobility)
- **Chainmail**: -30% Speed (heavy, restricts movement)
- **Leather**: No penalty ✅
- **Cloth**: No penalty ✅
- **Robe**: -5% Speed (not designed for agility)
- **Shield**: -20% Speed (interferes with dual-wielding and nimble combat)

**Playstyle**: Glass cannon with extreme mobility. Can dodge attacks and strike quickly. Heavy armor destroys their speed advantage.

---

### 3. Mage (Robe/Cloth Specialist)
**Fantasy**: Spellcaster who channels magical energy through light armor.

**Base Bonuses**:
- +0% Max Health (neutral)
- +10% Ability Power (abilities do more damage/healing)
- +10% Speed

**Armor Proficiencies**:
- **Plate**: -40% Speed, -20% Ability Power (metal interferes with magic)
- **Chainmail**: -25% Speed, -15% Ability Power (metal disrupts spell channeling)
- **Leather**: -10% Ability Power (restricts magical flow)
- **Cloth**: No penalty ✅
- **Robe**: +10% Ability Power bonus ✅ (designed for spellcasting)
- **Shield**: -10% Ability Power (occupies casting hand)

**Playstyle**: Fragile but powerful spellcaster. Heavy armor severely interferes with magic. Robes enhance magical abilities.

---

### 4. Ranger (Balanced Armor Specialist)
**Fantasy**: Versatile hunter who balances mobility and protection.

**Base Bonuses**:
- +10% Max Health
- +15% Speed
- +5% Accuracy (trained marksman)

**Armor Proficiencies**:
- **Plate**: -30% Speed (too heavy for wilderness travel)
- **Chainmail**: -15% Speed (restricts mobility)
- **Leather**: No penalty ✅
- **Cloth**: -5% Defense (too light for wilderness dangers)
- **Robe**: -10% Defense (impractical for outdoors)
- **Shield**: -15% Accuracy (interferes with bow/ranged weapons)

**Playstyle**: Jack-of-all-trades. Good mobility and moderate defense in leather armor. Best for ranged combat.

---

### 5. Paladin (Holy Warrior)
**Fantasy**: Divine warrior who combines heavy armor with magical abilities.

**Base Bonuses**:
- +15% Max Health
- +5% Ability Power
- +5% Defense

**Armor Proficiencies**:
- **Plate**: No penalty ✅
- **Chainmail**: No penalty ✅
- **Leather**: -10% Defense (insufficient protection)
- **Cloth**: -15% Defense (too fragile)
- **Robe**: +5% Ability Power, -10% Defense (boosts magic but sacrifices protection)
- **Shield**: +5% Defense bonus (divine protection)

**Playstyle**: Hybrid class that can wear heavy armor while using abilities. Balanced between Warrior and Mage.

---

## Armor System Changes

### Single Armor Slot
- **Old System**: Separate slots for Head, Chest, Legs, Feet, Hands, Shield
- **New System**: Single "Armor" slot that represents your entire armor set
- Each armor piece now represents a full set (e.g., "Iron Plate Armor" = helmet + chestplate + greaves + gauntlets + boots)

### ArmorSlot Enum Update
```csharp
public enum ArmorSlot
{
    FullSet  // Represents complete armor ensemble
}
```

### Armor Penalty/Bonus Calculation
When armor is equipped, the game checks:
1. Player's class
2. Armor type
3. Applies appropriate bonuses/penalties to stats

**Example**:
- Rogue equips Plate Armor (50 defense)
  - Base Defense: 50
  - Speed Penalty: -50% to player's speed
  - Result: High defense but unable to dodge or act quickly

- Warrior equips same Plate Armor
  - Base Defense: 50
  - Speed Penalty: None
  - Result: High defense with no downsides

## Class Selection

### When?
Character creation, after choosing name and before starting the game.

### Prompt:
```
Choose your class:
1. Warrior - Heavy armor tank with high survivability
2. Rogue - Swift combatant who strikes from the shadows
3. Mage - Powerful spellcaster with magical prowess
4. Ranger - Versatile hunter balancing speed and protection
5. Paladin - Holy warrior combining armor and magic
```

### Permanent Choice
Once selected, class cannot be changed. This is a core part of character identity.

## Implementation Notes

### Stat Modifications
- Speed penalties are multiplicative (e.g., -50% speed on a Rogue with 13 speed = 6.5 speed)
- Defense bonuses/penalties are additive (e.g., +10% on 50 defense = +5 defense)
- Ability Power affects ability damage/healing scaling

### Visual Indicators
- Armor tooltip should show penalties for player's class
- Equipment screen should warn when equipping incompatible armor
- Color coding: Green (no penalty), Yellow (minor penalty), Red (major penalty)

## Balance Considerations

### Why These Penalties?
- **Encourages class fantasy**: Warriors want heavy armor, Rogues want light armor
- **Meaningful choices**: Can't just equip highest defense armor on every class
- **Build diversity**: Different classes seek different armor types
- **Risk/Reward**: Can wear "wrong" armor type if benefits outweigh penalties

### Preventing Exploits
- Penalties are significant enough to discourage mismatching
- Each class has a "home" armor type with no penalties
- No class can excel at everything

## Summary

✅ 5 distinct player classes with unique identities
✅ Each class has specific armor proficiencies
✅ Single armor slot (simplified equipment)
✅ Meaningful stat bonuses per class
✅ Penalties for wearing incompatible armor
✅ Encourages class-appropriate builds
✅ Permanent choice adds weight to decision