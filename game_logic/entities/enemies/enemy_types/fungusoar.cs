namespace GameLogic.Entities.Enemies.EnemyTypes
{
    /// <summary>
    /// Fungusoar - a mushroom dinosaur (the user's idea, 2026-11-23 - not humanoid mushroom
    /// folk). Big and hard-hitting, goes after the biggest threat, never flees. Spore Roar: 25%
    /// of its hits poison. (Mushroom components dropping at 2x weight comes with the component
    /// drops item.) Numbers in EnemyRoster.Fungusoar (enemy_roster.md, 2026-11-24).
    /// </summary>
    public class Fungusoar : StandardEnemy
    {
        public Fungusoar(int level) : base(EnemyRoster.Fungusoar, level)
        {
        }
    }
}
