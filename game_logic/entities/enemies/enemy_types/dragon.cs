using System;

namespace GameLogic.Entities.Enemies.EnemyTypes
{
    /// <summary>
    /// Dragon - Powerful boss-type enemy
    /// Extremely high health, massive damage, and strong defense
    /// Can use special fire breath ability
    /// </summary>
    public class Dragon : EnemyBase
    {
        public Dragon(int level)
        {
            Name = "Dragon";
            Level = level;
            Type = EnemyType.Boss;
            Behavior = EnemyBehavior.Tactical;

            InitializeStats(level);
        }

        public override void InitializeStats(int level)
        {
            // Health: Dragons are extremely tanky
            MaxHealth = 100 + (level * 25);
            Health = MaxHealth;

            // Damage: Devastating attacks
            MinDamage = 10 + (level * 3);
            MaxDamage = 20 + (level * 6);

            // Accuracy: Dragons rarely miss
            Accuracy = 85;

            // Critical: High crit chance due to powerful claws/fangs
            CritChance = 20;

            // Defense: Tough scales provide excellent armor
            Defense = 5 + (level * 2);

            // Speed: Large but not slow
            Speed = 8 + (level / 3); // Dragons are slower than smaller enemies but still formidable

            // Rewards: Dragons are valuable opponents
            XPValue = level * 200;
            GoldValue = level * 50;
        }

        public override EnemyAction DecideAction()
        {
            Random rand = new Random();

            // Dragons use tactical AI

            // Below 30% health - becomes more aggressive/desperate
            if (Health < MaxHealth * 0.3)
            {
                // 40% chance to use special ability when low on health
                if (rand.Next(1, 101) <= 40)
                {
                    return EnemyAction.UseAbility; // Fire breath or other special attack
                }
            }
            // Above 70% health - confident and balanced
            else if (Health > MaxHealth * 0.7)
            {
                // 20% chance to defend (conserve energy)
                if (rand.Next(1, 101) <= 20)
                {
                    return EnemyAction.Defend;
                }
            }
            // Between 30-70% health - tactical mix
            else
            {
                // 30% chance to use ability
                if (rand.Next(1, 101) <= 30)
                {
                    return EnemyAction.UseAbility;
                }
                // 10% chance to defend
                else if (rand.Next(1, 101) <= 10)
                {
                    return EnemyAction.Defend;
                }
            }

            // Default: Attack
            return EnemyAction.Attack;
        }

        public override string GetDescription()
        {
            return $"{Name} - A legendary beast of immense power! (Level {Level})";
        }
    }
}