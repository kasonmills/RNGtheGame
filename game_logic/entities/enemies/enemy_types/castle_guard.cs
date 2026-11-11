namespace GameLogic.Entities.Enemies.EnemyTypes
{
    /// <summary>
    /// Castle Guard - called into the final fight two at a time by the final boss (only when none
    /// of hers are alive - FinalBossMechanic). A sturdy regular enemy at the party's level (the
    /// boss's level - 5), with normal rewards - farming them is accepted (decided 2026-10-14).
    /// Stats are my defaults (2026-11-11), not decided.
    /// </summary>
    public class CastleGuard : EnemyBase
    {
        public CastleGuard(int level)
        {
            Name = "Castle Guard";
            Level = System.Math.Max(1, level);
            Type = EnemyType.Common;
            Behavior = EnemyBehavior.Aggressive;

            InitializeStats(Level);
        }

        public override void InitializeStats(int level)
        {
            Level = System.Math.Max(1, level);

            // Health: sturdier than a dragling (level 40: 260)
            MaxHealth = 20 + (Level * 6);
            Health = MaxHealth;

            // Damage (level 40: 15-31)
            MinDamage = 2 + (Level / 3);
            MaxDamage = 5 + (Level * 2 / 3);

            Accuracy = 75;
            CritChance = 5;
            Defense = Level / 4;
            AggressionLevel = 50;

            // Steady, not fast (level 40: 20)
            Speed = 10 + (Level / 4);

            XPValue = Level * 30;
            GoldValue = Level * 6;
        }

        public override EnemyAction DecideAction()
        {
            return EnemyAction.Attack;
        }

        public override string GetDescription()
        {
            return $"{Name} - A guard of the royal castle, armored and loyal (Level {Level})";
        }
    }
}
