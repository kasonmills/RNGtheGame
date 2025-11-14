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
            Console.WriteLine("Your adventure begins...\n");
            
            // TODO: Let player choose starting ability
            
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
                // TODO: Load map state, etc.

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
        /// Trigger a combat encounter - called by Explore() or MapManager
        /// This method works for both text-based AND Godot versions
        /// </summary>
        public void TriggerCombatEncounter()
        {
            Console.WriteLine("An enemy appears!\n");
            
            // TODO: Get enemy from EnemyFactory based on current location difficulty
            // For now, placeholder until Enemy class exists
            
            PushState(GameState.Combat); // Save Playing state, go to Combat
            
            // CombatManager handles the actual combat
            bool playerWon = _combatManager.StartCombat(_player, null); // null until Enemy exists
            
            if (playerWon)
            {
                Console.WriteLine("\nVictory!");
                // TODO: Give XP and loot based on enemy
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
            
            // TODO: Use LootTable system to generate items/weapons/armor
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
            
            // TODO: Add ability info, attack/defense stats
            
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