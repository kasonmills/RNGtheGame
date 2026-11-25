using System;
using System.Collections.Generic;
using System.Linq;
using GameLogic.Entities;
using GameLogic.Entities.Enemies;
using GameLogic.Entities.Enemies.EnemyTypes;
using GameLogic.Entities.NPCs.Companions;
using GameLogic.Entities.Player;
using GameLogic.Items;
using GameLogic.Systems;

namespace GameLogic.Combat
{
    public enum ArmyCampState
    {
        NotStarted,
        InWave,         // A wave is being fought
        BetweenWaves,   // A wave was beaten - the player picks NextWave or Leave
        Finished        // The attempt is over and the pot was paid (LastResult)
    }

    /// <summary>
    /// What an army camp attempt paid out.
    /// </summary>
    public class ArmyCampResult
    {
        public int WavesBeaten { get; set; }
        /// <summary>True when the player left between waves (whole pot); false after losing or surrendering (half).</summary>
        public bool KeptWholePot { get; set; }
        public int ExperienceGained { get; set; }
        public int GoldGained { get; set; }
        public List<Item> ItemsGained { get; } = new List<Item>();
        /// <summary>Prize items that didn't fit in the bag.</summary>
        public List<Item> ItemsLeftBehind { get; } = new List<Item>();
        public bool NewBestWave { get; set; }
    }

    /// <summary>
    /// The army camp's endless "test your strength" challenge (rules decided 2026-11-23 - see
    /// to_do_list.md section 3): back-to-back waves of Army Soldiers with no healing between
    /// them. The loop is built before the camp itself (which needs the fixed map).
    /// - Waves: wave 1 is 1 Army Soldier at the lineup's level; each wave adds one (max 8),
    ///   and +1 level every 3 waves.
    /// - The pot: each wave beaten adds the XP and gold of a normal fight that size, plus a
    ///   prize item every 5th wave (its rarity rises with the wave).
    /// - Leaving between waves keeps the whole pot. Surrendering mid-wave, or the whole lineup
    ///   falling, keeps half. Never a Game Over.
    /// - The lineup (the player + up to 3 companions) is fixed for the attempt; no Flee, no
    ///   items; the fallen stay down; spells are free (FightRules.ArmyCampWave).
    /// - When it ends, everyone in the lineup is fully healed and revived.
    /// No console output - listen to WaveFinished / AttemptFinished.
    /// </summary>
    public class ArmyCampChallenge
    {
        public const int MaxSoldiers = 8;
        public const int WavesPerLevelUp = 3;
        public const int PrizeEveryWaves = 5;
        public const int MaxLineupCompanions = 3;

        private readonly CombatManager _combat;
        private readonly RNGManager _rng;
        private Player _player;
        private List<CompanionBase> _companions = new List<CompanionBase>();
        private readonly List<Item> _potItems = new List<Item>();

        public ArmyCampState State { get; private set; } = ArmyCampState.NotStarted;

        /// <summary>The wave being fought, or the next one when between waves (starts at 1).</summary>
        public int CurrentWave { get; private set; }

        public int WavesBeaten { get; private set; }

        /// <summary>The lineup's level when the attempt began - wave 1's soldiers are this level.</summary>
        public int StartLevel { get; private set; }

        public int PotExperience { get; private set; }
        public int PotGold { get; private set; }
        public IReadOnlyList<Item> PotItems => _potItems;

        /// <summary>The companions fighting alongside the player this attempt.</summary>
        public IReadOnlyList<CompanionBase> LineupCompanions => _companions;

        /// <summary>The last finished attempt's payout (null until one has finished).</summary>
        public ArmyCampResult LastResult { get; private set; }

        /// <summary>A wave ended (its CombatResult) - won, surrendered or lost.</summary>
        public event Action<CombatResult> WaveFinished;

        /// <summary>The attempt is over and the pot was paid.</summary>
        public event Action<ArmyCampResult> AttemptFinished;

        /// <summary>
        /// Checks a new best wave. Set by GameManager to the save's statistics; null in tests
        /// that don't need it.
        /// </summary>
        public StatisticsTracker Statistics { get; set; }

        public ArmyCampChallenge(CombatManager combat, RNGManager rng)
        {
            _combat = combat ?? throw new ArgumentNullException(nameof(combat));
            _rng = rng ?? new RNGManager();
        }

        // ----- The numbers -----

        /// <summary>Soldiers in a wave: 1 more each wave, at most 8.</summary>
        public static int SoldiersInWave(int wave) => Math.Clamp(wave, 1, MaxSoldiers);

        /// <summary>The soldiers' level in a wave: the start level, +1 every 3 waves.</summary>
        public static int SoldierLevel(int startLevel, int wave) => Math.Max(1, startLevel) + (Math.Max(1, wave) - 1) / WavesPerLevelUp;

        /// <summary>
        /// The prize for beating a wave: every 5th wave, rising with the wave (my defaults):
        /// wave 5 Uncommon, 10 Rare, 15 and later Legendary. Null for a wave with no prize.
        /// </summary>
        public static ItemRarity? PrizeRarity(int wave)
        {
            if (wave <= 0 || wave % PrizeEveryWaves != 0) return null;
            if (wave >= 15) return ItemRarity.Legendary;
            if (wave >= 10) return ItemRarity.Rare;
            return ItemRarity.Uncommon;
        }

        // ----- Running an attempt -----

        /// <summary>
        /// Start an attempt with the player and up to 3 companions (the fixed lineup), and
        /// begin wave 1. Refused (false, with the reason) if an attempt is already running,
        /// a fight is on, or the lineup isn't valid.
        /// </summary>
        public bool Start(Player player, List<CompanionBase> companions, out string reason)
        {
            reason = null;
            companions ??= new List<CompanionBase>();

            if (State == ArmyCampState.InWave || State == ArmyCampState.BetweenWaves)
            {
                reason = "An attempt is already under way.";
                return false;
            }
            if (player == null)
            {
                reason = "No player.";
                return false;
            }
            if (_combat.IsCombatActive)
            {
                reason = "A fight is already on.";
                return false;
            }
            if (companions.Count > MaxLineupCompanions || companions.Distinct().Count() != companions.Count || companions.Any(c => c == null))
            {
                reason = $"Pick up to {MaxLineupCompanions} different companions to fight alongside you.";
                return false;
            }

            _player = player;
            _companions = new List<CompanionBase>(companions);
            _potItems.Clear();
            PotExperience = 0;
            PotGold = 0;
            WavesBeaten = 0;
            CurrentWave = 1;
            LastResult = null;

            // The lineup's average level (rounded) - like a boss fight's party level
            var levels = new List<int> { player.Level };
            levels.AddRange(_companions.Select(c => c.Level));
            StartLevel = Math.Max(1, (int)Math.Round(levels.Average(), MidpointRounding.AwayFromZero));

            StartWave();
            return true;
        }

        /// <summary>Fight the next wave. Only between waves.</summary>
        public bool NextWave()
        {
            if (State != ArmyCampState.BetweenWaves) return false;
            StartWave();
            return true;
        }

        /// <summary>Leave between waves, keeping the whole pot. Only between waves.</summary>
        public bool Leave()
        {
            if (State != ArmyCampState.BetweenWaves) return false;
            Finish(keepWholePot: true);
            return true;
        }

        private void StartWave()
        {
            State = ArmyCampState.InWave;

            int level = SoldierLevel(StartLevel, CurrentWave);
            var soldiers = Enumerable.Range(0, SoldiersInWave(CurrentWave))
                .Select(_ => (EnemyBase)new ArmySoldier(level))
                .ToList();

            _combat.CombatFinished -= OnWaveFinished;
            _combat.CombatFinished += OnWaveFinished;
            _combat.StartCombat(_player, soldiers, _companions.Cast<Entity>().ToList(), rules: FightRules.ArmyCampWave);
        }

        private void OnWaveFinished(CombatResult result)
        {
            _combat.CombatFinished -= OnWaveFinished;

            bool won = result.Outcome == CombatOutcome.Won || result.Outcome == CombatOutcome.OpponentFled;
            if (won)
            {
                PotExperience += result.ExperienceGained;
                PotGold += result.GoldGained;

                var rarity = PrizeRarity(CurrentWave);
                if (rarity.HasValue)
                {
                    var prize = CreatePrize(rarity.Value, SoldierLevel(StartLevel, CurrentWave));
                    if (prize != null) _potItems.Add(prize);
                }

                WavesBeaten = CurrentWave;
                CurrentWave++;
                State = ArmyCampState.BetweenWaves;
                WaveFinished?.Invoke(result);
                return;
            }

            // Lost or surrendered mid-wave (fleeing is refused): half the pot
            WaveFinished?.Invoke(result);
            Finish(keepWholePot: false);
        }

        /// <summary>
        /// A random weapon or armor piece of the prize rarity (or the nearest lower one there
        /// is) at the wave's level.
        /// </summary>
        private Item CreatePrize(ItemRarity rarity, int level)
        {
            for (var r = rarity; r >= ItemRarity.Common; r--)
            {
                var weapons = ItemDatabase.GetAllWeapons().Where(w => w.Rarity == r).Select(w => w.Name).ToList();
                var armor = ItemDatabase.GetAllArmor().Where(a => a.Rarity == r).Select(a => a.Name).ToList();
                if (weapons.Count == 0 && armor.Count == 0) continue;

                bool pickWeapon = armor.Count == 0 || (weapons.Count > 0 && _rng.Roll(1, 2) == 1);
                if (pickWeapon)
                {
                    return ItemDatabase.GetWeapon(weapons[_rng.Next(0, weapons.Count)], level);
                }
                return ItemDatabase.GetArmor(armor[_rng.Next(0, armor.Count)], level);
            }
            return null;
        }

        /// <summary>
        /// Pay the pot (all of it, or half), heal and revive the whole lineup, record the best
        /// wave. Half rounds down; for the prize items, the best half is kept (rounded down -
        /// my default).
        /// </summary>
        private void Finish(bool keepWholePot)
        {
            var result = new ArmyCampResult
            {
                WavesBeaten = WavesBeaten,
                KeptWholePot = keepWholePot,
                ExperienceGained = keepWholePot ? PotExperience : PotExperience / 2,
                GoldGained = keepWholePot ? PotGold : PotGold / 2
            };

            // Later prizes are rarer, so the best half is the last half
            var items = keepWholePot ? _potItems.ToList() : _potItems.Skip(_potItems.Count - _potItems.Count / 2).ToList();

            // XP: the player and every companion in the lineup get the whole amount (my
            // default - benched companions get nothing; they weren't in the attempt)
            _player.AddExperience(result.ExperienceGained);
            foreach (var companion in _companions)
            {
                companion.GainExperience(result.ExperienceGained);
            }

            _player.Gold += result.GoldGained;

            foreach (var item in items)
            {
                if (_player.AddToInventory(item)) result.ItemsGained.Add(item);
                else result.ItemsLeftBehind.Add(item);
            }

            // Everyone in the lineup is fully healed and revived, with nothing left on them
            foreach (var member in new Entity[] { _player }.Concat(_companions))
            {
                member.Health = member.MaxHealth;
                member.ActiveEffects.Clear();
            }

            result.NewBestWave = Statistics?.RecordArmyCampAttempt(WavesBeaten) ?? false;

            State = ArmyCampState.Finished;
            LastResult = result;
            AttemptFinished?.Invoke(result);
        }
    }
}
