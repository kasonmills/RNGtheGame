using System;
using System.Collections.Generic;
using GameLogic.Entities.Player;
using GameLogic.World;
using GameLogic.Systems;
using GameLogic.Combat;
using GameLogic.Data;

namespace GameLogic.Core
{
    /// <summary>
    /// Central coordinator for the entire game. Manages game state, initializes systems,
    /// and runs the main game loop.
    /// </summary>
    public class GameManager
    {
        // === Core Systems ===
        private Player _player;
        private MapManager _mapManager;
        private CombatManager _combatManager;
        private RNGManager _rngManager;
        
        // === Game State Management ===
        private GameState _currentState;
        private Stack<GameState> _stateStack; // Remember previous states
        private bool _isRunning;

        // === Constructor ===
        public GameManager()
        {
            _stateStack = new Stack<GameState>();
            InitializeSystems();
        }

        /// <summary>
        /// Initialize all game systems
        /// </summary>
        private void InitializeSystems()
        {
            Console.WriteLine("Initializing game systems...");

            _rngManager = new RNGManager();
            Data.SaveManager.Initialize(); // Initialize static SaveManager
            _mapManager = new MapManager();
            _combatManager = new CombatManager(_rngManager);

            _currentState = GameState.MainMenu;
            _isRunning = false;

            Console.WriteLine("Systems initialized!\n");
        }

        /// <summary>
        /// Change to a new state, pushing current state onto stack
        /// Use this when you want to be able to return to previous state
        /// </summary>
        public void PushState(GameState newState)
        {
            _stateStack.Push(_currentState);
            _currentState = newState;
        }

        /// <summary>
        /// Return to the previous state
        /// </summary>
        public void PopState()
        {
            if (_stateStack.Count > 0)
            {
                _currentState = _stateStack.Pop();
            }
        }

        /// <summary>
        /// Change to a new state without saving previous state
        /// Use this for one-way transitions (like MainMenu -> Playing)
        /// </summary>
        public void ChangeState(GameState newState)
        {
            _currentState = newState;
        }

        /// <summary>
        /// Let the player choose their permanent starting ability
        /// </summary>
        private void ChooseStartingAbility()
        {
            Console.WriteLine("\n=== Choose Your Ability ===");
            Console.WriteLine("This choice is permanent and will stay with you throughout the game!");
            Console.WriteLine();

            var abilities = Player.GetAvailableAbilities();

            // Display all available abilities with descriptions
            for (int i = 0; i < abilities.Length; i++)
            {
                Console.WriteLine($"{i + 1}. {abilities[i].Name}");
                Console.WriteLine($"   {abilities[i].Description}");
                Console.WriteLine($"   {abilities[i].GetInfo()}");
                Console.WriteLine();
            }

            // Get player choice
            int choice = -1;
            while (choice < 1 || choice > abilities.Length)
            {
                Console.Write($"Choose your ability (1-{abilities.Length}): ");
                string input = Console.ReadLine();

                if (int.TryParse(input, out choice) && choice >= 1 && choice <= abilities.Length)
                {
                    break;
                }

                Console.WriteLine("Invalid choice. Please try again.");
            }

            // Set the chosen ability
            _player.SetAbility(abilities[choice - 1]);
        }

        /// <summary>
        /// Start a brand new game
        /// </summary>
        public void StartNewGame()
        {
            Console.Clear();
            Console.WriteLine("=== Welcome to RNG: The Game ===\n");
            
            // Get player name
            Console.Write("Enter your character's name: ");
            string playerName = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(playerName))
            {
                playerName = "Hero";
            }
            
            // Create new player
            _player = new Player(playerName);

            Console.WriteLine($"\nWelcome, {_player.Name}!");

            // Let player choose starting ability
            ChooseStartingAbility();

            Console.WriteLine("\nYour adventure begins...\n");
            
            // Generate starting map
            _mapManager.GenerateNewMap();
            
            ChangeState(GameState.Playing);
            _isRunning = true;
        }

        /// <summary>
        /// Load an existing saved game
        /// </summary>
        public void LoadGame()
        {
            Console.WriteLine("Loading saved game...");

            SaveData saveData = Data.SaveManager.LoadGame("save1"); // Default save slot

            if (saveData != null)
            {
                _player = Player.LoadFromSave(saveData);

                // Load/regenerate map based on saved seed
                if (saveData.MapSeed != 0)
                {
                    _mapManager.GenerateMapFromSeed(saveData.MapSeed);
                }
                else
                {
                    // Fallback: generate new map if no seed saved
                    _mapManager.GenerateNewMap();
                }

                ChangeState(GameState.Playing);
                _isRunning = true;

                Console.WriteLine($"Welcome back, {_player.Name}!");
            }
            else
            {
                Console.WriteLine("\nNo save file found or load failed.");
                Console.WriteLine("Returning to main menu...\n");
                ChangeState(GameState.MainMenu);
            }
        }

        /// <summary>
        /// Main game loop - runs until player quits
        /// </summary>
        public void Run()
        {
            while (_isRunning)
            {
                switch (_currentState)
                {
                    case GameState.MainMenu:
                        ShowMainMenu();
                        break;
                    
                    case GameState.Playing:
                        GameLoop();
                        break;
                    
                    case GameState.Combat:
                        // Combat is handled by CombatManager
                        // When combat ends, we return to Playing state
                        break;
                    
                    case GameState.Paused:
                        ShowPauseMenu();
                        break;
                    
                    case GameState.GameOver:
                        HandleGameOver();
                        break;
                }
            }
            
            Console.WriteLine("\nThanks for playing!");
        }

        /// <summary>
        /// Main gameplay loop - exploration and navigation
        /// </summary>
        private void GameLoop()
        {
            Console.WriteLine("\n=== Current Status ===");
            Console.WriteLine($"Location: {_mapManager.GetCurrentLocationName()}");
            Console.WriteLine($"Health: {_player.Health}/{_player.MaxHealth}");
            Console.WriteLine($"Gold: {_player.Gold}");
            Console.WriteLine($"Level: {_player.Level}");
            
            Console.WriteLine("\n=== What do you want to do? ===");
            Console.WriteLine("1. Explore (encounter enemies/find loot)");
            Console.WriteLine("2. View Inventory");
            Console.WriteLine("3. View Stats");
            Console.WriteLine("4. Rest (restore health)");
            Console.WriteLine("5. Save Game");
            Console.WriteLine("6. Pause Menu");
            Console.WriteLine("7. Quit");
            
            Console.Write("\nChoice: ");
            string choice = Console.ReadLine();
            
            switch (choice)
            {
                case "1":
                    Explore();
                    break;
                case "2":
                    ShowInventory();
                    break;
                case "3":
                    ShowStats();
                    break;
                case "4":
                    Rest();
                    break;
                case "5":
                    SaveGame();
                    break;
                case "6":
                    PushState(GameState.Paused); // Save current state and go to pause
                    break;
                case "7":
                    _isRunning = false;
                    break;
                default:
                    Console.WriteLine("Invalid choice. Try again.");
                    break;
            }
        }

        /// <summary>
        /// Explore the current area - will be replaced by movement system in 2D
        /// For text-based: simplified encounter trigger
        /// For Godot: this becomes player movement detection of events on map
        /// </summary>
        private void Explore()
        {
            // === TEXT-BASED VERSION (comment out when moving to Godot) ===
            Console.WriteLine("\nYou venture deeper into the unknown...\n");
            
            // Simple encounter roll for console testing
            int encounterRoll = _rngManager.Roll(1, 100);
            
            if (encounterRoll <= 60) // 60% chance for combat
            {
                TriggerCombatEncounter();
            }
            else if (encounterRoll <= 85) // 25% chance for loot
            {
                TriggerLootEvent();
            }
            else // 15% chance for nothing
            {
                Console.WriteLine("You find nothing of interest...");
            }
            // === END TEXT-BASED VERSION ===
            
            // === FOR GODOT: This method will be called by MapManager when player
            // === moves to a new node/location with an event
        }

        /// <summary>
        /// Spawn an enemy based on player level
        /// </summary>
        private Entities.Enemies.EnemyBase SpawnEnemy()
        {
            // Enemy level should be close to player level
            int enemyLevel = Math.Max(1, _player.Level + _rngManager.Roll(-1, 2));

            // Roll for enemy type based on rarity
            int roll = _rngManager.Roll(1, 100);

            if (roll <= 70) // 70% chance for Goblin (common)
            {
                return new Entities.Enemies.EnemyTypes.Goblin(enemyLevel);
            }
            else if (roll <= 95) // 25% chance for Bandit (elite)
            {
                return new Entities.Enemies.EnemyTypes.Bandit(enemyLevel);
            }
            else // 5% chance for Dragon (boss)
            {
                return new Entities.Enemies.EnemyTypes.Dragon(enemyLevel);
            }
        }

        /// <summary>
        /// Trigger a combat encounter - called by Explore() or MapManager
        /// This method works for both text-based AND Godot versions
        /// </summary>
        public void TriggerCombatEncounter()
        {
            // Spawn an appropriate enemy
            var enemy = SpawnEnemy();

            Console.WriteLine($"A {enemy.Name} appears!\n");
            Console.WriteLine(enemy.GetDescription());

            PushState(GameState.Combat); // Save Playing state, go to Combat

            // CombatManager handles the actual combat (including XP and loot rewards)
            bool playerWon = _combatManager.StartCombat(_player, enemy);

            if (playerWon)
            {
                PopState(); // Return to Playing state
            }
            else
            {
                Console.WriteLine("\nYou have been defeated...");
                ChangeState(GameState.GameOver); // One-way to game over
            }
        }

        /// <summary>
        /// Trigger a loot event - called by Explore() or MapManager
        /// Works for both text-based AND Godot versions
        /// </summary>
        public void TriggerLootEvent()
        {
            Console.WriteLine("You found a treasure chest!");

            int goldFound = _rngManager.Roll(10, 50);
            _player.Gold += goldFound;

            Console.WriteLine($"You gained {goldFound} gold!");

            // Roll for item drop
            int itemRoll = _rngManager.Roll(1, 100);

            if (itemRoll <= 60) // 60% chance for an item
            {
                // Determine item type
                int typeRoll = _rngManager.Roll(1, 100);

                if (typeRoll <= 50) // 50% consumable
                {
                    var potion = Items.ItemDatabase.GetHealthPotion(_player.Level);
                    if (potion != null)
                    {
                        _player.AddToInventory(potion);
                        Console.WriteLine($"Found: {potion.GetDisplayName()}");
                    }
                }
                else if (typeRoll <= 75) // 25% weapon
                {
                    var weapon = Items.ItemDatabase.GetWeapon("Sword", _player.Level, _player.Level);
                    if (weapon != null)
                    {
                        _player.AddToInventory(weapon);
                        Console.WriteLine($"Found: {weapon.GetDisplayName()}");
                    }
                }
                else // 25% armor
                {
                    var armor = Items.ItemDatabase.GetArmor("Leather Armor", _player.Level, _player.Level);
                    if (armor != null)
                    {
                        _player.AddToInventory(armor);
                        Console.WriteLine($"Found: {armor.GetDisplayName()}");
                    }
                }
            }

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        /// <summary>
        /// Display player inventory
        /// </summary>
        private void ShowInventory()
        {
            Console.WriteLine("\n=== Inventory ===");
            
            if (_player.Inventory.Count == 0)
            {
                Console.WriteLine("Your inventory is empty.");
            }
            else
            {
                for (int i = 0; i < _player.Inventory.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {_player.Inventory[i].Name}");
                }
            }
            
            Console.WriteLine($"\nEquipped Weapon: {(_player.EquippedWeapon?.Name ?? "None")}");
            Console.WriteLine($"Equipped Armor: {(_player.EquippedArmor?.Name ?? "None")}");
            
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        /// <summary>
        /// Display detailed player stats
        /// </summary>
        private void ShowStats()
        {
            Console.WriteLine("\n=== Character Stats ===");
            Console.WriteLine($"Name: {_player.Name}");
            Console.WriteLine($"Level: {_player.Level}");
            Console.WriteLine($"Experience: {_player.Experience}");
            Console.WriteLine($"Health: {_player.Health}/{_player.MaxHealth}");
            Console.WriteLine($"Gold: {_player.Gold}");

            // Ability info
            Console.WriteLine("\n--- Ability ---");
            if (_player.SelectedAbility != null)
            {
                Console.WriteLine(_player.SelectedAbility.GetInfo());
            }
            else
            {
                Console.WriteLine("No ability selected");
            }

            // Attack stats
            Console.WriteLine("\n--- Combat Stats ---");
            if (_player.EquippedWeapon != null)
            {
                Console.WriteLine($"Weapon: {_player.EquippedWeapon.Name}");
                Console.WriteLine($"  Damage: {_player.EquippedWeapon.MinDamage}-{_player.EquippedWeapon.MaxDamage}");
                Console.WriteLine($"  Accuracy: {_player.EquippedWeapon.Accuracy}%");
                Console.WriteLine($"  Crit Chance: {_player.EquippedWeapon.CritChance}%");
            }
            else
            {
                Console.WriteLine("Weapon: None (unarmed - 1-3 damage)");
            }

            // Defense stats
            if (_player.EquippedArmor != null)
            {
                Console.WriteLine($"Armor: {_player.EquippedArmor.Name}");
                Console.WriteLine($"  Defense: {_player.EquippedArmor.Defense}");
            }
            else
            {
                Console.WriteLine("Armor: None");
            }

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        /// <summary>
        /// Rest to restore health
        /// </summary>
        private void Rest()
        {
            Console.WriteLine("\nYou take a moment to rest...");
            
            int healAmount = _player.MaxHealth / 2;
            _player.Heal(healAmount);
            
            Console.WriteLine("You feel refreshed!");
            
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        /// <summary>
        /// Save the current game
        /// </summary>
        private void SaveGame()
        {
            Console.WriteLine("\nSaving game...");

            bool success = Data.SaveManager.SaveGame(_player, "save1"); // Default save slot

            if (success)
            {
                Console.WriteLine("Game saved successfully!");
            }
            else
            {
                Console.WriteLine("Failed to save game.");
            }

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        } 

        /// <summary>
        /// Show main menu (for future use)
        /// </summary>
        private void ShowMainMenu()
        {
            // Placeholder - you'd expand this later
            _currentState = GameState.Playing;
        }

        /// <summary>
        /// Show pause menu
        /// </summary>
        private void ShowPauseMenu()
        {
            Console.Clear();
            Console.WriteLine("\n=== PAUSED ===");
            Console.WriteLine("1. Resume");
            Console.WriteLine("2. Save Game");
            Console.WriteLine("3. Load Game");
            Console.WriteLine("4. Settings (placeholder)");
            Console.WriteLine("5. Quit to Main Menu");
            Console.WriteLine("6. Quit Game");
            
            Console.Write("\nChoice: ");
            string choice = Console.ReadLine();
            
            switch (choice)
            {
                case "1":
                    PopState(); // Return to previous state (Playing or Combat)
                    break;
                case "2":
                    SaveGame();
                    // Stay in pause menu
                    break;
                case "3":
                    LoadGame();
                    PopState(); // Clear pause state
                    break;
                case "4":
                    Console.WriteLine("Settings not implemented yet.");
                    Console.WriteLine("\nPress any key to continue...");
                    Console.ReadKey();
                    break;
                case "5":
                    _stateStack.Clear(); // Clear state stack
                    ChangeState(GameState.MainMenu);
                    break;
                case "6":
                    _isRunning = false;
                    break;
                default:
                    Console.WriteLine("Invalid choice.");
                    break;
            }
        }

        /// <summary>
        /// Handle game over state
        /// </summary>
        private void HandleGameOver()
        {
            Console.WriteLine("\n=== GAME OVER ===");
            Console.WriteLine($"You reached level {_player.Level}");
            Console.WriteLine($"You collected {_player.Gold} gold");
            
            Console.WriteLine("\n1. Load Last Save");
            Console.WriteLine("2. Return to Main Menu");
            Console.WriteLine("3. Quit");
            
            Console.Write("\nChoice: ");
            string choice = Console.ReadLine();
            
            switch (choice)
            {
                case "1":
                    LoadGame();
                    break;
                case "2":
                    ChangeState(GameState.MainMenu);
                    break;
                case "3":
                    _isRunning = false;
                    break;
                default:
                    Console.WriteLine("Invalid choice.");
                    HandleGameOver(); // Ask again
                    break;
            }
        }
    }
}