using System;
using System.Collections.Generic;
using System.Linq;
using GameLogic.Entities.Player;
using GameLogic.World;
using GameLogic.Systems;
using GameLogic.Combat;
using GameLogic.Data;
using GameLogic.Quests;
using GameLogic.Entities.NPCs;

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
        private List<Entities.NPCs.Companions.CompanionBase> _activeCompanions; // Companions in the party
        private MapManager _mapManager;
        private CombatManager _combatManager;
        private RNGManager _rngManager;
        private Progression.BossManager _bossManager;
        private QuestManager _questManager;
        private QuestGiver _questGiver;
        private GameSettings _gameSettings;
        private StatisticsTracker _statistics;

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
            _gameSettings = new GameSettings(); // Initialize with default settings
            _statistics = new StatisticsTracker(); // Initialize statistics tracker
            Data.SaveManager.Initialize(); // Initialize static SaveManager
            _mapManager = new MapManager();
            _combatManager = new CombatManager(_rngManager);
            _activeCompanions = new List<Entities.NPCs.Companions.CompanionBase>();

            _currentState = GameState.MainMenu;
            _isRunning = false;

            Console.WriteLine("Systems initialized!\n");
        }

        /// <summary>
        /// Get the maximum party size based on player's Leadership ability
        /// Base size is 4 (player + 3 companions)
        /// Leadership passive ability can increase this up to 8 at level 100
        /// </summary>
        private int GetMaxPartySize()
        {
            const int basePartySize = 4;

            if (_player == null || _player.SelectedAbility == null)
            {
                return basePartySize;
            }

            // Check if player has Leadership passive ability
            if (_player.SelectedAbility is Abilities.LeadershipAbility leadership)
            {
                int bonus = leadership.GetPassiveBonusValue();
                return basePartySize + bonus;
            }

            return basePartySize;
        }

        /// <summary>
        /// Check if party has room for another companion
        /// </summary>
        private bool CanAddCompanion()
        {
            // _activeCompanions doesn't include the player, so max is (GetMaxPartySize() - 1)
            int maxCompanions = GetMaxPartySize() - 1;
            return _activeCompanions.Count < maxCompanions;
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
        /// Let the player select difficulty (immutable after creation)
        /// </summary>
        private void SelectDifficulty()
        {
            Console.WriteLine("=== Select Difficulty ===");
            Console.WriteLine("This choice is permanent for this save file and cannot be changed!\n");

            Console.WriteLine("1. Easy");
            Console.WriteLine("   - Enemies have 75% stats");
            Console.WriteLine("   - Rewards are 80% of normal");
            Console.WriteLine("   - Recommended for learning the game\n");

            Console.WriteLine("2. Normal (Recommended)");
            Console.WriteLine("   - Balanced gameplay");
            Console.WriteLine("   - Standard enemies and rewards");
            Console.WriteLine("   - The intended experience\n");

            Console.WriteLine("3. Hard");
            Console.WriteLine("   - Enemies have 150% stats");
            Console.WriteLine("   - Rewards are 130% of normal");
            Console.WriteLine("   - For experienced players\n");

            Console.WriteLine("4. Very Hard");
            Console.WriteLine("   - Enemies have 200% stats");
            Console.WriteLine("   - Rewards are 150% of normal");
            Console.WriteLine("   - Extreme challenge\n");

            int choice = -1;
            while (choice < 1 || choice > 4)
            {
                Console.Write("Select difficulty (1-4): ");
                string input = Console.ReadLine();

                if (int.TryParse(input, out choice) && choice >= 1 && choice <= 4)
                {
                    break;
                }

                Console.WriteLine("Invalid choice. Please try again.");
            }

            _gameSettings.Difficulty = choice switch
            {
                1 => DifficultyLevel.Easy,
                2 => DifficultyLevel.Normal,
                3 => DifficultyLevel.Hard,
                4 => DifficultyLevel.VeryHard,
                _ => DifficultyLevel.Normal
            };

            Console.WriteLine($"\nDifficulty set to: {_gameSettings.Difficulty}");
            Console.WriteLine("Remember: This cannot be changed for this save file!\n");
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

            // Select difficulty (once per save file, immutable)
            SelectDifficulty();

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

            Console.WriteLine("\n🎲 Initializing Champion Bosses...");

            // Create new boss manager and register all 15 bosses
            _bossManager = new Progression.BossManager();
            var championBosses = Progression.BossDefinitions.GetAllChampionBosses();
            _bossManager.RegisterBosses(championBosses.ToArray());

            // Randomly select the final boss
            _bossManager.SelectRandomFinalBoss(_rngManager);

            Console.WriteLine("\nThe champions await your challenge!");
            Console.WriteLine("Defeat 10 of the 15 Champions to unlock the Final Gate.\n");

            // Initialize quest system
            Console.WriteLine("🎲 Initializing Quest System...");
            _questManager = new QuestManager();
            InitializeQuests();
            _questGiver = new QuestGiver("Veteran Ranger", _questManager, _bossManager);
            Console.WriteLine("Quests are now available at the Job Board and Quest Giver!\n");

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

                // Load game settings from save data
                _gameSettings = Data.SaveManager.LoadSettingsFromSaveData(saveData);

                // Load statistics from save data
                _statistics = Data.SaveManager.LoadStatisticsFromSaveData(saveData);

                // Apply RNG settings
                _rngManager.SetStatisticsTracking(_gameSettings.RngStatisticsTracking);
                if (!string.IsNullOrEmpty(_gameSettings.RngAlgorithm))
                {
                    _rngManager.SwitchAlgorithm(_gameSettings.RngAlgorithm);
                }

                // Load boss manager from save data
                _bossManager = Data.SaveManager.LoadBossManager(saveData, _rngManager);

                // Initialize quest system and reconstruct quests from save
                _questManager = new QuestManager();
                if (saveData.Quests != null && saveData.Quests.Count > 0)
                {
                    // Reconstruct quests with their original RNG values from save
                    QuestSerializationHelper.ReconstructQuests(_questManager, saveData.Quests, _bossManager);
                    if (!string.IsNullOrEmpty(saveData.ActiveQuestId))
                    {
                        _questManager.SetActiveQuestId(saveData.ActiveQuestId);
                    }
                }
                else
                {
                    // Fallback: If no quests in save (old save file), initialize new quests
                    InitializeQuests();
                }
                _questGiver = new QuestGiver("Veteran Ranger", _questManager, _bossManager);

                // Load/regenerate map based on saved seed
                if (saveData.MapSeed != 0)
                {
                    _mapManager.GenerateMapFromSeed(saveData.MapSeed);
                    _mapManager.SetCurrentNode(saveData.CurrentMapNodeId);
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
            Console.WriteLine("7. ⚔️  Champion Challenges (Boss Fights)");
            Console.WriteLine("8. 📋 Quest Giver (Boss Quests)");
            Console.WriteLine("9. 📌 Job Board (Other Quests)");
            Console.WriteLine("10. 📖 Quest Log");
            Console.WriteLine("11. 📊 Statistics");
            Console.WriteLine("12. Quit");
            
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
                    ShowChampionMenu();
                    break;
                case "8":
                    _questGiver.Interact();
                    break;
                case "9":
                    Menus.JobBoard.DisplayJobBoard(_questManager);
                    break;
                case "10":
                    _questManager.DisplayQuestLog();
                    Console.WriteLine("\nPress any key to continue...");
                    Console.ReadKey();
                    break;
                case "11":
                    Menus.StatisticsMenu.DisplayStatisticsMenu(_statistics);
                    break;
                case "12":
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
            bool playerWon = _combatManager.StartCombat(_player, enemy, _activeCompanions);

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
            _player.Inventory.DisplayInventory();

            Console.WriteLine($"Equipped Weapon: {(_player.EquippedWeapon?.Name ?? "None")}");
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

            // Update statistics before saving
            _statistics.UpdatePlayTime(_player.PlayTime);
            _statistics.UpdateCurrentGold(_player.Gold);
            _statistics.UpdateCurrentLevel(_player.Level);
            _statistics.RecordGameSave();

            bool success = Data.SaveManager.SaveGame(_player, "save1", _bossManager, _mapManager, _questManager, _gameSettings, _statistics);

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
                    Menus.SettingsMenu.DisplaySettingsMenu(_gameSettings, _rngManager, isDuringSaveFile: true);
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
        /// Show Champion Challenges menu (Boss Fights)
        /// </summary>
        private void ShowChampionMenu()
        {
            Console.Clear();

            // Display boss progression summary
            Console.WriteLine(_bossManager.GetProgressionSummary());
            Console.WriteLine();

            // Display final gate status
            int keyCount = _bossManager.CountChampionKeys(_player.Inventory);
            Console.WriteLine(_bossManager.GetFinalGateStatus(_player.Inventory));
            Console.WriteLine();

            // Main menu options
            Console.WriteLine("═══ CHAMPION CHALLENGES ═══");
            Console.WriteLine("1. Challenge a Champion Boss");
            Console.WriteLine("2. View Boss List");
            Console.WriteLine("3. Enter Final Gate (Requires 10 Keys)");
            Console.WriteLine("4. Return to Adventure");
            Console.Write("\nChoice: ");

            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    SelectAndChallengeBoss();
                    break;
                case "2":
                    ViewBossList();
                    break;
                case "3":
                    ChallengeFinalBoss();
                    break;
                case "4":
                    // Return to game loop
                    break;
                default:
                    Console.WriteLine("Invalid choice.");
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                    ShowChampionMenu(); // Re-show menu
                    break;
            }
        }

        /// <summary>
        /// Select and challenge a champion boss
        /// </summary>
        private void SelectAndChallengeBoss()
        {
            Console.Clear();

            // Display available bosses
            World.BossEncounter.DisplayBossSelectionMenu(_bossManager, true);

            var availableBosses = World.BossEncounter.GetAvailableChampionBosses(_bossManager);

            Console.WriteLine($"[{availableBosses.Count + 1}] Return to Champion Menu");
            Console.Write("\nSelect a boss to challenge: ");

            string choice = Console.ReadLine();

            if (int.TryParse(choice, out int bossIndex))
            {
                if (bossIndex >= 1 && bossIndex <= availableBosses.Count)
                {
                    var selectedBoss = availableBosses[bossIndex - 1];

                    // Start boss encounter
                    bool victory = World.BossEncounter.StartBossEncounter(
                        _player,
                        selectedBoss,
                        _bossManager,
                        _combatManager,
                        null); // TODO: Add companion support when implemented

                    if (victory)
                    {
                        Console.WriteLine("\n✨ You may now save your progress or continue your adventure.");
                    }
                    else
                    {
                        // Player lost or fled
                        if (_player.Health <= 0)
                        {
                            ChangeState(GameState.GameOver);
                            return;
                        }
                    }
                }
                else if (bossIndex == availableBosses.Count + 1)
                {
                    // Return to champion menu
                    ShowChampionMenu();
                    return;
                }
                else
                {
                    Console.WriteLine("Invalid choice.");
                }
            }
            else
            {
                Console.WriteLine("Invalid input.");
            }

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
            ShowChampionMenu();
        }

        /// <summary>
        /// View detailed list of all bosses
        /// </summary>
        private void ViewBossList()
        {
            Console.Clear();
            Console.WriteLine("═══ ALL CHAMPION BOSSES ═══\n");

            var allBosses = _bossManager.AllBosses.Values.OrderBy(b => b.Level).ToList();

            foreach (var boss in allBosses)
            {
                bool isDefeated = _bossManager.IsBossDefeated(boss.BossId);
                bool isFinalBoss = boss.BossId == _bossManager.FinalBossId;

                string status = isFinalBoss ? " [FINAL BOSS]" :
                               isDefeated ? " [✓ DEFEATED]" : " [NEW]";

                Console.WriteLine($"{boss.Name}{status}");
                Console.WriteLine($"  Level: {boss.Level} | Mechanic: {boss.MechanicType}");

                if (isDefeated)
                {
                    Console.WriteLine($"  Times Defeated: {boss.TimesDefeated}");
                }

                if (isFinalBoss)
                {
                    Console.WriteLine($"  ⚠️  Requires 10 Champion Keys to unlock");
                }

                Console.WriteLine($"  {boss.Title}");
                Console.WriteLine();
            }

            Console.WriteLine("Press any key to return to Champion Menu...");
            Console.ReadKey();
            ShowChampionMenu();
        }

        /// <summary>
        /// Challenge the final boss
        /// </summary>
        private void ChallengeFinalBoss()
        {
            bool victory = World.BossEncounter.StartFinalBossEncounter(
                _player,
                _bossManager,
                _combatManager,
                null); // TODO: Add companion support when implemented

            if (victory)
            {
                Console.WriteLine("\n🎉 YOU HAVE COMPLETED THE GAME! 🎉");
                Console.WriteLine("You may continue playing to explore or challenge bosses again.");
            }
            else
            {
                // Player lost or didn't meet requirements
                if (_player.Health <= 0)
                {
                    ChangeState(GameState.GameOver);
                    return;
                }
            }

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
            ShowChampionMenu();
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

        /// <summary>
        /// Initialize all quests for the game (ONLY called on new game creation)
        /// </summary>
        private void InitializeQuests()
        {
            // Create boss defeat quests for each champion (except final boss)
            foreach (var boss in _bossManager.AllBosses.Values)
            {
                if (boss.BossId != _bossManager.FinalBossId)
                {
                    // Boss quests: 100 gold + 50 XP per quest (no RNG - these are fixed)
                    var bossQuest = new BossDefeatQuest(boss.BossId, boss.Name, 100, 50);
                    bossQuest.Discover(); // Make available immediately
                    _questManager.RegisterQuest(bossQuest);
                }
            }

            // Create final boss quest (no RNG - this is fixed)
            var finalBoss = _bossManager.GetFinalBoss();
            if (finalBoss != null)
            {
                var finalQuest = new FinalBossQuest(finalBoss.BossId, finalBoss.Name);
                finalQuest.Discover(); // Make available immediately
                _questManager.RegisterQuest(finalQuest);
            }

            // Create level progression quests (no RNG - fixed milestones)
            int[] levelMilestones = { 5, 10, 15, 20, 25 };
            foreach (int level in levelMilestones)
            {
                // Add RNG to rewards: ±20% variation
                int baseGold = level * 50;
                int baseXP = level * 25;
                int gold = ApplyRewardVariation(baseGold, 0.2);
                int xp = ApplyRewardVariation(baseXP, 0.2);

                var levelQuest = new LevelQuest(level, gold, xp);
                levelQuest.Discover();
                _questManager.RegisterQuest(levelQuest);
            }

            // Create enemy kill quests with RNG on requirements and rewards
            // Tier 1: 10 enemies (±6 = 4-16 range)
            int tier1Kills = ApplyRequirementVariation(10, 6);
            int tier1Gold = ApplyRewardVariation(75, 0.3);  // ±30% variation
            int tier1XP = ApplyRewardVariation(40, 0.3);
            var enemyQuest1 = new EnemyKillQuest("kill_tier1_enemies", "Novice Hunter", tier1Kills, tier1Gold, tier1XP);
            enemyQuest1.Discover();
            _questManager.RegisterQuest(enemyQuest1);

            // Tier 2: 25 enemies (±6 = 19-31 range)
            int tier2Kills = ApplyRequirementVariation(25, 6);
            int tier2Gold = ApplyRewardVariation(150, 0.3);
            int tier2XP = ApplyRewardVariation(75, 0.3);
            var enemyQuest2 = new EnemyKillQuest("kill_tier2_enemies", "Experienced Hunter", tier2Kills, tier2Gold, tier2XP);
            enemyQuest2.Discover();
            _questManager.RegisterQuest(enemyQuest2);

            // Tier 3: 50 enemies (±6 = 44-56 range)
            int tier3Kills = ApplyRequirementVariation(50, 6);
            int tier3Gold = ApplyRewardVariation(300, 0.3);
            int tier3XP = ApplyRewardVariation(150, 0.3);
            var enemyQuest3 = new EnemyKillQuest("kill_tier3_enemies", "Master Hunter", tier3Kills, tier3Gold, tier3XP);
            enemyQuest3.Discover();
            _questManager.RegisterQuest(enemyQuest3);

            // Create gold collection quests with RNG on requirements and rewards
            // Tier 1: 500 gold (±6 * 25 = ±150 = 350-650 range)
            int tier1GoldReq = ApplyRequirementVariation(500, 6 * 25);
            int tier1GoldReward = ApplyRewardVariation(100, 0.3);
            int tier1GoldXP = ApplyRewardVariation(50, 0.3);
            var goldQuest1 = new GoldCollectionQuest(tier1GoldReq, tier1GoldReward, tier1GoldXP);
            goldQuest1.Discover();
            _questManager.RegisterQuest(goldQuest1);

            // Tier 2: 1000 gold (±150 = 850-1150 range)
            int tier2GoldReq = ApplyRequirementVariation(1000, 150);
            int tier2GoldReward = ApplyRewardVariation(200, 0.3);
            int tier2GoldXP = ApplyRewardVariation(100, 0.3);
            var goldQuest2 = new GoldCollectionQuest(tier2GoldReq, tier2GoldReward, tier2GoldXP);
            goldQuest2.Discover();
            _questManager.RegisterQuest(goldQuest2);

            // Tier 3: 2500 gold (±300 = 2200-2800 range)
            int tier3GoldReq = ApplyRequirementVariation(2500, 300);
            int tier3GoldReward = ApplyRewardVariation(500, 0.3);
            int tier3GoldXP = ApplyRewardVariation(250, 0.3);
            var goldQuest3 = new GoldCollectionQuest(tier3GoldReq, tier3GoldReward, tier3GoldXP);
            goldQuest3.Discover();
            _questManager.RegisterQuest(goldQuest3);

            // Create weapon upgrade quests with RNG on requirements and rewards
            // Tier 1: Level 5 (±2 = 3-7 range)
            int tier1WeaponLevel = ApplyRequirementVariation(5, 2);
            int tier1WeaponGold = ApplyRewardVariation(150, 0.3);
            int tier1WeaponXP = ApplyRewardVariation(75, 0.3);
            var weaponQuest1 = new WeaponUpgradeQuest(tier1WeaponLevel, tier1WeaponGold, tier1WeaponXP);
            weaponQuest1.Discover();
            _questManager.RegisterQuest(weaponQuest1);

            // Tier 2: Level 10 (±3 = 7-13 range)
            int tier2WeaponLevel = ApplyRequirementVariation(10, 3);
            int tier2WeaponGold = ApplyRewardVariation(300, 0.3);
            int tier2WeaponXP = ApplyRewardVariation(150, 0.3);
            var weaponQuest2 = new WeaponUpgradeQuest(tier2WeaponLevel, tier2WeaponGold, tier2WeaponXP);
            weaponQuest2.Discover();
            _questManager.RegisterQuest(weaponQuest2);

            // Create equipment quests with RNG on requirements and rewards
            // Tier 1: Level 3 (±1 = 2-4 range)
            int tier1EquipLevel = ApplyRequirementVariation(3, 1);
            int tier1EquipGold = ApplyRewardVariation(100, 0.3);
            int tier1EquipXP = ApplyRewardVariation(50, 0.3);
            var equipQuest1 = new EquipmentQuest(tier1EquipLevel, tier1EquipGold, tier1EquipXP);
            equipQuest1.Discover();
            _questManager.RegisterQuest(equipQuest1);

            // Tier 2: Level 5 (±2 = 3-7 range)
            int tier2EquipLevel = ApplyRequirementVariation(5, 2);
            int tier2EquipGold = ApplyRewardVariation(200, 0.3);
            int tier2EquipXP = ApplyRewardVariation(100, 0.3);
            var equipQuest2 = new EquipmentQuest(tier2EquipLevel, tier2EquipGold, tier2EquipXP);
            equipQuest2.Discover();
            _questManager.RegisterQuest(equipQuest2);

            // Create challenge quests with RNG on rewards
            int challenge1Gold = ApplyRewardVariation(200, 0.3);
            int challenge1XP = ApplyRewardVariation(100, 0.3);
            var challenge1 = new ChallengeQuest("flawless_victory", "Flawless Victory", "Win a battle without taking any damage. Perfect defense and timing are key!", ChallengeType.FlawlessVictory, challenge1Gold, challenge1XP);
            challenge1.Discover();
            _questManager.RegisterQuest(challenge1);

            int challenge2Gold = ApplyRewardVariation(150, 0.3);
            int challenge2XP = ApplyRewardVariation(75, 0.3);
            var challenge2 = new ChallengeQuest("crit_master", "Critical Master", "Land 5 critical hits in a single battle. Show your mastery of precision strikes!", ChallengeType.CriticalMaster, challenge2Gold, challenge2XP);
            challenge2.Discover();
            _questManager.RegisterQuest(challenge2);

            int challenge3Gold = ApplyRewardVariation(175, 0.3);
            int challenge3XP = ApplyRewardVariation(90, 0.3);
            var challenge3 = new ChallengeQuest("survivor", "Survivor", "Win a battle with less than 10% health remaining. Live on the edge!", ChallengeType.Survivor, challenge3Gold, challenge3XP);
            challenge3.Discover();
            _questManager.RegisterQuest(challenge3);

            // Win streak: 5 battles (±2 = 3-7 range)
            int winStreakReq = ApplyRequirementVariation(5, 2);
            int challenge4Gold = ApplyRewardVariation(250, 0.3);
            int challenge4XP = ApplyRewardVariation(125, 0.3);
            // Update description dynamically based on RNG requirement
            string winStreakDesc = $"Win {winStreakReq} battles in a row without fleeing. Prove your consistency!";
            var challenge4 = new ChallengeQuest("win_streak", "Undefeated", winStreakDesc, ChallengeType.WinStreak, challenge4Gold, challenge4XP, winStreakReq);
            challenge4.Discover();
            _questManager.RegisterQuest(challenge4);

            Console.WriteLine($"✓ {_questManager.AllQuests.Count} quests initialized with randomized requirements and rewards");
        }

        /// <summary>
        /// Apply RNG variation to quest requirements (±variance)
        /// </summary>
        private int ApplyRequirementVariation(int baseValue, int maxVariance)
        {
            int variation = _rngManager.Roll(-maxVariance, maxVariance);
            return Math.Max(1, baseValue + variation); // Ensure minimum of 1
        }

        /// <summary>
        /// Apply percentage-based RNG variation to rewards
        /// </summary>
        private int ApplyRewardVariation(int baseValue, double variancePercent)
        {
            int maxVariance = (int)(baseValue * variancePercent);
            int variation = _rngManager.Roll(-maxVariance, maxVariance);
            return Math.Max(1, baseValue + variation); // Ensure minimum of 1
        }
    }
}