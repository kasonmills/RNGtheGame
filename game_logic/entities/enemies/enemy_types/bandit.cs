using System;

namespace GameLogic.Entities.Enemies.EnemyTypes
{
    /// <summary>
    /// Bandit - Elite/Common enemy that's cunning and unpredictable
    /// Moderate health, good damage, high crit chance (sneaky attacks)
    /// May steal gold or flee when outmatched
    /// </summary>
    public class Bandit : EnemyBase
    {
        public Bandit(int level)
        {
            Name = "Bandit";
            Level = level;
            Type = EnemyType.Elite;
            Behavior = EnemyBehavior.Tactical;

            InitializeStats(level);
        }

        public override void InitializeStats(int level)
        {
            // Health: More durable than goblins but less than dragons
            MaxHealth = 25 + (level * 10);
            Health = MaxHealth;

            // Damage: Moderate but relies on critical hits (sneak attacks)
            MinDamage = 3 + (level * 2);
            MaxDamage = 8 + (level * 3);

            // Accuracy: Good aim from training
            Accuracy = 80;

            // Critical: High crit chance (backstabs and sneak attacks)
            CritChance = 25;

            // Defense: Light armor for mobility
            Defense = 1 + level;

            // Rewards: Bandits carry stolen goods
            XPValue = level * 75;
            GoldValue = level * 20; // Higher gold than goblins
        }

        public override EnemyAction DecideAction()
        {
            Random rand = new Random();

            // Bandits are tactical and opportunistic

            // Below 40% health - consider fleeing (self-preservation)
            if (Health < MaxHealth * 0.4)
            {
                // 25% chance to flee when low on health
                if (rand.Next(1, 101) <= 25)
                {
                    return EnemyAction.Flee;
                }
                // 20% chance to defend and recover
                else if (rand.Next(1, 101) <= 20)
                {
                    return EnemyAction.Defend;
                }
            }
            // Above 40% health - aggressive and tactical
            else
            {
                // 15% chance to use special ability (poison dagger, smoke bomb, etc.)
                if (rand.Next(1, 101) <= 15)
                {
                    return EnemyAction.UseAbility;
                }
                // 10% chance to defend (feint/parry)
                else if (rand.Next(1, 101) <= 10)
                {
                    return EnemyAction.Defend;
                }
            }

            // Default: Attack (bandits are aggressive for loot)
            return EnemyAction.Attack;
        }

        public override string GetDescription()
        {
            return $"{Name} - A cunning thief with deadly precision (Level {Level})";
        }
    }
}
