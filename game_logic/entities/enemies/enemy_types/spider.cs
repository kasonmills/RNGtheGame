namespace GameLogic.Entities.Enemies.EnemyTypes
{
    /// <summary>
    /// Spider - goes after the party member with the weakest defense; 20% of its bites poison.
    /// May try to flee below 20% health.
    /// Numbers in EnemyRoster.Spider (enemy_roster.md, 2026-11-24).
    /// </summary>
    public class Spider : StandardEnemy
    {
        public Spider(int level) : base(EnemyRoster.Spider, level)
        {
        }
    }
}
