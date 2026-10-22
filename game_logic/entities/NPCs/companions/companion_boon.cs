using System;
using System.Collections.Generic;
using GameLogic.Abilities;
using GameLogic.Entities;
using GameLogic.Items;
using GameLogic.Systems;
using Enemy = GameLogic.Entities.Enemies.EnemyBase;

namespace GameLogic.Entities.NPCs.Companions
{
    /// <summary>
    /// Boon, the Sharpshooter - a dwarf with a crossbow he built himself (reworked from the old Ranger companion, 2026-10-21)
    /// Rarely misses: 95% with Boon's Crossbow (the one he built), on every difficulty.
    /// With any other weapon his accuracy is that weapon's, and the difficulty penalty applies.
    /// Unique Ability: "Special Shot" - fires a special bolt from the party bag (Piercing, Pinning, Fire, Ice, Acid).
    /// If the chosen bolt type has run out, he throws an axe instead of wasting the turn.
    /// Joins with 5 Piercing and 5 Pinning bolts. Ordinary bolts are unlimited.
    /// Passive Bonus: +10% accuracy for player
    /// </summary>
    public class CompanionBoon : CompanionBase
    {
        public override string PassiveBonusDescription => "+10% Accuracy for player";

        /// <summary>
        /// His hit chance with his own crossbow, on every difficulty
        /// </summary>
        public const int OwnCrossbowAccuracy = 95;

        /// <summary>
        /// Item database name of the crossbow he built himself
        /// </summary>
        public const string OwnCrossbowName = "boon's crossbow";

        /// <summary>
        /// Piercing and Pinning bolts he brings into the party bag when he joins
        /// </summary>
        public const int StartingBoltsEach = 5;

        /// <summary>
        /// On auto, a Piercing bolt is used on targets with at least this much defense
        /// </summary>
        private const int PiercingDefenseThreshold = 5;

        private int _accuracyBonus = 10;

        public SpecialShotAbility SpecialShot { get; }

        public CompanionBoon(int startingLevel = 1)
            : base("Boon", startingLevel, CompanionAIStyle.Balanced)
        {
            Description = "A dwarven sharpshooter with a crossbow of his own making. He rarely misses, and he'll tell you so.";
            Greeting = "Point me at it. I won't miss.";

            // Add some dialogue - he never lets the party forget how rarely he misses
            AddDialogue("Did you see that? Didn't miss.");
            AddDialogue("Built this crossbow myself. It doesn't miss either.");
            AddDialogue("Remind me again how many times I've missed? That's right.");

            // Set starting stats (medium health, high accuracy)
            MaxHealth = 95 + (startingLevel * 9);
            Health = MaxHealth;

            // Speed: Fast - light armor and nimble
            Speed = 13 + (startingLevel / 2);

            // Unique ability: special bolts
            SpecialShot = new SpecialShotAbility(this);
            UniqueAbility = SpecialShot;

            // Starting gear - his own crossbow; armor is a placeholder until default equipment is decided
            EquippedWeapon = Items.ItemDatabase.GetWeapon(OwnCrossbowName);
            EquippedArmor = Items.ItemDatabase.GetArmor("worn leather armor");
        }

        /// <summary>
        /// True while he's using the crossbow he built himself
        /// </summary>
        public bool HasOwnCrossbow => EquippedWeapon != null
            && string.Equals(EquippedWeapon.Name, "Boon's Crossbow", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// He rarely misses - but only with his own crossbow (95% on every difficulty).
        /// With any other weapon, his accuracy is that weapon's, like anyone else.
        /// </summary>
        public override int AttackAccuracy => HasOwnCrossbow ? OwnCrossbowAccuracy : base.AttackAccuracy;

        /// <summary>
        /// 5 Piercing and 5 Pinning bolts go into the party bag when he first joins
        /// </summary>
        public override List<Item> GetStartingPartyItems()
        {
            return new List<Item>
            {
                new Ammunition(BoltType.Piercing, StartingBoltsEach),
                new Ammunition(BoltType.Pinning, StartingBoltsEach)
            };
        }

        /// <summary>
        /// On auto his special shots are chosen by ChooseBolt (the combat manager asks first),
        /// so the generic "use ability" decision falls back to a normal shot.
        /// </summary>
        public override Ability ChooseCombatAbility(List<Entity> allies)
        {
            return null;
        }

        /// <summary>
        /// On auto: which special bolt fits this target, from the types in the party bag, or null
        /// for an ordinary shot. Pinning on a target at least as fast as him that isn't pinned yet;
        /// Piercing on a well-armored target; Fire on a target that isn't already burning.
        /// Ice and Acid have no effect designed yet, so he doesn't pick them on his own.
        /// </summary>
        public BoltType? ChooseBolt(Enemy target, ICollection<BoltType> inStock)
        {
            if (target == null || inStock.Count == 0) return null;

            if (inStock.Contains(BoltType.Pinning) && target.Speed >= Speed && !target.HasEffect<PinnedEffect>())
            {
                return BoltType.Pinning;
            }
            if (inStock.Contains(BoltType.Piercing) && target.Defense >= PiercingDefenseThreshold)
            {
                return BoltType.Piercing;
            }
            if (inStock.Contains(BoltType.Fire) && !target.HasEffect<BurnEffect>())
            {
                return BoltType.Fire;
            }
            return null;
        }

        /// <summary>
        /// Apply Boon's passive bonus: +10% accuracy
        /// </summary>
        public override void ApplyPassiveBonus(Player.Player player)
        {
            GameLog.Write($"{Name}'s keen eye steadies your aim! (+{_accuracyBonus}% Accuracy)");
        }

        /// <summary>
        /// Remove Boon's passive bonus
        /// </summary>
        public override void RemovePassiveBonus(Player.Player player)
        {
            GameLog.Write($"{Name}'s guidance fades. (-{_accuracyBonus}% Accuracy)");
        }

        /// <summary>
        /// Get accuracy bonus amount
        /// </summary>
        public int GetAccuracyBonus()
        {
            return InParty ? _accuracyBonus : 0;
        }

        /// <summary>
        /// Boon levels up - fast speed gain (light armor, quick reflexes)
        /// </summary>
        public override void LevelUp()
        {
            base.LevelUp(); // Handle health and base stats

            // Gains speed quickly: every 2 levels
            if (Level % 2 == 0)
            {
                Speed++;
                GameLog.Write($"{Name}'s reflexes sharpened! Speed increased to {Speed}!");
            }
        }
    }

    /// <summary>
    /// Special Shot - Boon's unique ability. Fires one special bolt from the party bag as a
    /// crossbow attack (same hit, crit and defense rules as his normal shot), with the bolt's
    /// effect on top. Resolved by CombatManager, which owns the party bag; the bolt type
    /// travels on the action as an Ammunition item.
    /// </summary>
    public class SpecialShotAbility : Ability
    {
        /// <summary>
        /// A thrown axe (when the chosen bolt has run out) hits for this share of a normal shot
        /// </summary>
        public const double AxeDamageShare = 0.75;

        // Pinning bolt slow - placeholder numbers
        public const int PinDuration = 2;
        public const int PinSpeedReduction = 5;

        // Fire bolt burn - placeholder numbers
        public const int BurnDuration = 3;

        private readonly CompanionBoon _boon;

        public SpecialShotAbility(CompanionBoon boon)
        {
            _boon = boon;
            Name = "Special Shot";
            Description = "Fire a special bolt from the party bag. Out of that bolt? He throws an axe instead.";
            TargetType = AbilityTarget.SingleEnemy;
            Rarity = AbilityRarity.Uncommon;

            Cooldown = 0;  // Limited by bolts, not by a cooldown
            Level = boon.Level;
        }

        /// <summary>
        /// Burn strength per turn for a Fire bolt (scales with Boon's level)
        /// </summary>
        public int BurnPotency => 3 + _boon.Level / 5;

        /// <summary>
        /// Special shots are resolved by CombatManager (it needs the party bag and the attack rules)
        /// </summary>
        public override void Execute(Entity user, Entity target, RNGManager rng)
        {
            GameLog.Write($"{Name} has to be fired in a fight.");
        }

        public override string GetInfo()
        {
            return $"{Name}\n" +
                   $"{Description}\n" +
                   $"Piercing: ignores defense\n" +
                   $"Pinning: -{PinSpeedReduction} speed for {PinDuration} rounds\n" +
                   $"Fire: burns for {BurnDuration} rounds\n" +
                   $"Ice, Acid: effects to come";
        }

        protected override void OnLevelUp()
        {
            GameLog.Write($"{Name} becomes more precise!");
        }
    }

    /// <summary>
    /// Pinned status effect (Boon's pinning bolt) - reduces the target's speed so it acts later.
    /// Read by CombatManager's effective-speed calculation, like Skarn's grapple.
    /// </summary>
    public class PinnedEffect : AbilityEffect
    {
        public PinnedEffect(int duration, int speedReduction)
            : base(duration, speedReduction) // Potency = speedReduction
        {
            Name = "Pinned";
            Description = "Speed reduced - pinned in place by a barbed bolt.";
            Type = EffectType.CrowdControl;
            CanStack = false;
        }

        public override void ApplyEffect(Entity target, RNGManager rng)
        {
            // No recurring damage - purely a speed debuff for its duration.
        }

        public override void OnApplied(Entity target)
        {
            GameLog.Write($"📌 {target.Name} is pinned in place!");
        }

        public override void OnExpired(Entity target)
        {
            GameLog.Write($"📌 {target.Name} pulls free.");
        }
    }
}
