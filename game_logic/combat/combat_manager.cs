using System;
using GameLogic.Entities.Player;
using GameLogic.Entities.Enemies;
using GameLogic.Systems;

namespace GameLogic.Combat
{
    /// <summary>
    /// Manages turn-based combat between player and enemies
    /// Handles combat flow, damage calculation, and turn order
    /// </summary>
    public class CombatManager
    {
        private RNGManager _rngManager;
        private Player _player;
        private Enemy _enemy;
        private int _turnCount;

        public CombatManager(RNGManager rngManager)
        {
            _rngManager = rngManager;
        }

        /// <summary>
        /// Start a combat encounter
        /// </summary>
        /// <returns>True if player won, False if player lost</returns>
        public bool StartCombat(Player player, Enemy enemy)
        {
            _player = player;
            _enemy = enemy;
            _turnCount = 0;

            Console.Clear();
            Console.WriteLine("=".PadRight(50, '='));
            Console.WriteLine($"    COMBAT: {_player.Name} vs {_enemy.Name}");
            Console.WriteLine("=".PadRight(50, '='));
            Console.WriteLine();

            // Main combat loop
            while (_player.Health > 0 && _enemy.Health > 0)
            {
                _turnCount++;
                Console.WriteLine($"\n--- Turn {_turnCount} ---");
                
                // Player turn
                PlayerTurn();
                
                if (_enemy.Health <= 0)
                {
                    return PlayerVictory();
                }

                // Enemy turn
                EnemyTurn();
                
                if (_player.Health <= 0)
                {
                    return PlayerDefeat();
                }

                // Show status after both turns
                ShowCombatStatus();
            }

            // Should never reach here, but just in case
            return _player.Health > 0;
        }

        /// <summary>
        /// Player's turn - choose action
        /// </summary>
        private void PlayerTurn()
        {
            Console.WriteLine($"\n{_player.Name}'s Turn!");
            Console.WriteLine($"Your HP: {_player.Health}/{_player.MaxHealth}");
            Console.WriteLine($"Enemy HP: {_enemy.Health}/{_enemy.MaxHealth}");
            
            Console.WriteLine("\nChoose your action:");
            Console.WriteLine("1. Attack");
            Console.WriteLine("2. Use Active Ability");
            Console.WriteLine("3. Use Item (TODO)");
            Console.WriteLine("4. Defend (reduce damage this turn)");
            Console.WriteLine("5. Try to Flee");

            Console.Write("\nChoice: ");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    PlayerAttack();
                    break;
                case "2":
                    UseActiveAbility();
                    break;
                case "3":
                    Console.WriteLine("Items not implemented yet. Attacking instead!");
                    PlayerAttack();
                    break;
                case "4":
                    PlayerDefend();
                    break;
                case "5":
                    AttemptFlee();
                    break;
                default:
                    Console.WriteLine("Invalid choice. Attacking by default.");
                    PlayerAttack();
                    break;
            }
        }

        /// <summary>
        /// Player uses their active ability (if they have one)
        /// Passive abilities don't need activation - they apply automatically
        /// </summary>
        private void UseActiveAbility()
        {
            // TODO: Check if player has an ability assigned
            // TODO: Check if ability REQUIRES ACTIVATION (active abilities only)
            // TODO: Execute the ability
            
            Console.WriteLine("\nActive abilities not fully implemented yet!");
            Console.WriteLine("For now, attacking instead...");
            PlayerAttack();
            
            // FUTURE IMPLEMENTATION:
            // if (_player.SelectedAbility != null && _player.SelectedAbility.RequiresActivation)
            // {
            //     // Player must use a turn to activate (heal, special attack, buff)
            //     _player.SelectedAbility.Execute(_player, _enemy);
            // }
            // else
            // {
            //     Console.WriteLine("You don't have an active ability to use!");
            //     Console.WriteLine("(Passive abilities apply automatically)");
            // }
        }

        /// <summary>
        /// Player defends - reduces incoming damage
        /// </summary>
        private void PlayerDefend()
        {
            Console.WriteLine($"\n{_player.Name} takes a defensive stance!");
            // TODO: Implement defense buff that reduces next attack damage
            // For now, just skip turn
        }

        /// <summary>
        /// Player attempts to flee from combat
        /// </summary>
        private void AttemptFlee()
        {
            int fleeChance = 30; // Base 30% chance
            int fleeRoll = _rngManager.Roll(1, 100);

            if (fleeRoll <= fleeChance)
            {
                Console.WriteLine($"\n{_player.Name} successfully fled from combat!");
                Console.WriteLine("You escaped, but gained no rewards...");
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
                
                // End combat as a "loss" (no XP/loot)
                _enemy.Health = 0; // Trick to end combat loop
                _player.Health = 1; // Keep player alive
            }
            else
            {
                Console.WriteLine($"\n{_player.Name} failed to escape!");
                Console.WriteLine("The enemy gets a free attack!");
                // Enemy gets bonus attack as punishment for failed flee
            }
        }

        /// <summary>
        /// Enemy's turn - AI decides action
        /// </summary>
        private void EnemyTurn()
        {
            Console.WriteLine($"\n{_enemy.Name}'s Turn!");

            // Simple AI: Always attack for now
            // TODO: Make AI use abilities, defend, etc.
            EnemyAttack();
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
        private bool PlayerVictory()
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

            // TODO: Roll for loot drops

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();

            return true; // Player won
        }

        /// <summary>
        /// Handle player defeat
        /// </summary>
        private bool PlayerDefeat()
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
        /// Calculate XP reward with more randomness
        /// </summary>
        private int CalculateXPReward()
        {
            // Base XP range scales with enemy level
            int minXP = _enemy.Level * 30;
            int maxXP = _enemy.Level * 70;
            
            // Add some randomness
            int baseXP = _rngManager.Roll(minXP, maxXP);
            
            // Bonus XP chance (10% chance for +50% XP)
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
        /// Calculate gold reward with more randomness
        /// </summary>
        private int CalculateGoldReward()
        {
            // Base gold range scales with enemy level
            int minGold = _enemy.Level * 3;
            int maxGold = _enemy.Level * 20;
            
            // Random gold within range
            int gold = _rngManager.Roll(minGold, maxGold);
            
            // Small chance for treasure bonus (5% chance for double gold)
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