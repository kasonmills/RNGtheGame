using System;
using System.Collections.Generic;
using System.Linq;
using GameLogic.Items;
using GameLogic.Systems;

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

        /// <summary>
        /// Spell components live here, not in Items, and take no inventory slots
        /// </summary>
        public ComponentPouch Pouch { get; private set; }

        public PlayerInventory(int maxCapacity = 100)
        {
            Items = new List<Item>();
            MaxCapacity = maxCapacity;
            Pouch = new ComponentPouch();
        }

        /// <summary>
        /// Fired when an item can't be added, with the message to show the player (bag full,
        /// or a stack would go over 99). Nothing is added in that case - the item stays where
        /// it was found.
        /// TODO-GODOT: Notifier shows the message as a toast/pop-up.
        /// </summary>
        public event Action<Item, string> ItemNotAdded;

        /// <summary>
        /// Add an item to inventory with smart stacking. Spell components go to the pouch.
        /// All or nothing: if the bag is full, or a stack (bolts, components, potions...) would
        /// go over 99, nothing is added, ItemNotAdded fires and it returns false.
        /// </summary>
        public bool AddItem(Item item)
        {
            if (item is SpellComponent component)
            {
                if (Pouch.Add(component))
                    return true;

                ItemNotAdded?.Invoke(item, $"You can only carry {ComponentPouch.MaxPerComponent} {item.Name}. Nothing more can be added - it was left behind.");
                return false;
            }

            // Try to stack with existing items
            if (item.IsStackable)
            {
                Item existingStack = FindStackableMatch(item);
                if (existingStack != null)
                {
                    if (existingStack.AddToStack(item.Quantity))
                        return true;

                    ItemNotAdded?.Invoke(item, $"You can only carry {existingStack.MaxStackSize} {item.Name}. Nothing more can be added - it was left behind.");
                    return false;
                }
            }

            if (Items.Count >= MaxCapacity)
            {
                ItemNotAdded?.Invoke(item, $"Your bag is full. {item.Name} was left behind.");
                return false;
            }

            // Add as new item
            Items.Add(item);
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
                // For bolts: match the bolt type
                else if (item is Ammunition ammo && existingItem is Ammunition existingAmmo)
                {
                    if (existingAmmo.Type == ammo.Type)
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
        /// Remove an item from inventory
        /// </summary>
        public bool RemoveItem(Item item)
        {
            if (Items.Remove(item))
            {
                GameLog.Write($"{item.Name} removed from inventory.");
                return true;
            }

            GameLog.Write($"{item.Name} is not in your inventory.");
            return false;
        }

        /// <summary>
        /// Remove a specific quantity from a stack
        /// </summary>
        public bool RemoveQuantity(Item item, int quantity)
        {
            if (!Items.Contains(item))
            {
                GameLog.Write($"{item.Name} is not in your inventory.");
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
        /// Get all items in inventory
        /// </summary>
        public List<Item> GetAllItems()
        {
            return Items.ToList();
        }

        /// <summary>
        /// Check if inventory is at capacity
        /// </summary>
        public bool IsFull()
        {
            return Items.Count >= MaxCapacity;
        }

        /// <summary>
        /// Get total count including stacks
        /// </summary>
        public int GetTotalItemCount()
        {
            return Items.Sum(item => item.Quantity);
        }

        // Console-only screen, commented out 2026-11-10 (no console output - nothing calls it; the Godot screen replaces it). Kept for reference:
        // /// <summary>
        // /// Display inventory with categories
        // /// </summary>
        // public void DisplayInventory()
        // {
        //     if (Items.Count == 0)
        //     {
        //         Console.WriteLine("Inventory is empty.");
        //         return;
        //     }
        //
        //     Console.WriteLine($"\n=== Inventory ({GetUniqueItemCount()}/{MaxCapacity} slots) ===");
        //     Console.WriteLine($"Total Items: {GetTotalItemCount()}\n");
        //
        //     // Weapons
        //     var weapons = GetWeapons();
        //     if (weapons.Count > 0)
        //     {
        //         Console.WriteLine("--- Weapons ---");
        //         foreach (var weapon in weapons)
        //         {
        //             Console.WriteLine($"  {weapon.GetDisplayName()}");
        //         }
        //         Console.WriteLine();
        //     }
        //
        //     // Armor
        //     var armor = GetArmor();
        //     if (armor.Count > 0)
        //     {
        //         Console.WriteLine("--- Armor ---");
        //         foreach (var a in armor)
        //         {
        //             Console.WriteLine($"  {a.GetDisplayName()}");
        //         }
        //         Console.WriteLine();
        //     }
        //
        //     // Consumables
        //     var consumables = GetConsumables();
        //     if (consumables.Count > 0)
        //     {
        //         Console.WriteLine("--- Consumables ---");
        //         foreach (var consumable in consumables)
        //         {
        //             Console.WriteLine($"  {consumable.GetDisplayName()}");
        //         }
        //         Console.WriteLine();
        //     }
        //
        //     // Other items
        //     var others = Items.Where(i => !(i is Weapon) && !(i is Armor) && !(i is Consumable)).ToList();
        //     if (others.Count > 0)
        //     {
        //         Console.WriteLine("--- Other Items ---");
        //         foreach (var other in others)
        //         {
        //             Console.WriteLine($"  {other.GetDisplayName()}");
        //         }
        //         Console.WriteLine();
        //     }
        // }

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
            Pouch.Clear();
        }
    }
}