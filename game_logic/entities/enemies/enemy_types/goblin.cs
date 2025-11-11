using System;

namespace GameLogic.Entities.Enemies.EnemyTypes
{
    /// <summary>
    /// Goblin - Common weak enemy
    /// Low health, low damage, but decent accuracy
    /// </summary>
    public class Goblin : EnemyBase
    {
        public Goblin(int level)
        {
            Name = "Goblin";
            Level = level;
            Type = EnemyType.Common;
            Behavior = EnemyBehavior.Aggressive;
            
            InitializeStats(level);
        }

        public override void InitializeStats(int level)
        {
            // Health: Goblins are fragile
            MaxHealth = 10 + (level * 5);
            Health = MaxHealth;

            // Damage: Low but consistent
            MinDamage = 1 + (level * 1);
            MaxDamage = 3 + (level * 2);

            // Accuracy: Decent aim
            Accuracy = 75;

            // Critical: Low crit chance
            CritChance = 5;

            // Defense: Weak armor
            Defense = level / 2;

            // Rewards: Basic
            XPValue = level * 40;
            GoldValue = level * 8;
        }

        public override EnemyAction DecideAction()
        {
            // Goblins are cowardly - flee if low on health
            if (Health < MaxHealth * 0.25) // Below 25% health
            {
                Random rand = new Random();
                if (rand.Next(1, 101) <= 30) // 30% chance to flee
                {
                    return EnemyAction.Flee;
                }
            }

            // Otherwise, always attack
            return EnemyAction.Attack;
        }

        public override string GetDescription()
        {
            return $"{Name} - A weak but numerous creature (Level {Level})";
        }
    }
}