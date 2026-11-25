namespace GameLogic.Entities.Enemies.EnemyTypes
{
    /// <summary>
    /// Kingdom B Soldier - a soldier of the enemy kingdom ("Kingdom B" is a placeholder until
    /// the kingdom has a name - the class and type name change with it). Tough armor (2x
    /// defense); defends more when wounded; goes after the biggest threat; may try to flee below
    /// 20%. Numbers in EnemyRoster.KingdomBSoldier (enemy_roster.md, 2026-11-24).
    /// </summary>
    public class KingdomBSoldier : StandardEnemy
    {
        public KingdomBSoldier(int level) : base(EnemyRoster.KingdomBSoldier, level)
        {
        }
    }
}
