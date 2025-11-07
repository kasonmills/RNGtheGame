using System;
using GameLogic.Entities.Player;
using GameLogic.Entities.Enemies;
using GameLogic.Systems;

namespace GameLogic.Combat
{
    /// <summary>
    /// Orchestrates combat encounters between player and enemies
    /// Delegates to specialized managers for turn order, damage, and actions
    /// </summary>
    public class CombatManager
    {
        private RNGManager _rngManager;
        private TurnManager _turnManager;
        private DamageCalculator _damageCalculator;
        
        private Player _player;
        private Enemy _enemy;
        private bool _combatActive;
        private bool _playerDefending;

        public CombatManager(RNGManager rngManager)
        {
            _rngManager = rngManager;
            _turnManager = new TurnManager();
            _damageCalculator = new DamageCalculator(rngManager);
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
            _playerDefending = false;

            // Initialize turn manager
            _turnManager.InitializeCombat(player, enemy);

            // Display combat start
            ShowCombatIntro();

            // Main combat loop
            while (_combatActive && _player.Health > 0 && _enemy.Health > 0)
            {
                Console.WriteLine($"\n{_turnManager.GetTurnStatus()}");
                
                if (_turnManager.IsPlayerTurn())
                {
                    PlayerTurn();
                }
                else
                {
                    EnemyTurn();
                }

                // Check for combat end
                if (_enemy.Health <= 0)
                {
                    return HandleVictory();
                }
                else if (_player.Health <= 0)
                {
                    return HandleDefeat();
                }

                _turnManager.NextTurn();
                
                // Show status between turns
                if (_combatActive)
                {
                    ShowCombatStatus();
                }
            }

            // Fled from combat
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
        /// Handle the player's turn
        /// </summary>
        private void PlayerTurn()
        {
            // Reset defense status at start of turn
            _playerDefending = false;

            Console.WriteLine($"\n{_player.Name}'s Turn!");
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
            Console.WriteLine("3. Use Item (TODO)");
            Console.WriteLine("4. Defend");
            Console.WriteLine("5. Try to Flee");

            Console.Write("\nChoice: ");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    return CombatAction.Attack(_player, _enemy);
                case "2":
                    // TODO: Implement ability selection
                    Console.WriteLine("\nActive abilities not implemented yet. Attacking instead!");
                    return CombatAction.Attack(_player, _enemy);
                case "3":
                    // TODO: Implement item selection
                    Console.WriteLine("\nItems not implemented yet. Attacking instead!");
                    return CombatAction.Attack(_player, _enemy);
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
        /// </summary>
        private void EnemyTurn()
        {
            Console.WriteLine($"\n{_enemy.Name}'s Turn!");

            // Simple AI: Always attack
            // TODO: Implement more sophisticated AI (use abilities, etc.)
            CombatAction action = CombatAction.Attack(_enemy, _player);
            
            ProcessAction(action);
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
            }
            else
            {
                result = _damageCalculator.CalculateEnemyAttackDamage(_enemy, _player);
                
                // Apply defense reduction if player is defending
                if (_playerDefending && action.Target == _player)
                {
                    result.FinalDamage = result.FinalDamage / 2;
                    Console.WriteLine($"Defensive stance reduced damage by {result.FinalDamage}!");
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
                    Console.WriteLine($"Armor blocked {result.DamageReduced} damage!");
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
            // TODO: Implement ability processing
            // action.Ability.Execute(action.Actor, action.Target);
            Console.WriteLine("Abilities not fully implemented yet!");
        }

        /// <summary>
        /// Process an item action
        /// </summary>
        private void ProcessItem(CombatAction action)
        {
            // TODO: Implement item processing
            Console.WriteLine("Items not implemented yet!");
        }

        /// <summary>
        /// Process a defend action
        /// </summary>
        private void ProcessDefend(CombatAction action)
        {
            Console.WriteLine($"\n{action.Actor.Name} takes a defensive stance!");
            
            if (action.Actor == _player)
            {
                _playerDefending = true;
                Console.WriteLine("Next incoming attack will deal reduced damage!");
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

            // TODO: Roll for loot drops using LootTable

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