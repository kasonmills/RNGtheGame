using System;
using System.Collections.Generic;
using System.Linq;
using GameLogic.Abilities;
using GameLogic.Entities;
using GameLogic.Systems;

namespace GameLogic.Entities.NPCs.Companions
{
    /// <summary>
    /// Felicity, the Healer - the party's healer (reworked from the old Healer companion, 2026-10-20)
    /// Unique Ability: "Divine Light" - heals one ally (herself included) and removes debuffs
    /// Level 20: also learns "Healing Circle" - heals the whole party
    /// On auto: heals whenever anyone in the party is below 50% health, otherwise attacks with her quarterstaff
    /// </summary>
    public class CompanionFelicity : CompanionBase
    {
        /// <summary>
        /// Companion level at which Felicity learns Healing Circle
        /// </summary>
        public const int PartyHealLevel = 20;

        /// <summary>
        /// Below this share of max health, Felicity heals instead of attacking
        /// </summary>
        private const float HealThreshold = 0.5f;

        public HealingCircleAbility PartyHeal { get; private set; }

        public bool HasPartyHeal => Level >= PartyHealLevel;

        public CompanionFelicity(int startingLevel = 1)
            : base("Felicity", startingLevel, CompanionAIStyle.Supportive)
        {
            Description = "The party's healer. She keeps everyone standing, herself included.";
            Greeting = "Stay close - I can't heal you from across the field.";

            // Add some dialogue
            AddDialogue("I will keep you safe.");
            AddDialogue("Hold still, let me tend to that.");
            AddDialogue("Nobody falls while I'm here.");

            // Set starting stats (medium health, support focus)
            MaxHealth = 90 + (startingLevel * 9);
            Health = MaxHealth;

            // Speed: Moderate - focused on support rather than speed
            Speed = 11 + (startingLevel / 3); // Healers are moderate speed

            // Abilities: single-target heal from the start, party heal from level 20
            UniqueAbility = new DivineLightAbility(this);
            PartyHeal = new HealingCircleAbility(this);

            // Equip starting weapon
            EquippedWeapon = Items.ItemDatabase.GetWeapon("quarterstaff");
            EquippedArmor = Items.ItemDatabase.GetArmor("rough linen set");
        }

        public override List<Ability> GetCombatAbilities()
        {
            var abilities = base.GetCombatAbilities();
            if (HasPartyHeal)
            {
                abilities.Add(PartyHeal);
            }
            return abilities;
        }

        /// <summary>
        /// Heal when anyone standing is below 50% health, otherwise attack. She never defends on auto.
        /// </summary>
        public override int DecideCombatAction(Enemies.EnemyBase target, List<Entity> allies, RNGManager rng)
        {
            if (CountBelowThreshold(allies) > 0 && ChooseCombatAbility(allies) != null)
            {
                return 1; // Heal
            }
            return 0; // Attack with her quarterstaff
        }

        /// <summary>
        /// Healing Circle when two or more allies are below 50% (or Divine Light is on cooldown),
        /// otherwise Divine Light on the most wounded. Null when no heal is ready.
        /// </summary>
        public override Ability ChooseCombatAbility(List<Entity> allies)
        {
            int hurt = CountBelowThreshold(allies);
            bool singleReady = UniqueAbility != null && !UniqueAbility.IsOnCooldown();
            bool partyReady = HasPartyHeal && !PartyHeal.IsOnCooldown();

            if (partyReady && (hurt >= 2 || !singleReady))
            {
                return PartyHeal;
            }
            return singleReady ? UniqueAbility : null;
        }

        private static int CountBelowThreshold(List<Entity> allies)
        {
            return allies.Count(ally => ally.IsAlive() && (float)ally.Health / ally.MaxHealth < HealThreshold);
        }

        /// <summary>
        /// Felicity levels up - moderate speed gain (same as player); learns Healing Circle at level 20
        /// </summary>
        public override void LevelUp()
        {
            base.LevelUp(); // Handle health and base stats

            // Healer gains speed at player rate: every 3 levels
            if (Level % 3 == 0)
            {
                Speed++;
                GameLog.Write($"{Name}'s agility improved! Speed increased to {Speed}!");
            }

            if (Level == PartyHealLevel)
            {
                GameLog.Write($"✨ {Name} learned {PartyHeal.Name}! She can now heal the whole party at once.");
            }
        }
    }

    /// <summary>
    /// Shared base for Felicity's heals - anything both heals do goes here once
    /// </summary>
    public abstract class FelicityHealAbility : Ability
    {
        /// <summary>
        /// Called by Ability.LevelUp() whenever either heal levels up
        /// </summary>
        protected override void OnLevelUp()
        {
            GameLog.Write($"{Name} grows stronger!");
        }
    }

    /// <summary>
    /// Divine Light - Felicity's single-target heal
    /// Heals one ally (scaling with her level) and cleanses debuffs
    /// </summary>
    public class DivineLightAbility : FelicityHealAbility
    {
        private readonly CompanionFelicity _healer;

        public DivineLightAbility(CompanionFelicity healer)
        {
            _healer = healer;
            Name = "Divine Light";
            Description = "Heal one ally and cleanse their debuffs";
            TargetType = AbilityTarget.Ally;  // Any standing party member, herself included
            Rarity = AbilityRarity.Rare;

            Cooldown = 2;  // 2 turn cooldown (was 4 - shortened 2026-10-20 so she can keep up)
            Level = healer.Level;
        }

        public override void Execute(Entity user, Entity target, RNGManager rng)
        {
            GameLog.Write($"✨ Divine light radiates from {_healer.Name}!");

            // Calculate healing based on healer's level
            int baseHealing = 20 + (_healer.Level * 2);  // 20-220 healing
            int healing = rng.Roll(baseHealing, baseHealing + 15);

            target.Heal(healing);
            GameLog.Write($"💚 {target.Name} is healed for {healing} HP!");
            GameLog.Write($"   {target.Name}'s HP: {target.Health}/{target.MaxHealth}");

            // Remove debuffs with chance (scales with level)
            int cleanseChance = 60 + _healer.Level / 2;  // 60-110% chance (capped at 100)
            if (cleanseChance > 100) cleanseChance = 100;

            if (target.ActiveEffects.Count > 0)
            {
                // Try to cleanse debuffs
                var debuffs = target.ActiveEffects.FindAll(e =>
                    e.Type == EffectType.Debuff || e.Type == EffectType.DamageOverTime);

                foreach (var debuff in debuffs)
                {
                    if (rng.Roll(1, 100) <= cleanseChance)
                    {
                        target.RemoveEffect(debuff);
                        GameLog.Write($"✨ {debuff.Name} was cleansed from {target.Name}!");
                    }
                }
            }

            // Apply temporary regeneration buff
            int regenDuration = 3;
            int regenPotency = 5 + (_healer.Level / 5);
            var regenEffect = new RegenerationEffect(regenDuration, regenPotency);
            target.AddEffect(regenEffect);

            // Set cooldown (uses level-based reduction)
            CurrentCooldown = GetEffectiveCooldown();

            // Gain scaled combat experience
            GainCombatExperience();
        }

        public override string GetInfo()
        {
            int baseHealing = 20 + (_healer.Level * 2);
            int cleanseChance = Math.Min(100, 60 + _healer.Level / 2);
            int regenPotency = 5 + (_healer.Level / 5);
            string cooldownInfo = CurrentCooldown > 0 ? $" (Cooldown: {CurrentCooldown})" : "";

            return $"{Name}{cooldownInfo}\n" +
                   $"{Description}\n" +
                   $"Healing: {baseHealing}-{baseHealing + 15} HP\n" +
                   $"Cleanse: {cleanseChance}% chance to remove debuffs\n" +
                   $"Effect: Regeneration ({regenPotency} HP/turn for 3 turns)";
        }
    }

    /// <summary>
    /// Healing Circle - Felicity's party heal, learned at level 20
    /// Heals every standing party member for 60% of what Divine Light would heal
    /// </summary>
    public class HealingCircleAbility : FelicityHealAbility
    {
        private const double HealShare = 0.6;

        private readonly CompanionFelicity _healer;

        public HealingCircleAbility(CompanionFelicity healer)
        {
            _healer = healer;
            Name = "Healing Circle";
            Description = "Heal the whole party at once";
            TargetType = AbilityTarget.AllAllies;
            Rarity = AbilityRarity.Rare;

            Cooldown = 4;
            Level = healer.Level;
        }

        private int MinHealing => (int)((20 + _healer.Level * 2) * HealShare);
        private int MaxHealing => (int)((35 + _healer.Level * 2) * HealShare);

        /// <summary>
        /// Used on its own it only heals the one target - the combat manager calls ExecuteOnGroup
        /// </summary>
        public override void Execute(Entity user, Entity target, RNGManager rng)
        {
            ExecuteOnGroup(user, new List<Entity> { target }, rng);
        }

        public override void ExecuteOnGroup(Entity user, List<Entity> targets, RNGManager rng)
        {
            GameLog.Write($"✨ A circle of light spreads out from {_healer.Name}!");

            foreach (var target in targets.Where(t => t.IsAlive()))
            {
                int healing = rng.Roll(MinHealing, MaxHealing);
                target.Heal(healing);
                GameLog.Write($"💚 {target.Name} is healed for {healing} HP! ({target.Health}/{target.MaxHealth})");
            }

            // Set cooldown (uses level-based reduction)
            CurrentCooldown = GetEffectiveCooldown();

            // Gain scaled combat experience
            GainCombatExperience();
        }

        public override string GetInfo()
        {
            string cooldownInfo = CurrentCooldown > 0 ? $" (Cooldown: {CurrentCooldown})" : "";

            return $"{Name}{cooldownInfo}\n" +
                   $"{Description}\n" +
                   $"Healing: {MinHealing}-{MaxHealing} HP to each party member";
        }
    }

    /// <summary>
    /// Regeneration status effect - Healing over time
    /// </summary>
    public class RegenerationEffect : AbilityEffect
    {
        private readonly int _healingPerTurn;

        public RegenerationEffect(int duration, int healingPerTurn)
            : base(duration, healingPerTurn)
        {
            _healingPerTurn = healingPerTurn;
            Name = "Regeneration";
            Description = $"Healing {healingPerTurn} HP per turn";
            Type = EffectType.Buff;
            CanStack = false;
        }

        public override void ApplyEffect(Entity target, RNGManager rng)
        {
            target.Heal(_healingPerTurn);
            GameLog.Write($"  💚 {Name}: {target.Name} regenerates {_healingPerTurn} HP! ({Duration} turns left)");
            GameLog.Write($"     {target.Name}'s HP: {target.Health}/{target.MaxHealth}");
        }

        public override void OnApplied(Entity target)
        {
            GameLog.Write($"💚 {target.Name} is regenerating health!");
        }

        public override void OnExpired(Entity target)
        {
            GameLog.Write($"💚 {target.Name}'s regeneration fades.");
        }

        public override string GetDisplayInfo()
        {
            return $"{Name} (+{_healingPerTurn} HP, {Duration} turns)";
        }
    }
}
