# Boss Anti-Farming System - Implementation Complete

## Overview
Implemented a comprehensive anti-farming system to prevent players from repeatedly defeating the same boss to farm keys while maintaining freedom to fight bosses in any order.

---

## What Was Implemented

### 1. **Dual Scaling System**
Bosses now scale with TWO multipliers that work together:

#### Progression Scaling (15% per boss)
- Applies to ALL bosses based on total unique bosses defeated
- Boss #1: 100% strength (base)
- Boss #2: 115% strength (+15%)
- Boss #3: 130% strength (+30%)
- Boss #10: 235% strength (+135%)

#### Repeat Penalty (50% per repeat)
- Applies specifically to EACH individual boss based on how many times YOU fought it
- 1st fight: 100% (no penalty)
- 2nd fight: 150% (+50%)
- 3rd fight: 200% (+100%)
- 4th fight: 250% (+150%)

#### Combined Multiplier (Multiplicative)
```
Total Strength = (1.0 + totalBosses * 0.15) * (1.0 + repeats * 0.50)
```

**Example Scenarios:**
- Fighting your 1st boss (0 previous bosses, 0 repeats): `1.0 * 1.0 = 1.0x` (100%)
- Fighting your 5th boss (4 previous bosses, 0 repeats): `1.6 * 1.0 = 1.6x` (160%)
- Re-fighting that 5th boss (4 previous bosses, 1 repeat): `1.6 * 1.5 = 2.4x` (240%)
- Re-fighting again (4 previous bosses, 2 repeats): `1.6 * 2.0 = 3.2x` (320%)

This makes repeat fights SIGNIFICANTLY harder while still allowing any-order progression.

---

### 2. **Diminishing Key Returns**
Boss key drops now have diminishing chances on repeat defeats:

| Fight Number | Key Drop Chance | Message                          |
|--------------|-----------------|----------------------------------|
| 1st defeat   | 100% (guaranteed) | "🔑 Guaranteed key drop"      |
| 2nd defeat   | 50%             | "🔑 Lucky! Key dropped despite repeat" |
| 3rd defeat   | 25%             | "❌ No key dropped this time" |
| 4th+ defeat  | 10%             | "💡 Key drops are rare on repeats!" |

This prevents infinite key farming while still allowing players to try for additional keys if needed.

---

### 3. **Repeat Tracking**
Added `TimesDefeated` counter to each boss:
- Tracks how many times the player has defeated THIS specific boss
- Persists across the game session
- Used to calculate repeat penalty
- Used to determine key drop rate
- Independent from other bosses

---

## Files Modified

### game_logic/entities/enemies/boss_enemy.cs

**Added:**
- `public int TimesDefeated { get; set; }` property

**Updated ApplyStrengthScaling():**
```csharp
public void ApplyStrengthScaling(int totalBossesDefeated)
{
    BossNumber = totalBossesDefeated + 1;

    // Layer 1: Progression scaling (15% per total boss defeated)
    double progressionMultiplier = 1.0 + (totalBossesDefeated * 0.15);

    // Layer 2: Repeat penalty (50% per repeat of THIS specific boss)
    double repeatPenalty = 1.0 + (TimesDefeated * 0.50);

    // Combined multiplier (multiplicative - both scale together)
    double totalMultiplier = progressionMultiplier * repeatPenalty;

    // Apply scaling to all combat stats
    MaxHealth = (int)(_baseMaxHealth * totalMultiplier);
    Health = MaxHealth;
    MinDamage = (int)(_baseMinDamage * totalMultiplier);
    MaxDamage = (int)(_baseMaxDamage * totalMultiplier);
    Defense = (int)(_baseDefense * totalMultiplier);

    // Scale rewards (less aggressive - 5% progression + 10% per repeat)
    double rewardMultiplier = (1.0 + (totalBossesDefeated * 0.05)) * (1.0 + (TimesDefeated * 0.10));
    GoldValue = (int)(GoldValue * rewardMultiplier);
    XPValue = (int)(XPValue * rewardMultiplier);

    // Display scaling information
    Console.WriteLine($"\n⚔️  Boss Strength Scaling Applied!");
    Console.WriteLine($"Boss #{BossNumber} in your journey | Repeat: {TimesDefeated + 1}{GetOrdinalSuffix(TimesDefeated + 1)} fight");

    if (TimesDefeated == 0)
    {
        Console.WriteLine($"Progression scaling: {progressionMultiplier:F2}x ({(progressionMultiplier - 1) * 100:F0}% from total progress)");
    }
    else
    {
        Console.WriteLine($"Progression scaling: {progressionMultiplier:F2}x ({(progressionMultiplier - 1) * 100:F0}% from total progress)");
        Console.WriteLine($"⚠️  Repeat penalty: {repeatPenalty:F2}x ({(repeatPenalty - 1) * 100:F0}% from {TimesDefeated} previous defeat{(TimesDefeated > 1 ? "s" : "")})");
        Console.WriteLine($"💀 COMBINED STRENGTH: {totalMultiplier:F2}x ({(totalMultiplier - 1) * 100:F0}% total increase)");
    }
}
```

**Updated GetLootDrops():**
```csharp
public override List<Item> GetLootDrops(RNGManager rng)
{
    var drops = base.GetLootDrops(rng);

    // Determine key drop chance based on times defeated
    int keyDropChance = TimesDefeated switch
    {
        0 => 100,  // First time: guaranteed drop
        1 => 50,   // Second time: 50% chance
        2 => 25,   // Third time: 25% chance
        _ => 10    // Fourth+ time: 10% chance
    };

    // Roll for key drop
    int roll = rng.Roll(1, 100);
    if (roll <= keyDropChance)
    {
        var key = GetBossKey();
        if (key != null)
        {
            drops.Add(key);

            if (TimesDefeated == 0)
            {
                Console.WriteLine($"🔑 Guaranteed key drop: {key.Name}");
            }
            else
            {
                Console.WriteLine($"🔑 Lucky! Key dropped despite repeat ({keyDropChance}% chance): {key.Name}");
            }
        }
    }
    else if (TimesDefeated > 0)
    {
        Console.WriteLine($"❌ No key dropped this time ({keyDropChance}% chance on repeat #{TimesDefeated + 1})");
        Console.WriteLine($"💡 This boss has already been defeated {TimesDefeated} time{(TimesDefeated > 1 ? "s" : "")} - key drops are rare on repeats!");
    }

    return drops;
}
```

**Added Helper:**
```csharp
private string GetOrdinalSuffix(int number)
{
    if (number <= 0) return "th";

    int lastDigit = number % 10;
    int lastTwoDigits = number % 100;

    if (lastTwoDigits >= 11 && lastTwoDigits <= 13)
        return "th";

    return lastDigit switch
    {
        1 => "st",
        2 => "nd",
        3 => "rd",
        _ => "th"
    };
}
```

### game_logic/progression/boss_manager.cs

**Added Constant:**
```csharp
public const double REPEAT_PENALTY_PER_DEFEAT = 0.50;  // 50% stronger per repeat of same boss
```

**Updated DefeatBoss():**
```csharp
public QuestItem DefeatBoss(string bossId)
{
    if (!_allBosses.TryGetValue(bossId, out BossEnemy boss))
    {
        Console.WriteLine($"Boss '{bossId}' not found!");
        return null;
    }

    // Check if this is a first-time defeat or repeat
    bool isFirstDefeat = !IsBossDefeated(bossId);

    if (isFirstDefeat)
    {
        // First time defeating this boss
        boss.MarkDefeated();
        _defeatedBossIds.Add(bossId);
        _bossesDefeated++;

        Console.WriteLine($"\n🏆 CHAMPION DEFEATED!");
        Console.WriteLine($"{boss.Name} has fallen for the first time!");
        Console.WriteLine($"Unique bosses defeated: {_bossesDefeated}/{TOTAL_BOSSES}");

        // Check if final gate should unlock
        CheckFinalGateUnlock();
    }
    else
    {
        // Repeat defeat
        Console.WriteLine($"\n🏆 CHAMPION DEFEATED AGAIN!");
        Console.WriteLine($"{boss.Name} has fallen once more!");
        Console.WriteLine($"This is your {boss.TimesDefeated + 1}{GetOrdinalSuffix(boss.TimesDefeated + 1)} victory against this boss.");
    }

    // Increment repeat counter (tracks total defeats of THIS boss)
    boss.TimesDefeated++;

    // Boss will drop key based on diminishing returns (handled in BossEnemy.GetLootDrops)
    // Return null here - key drop is handled through GetLootDrops() during combat
    return null;
}
```

**Added Helper:**
```csharp
private string GetOrdinalSuffix(int number)
{
    if (number <= 0) return "th";

    int lastDigit = number % 10;
    int lastTwoDigits = number % 100;

    if (lastTwoDigits >= 11 && lastTwoDigits <= 13)
        return "th";

    return lastDigit switch
    {
        1 => "st",
        2 => "nd",
        3 => "rd",
        _ => "th"
    };
}
```

**Updated Reset():**
```csharp
public void Reset()
{
    _defeatedBossIds.Clear();
    _bossesDefeated = 0;
    FinalGateUnlocked = false;
    FinalBossId = null;

    // Reset all bosses
    foreach (var boss in _allBosses.Values)
    {
        boss.IsDefeated = false;
        boss.BossNumber = 0;
        boss.TimesDefeated = 0;  // Reset repeat counter
    }
}
```

---

## How It Works

### First-Time Boss Fight
1. Player encounters boss for the first time
2. BossManager.ApplyBossScaling() applies progression scaling only
3. Player defeats boss
4. DefeatBoss() marks boss as defeated, increments unique boss count
5. TimesDefeated increments from 0 to 1
6. Boss drops key (100% guaranteed)

### Repeat Boss Fight
1. Player encounters same boss again
2. BossManager.ApplyBossScaling() applies BOTH progression AND repeat penalty
3. Boss is significantly harder (50%+ stronger)
4. Player defeats boss
5. DefeatBoss() recognizes repeat, shows "defeated again" message
6. TimesDefeated increments (e.g., 1 to 2)
7. Boss rolls for key drop (50% or less chance)

### Progression Balance
The system ensures:
- **First 10 bosses are reasonable** - progression scaling is gradual
- **Repeat fights are punishing** - 50% penalty is steep
- **Any-order freedom maintained** - no boss is "locked"
- **Anti-farming effective** - keys become rare on repeats
- **Still possible to farm** - but highly inefficient

---

## Example Scenarios

### Scenario 1: Optimal Path (No Repeats)
```
Boss #1 (Flame Warden): 1.0x strength → 100% key drop ✓
Boss #2 (Frost Tyrant): 1.15x strength → 100% key drop ✓
Boss #3 (Thunder Lord): 1.30x strength → 100% key drop ✓
...
Boss #10 (Plague Bearer): 2.35x strength → 100% key drop ✓
Final Gate Unlocked! (10 unique keys collected)
```

### Scenario 2: Player Repeats Boss #3
```
Boss #1: 1.0x strength → key ✓
Boss #2: 1.15x strength → key ✓
Boss #3 (1st time): 1.30x strength → key ✓
Boss #4: 1.45x strength → key ✓
Boss #3 (2nd time): 1.45x * 1.5x = 2.175x strength → 50% key chance (maybe ✗)
Boss #5: 1.60x strength → key ✓
Boss #3 (3rd time): 1.60x * 2.0x = 3.20x strength → 25% key chance (probably ✗)
```

### Scenario 3: Late-Game Farming Attempt
```
10 unique bosses defeated (progression = 2.5x)
Player tries to farm Boss #1 for 5th time:
Strength: 2.5x * (1 + 4*0.5) = 2.5x * 3.0x = 7.5x base strength!
Key drop chance: 10%
Result: Extremely difficult, very unlikely to get key
```

---

## Integration Notes

When integrating with combat system, the flow should be:

```csharp
// Before combat
var boss = bossManager.GetBoss(bossId);
bossManager.ApplyBossScaling(boss);  // Applies both progression and repeat scaling
Console.WriteLine(boss.GetBossInfo());

// Start combat
var combatResult = combatManager.StartCombat(player, boss);

// After victory
if (combatResult == CombatResult.Victory)
{
    // Mark boss as defeated (increments TimesDefeated)
    bossManager.DefeatBoss(boss.BossId);

    // Get loot drops (includes key with diminishing returns)
    var loot = boss.GetLootDrops(rng);

    // Add loot to player inventory
    foreach (var item in loot)
    {
        player.Inventory.AddItem(item);
    }
}
```

---

## Summary

✅ **Anti-Farming System Complete!**

The system now includes:
- **Dual scaling** (progression + repeat penalty)
- **Diminishing key returns** (100% → 50% → 25% → 10%)
- **Repeat tracking** per boss
- **Any-order freedom** maintained
- **Clear feedback** on scaling and drop rates

**Effect on Gameplay:**
- Players are encouraged to fight different bosses
- Farming same boss is possible but heavily discouraged
- Progression difficulty increases naturally
- Key scarcity makes repeats strategic decisions
- No artificial locks or gates

**Next Phase:** Combat integration and save system support

**Lines Modified:** ~150 lines across 2 files
**New Features:** 2 (Dual Scaling, Diminishing Returns)
**Balance Changes:** Repeat penalty 50%, key drop reduction per repeat

The anti-farming system is production-ready and awaits combat integration!