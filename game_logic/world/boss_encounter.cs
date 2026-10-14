using System;
using System.Collections.Generic;
using System.Linq;
using GameLogic.Entities.Player;
using GameLogic.Entities.Enemies.Bosses;
using GameLogic.Combat;
using GameLogic.Systems;

namespace GameLogic.World
{
    /// <summary>
    /// Handles boss encounter logic and player interaction
    /// Provides methods to start boss fights with proper context
    /// </summary>
    public static class BossEncounter
    {
        /// <summary>
        /// Check whether a boss fight can start: the boss exists, is unlocked (every earlier
        /// boss beaten), and hasn't been beaten already (no rematches in the MVP - decided
        /// 2026-10-14). Returns false with the reason to show the player.
        /// </summary>
        public static bool CanStartBossEncounter(BossEnemy boss, BossManager bossManager, out string reason)
        {
            reason = null;

            if (boss == null)
            {
                reason = "Error: Boss not found!";
                return false;
            }

            if (!bossManager.IsBossUnlocked(boss.BossId))
            {
                var nextBoss = bossManager.GetNextBoss();
                reason = $"This boss is not accessible yet. Defeat {(nextBoss != null ? nextBoss.Name : "the previous boss")} first.";
                return false;
            }

            // MVP: no rematches against a beaten boss (decided 2026-10-14 - rematches and their
            // strength multipliers are kept in the code for a possible update/DLC)
            if (bossManager.IsBossDefeated(boss.BossId))
            {
                reason = $"{boss.Name} has already been defeated.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Start a boss encounter with a specific boss. Returns false (and starts nothing) if
        /// CanStartBossEncounter() says the fight can't happen.
        /// TODO-GODOT: no longer blocks on a Yes/No confirmation - a Godot confirmation
        /// dialog (if any) should happen before this is called at all.
        /// </summary>
        /// <param name="player">The player character</param>
        /// <param name="boss">The boss to fight</param>
        /// <param name="bossManager">The boss manager tracking progression</param>
        /// <param name="combatManager">The combat manager</param>
        /// <param name="companions">The party's companions</param>
        public static bool StartBossEncounter(
            Player player,
            BossEnemy boss,
            BossManager bossManager,
            CombatManager combatManager,
            List<Entities.Entity> companions = null)
        {
            if (!CanStartBossEncounter(boss, bossManager, out string reason))
            {
                Console.WriteLine(reason);
                return false;
            }

            // TODO-GODOT: boss intro (name, title, description) -> boss intro screen
            Console.WriteLine(boss.GetBossInfo());

            // MVP: the repeat-fight warning is switched off along with rematches. Kept for a
            // possible update/DLC:
            // if (bossManager.IsBossDefeated(boss.BossId))
            // {
            //     Console.WriteLine("\n⚠️  WARNING: You have already defeated this boss!");
            //     Console.WriteLine("Repeat fights are MUCH harder!");
            //     Console.WriteLine($"This boss has been defeated {boss.TimesDefeated} time{(boss.TimesDefeated > 1 ? "s" : "")} before.");
            // }

            Console.WriteLine("\n⚔️  THE BATTLE BEGINS! ⚔️");

            combatManager.StartCombat(player, new List<Entities.Enemies.EnemyBase> { boss }, companions, bossManager);
            return true;
        }

        /// <summary>
        /// Start the final boss encounter. Accessible once every prior boss in the story
        /// order has been defeated (same order-based unlock as any other boss).
        /// </summary>
        /// <param name="player">The player character</param>
        /// <param name="bossManager">The boss manager</param>
        /// <param name="combatManager">The combat manager</param>
        /// <param name="companions">List of active companions</param>
        /// <summary>
        /// TODO-GODOT: no longer blocks on a Yes/No confirmation - a Godot confirmation
        /// dialog (if any) should happen before this is called at all.
        /// Returns false (and starts nothing) if the final battle isn't available.
        /// </summary>
        public static bool StartFinalBossEncounter(
            Player player,
            BossManager bossManager,
            CombatManager combatManager,
            List<Entities.Entity> companions = null)
        {
            var finalBoss = bossManager.GetFinalBoss();

            if (!CanStartBossEncounter(finalBoss, bossManager, out string reason))
            {
                Console.WriteLine(reason);
                return false;
            }

            // Display epic final boss screen
            // TODO-GODOT: final battle intro -> its own scene/cutscene
            Console.WriteLine("═══════════════════════════════════════════════════");
            Console.WriteLine("           🔥 THE FINAL BATTLE 🔥");
            Console.WriteLine("═══════════════════════════════════════════════════");
            Console.WriteLine();
            Console.WriteLine("Every other threat has fallen before you...");
            Console.WriteLine();
            Console.WriteLine("Before you stands the final challenge...");
            Console.WriteLine();
            Console.WriteLine(finalBoss.GetBossInfo());
            Console.WriteLine();
            Console.WriteLine("═══════════════════════════════════════════════════");
            Console.WriteLine("This is the final challenge.");
            Console.WriteLine("Victory here will prove you are the realm's greatest warrior.");
            Console.WriteLine("═══════════════════════════════════════════════════");
            Console.WriteLine();

            // Start final combat
            Console.WriteLine("\n⚔️  THE ULTIMATE BATTLE BEGINS! ⚔️");

            void OnCombatFinished(CombatResult result)
            {
                combatManager.CombatFinished -= OnCombatFinished;

                if (result.Outcome == CombatOutcome.Won)
                {
                    DisplayFinalVictoryScreen(finalBoss);
                }
            }

            combatManager.CombatFinished += OnCombatFinished;

            combatManager.StartCombat(player, new List<Entities.Enemies.EnemyBase> { finalBoss }, companions, bossManager);
            return true;
        }

        /// <summary>
        /// Display the final victory screen after defeating the final boss
        /// </summary>
        private static void DisplayFinalVictoryScreen(BossEnemy finalBoss)
        {
            // TODO-GODOT: final victory -> ending cutscene/credits state (to-do 16 under core/)
            Console.WriteLine("\n\n");
            Console.WriteLine("═══════════════════════════════════════════════════");
            Console.WriteLine("═══════════════════════════════════════════════════");
            Console.WriteLine("          🏆 ULTIMATE VICTORY! 🏆");
            Console.WriteLine("═══════════════════════════════════════════════════");
            Console.WriteLine("═══════════════════════════════════════════════════");
            Console.WriteLine();
            Console.WriteLine($"You have defeated {finalBoss.Name}!");
            Console.WriteLine();
            Console.WriteLine("Through skill, determination, and the favor of RNG,");
            Console.WriteLine("you have proven yourself the greatest warrior!");
            Console.WriteLine();
            Console.WriteLine("The realm is saved. The legend is complete.");
            Console.WriteLine();
            Console.WriteLine("═══════════════════════════════════════════════════");
            Console.WriteLine("           🎉 CONGRATULATIONS! 🎉");
            Console.WriteLine("═══════════════════════════════════════════════════");
            Console.WriteLine();
        }

        /// <summary>
        /// Display list of available bosses for selection
        /// </summary>
        /// <param name="bossManager">The boss manager</param>
        /// <returns>List of all bosses</returns>
        public static List<BossEnemy> GetAvailableBosses(BossManager bossManager)
        {
            return bossManager.AllBosses.Values.ToList();
        }

        /// <summary>
        /// Display boss selection menu
        /// </summary>
        /// <param name="bossManager">The boss manager</param>
        /// <param name="showDefeated">Whether to show defeated bosses differently</param>
        public static void DisplayBossSelectionMenu(BossManager bossManager, bool showDefeated = true)
        {
            var bosses = GetAvailableBosses(bossManager);

            Console.WriteLine("\n═══ BOSSES ═══");
            Console.WriteLine($"Bosses Defeated: {bossManager.BossesDefeated}/{bossManager.TotalBosses}");
            Console.WriteLine();

            int index = 1;
            foreach (var boss in bosses)
            {
                bool isDefeated = bossManager.IsBossDefeated(boss.BossId);
                bool isUnlocked = bossManager.IsBossUnlocked(boss.BossId);
                string defeatedMark = isDefeated ? " [✓ DEFEATED]" : (isUnlocked ? " [NEW]" : " [🔒 LOCKED]");
                string repeatCount = isDefeated && boss.TimesDefeated > 0 ? $" (x{boss.TimesDefeated})" : "";

                Console.WriteLine($"[{index}] {boss.Name}{defeatedMark}{repeatCount}");
                Console.WriteLine($"    Level {boss.Level} | {boss.MechanicType}");

                if (isDefeated && showDefeated)
                {
                    Console.WriteLine($"    ⚠️  Repeat fight - Boss will be {(1 + boss.TimesDefeated * 0.5):P0} stronger!");
                }

                Console.WriteLine();
                index++;
            }
        }
    }
}