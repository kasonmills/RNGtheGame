namespace GameLogic.Entities.Enemies.EnemyTypes
{
    /// <summary>
    /// Wolf - fast pack hunters (groups of 2-5) that go after the most wounded. 15% of their
    /// bites cause Bleeding. May try to flee below 20% health. Mostly at night.
    /// Numbers in EnemyRoster.Wolf (enemy_roster.md, 2026-11-24).
    /// </summary>
    public class Wolf : StandardEnemy
    {
        public Wolf(int level) : base(EnemyRoster.Wolf, level)
        {
        }
    }
}
