namespace GameLogic.Entities.Enemies.EnemyTypes
{
    /// <summary>
    /// Kingdom B Archer - joins Kingdom B soldier groups. High accuracy (90%); goes after the
    /// weakest defense; ranged, so it fights from the back row with no penalty (once rows are
    /// built). May try to flee below 30%. ("Kingdom B" is a placeholder name.)
    /// Numbers in EnemyRoster.KingdomBArcher (enemy_roster.md, 2026-11-24).
    /// </summary>
    public class KingdomBArcher : StandardEnemy
    {
        public KingdomBArcher(int level) : base(EnemyRoster.KingdomBArcher, level)
        {
        }
    }
}
