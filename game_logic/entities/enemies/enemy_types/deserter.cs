namespace GameLogic.Entities.Enemies.EnemyTypes
{
    /// <summary>
    /// Deserter - a soldier of the player's own army who ran from the war, hostile and roaming
    /// the routes (loyal soldiers are never random encounters - decided 2026-11-23). Sturdy;
    /// defends more when wounded; goes after the biggest threat; may try to flee below 30%.
    /// Numbers in EnemyRoster.Deserter (enemy_roster.md, 2026-11-24).
    /// </summary>
    public class Deserter : StandardEnemy
    {
        public Deserter(int level) : base(EnemyRoster.Deserter, level)
        {
        }
    }
}
