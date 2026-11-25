namespace GameLogic.Entities.Enemies.EnemyTypes
{
    /// <summary>
    /// Dragon - a rare late-game enemy, always alone, found only in one or two mountain areas
    /// (decided 2026-11-23 - not a random 5% roll once the spawn lists are built). Huge health,
    /// damage and defense; goes after the biggest threat; never flees. Fire breath: 30% of its
    /// hits burn. Numbers in EnemyRoster.Dragon (enemy_roster.md, retuned 2026-11-24).
    /// </summary>
    public class Dragon : StandardEnemy
    {
        public Dragon(int level) : base(EnemyRoster.Dragon, level)
        {
        }
    }
}
