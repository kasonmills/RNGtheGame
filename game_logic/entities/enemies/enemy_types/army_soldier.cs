namespace GameLogic.Entities.Enemies.EnemyTypes
{
    /// <summary>
    /// Army Soldier - a loyal soldier of the player's army, fought only as a sparring opponent at
    /// the army camp (never a random encounter). The Deserter's numbers, but it never flees.
    /// Its rewards come from the camp's exit reward, not normal loot - wired with the army camp.
    /// Numbers in EnemyRoster.ArmySoldier (enemy_roster.md, 2026-11-24).
    /// </summary>
    public class ArmySoldier : StandardEnemy
    {
        public ArmySoldier(int level) : base(EnemyRoster.ArmySoldier, level)
        {
        }
    }
}
