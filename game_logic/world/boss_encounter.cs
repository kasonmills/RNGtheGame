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
        /// Start a boss encounter with a specific boss
        /// </summary>
        /// <param name="player">The player character</param>
        /// <param name="boss">The boss to fight</param>
        /// <param name="bossManager">The boss manager tracking progression</param>
        /// <param name="combatManager">The combat manager</param>
        /// <param name="companions">List of active companions</param>
        /// <returns>True if player won, False if player lost or fled</returns>
        /// <summary>
        /// TODO-GODOT: no longer blocks on a Yes/No confirmation - a Godot confirmation
        /// dialog (if any) should happen before this is called at all.
        /// </summary>
        public static void StartBossEncounter(
            Player player,
            BossEnemy boss,
            BossManager bossManager,
            CombatManager combatManager,
            List<Entities.NPCs.Companions.CompanionBase> companions = null)
        {
            if (boss == null)
            {
                Console.WriteLine("Error: Boss not found!");
                return;
            }

            if (!bossManager.IsBossUnlocked(boss.BossId))
            {
                var nextBoss = bossManager.GetNextBoss();
                Console.WriteLine($"This boss is not accessible yet. Defeat {(nextBoss != null ? nextBoss.Name : "the previous boss")} first.");
                return;
            }

            // Display boss encounter screen
            Console.Clear();
            Console.WriteLine(boss.GetBossInfo());

            // Check if boss has been defeated before
            if (bossManager.IsBossDefeated(boss.BossId))
            {
                Console.WriteLine("\n⚠️  WARNING: You have already defeated this boss!");
                Console.WriteLine("Repeat fights are MUCH harder!");
                Console.WriteLine($"This boss has been defeated {boss.TimesDefeated} time{(boss.TimesDefeated > 1 ? "s" : "")} before.");
            }
            else
            {
                Console.WriteLine("\n🏆 First-time encounter!");
            }

            Console.WriteLine("\n⚔️  THE BATTLE BEGINS! ⚔️");

            // Convert companions to Entity list for combat manager
            var companionEntities = companions?.Cast<Entities.Entity>().ToList();
            combatManager.StartCombat(player, new List<Entities.Enemies.EnemyBase> { boss }, companionEntities, bossManager);
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
        /// </summary>
        public static void StartFinalBossEncounter(
            Player player,
            BossManager bossManager,
            CombatManager combatManager,
            List<Entities.NPCs.Companions.CompanionBase> companions = null)
        {
            var finalBoss = bossManager.GetFinalBoss();

            if (finalBoss == null)
            {
                Console.WriteLine("Error: Final boss not found!");
                return;
            }

            if (!bossManager.IsBossUnlocked(finalBoss.BossId))
            {
                var nextBoss = bossManager.GetNextBoss();
                Console.WriteLine($"The final battle is not accessible yet. Defeat {(nextBoss != null ? nextBoss.Name : "the remaining bosses")} first.");
                return;
            }

            // Display epic final boss screen
            Console.Clear();
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

            void OnCombatEnded(bool victory)
            {
                combatManager.CombatEnded -= OnCombatEnded;

                if (victory)
                {
                    DisplayFinalVictoryScreen(finalBoss);
                }
            }

            combatManager.CombatEnded += OnCombatEnded;

            // Convert companions to Entity list for combat manager
            var companionEntities = companions?.Cast<Entities.Entity>().ToList();
            combatManager.StartCombat(player, new List<Entities.Enemies.EnemyBase> { finalBoss }, companionEntities, bossManager);
        }

        /// <summary>
        /// Display the final victory screen after defeating the final boss
        /// </summary>
        private static void DisplayFinalVictoryScreen(BossEnemy finalBoss)
        {
            Console.Clear();
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
            Console.WriteLine("Press any key to continue your journey...");
            Console.ReadKey();
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