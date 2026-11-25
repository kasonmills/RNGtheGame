namespace GameLogic.Entities.Enemies.EnemyTypes
{
    /// <summary>
    /// Slime - lots of health, slow and weak-hitting; 20% of its hits slow the target (-2 speed
    /// for 2 turns). Mindless - never flees, targets at random. Slimes share one
    /// diminishing-rewards group, so a Big Slime's split can't be farmed.
    /// Numbers in EnemyRoster.Slime (enemy_roster.md, 2026-11-24).
    /// </summary>
    public class Slime : StandardEnemy
    {
        public Slime(int level) : base(EnemyRoster.Slime, level)
        {
        }
    }
}
