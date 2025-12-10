using System;
using System.Linq;
using Xunit;
using GameLogic.Systems;

namespace GameLogic.Tests
{
    /// <summary>
    /// Unit tests for StatisticsTracker
    /// Tests all tracking functionality and calculated statistics
    /// </summary>
    public class StatisticsTrackerTests
    {
        #region Initialization Tests

        [Fact]
        public void Constructor_InitializesWithDefaultValues()
        {
            // Act
            var stats = new StatisticsTracker();

            // Assert
            Assert.Equal(0, stats.TotalBattlesFought);
            Assert.Equal(0, stats.BattlesWon);
            Assert.Equal(0, stats.EnemiesKilled);
            Assert.Equal(0, stats.TotalGoldEarned);
            Assert.NotNull(stats.KillsByEnemyType);
            Assert.NotNull(stats.BossDefeats);
            Assert.NotNull(stats.WeaponsUsed);
            Assert.NotNull(stats.ConsumablesByType);
        }

        [Fact]
        public void Constructor_SetsGameStartDate()
        {
            // Arrange
            var before = DateTime.Now.AddSeconds(-1);

            // Act
            var stats = new StatisticsTracker();

            // Assert
            var after = DateTime.Now.AddSeconds(1);
            Assert.True(stats.GameStartDate >= before);
            Assert.True(stats.GameStartDate <= after);
        }

        [Fact]
        public void Constructor_SetsGameSessionsToOne()
        {
            // Act
            var stats = new StatisticsTracker();

            // Assert
            Assert.Equal(1, stats.GameSessions);
        }

        #endregion

        #region Combat Statistics Tests

        [Fact]
        public void RecordBattleStart_IncrementsTotalBattles()
        {
            // Arrange
            var stats = new StatisticsTracker();

            // Act
            stats.RecordBattleStart();
            stats.RecordBattleStart();
            stats.RecordBattleStart();

            // Assert
            Assert.Equal(3, stats.TotalBattlesFought);
        }

        [Fact]
        public void RecordBattleVictory_IncrementsBattlesWon()
        {
            // Arrange
            var stats = new StatisticsTracker();

            // Act
            stats.RecordBattleVictory(100, 50, false, false);
            stats.RecordBattleVictory(150, 30, false, false);

            // Assert
            Assert.Equal(2, stats.BattlesWon);
        }

        [Fact]
        public void RecordBattleVictory_TracksDamage()
        {
            // Arrange
            var stats = new StatisticsTracker();

            // Act
            stats.RecordBattleVictory(100, 50, false, false);
            stats.RecordBattleVictory(150, 30, false, false);

            // Assert
            Assert.Equal(250, stats.TotalDamageDealt);
            Assert.Equal(80, stats.TotalDamageTaken);
        }

        [Fact]
        public void RecordBattleVictory_IncrementsWinStreak()
        {
            // Arrange
            var stats = new StatisticsTracker();

            // Act
            stats.RecordBattleVictory(100, 50, false, false);
            stats.RecordBattleVictory(100, 50, false, false);
            stats.RecordBattleVictory(100, 50, false, false);

            // Assert
            Assert.Equal(3, stats.CurrentWinStreak);
            Assert.Equal(3, stats.LongestWinStreak);
        }

        [Fact]
        public void RecordBattleLoss_ResetsWinStreak()
        {
            // Arrange
            var stats = new StatisticsTracker();
            stats.RecordBattleVictory(100, 50, false, false);
            stats.RecordBattleVictory(100, 50, false, false);

            // Act
            stats.RecordBattleLoss();

            // Assert
            Assert.Equal(0, stats.CurrentWinStreak);
            Assert.Equal(2, stats.LongestWinStreak);
            Assert.Equal(1, stats.BattlesLost);
        }

        [Fact]
        public void RecordBattleFlee_IncrementsFleeAndResetsStreak()
        {
            // Arrange
            var stats = new StatisticsTracker();
            stats.RecordBattleVictory(100, 50, false, false);

            // Act
            stats.RecordBattleFlee();

            // Assert
            Assert.Equal(1, stats.BattlesFled);
            Assert.Equal(0, stats.CurrentWinStreak);
        }

        [Fact]
        public void RecordBattleVictory_Flawless_IncrementsFlawlessVictories()
        {
            // Arrange
            var stats = new StatisticsTracker();

            // Act
            stats.RecordBattleVictory(100, 0, true, false);

            // Assert
            Assert.Equal(1, stats.FlawlessVictories);
        }

        [Fact]
        public void RecordBattleVictory_CloseCall_IncrementsCloseCallVictories()
        {
            // Arrange
            var stats = new StatisticsTracker();

            // Act
            stats.RecordBattleVictory(100, 50, false, true);

            // Assert
            Assert.Equal(1, stats.CloseCallVictories);
        }

        [Fact]
        public void RecordDamageDealt_UpdatesHighestDamage()
        {
            // Arrange
            var stats = new StatisticsTracker();

            // Act
            stats.RecordDamageDealt(50);
            stats.RecordDamageDealt(150);
            stats.RecordDamageDealt(100);

            // Assert
            Assert.Equal(150, stats.HighestDamageDealt);
        }

        [Fact]
        public void RecordCriticalHit_IncrementsCritCount()
        {
            // Arrange
            var stats = new StatisticsTracker();

            // Act
            stats.RecordCriticalHit();
            stats.RecordCriticalHit();
            stats.RecordCriticalHit();

            // Assert
            Assert.Equal(3, stats.CriticalHitsLanded);
        }

        [Fact]
        public void RecordEnemyKill_TracksKillsByType()
        {
            // Arrange
            var stats = new StatisticsTracker();

            // Act
            stats.RecordEnemyKill("Goblin");
            stats.RecordEnemyKill("Goblin");
            stats.RecordEnemyKill("Orc");
            stats.RecordEnemyKill("Goblin");

            // Assert
            Assert.Equal(3, stats.EnemiesKilled);
            Assert.Equal(2, stats.KillsByEnemyType["Goblin"]);
            Assert.Equal(1, stats.KillsByEnemyType["Orc"]);
        }

        [Fact]
        public void RecordBossDefeat_TracksBossDefeats()
        {
            // Arrange
            var stats = new StatisticsTracker();

            // Act
            stats.RecordBossDefeat("Dragon King");
            stats.RecordBossDefeat("Demon Lord");
            stats.RecordBossDefeat("Dragon King");

            // Assert
            Assert.Equal(2, stats.BossesDefeated);
            Assert.Equal(2, stats.BossDefeats["Dragon King"]);
            Assert.Equal(1, stats.BossDefeats["Demon Lord"]);
        }

        [Fact]
        public void RecordPerfectCritRun_IncrementsPerfectCritRuns()
        {
            // Arrange
            var stats = new StatisticsTracker();

            // Act
            stats.RecordPerfectCritRun();

            // Assert
            Assert.Equal(1, stats.PerfectCritRuns);
        }

        #endregion

        #region Economic Statistics Tests

        [Fact]
        public void RecordGoldEarned_UpdatesTotalGoldEarned()
        {
            // Arrange
            var stats = new StatisticsTracker();

            // Act
            stats.RecordGoldEarned(100);
            stats.RecordGoldEarned(250);

            // Assert
            Assert.Equal(350, stats.TotalGoldEarned);
        }

        [Fact]
        public void RecordGoldSpent_UpdatesTotalGoldSpent()
        {
            // Arrange
            var stats = new StatisticsTracker();

            // Act
            stats.RecordGoldSpent(50);
            stats.RecordGoldSpent(75);

            // Assert
            Assert.Equal(125, stats.TotalGoldSpent);
        }

        [Fact]
        public void RecordItemPurchase_TracksItemsAndValue()
        {
            // Arrange
            var stats = new StatisticsTracker();

            // Act
            stats.RecordItemPurchase("Sword", 100);
            stats.RecordItemPurchase("Shield", 75);

            // Assert
            Assert.Equal(2, stats.ItemsBought);
            Assert.Equal(175, stats.TotalValueBought);
            Assert.Equal(175, stats.TotalGoldSpent);
        }

        [Fact]
        public void RecordItemPurchase_UpdatesMostExpensive()
        {
            // Arrange
            var stats = new StatisticsTracker();

            // Act
            stats.RecordItemPurchase("Sword", 100);
            stats.RecordItemPurchase("Legendary Axe", 500);
            stats.RecordItemPurchase("Potion", 10);

            // Assert
            Assert.Equal(500, stats.MostExpensivePurchase);
            Assert.Equal("Legendary Axe", stats.MostExpensivePurchaseName);
        }

        [Fact]
        public void RecordItemSale_TracksItemsAndValue()
        {
            // Arrange
            var stats = new StatisticsTracker();

            // Act
            stats.RecordItemSale("Old Sword", 50);
            stats.RecordItemSale("Potion", 5);

            // Assert
            Assert.Equal(2, stats.ItemsSold);
            Assert.Equal(55, stats.TotalValueSold);
            Assert.Equal(55, stats.TotalGoldEarned);
        }

        #endregion

        #region Equipment Statistics Tests

        [Fact]
        public void RecordWeaponUpgrade_IncrementsUpgradeCount()
        {
            // Arrange
            var stats = new StatisticsTracker();

            // Act
            stats.RecordWeaponUpgrade();
            stats.RecordWeaponUpgrade();

            // Assert
            Assert.Equal(2, stats.WeaponUpgradesPerformed);
        }

        [Fact]
        public void RecordArmorUpgrade_IncrementsUpgradeCount()
        {
            // Arrange
            var stats = new StatisticsTracker();

            // Act
            stats.RecordArmorUpgrade();

            // Assert
            Assert.Equal(1, stats.ArmorUpgradesPerformed);
        }

        [Fact]
        public void RecordWeaponEquip_TracksWeaponUsage()
        {
            // Arrange
            var stats = new StatisticsTracker();

            // Act
            stats.RecordWeaponEquip("Iron Sword", 1);
            stats.RecordWeaponEquip("Steel Sword", 5);
            stats.RecordWeaponEquip("Iron Sword", 2);

            // Assert
            Assert.Equal(2, stats.WeaponsUsed["Iron Sword"]);
            Assert.Equal(1, stats.WeaponsUsed["Steel Sword"]);
            Assert.Equal(3, stats.TotalEquipmentChanges);
        }

        [Fact]
        public void RecordWeaponEquip_UpdatesHighestLevel()
        {
            // Arrange
            var stats = new StatisticsTracker();

            // Act
            stats.RecordWeaponEquip("Sword", 5);
            stats.RecordWeaponEquip("Axe", 10);
            stats.RecordWeaponEquip("Spear", 7);

            // Assert
            Assert.Equal(10, stats.HighestWeaponLevel);
        }

        [Fact]
        public void RecordArmorEquip_UpdatesHighestLevel()
        {
            // Arrange
            var stats = new StatisticsTracker();

            // Act
            stats.RecordArmorEquip(3);
            stats.RecordArmorEquip(7);
            stats.RecordArmorEquip(5);

            // Assert
            Assert.Equal(7, stats.HighestArmorLevel);
        }

        #endregion

        #region Item Usage Statistics Tests

        [Fact]
        public void RecordConsumableUse_TracksConsumablesByType()
        {
            // Arrange
            var stats = new StatisticsTracker();

            // Act
            stats.RecordConsumableUse("Health Potion");
            stats.RecordConsumableUse("Health Potion");
            stats.RecordConsumableUse("Mana Potion");

            // Assert
            Assert.Equal(3, stats.ConsumablesUsed);
            Assert.Equal(2, stats.ConsumablesByType["Health Potion"]);
            Assert.Equal(1, stats.ConsumablesByType["Mana Potion"]);
        }

        [Fact]
        public void RecordConsumableUse_TracksHealingPotions()
        {
            // Arrange
            var stats = new StatisticsTracker();

            // Act
            stats.RecordConsumableUse("Healing Potion");
            stats.RecordConsumableUse("Health Potion");

            // Assert
            Assert.Equal(2, stats.HealingPotionsUsed);
        }

        [Fact]
        public void RecordConsumableUse_TracksRevivalPotions()
        {
            // Arrange
            var stats = new StatisticsTracker();

            // Act
            stats.RecordConsumableUse("Revival Potion");
            stats.RecordConsumableUse("Revive Potion");

            // Assert
            Assert.Equal(2, stats.RevivalPotionsUsed);
        }

        [Fact]
        public void RecordAbilityUse_IncrementsAbilityCount()
        {
            // Arrange
            var stats = new StatisticsTracker();

            // Act
            stats.RecordAbilityUse();
            stats.RecordAbilityUse();
            stats.RecordAbilityUse();

            // Assert
            Assert.Equal(3, stats.AbilitiesActivated);
        }

        #endregion

        #region Exploration Statistics Tests

        [Fact]
        public void RecordShopVisit_IncrementsShopVisits()
        {
            // Arrange
            var stats = new StatisticsTracker();

            // Act
            stats.RecordShopVisit();
            stats.RecordShopVisit();

            // Assert
            Assert.Equal(2, stats.ShopsVisited);
        }

        [Fact]
        public void RecordNPCInteraction_IncrementsNPCInteractions()
        {
            // Arrange
            var stats = new StatisticsTracker();

            // Act
            stats.RecordNPCInteraction();

            // Assert
            Assert.Equal(1, stats.NPCsInteracted);
        }

        [Fact]
        public void QuestMethods_TrackQuestProgress()
        {
            // Arrange
            var stats = new StatisticsTracker();

            // Act
            stats.RecordQuestAccepted();
            stats.RecordQuestAccepted();
            stats.RecordQuestCompleted();
            stats.RecordQuestClaimed();

            // Assert
            Assert.Equal(2, stats.QuestsAccepted);
            Assert.Equal(1, stats.QuestsCompleted);
            Assert.Equal(1, stats.QuestsClaimed);
        }

        #endregion

        #region Progression Statistics Tests

        [Fact]
        public void RecordLevelUp_TracksLevelAndExperience()
        {
            // Arrange
            var stats = new StatisticsTracker();

            // Act
            stats.RecordLevelUp(5, 1000);
            stats.RecordLevelUp(6, 1200);

            // Assert
            Assert.Equal(2, stats.LevelsGained);
            Assert.Equal(6, stats.CurrentLevel);
            Assert.Equal(6, stats.HighestLevelReached);
            Assert.Equal(2200, stats.TotalExperienceEarned);
        }

        [Fact]
        public void UpdateCurrentLevel_UpdatesLevelAndHighest()
        {
            // Arrange
            var stats = new StatisticsTracker();

            // Act
            stats.UpdateCurrentLevel(10);
            stats.UpdateCurrentLevel(15);
            stats.UpdateCurrentLevel(12);

            // Assert
            Assert.Equal(12, stats.CurrentLevel);
            Assert.Equal(15, stats.HighestLevelReached);
        }

        #endregion

        #region Miscellaneous Statistics Tests

        [Fact]
        public void RecordGameSave_IncrementsGamesSaved()
        {
            // Arrange
            var stats = new StatisticsTracker();

            // Act
            stats.RecordGameSave();
            stats.RecordGameSave();

            // Assert
            Assert.Equal(2, stats.GamesSaved);
        }

        [Fact]
        public void RecordGameLoad_IncrementsLoadsAndSessions()
        {
            // Arrange
            var stats = new StatisticsTracker();
            var initialSessions = stats.GameSessions;

            // Act
            stats.RecordGameLoad();

            // Assert
            Assert.Equal(1, stats.GamesLoaded);
            Assert.Equal(initialSessions + 1, stats.GameSessions);
        }

        [Fact]
        public void UpdatePlayTime_UpdatesTotalPlayTime()
        {
            // Arrange
            var stats = new StatisticsTracker();
            var playTime = TimeSpan.FromHours(5.5);

            // Act
            stats.UpdatePlayTime(playTime);

            // Assert
            Assert.Equal(playTime, stats.TotalPlayTime);
        }

        #endregion

        #region Calculated Statistics Tests

        [Fact]
        public void GetWinRate_CalculatesCorrectly()
        {
            // Arrange
            var stats = new StatisticsTracker();
            stats.RecordBattleStart();
            stats.RecordBattleStart();
            stats.RecordBattleStart();
            stats.RecordBattleStart();
            stats.RecordBattleVictory(100, 50, false, false);
            stats.RecordBattleVictory(100, 50, false, false);
            stats.RecordBattleVictory(100, 50, false, false);

            // Act
            double winRate = stats.GetWinRate();

            // Assert
            Assert.Equal(75.0, winRate); // 3 wins / 4 battles = 75%
        }

        [Fact]
        public void GetWinRate_NoBattles_ReturnsZero()
        {
            // Arrange
            var stats = new StatisticsTracker();

            // Act
            double winRate = stats.GetWinRate();

            // Assert
            Assert.Equal(0, winRate);
        }

        [Fact]
        public void GetAverageDamagePerBattle_CalculatesCorrectly()
        {
            // Arrange
            var stats = new StatisticsTracker();
            stats.RecordBattleVictory(100, 50, false, false);
            stats.RecordBattleVictory(200, 30, false, false);

            // Act
            double avgDamage = stats.GetAverageDamagePerBattle();

            // Assert
            Assert.Equal(150.0, avgDamage); // (100 + 200) / 2
        }

        [Fact]
        public void GetNetGold_CalculatesCorrectly()
        {
            // Arrange
            var stats = new StatisticsTracker();
            stats.RecordGoldEarned(1000);
            stats.RecordGoldSpent(350);

            // Act
            long netGold = stats.GetNetGold();

            // Assert
            Assert.Equal(650, netGold);
        }

        [Fact]
        public void GetBossWinRate_CalculatesCorrectly()
        {
            // Arrange
            var stats = new StatisticsTracker();
            stats.RecordBossAttempt();
            stats.RecordBossAttempt();
            stats.RecordBossAttempt();
            stats.RecordBossAttempt();
            stats.RecordBossDefeat("Boss1");
            stats.RecordBossDefeat("Boss2");

            // Act
            double bossWinRate = stats.GetBossWinRate();

            // Assert
            Assert.Equal(50.0, bossWinRate); // 2 / 4 = 50%
        }

        [Fact]
        public void GetMostKilledEnemy_ReturnsCorrectEnemy()
        {
            // Arrange
            var stats = new StatisticsTracker();
            stats.RecordEnemyKill("Goblin");
            stats.RecordEnemyKill("Orc");
            stats.RecordEnemyKill("Goblin");
            stats.RecordEnemyKill("Goblin");
            stats.RecordEnemyKill("Orc");

            // Act
            string mostKilled = stats.GetMostKilledEnemy();

            // Assert
            Assert.Equal("Goblin", mostKilled);
            Assert.Equal(3, stats.GetMostKilledEnemyCount());
        }

        [Fact]
        public void GetFavoriteWeapon_ReturnsCorrectWeapon()
        {
            // Arrange
            var stats = new StatisticsTracker();
            stats.RecordWeaponEquip("Sword", 1);
            stats.RecordWeaponEquip("Axe", 1);
            stats.RecordWeaponEquip("Sword", 2);
            stats.RecordWeaponEquip("Sword", 3);

            // Act
            string favorite = stats.GetFavoriteWeapon();

            // Assert
            Assert.Equal("Sword", favorite);
        }

        [Fact]
        public void GetMostUsedConsumable_ReturnsCorrectItem()
        {
            // Arrange
            var stats = new StatisticsTracker();
            stats.RecordConsumableUse("Health Potion");
            stats.RecordConsumableUse("Mana Potion");
            stats.RecordConsumableUse("Health Potion");

            // Act
            string mostUsed = stats.GetMostUsedConsumable();

            // Assert
            Assert.Equal("Health Potion", mostUsed);
        }

        [Fact]
        public void GetFormattedPlayTime_FormatsHoursAndMinutes()
        {
            // Arrange
            var stats = new StatisticsTracker();
            stats.UpdatePlayTime(TimeSpan.FromHours(2.5));

            // Act
            string formatted = stats.GetFormattedPlayTime();

            // Assert
            Assert.Contains("2h", formatted);
            Assert.Contains("30m", formatted);
        }

        [Fact]
        public void GetFormattedPlayTime_FormatsMinutesOnly()
        {
            // Arrange
            var stats = new StatisticsTracker();
            stats.UpdatePlayTime(TimeSpan.FromMinutes(45));

            // Act
            string formatted = stats.GetFormattedPlayTime();

            // Assert
            Assert.Contains("45m", formatted);
        }

        [Fact]
        public void GetFormattedPlayTime_FormatsSecondsOnly()
        {
            // Arrange
            var stats = new StatisticsTracker();
            stats.UpdatePlayTime(TimeSpan.FromSeconds(30));

            // Act
            string formatted = stats.GetFormattedPlayTime();

            // Assert
            Assert.Contains("30s", formatted);
        }

        #endregion

        #region Edge Cases Tests

        [Fact]
        public void GetMostKilledEnemy_NoKills_ReturnsNone()
        {
            // Arrange
            var stats = new StatisticsTracker();

            // Act
            string mostKilled = stats.GetMostKilledEnemy();

            // Assert
            Assert.Equal("None", mostKilled);
            Assert.Equal(0, stats.GetMostKilledEnemyCount());
        }

        [Fact]
        public void GetFavoriteWeapon_NoWeapons_ReturnsNone()
        {
            // Arrange
            var stats = new StatisticsTracker();

            // Act
            string favorite = stats.GetFavoriteWeapon();

            // Assert
            Assert.Equal("None", favorite);
        }

        [Fact]
        public void GetMostUsedConsumable_NoConsumables_ReturnsNone()
        {
            // Arrange
            var stats = new StatisticsTracker();

            // Act
            string mostUsed = stats.GetMostUsedConsumable();

            // Assert
            Assert.Equal("None", mostUsed);
        }

        #endregion
    }
}