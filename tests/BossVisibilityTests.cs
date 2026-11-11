using System.Linq;
using Xunit;
using GameLogic.Quests;
using GameLogic.Entities.Enemies.Bosses;

namespace GameLogic.Tests
{
    /// <summary>
    /// Only the next boss's information is ever shown (decided 2026-11-11): the boss list and the
    /// boss quests show the next boss the player hasn't beaten; every boss after it is locked,
    /// with nothing about it shown.
    /// </summary>
    public class BossVisibilityTests
    {
        private static BossManager MakeBossManager()
        {
            var bossManager = new BossManager();
            bossManager.RegisterBosses(BossDefinitions.GetAllBosses().ToArray());
            return bossManager;
        }

        /// <summary>The boss quests the way a new game makes them (GameStartup.InitializeQuests).</summary>
        private static QuestManager MakeQuests(BossManager bossManager)
        {
            var quests = new QuestManager();
            foreach (var boss in bossManager.AllBosses.Values)
            {
                if (!bossManager.IsFinalBoss(boss.BossId))
                {
                    quests.RegisterQuest(new BossDefeatQuest(boss.BossId, boss.Name, 100, 50));
                }
            }
            var finalBoss = bossManager.GetFinalBoss();
            quests.RegisterQuest(new FinalBossQuest(finalBoss.BossId, finalBoss.Name));
            return quests;
        }

        private static Quest QuestFor(QuestManager quests, string bossId)
        {
            return quests.AllQuests.Values.First(q =>
                (q is BossDefeatQuest b && b.BossId == bossId) || (q is FinalBossQuest f && f.FinalBossId == bossId));
        }

        #region Boss list

        [Fact]
        public void BossList_AtTheStart_OnlySkarnShowsAnything()
        {
            var list = MakeBossManager().GetBossList();

            var first = list[0];
            Assert.True(first.IsNext);
            Assert.False(first.IsLocked);
            Assert.Equal("Skarn", first.Name);
            Assert.NotNull(first.Title);
            Assert.NotNull(first.Description);

            foreach (var entry in list.Skip(1))
            {
                Assert.True(entry.IsLocked);
                Assert.False(entry.IsNext);
                Assert.Null(entry.BossId);
                Assert.Null(entry.Name);
                Assert.Null(entry.Title);
                Assert.Null(entry.Description);
                Assert.Null(entry.UniqueAbilityDescription);
            }
        }

        [Fact]
        public void BossList_AfterSkarn_TheMonkIsNext_SkarnShowsAsBeaten_TheRestStayLocked()
        {
            var bossManager = MakeBossManager();
            bossManager.DefeatBoss(BossStatSheet.Skarn);

            var list = bossManager.GetBossList();

            Assert.True(list[0].IsDefeated);
            Assert.Equal("Skarn", list[0].Name);
            Assert.Null(list[0].Description); // Beaten: name and title only

            Assert.True(list[1].IsNext);
            Assert.Equal(BossStatSheet.LibrarianMonk, list[1].BossId);
            Assert.NotNull(list[1].Description);

            Assert.All(list.Skip(2), entry => Assert.True(entry.IsLocked && entry.Name == null));
        }

        [Fact]
        public void BossList_PositionsFollowTheStoryOrder()
        {
            var list = MakeBossManager().GetBossList();

            Assert.Equal(Enumerable.Range(1, list.Count), list.Select(e => e.Position));
        }

        #endregion

        #region Boss quests

        [Fact]
        public void Quests_AtANewGame_OnlySkarnsIsShown()
        {
            var bossManager = MakeBossManager();
            var quests = MakeQuests(bossManager);

            quests.UpdateBossQuestVisibility(bossManager);

            Assert.Equal(QuestState.Available, QuestFor(quests, BossStatSheet.Skarn).State);
            foreach (var quest in quests.AllQuests.Values.Where(q => q != QuestFor(quests, BossStatSheet.Skarn)))
            {
                Assert.Equal(QuestState.NotDiscovered, quest.State);
            }
        }

        [Fact]
        public void Quests_BeatingABoss_ShowsTheNextOnesQuest()
        {
            var bossManager = MakeBossManager();
            var quests = MakeQuests(bossManager);
            quests.UpdateBossQuestVisibility(bossManager);

            bossManager.DefeatBoss(BossStatSheet.Skarn);
            quests.UpdateBossQuestVisibility(bossManager);

            Assert.Equal(QuestState.Available, QuestFor(quests, BossStatSheet.Skarn).State); // Left as it was
            Assert.Equal(QuestState.Available, QuestFor(quests, BossStatSheet.LibrarianMonk).State);
            Assert.Equal(QuestState.NotDiscovered, QuestFor(quests, BossStatSheet.Centipede).State);
        }

        [Fact]
        public void Quests_TheFinalQuestStaysHidden_UntilItsBossIsNext()
        {
            var bossManager = MakeBossManager();
            var quests = MakeQuests(bossManager);
            var finalBoss = bossManager.GetFinalBoss();
            var finalQuest = QuestFor(quests, finalBoss.BossId);

            foreach (var boss in BossDefinitions.GetAllBosses().Where(b => b.BossId != finalBoss.BossId))
            {
                quests.UpdateBossQuestVisibility(bossManager);
                Assert.Equal(QuestState.NotDiscovered, finalQuest.State);
                bossManager.DefeatBoss(boss.BossId);
            }

            quests.UpdateBossQuestVisibility(bossManager);
            Assert.Equal(QuestState.Available, finalQuest.State);
        }

        [Fact]
        public void Quests_FromAnOldSave_LaterBossesAreHiddenAgain_ButAnAcceptedOneIsKept()
        {
            var bossManager = MakeBossManager();
            var quests = MakeQuests(bossManager);
            foreach (var quest in quests.AllQuests.Values) quest.Discover(); // The old rule: all shown at the start
            QuestFor(quests, BossStatSheet.Centipede).Accept();

            quests.UpdateBossQuestVisibility(bossManager);

            Assert.Equal(QuestState.Available, QuestFor(quests, BossStatSheet.Skarn).State);
            Assert.Equal(QuestState.NotDiscovered, QuestFor(quests, BossStatSheet.LibrarianMonk).State);
            Assert.Equal(QuestState.Accepted, QuestFor(quests, BossStatSheet.Centipede).State);
        }

        #endregion
    }
}
