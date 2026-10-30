using System;
using System.Collections.Generic;
using System.Linq;

namespace GameLogic.Items
{
    /// <summary>
    /// Every spell component. Spells (not built yet) will spend these when cast.
    /// Grouped by comment only - how components are categorized is still being designed.
    /// </summary>
    public enum SpellComponentType
    {
        // Crystals
        NegativityCrystal,

        // Lucky components
        Ladybug,
        Horseshoe,
        Bamboo,
        Acorns,
        Fish,
        Jade,

        // Unlucky components
        BlackCatFur,
        BrokenMirror,
        RavenFeather,

        // Mushrooms
        Spotshroom,
        Poisonshroom,
        Fairyshroom,
        Haleshroom,
        Flatshroom,
        Shieldshroom,
        Strengthshroom,

        // Fruits
        Blackberries,
        Blueberries,
        Roseberries,
        Nanaberries,
        Melonberries,
        Appleberries,
        Honeyberries,

        // Bugs
        Berylflies,
        Crystacricket,
        Rubeetles,
        Draviteflies,
        Amberbees,
        Sapphider,
        Opalant,

        // Herbs
        ForestFern,
        EmeraldFronds,
        Lavenjade,
        Kellycones,
        Kombuthyme,
        LaurelScales,
        Mossmary,
        MyrtleNeedles,
        Min
    }

    /// <summary>
    /// A physical component spells need to be cast. Stackable, kept in the shared party bag.
    /// Unlike gear, a component's rarity is fixed - loot rolls pick which component drops
    /// rather than changing its rarity.
    /// </summary>
    public class SpellComponent : Item
    {
        // PLACEHOLDER rarities, descriptions and purchasable flags - to be defined with the
        // magic system. IsPurchasable = the trader can stock it for the player to buy.
        // Components not purchasable only come from loot.
        private static readonly Dictionary<SpellComponentType, (string Name, string Description, ItemRarity Rarity, bool IsPurchasable)> Data = new()
        {
            [SpellComponentType.NegativityCrystal] = ("Negativity Crystal", "A crystal that increases negative probability.", ItemRarity.Rare, false),

            [SpellComponentType.Ladybug] = ("Ladybug", "A lucky component.", ItemRarity.Uncommon, false),
            [SpellComponentType.Horseshoe] = ("Horseshoe", "A lucky component.", ItemRarity.Uncommon, false),
            [SpellComponentType.Bamboo] = ("Bamboo", "A lucky component.", ItemRarity.Uncommon, false),
            [SpellComponentType.Acorns] = ("Acorns", "A lucky component.", ItemRarity.Uncommon, false),
            [SpellComponentType.Fish] = ("Fish", "A lucky component.", ItemRarity.Uncommon, false),
            [SpellComponentType.Jade] = ("Jade", "A lucky component.", ItemRarity.Uncommon, false),

            [SpellComponentType.BlackCatFur] = ("Black Cat Fur", "An unlucky component.", ItemRarity.Uncommon, false),
            [SpellComponentType.BrokenMirror] = ("Broken Mirror", "An unlucky component.", ItemRarity.Uncommon, false),
            [SpellComponentType.RavenFeather] = ("Raven Feather", "An unlucky component.", ItemRarity.Uncommon, false),

            [SpellComponentType.Spotshroom] = ("Spotshroom", "A mushroom.", ItemRarity.Common, true),
            [SpellComponentType.Poisonshroom] = ("Poisonshroom", "A mushroom.", ItemRarity.Common, true),
            [SpellComponentType.Fairyshroom] = ("Fairyshroom", "A mushroom.", ItemRarity.Common, true),
            [SpellComponentType.Haleshroom] = ("Haleshroom", "A mushroom.", ItemRarity.Common, true),
            [SpellComponentType.Flatshroom] = ("Flatshroom", "A mushroom.", ItemRarity.Common, true),
            [SpellComponentType.Shieldshroom] = ("Shieldshroom", "A mushroom.", ItemRarity.Common, true),
            [SpellComponentType.Strengthshroom] = ("Strengthshroom", "A mushroom.", ItemRarity.Common, true),

            [SpellComponentType.Blackberries] = ("Blackberries", "A fruit.", ItemRarity.Common, true),
            [SpellComponentType.Blueberries] = ("Blueberries", "A fruit.", ItemRarity.Common, true),
            [SpellComponentType.Roseberries] = ("Roseberries", "A fruit.", ItemRarity.Common, true),
            [SpellComponentType.Nanaberries] = ("Nanaberries", "A fruit.", ItemRarity.Common, true),
            [SpellComponentType.Melonberries] = ("Melonberries", "A fruit.", ItemRarity.Common, true),
            [SpellComponentType.Appleberries] = ("Appleberries", "A fruit.", ItemRarity.Common, true),
            [SpellComponentType.Honeyberries] = ("Honeyberries", "A fruit.", ItemRarity.Common, true),

            [SpellComponentType.Berylflies] = ("Berylflies", "A bug.", ItemRarity.Uncommon, false),
            [SpellComponentType.Crystacricket] = ("Crystacricket", "A bug.", ItemRarity.Uncommon, false),
            [SpellComponentType.Rubeetles] = ("Rubeetles", "A bug.", ItemRarity.Uncommon, false),
            [SpellComponentType.Draviteflies] = ("Draviteflies", "A bug.", ItemRarity.Uncommon, false),
            [SpellComponentType.Amberbees] = ("Amberbees", "A bug.", ItemRarity.Uncommon, false),
            [SpellComponentType.Sapphider] = ("Sapphider", "A bug.", ItemRarity.Uncommon, false),
            [SpellComponentType.Opalant] = ("Opalant", "A bug.", ItemRarity.Uncommon, false),

            [SpellComponentType.ForestFern] = ("Forest Fern", "An herb.", ItemRarity.Common, true),
            [SpellComponentType.EmeraldFronds] = ("Emerald Fronds", "An herb.", ItemRarity.Common, true),
            [SpellComponentType.Lavenjade] = ("Lavenjade", "An herb.", ItemRarity.Common, true),
            [SpellComponentType.Kellycones] = ("Kellycones", "An herb.", ItemRarity.Common, true),
            [SpellComponentType.Kombuthyme] = ("Kombuthyme", "An herb.", ItemRarity.Common, true),
            [SpellComponentType.LaurelScales] = ("Laurel Scales", "An herb.", ItemRarity.Common, true),
            [SpellComponentType.Mossmary] = ("Mossmary", "An herb.", ItemRarity.Common, true),
            [SpellComponentType.MyrtleNeedles] = ("Myrtle Needles", "An herb.", ItemRarity.Common, true),
            [SpellComponentType.Min] = ("Min", "An herb.", ItemRarity.Common, true),
        };

        public SpellComponentType Type { get; }

        /// <summary>
        /// Whether a trader can stock this component for the player to buy.
        /// (Every item in the bag can still be sold back to a trader - that's IsSellable.)
        /// </summary>
        public bool IsPurchasable { get; }

        public SpellComponent(SpellComponentType type, int quantity = 1)
            : base(Data[type].Name, Data[type].Description, Data[type].Rarity, ItemCategory.SpellComponent, GetValue(Data[type].Rarity))
        {
            Type = type;
            IsPurchasable = Data[type].IsPurchasable;
            IsStackable = true;
            MaxStackSize = 99;
            Quantity = quantity;
        }

        public static string GetName(SpellComponentType type) => Data[type].Name;

        public static ItemRarity GetRarity(SpellComponentType type) => Data[type].Rarity;

        public static bool GetIsPurchasable(SpellComponentType type) => Data[type].IsPurchasable;

        /// <summary>
        /// Every component type, optionally only the ones a trader can stock
        /// </summary>
        public static List<SpellComponentType> GetAllTypes(bool purchasableOnly = false)
        {
            return Data.Keys.Where(t => !purchasableOnly || Data[t].IsPurchasable).ToList();
        }

        /// <summary>
        /// Find a component type by its display name (case-insensitive)
        /// </summary>
        public static SpellComponentType? FindByName(string name)
        {
            foreach (var entry in Data)
            {
                if (entry.Value.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    return entry.Key;
            }
            return null;
        }

        // Placeholder prices, to be tuned with the shops
        private static int GetValue(ItemRarity rarity) => rarity switch
        {
            ItemRarity.Common => 5,
            ItemRarity.Uncommon => 15,
            ItemRarity.Rare => 40,
            ItemRarity.Legendary => 100,
            _ => 5
        };

        /// <summary>
        /// Components change no stats - they are spent by casting spells
        /// </summary>
        public override void ModifyStat(Entities.Player.Player player)
        {
        }

        public override string GetInfo()
        {
            return base.GetInfo() + "Used by: spells\n" + (IsPurchasable ? "" : "Not sold by traders\n");
        }
    }
}
