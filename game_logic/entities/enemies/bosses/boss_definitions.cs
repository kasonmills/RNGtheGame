using System;
using System.Collections.Generic;

namespace GameLogic.Entities.Enemies.Bosses
{
    /// <summary>
    /// Defines the bosses in the game, in fixed story order.
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
        /// Create and return the bosses, in fixed story order. Order matters: BossManager
        /// derives its order-based unlock progression (defeat boss N-1 to unlock boss N) and
        /// its final-boss identity (always the last entry) directly from this list's order.
        /// TODO: only boss #1 (the tutorial fight) exists so far - the other 8 are still
        /// pending design (see docs/boss_ideas_archive.md for the retired 15-boss list this replaces).
        /// </summary>
        public static List<BossEnemy> GetAllBosses()
        {
            return new List<BossEnemy>
            {
                // Boss #1 - the tutorial fight (see TutorialManager, game_logic/core/tutorial_manager.cs).
                // Level/mechanic are a first-pass balance choice - easy to retune after playtesting.
                // MechanicType is flavor/display text only - Mechanic is the real implementation
                // (see EagleBearMechanic, game_logic/entities/enemies/bosses/boss_mechanic.cs).
                new BossEnemy(
                    bossId: "skarn_eagle_bear",
                    name: "Skarn",
                    title: "the Eagle Bear",
                    description: "A monstrous fusion of eagle and bear - taloned wings and a crushing, "
                        + "furred bulk - that struck from the treeline while the princess's escort was ambushed.",
                    level: 2,
                    mechanicType: BossMechanicType.MultiPhase)
                {
                    Mechanic = new EagleBearMechanic()
                }
            };
        }

        /// <summary>
        /// Get a specific boss by ID
        /// </summary>
        public static BossEnemy GetBoss(string bossId)
        {
            var bosses = GetAllBosses();
            return bosses.Find(b => b.BossId == bossId);
        }

        /// <summary>
        /// Get boss names and levels for display
        /// </summary>
        public static string GetBossListSummary()
        {
            var bosses = GetAllBosses();
            string summary = "═══ BOSSES ═══\n\n";

            foreach (var boss in bosses)
            {
                summary += $"• {boss.Name} (Lv.{boss.Level}) - {boss.MechanicType}\n";
            }

            return summary;
        }
    }
}
