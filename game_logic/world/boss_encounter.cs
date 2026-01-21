using System;
using System.Collections.Generic;
using System.Linq;
using GameLogic.Entities.Player;
using GameLogic.Entities.Enemies;
using GameLogic.Combat;
using GameLogic.Progression;
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
        public static bool StartBossEncounter(
            Player player,
            BossEnemy boss,
            BossManager bossManager,
            CombatManager combatManager,
            List<Entities.NPCs.Companions.CompanionBase> companions = null)
        {
            if (boss == null)
            {
                Console.WriteLine("Error: Boss not found!");
                return false;
            }

            // Display boss encounter screen
            Console.Clear();
            Console.WriteLine(boss.GetBossInfo());

            // Check if boss has been defeated before
            if (bossManager.IsBossDefeated(boss.BossId))
            {
                Console.WriteLine("\n⚠️  WARNING: You have already defeated this boss!");
                Console.WriteLine("Repeat fights are MUCH harder and keys are NOT guaranteed to drop!");
                Console.WriteLine($"This boss has been defeated {boss.TimesDefeated} time{(boss.TimesDefeated > 1 ? "s" : "")} before.");
            }
            else
            {
                Console.WriteLine("\n🏆 First-time encounter! Key drop is GUARANTEED on victory!");
            }

            // Confirm engagement
            Console.WriteLine("\n═══════════════════════════════════════");
            Console.WriteLine("Do you wish to challenge this Champion?");
            Console.WriteLine("[1] Yes, begin the battle!");
            Console.WriteLine("[2] No, retreat for now");
            Console.Write("\nChoice: ");

            string choice = Console.ReadLine();

            if (choice != "1")
            {
                Console.WriteLine("\nYou retreat to safety...");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return false;
            }

            // Start combat
            Console.WriteLine("\n⚔️  THE BATTLE BEGINS! ⚔️");
            Console.WriteLine("Press any key to start...");
            Console.ReadKey();

            // Convert companions to Entity list for combat manager
            var companionEntities = companions?.Cast<Entities.Entity>().ToList();
            bool victory = combatManager.StartCombat(player, boss, companionEntities, bossManager);

            return victory;
        }

        /// <summary>
        /// Check if player can access the final boss
        /// Player must have 10 unique champion keys in their inventory
        /// </summary>
        /// <param name="bossManager">The boss manager</param>
        /// <param name="playerInventory">The player's inventory</param>
        /// <returns>True if player has 10 or more unique keys</returns>
        public static bool CanAccessFinalBoss(BossManager bossManager, PlayerInventory playerInventory)
        {
            int keyCount = bossManager.CountChampionKeys(playerInventory);
            return keyCount >= BossManager.KEYS_REQUIRED;
        }

        /// <summary>
        /// Start the final boss encounter
        /// Requires 10 unique champion keys in inventory (keys will be consumed to open the gate)
        /// </summary>
        /// <param name="player">The player character</param>
        /// <param name="bossManager">The boss manager</param>
        /// <param name="combatManager">The combat manager</param>
        /// <param name="companions">List of active companions</param>
        /// <returns>True if player won, False if player lost or fled</returns>
        public static bool StartFinalBossEncounter(
            Player player,
            BossManager bossManager,
            CombatManager combatManager,
            List<Entities.NPCs.Companions.CompanionBase> companions = null)
        {
            var finalBoss = bossManager.GetFinalBoss();

            if (finalBoss == null)
            {
                Console.WriteLine("Error: Final boss not found!");
                return false;
            }

            // Check if player has enough keys
            int keyCount = bossManager.CountChampionKeys(player.Inventory);

            if (keyCount < BossManager.KEYS_REQUIRED)
            {
                Console.WriteLine("The Final Gate remains sealed!");
                Console.WriteLine($"Champion Keys required: {BossManager.KEYS_REQUIRED}");
                Console.WriteLine($"Champion Keys in inventory: {keyCount}");
                Console.WriteLine($"You need {BossManager.KEYS_REQUIRED - keyCount} more unique Champion Key{(BossManager.KEYS_REQUIRED - keyCount > 1 ? "s" : "")}!");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return false;
            }

            // Display epic final boss screen
            Console.Clear();
            Console.WriteLine("═══════════════════════════════════════════════════");
            Console.WriteLine("           🔥 THE FINAL GATE OPENS 🔥");
            Console.WriteLine("═══════════════════════════════════════════════════");
            Console.WriteLine();
            Console.WriteLine($"You possess {keyCount} Champion Keys!");
            Console.WriteLine($"The Final Gate recognizes your victories...");
            Console.WriteLine();
            Console.WriteLine($"Before you stands the ultimate Champion...");
            Console.WriteLine();
            Console.WriteLine(finalBoss.GetBossInfo());
            Console.WriteLine();
            Console.WriteLine("═══════════════════════════════════════════════════");
            Console.WriteLine("This is the final challenge.");
            Console.WriteLine("Victory here will prove you are the ultimate Champion.");
            Console.WriteLine("═══════════════════════════════════════════════════");
            Console.WriteLine();

            // Confirm engagement
            Console.WriteLine("Do you wish to enter the Final Gate?");
            Console.WriteLine($"⚠️  WARNING: This will consume {BossManager.KEYS_REQUIRED} Champion Keys!");
            Console.WriteLine("[1] Yes, face my destiny!");
            Console.WriteLine("[2] No, I need more preparation");
            Console.Write("\nChoice: ");

            string choice = Console.ReadLine();

            if (choice != "1")
            {
                Console.WriteLine("\nYou step back from the gate...");
                Console.WriteLine("The final battle can wait.");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return false;
            }

            // Consume the keys to open the gate
            Console.WriteLine("\n🔑 Using Champion Keys to unlock the Final Gate...");
            int keysConsumed = ConsumeChampionKeys(player.Inventory, bossManager, BossManager.KEYS_REQUIRED);

            if (keysConsumed < BossManager.KEYS_REQUIRED)
            {
                Console.WriteLine($"\nError: Failed to consume keys! Only {keysConsumed} keys were removed.");
                Console.WriteLine("The gate did not open. Contact the developers!");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return false;
            }

            Console.WriteLine($"✨ {BossManager.KEYS_REQUIRED} Champion Keys consumed!");

            // Unlock the final gate (this sets FinalGateUnlocked = true)
            bossManager.UnlockFinalGate();

            // Start final combat
            Console.WriteLine("\n⚔️  THE ULTIMATE BATTLE BEGINS! ⚔️");
            Console.WriteLine("Press any key to start...");
            Console.ReadKey();

            // Convert companions to Entity list for combat manager
            var companionEntities = companions?.Cast<Entities.Entity>().ToList();
            bool victory = combatManager.StartCombat(player, finalBoss, companionEntities, bossManager);

            if (victory)
            {
                // Display epic victory screen
                DisplayFinalVictoryScreen(finalBoss);
            }

            return victory;
        }

        /// <summary>
        /// Consume champion keys from player inventory
        /// </summary>
        /// <param name="inventory">Player inventory</param>
        /// <param name="bossManager">Boss manager to identify keys</param>
        /// <param name="count">Number of keys to consume</param>
        /// <returns>Number of keys actually consumed</returns>
        private static int ConsumeChampionKeys(PlayerInventory inventory, BossManager bossManager, int count)
        {
            int consumed = 0;
            var allItems = inventory.GetAllItems().ToList(); // Create a copy to avoid modification issues

            foreach (var item in allItems)
            {
                if (consumed >= count)
                    break;

                if (item is Items.QuestItem questItem && IsChampionKey(questItem, bossManager))
                {
                    inventory.RemoveItem(item);
                    Console.WriteLine($"   🔑 {questItem.Name} consumed...");
                    consumed++;
                }
            }

            return consumed;
        }

        /// <summary>
        /// Check if an item is a champion key
        /// </summary>
        private static bool IsChampionKey(Items.QuestItem item, BossManager bossManager)
        {
            foreach (var boss in bossManager.AllBosses.Values)
            {
                if (item.QuestId == boss.KeyId)
                {
                    return true;
                }
            }
            return false;
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
            Console.WriteLine("The ultimate Champion has fallen!");
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
        /// <returns>List of all champion bosses (except final boss)</returns>
        public static List<BossEnemy> GetAvailableChampionBosses(BossManager bossManager)
        {
            var allBosses = bossManager.AllBosses.Values.ToList();

            // Exclude the final boss from regular encounters
            return allBosses.Where(b => b.BossId != bossManager.FinalBossId).ToList();
        }

        /// <summary>
        /// Display boss selection menu
        /// </summary>
        /// <param name="bossManager">The boss manager</param>
        /// <param name="showDefeated">Whether to show defeated bosses differently</param>
        public static void DisplayBossSelectionMenu(BossManager bossManager, bool showDefeated = true)
        {
            var championBosses = GetAvailableChampionBosses(bossManager);

            Console.WriteLine("\n═══ AVAILABLE CHAMPIONS ═══");
            Console.WriteLine($"Bosses Defeated: {bossManager.BossesDefeated}/{BossManager.TOTAL_BOSSES - 1}"); // -1 to exclude final boss
            Console.WriteLine();

            int index = 1;
            foreach (var boss in championBosses)
            {
                bool isDefeated = bossManager.IsBossDefeated(boss.BossId);
                string defeatedMark = isDefeated ? " [✓ DEFEATED]" : " [NEW]";
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