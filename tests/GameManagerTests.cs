using Xunit;
using GameLogic.Core;
using GameLogic.Entities.Player;
using GameLogic.Quests;
using GameLogic.Systems;

namespace GameLogic.Tests
{
    /// <summary>
    /// Unit tests for GameManager's quest-reward claiming and QuestManager's boss-defeat
    /// progress wiring (to-do "Give quest rewards to the player" / "Wire quest progress into
    /// play", section 1). Uses a real StartNewGame() rather than hand-built fixtures, since
    /// InitializeQuests() is what actually registers and discovers the boss quests this exercises.
    /// </summary>
    public class GameManagerTests
    {
        private GameManager CreateStartedGame()
        {
            var gameManager = new GameManager();
            var weapon = gameManager.GetStarterWeaponChoices()[0];
            var ability = Player.GetAvailableAbilities()[0];
            gameManager.StartNewGame("TestHero", DifficultyLevel.Normal, ability, weapon);
            return gameManager;
        }

        #region NotifyBossDefeated

        [Fact]
        public void NotifyBossDefeated_CompletesThatBossesQuest()
        {
            var gameManager = CreateStartedGame();
            string firstBossId = gameManager.Bosses.GetNextBoss().BossId; // Skarn - already discovered at game start
            string questId = $"boss_defeat_{firstBossId}";

            gameManager.Quests.NotifyBossDefeated(firstBossId);

            var quest = gameManager.Quests.GetQuest(questId);
            Assert.Equal(QuestState.Completed, quest.State);
        }

        [Fact]
        public void NotifyBossDefeated_WrongBossId_DoesNotCompleteQuest()
        {
            var gameManager = CreateStartedGame();
            string firstBossId = gameManager.Bosses.GetNextBoss().BossId;
            string questId = $"boss_defeat_{firstBossId}";

            gameManager.Quests.NotifyBossDefeated("not_a_real_boss_id");

            var quest = gameManager.Quests.GetQuest(questId);
            Assert.NotEqual(QuestState.Completed, quest.State);
        }

        #endregion

        #region ClaimQuestReward

        [Fact]
        public void ClaimQuestReward_CompletedQuest_GrantsGoldAndXPAndReturnsTrue()
        {
            var gameManager = CreateStartedGame();
            string firstBossId = gameManager.Bosses.GetNextBoss().BossId;
            string questId = $"boss_defeat_{firstBossId}";
            gameManager.Quests.NotifyBossDefeated(firstBossId);

            int goldBefore = gameManager.Player.Gold;
            int levelBefore = gameManager.Player.Level;
            int xpBefore = gameManager.Player.Experience;
            var reward = gameManager.Quests.GetQuest(questId).Reward;

            bool claimed = gameManager.ClaimQuestReward(questId);

            Assert.True(claimed);
            Assert.Equal(goldBefore + reward.Gold, gameManager.Player.Gold);
            // AddExperience() can consume the XP into a level-up (it resets Experience to the
            // remainder, not a running total), so just confirm it moved the needle one way or
            // the other rather than re-deriving the level curve here.
            Assert.True(gameManager.Player.Level > levelBefore || gameManager.Player.Experience != xpBefore);
        }

        [Fact]
        public void ClaimQuestReward_NotYetCompleted_ReturnsFalseAndGivesNothing()
        {
            var gameManager = CreateStartedGame();
            string firstBossId = gameManager.Bosses.GetNextBoss().BossId;
            string questId = $"boss_defeat_{firstBossId}"; // Discovered, but boss not defeated yet

            int goldBefore = gameManager.Player.Gold;

            bool claimed = gameManager.ClaimQuestReward(questId);

            Assert.False(claimed);
            Assert.Equal(goldBefore, gameManager.Player.Gold);
        }

        [Fact]
        public void ClaimQuestReward_UnknownQuestId_ReturnsFalse()
        {
            var gameManager = CreateStartedGame();

            bool claimed = gameManager.ClaimQuestReward("not_a_real_quest_id");

            Assert.False(claimed);
        }

        [Fact]
        public void ClaimQuestReward_CalledTwice_SecondCallReturnsFalse()
        {
            var gameManager = CreateStartedGame();
            string firstBossId = gameManager.Bosses.GetNextBoss().BossId;
            string questId = $"boss_defeat_{firstBossId}";
            gameManager.Quests.NotifyBossDefeated(firstBossId);

            Assert.True(gameManager.ClaimQuestReward(questId));
            Assert.False(gameManager.ClaimQuestReward(questId));
        }

        #endregion
    }
}
