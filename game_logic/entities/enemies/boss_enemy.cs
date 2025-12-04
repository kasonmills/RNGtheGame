using System;
using System.Collections.Generic;
using GameLogic.Items;
using GameLogic.Abilities;
using GameLogic.Systems;

namespace GameLogic.Entities.Enemies
{
    /// <summary>
    /// Boss enemy class - powerful unique enemies that drop champion keys
    /// Each boss has unique combat mechanics and drops a specific key required to unlock the Final Gate
    /// Boss strength scales based on number of bosses defeated (15% per boss)
    /// </summary>
    public class BossEnemy : EnemyBase
    {
        // Boss Identity
        public string BossId { get; set; }              // Unique boss identifier (e.g., "flame_warden")
        public string KeyId { get; set; }               // ID of the key this boss drops
        public string Title { get; set; }               // Boss title (e.g., "Guardian of the Eternal Flame")

        // Boss Tracking
        public bool IsDefeated { get; set; }            // Has this boss been defeated at least once?
        public int BossNumber { get; set; }             // Sequence number for scaling (managed by BossManager)
        public int TimesDefeated { get; set; }          // How many times this specific boss has been defeated

        // Unique Combat Mechanics
        public string UniqueAbilityDescription { get; set; }  // Description of unique mechanic
        public BossMechanicType MechanicType { get; set; }    // Type of unique mechanic

        // Base stats (before scaling)
        private int _baseMinDamage;
        private int _baseMaxDamage;
        private int _baseHealth;
        private int _baseMaxHealth;
        private int _baseDefense;

        /// <summary>
        /// Constructor for boss enemies
        /// </summary>
        public BossEnemy(
            string bossId,
            string name,
            string title,
            string description,
            string keyId,
            int level,
            BossMechanicType mechanicType = BossMechanicType.Standard)
        {
            BossId = bossId;
            Name = name;
            Title = title;
            Description = description;
            KeyId = keyId;
            Level = level;
            Type = EnemyType.Boss;
            Behavior = EnemyBehavior.Tactical;
            IsDefeated = false;
            BossNumber = 0;
            TimesDefeated = 0;
            MechanicType = mechanicType;

            // Set unique ability description based on mechanic type
            UniqueAbilityDescription = GetMechanicDescription(mechanicType);
        }

        /// <summary>
        /// Initialize boss stats based on level
        /// Stores base stats for scaling later
        /// </summary>
        public override void InitializeStats(int level)
        {
            Level = level;

            // Boss base stats (much stronger than regular enemies)
            _baseHealth = 100 + (level * 30);      // Bosses have 3x health of regular enemies
            _baseMaxHealth = _baseHealth;
            _baseMinDamage = 10 + (level * 3);     // Higher base damage
            _baseMaxDamage = 20 + (level * 5);
            _baseDefense = 5 + (level / 2);        // Good defense

            // Apply base stats
            Health = _baseHealth;
            MaxHealth = _baseMaxHealth;
            MinDamage = _baseMinDamage;
            MaxDamage = _baseMaxDamage;
            Defense = _baseDefense;

            // Boss combat stats
            Accuracy = 75 + (level / 2);           // High accuracy
            CritChance = 10 + (level / 5);         // Decent crit chance
            Speed = 15 + (level / 3);              // Faster than regular enemies

            // Boss rewards
            GoldValue = 100 + (level * 25);        // Generous gold
            XPValue = 200 + (level * 50);          // Massive XP
        }

        /// <summary>
        /// Apply strength scaling based on number of bosses defeated AND repeat fights
        /// Each defeated boss makes remaining bosses 15% stronger (progression scaling)
        /// Each repeat of THIS boss makes it 50% stronger (repeat penalty)
        /// Both scale together multiplicatively
        /// </summary>
        public void ApplyStrengthScaling(int totalBossesDefeated)
        {
            BossNumber = totalBossesDefeated + 1;  // This is boss #X in the sequence

            // Layer 1: Progression scaling (15% per total boss defeated)
            double progressionMultiplier = 1.0 + (totalBossesDefeated * 0.15);

            // Layer 2: Repeat penalty (50% per repeat of THIS specific boss)
            // First fight = 0 repeats = 1.0x
            // Second fight = 1 repeat = 1.5x
            // Third fight = 2 repeats = 2.0x
            double repeatPenalty = 1.0 + (TimesDefeated * 0.50);

            // Combined multiplier (multiplicative - both scale together)
            double totalMultiplier = progressionMultiplier * repeatPenalty;

            // Apply scaling to all combat stats
            MaxHealth = (int)(_baseMaxHealth * totalMultiplier);
            Health = MaxHealth;  // Boss starts at full health
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
                // First time fighting this boss
                Console.WriteLine($"Progression scaling: {progressionMultiplier:F2}x ({(progressionMultiplier - 1) * 100:F0}% from total progress)");
            }
            else
            {
                // Repeat fight - show both scaling factors
                Console.WriteLine($"Progression scaling: {progressionMultiplier:F2}x ({(progressionMultiplier - 1) * 100:F0}% from total progress)");
                Console.WriteLine($"⚠️  Repeat penalty: {repeatPenalty:F2}x ({(repeatPenalty - 1) * 100:F0}% from {TimesDefeated} previous defeat{(TimesDefeated > 1 ? "s" : "")})");
                Console.WriteLine($"💀 COMBINED STRENGTH: {totalMultiplier:F2}x ({(totalMultiplier - 1) * 100:F0}% total increase)");
            }
        }

        /// <summary>
        /// Helper method to get ordinal suffix (1st, 2nd, 3rd, etc.)
        /// </summary>
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

        /// <summary>
        /// Get the key item this boss drops
        /// </summary>
        public QuestItem GetBossKey()
        {
            // This will be implemented in ItemDatabase
            return Items.ItemDatabase.GetQuestItem(KeyId);
        }

        /// <summary>
        /// Override GetDescription to include boss title
        /// </summary>
        public override string GetDescription()
        {
            string scalingInfo = BossNumber > 0 ? $" [Strength: x{1.0 + ((BossNumber - 1) * 0.15):F2}]" : "";
            return $"{Name}, {Title} (Level {Level}){scalingInfo}";
        }

        /// <summary>
        /// Get full boss information display
        /// </summary>
        public string GetBossInfo()
        {
            string info = "═══════════════════════════════════════\n";
            info += $"⚔️  CHAMPION BOSS  ⚔️\n";
            info += "═══════════════════════════════════════\n";
            info += $"{Name}\n";
            info += $"{Title}\n";
            info += "───────────────────────────────────────\n";
            info += $"{Description}\n";
            info += "───────────────────────────────────────\n";
            info += $"Level: {Level}\n";
            info += $"Health: {Health}/{MaxHealth}\n";
            info += $"Damage: {MinDamage}-{MaxDamage}\n";
            info += $"Defense: {Defense}\n";
            info += $"Speed: {Speed}\n";
            info += "───────────────────────────────────────\n";
            info += $"Unique Mechanic: {MechanicType}\n";
            info += $"{UniqueAbilityDescription}\n";
            info += "───────────────────────────────────────\n";
            info += $"Drops: 🔑 {KeyId} (Champion Key)\n";

            if (BossNumber > 0)
            {
                info += $"Boss #{BossNumber} | Strength: x{1.0 + ((BossNumber - 1) * 0.15):F2}\n";
            }

            info += "═══════════════════════════════════════\n";

            return info;
        }

        /// <summary>
        /// Override DecideAction to use boss-specific AI
        /// Bosses use their unique mechanics strategically
        /// </summary>
        public override EnemyAction DecideAction()
        {
            // Boss AI is more sophisticated
            // Uses abilities more frequently
            // TODO: Implement mechanic-specific AI when combat integration happens

            // For now, bosses attack aggressively
            return EnemyAction.Attack;
        }

        /// <summary>
        /// Override GetLootDrops - bosses drop their key with diminishing returns on repeats
        /// First defeat: 100% key drop (guaranteed)
        /// Second defeat: 50% key drop
        /// Third defeat: 25% key drop
        /// Fourth+ defeat: 10% key drop
        /// </summary>
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

        /// <summary>
        /// Mark boss as defeated
        /// </summary>
        public void MarkDefeated()
        {
            IsDefeated = true;
        }

        /// <summary>
        /// Get description for boss mechanic type
        /// </summary>
        private string GetMechanicDescription(BossMechanicType mechanic)
        {
            return mechanic switch
            {
                BossMechanicType.Enrage => "Becomes more dangerous as health decreases. Damage increases by 50% below 50% HP.",
                BossMechanicType.MultiPhase => "Changes combat strategy at specific health thresholds (75%, 50%, 25%).",
                BossMechanicType.Summoner => "Summons minions to assist in battle. Focus on adds or boss?",
                BossMechanicType.Regeneration => "Regenerates health each turn. Must deal sustained damage quickly.",
                BossMechanicType.Counter => "Counters player attacks with automatic retaliation damage.",
                BossMechanicType.ElementalShift => "Cycles through elemental resistances and vulnerabilities.",
                BossMechanicType.TimeLimit => "Must be defeated within limited turns or instant defeat.",
                BossMechanicType.Berserker => "Massive damage output but low accuracy. High risk, high reward.",
                BossMechanicType.Defensive => "Extremely high defense. Requires armor-piercing or critical hits.",
                BossMechanicType.Speed => "Attacks multiple times per turn. Overwhelming offense.",
                BossMechanicType.Draining => "Drains player health to heal itself. Race against time.",
                BossMechanicType.Standard => "Powerful but predictable. Pure stat check.",
                _ => "Unique combat mechanics."
            };
        }
    }

    /// <summary>
    /// Types of unique boss mechanics
    /// Defines special combat behavior for each boss
    /// </summary>
    public enum BossMechanicType
    {
        Standard,       // No special mechanic - pure stat check
        Enrage,         // Gets stronger at low HP
        MultiPhase,     // Changes strategy at health thresholds
        Summoner,       // Spawns adds during fight
        Regeneration,   // Heals over time
        Counter,        // Reflects damage back
        ElementalShift, // Cycling resistances
        TimeLimit,      // Must win in X turns
        Berserker,      // High damage, low accuracy
        Defensive,      // High armor/defense
        Speed,          // Multiple attacks per turn
        Draining        // Life drain mechanics
    }
}