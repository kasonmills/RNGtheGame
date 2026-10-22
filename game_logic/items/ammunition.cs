using System;

namespace GameLogic.Items
{
    /// <summary>
    /// Every special crossbow bolt type (Boon's special shots). Ordinary bolts are unlimited
    /// and aren't items. More types may be added later.
    /// </summary>
    public enum BoltType
    {
        Piercing,   // Ignores the target's defense
        Pinning,    // Slows the target so it acts later in the round
        Fire,       // Sets the target burning (damage over time)
        Ice,        // Effect not designed yet - hits like an ordinary bolt for now
        Acid        // Effect not designed yet - hits like an ordinary bolt for now
    }

    /// <summary>
    /// Special crossbow bolts - stackable items kept in the shared party bag. Boon fires one
    /// per special shot. Not usable as a combat item on their own.
    /// </summary>
    public class Ammunition : Item
    {
        public BoltType Type { get; }

        public Ammunition(BoltType type, int quantity = 1)
            : base(GetName(type), GetDescription(type), ItemRarity.Common, ItemCategory.Ammunition, GetValue(type))
        {
            Type = type;
            IsStackable = true;
            MaxStackSize = 99;
            Quantity = quantity;
        }

        public static string GetName(BoltType type) => type switch
        {
            BoltType.Piercing => "Piercing Bolt",
            BoltType.Pinning => "Pinning Bolt",
            BoltType.Fire => "Fire Bolt",
            BoltType.Ice => "Ice Bolt",
            BoltType.Acid => "Acid Bolt",
            _ => "Bolt"
        };

        private static string GetDescription(BoltType type) => type switch
        {
            BoltType.Piercing => "A hardened bolt that punches straight through armor. Ignores the target's defense.",
            BoltType.Pinning => "A barbed bolt that pins the target in place, slowing it down.",
            BoltType.Fire => "A bolt wrapped in burning pitch. Sets the target on fire.",
            BoltType.Ice => "A bolt tipped with frost.",
            BoltType.Acid => "A bolt carrying a vial of acid.",
            _ => "A crossbow bolt."
        };

        // Placeholder prices, to be tuned with the shops
        private static int GetValue(BoltType type) => type switch
        {
            BoltType.Piercing => 8,
            BoltType.Pinning => 6,
            BoltType.Fire => 10,
            BoltType.Ice => 10,
            BoltType.Acid => 10,
            _ => 1
        };

        /// <summary>
        /// Bolts change no stats - they are spent by Boon's special shot
        /// </summary>
        public override void ModifyStat(Entities.Player.Player player)
        {
        }

        public override string GetInfo()
        {
            return base.GetInfo() + "Used by: Boon's special shot\n";
        }
    }
}
