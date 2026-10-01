using System;
using System.Collections.Generic;
using System.Linq;

namespace GameLogic.Entities.Enemies.Bosses
{
    /// <summary>
    /// Manages boss progression and tracking.
    /// Handles boss strength scaling and order-based unlock logic - defeating boss N-1
    /// unlocks boss N, following the fixed story order bosses are registered in.
    /// </summary>
    public class BossManager
    {
        // Boss tracking
        private Dictionary<string, BossEnemy> _allBosses;           // All registered bosses
        private List<string> _bossOrder;                             // Story order (registration order)
        private List<string> _defeatedBossIds;                       // IDs of defeated bosses
        private int _bossesDefeated;                                 // Count of defeated bosses

        // Public getters for save system
        public Dictionary<string, BossEnemy> AllBosses { get { return _allBosses; } }
        public List<string> DefeatedBossIds { get { return _defeatedBossIds; } }
        public int BossesDefeated { get { return _bossesDefeated; } }

        // Total bosses in the roster - tracks whatever has been registered so far
        // (currently 1 of the confirmed 9; grows as the other 8 are added).
        public int TotalBosses { get { return _allBosses.Count; } }

        // Configuration
        public const double STRENGTH_SCALING_PER_BOSS = 0.15;        // 15% stronger per boss defeated
        public const double REPEAT_PENALTY_PER_DEFEAT = 0.50;        // 50% stronger per repeat of same boss

        /// <summary>
        /// Constructor
        /// </summary>
        public BossManager()
        {
            _allBosses = new Dictionary<string, BossEnemy>();
            _bossOrder = new List<string>();
            _defeatedBossIds = new List<string>();
            _bossesDefeated = 0;
        }

        /// <summary>
        /// Register a boss in the system
        /// </summary>
        public void RegisterBoss(BossEnemy boss)
        {
            if (!_allBosses.ContainsKey(boss.BossId))
            {
                _allBosses[boss.BossId] = boss;
                _bossOrder.Add(boss.BossId);
            }
        }

        /// <summary>
        /// Register multiple bosses at once
        /// </summary>
        public void RegisterBosses(params BossEnemy[] bosses)
        {
            foreach (var boss in bosses)
            {
                RegisterBoss(boss);
            }
        }

        /// <summary>
        /// Get a boss by ID
        /// </summary>
        public BossEnemy GetBoss(string bossId)
        {
            _allBosses.TryGetValue(bossId, out BossEnemy boss);
            return boss;
        }

        /// <summary>
        /// Check whether a boss is currently accessible - the first boss in the story order
        /// is always unlocked, and every boss after that unlocks once the previous one falls.
        /// </summary>
        public bool IsBossUnlocked(string bossId)
        {
            int index = _bossOrder.IndexOf(bossId);
            if (index < 0) return false;
            if (index == 0) return true;

            string previousBossId = _bossOrder[index - 1];
            return IsBossDefeated(previousBossId);
        }

        /// <summary>
        /// Get the next boss the player needs to face (first undefeated boss in story order),
        /// or null if every registered boss has been defeated.
        /// </summary>
        public BossEnemy GetNextBoss()
        {
            foreach (var bossId in _bossOrder)
            {
                if (!IsBossDefeated(bossId))
                {
                    return _allBosses[bossId];
                }
            }

            return null;
        }

        /// <summary>
        /// Get the final boss - always the last boss in the fixed story order.
        /// </summary>
        public BossEnemy GetFinalBoss()
        {
            if (_bossOrder.Count == 0) return null;
            return GetBoss(_bossOrder[_bossOrder.Count - 1]);
        }

        /// <summary>
        /// Check whether a given boss is the final boss (last in story order).
        /// </summary>
        public bool IsFinalBoss(string bossId)
        {
            return _bossOrder.Count > 0 && bossId == _bossOrder[_bossOrder.Count - 1];
        }

        /// <summary>
        /// Check if a boss has been defeated
        /// </summary>
        public bool IsBossDefeated(string bossId)
        {
            return _defeatedBossIds.Contains(bossId);
        }

        /// <summary>
        /// Mark a boss as defeated and update scaling
        /// Handles both first-time defeats and repeat fights
        /// </summary>
        public void DefeatBoss(string bossId)
        {
            if (!_allBosses.TryGetValue(bossId, out BossEnemy boss))
            {
                Console.WriteLine($"Boss '{bossId}' not found!");
                return;
            }

            // Check if this is a first-time defeat or repeat
            bool isFirstDefeat = !IsBossDefeated(bossId);

            if (isFirstDefeat)
            {
                // First time defeating this boss
                boss.MarkDefeated();
                _defeatedBossIds.Add(bossId);
                _bossesDefeated++;

                Console.WriteLine($"\n🏆 BOSS DEFEATED!");
                Console.WriteLine($"{boss.Name} has fallen for the first time!");
                Console.WriteLine($"Unique bosses defeated: {_bossesDefeated}/{TotalBosses}");
            }
            else
            {
                // Repeat defeat
                Console.WriteLine($"\n🏆 BOSS DEFEATED AGAIN!");
                Console.WriteLine($"{boss.Name} has fallen once more!");
                Console.WriteLine($"This is your {boss.TimesDefeated + 1}{GetOrdinalSuffix(boss.TimesDefeated + 1)} victory against this boss.");
            }

            // Increment repeat counter (tracks total defeats of THIS boss)
            boss.TimesDefeated++;
        }

        /// <summary>
        /// Apply strength scaling to a boss before combat
        /// </summary>
        public void ApplyBossScaling(BossEnemy boss)
        {
            boss.ApplyStrengthScaling(_bossesDefeated);
        }

        /// <summary>
        /// Get list of defeated boss names
        /// </summary>
        public List<string> GetDefeatedBossNames()
        {
            return _defeatedBossIds
                .Select(id => _allBosses.ContainsKey(id) ? _allBosses[id].Name : id)
                .ToList();
        }

        /// <summary>
        /// Get list of remaining (undefeated) bosses
        /// </summary>
        public List<BossEnemy> GetRemainingBosses()
        {
            return _allBosses.Values
                .Where(boss => !IsBossDefeated(boss.BossId))
                .ToList();
        }

        /// <summary>
        /// Get progression summary
        /// </summary>
        public string GetProgressionSummary()
        {
            string summary = "═══ BOSS PROGRESSION ═══\n";
            summary += $"Bosses Defeated: {_bossesDefeated}/{TotalBosses}\n";

            if (_bossesDefeated > 0)
            {
                summary += "\nDefeated Bosses:\n";
                foreach (var bossId in _defeatedBossIds)
                {
                    if (_allBosses.ContainsKey(bossId))
                    {
                        summary += $"  • {_allBosses[bossId].Name}\n";
                    }
                }
            }

            var nextBoss = GetNextBoss();
            if (nextBoss != null)
            {
                summary += $"\n⚔️  Next Boss: {nextBoss.Name}\n";
            }

            return summary;
        }

        /// <summary>
        /// Get boss strength multiplier based on current progress
        /// </summary>
        public double GetCurrentStrengthMultiplier()
        {
            return 1.0 + (_bossesDefeated * STRENGTH_SCALING_PER_BOSS);
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
        /// Reset boss manager (for new game)
        /// </summary>
        public void Reset()
        {
            _defeatedBossIds.Clear();
            _bossesDefeated = 0;

            // Reset all bosses
            foreach (var boss in _allBosses.Values)
            {
                boss.IsDefeated = false;
                boss.BossNumber = 0;
                boss.TimesDefeated = 0;  // Reset repeat counter
            }
        }

        // === Setter Methods for Save System ===

        /// <summary>
        /// Add a defeated boss ID (used when loading from save)
        /// </summary>
        public void AddDefeatedBoss(string bossId)
        {
            if (!_defeatedBossIds.Contains(bossId))
            {
                _defeatedBossIds.Add(bossId);
            }
        }

        /// <summary>
        /// Set the boss defeated count (used when loading from save)
        /// </summary>
        public void SetBossesDefeated(int count)
        {
            _bossesDefeated = count;
        }
    }
}