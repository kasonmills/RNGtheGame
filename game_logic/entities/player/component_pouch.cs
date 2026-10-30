using System;
using System.Collections.Generic;
using System.Linq;
using GameLogic.Items;

namespace GameLogic.Entities.Player
{
    /// <summary>
    /// The component pouch - spell components are kept here instead of the main bag, so they
    /// never take inventory slots. There is no limit on how many component types it holds;
    /// each type is one stack of at most 99 (decided 2026-10-30).
    /// PlayerInventory.AddItem routes components here automatically.
    /// </summary>
    public class ComponentPouch
    {
        public const int MaxPerComponent = 99;

        private readonly List<SpellComponent> _stacks = new List<SpellComponent>();

        /// <summary>
        /// Every component stack in the pouch (one per component type)
        /// </summary>
        public IReadOnlyList<SpellComponent> Stacks => _stacks;

        /// <summary>
        /// Whether the whole stack fits without going over 99
        /// </summary>
        public bool CanAdd(SpellComponent component)
        {
            return component != null && GetCount(component.Type) + component.Quantity <= MaxPerComponent;
        }

        /// <summary>
        /// Add a component stack to the pouch - all of it or none of it. If it would take the
        /// stack over 99, nothing is added and it returns false (decided 2026-10-30: the
        /// player can only carry 99, the rest is left where it was found).
        /// </summary>
        public bool Add(SpellComponent component)
        {
            if (component == null || component.Quantity <= 0 || !CanAdd(component))
                return false;

            var stack = GetStack(component.Type);
            if (stack == null)
                _stacks.Add(new SpellComponent(component.Type, component.Quantity));
            else
                stack.Quantity += component.Quantity;

            return true;
        }

        /// <summary>
        /// Remove some of a component (e.g. spent casting a spell, or sold). Returns false and
        /// removes nothing if there aren't enough.
        /// </summary>
        public bool Remove(SpellComponentType type, int amount)
        {
            var stack = GetStack(type);
            if (stack == null || amount <= 0 || stack.Quantity < amount)
                return false;

            stack.Quantity -= amount;
            if (stack.Quantity <= 0)
                _stacks.Remove(stack);
            return true;
        }

        /// <summary>
        /// How many of a component the pouch holds (0 if none)
        /// </summary>
        public int GetCount(SpellComponentType type)
        {
            return GetStack(type)?.Quantity ?? 0;
        }

        /// <summary>
        /// Whether the pouch holds at least this many of a component
        /// </summary>
        public bool Has(SpellComponentType type, int amount = 1)
        {
            return GetCount(type) >= amount;
        }

        /// <summary>
        /// The stack for a component type, or null if the pouch has none
        /// </summary>
        public SpellComponent GetStack(SpellComponentType type)
        {
            return _stacks.FirstOrDefault(s => s.Type == type);
        }

        public void Clear()
        {
            _stacks.Clear();
        }
    }
}
