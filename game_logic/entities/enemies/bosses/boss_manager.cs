using System;
using System.Collections.Generic;
using System.Linq;
using GameLogic.Systems;

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
        // (8 entries today - the Cyclops Brothers are one; the final boss is still to come).
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
        /// The boss list for a Godot screen, in story order (decided 2026-11-11): only the next
        /// boss the player hasn't beaten shows its information. Every boss after it is locked -
        /// no name, title or description, nothing at all, just that it is locked. Bosses already
        /// beaten show their name and title (my default - they have been met).
        /// </summary>
        public List<BossListEntry> GetBossList()
        {
            var list = new List<BossListEntry>();
            string nextBossId = GetNextBoss()?.BossId;

            for (int i = 0; i < _bossOrder.Count; i++)
            {
                var boss = _allBosses[_bossOrder[i]];
                bool defeated = IsBossDefeated(boss.BossId);
                bool isNext = boss.BossId == nextBossId;

                var entry = new BossListEntry
                {
                    Position = i + 1,
                    IsDefeated = defeated,
                    IsNext = isNext,
                    IsLocked = !defeated && !isNext
                };

                if (!entry.IsLocked)
                {
                    entry.BossId = boss.BossId;
                    entry.Name = boss.Name;
                    entry.Title = boss.Title;
                }
                if (isNext)
                {
                    entry.Description = boss.Description;
                    entry.UniqueAbilityDescription = boss.UniqueAbilityDescription;
                }

                list.Add(entry);
            }

            return list;
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
                GameLog.Write($"Boss '{bossId}' not found!");
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

                GameLog.Write($"\n🏆 BOSS DEFEATED!");
                GameLog.Write($"{boss.Name} has fallen for the first time!");
                GameLog.Write($"Unique bosses defeated: {_bossesDefeated}/{TotalBosses}");
            }
            else
            {
                // Repeat defeat
                GameLog.Write($"\n🏆 BOSS DEFEATED AGAIN!");
                GameLog.Write($"{boss.Name} has fallen once more!");
                GameLog.Write($"This is your {boss.TimesDefeated + 1}{GetOrdinalSuffix(boss.TimesDefeated + 1)} victory against this boss.");
            }

            // Increment repeat counter (tracks total defeats of THIS boss)
            boss.TimesDefeated++;
        }

        /// <summary>
        /// Apply strength scaling to a boss before combat. Returns the scaling summary for a
        /// pop-up. MVP: dormant (the call in CombatManager.StartCombat is commented out) - kept
        /// for a possible update/DLC.
        /// </summary>
        public string ApplyBossScaling(BossEnemy boss)
        {
            return boss.ApplyStrengthScaling(_bossesDefeated);
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

    /// <summary>
    /// One line of the boss list (BossManager.GetBossList). A locked entry has only Position and
    /// IsLocked - every other field is null, so a screen can't show anything about that boss.
    /// </summary>
    public class BossListEntry
    {
        public int Position { get; set; }                    // 1 = first in story order
        public bool IsDefeated { get; set; }
        public bool IsNext { get; set; }                     // The next boss to beat - the only one with full information
        public bool IsLocked { get; set; }                   // Show "Locked" - nothing else

        public string BossId { get; set; }                   // Null when locked
        public string Name { get; set; }                     // Null when locked
        public string Title { get; set; }                    // Null when locked
        public string Description { get; set; }              // The next boss only
        public string UniqueAbilityDescription { get; set; } // The next boss only
    }
}
