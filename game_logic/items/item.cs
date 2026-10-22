using System;
using AbilityRarity = GameLogic.Abilities.AbilityRarity;

namespace GameLogic.Items
{
    /// <summary>
    /// Rarity levels for items (affects sell value and drop rates).
    /// Epic and Mythic were removed 2026-10-22 as out of scope for the MVP.
    /// </summary>
    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Legendary
    }

    /// <summary>
    /// Level caps for everything that levels up (decided 2026-10-22).
    /// The overall cap is 40: the player, companions, and Legendary weapons, armor and abilities.
    /// Lower rarities stop sooner: Common 10, Uncommon 20, Rare 30.
    /// </summary>
    public static class LevelCaps
    {
        /// <summary>
        /// The highest level anything in the game can reach
        /// </summary>
        public const int MaxLevel = 40;

        public const int Common = 10;
        public const int Uncommon = 20;
        public const int Rare = 30;
        public const int Legendary = MaxLevel;

        /// <summary>
        /// Level cap for a weapon or armor of this rarity
        /// </summary>
        public static int ForRarity(ItemRarity rarity) => rarity switch
        {
            ItemRarity.Common => Common,
            ItemRarity.Uncommon => Uncommon,
            ItemRarity.Rare => Rare,
            ItemRarity.Legendary => Legendary,
            _ => Common
        };

        /// <summary>
        /// Level cap for an ability of this rarity
        /// </summary>
        public static int ForRarity(AbilityRarity rarity) => rarity switch
        {
            AbilityRarity.Common => Common,
            AbilityRarity.Uncommon => Uncommon,
            AbilityRarity.Rare => Rare,
            AbilityRarity.Legendary => Legendary,
            _ => Common
        };
    }

    /// <summary>
    /// Categories of items
    /// </summary>
    public enum ItemCategory
    {
        Weapon,
        Armor,
        Consumable,
        QuestItem,
        Material,
        Treasure,
        Ammunition
    }

    /// <summary>
    /// Base class for all items in the game
    /// Child classes (Weapon, Armor, Consumable) handle their own leveling logic
    /// </summary>
    public abstract class Item
    {
        // Basic Properties
        public string Name { get; set; }
        public string Description { get; set; }
        public ItemRarity Rarity { get; set; }
        public ItemCategory Category { get; protected set; }
        public int Value { get; set; }  // Base gold value

        // Economy Properties
        public bool IsSellable { get; set; }
        public bool IsTradeable { get; set; }

        // Stacking Properties
        public bool IsStackable { get; protected set; }
        public int MaxStackSize { get; protected set; }
        public int Quantity { get; set; }

        /// <summary>
        /// Base constructor for all items
        /// </summary>
        protected Item(string name, string description, ItemRarity rarity, ItemCategory category, int value = 0)
        {
            Name = name;
            Description = description;
            Rarity = rarity;
            Category = category;
            Value = value;

            // Default economy settings
            IsSellable = category != ItemCategory.QuestItem;  // Quest items usually can't be sold
            IsTradeable = true;

            // Default stacking settings
            IsStackable = category == ItemCategory.Consumable || category == ItemCategory.Material;
            MaxStackSize = IsStackable ? 99 : 1;
            Quantity = 1;
        }

        /// <summary>
        /// Get display name with rarity prefix
        /// Child classes can override to add level suffix
        /// </summary>
        public virtual string GetDisplayName()
        {
            string rarityPrefix = Rarity switch
            {
                ItemRarity.Common => "",
                ItemRarity.Uncommon => "[U] ",
                ItemRarity.Rare => "[R] ",
                ItemRarity.Legendary => "[L] ",
                _ => ""
            };

            string stackSuffix = IsStackable && Quantity > 1 ? $" x{Quantity}" : "";

            return $"{rarityPrefix}{Name}{stackSuffix}";
        }

        /// <summary>
        /// Get detailed item information
        /// Child classes should override to add their specific info
        /// </summary>
        public virtual string GetInfo()
        {
            string info = $"{GetDisplayName()}\n";
            info += $"{Description}\n";
            info += $"Rarity: {Rarity}\n";
            info += $"Value: {Value} gold\n";

            if (IsStackable)
            {
                info += $"Quantity: {Quantity}/{MaxStackSize}\n";
            }

            return info;
        }

        /// <summary>
        /// Calculate sell price based on rarity and value
        /// </summary>
        public int GetSellPrice()
        {
            if (!IsSellable)
            {
                return 0;
            }

            // Base sell price is half the value
            float sellMultiplier = 0.5f;

            // Rarity affects sell price
            sellMultiplier += Rarity switch
            {
                ItemRarity.Uncommon => 0.1f,
                ItemRarity.Rare => 0.2f,
                ItemRarity.Legendary => 0.4f,
                _ => 0f
            };

            return (int)(Value * sellMultiplier);
        }

        /// <summary>
        /// Try to add quantity to stack
        /// Returns true if successful, false if can't stack or would exceed max
        /// </summary>
        public bool AddToStack(int amount)
        {
            if (!IsStackable)
            {
                return false;
            }

            if (Quantity + amount > MaxStackSize)
            {
                return false;
            }

            Quantity += amount;
            return true;
        }

        /// <summary>
        /// Try to remove quantity from stack
        /// Returns true if successful, false if not enough quantity
        /// </summary>
        public bool RemoveFromStack(int amount)
        {
            if (Quantity < amount)
            {
                return false;
            }

            Quantity -= amount;
            return true;
        }

        /// <summary>
        /// Abstract method for item-specific stat modifications
        /// Override in subclasses (weapons, armor, etc.)
        /// </summary>
        public abstract void ModifyStat(Entities.Player.Player player);

        /// <summary>
        /// Use the item (for consumables, quest items, etc.)
        /// Override in subclasses that can be "used"
        /// </summary>
        public virtual void Use(Entities.Player.Player player)
        {
            // Base implementation does nothing
            // Subclasses like Consumable will override this
        }
    }
}