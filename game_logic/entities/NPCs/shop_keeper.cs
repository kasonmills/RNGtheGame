using System;
using System.Collections.Generic;
using System.Linq;
using GameLogic.Items;

namespace GameLogic.Entities.NPCs
{
    /// <summary>
    /// ShopKeeper NPC - Buys and sells items with dynamic pricing and time-based restocking
    ///
    /// FEATURES:
    /// - 3-layer price variability (personality, market conditions, per-item)
    /// - Dual restock system: 25±(1-5) minutes OR 4±(1-2) combat encounters
    /// - Tier-based shops (1-5) with quality and price differences
    /// - Anti-scumming flee penalty system
    ///
    /// INTEGRATION:
    /// After combat victory: ShopKeeper.NotifyAllShopsOfCombat() - counts encounter, resets flee counter
    /// After successful flee: ShopKeeper.NotifyAllShopsOfFlee() - first flee counts, consecutive flees add penalties
    /// </summary>
    public class ShopKeeper : NPCBase
    {
        // Static list to track all shops for combat encounter updates
        private static List<ShopKeeper> _allShops = new List<ShopKeeper>();

        // Shop tier affects inventory quality
        public int ShopTier { get; set; }  // 1-5, higher tier = better items

        // Price variability system (Option 4: Combination approach)
        // Layer 1: Shop personality (set at creation, permanent)
        public double ShopPersonalityModifier { get; set; }  // 0.85-1.15 (greedy to generous)

        // Layer 2: Market conditions (changes per restock)
        public double MarketConditionsModifier { get; set; }  // 0.95-1.05 (market fluctuations)

        // Layer 3: Per-item variations (stored with each item)
        private Dictionary<Item, double> _itemPriceModifiers = new Dictionary<Item, double>();

        // RNG for price calculations
        private Random _rng;

        // Restock tracking (dual system: time-based OR combat-based)
        public DateTime LastRestockTime { get; private set; }
        public int CombatEncountersSinceRestock { get; private set; }
        private static int _consecutiveFlees = 0; // Track consecutive flee attempts across all combat

        // Restock requirements (with randomness applied at creation)
        private int _restockMinutes;           // Base: 25 minutes ± 1-5 minutes
        private int _restockCombatEncounters;  // Base: 4 encounters ± 1-2 encounters

        /// <summary>
        /// Constructor for shopkeeper NPCs
        /// </summary>
        public ShopKeeper(string name, int shopTier = 1, string greeting = null, Random rng = null, int initialPlayerLevel = 1)
            : base(name, NPCType.Merchant, greeting)
        {
            _rng = rng ?? new Random();

            ShopTier = Math.Clamp(shopTier, 1, 5);

            // Set price multipliers based on tier
            // Lower tier shops have worse prices
            BuyPriceMultiplier = 100 + ((5 - ShopTier) * 10);  // Tier 1: 140%, Tier 5: 100%
            SellPriceMultiplier = 50 - ((5 - ShopTier) * 5);   // Tier 1: 30%, Tier 5: 50%

            // Layer 1: Set shop personality modifier (permanent, varies by ±15%)
            // 0.85 = generous (15% cheaper to buy), 1.15 = greedy (15% more expensive)
            ShopPersonalityModifier = 0.85 + (_rng.NextDouble() * 0.30);

            // Layer 2: Initialize market conditions (will change with restock)
            MarketConditionsModifier = 0.95 + (_rng.NextDouble() * 0.10);

            // Initialize restock requirements with randomness
            // Time: 25 minutes ± (1-5 minutes)
            int timeVariation = _rng.Next(1, 6); // 1-5 minutes
            int timeDirection = _rng.Next(0, 2) == 0 ? -1 : 1; // Add or subtract
            _restockMinutes = 25 + (timeVariation * timeDirection);

            // Combat: 4 encounters ± (1-2 encounters)
            int combatVariation = _rng.Next(1, 3); // 1-2 encounters
            int combatDirection = _rng.Next(0, 2) == 0 ? -1 : 1; // Add or subtract
            _restockCombatEncounters = Math.Max(2, 4 + (combatVariation * combatDirection)); // Minimum 2 encounters

            // Initialize restock tracking
            LastRestockTime = DateTime.Now;
            CombatEncountersSinceRestock = 0;

            // Perform initial stock
            RestockShop(initialPlayerLevel, _rng);

            // Register this shop to the global list
            _allShops.Add(this);

            // Set default greeting
            if (greeting == null)
            {
                Greeting = "Welcome to my shop! Looking to buy or sell?";
            }

            // Add some shopkeeper dialogues (personality-based)
            if (ShopPersonalityModifier < 0.95) // Generous
            {
                AddDialogue("I believe in fair prices for all adventurers!");
                AddDialogue("Your coin goes further here than anywhere else!");
                AddDialogue("I give the best deals in town!");
            }
            else if (ShopPersonalityModifier > 1.05) // Greedy
            {
                AddDialogue("Quality has a price, you know.");
                AddDialogue("These are premium goods, worth every coin!");
                AddDialogue("You won't find better... though you might find cheaper elsewhere.");
            }
            else // Neutral
            {
                AddDialogue("I've got the finest goods in town!");
                AddDialogue("Come back anytime, I'm always open for business.");
                AddDialogue("Quality merchandise at fair prices!");
            }

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

            // Show shop personality
            string personalityDesc = ShopPersonalityModifier < 0.95 ? "(Generous Prices)" :
                                     ShopPersonalityModifier > 1.05 ? "(Premium Prices)" :
                                     "(Fair Prices)";
            Console.WriteLine($"Shop Style: {personalityDesc}");

            // Show current market conditions
            string marketDesc = MarketConditionsModifier < 0.98 ? "Buyer's Market" :
                               MarketConditionsModifier > 1.02 ? "Seller's Market" :
                               "Stable Market";
            Console.WriteLine($"Market: {marketDesc}");

            // Show next restock info
            Console.WriteLine($"\nNext Restock: {GetTimeUntilRestock()} OR {GetCombatUntilRestock()}");

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
                int sellPrice = CalculateSellPrice(item, _rng);
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
            int sellPrice = CalculateSellPrice(item, _rng);

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

            int sellPrice = CalculateSellPrice(item, _rng);

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
        /// Uses 3-layer price variability system:
        /// Layer 1: Shop personality (permanent)
        /// Layer 2: Market conditions (per restock)
        /// Layer 3: Per-item variation (tiny individual modifier)
        /// </summary>
        public int CalculateBuyPrice(Item item)
        {
            int basePrice = item.Value;

            // Apply tier-based multiplier
            double price = basePrice * (BuyPriceMultiplier / 100.0);

            // Layer 1: Shop personality modifier
            price *= ShopPersonalityModifier;

            // Layer 2: Market conditions modifier
            price *= MarketConditionsModifier;

            // Layer 3: Per-item variation (if exists)
            if (_itemPriceModifiers.ContainsKey(item))
            {
                price *= _itemPriceModifiers[item];
            }

            return Math.Max(1, (int)Math.Round(price));
        }

        /// <summary>
        /// Calculate sell price (what player receives for selling)
        /// Uses inverse of shop personality (greedy shops pay less, generous pay more)
        /// Includes per-transaction randomness for items player is selling
        /// </summary>
        public int CalculateSellPrice(Item item, Random rng = null)
        {
            int basePrice = item.Value;

            // Apply tier-based multiplier
            double price = basePrice * (SellPriceMultiplier / 100.0);

            // Layer 1: Inverse shop personality (greedy = 1.15 becomes 0.87, generous = 0.85 becomes 1.18)
            // This makes greedy shops pay less when buying from player
            double inversePersonality = 2.0 - ShopPersonalityModifier;
            price *= inversePersonality;

            // Layer 2: Market conditions affect sell prices too (but less impact)
            price *= (1.0 + ((MarketConditionsModifier - 1.0) * 0.5));

            // Layer 3: Per-transaction variation for player's items (±2% tiny haggling variation)
            if (rng != null)
            {
                double transactionModifier = 0.98 + (rng.NextDouble() * 0.04);
                price *= transactionModifier;
            }

            return Math.Max(1, (int)Math.Round(price));
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

            // Show shop personality explanation
            string personalityExplanation;
            if (ShopPersonalityModifier < 0.95)
                personalityExplanation = "I run a GENEROUS shop - my prices are lower than most!";
            else if (ShopPersonalityModifier > 1.05)
                personalityExplanation = "I deal in PREMIUM goods - quality costs more here.";
            else
                personalityExplanation = "I offer FAIR prices - right in the middle.";

            Console.WriteLine($"Shop Personality: {personalityExplanation}\n");

            Console.WriteLine("PRICING SYSTEM:");
            Console.WriteLine("  Prices are affected by:");
            Console.WriteLine("    1. Shop Tier (base multiplier)");
            Console.WriteLine("    2. My personal pricing style (permanent)");
            Console.WriteLine("    3. Current market conditions (changes with restock)");
            Console.WriteLine("    4. Individual item variations (small differences)\n");

            Console.WriteLine("BUY PRICES:");
            var exampleWeapon = new Weapon("Example", "", WeaponType.Sword, 1, 1, 1, 1, ItemRarity.Common, 1, 100,
                minDamageUpgradeRange: (1, 2), damageShiftRange: (1, 1), maxDamageUpgradeRange: (1, 3));
            Console.WriteLine($"  Base: {BuyPriceMultiplier}% of item value (tier-based)");
            Console.WriteLine($"  Example: A 100g sword costs you ~{CalculateBuyPrice(exampleWeapon)}g\n");

            Console.WriteLine("SELL PRICES:");
            Console.WriteLine($"  Base: {SellPriceMultiplier}% of item value (tier-based)");
            Console.WriteLine($"  Example: A 100g sword sells for ~{CalculateSellPrice(exampleWeapon, _rng)}g");
            Console.WriteLine("  Note: Prices vary slightly with each transaction!\n");

            Console.WriteLine("RESTOCKING SYSTEM:");
            Console.WriteLine("  My shop restocks when EITHER condition is met:");
            Console.WriteLine($"    - Time: Every ~{_restockMinutes} minutes (varies slightly)");
            Console.WriteLine($"    - Combat: Every ~{_restockCombatEncounters} encounters (varies slightly)");
            Console.WriteLine("  Upon restock:");
            Console.WriteLine("    - New random inventory");
            Console.WriteLine("    - Market conditions change");
            Console.WriteLine("    - New item price variations\n");

            Console.WriteLine("NOTES:");
            Console.WriteLine("  - Quest items cannot be sold");
            Console.WriteLine("  - Can't force restock by leaving and returning");
            Console.WriteLine("  - Check 'Next Restock' timer at main menu");
            Console.WriteLine("  - Shop around for the best deals!\n");

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        /// <summary>
        /// Check if shop needs restocking based on time or combat encounters
        /// Returns true if either condition is met
        /// </summary>
        public bool ShouldRestock()
        {
            // Check time-based condition
            TimeSpan timeSinceRestock = DateTime.Now - LastRestockTime;
            bool timeConditionMet = timeSinceRestock.TotalMinutes >= _restockMinutes;

            // Check combat-based condition
            bool combatConditionMet = CombatEncountersSinceRestock >= _restockCombatEncounters;

            return timeConditionMet || combatConditionMet;
        }

        /// <summary>
        /// Increment combat encounter counter (call this after each combat)
        /// </summary>
        public void IncrementCombatEncounter()
        {
            CombatEncountersSinceRestock++;
        }

        /// <summary>
        /// Static method to notify all shops that a combat encounter occurred (victory)
        /// Call this from the combat manager after each battle victory
        /// Resets consecutive flee counter on victory
        /// </summary>
        public static void NotifyAllShopsOfCombat()
        {
            // Reset flee counter on successful combat completion
            _consecutiveFlees = 0;

            foreach (var shop in _allShops)
            {
                shop.IncrementCombatEncounter();
            }
        }

        /// <summary>
        /// Static method to notify all shops that player fled from combat
        /// Applies penalties to restock requirements if fleeing multiple times
        /// </summary>
        public static void NotifyAllShopsOfFlee()
        {
            _consecutiveFlees++;

            // Only count as encounter if first flee (no penalty)
            if (_consecutiveFlees == 1)
            {
                foreach (var shop in _allShops)
                {
                    shop.IncrementCombatEncounter();
                }
                Console.WriteLine("(First flee - combat counted for shop restocks)");
            }
            else
            {
                // Apply penalty: increase requirements for all shops
                foreach (var shop in _allShops)
                {
                    shop.ApplyFleePenalty();
                }
                Console.WriteLine($"(Consecutive flee #{_consecutiveFlees} - shop restock requirements increased!)");
            }
        }

        /// <summary>
        /// Apply penalty for fleeing from combat multiple times
        /// Increases both time and combat requirements
        /// </summary>
        private void ApplyFleePenalty()
        {
            // Add 5 minutes per consecutive flee after the first
            _restockMinutes += 5;

            // Add 1 encounter per consecutive flee after the first
            _restockCombatEncounters += 1;
        }

        /// <summary>
        /// Get all registered shops (for debugging/save system)
        /// </summary>
        public static List<ShopKeeper> GetAllShops()
        {
            return new List<ShopKeeper>(_allShops);
        }

        /// <summary>
        /// Clear all registered shops (useful for testing or new game)
        /// </summary>
        public static void ClearAllShops()
        {
            _allShops.Clear();
        }

        /// <summary>
        /// Get time remaining until next restock (for display purposes)
        /// </summary>
        public string GetTimeUntilRestock()
        {
            TimeSpan timeSinceRestock = DateTime.Now - LastRestockTime;
            double minutesRemaining = _restockMinutes - timeSinceRestock.TotalMinutes;

            if (minutesRemaining <= 0)
            {
                return "Ready to restock!";
            }

            return $"{Math.Ceiling(minutesRemaining)} minutes";
        }

        /// <summary>
        /// Get combat encounters remaining until restock (for display purposes)
        /// </summary>
        public string GetCombatUntilRestock()
        {
            int encountersRemaining = _restockCombatEncounters - CombatEncountersSinceRestock;

            if (encountersRemaining <= 0)
            {
                return "Ready to restock!";
            }

            return $"{encountersRemaining} encounter{(encountersRemaining > 1 ? "s" : "")}";
        }

        /// <summary>
        /// Populate shop with random items based on tier and player level
        /// Also refreshes market conditions and applies per-item price variations
        /// </summary>
        public void RestockShop(int playerLevel, Random rng)
        {
            ShopInventory.Clear();
            _itemPriceModifiers.Clear();

            // Reset restock tracking
            LastRestockTime = DateTime.Now;
            CombatEncountersSinceRestock = 0;

            // Re-randomize restock requirements for next time
            // Time: 25 minutes ± (1-5 minutes)
            int timeVariation = rng.Next(1, 6);
            int timeDirection = rng.Next(0, 2) == 0 ? -1 : 1;
            _restockMinutes = 25 + (timeVariation * timeDirection);

            // Combat: 4 encounters ± (1-2 encounters)
            int combatVariation = rng.Next(1, 3);
            int combatDirection = rng.Next(0, 2) == 0 ? -1 : 1;
            _restockCombatEncounters = Math.Max(2, 4 + (combatVariation * combatDirection));

            // Layer 2: Refresh market conditions (±5% variation per restock)
            MarketConditionsModifier = 0.95 + (rng.NextDouble() * 0.10);

            string marketCondition = MarketConditionsModifier < 0.98 ? "Buyer's market!" :
                                     MarketConditionsModifier > 1.02 ? "Seller's market!" :
                                     "Market is stable.";

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

                // Layer 3: Apply per-item price variation (±3% tiny random modifier)
                double itemModifier = 0.97 + (rng.NextDouble() * 0.06);
                _itemPriceModifiers[item] = itemModifier;

                ShopInventory.Add(item);
            }

            Console.WriteLine($"\n{Name}'s shop has been restocked with {itemCount} items!");
            Console.WriteLine($"Market conditions: {marketCondition}");
        }

        /// <summary>
        /// Main interaction loop with the shopkeeper
        /// </summary>
        public void Interact(Player.Player player)
        {
            // Check if shop should restock
            if (ShouldRestock())
            {
                Console.WriteLine($"\n✨ {Name} has new inventory!");
                RestockShop(player.Level, _rng);
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
            }

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