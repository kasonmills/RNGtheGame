using System.Collections.Generic;
using System.Linq;
using Xunit;
using GameLogic.Combat;
using GameLogic.Data;
using GameLogic.Items;
using GameLogic.Systems;
using GameLogic.Entities.Player;
using GameLogic.Entities.Enemies;
using GameLogic.Entities.Enemies.EnemyTypes;
using GameLogic.Entities.NPCs.Companions;
using Entity = GameLogic.Entities.Entity;

namespace GameLogic.Tests
{
    /// <summary>
    /// The party formation and rows (built 2026-11-25 from the rules decided 2026-11-23):
    /// slots, placing newcomers, saving, the fight's own copy, Reposition, the 2:1 targeting,
    /// the back-row hit penalty and the melee damage cut.
    /// </summary>
    public class FormationTests
    {
        private static Player SwordPlayer()
        {
            var player = new Player("TestHero") { Level = 10, Health = 50000, MaxHealth = 50000 };
            player.EquippedWeapon = ItemDatabase.GetWeapon("rusty sword");
            return player;
        }

        private static Goblin ToughGoblin()
        {
            var goblin = new Goblin(1);
            goblin.MaxHealth = 100000;
            goblin.Health = 100000;
            return goblin;
        }

        /// <summary>A fight with the player (slot 0) and a mage (staff - slot 2), on the player's turn.</summary>
        private static (CombatManager combat, Player player, CompanionMage mage) FightWithMage()
        {
            var combat = new CombatManager(new RNGManager(seed: 42));
            var player = SwordPlayer();
            var mage = new CompanionMage(1) { MaxHealth = 50000, Health = 50000 };
            mage.JoinParty();

            combat.StartCombat(player, new List<EnemyBase> { ToughGoblin() }, new List<Entity> { mage });
            return (combat, player, mage);
        }

        #region The rules

        [Fact]
        public void Slots_0And1_AreFront_2And3_AreBack()
        {
            Assert.Equal(FormationRow.Front, Formation.RowOf(0));
            Assert.Equal(FormationRow.Front, Formation.RowOf(1));
            Assert.Equal(FormationRow.Back, Formation.RowOf(2));
            Assert.Equal(FormationRow.Back, Formation.RowOf(3));
            Assert.False(Formation.IsValidSlot(4));
            Assert.False(Formation.IsValidSlot(Formation.NoSlot));
        }

        [Theory]
        [InlineData(WeaponType.Bow, true)]
        [InlineData(WeaponType.Crossbow, true)]
        [InlineData(WeaponType.Staff, true)]
        [InlineData(WeaponType.Wand, true)]
        [InlineData(WeaponType.Sword, false)]
        [InlineData(WeaponType.Spear, false)]
        [InlineData(WeaponType.Dagger, false)]
        public void RangedWeapons_AreBowCrossbowStaffAndWand(WeaponType type, bool ranged)
        {
            Assert.Equal(ranged, Formation.IsRangedWeapon(type));
        }

        [Fact]
        public void FirstFreeSlot_RangedTryTheBackFirst()
        {
            Assert.Equal(0, Formation.FirstFreeSlot(new int[0], ranged: false));
            Assert.Equal(2, Formation.FirstFreeSlot(new int[0], ranged: true));
            Assert.Equal(1, Formation.FirstFreeSlot(new[] { 0 }, ranged: false));
            Assert.Equal(0, Formation.FirstFreeSlot(new[] { 2, 3 }, ranged: true));
            Assert.Equal(Formation.NoSlot, Formation.FirstFreeSlot(new[] { 0, 1, 2, 3 }, ranged: false));
        }

        [Fact]
        public void BackRowDamage_Melee70_Spear90_Ranged100()
        {
            var player = new Player("TestHero");
            Assert.Equal(0.7, Formation.BackRowDamageMultiplier(player)); // Unarmed is melee

            player.EquippedWeapon = ItemDatabase.GetWeapon("rusty sword");
            Assert.Equal(0.7, Formation.BackRowDamageMultiplier(player));

            player.EquippedWeapon = ItemDatabase.GetWeapon("hunting spear");
            Assert.Equal(0.9, Formation.BackRowDamageMultiplier(player));

            player.EquippedWeapon = ItemDatabase.GetWeapon("hunting bow");
            Assert.Equal(1.0, Formation.BackRowDamageMultiplier(player));

            Assert.Equal(1.0, Formation.BackRowDamageMultiplier(new CompanionMage(1)));  // Staff
            Assert.Equal(0.7, Formation.BackRowDamageMultiplier(new CompanionChance())); // Cat form is melee
        }

        [Fact]
        public void Arrange_KeepsOwnSlots_AndPlacesTheRest()
        {
            var player = SwordPlayer();
            var chance = new CompanionChance();
            var mage = new CompanionMage(1);
            player.FormationSlot = 1;
            chance.FormationSlot = 1;   // Taken by the player - re-placed
            mage.FormationSlot = Formation.NoSlot;

            var slots = Formation.Arrange(new List<Entity> { player, chance, mage }, m => m.FormationSlot);

            Assert.Equal(1, slots[player]);
            Assert.Equal(0, slots[chance]); // Melee: first free in order
            Assert.Equal(2, slots[mage]);   // Ranged: the back first
            Assert.Equal(1, player.FormationSlot); // The members themselves aren't changed
            Assert.Equal(1, chance.FormationSlot);
        }

        #endregion

        #region The party menu and saving

        [Fact]
        public void Recruits_FillTheFirstEmptySlot_RangedAtTheBack()
        {
            var party = new PartyManager();
            var player = SwordPlayer();
            var chance = new CompanionChance();
            var mage = new CompanionMage(1);

            party.RecruitCompanion(player, chance);
            party.RecruitCompanion(player, mage);

            Assert.Equal(0, player.FormationSlot);
            Assert.Equal(1, chance.FormationSlot);
            Assert.Equal(2, mage.FormationSlot);
            Assert.Same(mage, party.GetMemberInSlot(player, 2));
        }

        [Fact]
        public void SetFormationSlot_SwapsWithWhoeverIsThere()
        {
            var party = new PartyManager();
            var player = SwordPlayer();
            var mage = new CompanionMage(1);
            party.RecruitCompanion(player, mage);

            Assert.True(party.SetFormationSlot(player, mage, 0));
            Assert.Equal(0, mage.FormationSlot);
            Assert.Equal(2, player.FormationSlot);

            Assert.True(party.SetFormationSlot(player, player, 3)); // An empty slot
            Assert.Equal(3, player.FormationSlot);

            Assert.False(party.SetFormationSlot(player, mage, 4));                 // No such slot
            Assert.False(party.SetFormationSlot(player, new CompanionChance(), 1)); // Not in the party
        }

        [Fact]
        public void Bench_ClearsTheSlot_AndASwapTakesTheOldOne()
        {
            var party = new PartyManager();
            var player = SwordPlayer();
            var chance = new CompanionChance();
            var mage = new CompanionMage(1);
            party.RecruitCompanion(player, chance); // Slot 1
            party.RecruitCompanion(player, mage);   // Slot 2

            Assert.True(party.MoveToBench(player, mage));
            Assert.Equal(Formation.NoSlot, mage.FormationSlot);

            Assert.True(party.SwapCompanions(player, chance, mage));
            Assert.Equal(1, mage.FormationSlot); // Took Chance's slot, not the back row
            Assert.Equal(Formation.NoSlot, chance.FormationSlot);
        }

        [Fact]
        public void Formation_IsSaved_AndOldSavesArePlaced()
        {
            var party = new PartyManager();
            var player = SwordPlayer();
            var mage = new CompanionMage(1);
            party.RecruitCompanion(player, mage);
            party.SetFormationSlot(player, player, 1);

            var save = SaveManager.CreateSaveData(player, party: party);
            Assert.Equal(1, save.PlayerFormationSlot);
            Assert.Equal(2, save.Companions.Single().FormationSlot);

            var loaded = SaveManager.LoadCompanions(save);
            Assert.Equal(2, loaded.Single().FormationSlot);

            // An old save has no slots: loaded as none, then placed
            save.Companions.Single().FormationSlot = null;
            var old = SaveManager.LoadCompanions(save).Single();
            Assert.Equal(Formation.NoSlot, old.FormationSlot);

            var restored = new PartyManager();
            restored.RestoreRoster(new[] { old });
            var freshPlayer = SwordPlayer();
            restored.FixFormation(freshPlayer);
            Assert.Equal(0, freshPlayer.FormationSlot);
            Assert.Equal(2, old.FormationSlot);
        }

        #endregion

        #region In a fight

        [Fact]
        public void Fight_StartsWithTheFormation()
        {
            var (combat, player, mage) = FightWithMage();

            Assert.Equal(0, combat.GetSlot(player));
            Assert.Equal(2, combat.GetSlot(mage));
            Assert.Equal(FormationRow.Back, combat.GetRow(mage));
            Assert.True(combat.IsBehindTheFrontRow(mage));
            Assert.False(combat.IsBehindTheFrontRow(player));
            Assert.Equal(new List<int> { 1, 3 }, combat.GetFreeSlots());
            Assert.Equal(FormationRow.Front, combat.GetRow(combat.Enemies[0])); // Goblins fight at the front
        }

        [Fact]
        public void BackRow_WithNobodyInFront_CountsAsTheFront()
        {
            var combat = new CombatManager(new RNGManager(seed: 42));
            var player = SwordPlayer();
            player.FormationSlot = 3;
            var mage = new CompanionMage(1) { MaxHealth = 50000, Health = 50000 };
            mage.JoinParty();

            combat.StartCombat(player, new List<EnemyBase> { ToughGoblin() }, new List<Entity> { mage });

            Assert.Equal(FormationRow.Back, combat.GetRow(player));
            Assert.False(combat.IsBehindTheFrontRow(player));
            Assert.False(combat.IsBehindTheFrontRow(mage));
        }

        [Fact]
        public void Reposition_SwapsPlaces_ForThisFightOnly()
        {
            var (combat, player, mage) = FightWithMage();
            Assert.True(combat.IsWaitingForPlayerAction);
            Assert.Same(player, combat.CurrentActor);

            var events = new List<CombatEvent>();
            combat.CombatEventOccurred += e => events.Add(e);

            Assert.False(combat.ValidateAction(CombatAction.Reposition(player, 0), out _)); // Already there
            Assert.False(combat.ValidateAction(CombatAction.Reposition(player, 7), out _)); // No such slot
            Assert.True(combat.SubmitPlayerAction(CombatAction.Reposition(player, 2)));

            Assert.Equal(2, combat.GetSlot(player));
            Assert.Equal(0, combat.GetSlot(mage));
            var moved = events.Single(e => e.Type == CombatEventType.Repositioned);
            Assert.Same(player, moved.Actor);
            Assert.Same(mage, moved.Target);
            Assert.Equal(2, moved.Amount);

            Assert.Equal(Formation.NoSlot, player.FormationSlot); // The saved formation isn't touched
        }

        [Fact]
        public void Reposition_IntoAnEmptySlot_AndPartyOnly()
        {
            var (combat, player, mage) = FightWithMage();

            Assert.Contains(mage, combat.GetValidTargets(player, ActionType.Reposition));
            Assert.True(combat.SubmitPlayerAction(CombatAction.Reposition(player, 1)));
            Assert.Equal(1, combat.GetSlot(player));
            Assert.Equal(2, combat.GetSlot(mage));
            Assert.Equal(Formation.NoSlot, combat.GetSlot(combat.Enemies[0]));
        }

        [Fact]
        public void EnemiesRandomPicks_FavourTheFrontRow_TwoToOne()
        {
            var (combat, player, mage) = FightWithMage();
            var goblin = combat.Enemies[0];
            var party = new List<Entity> { player, mage };

            int front = 0;
            const int picks = 3000;
            for (int i = 0; i < picks; i++)
            {
                if (combat.PickRandomTarget(goblin, party) == player) front++;
            }

            double share = (double)front / picks;
            Assert.InRange(share, 0.60, 0.73); // About 2/3
        }

        [Fact]
        public void HitChanceModifier_LowersTheHitChance_ToAtLeast1()
        {
            var calculator = new DamageCalculator(new RNGManager(seed: 7));
            var goblin = new Goblin(1) { Accuracy = 100 };
            var target = SwordPlayer();

            int plainHits = Enumerable.Range(0, 200).Count(_ => !calculator.CalculateAttack(goblin, target).Missed);
            int penaltyHits = Enumerable.Range(0, 200).Count(_ => !calculator.CalculateAttack(goblin, target, hitChanceModifier: -99).Missed);
            int floorHits = Enumerable.Range(0, 400).Count(_ => !calculator.CalculateAttack(goblin, target, hitChanceModifier: -500).Missed);

            Assert.Equal(200, plainHits);
            Assert.InRange(penaltyHits, 0, 15); // 1% hit chance
            Assert.InRange(floorHits, 0, 20);   // Never below 1%
        }

        #endregion
    }
}
