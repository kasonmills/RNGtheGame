using System;
using System.Collections.Generic;
using GameLogic.Entities.Player;
using GameLogic.Entities.Enemies;
using GameLogic.Systems;
using Enemy = GameLogic.Entities.Enemies.EnemyBase;

namespace GameLogic.Combat
{
    /// <summary>
    /// Orchestrates combat encounters between player and enemies
    /// Delegates to specialized managers for turn order, damage, and actions
    /// Uses a round-based system where each entity acts once per round
    /// </summary>
    public class CombatManager
    {
        private RNGManager _rngManager;
        private TurnManager _turnManager;
        private DamageCalculator _damageCalculator;

        private Player _player;
        private Enemy _enemy;
        private bool _combatActive;

        // Round-based combat tracking
        private int _currentRound;
        private List<Entity> _allCombatants;
        private HashSet<Entity> _entitiesActedThisRound;

        // Universal defend tracking - any entity can defend
        private Dictionary<Entity, bool> _defendingEntities;

        public CombatManager(RNGManager rngManager)
        {
            _rngManager = rngManager;
            _turnManager = new TurnManager();
            _damageCalculator = new DamageCalculator(rngManager);
            _defendingEntities = new Dictionary<Entity, bool>();
            _allCombatants = new List<Entity>();
            _entitiesActedThisRound = new HashSet<Entity>();
            _currentRound = 0;
        }

        /// <summary>
        /// Start a combat encounter
        /// </summary>
        /// <returns>True if player won, False if player lost</returns>
        public bool StartCombat(Player player, Enemy enemy)
        {
            _player = player;
            _enemy = enemy;
            _combatActive = true;

            // Setup combatants list
            _allCombatants.Clear();
            _allCombatants.Add(_player);
            _allCombatants.Add(_enemy);
            // TODO: Add companions here when implemented

            // Clear tracking structures
            _defendingEntities.Clear();
            _entitiesActedThisRound.Clear();
            _currentRound = 1;

            // Initialize turn manager
            _turnManager.InitializeCombat(player, enemy);

            // Display combat start
            ShowCombatIntro();

            // Main round-based combat loop
            while (_combatActive && _player.Health > 0 && _enemy.Health > 0)
            {
                // Start a new round
                StartNewRound();

                // Each entity gets one turn per round
                foreach (var entity in _allCombatants)
                {
                    // Skip if entity is dead
                    if (!entity.IsAlive())
                        continue;

                    // Skip if entity already acted this round
                    if (_entitiesActedThisRound.Contains(entity))
                        continue;

                    // Execute entity's turn
                    ExecuteEntityTurn(entity);

                    // Mark entity as having acted
                    _entitiesActedThisRound.Add(entity);

                    // Check for combat end after each action
                    if (_enemy.Health <= 0)
                    {
                        return HandleVictory();
                    }
                    else if (_player.Health <= 0)
                    {
                        return HandleDefeat();
                    }

                    // Show status between turns if combat is still active
                    if (_combatActive && entity.IsAlive())
                    {
                        ShowCombatStatus();
                    }

                    // If player or enemy fled, exit
                    if (!_combatActive)
                    {
                        return false;
                    }
                }

                // End of round - process end-of-round effects
                EndRound();
            }

            // Fled from combat or something else
            return false;
        }

        /// <summary>
        /// Display combat introduction
        /// </summary>
        private void ShowCombatIntro()
        {
            Console.Clear();
            Console.WriteLine("=".PadRight(50, '='));
            Console.WriteLine($"    COMBAT: {_player.Name} vs {_enemy.Name}");
            Console.WriteLine("=".PadRight(50, '='));
            Console.WriteLine($"\nEnemy Level: {_enemy.Level}");
            Console.WriteLine($"Enemy HP: {_enemy.Health}/{_enemy.MaxHealth}");
            Console.WriteLine();
        }

        /// <summary>
        /// Start a new round - reset tracking and display round info
        /// </summary>
        private void StartNewRound()
        {
            Console.WriteLine("\n" + "=".PadRight(50, '='));
            Console.WriteLine($"           ROUND {_currentRound}");
            Console.WriteLine("=".PadRight(50, '='));

            // Clear entities that acted this round
            _entitiesActedThisRound.Clear();

            // Sort combatants by speed (highest to lowest)
            // Entities with higher speed act first in the turn order
            _allCombatants.Sort((a, b) => b.Speed.CompareTo(a.Speed));

            // Display turn order for this round
            Console.WriteLine("\nTurn Order (by Speed):");
            foreach (var entity in _allCombatants)
            {
                if (entity.IsAlive())
                {
                    Console.WriteLine($"  {entity.Name} (Speed: {entity.Speed})");
                }
            }
            Console.WriteLine();

            // Reset defend status for all entities at start of new round
            foreach (var entity in _allCombatants)
            {
                if (_defendingEntities.ContainsKey(entity))
                {
                    _defendingEntities[entity] = false;
                }
            }
        }

        /// <summary>
        /// Execute a turn for the given entity
        /// </summary>
        private void ExecuteEntityTurn(Entity entity)
        {
            Console.WriteLine($"\n{entity.Name}'s Turn!");

            // Process status effects at start of turn
            entity.ProcessEffects(_rngManager);

            // Check if entity died from effects
            if (!entity.IsAlive())
            {
                Console.WriteLine($"{entity.Name} was defeated by status effects!");
                return;
            }

            // Determine and execute action based on entity type
            if (entity == _player)
            {
                PlayerTurn();
            }
            else if (entity == _enemy)
            {
                EnemyTurn();
            }
            // TODO: Add companion turn handling here
        }

        /// <summary>
        /// End the current round - process end-of-round effects
        /// </summary>
        private void EndRound()
        {
            Console.WriteLine("\n--- End of Round ---");

            // Tick down effect durations for all entities
            foreach (var entity in _allCombatants)
            {
                if (entity.IsAlive())
                {
                    entity.TickEffectDurations();
                }
            }

            // Reduce ability cooldowns for all entities at end of round
            if (_player.SelectedAbility != null && _player.SelectedAbility.CurrentCooldown > 0)
            {
                _player.SelectedAbility.CurrentCooldown--;
            }

            if (_enemy.SpecialAbility != null && _enemy.SpecialAbility.CurrentCooldown > 0)
            {
                _enemy.SpecialAbility.CurrentCooldown--;
            }

            // Increment round counter
            _currentRound++;

            Console.WriteLine("\nPress any key to continue to next round...");
            Console.ReadKey();
        }

        /// <summary>
        /// Handle the player's turn
        /// </summary>
        private void PlayerTurn()
        {
            Console.WriteLine($"Your HP: {_player.Health}/{_player.MaxHealth}");
            Console.WriteLine($"Enemy HP: {_enemy.Health}/{_enemy.MaxHealth}");

            // Get player's action choice
            CombatAction action = GetPlayerAction();

            // Process the action
            ProcessAction(action);
        }

        /// <summary>
        /// Get the player's chosen action
        /// </summary>
        private CombatAction GetPlayerAction()
        {
            Console.WriteLine("\nChoose your action:");
            Console.WriteLine("1. Attack");
            Console.WriteLine("2. Use Active Ability");
            Console.WriteLine("3. Use Item");
            Console.WriteLine("4. Defend");
            Console.WriteLine("5. Try to Flee");

            Console.Write("\nChoice: ");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    return CombatAction.Attack(_player, _enemy);
                case "2":
                    // Use player's selected ability
                    if (_player.SelectedAbility != null)
                    {
                        // Check if ability is on cooldown
                        if (_player.SelectedAbility.CurrentCooldown > 0)
                        {
                            Console.WriteLine($"\n{_player.SelectedAbility.Name} is on cooldown for {_player.SelectedAbility.CurrentCooldown} more rounds!");
                            Console.WriteLine("Attacking instead.");
                            return CombatAction.Attack(_player, _enemy);
                        }

                        return CombatAction.UseAbility(_player, _enemy, _player.SelectedAbility);
                    }
                    else
                    {
                        Console.WriteLine("\nYou don't have an ability selected! Attacking instead.");
                        return CombatAction.Attack(_player, _enemy);
                    }
                case "3":
                    // Use item from inventory
                    if (_player.Inventory.Items.Count == 0)
                    {
                        Console.WriteLine("\nYour inventory is empty! Attacking instead.");
                        return CombatAction.Attack(_player, _enemy);
                    }

                    // Show inventory
                    Console.WriteLine("\nYour Inventory:");
                    for (int i = 0; i < _player.Inventory.Items.Count; i++)
                    {
                        Console.WriteLine($"{i + 1}. {_player.Inventory.Items[i].Name}");
                    }
                    Console.WriteLine($"{_player.Inventory.Items.Count + 1}. Cancel");

                    Console.Write("\nSelect item: ");
                    string itemChoice = Console.ReadLine();

                    if (int.TryParse(itemChoice, out int itemIndex) && itemIndex > 0 && itemIndex <= _player.Inventory.Items.Count)
                    {
                        var item = _player.Inventory.Items[itemIndex - 1];
                        return CombatAction.UseItem(_player, item);
                    }
                    else
                    {
                        Console.WriteLine("Invalid choice. Attacking instead!");
                        return CombatAction.Attack(_player, _enemy);
                    }
                case "4":
                    return CombatAction.Defend(_player);
                case "5":
                    return CombatAction.Flee(_player);
                default:
                    Console.WriteLine("Invalid choice. Attacking by default.");
                    return CombatAction.Attack(_player, _enemy);
            }
        }

        /// <summary>
        /// Handle the enemy's turn
        /// Uses AI to decide action based on behavior and situation
        /// </summary>
        private void EnemyTurn()
        {
            // Determine action based on enemy behavior and situation
            CombatAction action = DetermineEnemyAction();

            ProcessAction(action);
        }

        /// <summary>
        /// Determine enemy action based on AI behavior
        /// </summary>
        private CombatAction DetermineEnemyAction()
        {
            // Calculate health percentage
            float healthPercent = (float)_enemy.Health / _enemy.MaxHealth;

            // Check if enemy has a special ability and can use it
            bool canUseAbility = _enemy.SpecialAbility != null &&
                                 _enemy.SpecialAbility.CurrentCooldown <= 0;

            // Decision logic based on behavior type
            switch (_enemy.Behavior)
            {
                case Entities.Enemies.EnemyBehavior.Aggressive:
                    // Always attack, use ability when available
                    if (canUseAbility && _rngManager.Roll(1, 100) <= 70)
                    {
                        return CombatAction.UseAbility(_enemy, _player, _enemy.SpecialAbility);
                    }
                    return CombatAction.Attack(_enemy, _player);

                case Entities.Enemies.EnemyBehavior.Defensive:
                    // Defend when low on health
                    if (healthPercent < 0.3f && _rngManager.Roll(1, 100) <= 60)
                    {
                        return CombatAction.Defend(_enemy);
                    }
                    if (canUseAbility && _rngManager.Roll(1, 100) <= 40)
                    {
                        return CombatAction.UseAbility(_enemy, _player, _enemy.SpecialAbility);
                    }
                    return CombatAction.Attack(_enemy, _player);

                case Entities.Enemies.EnemyBehavior.Tactical:
                    // Use abilities strategically
                    if (canUseAbility && _rngManager.Roll(1, 100) <= 80)
                    {
                        return CombatAction.UseAbility(_enemy, _player, _enemy.SpecialAbility);
                    }
                    if (healthPercent < 0.25f && _rngManager.Roll(1, 100) <= 50)
                    {
                        return CombatAction.Defend(_enemy);
                    }
                    return CombatAction.Attack(_enemy, _player);

                case Entities.Enemies.EnemyBehavior.Berserker:
                    // Always attack, rarely defend, use abilities aggressively
                    if (canUseAbility && _rngManager.Roll(1, 100) <= 90)
                    {
                        return CombatAction.UseAbility(_enemy, _player, _enemy.SpecialAbility);
                    }
                    return CombatAction.Attack(_enemy, _player);

                case Entities.Enemies.EnemyBehavior.Cautious:
                    // Defend frequently when low health, flee if very low
                    if (healthPercent < 0.15f && _rngManager.Roll(1, 100) <= 30)
                    {
                        return CombatAction.Flee(_enemy);
                    }
                    if (healthPercent < 0.4f && _rngManager.Roll(1, 100) <= 70)
                    {
                        return CombatAction.Defend(_enemy);
                    }
                    if (canUseAbility && _rngManager.Roll(1, 100) <= 50)
                    {
                        return CombatAction.UseAbility(_enemy, _player, _enemy.SpecialAbility);
                    }
                    return CombatAction.Attack(_enemy, _player);

                case Entities.Enemies.EnemyBehavior.Balanced:
                default:
                    // Mix of all actions
                    if (healthPercent < 0.3f && _rngManager.Roll(1, 100) <= 40)
                    {
                        return CombatAction.Defend(_enemy);
                    }
                    if (canUseAbility && _rngManager.Roll(1, 100) <= 60)
                    {
                        return CombatAction.UseAbility(_enemy, _player, _enemy.SpecialAbility);
                    }
                    return CombatAction.Attack(_enemy, _player);
            }
        }

        /// <summary>
        /// Process a combat action
        /// </summary>
        private void ProcessAction(CombatAction action)
        {
            switch (action.Type)
            {
                case ActionType.Attack:
                    ProcessAttack(action);
                    break;
                case ActionType.UseAbility:
                    ProcessAbility(action);
                    break;
                case ActionType.UseItem:
                    ProcessItem(action);
                    break;
                case ActionType.Defend:
                    ProcessDefend(action);
                    break;
                case ActionType.Flee:
                    ProcessFlee(action);
                    break;
            }
        }

        /// <summary>
        /// Process an attack action
        /// </summary>
        private void ProcessAttack(CombatAction action)
        {
            Console.WriteLine($"\n{action.Actor.Name} attacks {action.Target.Name}!");

            DamageResult result;

            // Calculate damage based on attacker type
            if (action.Actor == _player)
            {
                result = _damageCalculator.CalculatePlayerAttackDamage(_player, _enemy);

                // Check if player is unarmed
                if (_player.EquippedWeapon == null)
                {
                    Console.WriteLine("(Fighting unarmed - find a weapon!)");
                }

                // Apply defense reduction if enemy is defending
                if (_defendingEntities.ContainsKey(_enemy) && _defendingEntities[_enemy])
                {
                    int damageReduction = result.FinalDamage / 2;
                    result.FinalDamage -= damageReduction;
                    Console.WriteLine($"{_enemy.Name}'s defensive stance reduced damage by {damageReduction}!");
                }
            }
            else
            {
                result = _damageCalculator.CalculateEnemyAttackDamage(_enemy, _player);

                // Apply defense reduction if player is defending
                if (action.Target == _player && _defendingEntities.ContainsKey(_player) && _defendingEntities[_player])
                {
                    int damageReduction = result.FinalDamage / 2;
                    result.FinalDamage -= damageReduction;
                    Console.WriteLine($"Defensive stance reduced damage by {damageReduction}!");
                }
            }

            // Display result
            if (result.Missed)
            {
                Console.WriteLine("The attack missed!");
            }
            else
            {
                if (result.IsCritical)
                {
                    Console.WriteLine("CRITICAL HIT!");
                }

                if (result.DamageReduced > 0)
                {
                    // Display appropriate message based on who was attacked
                    if (action.Target == _player)
                    {
                        Console.WriteLine($"Armor blocked {result.DamageReduced} damage!");
                    }
                    else
                    {
                        Console.WriteLine($"Defense blocked {result.DamageReduced} damage!");
                    }
                }

                // Apply damage
                action.Target.TakeDamage(result.FinalDamage);
                Console.WriteLine($"{action.Target.Name} took {result.FinalDamage} damage!");
            }
        }

        /// <summary>
        /// Process an ability action
        /// </summary>
        private void ProcessAbility(CombatAction action)
        {
            if (action.Ability != null)
            {
                // Execute the ability
                action.Ability.Execute(action.Actor, action.Target, _rngManager);
            }
            else
            {
                Console.WriteLine("Error: No ability provided!");
            }
        }

        /// <summary>
        /// Process an item action
        /// </summary>
        private void ProcessItem(CombatAction action)
        {
            if (action.Item != null)
            {
                // Check if it's a consumable (most combat items)
                if (action.Item is Items.Consumable consumable)
                {
                    // Special handling for combat consumables
                    if (consumable.Type == Items.ConsumableType.Bomb)
                    {
                        // Apply bomb damage to enemy
                        Console.WriteLine($"\n{action.Actor.Name} throws a {consumable.Name}!");
                        _enemy.TakeDamage(consumable.EffectPower);
                        Console.WriteLine($"The explosion dealt {consumable.EffectPower} damage to {_enemy.Name}!");
                        consumable.RemoveFromStack(1);
                    }
                    else
                    {
                        // Use consumable normally (healing, etc.)
                        consumable.Use(_player);
                    }

                    // Remove from inventory if stack is empty
                    if (consumable.Quantity <= 0)
                    {
                        _player.Inventory.RemoveItem(consumable);
                    }
                }
                else
                {
                    // For non-consumable items, just use them
                    action.Item.Use(_player);
                }
            }
            else
            {
                Console.WriteLine("Error: No item provided!");
            }
        }

        /// <summary>
        /// Process a defend action
        /// </summary>
        private void ProcessDefend(CombatAction action)
        {
            Console.WriteLine($"\n{action.Actor.Name} takes a defensive stance!");

            // Set defending flag for any entity
            _defendingEntities[action.Actor] = true;

            if (action.Actor == _player)
            {
                Console.WriteLine("Next incoming attack will deal reduced damage!");
            }
            else
            {
                Console.WriteLine($"{action.Actor.Name} braces for impact!");
            }
        }

        /// <summary>
        /// Process a flee action
        /// </summary>
        private void ProcessFlee(CombatAction action)
        {
            int fleeChance = 30; // Base 30% chance
            int fleeRoll = _rngManager.Roll(1, 100);

            if (fleeRoll <= fleeChance)
            {
                Console.WriteLine($"\n{action.Actor.Name} successfully fled from combat!");
                Console.WriteLine("You escaped, but gained no rewards...");
                Console.WriteLine($"Remaining HP: {_player.Health}/{_player.MaxHealth}");

                // Reset combat usage for abilities
                if (_player.SelectedAbility != null)
                {
                    _player.SelectedAbility.ResetCombatUsage();
                }

                _combatActive = false;

                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
            }
            else
            {
                Console.WriteLine($"\n{action.Actor.Name} failed to escape!");
                Console.WriteLine("Turn wasted!");
            }
        }

        /// <summary>
        /// Show current combat status
        /// </summary>
        private void ShowCombatStatus()
        {
            Console.WriteLine("\n" + "-".PadRight(50, '-'));
            Console.WriteLine($"Player HP: {_player.Health}/{_player.MaxHealth}");
            Console.WriteLine($"Enemy HP: {_enemy.Health}/{_enemy.MaxHealth}");
            Console.WriteLine("-".PadRight(50, '-'));
            
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        /// <summary>
        /// Handle player victory
        /// </summary>
        private bool HandleVictory()
        {
            Console.Clear();
            Console.WriteLine("\n" + "=".PadRight(50, '='));
            Console.WriteLine("           VICTORY!");
            Console.WriteLine("=".PadRight(50, '='));

            // Calculate rewards
            int xpGained = CalculateXPReward();
            int goldGained = CalculateGoldReward();

            Console.WriteLine($"\n{_enemy.Name} has been defeated!");
            Console.WriteLine($"\nYou gained {xpGained} XP!");
            Console.WriteLine($"You gained {goldGained} gold!");

            // Apply rewards
            _player.AddExperience(xpGained);
            _player.Gold += goldGained;

            // Roll for loot drops
            var lootDrops = _enemy.GetLootDrops(_rngManager);
            if (lootDrops.Count > 0)
            {
                Console.WriteLine("\n--- LOOT DROPS ---");
                foreach (var item in lootDrops)
                {
                    _player.AddToInventory(item);
                    Console.WriteLine($"Obtained: {item.GetDisplayName()}");
                }
            }
            else
            {
                Console.WriteLine("\nNo items dropped.");
            }

            // Reset combat usage for abilities
            if (_player.SelectedAbility != null)
            {
                _player.SelectedAbility.ResetCombatUsage();
            }

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();

            return true; // Player won
        }

        /// <summary>
        /// Handle player defeat
        /// </summary>
        private bool HandleDefeat()
        {
            Console.Clear();
            Console.WriteLine("\n" + "=".PadRight(50, '='));
            Console.WriteLine("           DEFEAT");
            Console.WriteLine("=".PadRight(50, '='));

            Console.WriteLine($"\n{_player.Name} has been defeated by {_enemy.Name}...");
            Console.WriteLine("\nGAME OVER");

            // Reset combat usage for abilities
            if (_player.SelectedAbility != null)
            {
                _player.SelectedAbility.ResetCombatUsage();
            }

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();

            return false; // Player lost
        }

        /// <summary>
        /// Calculate XP reward with randomness
        /// </summary>
        private int CalculateXPReward()
        {
            int minXP = _enemy.Level * 30;
            int maxXP = _enemy.Level * 70;
            
            int baseXP = _rngManager.Roll(minXP, maxXP);
            
            // 10% chance for bonus XP
            int bonusRoll = _rngManager.Roll(1, 100);
            if (bonusRoll <= 10)
            {
                int bonus = baseXP / 2;
                Console.WriteLine($"Bonus XP! +{bonus}");
                baseXP += bonus;
            }
            
            return baseXP;
        }

        /// <summary>
        /// Calculate gold reward with randomness
        /// </summary>
        private int CalculateGoldReward()
        {
            int minGold = _enemy.Level * 3;
            int maxGold = _enemy.Level * 20;
            
            int gold = _rngManager.Roll(minGold, maxGold);
            
            // 5% chance for treasure bonus
            int treasureRoll = _rngManager.Roll(1, 100);
            if (treasureRoll <= 5)
            {
                Console.WriteLine("The enemy dropped extra gold!");
                gold *= 2;
            }
            
            return gold;
        }
    }
}