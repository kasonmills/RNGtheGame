using System;
using System.Collections.Generic;
using System.Linq;
using GameLogic.Items;

namespace GameLogic.Entities.NPCs
{
    /// <summary>
    /// Blacksmith NPC - Specializes in upgrading weapons and armor
    /// Players bring items that have met their XP requirements to be upgraded for gold
    /// </summary>
    public class Blacksmith : NPCBase
    {
        // Blacksmith specialization
        public int SkillLevel { get; set; }  // Higher skill = better prices/services
        public bool CanUpgradeLegendary { get; set; }  // Some blacksmiths can't work with legendary items

        /// <summary>
        /// Constructor for blacksmith NPCs
        /// </summary>
        public Blacksmith(string name, int skillLevel = 1, string greeting = null)
            : base(name, NPCType.Blacksmith, greeting)
        {
            SkillLevel = Math.Clamp(skillLevel, 1, 10);  // Skill levels 1-10
            CanUpgradeLegendary = skillLevel >= 7;  // High-skill blacksmiths can work with legendary items

            // Set default greeting
            if (greeting == null)
            {
                Greeting = "Welcome to my forge! I can upgrade your weapons and armor.";
            }

            // Add some blacksmith dialogues
            AddDialogue("The forge burns hot today!");
            AddDialogue("Bring me your worn equipment, I'll make it shine again.");
            AddDialogue("Every weapon has a story... and every story needs a strong blade.");
            AddDialogue("Quality work takes time and coin, but it's worth it.");
            AddDialogue("I've been smithing for years. Your equipment is in good hands.");
        }

        /// <summary>
        /// Show the blacksmith's main menu
        /// </summary>
        public void ShowServices(Player.Player player)
        {
            Console.Clear();
            Console.WriteLine("=".PadRight(50, '='));
            Console.WriteLine($"    {Name}'s Forge");
            Console.WriteLine("=".PadRight(50, '='));
            Greet();
            Console.WriteLine($"\nSkill Level: {SkillLevel}/10");
            Console.WriteLine($"Can upgrade legendary items: {(CanUpgradeLegendary ? "Yes" : "No")}");
            Console.WriteLine($"\nYour Gold: {player.Gold}");

            Console.WriteLine("\nWhat would you like to do?");
            Console.WriteLine("1. Upgrade Weapon");
            Console.WriteLine("2. Upgrade Armor");
            Console.WriteLine("3. View Upgrade Prices");
            Console.WriteLine("4. Leave");

            Console.Write("\nChoice: ");
        }

        /// <summary>
        /// Get all weapons from player's inventory that are ready for upgrade
        /// </summary>
        public List<Weapon> GetUpgradableWeapons(Player.Player player)
        {
            var upgradableWeapons = new List<Weapon>();

            // Check equipped weapon
            if (player.EquippedWeapon != null && player.EquippedWeapon.ReadyForUpgrade)
            {
                upgradableWeapons.Add(player.EquippedWeapon);
            }

            // Check inventory
            foreach (var item in player.Inventory.Items)
            {
                if (item is Weapon weapon && weapon.ReadyForUpgrade)
                {
                    // Skip if it's the equipped weapon (already added)
                    if (player.EquippedWeapon != null && weapon == player.EquippedWeapon)
                        continue;

                    upgradableWeapons.Add(weapon);
                }
            }

            return upgradableWeapons;
        }

        /// <summary>
        /// Get all armor from player's inventory that are ready for upgrade
        /// </summary>
        public List<Armor> GetUpgradableArmor(Player.Player player)
        {
            var upgradableArmor = new List<Armor>();

            // Check equipped armor
            if (player.EquippedArmor != null && player.EquippedArmor.ReadyForUpgrade)
            {
                upgradableArmor.Add(player.EquippedArmor);
            }

            // Check inventory
            foreach (var item in player.Inventory.Items)
            {
                if (item is Armor armor && armor.ReadyForUpgrade)
                {
                    // Skip if it's the equipped armor (already added)
                    if (player.EquippedArmor != null && armor == player.EquippedArmor)
                        continue;

                    upgradableArmor.Add(armor);
                }
            }

            return upgradableArmor;
        }

        /// <summary>
        /// Display weapon upgrade menu and handle selection
        /// </summary>
        public void UpgradeWeaponMenu(Player.Player player)
        {
            var upgradableWeapons = GetUpgradableWeapons(player);

            if (upgradableWeapons.Count == 0)
            {
                Console.WriteLine("\n❌ You don't have any weapons ready for upgrade.");
                Console.WriteLine("Weapons need to gain experience in combat before they can be upgraded!");
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
                return;
            }

            Console.Clear();
            Console.WriteLine("=== WEAPON UPGRADES ===\n");
            Console.WriteLine($"Your Gold: {player.Gold}\n");

            for (int i = 0; i < upgradableWeapons.Count; i++)
            {
                var weapon = upgradableWeapons[i];
                int cost = CalculateUpgradeCost(weapon);
                string equipped = weapon == player.EquippedWeapon ? " [EQUIPPED]" : "";

                Console.WriteLine($"{i + 1}. {weapon.GetDisplayName()}{equipped}");
                Console.WriteLine($"   Current: +{weapon.Level} | Next: +{weapon.Level + 1}");
                Console.WriteLine($"   Upgrade Cost: {cost} gold");
                Console.WriteLine($"   Damage: {weapon.MinDamage}-{weapon.MaxDamage} → {GetPreviewDamage(weapon)}");
                Console.WriteLine();
            }

            Console.WriteLine($"{upgradableWeapons.Count + 1}. Cancel");
            Console.Write("\nSelect weapon to upgrade: ");

            string input = Console.ReadLine();
            if (int.TryParse(input, out int choice) && choice > 0 && choice <= upgradableWeapons.Count)
            {
                UpgradeWeapon(player, upgradableWeapons[choice - 1]);
            }
        }

        /// <summary>
        /// Display armor upgrade menu and handle selection
        /// </summary>
        public void UpgradeArmorMenu(Player.Player player)
        {
            var upgradableArmor = GetUpgradableArmor(player);

            if (upgradableArmor.Count == 0)
            {
                Console.WriteLine("\n❌ You don't have any armor ready for upgrade.");
                Console.WriteLine("Armor needs to gain experience in combat before it can be upgraded!");
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
                return;
            }

            Console.Clear();
            Console.WriteLine("=== ARMOR UPGRADES ===\n");
            Console.WriteLine($"Your Gold: {player.Gold}\n");

            for (int i = 0; i < upgradableArmor.Count; i++)
            {
                var armor = upgradableArmor[i];
                int cost = CalculateUpgradeCost(armor);
                string equipped = armor == player.EquippedArmor ? " [EQUIPPED]" : "";

                Console.WriteLine($"{i + 1}. {armor.GetDisplayName()}{equipped}");
                Console.WriteLine($"   Current: +{armor.Level} | Next: +{armor.Level + 1}");
                Console.WriteLine($"   Upgrade Cost: {cost} gold");
                Console.WriteLine($"   Defense: {armor.Defense} → {GetPreviewDefense(armor)}");
                Console.WriteLine();
            }

            Console.WriteLine($"{upgradableArmor.Count + 1}. Cancel");
            Console.Write("\nSelect armor to upgrade: ");

            string input = Console.ReadLine();
            if (int.TryParse(input, out int choice) && choice > 0 && choice <= upgradableArmor.Count)
            {
                UpgradeArmor(player, upgradableArmor[choice - 1]);
            }
        }

        /// <summary>
        /// Calculate the upgrade cost with blacksmith skill discount
        /// </summary>
        private int CalculateUpgradeCost(Weapon weapon)
        {
            int baseCost = weapon.GetUpgradeCost();

            // Skill level provides discount: 5% per skill level (max 50% discount at level 10)
            double discount = 1.0 - (SkillLevel * 0.05);
            int finalCost = (int)(baseCost * discount);

            return Math.Max(1, finalCost);  // Minimum 1 gold
        }

        /// <summary>
        /// Calculate the upgrade cost with blacksmith skill discount
        /// </summary>
        private int CalculateUpgradeCost(Armor armor)
        {
            int baseCost = armor.GetUpgradeCost();

            // Skill level provides discount: 5% per skill level (max 50% discount at level 10)
            double discount = 1.0 - (SkillLevel * 0.05);
            int finalCost = (int)(baseCost * discount);

            return Math.Max(1, finalCost);  // Minimum 1 gold
        }

        /// <summary>
        /// Preview what the weapon's damage will be after upgrade
        /// </summary>
        private string GetPreviewDamage(Weapon weapon)
        {
            // Calculate what stats will be at next level
            float nextLevelScaling = 1.0f + (weapon.Level * 0.01f);
            int nextMinDamage = (int)(weapon.MinDamage / (1.0f + ((weapon.Level - 1) * 0.01f)) * nextLevelScaling);
            int nextMaxDamage = (int)(weapon.MaxDamage / (1.0f + ((weapon.Level - 1) * 0.01f)) * nextLevelScaling);

            return $"{nextMinDamage}-{nextMaxDamage}";
        }

        /// <summary>
        /// Preview what the armor's defense will be after upgrade
        /// </summary>
        private int GetPreviewDefense(Armor armor)
        {
            // Calculate what defense will be at next level
            float nextLevelScaling = 1.0f + (armor.Level * 0.01f);
            int nextDefense = (int)(armor.Defense / (1.0f + ((armor.Level - 1) * 0.01f)) * nextLevelScaling);

            return nextDefense;
        }

        /// <summary>
        /// Upgrade a weapon (handles payment and upgrade)
        /// </summary>
        public bool UpgradeWeapon(Player.Player player, Weapon weapon)
        {
            // Check if weapon can be upgraded
            if (!weapon.ReadyForUpgrade)
            {
                Speak("That weapon isn't ready for an upgrade yet. It needs more battle experience!");
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
                return false;
            }

            // Check legendary restriction
            if (weapon.Rarity >= ItemRarity.Legendary && !CanUpgradeLegendary)
            {
                Speak("I'm not skilled enough to work with legendary equipment. You'll need a master blacksmith!");
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
                return false;
            }

            int cost = CalculateUpgradeCost(weapon);

            // Check if player has enough gold
            if (player.Gold < cost)
            {
                Speak($"You need {cost} gold for this upgrade, but you only have {player.Gold} gold.");
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
                return false;
            }

            // Confirm upgrade
            Console.WriteLine($"\n⚒️  Upgrade {weapon.GetDisplayName()} for {cost} gold?");
            Console.Write("(Y/N): ");
            string confirm = Console.ReadLine()?.ToUpper();

            if (confirm != "Y")
            {
                Speak("Come back when you're ready.");
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
                return false;
            }

            // Perform upgrade
            player.Gold -= cost;

            Console.WriteLine("\n*CLANG* *CLANG* *CLANG*");
            Speak("Let me work my magic...");
            System.Threading.Thread.Sleep(1000);  // Dramatic pause

            weapon.CompleteUpgrade();

            Speak($"Done! Your {weapon.Name} is now +{weapon.Level}!");
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
            return true;
        }

        /// <summary>
        /// Upgrade armor (handles payment and upgrade)
        /// </summary>
        public bool UpgradeArmor(Player.Player player, Armor armor)
        {
            // Check if armor can be upgraded
            if (!armor.ReadyForUpgrade)
            {
                Speak("That armor isn't ready for an upgrade yet. It needs more battle experience!");
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
                return false;
            }

            // Check legendary restriction
            if (armor.Rarity >= ItemRarity.Legendary && !CanUpgradeLegendary)
            {
                Speak("I'm not skilled enough to work with legendary equipment. You'll need a master blacksmith!");
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
                return false;
            }

            int cost = CalculateUpgradeCost(armor);

            // Check if player has enough gold
            if (player.Gold < cost)
            {
                Speak($"You need {cost} gold for this upgrade, but you only have {player.Gold} gold.");
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
                return false;
            }

            // Confirm upgrade
            Console.WriteLine($"\n⚒️  Upgrade {armor.GetDisplayName()} for {cost} gold?");
            Console.Write("(Y/N): ");
            string confirm = Console.ReadLine()?.ToUpper();

            if (confirm != "Y")
            {
                Speak("Come back when you're ready.");
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
                return false;
            }

            // Perform upgrade
            player.Gold -= cost;

            Console.WriteLine("\n*CLANG* *CLANG* *CLANG*");
            Speak("Let me work my magic...");
            System.Threading.Thread.Sleep(1000);  // Dramatic pause

            armor.CompleteUpgrade();

            Speak($"Done! Your {armor.Name} is now +{armor.Level}!");
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
            return true;
        }

        /// <summary>
        /// Display price information based on rarity and level
        /// </summary>
        public void ShowPriceGuide()
        {
            Console.Clear();
            Console.WriteLine("=== UPGRADE PRICING GUIDE ===\n");
            Speak("Here's how I calculate my prices...\n");

            Console.WriteLine("Base Cost Formula: 30 gold × (item level + 1)\n");

            Console.WriteLine("Rarity Multipliers:");
            Console.WriteLine("  Common:    1.0x");
            Console.WriteLine("  Uncommon:  1.5x");
            Console.WriteLine("  Rare:      2.0x");
            Console.WriteLine("  Epic:      3.0x");
            Console.WriteLine("  Legendary: 5.0x");
            Console.WriteLine("  Mythic:    10.0x\n");

            Console.WriteLine($"My Skill Discount: {SkillLevel * 5}% off all upgrades!\n");

            Console.WriteLine("Examples:");
            Console.WriteLine($"  Common Weapon Lv1→2:  {(int)(60 * (1.0 - SkillLevel * 0.05))} gold");
            Console.WriteLine($"  Rare Weapon Lv5→6:    {(int)(360 * (1.0 - SkillLevel * 0.05))} gold");
            Console.WriteLine($"  Epic Armor Lv10→11:   {(int)(990 * (1.0 - SkillLevel * 0.05))} gold\n");

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        /// <summary>
        /// Main interaction loop with the blacksmith
        /// </summary>
        public void Interact(Player.Player player)
        {
            bool shopping = true;

            while (shopping)
            {
                ShowServices(player);
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        UpgradeWeaponMenu(player);
                        break;
                    case "2":
                        UpgradeArmorMenu(player);
                        break;
                    case "3":
                        ShowPriceGuide();
                        break;
                    case "4":
                        Speak("Come back anytime!");
                        shopping = false;
                        break;
                    default:
                        Console.WriteLine("\nInvalid choice. Press any key to continue...");
                        Console.ReadKey();
                        break;
                }
            }
        }
    }
}