using System;
using System.Collections.Generic;
using System.Linq;
using GameLogic.Items;

namespace GameLogic.Entities.NPCs
{
    /// <summary>
    /// ShopKeeper NPC - Buys and sells items
    /// Players can purchase items from shop inventory and sell their own items
    /// </summary>
    public class ShopKeeper : NPCBase
    {
        // Shop tier affects inventory quality
        public int ShopTier { get; set; }  // 1-5, higher tier = better items

        /// <summary>
        /// Constructor for shopkeeper NPCs
        /// </summary>
        public ShopKeeper(string name, int shopTier = 1, string greeting = null)
            : base(name, NPCType.Merchant, greeting)
        {
            ShopTier = Math.Clamp(shopTier, 1, 5);

            // Set price multipliers based on tier
            // Lower tier shops have worse prices
            BuyPriceMultiplier = 100 + ((5 - ShopTier) * 10);  // Tier 1: 140%, Tier 5: 100%
            SellPriceMultiplier = 50 - ((5 - ShopTier) * 5);   // Tier 1: 30%, Tier 5: 50%

            // Set default greeting
            if (greeting == null)
            {
                Greeting = "Welcome to my shop! Looking to buy or sell?";
            }

            // Add some shopkeeper dialogues
            AddDialogue("I've got the finest goods in town!");
            AddDialogue("Come back anytime, I'm always open for business.");
            AddDialogue("Quality merchandise at fair prices!");
            AddDialogue("Need supplies? You've come to the right place.");
            AddDialogue("Every adventurer needs good equipment. Take a look around!");
        }

        /// <summary>
        /// Show the shopkeeper's main menu
        /// </summary>
        public void ShowShop(Player.Player player)
        {
            Console.Clear();
            Console.WriteLine("=".PadRight(50, '='));
            Console.WriteLine($"    {Name}'s Shop");
            Console.WriteLine("=".PadRight(50, '='));
            Greet();
            Console.WriteLine($"\nShop Tier: {ShopTier}/5");
            Console.WriteLine($"Buy Price: {BuyPriceMultiplier}% of value");
            Console.WriteLine($"Sell Price: {SellPriceMultiplier}% of value");
            Console.WriteLine($"\nYour Gold: {player.Gold}");

            Console.WriteLine("\nWhat would you like to do?");
            Console.WriteLine("1. Buy Items");
            Console.WriteLine("2. Sell Items");
            Console.WriteLine("3. View Shop Info");
            Console.WriteLine("4. Leave");

            Console.Write("\nChoice: ");
        }

        /// <summary>
        /// Display shop inventory for purchase
        /// </summary>
        public void DisplayGoods()
        {
            if (ShopInventory.Count == 0)
            {
                Console.WriteLine("\nThe shop is currently out of stock!");
                return;
            }

            Console.WriteLine("\n=== SHOP INVENTORY ===\n");

            // Group by category
            var weapons = ShopInventory.Where(i => i.Category == ItemCategory.Weapon).ToList();
            var armor = ShopInventory.Where(i => i.Category == ItemCategory.Armor).ToList();
            var consumables = ShopInventory.Where(i => i.Category == ItemCategory.Consumable).ToList();

            int index = 1;

            if (weapons.Count > 0)
            {
                Console.WriteLine("--- WEAPONS ---");
                foreach (var item in weapons)
                {
                    int price = CalculateBuyPrice(item);
                    Console.WriteLine($"{index}. {item.GetDisplayName()} - {price} gold");
                    index++;
                }
                Console.WriteLine();
            }

            if (armor.Count > 0)
            {
                Console.WriteLine("--- ARMOR ---");
                foreach (var item in armor)
                {
                    int price = CalculateBuyPrice(item);
                    Console.WriteLine($"{index}. {item.GetDisplayName()} - {price} gold");
                    index++;
                }
                Console.WriteLine();
            }

            if (consumables.Count > 0)
            {
                Console.WriteLine("--- CONSUMABLES ---");
                foreach (var item in consumables)
                {
                    int price = CalculateBuyPrice(item);
                    Console.WriteLine($"{index}. {item.GetDisplayName()} - {price} gold");
                    index++;
                }
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Buy menu - player purchases items from shop
        /// </summary>
        public void BuyMenu(Player.Player player)
        {
            if (ShopInventory.Count == 0)
            {
                Console.WriteLine("\n❌ The shop is currently out of stock!");
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
                return;
            }

            Console.Clear();
            Console.WriteLine("=== BUY ITEMS ===\n");
            Console.WriteLine($"Your Gold: {player.Gold}\n");

            DisplayGoods();

            Console.WriteLine($"{ShopInventory.Count + 1}. Cancel");
            Console.Write("\nSelect item to buy (or enter number for details): ");

            string input = Console.ReadLine();
            if (int.TryParse(input, out int choice) && choice > 0 && choice <= ShopInventory.Count)
            {
                Item selectedItem = ShopInventory[choice - 1];
                ShowItemDetails(selectedItem, player);
            }
        }

        /// <summary>
        /// Show item details and confirm purchase
        /// </summary>
        private void ShowItemDetails(Item item, Player.Player player)
        {
            Console.Clear();
            Console.WriteLine("=== ITEM DETAILS ===\n");
            Console.WriteLine(item.GetInfo());

            int price = CalculateBuyPrice(item);
            Console.WriteLine($"\nPrice: {price} gold");
            Console.WriteLine($"Your Gold: {player.Gold}");

            if (player.Gold < price)
            {
                Console.WriteLine("\n❌ You don't have enough gold!");
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
                return;
            }

            Console.Write("\nBuy this item? (Y/N): ");
            string confirm = Console.ReadLine()?.ToUpper();

            if (confirm == "Y")
            {
                BuyItem(player, item);
            }
        }

        /// <summary>
        /// Process item purchase
        /// </summary>
        public bool BuyItem(Player.Player player, Item item)
        {
            int price = CalculateBuyPrice(item);

            if (player.Gold < price)
            {
                Speak("You don't have enough gold for that!");
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
                return false;
            }

            if (player.Inventory.IsFull() && !item.IsStackable)
            {
                Speak("Your inventory is full! You need to make space first.");
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
                return false;
            }

            // Complete the transaction
            player.Gold -= price;
            player.Inventory.AddItem(item);
            ShopInventory.Remove(item);

            Console.WriteLine($"\n✅ Purchased {item.GetDisplayName()} for {price} gold!");
            Speak("Pleasure doing business with you!");
            Console.WriteLine($"\nGold remaining: {player.Gold}");
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
            return true;
        }

        /// <summary>
        /// Sell menu - player sells items to shop
        /// </summary>
        public void SellMenu(Player.Player player)
        {
            if (player.Inventory.Items.Count == 0)
            {
                Console.WriteLine("\n❌ You don't have any items to sell!");
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
                return;
            }

            Console.Clear();
            Console.WriteLine("=== SELL ITEMS ===\n");
            Console.WriteLine($"Your Gold: {player.Gold}\n");

            // Display player inventory with sell prices
            var sellableItems = player.Inventory.Items
                .Where(i => i.Category != ItemCategory.QuestItem)  // Can't sell quest items
                .ToList();

            if (sellableItems.Count == 0)
            {
                Console.WriteLine("❌ You don't have any items that can be sold!");
                Console.WriteLine("(Quest items cannot be sold)");
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
                return;
            }

            for (int i = 0; i < sellableItems.Count; i++)
            {
                var item = sellableItems[i];
                int sellPrice = CalculateSellPrice(item);
                Console.WriteLine($"{i + 1}. {item.GetDisplayName()} - {sellPrice} gold");
            }

            Console.WriteLine($"{sellableItems.Count + 1}. Cancel");
            Console.Write("\nSelect item to sell: ");

            string input = Console.ReadLine();
            if (int.TryParse(input, out int choice) && choice > 0 && choice <= sellableItems.Count)
            {
                Item selectedItem = sellableItems[choice - 1];
                ConfirmSell(player, selectedItem);
            }
        }

        /// <summary>
        /// Confirm sell transaction
        /// </summary>
        private void ConfirmSell(Player.Player player, Item item)
        {
            int sellPrice = CalculateSellPrice(item);

            Console.Clear();
            Console.WriteLine("=== CONFIRM SALE ===\n");
            Console.WriteLine(item.GetInfo());
            Console.WriteLine($"\nSell Price: {sellPrice} gold");

            // Warn about equipped items
            if ((item is Weapon && item == player.EquippedWeapon) ||
                (item is Armor && item == player.EquippedArmor))
            {
                Console.WriteLine("\n⚠️  WARNING: This item is currently equipped!");
            }

            Console.Write("\nSell this item? (Y/N): ");
            string confirm = Console.ReadLine()?.ToUpper();

            if (confirm == "Y")
            {
                SellItem(player, item);
            }
        }

        /// <summary>
        /// Process item sale
        /// </summary>
        public bool SellItem(Player.Player player, Item item)
        {
            // Can't sell quest items
            if (item.Category == ItemCategory.QuestItem)
            {
                Speak("I can't buy quest items. Those are important to your adventure!");
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
                return false;
            }

            // Unequip if equipped
            if (item is Weapon && item == player.EquippedWeapon)
            {
                player.EquippedWeapon = null;
                Console.WriteLine("Weapon unequipped.");
            }
            else if (item is Armor && item == player.EquippedArmor)
            {
                player.EquippedArmor = null;
                Console.WriteLine("Armor unequipped.");
            }

            int sellPrice = CalculateSellPrice(item);

            // Complete the transaction
            player.Gold += sellPrice;
            player.Inventory.RemoveItem(item);

            // Add to shop inventory (optional - shops could have infinite gold)
            ShopInventory.Add(item);

            Console.WriteLine($"\n✅ Sold {item.GetDisplayName()} for {sellPrice} gold!");
            Speak("Thank you! I'll find a good home for this.");
            Console.WriteLine($"\nGold: {player.Gold}");
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
            return true;
        }

        /// <summary>
        /// Calculate purchase price (what player pays to buy)
        /// </summary>
        public int CalculateBuyPrice(Item item)
        {
            int basePrice = item.Value;
            int finalPrice = (int)(basePrice * (BuyPriceMultiplier / 100.0));
            return Math.Max(1, finalPrice);
        }

        /// <summary>
        /// Calculate sell price (what player receives for selling)
        /// </summary>
        public int CalculateSellPrice(Item item)
        {
            int basePrice = item.Value;
            int finalPrice = (int)(basePrice * (SellPriceMultiplier / 100.0));
            return Math.Max(1, finalPrice);
        }

        /// <summary>
        /// Display shop information
        /// </summary>
        public void ShowShopInfo()
        {
            Console.Clear();
            Console.WriteLine("=== SHOP INFORMATION ===\n");
            Speak("Here's how my shop works...\n");

            Console.WriteLine($"Shop Tier: {ShopTier}/5");
            Console.WriteLine("Higher tier shops have better prices!\n");

            Console.WriteLine("BUY PRICES:");
            Console.WriteLine($"  You pay {BuyPriceMultiplier}% of an item's base value");
            Console.WriteLine($"  Example: A 100g sword costs you {CalculateBuyPrice(new Weapon("Example", "", WeaponType.Sword, 1, 1, 1, 1, ItemRarity.Common, 1, 100))}g\n");

            Console.WriteLine("SELL PRICES:");
            Console.WriteLine($"  You receive {SellPriceMultiplier}% of an item's base value");
            Console.WriteLine($"  Example: A 100g sword sells for {CalculateSellPrice(new Weapon("Example", "", WeaponType.Sword, 1, 1, 1, 1, ItemRarity.Common, 1, 100))}g\n");

            Console.WriteLine("NOTES:");
            Console.WriteLine("  - Quest items cannot be sold");
            Console.WriteLine("  - Shop inventory refreshes periodically");
            Console.WriteLine("  - Better prices at higher tier shops\n");

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        /// <summary>
        /// Populate shop with random items based on tier and player level
        /// </summary>
        public void RestockShop(int playerLevel, Random rng)
        {
            ShopInventory.Clear();

            // Number of items based on tier
            int itemCount = 5 + (ShopTier * 2);  // Tier 1: 7 items, Tier 5: 15 items

            for (int i = 0; i < itemCount; i++)
            {
                // Determine category
                int categoryRoll = rng.Next(0, 100);
                ItemCategory category;

                if (categoryRoll < 30)
                    category = ItemCategory.Weapon;
                else if (categoryRoll < 60)
                    category = ItemCategory.Armor;
                else
                    category = ItemCategory.Consumable;

                // Generate item at appropriate level
                int itemLevel = Math.Max(1, playerLevel + rng.Next(-2, 3));
                Item item = ItemDatabase.GetRandomItem(category, rng, itemLevel);

                // Higher tier shops have better rarity
                if (ShopTier >= 3 && rng.Next(0, 100) < 20)
                {
                    item.Rarity = ItemRarity.Uncommon;
                }
                if (ShopTier >= 4 && rng.Next(0, 100) < 10)
                {
                    item.Rarity = ItemRarity.Rare;
                }
                if (ShopTier >= 5 && rng.Next(0, 100) < 5)
                {
                    item.Rarity = ItemRarity.Epic;
                }

                ShopInventory.Add(item);
            }

            Console.WriteLine($"\n{Name}'s shop has been restocked with {itemCount} items!");
        }

        /// <summary>
        /// Main interaction loop with the shopkeeper
        /// </summary>
        public void Interact(Player.Player player)
        {
            bool shopping = true;

            while (shopping)
            {
                ShowShop(player);
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        BuyMenu(player);
                        break;
                    case "2":
                        SellMenu(player);
                        break;
                    case "3":
                        ShowShopInfo();
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