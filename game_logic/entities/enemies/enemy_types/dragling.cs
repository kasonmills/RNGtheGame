namespace GameLogic.Entities.Enemies.EnemyTypes
{
    /// <summary>
    /// Dragling - a small slime spawn of the Slime Dragon (boss #7). Only appears in his fight:
    /// one for every hit he takes (at most 6 alive), and he tops them back up to 3 at the end of
    /// a round (SlimeDragonMechanic). Weak and fast - a few hits to bring down.
    ///
    /// Fights at the party's level (the dragon's level - 5), not the dragon's. Farming them is
    /// allowed, with diminishing rewards after 10 in one fight (DiminishingRewardsGroup, decided
    /// 2026-11-10). Stats are my defaults (2026-11-11), kept as built by the roster review
    /// (2026-11-24).
    ///
    /// Also a wild overworld enemy (decided 2026-11-23), in slime groups: a wild one may try to
    /// flee below 30% health. The Slime Dragon's own spawns never do - nothing flees in a boss
    /// fight (CombatManager). Targets at random.
    /// </summary>
    public class Dragling : EnemyBase
    {
        public const string RewardsGroup = "Dragling";

        public Dragling(int level)
        {
            Name = "Dragling";
            Level = System.Math.Max(1, level);
            Type = EnemyType.Common;
            Behavior = EnemyBehavior.Aggressive;
            DiminishingRewardsGroup = RewardsGroup;
            Family = EnemyRoster.Slimes;
            Targeting = EnemyTargetPattern.Random;
            FleeBelowHealth = 0.30;

            InitializeStats(Level);
        }

        public override void InitializeStats(int level)
        {
            Level = System.Math.Max(1, level);

            // Health: a few of the party's hits (level 21: 120)
            MaxHealth = 15 + (Level * 5);
            Health = MaxHealth;

            // Damage: pressure on top of the dragon's big hits (level 21: 8-17)
            MinDamage = 1 + (Level / 3);
            MaxDamage = 3 + (Level * 2 / 3);

            Accuracy = 70;
            CritChance = 5;
            Defense = Level / 5;
            AggressionLevel = 60;

            // Fast (level 21: 21)
            Speed = 14 + (Level / 3);

            XPValue = Level * 20;
            GoldValue = Level * 4;
        }

        public override string GetDescription()
        {
            return $"{Name} - A small, quick slime spawned by the Slime Dragon (Level {Level})";
        }
    }
}
