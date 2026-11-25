namespace GameLogic.Entities.Enemies.EnemyTypes
{
    /// <summary>
    /// Castle Guard - one enemy for two jobs (decided 2026-11-23): a regular enemy in and around
    /// the palace, and the final boss's summon (two at a time, only when none of hers are alive -
    /// FinalBossMechanic), at the party's level with normal rewards (farming them is accepted).
    /// It is the standard enemy (enemy_roster.md, retuned 2026-11-24 - its damage rose from
    /// 2 + level/3 to 5 + 2*level/3 to the standard's). Goes after whoever last hit it; may try to
    /// flee below 20% health, but never in the final fight (nothing flees in a boss fight).
    /// </summary>
    public class CastleGuard : StandardEnemy
    {
        public CastleGuard(int level) : base(EnemyRoster.CastleGuard, level)
        {
        }
    }
}
