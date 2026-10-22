using System;
using GameLogic.Abilities;
using GameLogic.Entities;
using GameLogic.Systems;

namespace GameLogic.Entities.NPCs.Companions
{
    /// <summary>
    /// Risk, Captain of the Royal Guard - tanky melee fighter (reworked from the old Warrior companion, 2026-10-21)
    /// Unique Ability: "Royal Guard's Armor" (passive) - while wearing armor, every hit that lands has a
    /// chance to do no damage at all: 15% at level 1, +1% every other level, doubled while defending,
    /// capped at 70%. Only weapon and other non-magical attacks; status effects the hit carries still apply.
    /// Passive Bonus: +10% max health for player when in party
    /// </summary>
    public class CompanionRisk : CompanionBase
    {
        public override string PassiveBonusDescription => "+10% Max Health for player";

        private int _playerMaxHealthBonus = 0; // Track the bonus we applied

        public RoyalGuardArmorAbility ArmorAbility { get; }

        public CompanionRisk(int startingLevel = 1)
            : base("Risk", startingLevel, CompanionAIStyle.Defensive)
        {
            Description = "Captain of the Royal Guard. His armor takes the hits so his allies don't have to.";
            Greeting = "I'll protect you with my life!";

            // Add some dialogue
            AddDialogue("Stay behind me!");
            AddDialogue("Their attacks mean nothing to me.");
            AddDialogue("I've faced worse odds.");

            // Set starting stats (higher health, medium damage)
            MaxHealth = 120 + (startingLevel * 12);  // More health than other companions
            Health = MaxHealth;

            // Speed: Slower due to heavy armor
            Speed = 7 + (startingLevel / 3); // Slow but steady

            // Unique ability: the passive no-damage chance
            ArmorAbility = new RoyalGuardArmorAbility(this);
            UniqueAbility = ArmorAbility;

            // Equip starting weapon
            EquippedWeapon = Items.ItemDatabase.GetWeapon("rusty sword");
            EquippedArmor = Items.ItemDatabase.GetArmor("rusty chainmail set");
        }

        /// <summary>
        /// Roll whether a hit that landed does no damage (called by the combat manager)
        /// </summary>
        public bool RollNoDamage(RNGManager rng, bool isDefending)
        {
            int chance = ArmorAbility.GetNoDamageChance(isDefending);
            return chance > 0 && rng.Roll(1, 100) <= chance;
        }

        /// <summary>
        /// Apply Risk's passive bonus: +10% max health to player
        /// </summary>
        public override void ApplyPassiveBonus(Player.Player player)
        {
            _playerMaxHealthBonus = (int)(player.MaxHealth * 0.10);
            player.MaxHealth += _playerMaxHealthBonus;
            player.Health += _playerMaxHealthBonus; // Also heal the amount

            GameLog.Write($"{Name}'s presence bolsters you! (+{_playerMaxHealthBonus} Max HP)");
        }

        /// <summary>
        /// Remove Risk's passive bonus
        /// </summary>
        public override void RemovePassiveBonus(Player.Player player)
        {
            player.MaxHealth -= _playerMaxHealthBonus;

            // Make sure current health doesn't exceed new max
            if (player.Health > player.MaxHealth)
            {
                player.Health = player.MaxHealth;
            }

            GameLog.Write($"{Name}'s bonus fades. (-{_playerMaxHealthBonus} Max HP)");
            _playerMaxHealthBonus = 0;
        }

        /// <summary>
        /// Risk levels up - slower speed gain due to heavy armor
        /// </summary>
        public override void LevelUp()
        {
            base.LevelUp(); // Handle health and base stats

            // Gains speed slowly: every 5 levels (slower than player's 3)
            if (Level % 5 == 0)
            {
                Speed++;
                GameLog.Write($"{Name}'s heavy armor training improved agility! Speed increased to {Speed}!");
            }

            // The no-damage chance goes up 1% on every even level
            if (Level % 2 == 0)
            {
                GameLog.Write($"{ArmorAbility.Name}: {Name} now shrugs off {ArmorAbility.GetNoDamageChance(false)}% of hits ({ArmorAbility.GetNoDamageChance(true)}% while defending).");
            }
        }
    }

    /// <summary>
    /// Royal Guard's Armor - Risk's unique ability (passive, always on)
    /// While he wears armor, a hit that lands has a chance to do no damage at all.
    /// Only the damage is stopped - poison, burns and other effects still apply.
    /// </summary>
    public class RoyalGuardArmorAbility : Ability
    {
        /// <summary>
        /// No-damage chance at level 1, in percent
        /// </summary>
        public const int BaseNoDamageChance = 15;

        /// <summary>
        /// Highest the chance can go, defending or not (reached at level 50 at the latest)
        /// </summary>
        public const int MaxNoDamageChance = 70;

        private readonly CompanionRisk _risk;

        public RoyalGuardArmorAbility(CompanionRisk risk)
        {
            _risk = risk;
            Name = "Royal Guard's Armor";
            Description = "Braced in his armor, weapon hits sometimes do him no damage at all. Twice as likely while defending. Magic still gets through.";
            IsPassive = true;
            TargetType = AbilityTarget.Self;
            Rarity = AbilityRarity.Uncommon;
            Cooldown = 0;
        }

        /// <summary>
        /// Chance (0-100) that a hit does no damage: 15% at level 1, +1% every other level
        /// (Risk's level, not the ability's), doubled while defending, capped at 70%. 0 without armor.
        /// Only weapon and other non-magical attacks can be stopped.
        /// </summary>
        public int GetNoDamageChance(bool isDefending)
        {
            if (_risk.EquippedArmor == null) return 0;

            int chance = BaseNoDamageChance + _risk.Level / 2;
            if (isDefending) chance *= 2;
            return Math.Min(MaxNoDamageChance, chance);
        }

        public override string GetInfo()
        {
            string armorNote = _risk.EquippedArmor == null ? "\n(No armor worn - inactive)" : "";

            return $"{Name} [PASSIVE]\n" +
                   $"{Description}\n" +
                   $"No damage: {GetNoDamageChance(false)}% ({GetNoDamageChance(true)}% while defending){armorNote}";
        }
    }
}
