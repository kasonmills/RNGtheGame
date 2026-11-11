using System;
using GameLogic.Entities.Enemies;
using GameLogic.Systems;

namespace GameLogic.Entities.Enemies.Bosses
{
    /// <summary>
    /// Boss enemy class - powerful unique enemies with their own combat mechanics.
    /// Each boss grows from its hand-set level-1 stats (BossStatSheet) to the level it is fought
    /// at - the party's average level + 5, or its own fixed level (Skarn).
    /// </summary>
    public class BossEnemy : EnemyBase
    {
        // Boss Identity
        public string BossId { get; set; }              // Unique boss identifier (e.g., "flame_warden")
        public string Title { get; set; }               // Boss title (e.g., "Guardian of the Eternal Flame")

        // Boss Tracking
        public bool IsDefeated { get; set; }            // Has this boss been defeated at least once?
        public int BossNumber { get; set; }             // Sequence number for scaling (managed by BossManager)

        /// <summary>
        /// True for a boss that keeps the level it was defined with instead of following the
        /// party (party average + 5, set by CombatManager when the fight starts). Only Skarn, the
        /// tutorial boss, so the tutorial can be tuned against the player's starting stats
        /// (decided 2026-11-10).
        /// </summary>
        public bool HasFixedLevel { get; set; }

        /// <summary>
        /// A second boss that is always fought together with this one, as one boss (the Cyclops
        /// Brothers: Fast is in the boss list, Slow is his partner). The partner isn't registered
        /// in BossManager - beating the pair counts as beating this boss, once. Null for most bosses.
        /// </summary>
        public BossEnemy PartnerBoss { get; set; }

        /// <summary>
        /// Everyone who comes into this boss's fight: the boss, and its partner if it has one.
        /// </summary>
        public System.Collections.Generic.List<EnemyBase> GetFightEnemies()
        {
            var enemies = new System.Collections.Generic.List<EnemyBase> { this };
            if (PartnerBoss != null) enemies.Add(PartnerBoss);
            return enemies;
        }
        public int TimesDefeated { get; set; }          // How many times this specific boss has been defeated

        // Unique Combat Mechanics
        public string UniqueAbilityDescription { get; set; }  // Description of unique mechanic
        public BossMechanicType MechanicType { get; set; }    // Type of unique mechanic (flavor/display only)

        /// <summary>
        /// The actual implementation of this boss's unique mechanic, if it has one.
        /// Null means "no override" - CombatManager falls back to the generic
        /// Behavior-based AI (i.e. MechanicType alone does nothing on its own).
        /// </summary>
        public IBossMechanic Mechanic { get; set; }

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
            int level,
            BossMechanicType mechanicType = BossMechanicType.Standard)
        {
            BossId = bossId;
            Name = name;
            Title = title;
            Description = description;
            Level = level;
            Type = EnemyType.Boss;
            Behavior = EnemyBehavior.Tactical;
            IsDefeated = false;
            BossNumber = 0;
            TimesDefeated = 0;
            MechanicType = mechanicType;

            // Set unique ability description based on mechanic type
            UniqueAbilityDescription = GetMechanicDescription(mechanicType);

            // Real stats from the start (CombatManager rebuilds them at the fight's level anyway).
            InitializeStats(level);
        }

        /// <summary>
        /// Initialize boss stats based on level
        /// Stores base stats for scaling later
        /// </summary>
        public override void InitializeStats(int level)
        {
            Level = level;
            DifficultyApplied = false; // Fresh stats - the difficulty is applied again at the next fight

            var sheet = BossStatSheet.Get(BossId);
            if (sheet != null)
            {
                // Hand-set level-1 stats grown to this level, boss multipliers included
                // (health x3, damage x1.5) - decided 2026-11-10. No difficulty yet.
                _baseHealth = DifficultyScaler.BossHealthAtLevel(sheet.Health, level);
                _baseMinDamage = DifficultyScaler.BossDamageAtLevel(sheet.MinDamage, level);
                _baseMaxDamage = Math.Max(_baseMinDamage, DifficultyScaler.BossDamageAtLevel(sheet.MaxDamage, level));
                _baseDefense = DifficultyScaler.BossStatAtLevel(sheet.Defense, sheet.DefensePerLevel, level);

                Accuracy = sheet.Accuracy;
                CritChance = sheet.CritChance;
                CritMultiplier = sheet.CritMultiplier;
                Speed = DifficultyScaler.BossStatAtLevel(sheet.Speed, sheet.SpeedPerLevel, level);
            }
            else
            {
                // Fallback for a boss with no row in BossStatSheet - the old shared stopgap formula
                _baseHealth = 100 + (level * 30);
                _baseMinDamage = 10 + (level * 3);
                _baseMaxDamage = 20 + (level * 5);
                _baseDefense = 5 + (level / 2);

                Accuracy = 75 + (level / 2);
                CritChance = 10 + (level / 5);
                Speed = 15 + (level / 3);
            }
            _baseMaxHealth = _baseHealth;

            Health = _baseHealth;
            MaxHealth = _baseMaxHealth;
            MinDamage = _baseMinDamage;
            MaxDamage = _baseMaxDamage;
            Defense = _baseDefense;

            // Boss rewards - not used by fight rewards yet (fight XP is rolled from the level and
            // gold comes from the loot table). Left for the XP rework.
            GoldValue = 100 + (level * 25);
            XPValue = 200 + (level * 50);
        }

        /// <summary>
        /// Apply strength scaling based on number of bosses defeated AND repeat fights
        /// Each defeated boss makes remaining bosses 15% stronger (progression scaling)
        /// Each repeat of THIS boss makes it 50% stronger (repeat penalty)
        /// Both scale together multiplicatively
        ///
        /// MVP (decided 2026-10-14): dormant - nothing calls this (CombatManager.StartCombat has
        /// the call to BossManager.ApplyBossScaling commented out). Kept for a possible
        /// update/DLC. Returns a summary of the scaling for a pop-up instead of printing it.
        /// </summary>
        public string ApplyStrengthScaling(int totalBossesDefeated)
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

            // Scaling summary, handed back for a pop-up (no console output)
            string summary = "Boss Strength Scaling Applied!\n";
            summary += $"Boss #{BossNumber} in your journey | Repeat: {TimesDefeated + 1}{GetOrdinalSuffix(TimesDefeated + 1)} fight\n";
            summary += $"Progression scaling: {progressionMultiplier:F2}x ({(progressionMultiplier - 1) * 100:F0}% from total progress)";

            if (TimesDefeated > 0)
            {
                // Repeat fight - show both scaling factors
                summary += $"\nRepeat penalty: {repeatPenalty:F2}x ({(repeatPenalty - 1) * 100:F0}% from {TimesDefeated} previous defeat{(TimesDefeated > 1 ? "s" : "")})";
                summary += $"\nCOMBINED STRENGTH: {totalMultiplier:F2}x ({(totalMultiplier - 1) * 100:F0}% total increase)";
            }

            return summary;
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
        /// Override GetDescription to include boss title
        /// </summary>
        public override string GetDescription()
        {
            // MVP: no strength label - the old 15%-per-boss multiplier is off (decided 2026-10-14).
            // Kept for a possible update/DLC:
            // string scalingInfo = BossNumber > 0 ? $" [Strength: x{1.0 + ((BossNumber - 1) * 0.15):F2}]" : "";
            // return $"{Name}, {Title} (Level {Level}){scalingInfo}";
            return $"{Name}, {Title} (Level {Level})";
        }

        /// <summary>
        /// Get full boss information display
        /// </summary>
        public string GetBossInfo()
        {
            string info = "═══════════════════════════════════════\n";
            info += "⚔️  BOSS  ⚔️\n";
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

            // MVP: no strength line (the old multiplier is off). Kept for a possible update/DLC:
            // if (BossNumber > 0)
            // {
            //     info += $"Boss #{BossNumber} | Strength: x{1.0 + ((BossNumber - 1) * 0.15):F2}\n";
            // }

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