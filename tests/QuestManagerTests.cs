using System;
using System.Linq;
using Xunit;
using GameLogic.Quests;
using GameLogic.Progression;

namespace GameLogic.Tests
{
    /// <summary>
    /// Unit tests for Quest System
    /// Tests quest management, state transitions, and quest types
    /// </summary>
    public class QuestManagerTests
    {
        #region Helper Methods

        private BossDefeatQuest CreateTestBossQuest(string id = "test_boss")
        {
            return new BossDefeatQuest(id, "Test Boss", 100, 50);
        }

        private LevelQuest CreateTestLevelQuest(int targetLevel = 5)
        {
            return new LevelQuest($"level_{targetLevel}", "Level Quest", targetLevel, 100, 50);
        }

        private EnemyKillQuest CreateTestKillQuest(int killCount = 10)
        {
            return new EnemyKillQuest("kill_test", "Kill Quest", killCount, 75, 40);
        }

        #endregion

        #region QuestManager Initialization Tests

        [Fact]
        public void Constructor_InitializesEmptyQuestList()
        {
            // Act
            var manager = new QuestManager();

            // Assert
            Assert.NotNull(manager.AllQuests);
            Assert.Empty(manager.AllQuests);
        }

        [Fact]
        public void Constructor_InitializesWithNoActiveQuest()
        {
            // Act
            var manager = new QuestManager();

            // Assert
            Assert.Null(manager.ActiveQuestId);
            Assert.Null(manager.GetActiveQuest());
        }

        #endregion

        #region Quest Registration Tests

        [Fact]
        public void RegisterQuest_AddsQuestToCollection()
        {
            // Arrange
            var manager = new QuestManager();
            var quest = CreateTestBossQuest();

            // Act
            manager.RegisterQuest(quest);

            // Assert
            Assert.Single(manager.AllQuests);
            Assert.Contains("test_boss", manager.AllQuests.Keys);
        }

        [Fact]
        public void RegisterQuest_MultipleQuests_AllRegistered()
        {
            // Arrange
            var manager = new QuestManager();
            var quest1 = CreateTestBossQuest("boss1");
            var quest2 = CreateTestBossQuest("boss2");
            var quest3 = CreateTestLevelQuest(5);

            // Act
            manager.RegisterQuest(quest1);
            manager.RegisterQuest(quest2);
            manager.RegisterQuest(quest3);

            // Assert
            Assert.Equal(3, manager.AllQuests.Count);
        }

        [Fact]
        public void GetQuestById_ExistingQuest_ReturnsQuest()
        {
            // Arrange
            var manager = new QuestManager();
            var quest = CreateTestBossQuest("findme");
            manager.RegisterQuest(quest);

            // Act
            var found = manager.GetQuestById("findme");

            // Assert
            Assert.NotNull(found);
            Assert.Equal("findme", found.QuestId);
        }

        [Fact]
        public void GetQuestById_NonExistentQuest_ReturnsNull()
        {
            // Arrange
            var manager = new QuestManager();

            // Act
            var found = manager.GetQuestById("nonexistent");

            // Assert
            Assert.Null(found);
        }

        #endregion

        #region Quest State Filtering Tests

        [Fact]
        public void GetQuestsByState_ReturnsCorrectQuests()
        {
            // Arrange
            var manager = new QuestManager();
            var quest1 = CreateTestBossQuest("boss1");
            var quest2 = CreateTestBossQuest("boss2");
            var quest3 = CreateTestBossQuest("boss3");

            quest1.Discover();
            quest2.Discover();
            quest2.Accept();
            quest3.Discover();

            manager.RegisterQuest(quest1);
            manager.RegisterQuest(quest2);
            manager.RegisterQuest(quest3);

            // Act
            var availableQuests = manager.GetQuestsByState(QuestState.Available);
            var acceptedQuests = manager.GetQuestsByState(QuestState.Accepted);

            // Assert
            Assert.Equal(2, availableQuests.Count);
            Assert.Single(acceptedQuests);
        }

        [Fact]
        public void GetQuestsByState_NoMatchingQuests_ReturnsEmpty()
        {
            // Arrange
            var manager = new QuestManager();
            var quest = CreateTestBossQuest();
            manager.RegisterQuest(quest);

            // Act
            var completedQuests = manager.GetQuestsByState(QuestState.Completed);

            // Assert
            Assert.Empty(completedQuests);
        }

        #endregion

        #region Quest Acceptance Tests

        [Fact]
        public void AcceptQuest_AvailableQuest_ChangesStateToAccepted()
        {
            // Arrange
            var manager = new QuestManager();
            var quest = CreateTestBossQuest();
            quest.Discover();
            manager.RegisterQuest(quest);

            // Act
            bool result = manager.AcceptQuest(quest.QuestId);

            // Assert
            Assert.True(result);
            Assert.Equal(QuestState.Accepted, quest.State);
        }

        [Fact]
        public void AcceptQuest_NotDiscoveredQuest_ReturnsFalse()
        {
            // Arrange
            var manager = new QuestManager();
            var quest = CreateTestBossQuest();
            manager.RegisterQuest(quest);

            // Act
            bool result = manager.AcceptQuest(quest.QuestId);

            // Assert
            Assert.False(result);
            Assert.Equal(QuestState.NotDiscovered, quest.State);
        }

        [Fact]
        public void AcceptQuest_AlreadyAcceptedQuest_ReturnsFalse()
        {
            // Arrange
            var manager = new QuestManager();
            var quest = CreateTestBossQuest();
            quest.Discover();
            quest.Accept();
            manager.RegisterQuest(quest);

            // Act
            bool result = manager.AcceptQuest(quest.QuestId);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region Active Quest Tests

        [Fact]
        public void SetActiveQuest_AcceptedQuest_SetsActiveQuest()
        {
            // Arrange
            var manager = new QuestManager();
            var quest = CreateTestBossQuest();
            quest.Discover();
            quest.Accept();
            manager.RegisterQuest(quest);

            // Act
            bool result = manager.SetActiveQuest(quest.QuestId);

            // Assert
            Assert.True(result);
            Assert.Equal(quest.QuestId, manager.ActiveQuestId);
            Assert.Equal(quest, manager.GetActiveQuest());
        }

        [Fact]
        public void SetActiveQuest_NotAcceptedQuest_ReturnsFalse()
        {
            // Arrange
            var manager = new QuestManager();
            var quest = CreateTestBossQuest();
            quest.Discover();
            manager.RegisterQuest(quest);

            // Act
            bool result = manager.SetActiveQuest(quest.QuestId);

            // Assert
            Assert.False(result);
            Assert.Null(manager.ActiveQuestId);
        }

        [Fact]
        public void SetActiveQuest_SwitchingBetweenQuests_UpdatesActiveQuest()
        {
            // Arrange
            var manager = new QuestManager();
            var quest1 = CreateTestBossQuest("quest1");
            var quest2 = CreateTestBossQuest("quest2");
            quest1.Discover();
            quest1.Accept();
            quest2.Discover();
            quest2.Accept();
            manager.RegisterQuest(quest1);
            manager.RegisterQuest(quest2);

            // Act
            manager.SetActiveQuest("quest1");
            manager.SetActiveQuest("quest2");

            // Assert
            Assert.Equal("quest2", manager.ActiveQuestId);
            Assert.Equal(quest2, manager.GetActiveQuest());
        }

        [Fact]
        public void ClearActiveQuest_SetsActiveQuestToNull()
        {
            // Arrange
            var manager = new QuestManager();
            var quest = CreateTestBossQuest();
            quest.Discover();
            quest.Accept();
            manager.RegisterQuest(quest);
            manager.SetActiveQuest(quest.QuestId);

            // Act
            manager.ClearActiveQuest();

            // Assert
            Assert.Null(manager.ActiveQuestId);
            Assert.Null(manager.GetActiveQuest());
        }

        #endregion

        #region Quest Completion Tests

        [Fact]
        public void CompleteQuest_QuestWithCompletedObjectives_ChangesStateToCompleted()
        {
            // Arrange
            var manager = new QuestManager();
            var quest = CreateTestLevelQuest(5);
            quest.Discover();
            quest.Accept();
            manager.RegisterQuest(quest);

            // Complete objective
            quest.Objectives[0].SetProgress(5);

            // Act
            bool result = manager.CompleteQuest(quest.QuestId);

            // Assert
            Assert.True(result);
            Assert.Equal(QuestState.Completed, quest.State);
        }

        [Fact]
        public void CompleteQuest_ObjectivesNotComplete_ReturnsFalse()
        {
            // Arrange
            var manager = new QuestManager();
            var quest = CreateTestLevelQuest(5);
            quest.Discover();
            quest.Accept();
            manager.RegisterQuest(quest);

            // Act
            bool result = manager.CompleteQuest(quest.QuestId);

            // Assert
            Assert.False(result);
            Assert.NotEqual(QuestState.Completed, quest.State);
        }

        #endregion

        #region Quest Objective Tests

        [Fact]
        public void QuestObjective_UpdateProgress_TracksProgress()
        {
            // Arrange
            var objective = new QuestObjective("Test Objective", 10);

            // Act
            objective.UpdateProgress(3);
            objective.UpdateProgress(4);

            // Assert
            Assert.Equal(7, objective.CurrentProgress);
            Assert.Equal(10, objective.RequiredProgress);
            Assert.False(objective.IsCompleted);
        }

        [Fact]
        public void QuestObjective_CompleteProgress_MarksAsCompleted()
        {
            // Arrange
            var objective = new QuestObjective("Test Objective", 10);

            // Act
            objective.SetProgress(10);

            // Assert
            Assert.True(objective.IsCompleted);
        }

        [Fact]
        public void QuestObjective_OverProgress_StillCompleted()
        {
            // Arrange
            var objective = new QuestObjective("Test Objective", 10);

            // Act
            objective.SetProgress(15);

            // Assert
            Assert.Equal(15, objective.CurrentProgress);
            Assert.True(objective.IsCompleted);
        }

        [Fact]
        public void QuestObjective_GetProgressPercentage_CalculatesCorrectly()
        {
            // Arrange
            var objective = new QuestObjective("Test Objective", 100);
            objective.SetProgress(75);

            // Act
            double percentage = objective.GetProgressPercentage();

            // Assert
            Assert.Equal(75.0, percentage);
        }

        [Fact]
        public void QuestObjective_Reset_ClearsProgress()
        {
            // Arrange
            var objective = new QuestObjective("Test Objective", 10);
            objective.SetProgress(7);

            // Act
            objective.Reset();

            // Assert
            Assert.Equal(0, objective.CurrentProgress);
        }

        #endregion

        #region Quest Reward Tests

        [Fact]
        public void QuestReward_HasRewards_ReturnsTrueWhenHasRewards()
        {
            // Arrange
            var reward = new QuestReward { Gold = 100, Experience = 50 };

            // Act
            bool hasRewards = reward.HasRewards();

            // Assert
            Assert.True(hasRewards);
        }

        [Fact]
        public void QuestReward_HasRewards_ReturnsFalseWhenNoRewards()
        {
            // Arrange
            var reward = new QuestReward { Gold = 0, Experience = 0 };

            // Act
            bool hasRewards = reward.HasRewards();

            // Assert
            Assert.False(hasRewards);
        }

        [Fact]
        public void QuestReward_GetRewardSummary_FormatsCorrectly()
        {
            // Arrange
            var reward = new QuestReward { Gold = 100, Experience = 250 };

            // Act
            string summary = reward.GetRewardSummary();

            // Assert
            Assert.Contains("100", summary);
            Assert.Contains("250", summary);
        }

        #endregion

        #region Specific Quest Type Tests

        [Fact]
        public void BossDefeatQuest_OnBossDefeated_CompletesObjective()
        {
            // Arrange
            var quest = CreateTestBossQuest("dragon");
            quest.Discover();

            // Act
            quest.OnBossDefeated("dragon");

            // Assert
            Assert.True(quest.Objectives[0].IsCompleted);
            Assert.True(quest.CanComplete());
        }

        [Fact]
        public void BossDefeatQuest_WrongBoss_DoesNotComplete()
        {
            // Arrange
            var quest = CreateTestBossQuest("dragon");
            quest.Discover();

            // Act
            quest.OnBossDefeated("goblin");

            // Assert
            Assert.False(quest.Objectives[0].IsCompleted);
        }

        [Fact]
        public void LevelQuest_CheckLevelStatus_CompletesWhenLevelReached()
        {
            // Arrange
            var quest = CreateTestLevelQuest(5);
            quest.Discover();

            // Act
            quest.CheckLevelStatus(5);

            // Assert
            Assert.True(quest.Objectives[0].IsCompleted);
            Assert.True(quest.CanComplete());
        }

        [Fact]
        public void LevelQuest_BelowTargetLevel_DoesNotComplete()
        {
            // Arrange
            var quest = CreateTestLevelQuest(5);
            quest.Discover();

            // Act
            quest.CheckLevelStatus(3);

            // Assert
            Assert.False(quest.Objectives[0].IsCompleted);
        }

        [Fact]
        public void EnemyKillQuest_OnEnemyKilled_IncrementsProgress()
        {
            // Arrange
            var quest = CreateTestKillQuest(10);
            quest.Discover();

            // Act
            quest.OnEnemyKilled();
            quest.OnEnemyKilled();
            quest.OnEnemyKilled();

            // Assert
            Assert.Equal(3, quest.Objectives[0].CurrentProgress);
            Assert.False(quest.Objectives[0].IsCompleted);
        }

        [Fact]
        public void EnemyKillQuest_ReachTarget_CompletesObjective()
        {
            // Arrange
            var quest = CreateTestKillQuest(5);
            quest.Discover();

            // Act
            for (int i = 0; i < 5; i++)
            {
                quest.OnEnemyKilled();
            }

            // Assert
            Assert.True(quest.Objectives[0].IsCompleted);
            Assert.True(quest.CanComplete());
        }

        #endregion

        #region Quest State Transition Tests

        [Fact]
        public void Quest_StateTransitions_FollowCorrectFlow()
        {
            // Arrange
            var quest = CreateTestBossQuest();

            // Act & Assert - Initial state
            Assert.Equal(QuestState.NotDiscovered, quest.State);

            // Discover
            quest.Discover();
            Assert.Equal(QuestState.Available, quest.State);

            // Accept
            quest.Accept();
            Assert.Equal(QuestState.Accepted, quest.State);

            // Complete (simulate boss defeat)
            quest.OnBossDefeated(quest.BossId);
            quest.Complete();
            Assert.Equal(QuestState.Completed, quest.State);

            // Claim rewards
            quest.ClaimRewards();
            Assert.Equal(QuestState.Claimed, quest.State);
        }

        [Fact]
        public void Quest_CannotAcceptBeforeDiscovery()
        {
            // Arrange
            var quest = CreateTestBossQuest();

            // Act
            quest.Accept();

            // Assert
            Assert.Equal(QuestState.NotDiscovered, quest.State);
        }

        [Fact]
        public void Quest_CannotCompleteBeforeAcceptance_WhenRequired()
        {
            // Arrange - FinalBossQuest requires acceptance
            var quest = new FinalBossQuest("final_boss", "Final Boss");
            quest.Discover();

            // Complete all objectives
            foreach (var objective in quest.Objectives)
            {
                objective.SetProgress(objective.RequiredProgress);
            }

            // Act
            bool canComplete = quest.CanComplete();

            // Assert
            Assert.False(canComplete); // Requires acceptance
        }

        #endregion

        #region Retroactive Completion Tests

        [Fact]
        public void Quest_RetroactiveCompletion_TracksProgressBeforeAcceptance()
        {
            // Arrange
            var quest = CreateTestKillQuest(10);
            quest.Discover();

            // Act - Progress before acceptance
            quest.OnEnemyKilled();
            quest.OnEnemyKilled();
            quest.OnEnemyKilled();

            // Assert
            Assert.Equal(3, quest.Objectives[0].CurrentProgress);
            Assert.Equal(QuestState.Available, quest.State);
        }

        [Fact]
        public void Quest_RetroactiveCompletion_CanCompleteAfterAcceptance()
        {
            // Arrange
            var quest = CreateTestLevelQuest(5);
            quest.Discover();

            // Progress before acceptance
            quest.CheckLevelStatus(5);

            // Act - Accept after completion
            quest.Accept();

            // Assert
            Assert.True(quest.CanComplete());
        }

        #endregion
    }
}