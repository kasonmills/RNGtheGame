namespace GameLogic.Entities.Enemies.EnemyTypes
{
    /// <summary>
    /// Goblin - weak, quick and cowardly: always tries to flee below 25% health. Targets at
    /// random. Numbers in EnemyRoster.Goblin (enemy_roster.md, retuned 2026-11-24 - was
    /// 10 + 5 per level health, tuned for the old 24 HP player).
    /// </summary>
    public class Goblin : StandardEnemy
    {
        public Goblin(int level) : base(EnemyRoster.Goblin, level)
        {
        }
    }
}
