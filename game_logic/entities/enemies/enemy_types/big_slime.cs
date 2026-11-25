using System.Collections.Generic;

namespace GameLogic.Entities.Enemies.EnemyTypes
{
    /// <summary>
    /// Big Slime - one per slime group, with a huge pool of health. When it's beaten it splits
    /// into 2 Slimes at its level (decided 2026-11-23). They join like a summon: limited by the
    /// 8-enemy cap, and acting from the next round. Never flees.
    /// Numbers in EnemyRoster.BigSlime (enemy_roster.md, 2026-11-24).
    /// </summary>
    public class BigSlime : StandardEnemy
    {
        public const int SplitCount = 2;

        public BigSlime(int level) : base(EnemyRoster.BigSlime, level)
        {
        }

        public override List<EnemyBase> GetSpawnsOnDefeat()
        {
            var spawns = new List<EnemyBase>();
            for (int i = 0; i < SplitCount; i++)
            {
                spawns.Add(new Slime(Level));
            }
            return spawns;
        }
    }
}
