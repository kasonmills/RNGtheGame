using System.Collections.Generic;

namespace GameLogic.Entities.Enemies.Bosses
{
    /// <summary>
    /// One boss's hand-set level-1 stats - the numbers its fight stats grow from.
    /// BossEnemy.InitializeStats grows them to the boss's fight level through DifficultyScaler
    /// (health x(1 + 10% per level above 1) x3, damage x(1 + 8% per level above 1) x1.5,
    /// defense and speed + their growth per level). Accuracy, crit chance and crit multiplier
    /// don't grow. The difficulty is applied afterwards, once, by CombatManager.
    /// </summary>
    public class BossBaseStats
    {
        public int Health { get; init; }
        public int MinDamage { get; init; }
        public int MaxDamage { get; init; }
        public int Accuracy { get; init; }          // Hit chance (1-100)
        public int CritChance { get; init; }        // 1-100
        public double CritMultiplier { get; init; } = 1.5;
        public int Defense { get; init; }
        public double DefensePerLevel { get; init; }
        public int Speed { get; init; }
        public double SpeedPerLevel { get; init; }
    }

    /// <summary>
    /// Every boss's level-1 stats, by boss id. Approved 2026-11-10 from docs/boss_stats_draft.md
    /// (version 2, against the 100 HP player). A boss picks up its row when it is created, so the
    /// bosses still to be built (#2-#9) only need to use these ids in BossDefinitions.
    /// Change the numbers here - the draft is the reasoning behind them.
    /// </summary>
    public static class BossStatSheet
    {
        // Boss ids - the ones for bosses not built yet are reserved here so the sheet and
        // BossDefinitions can't drift apart.
        public const string Skarn = "skarn_eagle_bear";
        public const string LibrarianMonk = "librarian_monk";
        public const string Centipede = "giant_house_centipede";
        public const string CyclopsBrotherOne = "cyclops_brother_one";   // "Fast" (placeholder) - the faster brother, in the boss list
        public const string CyclopsBrotherTwo = "cyclops_brother_two";   // "Slow" (placeholder) - his partner
        public const string DemonPrince = "demon_prince";
        public const string SlimeDragon = "slime_dragon";
        public const string Captain = "captain_royal_guard";
        public const string FinalBoss = "final_boss";

        private static readonly Dictionary<string, BossBaseStats> _sheet = new()
        {
            // #1 Skarn - the tutorial teacher (fixed level 2). Tuned so the tutorial lasts about 5 rounds.
            [Skarn] = new BossBaseStats
            {
                Health = 70, MinDamage = 9, MaxDamage = 17, Accuracy = 72, CritChance = 5,
                Defense = 1, DefensePerLevel = 0, Speed = 12, SpeedPerLevel = 0
            },
            // #2 Librarian-Monk - slow wall; his shield is on top of this HP
            [LibrarianMonk] = new BossBaseStats
            {
                Health = 60, MinDamage = 6, MaxDamage = 11, Accuracy = 80, CritChance = 5,
                Defense = 2, DefensePerLevel = 0.25, Speed = 8, SpeedPerLevel = 0.2
            },
            // #3 Giant House Centipede - fast, the poison does the work
            [Centipede] = new BossBaseStats
            {
                Health = 55, MinDamage = 6, MaxDamage = 10, Accuracy = 85, CritChance = 5,
                Defense = 1, DefensePerLevel = 0.2, Speed = 14, SpeedPerLevel = 0.25
            },
            // #4/#5 Cyclops Brothers (each) - a pair, every attack lands twice
            [CyclopsBrotherOne] = new BossBaseStats
            {
                Health = 65, MinDamage = 4, MaxDamage = 7, Accuracy = 75, CritChance = 8,
                Defense = 3, DefensePerLevel = 0.25, Speed = 9, SpeedPerLevel = 0.2
            },
            [CyclopsBrotherTwo] = new BossBaseStats
            {
                Health = 65, MinDamage = 4, MaxDamage = 7, Accuracy = 75, CritChance = 8,
                Defense = 3, DefensePerLevel = 0.25, Speed = 7, SpeedPerLevel = 0.2
            },
            // #6 Demon Prince - weak hits, a condition on every hit
            [DemonPrince] = new BossBaseStats
            {
                Health = 85, MinDamage = 4, MaxDamage = 8, Accuracy = 85, CritChance = 5,
                Defense = 2, DefensePerLevel = 0.25, Speed = 13, SpeedPerLevel = 0.25
            },
            // #7 Slime Dragon - big hits, slow; the draglings are extra
            [SlimeDragon] = new BossBaseStats
            {
                Health = 85, MinDamage = 11, MaxDamage = 19, Accuracy = 75, CritChance = 5,
                Defense = 3, DefensePerLevel = 0.3, Speed = 8, SpeedPerLevel = 0.2
            },
            // #8 Captain of the Royal Guard - glass cannon, crits
            [Captain] = new BossBaseStats
            {
                Health = 70, MinDamage = 4, MaxDamage = 7, Accuracy = 90, CritChance = 55,
                CritMultiplier = 2.5, Defense = 2, DefensePerLevel = 0.2, Speed = 15, SpeedPerLevel = 0.3
            },
            // #9 Final boss - two turns a round, guards
            [FinalBoss] = new BossBaseStats
            {
                Health = 125, MinDamage = 8, MaxDamage = 14, Accuracy = 85, CritChance = 10,
                Defense = 3, DefensePerLevel = 0.3, Speed = 14, SpeedPerLevel = 0.3
            },
        };

        /// <summary>
        /// The level-1 stats for a boss id, or null if the boss has no row.
        /// </summary>
        public static BossBaseStats Get(string bossId)
        {
            if (bossId == null) return null;
            return _sheet.TryGetValue(bossId, out var stats) ? stats : null;
        }

        /// <summary>
        /// Every boss id on the sheet.
        /// </summary>
        public static IEnumerable<string> AllIds => _sheet.Keys;
    }
}
