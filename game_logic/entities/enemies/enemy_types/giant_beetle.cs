namespace GameLogic.Entities.Enemies.EnemyTypes
{
    /// <summary>
    /// Giant Beetle - very high defense (2.5x the standard), so piercing bolts and spells are the
    /// answer. Goes after whoever last hit it. Never flees.
    /// Numbers in EnemyRoster.GiantBeetle (enemy_roster.md, 2026-11-24).
    /// </summary>
    public class GiantBeetle : StandardEnemy
    {
        public GiantBeetle(int level) : base(EnemyRoster.GiantBeetle, level)
        {
        }
    }
}
