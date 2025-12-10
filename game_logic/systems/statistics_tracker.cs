using System;
using System.Collections.Generic;
using System.Linq;

namespace GameLogic.Systems
{
    /// <summary>
    /// Tracks all player statistics and achievements throughout the game
    /// </summary>
    public class StatisticsTracker
    {
        // ===== Combat Statistics =====
        public int TotalBattlesFought { get; set; }
        public int BattlesWon { get; set; }
        public int BattlesLost { get; set; }
        public int BattlesFled { get; set; }
        public long TotalDamageDealt { get; set; }
        public long TotalDamageTaken { get; set; }
        public int HighestDamageDealt { get; set; }
        public int CriticalHitsLanded { get; set; }
        public int TotalPlayerDeaths { get; set; }
        public int EnemiesKilled { get; set; }
        public Dictionary<string, int> KillsByEnemyType { get; set; } // enemyName -> killCount

        // Boss Statistics
        public int BossesDefeated { get; set; }
        public int BossAttempts { get; set; }
        public Dictionary<string, int> BossDefeats { get; set; } // bossName -> defeatCount

        // ===== Economic Statistics =====
        public long TotalGoldEarned { get; set; }
        public long TotalGoldSpent { get; set; }
        public int CurrentGold { get; set; }
        public int ItemsBought { get; set; }
        public int ItemsSold { get; set; }
        public long TotalValueBought { get; set; }
        public long TotalValueSold { get; set; }
        public int MostExpensivePurchase { get; set; }
        public string MostExpensivePurchaseName { get; set; }

        // ===== Equipment Statistics =====
        public int WeaponUpgradesPerformed { get; set; }
        public int ArmorUpgradesPerformed { get; set; }
        public int HighestWeaponLevel { get; set; }
        public int HighestArmorLevel { get; set; }
        public Dictionary<string, int> WeaponsUsed { get; set; } // weaponName -> timesEquipped
        public int TotalEquipmentChanges { get; set; }

        // ===== Item Usage Statistics =====
        public int ConsumablesUsed { get; set; }
        public Dictionary<string, int> ConsumablesByType { get; set; } // itemName -> useCount
        public int HealingPotionsUsed { get; set; }
        public int RevivalPotionsUsed { get; set; }
        public int AbilitiesActivated { get; set; }

        // ===== Exploration Statistics =====
        public int ShopsVisited { get; set; }
        public int NPCsInteracted { get; set; }
        public int QuestsCompleted { get; set; }
        public int QuestsAccepted { get; set; }
        public int QuestsClaimed { get; set; }

        // ===== Progression Statistics =====
        public int CurrentLevel { get; set; }
        public int HighestLevelReached { get; set; }
        public long TotalExperienceEarned { get; set; }
        public int LevelsGained { get; set; }

        // ===== Miscellaneous Statistics =====
        public DateTime GameStartDate { get; set; }
        public TimeSpan TotalPlayTime { get; set; }
        public int GameSessions { get; set; }
        public int GamesSaved { get; set; }
        public int GamesLoaded { get; set; }
        public string GameVersion { get; set; }

        // ===== Streak/Achievement Statistics =====
        public int CurrentWinStreak { get; set; }
        public int LongestWinStreak { get; set; }
        public int FlawlessVictories { get; set; } // No damage taken
        public int CloseCallVictories { get; set; } // Victory with less than 10% HP
        public int PerfectCritRuns { get; set; } // All attacks were crits in a battle

        /// <summary>
        /// Initialize a new statistics tracker
        /// </summary>
        public StatisticsTracker()
        {
            KillsByEnemyType = new Dictionary<string, int>();
            BossDefeats = new Dictionary<string, int>();
            WeaponsUsed = new Dictionary<string, int>();
            ConsumablesByType = new Dictionary<string, int>();
            GameStartDate = DateTime.Now;
            GameVersion = "1.0.0"; // Update this with actual version
            TotalPlayTime = TimeSpan.Zero;
            GameSessions = 1; // Starting a new game is session 1
        }

        // ===== Combat Tracking Methods =====

        public void RecordBattleStart()
        {
            TotalBattlesFought++;
        }

        public void RecordBattleVictory(int damageDealt, int damageTaken, bool wasFlawless, bool wasCloseCall)
        {
            BattlesWon++;
            TotalDamageDealt += damageDealt;
            TotalDamageTaken += damageTaken;

            CurrentWinStreak++;
            if (CurrentWinStreak > LongestWinStreak)
            {
                LongestWinStreak = CurrentWinStreak;
            }

            if (wasFlawless)
            {
                FlawlessVictories++;
            }

            if (wasCloseCall)
            {
                CloseCallVictories++;
            }
        }

        public void RecordBattleLoss()
        {
            BattlesLost++;
            CurrentWinStreak = 0;
        }

        public void RecordBattleFlee()
        {
            BattlesFled++;
            CurrentWinStreak = 0;
        }

        public void RecordDamageDealt(int damage)
        {
            if (damage > HighestDamageDealt)
            {
                HighestDamageDealt = damage;
            }
        }

        public void RecordCriticalHit()
        {
            CriticalHitsLanded++;
        }

        public void RecordPlayerDeath()
        {
            TotalPlayerDeaths++;
        }

        public void RecordEnemyKill(string enemyName)
        {
            EnemiesKilled++;

            if (KillsByEnemyType.ContainsKey(enemyName))
            {
                KillsByEnemyType[enemyName]++;
            }
            else
            {
                KillsByEnemyType[enemyName] = 1;
            }
        }

        public void RecordBossAttempt()
        {
            BossAttempts++;
        }

        public void RecordBossDefeat(string bossName)
        {
            BossesDefeated++;

            if (BossDefeats.ContainsKey(bossName))
            {
                BossDefeats[bossName]++;
            }
            else
            {
                BossDefeats[bossName] = 1;
            }
        }

        public void RecordPerfectCritRun()
        {
            PerfectCritRuns++;
        }

        // ===== Economic Tracking Methods =====

        public void RecordGoldEarned(int amount)
        {
            TotalGoldEarned += amount;
        }

        public void RecordGoldSpent(int amount)
        {
            TotalGoldSpent += amount;
        }

        public void RecordItemPurchase(string itemName, int cost)
        {
            ItemsBought++;
            TotalValueBought += cost;
            RecordGoldSpent(cost);

            if (cost > MostExpensivePurchase)
            {
                MostExpensivePurchase = cost;
                MostExpensivePurchaseName = itemName;
            }
        }

        public void RecordItemSale(string itemName, int value)
        {
            ItemsSold++;
            TotalValueSold += value;
            RecordGoldEarned(value);
        }

        public void UpdateCurrentGold(int gold)
        {
            CurrentGold = gold;
        }

        // ===== Equipment Tracking Methods =====

        public void RecordWeaponUpgrade()
        {
            WeaponUpgradesPerformed++;
        }

        public void RecordArmorUpgrade()
        {
            ArmorUpgradesPerformed++;
        }

        public void RecordWeaponEquip(string weaponName, int level)
        {
            if (WeaponsUsed.ContainsKey(weaponName))
            {
                WeaponsUsed[weaponName]++;
            }
            else
            {
                WeaponsUsed[weaponName] = 1;
            }

            TotalEquipmentChanges++;

            if (level > HighestWeaponLevel)
            {
                HighestWeaponLevel = level;
            }
        }

        public void RecordArmorEquip(int level)
        {
            TotalEquipmentChanges++;

            if (level > HighestArmorLevel)
            {
                HighestArmorLevel = level;
            }
        }

        // ===== Item Usage Tracking Methods =====

        public void RecordConsumableUse(string itemName)
        {
            ConsumablesUsed++;

            if (ConsumablesByType.ContainsKey(itemName))
            {
                ConsumablesByType[itemName]++;
            }
            else
            {
                ConsumablesByType[itemName] = 1;
            }

            // Track specific types
            if (itemName.Contains("Healing") || itemName.Contains("Health"))
            {
                HealingPotionsUsed++;
            }
            else if (itemName.Contains("Revival") || itemName.Contains("Revive"))
            {
                RevivalPotionsUsed++;
            }
        }

        public void RecordAbilityUse()
        {
            AbilitiesActivated++;
        }

        // ===== Exploration Tracking Methods =====

        public void RecordShopVisit()
        {
            ShopsVisited++;
        }

        public void RecordNPCInteraction()
        {
            NPCsInteracted++;
        }

        public void RecordQuestAccepted()
        {
            QuestsAccepted++;
        }

        public void RecordQuestCompleted()
        {
            QuestsCompleted++;
        }

        public void RecordQuestClaimed()
        {
            QuestsClaimed++;
        }

        // ===== Progression Tracking Methods =====

        public void RecordLevelUp(int newLevel, int experienceGained)
        {
            LevelsGained++;
            CurrentLevel = newLevel;

            if (newLevel > HighestLevelReached)
            {
                HighestLevelReached = newLevel;
            }

            TotalExperienceEarned += experienceGained;
        }

        public void UpdateCurrentLevel(int level)
        {
            CurrentLevel = level;
            if (level > HighestLevelReached)
            {
                HighestLevelReached = level;
            }
        }

        // ===== Miscellaneous Tracking Methods =====

        public void RecordGameSave()
        {
            GamesSaved++;
        }

        public void RecordGameLoad()
        {
            GamesLoaded++;
            GameSessions++;
        }

        public void UpdatePlayTime(TimeSpan playTime)
        {
            TotalPlayTime = playTime;
        }

        // ===== Calculated Statistics =====

        public double GetWinRate()
        {
            if (TotalBattlesFought == 0) return 0;
            return (double)BattlesWon / TotalBattlesFought * 100;
        }

        public double GetAverageDamagePerBattle()
        {
            if (BattlesWon == 0) return 0;
            return (double)TotalDamageDealt / BattlesWon;
        }

        public long GetNetGold()
        {
            return TotalGoldEarned - TotalGoldSpent;
        }

        public double GetBossWinRate()
        {
            if (BossAttempts == 0) return 0;
            return (double)BossesDefeated / BossAttempts * 100;
        }

        public string GetMostKilledEnemy()
        {
            if (KillsByEnemyType.Count == 0) return "None";
            return KillsByEnemyType.OrderByDescending(x => x.Value).First().Key;
        }

        public int GetMostKilledEnemyCount()
        {
            if (KillsByEnemyType.Count == 0) return 0;
            return KillsByEnemyType.OrderByDescending(x => x.Value).First().Value;
        }

        public string GetFavoriteWeapon()
        {
            if (WeaponsUsed.Count == 0) return "None";
            return WeaponsUsed.OrderByDescending(x => x.Value).First().Key;
        }

        public string GetMostUsedConsumable()
        {
            if (ConsumablesByType.Count == 0) return "None";
            return ConsumablesByType.OrderByDescending(x => x.Value).First().Key;
        }

        public string GetFormattedPlayTime()
        {
            if (TotalPlayTime.TotalHours >= 1)
            {
                return $"{(int)TotalPlayTime.TotalHours}h {TotalPlayTime.Minutes}m";
            }
            else if (TotalPlayTime.TotalMinutes >= 1)
            {
                return $"{(int)TotalPlayTime.TotalMinutes}m {TotalPlayTime.Seconds}s";
            }
            else
            {
                return $"{TotalPlayTime.Seconds}s";
            }
        }
    }
}