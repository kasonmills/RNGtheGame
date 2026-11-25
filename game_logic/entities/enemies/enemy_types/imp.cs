namespace GameLogic.Entities.Enemies.EnemyTypes
{
    /// <summary>
    /// Imp - a lesser demon: fragile, fast, fights from the back row (once rows are built).
    /// Always tries to flee below 25% health. 20% of its hits add a small condition - only a
    /// weaker Burning for now; Hexed and Weakened join it when the Demon Prince's new conditions
    /// are built. Numbers in EnemyRoster.Imp (enemy_roster.md, 2026-11-24).
    /// </summary>
    public class Imp : StandardEnemy
    {
        public Imp(int level) : base(EnemyRoster.Imp, level)
        {
        }
    }
}
