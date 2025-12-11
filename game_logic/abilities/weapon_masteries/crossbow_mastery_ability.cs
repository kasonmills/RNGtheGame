using GameLogic.Entities;
using GameLogic.Systems;

namespace GameLogic.Abilities.WeaponMasteries
{
    /// <summary>
    /// Crossbow Mastery - Passive ability that provides bonuses when wielding a crossbow
    /// Level 1: +1% accuracy, +1% attack damage, +1% crit chance
    /// Level 100: +40% accuracy, +70% attack damage, +25% crit chance
    /// </summary>
    public class CrossbowMasteryAbility : Ability
    {
        public CrossbowMasteryAbility()
        {
            Name = "Crossbow Mastery";
            Description = "Your expertise with crossbows grants you enhanced accuracy, damage, and critical strike potential when wielding a crossbow.";
            IsPassive = true;
            Rarity = AbilityRarity.Rare;
            Cooldown = 0;
            TargetType = AbilityTarget.Self;
        }

        public int GetAccuracyBonus() => GetScaledValueInt(1, 40);
        public int GetAttackDamageBonus() => GetScaledValueInt(1, 70);
        public int GetCritChanceBonus() => GetScaledValueInt(1, 25);
        public double GetAccuracyMultiplier() => GetAccuracyBonus() / 100.0;
        public double GetDamageMultiplier() => 1.0 + (GetAttackDamageBonus() / 100.0);

        public bool IsWieldingCorrectWeapon(Entity entity)
        {
            if (entity is Entities.Player.Player player)
            {
                return player.EquippedWeapon != null &&
                       player.EquippedWeapon.Type == Items.WeaponType.Crossbow;
            }
            return false;
        }

        protected override void OnLevelUp()
        {
            if (Level % 10 == 0)
            {
                Console.WriteLine($"Your crossbow mastery improves! Now: +{GetAccuracyBonus()}% accuracy, +{GetAttackDamageBonus()}% damage, +{GetCritChanceBonus()}% crit chance!");
            }
        }

        public override string GetInfo()
        {
            string info = $"{Name} (Lv.{Level}/{MaxLevel}) [PASSIVE]\\n" +
                         $"{Description}\\n\\n" +
                         $"Current Bonuses (when wielding a Crossbow):\\n" +
                         $"  Accuracy: +{GetAccuracyBonus()}%\\n" +
                         $"  Attack Damage: +{GetAttackDamageBonus()}%\\n" +
                         $"  Critical Hit Chance: +{GetCritChanceBonus()}%\\n";

            if (Level < 100)
            {
                int nextMilestone = ((Level / 10) + 1) * 10;
                if (nextMilestone > 100) nextMilestone = 100;
                Ability tempAbility = new CrossbowMasteryAbility();
                tempAbility.Level = nextMilestone;
                var temp = (CrossbowMasteryAbility)tempAbility;
                info += $"\\nNext Milestone: Level {nextMilestone}\\n" +
                       $"  Accuracy: +{temp.GetAccuracyBonus()}%\\n" +
                       $"  Damage: +{temp.GetAttackDamageBonus()}%\\n" +
                       $"  Crit Chance: +{temp.GetCritChanceBonus()}%";
            }
            else
            {
                info += $"\\nMax level reached! All bonuses at maximum.";
            }

            info += $"\\n\\nXP: {Experience}/{ExperienceToNextLevel}";
            return info;
        }

        public override void Execute(Entity user, Entity target, RNGManager rng)
        {
            Console.WriteLine($"{Name} is a passive ability providing bonuses when wielding a crossbow.");
            Console.WriteLine($"Current bonuses: +{GetAccuracyBonus()}% accuracy, +{GetAttackDamageBonus()}% damage, +{GetCritChanceBonus()}% crit chance");
            Console.WriteLine("It is always active and cannot be manually used.");
        }
    }
}