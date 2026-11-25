using System;
using GameLogic.Abilities;
using GameLogic.Abilities.EnemyAbilities;

namespace GameLogic.Entities.Enemies.EnemyTypes
{
    /// <summary>
    /// The MVP regular enemies (docs/enemy_roster.md - families, groups and names decided
    /// 2026-11-23, the sheet reviewed 2026-11-24). Every enemy is the standard enemy times these
    /// multipliers (StandardEnemy). All numbers are placeholders, to retune after playtesting.
    ///
    /// Not here yet (their own to-do items): level bands and group sizes (the spawn lists and the
    /// encounter-size roll, with the fixed map), random names, the day/night lean, component
    /// drop weights (spiders, beetles and Fungusoars favour their own group), and back-row
    /// combat (IsBackRow is set, rows aren't built).
    /// </summary>
    public static class EnemyRoster
    {
        public const string Goblins = "Goblins";
        public const string Bandits = "Bandits";
        public const string Wolves = "Wolves";
        public const string Slimes = "Slimes";
        public const string Bugs = "Bugs";
        public const string Fungusoars = "Fungusoars";
        public const string Imps = "Imps";
        public const string OwnArmy = "Own army";
        public const string KingdomB = "Kingdom B";   // Placeholder until the kingdom has a name
        public const string CastleGuards = "Castle guards";
        public const string Dragons = "Dragon";

        /// <summary>Slimes share one diminishing-rewards group, so a Big Slime's split can't be farmed.</summary>
        public const string SlimeRewardsGroup = "Slime";

        // ----- Effect strengths (my defaults, 2026-11-24) -----

        /// <summary>Damage per turn of a full-strength burn or bleed at a level (the middle of the poison formula).</summary>
        private static int DotDamage(int level) => Bosses.DemonPrinceMechanic.DotDamage(level);

        private static AbilityEffect Poison(int level)
        {
            var (min, max, duration) = PoisonAttack.GetPoisonStatsForLevel(level);
            return new PoisonedEffect(duration, min, max);
        }

        // ----- The roster -----

        public static readonly EnemyProfile Goblin = new EnemyProfile
        {
            Name = "Goblin", Family = Goblins,
            Description = "A weak but numerous creature",
            Health = 0.7, Damage = 0.8, Defense = 0.5, SpeedBonus = 2,
            Accuracy = 75, CritChance = 5, XPWeight = 0.7, Aggression = 30,
            Behavior = EnemyBehavior.Aggressive, Targeting = EnemyTargetPattern.Random,
            FleeBelowHealth = 0.25, FleeAttemptChance = 100 // Cowardly - always tries
        };

        public static readonly EnemyProfile Bandit = new EnemyProfile
        {
            Name = "Bandit", Family = Bandits,
            Description = "A cunning thief with deadly precision",
            Health = 1.0, Damage = 1.0, Defense = 1.0, SpeedBonus = 3,
            Accuracy = 80, CritChance = 25, XPWeight = 1.2, Aggression = 55, // High crit - sneak attacks
            Behavior = EnemyBehavior.Balanced, Targeting = EnemyTargetPattern.MostWounded,
            FleeBelowHealth = 0.40
        };

        public static readonly EnemyProfile Wolf = new EnemyProfile
        {
            Name = "Wolf", Family = Wolves,
            Description = "A hungry pack hunter",
            Health = 0.8, Damage = 0.9, Defense = 0.5, SpeedBonus = 4,
            Accuracy = 80, CritChance = 10, XPWeight = 0.9, Aggression = 60,
            Behavior = EnemyBehavior.Aggressive, Targeting = EnemyTargetPattern.MostWounded,
            FleeBelowHealth = 0.20,
            OnHitEffectChance = 15, OnHitEffect = level => new BleedEffect(Math.Max(1, DotDamage(level) / 2), 3)
        };

        public static readonly EnemyProfile Slime = new EnemyProfile
        {
            Name = "Slime", Family = Slimes,
            Description = "A wobbling, sticky blob",
            Health = 1.4, Damage = 0.6, Defense = 0.5, SpeedBonus = -3,
            Accuracy = 70, CritChance = 0, XPWeight = 0.8, Aggression = 25,
            Behavior = EnemyBehavior.Aggressive, Targeting = EnemyTargetPattern.Random,
            FleeBelowHealth = 0, // Mindless - never flees
            OnHitEffectChance = 20, OnHitEffect = _ => new SlowedEffect(duration: 2, speedReduction: 2),
            RewardsGroup = SlimeRewardsGroup
        };

        public static readonly EnemyProfile BigSlime = new EnemyProfile
        {
            Name = "Big Slime", Family = Slimes,
            Description = "A huge slime that splits apart when beaten",
            Health = 2.5, Damage = 0.9, Defense = 0.75, SpeedBonus = -4,
            Accuracy = 70, CritChance = 0, XPWeight = 1.8, Aggression = 30,
            Behavior = EnemyBehavior.Aggressive, Targeting = EnemyTargetPattern.Random,
            FleeBelowHealth = 0,
            RewardsGroup = SlimeRewardsGroup
        };

        public static readonly EnemyProfile Spider = new EnemyProfile
        {
            Name = "Spider", Family = Bugs,
            Description = "A venomous, skittering hunter",
            Health = 0.8, Damage = 0.9, Defense = 0.5, SpeedBonus = 2,
            Accuracy = 80, CritChance = 10, XPWeight = 1.0, Aggression = 45,
            Behavior = EnemyBehavior.Aggressive, Targeting = EnemyTargetPattern.WeakestDefense,
            FleeBelowHealth = 0.20,
            OnHitEffectChance = 20, OnHitEffect = Poison
        };

        public static readonly EnemyProfile GiantBeetle = new EnemyProfile
        {
            Name = "Giant Beetle", Family = Bugs,
            Description = "A heavily armored bug - piercing bolts and spells get through",
            Health = 1.8, Damage = 1.0, Defense = 2.5, SpeedBonus = -3,
            Accuracy = 70, CritChance = 5, XPWeight = 1.5, Aggression = 35,
            Behavior = EnemyBehavior.Balanced, Targeting = EnemyTargetPattern.LastAttacker,
            FleeBelowHealth = 0
        };

        public static readonly EnemyProfile Fungusoar = new EnemyProfile
        {
            Name = "Fungusoar", Family = Fungusoars,
            Description = "A mushroom dinosaur with a roar full of spores",
            Health = 2.5, Damage = 1.3, Defense = 1.0, SpeedBonus = -2,
            Accuracy = 70, CritChance = 5, XPWeight = 2.0, Aggression = 55,
            Behavior = EnemyBehavior.Aggressive, Targeting = EnemyTargetPattern.BiggestThreat,
            FleeBelowHealth = 0,
            OnHitEffectChance = 25, OnHitEffect = Poison // Spore Roar
        };

        public static readonly EnemyProfile Imp = new EnemyProfile
        {
            Name = "Imp", Family = Imps,
            Description = "A small, spiteful demon",
            Health = 0.6, Damage = 0.8, Defense = 0.5, SpeedBonus = 4,
            Accuracy = 80, CritChance = 10, XPWeight = 1.0, Aggression = 50,
            Behavior = EnemyBehavior.Aggressive, Targeting = EnemyTargetPattern.Random,
            FleeBelowHealth = 0.25, FleeAttemptChance = 100, BackRow = true,
            // A weaker Burning for now - Hexed and Weakened join when the Demon Prince's new
            // conditions are built
            OnHitEffectChance = 20, OnHitEffect = level => new BurnEffect(Math.Max(1, DotDamage(level) / 2), 2)
        };

        public static readonly EnemyProfile Deserter = new EnemyProfile
        {
            Name = "Deserter", Family = OwnArmy,
            Description = "A soldier who ran from the war",
            Health = 1.1, Damage = 1.0, Defense = 1.5, SpeedBonus = 0,
            Accuracy = 80, CritChance = 10, XPWeight = 1.1, Aggression = 50,
            Behavior = EnemyBehavior.Defensive, Targeting = EnemyTargetPattern.BiggestThreat,
            FleeBelowHealth = 0.30
        };

        public static readonly EnemyProfile ArmySoldier = new EnemyProfile
        {
            Name = "Army Soldier", Family = OwnArmy,
            Description = "A soldier of your army, sparring at the camp",
            Health = 1.1, Damage = 1.0, Defense = 1.5, SpeedBonus = 0,
            Accuracy = 80, CritChance = 10, XPWeight = 1.1, Aggression = 0, // Never a random encounter
            Behavior = EnemyBehavior.Defensive, Targeting = EnemyTargetPattern.BiggestThreat,
            FleeBelowHealth = 0 // Sparring - never flees
        };

        public static readonly EnemyProfile KingdomBSoldier = new EnemyProfile
        {
            Name = "Kingdom B Soldier", Family = KingdomB,
            Description = "A soldier of the enemy kingdom",
            Health = 1.3, Damage = 1.1, Defense = 2.0, SpeedBonus = 0,
            Accuracy = 80, CritChance = 10, XPWeight = 1.3, Aggression = 60,
            Behavior = EnemyBehavior.Defensive, Targeting = EnemyTargetPattern.BiggestThreat,
            FleeBelowHealth = 0.20
        };

        public static readonly EnemyProfile KingdomBArcher = new EnemyProfile
        {
            Name = "Kingdom B Archer", Family = KingdomB,
            Description = "A sharp-eyed archer of the enemy kingdom",
            Health = 0.8, Damage = 1.1, Defense = 1.0, SpeedBonus = 1,
            Accuracy = 90, CritChance = 15, XPWeight = 1.2, Aggression = 50,
            Behavior = EnemyBehavior.Balanced, Targeting = EnemyTargetPattern.WeakestDefense,
            FleeBelowHealth = 0.30, BackRow = true
        };

        public static readonly EnemyProfile CastleGuard = new EnemyProfile
        {
            Name = "Castle Guard", Family = CastleGuards,
            Description = "A guard of the royal castle, armored and loyal",
            Health = 1.0, Damage = 1.0, Defense = 1.0, SpeedBonus = 0,
            Accuracy = 75, CritChance = 5, XPWeight = 1.0, Aggression = 50,
            Behavior = EnemyBehavior.Balanced, Targeting = EnemyTargetPattern.LastAttacker,
            FleeBelowHealth = 0.20 // Never in the final fight - nothing flees in a boss fight
        };

        public static readonly EnemyProfile Dragon = new EnemyProfile
        {
            Name = "Dragon", Family = Dragons,
            Description = "A legendary beast of immense power",
            Type = EnemyType.Elite,
            Health = 4.0, Damage = 2.0, Defense = 3.0, SpeedBonus = -2,
            Accuracy = 85, CritChance = 20, XPWeight = 5.0, Aggression = 80,
            Behavior = EnemyBehavior.Tactical, Targeting = EnemyTargetPattern.BiggestThreat,
            FleeBelowHealth = 0,
            OnHitEffectChance = 30, OnHitEffect = level => new BurnEffect(DotDamage(level), 3) // Fire breath
        };
    }
}
