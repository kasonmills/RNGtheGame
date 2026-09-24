using System;
using System.Collections.Generic;

namespace GameLogic.Entities.Enemies.Bosses
{
    /// <summary>
    /// Defines the champion bosses in the game.
    /// The original 15-boss roster (procedural-map era) was pulled from here on 2026-09-11
    /// after the scope was reduced to a fixed, story-driven map with specific spawn locations.
    /// That original roster is archived at docs/boss_ideas_archive.md for inspiration - none
    /// of it is wired into the active game right now.
    /// The confirmed roster is 9 bosses total; only boss #1 (the tutorial fight) exists below
    /// so far - the other 8 still need to be designed and added here.
    /// </summary>
    public static class BossDefinitions
    {
        /// <summary>
        /// Create and return the champion bosses.
        /// TODO: only boss #1 (the tutorial fight) exists so far - the other 8 are still
        /// pending design (see docs/boss_ideas_archive.md for the retired 15-boss list this replaces).
        /// </summary>
        public static List<BossEnemy> GetAllChampionBosses()
        {
            return new List<BossEnemy>
            {
                // Boss #1 - the tutorial fight (see TutorialManager, game_logic/core/tutorial_manager.cs).
                // Level/mechanic are a first-pass balance choice - easy to retune after playtesting.
                new BossEnemy(
                    bossId: "skarn_eagle_bear",
                    name: "Skarn",
                    title: "the Eagle Bear",
                    description: "A monstrous fusion of eagle and bear - taloned wings and a crushing, "
                        + "furred bulk - that struck from the treeline while the princess's escort was ambushed.",
                    keyId: "skarn_key",
                    level: 2,
                    mechanicType: BossMechanicType.Standard)
            };
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
