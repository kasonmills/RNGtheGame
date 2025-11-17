using System;
using System.Collections.Generic;
using System.Linq;
using GameLogic.Items;

namespace GameLogic.Entities.Player
{
    /// <summary>
    /// Manages player inventory with smart stacking logic
    /// - Consumables automatically stack
    /// - Weapons/Armor only stack if same name and level
    /// </summary>
    public class PlayerInventory
    {
        public List<Item> Items { get; private set; }
        public int MaxCapacity { get; set; }

        public PlayerInventory(int maxCapacity = 100)
        {
            Items = new List<Item>();
            MaxCapacity = maxCapacity;
        }

        /// <summary>
        /// Add an item to inventory with smart stacking
        /// </summary>
        public bool AddItem(Item item)
        {
            if (Items.Count >= MaxCapacity && !CanStack(item))
            {
                Console.WriteLine($"Inventory full! Cannot add {item.Name}.");
                return false;
            }

            // Try to stack with existing items
            if (item.IsStackable)
            {
                Item existingStack = FindStackableMatch(item);
                if (existingStack != null)
                {
                    existingStack.AddToStack(item.Quantity);
                    Console.WriteLine($"{item.Name} x{item.Quantity} added to existing stack.");
                    return true;
                }
            }

            // Add as new item
            Items.Add(item);
            Console.WriteLine($"{item.GetDisplayName()} added to inventory.");
            return true;
        }

        /// <summary>
        /// Find an existing item that can be stacked with the new item
        /// </summary>
        private Item FindStackableMatch(Item item)
        {
            if (!item.IsStackable)
                return null;

            foreach (var existingItem in Items)
            {
                // Must be stackable and same type
                if (!existingItem.IsStackable || existingItem.GetType() != item.GetType())
                    continue;

                // For consumables: just match name
                if (item is Consumable consumable && existingItem is Consumable existingConsumable)
                {
                    if (existingConsumable.Name == consumable.Name &&
                        existingConsumable.Level == consumable.Level &&
                        existingConsumable.Type == consumable.Type)
                    {
                        return existingItem;
                    }
                }
                // For weapons: match name AND level
                else if (item is Weapon weapon && existingItem is Weapon existingWeapon)
                {
                    if (existingWeapon.Name == weapon.Name &&
                        existingWeapon.Level == weapon.Level &&
                        existingWeapon.MinDamage == weapon.MinDamage &&
                        existingWeapon.MaxDamage == weapon.MaxDamage)
                    {
                        return existingItem;
                    }
                }
                // For armor: match name AND level
                else if (item is Armor armor && existingItem is Armor existingArmor)
                {
                    if (existingArmor.Name == armor.Name &&
                        existingArmor.Level == armor.Level &&
                        existingArmor.Defense == armor.Defense)
                    {
                        return existingItem;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Check if an item can stack with existing items
        /// </summary>
        private bool CanStack(Item item)
        {
            return item.IsStackable && FindStackableMatch(item) != null;
        }

        /// <summary>
        /// Remove an item from inventory
        /// </summary>
        public bool RemoveItem(Item item)
        {
            if (Items.Remove(item))
            {
                Console.WriteLine($"{item.Name} removed from inventory.");
                return true;
            }

            Console.WriteLine($"{item.Name} is not in your inventory.");
            return false;
        }

        /// <summary>
        /// Remove a specific quantity from a stack
        /// </summary>
        public bool RemoveQuantity(Item item, int quantity)
        {
            if (!Items.Contains(item))
            {
                Console.WriteLine($"{item.Name} is not in your inventory.");
                return false;
            }

            item.RemoveFromStack(quantity);

            // Remove item entirely if stack is empty
            if (item.Quantity <= 0)
            {
                Items.Remove(item);
            }

            return true;
        }

        /// <summary>
        /// Get all items of a specific type
        /// </summary>
        public List<T> GetItemsOfType<T>() where T : Item
        {
            return Items.OfType<T>().ToList();
        }

        /// <summary>
        /// Get all consumables
        /// </summary>
        public List<Consumable> GetConsumables()
        {
            return GetItemsOfType<Consumable>();
        }

        /// <summary>
        /// Get all weapons
        /// </summary>
        public List<Weapon> GetWeapons()
        {
            return GetItemsOfType<Weapon>();
        }

        /// <summary>
        /// Get all armor
        /// </summary>
        public List<Armor> GetArmor()
        {
            return GetItemsOfType<Armor>();
        }

        /// <summary>
        /// Find an item by name
        /// </summary>
        public Item FindItemByName(string name)
        {
            return Items.FirstOrDefault(i => i.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Get total count of unique items
        /// </summary>
        public int GetUniqueItemCount()
        {
            return Items.Count;
        }

        /// <summary>
        /// Get total count including stacks
        /// </summary>
        public int GetTotalItemCount()
        {
            return Items.Sum(item => item.Quantity);
        }

        /// <summary>
        /// Display inventory with categories
        /// </summary>
        public void DisplayInventory()
        {
            if (Items.Count == 0)
            {
                Console.WriteLine("Inventory is empty.");
                return;
            }

            Console.WriteLine($"\n=== Inventory ({GetUniqueItemCount()}/{MaxCapacity} slots) ===");
            Console.WriteLine($"Total Items: {GetTotalItemCount()}\n");

            // Weapons
            var weapons = GetWeapons();
            if (weapons.Count > 0)
            {
                Console.WriteLine("--- Weapons ---");
                foreach (var weapon in weapons)
                {
                    Console.WriteLine($"  {weapon.GetDisplayName()}");
                }
                Console.WriteLine();
            }

            // Armor
            var armor = GetArmor();
            if (armor.Count > 0)
            {
                Console.WriteLine("--- Armor ---");
                foreach (var a in armor)
                {
                    Console.WriteLine($"  {a.GetDisplayName()}");
                }
                Console.WriteLine();
            }

            // Consumables
            var consumables = GetConsumables();
            if (consumables.Count > 0)
            {
                Console.WriteLine("--- Consumables ---");
                foreach (var consumable in consumables)
                {
                    Console.WriteLine($"  {consumable.GetDisplayName()}");
                }
                Console.WriteLine();
            }

            // Other items
            var others = Items.Where(i => !(i is Weapon) && !(i is Armor) && !(i is Consumable)).ToList();
            if (others.Count > 0)
            {
                Console.WriteLine("--- Other Items ---");
                foreach (var other in others)
                {
                    Console.WriteLine($"  {other.GetDisplayName()}");
                }
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Sort inventory by type and name
        /// </summary>
        public void SortInventory()
        {
            Items = Items.OrderBy(i => i.Category)
                        .ThenBy(i => i.Name)
                        .ThenBy(i => i is Weapon w ? w.Level : i is Armor a ? a.Level : 0)
                        .ToList();
        }

        /// <summary>
        /// Clear all items from inventory
        /// </summary>
        public void Clear()
        {
            Items.Clear();
            Console.WriteLine("Inventory cleared.");
        }
    }
}