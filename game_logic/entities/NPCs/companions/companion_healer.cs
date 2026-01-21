using System;
using GameLogic.Abilities;
using GameLogic.Entities;
using GameLogic.Systems;

namespace GameLogic.Entities.NPCs.Companions
{
    /// <summary>
    /// Healer companion - Support and healing specialist
    /// Unique Ability: "Divine Light" - Heals player and removes debuffs
    /// Passive Bonus: +2 HP regeneration per turn for player
    /// </summary>
    public class CompanionHealer : CompanionBase
    {
        public override string PassiveBonusDescription => "+2 HP Regeneration per turn";

        private int _regenAmount = 2;

        public CompanionHealer(int startingLevel = 1)
            : base("Aria the Pure", startingLevel, CompanionAIStyle.Supportive)
        {
            Description = "A devoted healer who protects allies with divine magic.";
            Greeting = "May the light guide us.";

            // Add some dialogue
            AddDialogue("I will keep you safe.");
            AddDialogue("The divine protects us.");
            AddDialogue("Let me tend to your wounds.");

            // Set starting stats (medium health, support focus)
            MaxHealth = 90 + (startingLevel * 9);
            Health = MaxHealth;

            // Speed: Moderate - focused on support rather than speed
            Speed = 11 + (startingLevel / 3); // Healers are moderate speed

            // Initialize unique ability
            UniqueAbility = new DivineLightAbility(this);

            // Equip starting weapon
            EquippedWeapon = Items.ItemDatabase.GetWeapon("cracked staff");
            EquippedArmor = Items.ItemDatabase.GetArmor("linen robes");
        }

        /// <summary>
        /// Apply healer's passive bonus: +2 HP regen per turn
        /// </summary>
        public override void ApplyPassiveBonus(Player.Player player)
        {
            Console.WriteLine($"{Name}'s presence is soothing! (+{_regenAmount} HP regeneration per turn)");
        }

        /// <summary>
        /// Remove healer's passive bonus
        /// </summary>
        public override void RemovePassiveBonus(Player.Player player)
        {
            Console.WriteLine($"{Name}'s healing aura fades. (-{_regenAmount} HP regeneration per turn)");
        }

        /// <summary>
        /// Apply regeneration to player (called each turn by game manager)
        /// </summary>
        public void ApplyRegeneration(Player.Player player)
        {
            if (!InParty) return;

            int healAmount = _regenAmount + (Level / 10);  // Scales slightly with level
            player.Heal(healAmount);
            Console.WriteLine($"✨ {Name}'s divine aura heals you for {healAmount} HP!");
        }

        /// <summary>
        /// Healer levels up - moderate speed gain (same as player)
        /// </summary>
        public override void LevelUp()
        {
            base.LevelUp(); // Handle health and base stats

            // Healer gains speed at player rate: every 3 levels
            if (Level % 3 == 0)
            {
                Speed++;
                Console.WriteLine($"{Name}'s divine grace improved agility! Speed increased to {Speed}!");
            }
        }
    }

    /// <summary>
    /// Divine Light - Unique ability for healer companion
    /// Heals player and cleanses debuffs
    /// </summary>
    public class DivineLightAbility : Ability
    {
        private readonly CompanionHealer _healer;

        public DivineLightAbility(CompanionHealer healer)
        {
            _healer = healer;
            Name = "Divine Light";
            Description = "Channel divine energy to heal and cleanse";
            TargetType = AbilityTarget.Ally;  // Targets player/self
            Rarity = AbilityRarity.Rare;

            Cooldown = 4;  // 4 turn cooldown
            Level = healer.Level;
        }

        public override void Execute(Entity user, Entity target, RNGManager rng)
        {
            Console.WriteLine($"\n{_healer.Name} uses {Name}!");
            Console.WriteLine($"✨ Divine light radiates from {_healer.Name}!");

            // Calculate healing based on healer's level
            int baseHealing = 20 + (_healer.Level * 2);  // 20-220 healing
            int healing = rng.Roll(baseHealing, baseHealing + 15);

            target.Heal(healing);
            Console.WriteLine($"💚 {target.Name} is healed for {healing} HP!");
            Console.WriteLine($"   {target.Name}'s HP: {target.Health}/{target.MaxHealth}");

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
                        Console.WriteLine($"✨ {debuff.Name} was cleansed from {target.Name}!");
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

        protected override void OnLevelUp()
        {
            Console.WriteLine($"{Name}'s divine power grows stronger!");
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
            Console.WriteLine($"  💚 {Name}: {target.Name} regenerates {_healingPerTurn} HP! ({Duration} turns left)");
            Console.WriteLine($"     {target.Name}'s HP: {target.Health}/{target.MaxHealth}");
        }

        public override void OnApplied(Entity target)
        {
            Console.WriteLine($"💚 {target.Name} is regenerating health!");
        }

        public override void OnExpired(Entity target)
        {
            Console.WriteLine($"💚 {target.Name}'s regeneration fades.");
        }

        public override string GetDisplayInfo()
        {
            return $"{Name} (+{_healingPerTurn} HP, {Duration} turns)";
        }
    }
}