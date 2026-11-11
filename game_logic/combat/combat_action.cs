using System;
using System.Collections.Generic;
using GameLogic.Entities;
using GameLogic.Entities.Enemies;
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

        // Optional, generically reusable by any boss mechanic:
        public double DamageMultiplier { get; set; } = 1.0;  // Applied to attack damage before defend/evasion (e.g. a charge attack)
        public AbilityEffect AppliesEffect { get; set; }      // Applied to the target if the attack lands (e.g. a slow/grapple)
        public bool EffectEvenWithoutDamage { get; set; }     // AppliesEffect lands on a hit that does 0 damage too (the Demon Prince) - never on a miss
        public bool IsMagical { get; set; }                   // A magic attack - armor-based defenses (Risk's no-damage chance) don't apply
        public bool IgnoresDefense { get; set; }              // Skips the target's defense/armor reduction (e.g. a piercing bolt)

        // For Summon: the enemies brought into the fight (e.g. the Princess's castle guards)
        public List<EnemyBase> SummonedEnemies { get; set; }

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

        /// <summary>
        /// No offensive action this turn (e.g. a boss occupied holding a grapple).
        /// </summary>
        public static CombatAction None(Entity actor)
        {
            return new CombatAction(ActionType.None, actor);
        }

        /// <summary>
        /// Enemy-only: put an effect on a target with no attack roll (e.g. the Princess's
        /// "force a skip" with a SkipTurnEffect). The effect goes in AppliesEffect.
        /// </summary>
        public static CombatAction Inflict(Entity actor, Entity target, AbilityEffect effect)
        {
            return new CombatAction(ActionType.InflictEffect, actor, target)
            {
                AppliesEffect = effect
            };
        }

        /// <summary>
        /// Enemy-only: bring new enemies into the fight on the actor's side (e.g. the
        /// Princess calling two castle guards). They act from the next round.
        /// </summary>
        public static CombatAction Summon(Entity actor, List<EnemyBase> enemies)
        {
            return new CombatAction(ActionType.Summon, actor)
            {
                SummonedEnemies = enemies
            };
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
        Flee,
        None,

        // Enemy-only (bosses) - the player can't submit these (ValidateAction refuses them)
        InflictEffect,  // Put AppliesEffect on Target, no attack roll
        Summon          // Bring SummonedEnemies into the fight
    }
}