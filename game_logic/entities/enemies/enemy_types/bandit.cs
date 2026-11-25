namespace GameLogic.Entities.Enemies.EnemyTypes
{
    /// <summary>
    /// Bandit - the standard enemy with a high crit chance (sneak attacks). Goes after the most
    /// wounded, and may try to flee below 40% health. Found all game. Numbers in
    /// EnemyRoster.Bandit (enemy_roster.md, retuned 2026-11-24).
    /// </summary>
    public class Bandit : StandardEnemy
    {
        public Bandit(int level) : base(EnemyRoster.Bandit, level)
        {
        }
    }
}
