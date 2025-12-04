using System;
using System.Collections.Generic;
using System.Linq;
using GameLogic.Entities.Enemies;
using GameLogic.Items;

namespace GameLogic.Progression
{
    /// <summary>
    /// Manages boss progression, tracking, and the champion key system
    /// Handles boss strength scaling and final gate unlock logic
    /// </summary>
    public class BossManager
    {
        // Boss tracking
        private Dictionary<string, BossEnemy> _allBosses;           // All registered bosses
        private List<string> _defeatedBossIds;                       // IDs of defeated bosses
        private int _bossesDefeated;                                 // Count of defeated bosses

        // Final boss configuration
        public string FinalBossId { get; private set; }             // Randomly selected final boss
        public bool FinalGateUnlocked { get; private set; }         // Is final boss accessible?

        // Configuration
        public const int TOTAL_BOSSES = 15;                          // Total champion bosses available
        public const int KEYS_REQUIRED = 10;                         // Keys needed to unlock final gate
        public const double STRENGTH_SCALING_PER_BOSS = 0.15;        // 15% stronger per boss defeated
        public const double REPEAT_PENALTY_PER_DEFEAT = 0.50;        // 50% stronger per repeat of same boss

        /// <summary>
        /// Constructor
        /// </summary>
        public BossManager()
        {
            _allBosses = new Dictionary<string, BossEnemy>();
            _defeatedBossIds = new List<string>();
            _bossesDefeated = 0;
            FinalGateUnlocked = false;
            FinalBossId = null;
        }

        /// <summary>
        /// Register a boss in the system
        /// </summary>
        public void RegisterBoss(BossEnemy boss)
        {
            if (!_allBosses.ContainsKey(boss.BossId))
            {
                _allBosses[boss.BossId] = boss;
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
        /// Select a random final boss from available bosses
        /// Called at game start / save creation
        /// </summary>
        public void SelectRandomFinalBoss(Systems.RNGManager rng)
        {
            if (_allBosses.Count == 0)
            {
                Console.WriteLine("No bosses registered! Cannot select final boss.");
                return;
            }

            // Pick a random boss to be the final boss
            var bossIds = _allBosses.Keys.ToList();
            int randomIndex = rng.Roll(0, bossIds.Count - 1);
            FinalBossId = bossIds[randomIndex];

            Console.WriteLine($"\n🎲 The Final Champion has been determined...");
            Console.WriteLine($"Defeat 10 of the 15 Champions to face: {_allBosses[FinalBossId].Name}");
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
        /// Check if a boss has been defeated
        /// </summary>
        public bool IsBossDefeated(string bossId)
        {
            return _defeatedBossIds.Contains(bossId);
        }

        /// <summary>
        /// Mark a boss as defeated and update scaling
        /// Returns the boss key that should be dropped (may be null on repeat defeats with bad RNG)
        /// Handles both first-time defeats and repeat fights
        /// </summary>
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

        /// <summary>
        /// Apply strength scaling to a boss before combat
        /// </summary>
        public void ApplyBossScaling(BossEnemy boss)
        {
            boss.ApplyStrengthScaling(_bossesDefeated);
        }

        /// <summary>
        /// Check if player has enough keys to unlock final gate
        /// </summary>
        private void CheckFinalGateUnlock()
        {
            if (_bossesDefeated >= KEYS_REQUIRED && !FinalGateUnlocked)
            {
                FinalGateUnlocked = true;
                Console.WriteLine($"\n✨✨✨ THE FINAL GATE HAS BEEN UNLOCKED! ✨✨✨");
                Console.WriteLine($"You have proven yourself against {KEYS_REQUIRED} Champions.");
                Console.WriteLine($"The path to {_allBosses[FinalBossId].Name} is now open!");
            }
        }

        /// <summary>
        /// Count unique champion keys in player inventory
        /// </summary>
        public int CountChampionKeys(Player.PlayerInventory inventory)
        {
            int keyCount = 0;

            foreach (var item in inventory.GetAllItems())
            {
                if (item is QuestItem questItem && IsChampionKey(questItem))
                {
                    keyCount++;
                }
            }

            return keyCount;
        }

        /// <summary>
        /// Check if an item is a champion key
        /// </summary>
        private bool IsChampionKey(QuestItem item)
        {
            // Champion keys follow naming pattern: "{boss_id}_key"
            foreach (var boss in _allBosses.Values)
            {
                if (item.QuestId == boss.KeyId)
                {
                    return true;
                }
            }
            return false;
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
                .Where(boss => !IsBossDefeated(boss.BossId) && boss.BossId != FinalBossId)
                .ToList();
        }

        /// <summary>
        /// Check if player can fight the final boss
        /// </summary>
        public bool CanFightFinalBoss(Player.PlayerInventory inventory)
        {
            int keyCount = CountChampionKeys(inventory);
            return keyCount >= KEYS_REQUIRED;
        }

        /// <summary>
        /// Get final boss (if unlocked and selected)
        /// </summary>
        public BossEnemy GetFinalBoss()
        {
            if (string.IsNullOrEmpty(FinalBossId))
            {
                return null;
            }

            return GetBoss(FinalBossId);
        }

        /// <summary>
        /// Display final gate status
        /// </summary>
        public string GetFinalGateStatus(Player.PlayerInventory inventory)
        {
            int keyCount = CountChampionKeys(inventory);
            string status = "═══ FINAL GATE STATUS ═══\n";

            if (FinalGateUnlocked)
            {
                status += "🔓 UNLOCKED\n";
                status += $"Final Boss: {_allBosses[FinalBossId].Name}\n";
                status += "You may now challenge the ultimate Champion!\n";
            }
            else
            {
                status += "🔒 SEALED\n";
                status += $"Champion Keys: {keyCount}/{KEYS_REQUIRED}\n";
                status += $"Defeat {KEYS_REQUIRED - keyCount} more Champions to unlock.\n";
            }

            return status;
        }

        /// <summary>
        /// Get progression summary
        /// </summary>
        public string GetProgressionSummary()
        {
            string summary = "═══ CHAMPION PROGRESSION ═══\n";
            summary += $"Bosses Defeated: {_bossesDefeated}/{TOTAL_BOSSES}\n";
            summary += $"Final Gate: {(FinalGateUnlocked ? "UNLOCKED" : "LOCKED")}\n";

            if (_bossesDefeated > 0)
            {
                summary += "\nDefeated Champions:\n";
                foreach (var bossId in _defeatedBossIds)
                {
                    if (_allBosses.ContainsKey(bossId))
                    {
                        summary += $"  • {_allBosses[bossId].Name}\n";
                    }
                }
            }

            int remaining = TOTAL_BOSSES - _bossesDefeated;
            if (remaining > 0)
            {
                summary += $"\nRemaining Champions: {remaining}\n";
            }

            if (FinalGateUnlocked && !string.IsNullOrEmpty(FinalBossId))
            {
                summary += $"\n⚔️  Final Boss: {_allBosses[FinalBossId].Name}\n";
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
    }
}