using System;
using GameLogic.Entities;
using GameLogic.Abilities;
using GameLogic.Items;

namespace GameLogic.Combat
{
    /// <summary>
    /// Represents a combat action that can be taken by player or enemy
    /// This is a data structure - execution happens in CombatManager
    /// </summary>
    public class CombatAction
    {
        public ActionType Type { get; set; }
        public Entity Actor { get; set; }        // Who is performing the action
        public Entity Target { get; set; }       // Who is being targeted (can be null for defend/flee)

        // Optional data depending on action type
        public Ability Ability { get; set; }     // If using an ability
        public Item Item { get; set; }           // If using an item
        
        public CombatAction(ActionType type, Entity actor, Entity target = null)
        {
            Type = type;
            Actor = actor;
            Target = target;
        }
        
        // Convenience constructors for specific actions
        public static CombatAction Attack(Entity attacker, Entity target)
        {
            return new CombatAction(ActionType.Attack, attacker, target);
        }
        
        public static CombatAction UseAbility(Entity user, Ability ability, Entity target = null)
        {
            return new CombatAction(ActionType.UseAbility, user, target)
            {
                Ability = ability
            };
        }
        
        public static CombatAction UseItem(Entity user, Item item, Entity target = null)
        {
            return new CombatAction(ActionType.UseItem, user, target)
            {
                Item = item
            };
        }
        
        public static CombatAction Defend(Entity defender)
        {
            return new CombatAction(ActionType.Defend, defender);
        }
        
        public static CombatAction Flee(Entity actor)
        {
            return new CombatAction(ActionType.Flee, actor);
        }
    }
    
    /// <summary>
    /// All possible combat actions
    /// </summary>
    public enum ActionType
    {
        Attack,
        UseAbility,
        UseItem,
        Defend,
        Flee
    }
}