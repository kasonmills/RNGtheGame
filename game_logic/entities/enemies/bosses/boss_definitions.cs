using System;
using System.Collections.Generic;

namespace GameLogic.Entities.Enemies.Bosses
{
    /// <summary>
    /// Defines the champion bosses in the game.
    /// The original 15-boss roster (procedural-map era) was pulled from here on 2026-09-11
    /// after the scope was reduced to 8 bosses on a fixed, story-driven map with specific
    /// spawn locations. That original roster is archived at docs/boss_ideas_archive.md for
    /// inspiration - none of it is wired into the active game right now.
    /// The new 8-boss roster still needs to be designed and added here.
    /// </summary>
    public static class BossDefinitions
    {
        /// <summary>
        /// Create and return the champion bosses.
        /// TODO: currently empty pending the new 8-boss roster design (see docs/boss_ideas_archive.md
        /// for the retired 15-boss list this replaces).
        /// </summary>
        public static List<BossEnemy> GetAllChampionBosses()
        {
            return new List<BossEnemy>();
        }

        /// <summary>
        /// Get a specific boss by ID
        /// </summary>
        public static BossEnemy GetBoss(string bossId)
        {
            var bosses = GetAllChampionBosses();
            return bosses.Find(b => b.BossId == bossId);
        }

        /// <summary>
        /// Get boss names and levels for display
        /// </summary>
        public static string GetBossListSummary()
        {
            var bosses = GetAllChampionBosses();
            string summary = "═══ CHAMPION BOSSES ═══\n";
            summary += "Defeat any 10 to unlock the Final Gate\n\n";

            foreach (var boss in bosses)
            {
                summary += $"• {boss.Name} (Lv.{boss.Level}) - {boss.MechanicType}\n";
            }

            return summary;
        }
    }
}
