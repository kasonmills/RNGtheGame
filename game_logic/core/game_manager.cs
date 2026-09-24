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
        private Entities.NPCs.Companions.PartyManager _partyManager;
        private MapManager _mapManager;
        private CombatManager _combatManager;
        private RNGManager _rngManager;
        private Entities.Enemies.Bosses.BossManager _bossManager;
        private QuestManager _questManager;
        private QuestGiver _questGiver;
        private GameSettings _gameSettings;
        private StatisticsTracker _statistics;
        private GameStartup _gameStartup;
        private TutorialManager _tutorialManager;

        // === Game State Management ===
        private GameState _currentState;
        private Stack<GameState> _stateStack; // Remember previous states
        private bool _isRunning;

        // === Overworld Encounters ===
        private List<World.OverworldEnemySpawn> _activeOverworldEnemies;

        /// <summary>
        /// The single running GameManager instance. Minimum viable access pattern for a
        /// Godot script to reach GameManager.Instance.Combat, etc.
        /// TODO-GODOT: a fuller singleton/DI story for non-combat scenes is still deferred.
        /// </summary>
        public static GameManager Instance { get; private set; }

        /// <summary>
        /// The combat engine - subscribe to its CombatMessage/CombatEnded events and call
        /// SubmitPlayerAction() from a combat scene.
        /// </summary>
        public CombatManager Combat => _combatManager;

        /// <summary>
        /// The opening tutorial's content and combat-teaching gate.
        /// </summary>
        public TutorialManager Tutorial => _tutorialManager;

        /// <summary>
        /// Fired whenever the game state changes (old state, new state) via ChangeState/PushState/PopState.
        /// TODO-GODOT: subscribe to this instead of polling the current state.
        /// </summary>
        public event Action<GameState, GameState> StateChanged;

        // === Constructor ===
        public GameManager()
        {
            Instance = this;

            _stateStack = new Stack<GameState>();
            _activeOverworldEnemies = new List<World.OverworldEnemySpawn>();
            _partyManager = new Entities.NPCs.Companions.PartyManager();
            _gameStartup = new GameStartup();
            _tutorialManager = new TutorialManager();

            var systems = _gameStartup.InitializeSystems();
            _rngManager = systems.RngManager;
            _gameSettings = systems.GameSettings;
            _statistics = systems.Statistics;
            _mapManager = systems.MapManager;
            _combatManager = systems.CombatManager;

            _currentState = GameState.MainMenu;
            _isRunning = true;
        }

        /// <summary>
        /// Change to a new state, pushing current state onto stack
        /// Use this when you want to be able to return to previous state
        /// </summary>
        public void PushState(GameState newState)
        {
            var oldState = _currentState;
            _stateStack.Push(_currentState);
            _currentState = newState;
            StateChanged?.Invoke(oldState, newState);
        }

        /// <summary>
        /// Return to the previous state
        /// </summary>
        public void PopState()
        {
            if (_stateStack.Count > 0)
            {
                var oldState = _currentState;
                _currentState = _stateStack.Pop();
                StateChanged?.Invoke(oldState, _currentState);
            }
        }

        /// <summary>
        /// Change to a new state without saving previous state
        /// Use this for one-way transitions (like MainMenu -> Playing)
        /// </summary>
        public void ChangeState(GameState newState)
        {
            var oldState = _currentState;
            _currentState = newState;
            StateChanged?.Invoke(oldState, newState);
        }

        /// <summary>
        /// Start a brand new game. TODO-GODOT: call this from the character-creation scene
        /// once it's collected name/difficulty/ability/weapon via UI (see GetStarterWeaponChoices()
        /// and Player.GetAvailableAbilities() for the choices to present).
        /// </summary>
        public void StartNewGame(string playerName, DifficultyLevel difficulty, Abilities.Ability ability, Items.Weapon startingWeapon)
        {
            var result = _gameStartup.CreateNewGame(playerName, difficulty, ability, startingWeapon, _rngManager, _mapManager, _gameSettings);

            _player = result.Player;
            _bossManager = result.BossManager;
            _questManager = result.QuestManager;
            _questGiver = result.QuestGiver;

            ChangeState(GameState.Tutorial); // Opening tutorial before the player is free to roam
            _isRunning = true;
        }

        /// <summary>
        /// Starter weapon choices for the character-creation scene's weapon-select step.
        /// </summary>
        public List<Items.Weapon> GetStarterWeaponChoices()
        {
            return _gameStartup.GetStarterWeaponChoices();
        }

        /// <summary>
        /// Recruit the tutorial's starter companion into the party, before the Eagle Bear fight.
        /// TODO-GODOT: call this from the tutorial/story scene once the player continues past the intro.
        /// </summary>
        public void RecruitStarterCompanion()
        {
            _partyManager.RecruitCompanion(_player, _tutorialManager.GetStarterCompanion());
        }

        /// <summary>
        /// Begin the tutorial's first boss fight (Skarn, the Eagle Bear), with the tutorial's
        /// action-teaching gate active.
        /// TODO-GODOT: call this once the intro story/companion recruitment scene finishes.
        /// During this fight, submit actions via GameManager.Instance.Tutorial.TrySubmitAction(...)
        /// instead of Combat.SubmitPlayerAction(...) directly.
        /// </summary>
        public void BeginEagleBearEncounter()
        {
            var eagleBear = _tutorialManager.CreateEagleBearEncounter(_bossManager);

            ChangeState(GameState.Combat);

            void OnCombatEnded(bool victory)
            {
                _combatManager.CombatEnded -= OnCombatEnded;

                if (victory)
                {
                    ChangeState(GameState.Playing); // Tutorial complete - the player is free to roam
                }
                else if (_player.Health <= 0)
                {
                    ChangeState(GameState.GameOver);
                }
            }

            _combatManager.CombatEnded += OnCombatEnded;

            _combatManager.StartCombat(_player, new List<Entities.Enemies.EnemyBase> { eagleBear }, _partyManager.ActiveCompanions, _bossManager);
        }

        /// <summary>
        /// Load an existing saved game
        /// </summary>
        public void LoadGame()
        {
            var result = _gameStartup.LoadGame("save1", _rngManager, _mapManager); // Default save slot

            if (result.Success)
            {
                _player = result.Player;
                _gameSettings = result.GameSettings;
                _statistics = result.Statistics;
                _bossManager = result.BossManager;
                _questManager = result.QuestManager;
                _questGiver = result.QuestGiver;

                ChangeState(GameState.Playing);
                _isRunning = true;
            }
            else
            {
                // TODO-GODOT: load-failure feedback -> error dialog/toast
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
                    
                    case GameState.Paused:
                        ShowPauseMenu();
                        break;
                    
                    case GameState.GameOver:
                        HandleGameOver();
                        break;
                }
            }
            
            // Console.WriteLine("\nThanks for playing!"); // TODO-GODOT: not needed once this loop moves to Godot
        }

        /// <summary>
        /// Main gameplay loop - exploration and navigation
        /// TODO-GODOT: this text-menu dispatch is a stub until Godot scene/input wiring exists.
        /// The action methods below (Explore, OpenInventory, OpenStats, Rest, SaveGame,
        /// quest giver/job board/quest log/statistics menus) are still valid and will be called by
        /// player movement/interaction signals (walking onto an encounter tile, pressing an interact
        /// key near an NPC, opening a UI panel, etc.) instead of a numbered console choice.
        /// Champion boss challenges no longer go through a menu at all - the player finds
        /// champions by roaming, same as regular overworld encounters.
        /// </summary>
        private void GameLoop()
        {
            // === TEXT-BASED VERSION (kept for reference, not called until Godot wiring replaces it) ===
            // Console.WriteLine("\n=== Current Status ===");
            // Console.WriteLine($"Location: {_mapManager.GetCurrentLocationName()}");
            // Console.WriteLine($"Health: {_player.Health}/{_player.MaxHealth}");
            // Console.WriteLine($"Gold: {_player.Gold}");
            // Console.WriteLine($"Level: {_player.Level}");
            //
            // Console.WriteLine("\n=== What do you want to do? ===");
            // Console.WriteLine("1. Explore (encounter enemies/find loot)");
            // Console.WriteLine("2. View Inventory");
            // Console.WriteLine("3. View Stats");
            // Console.WriteLine("4. Rest (restore health)");
            // Console.WriteLine("5. Save Game");
            // Console.WriteLine("6. Pause Menu");
            // Console.WriteLine("7. ⚔️  Champion Challenges (Boss Fights)");
            // Console.WriteLine("8. 📋 Quest Giver (Boss Quests)");
            // Console.WriteLine("9. 📌 Job Board (Other Quests)");
            // Console.WriteLine("10. 📖 Quest Log");
            // Console.WriteLine("11. 📊 Statistics");
            // Console.WriteLine("12. Quit");
            //
            // Console.Write("\nChoice: ");
            // string choice = Console.ReadLine();
            //
            // switch (choice)
            // {
            //     case "1":
            //         Explore();
            //         break;
            //     case "2":
            //         OpenInventory();
            //         break;
            //     case "3":
            //         OpenStats();
            //         break;
            //     case "4":
            //         Rest();
            //         break;
            //     case "5":
            //         SaveGame();
            //         break;
            //     case "6":
            //         PushState(GameState.Paused); // Save current state and go to pause
            //         break;
            //     case "7":
            //         // Removed - champions are found by roaming, not picked from a menu
            //         break;
            //     case "8":
            //         _questGiver.Interact();
            //         break;
            //     case "9":
            //         Menus.JobBoard.DisplayJobBoard(_questManager);
            //         break;
            //     case "10":
            //         _questManager.DisplayQuestLog();
            //         Console.WriteLine("\nPress any key to continue...");
            //         Console.ReadKey();
            //         break;
            //     case "11":
            //         Menus.StatisticsMenu.DisplayStatisticsMenu(_statistics);
            //         break;
            //     case "12":
            //         _isRunning = false;
            //         break;
            //     default:
            //         Console.WriteLine("Invalid choice. Try again.");
            //         break;
            // }
            // === END TEXT-BASED VERSION ===
        }

        /// <summary>
        /// Explore the current area - will be replaced by movement system in 2D
        /// For text-based: simplified encounter trigger
        /// For Godot: this becomes player movement detection of events on map
        /// </summary>
        private void Explore()
        {
            // === TEXT-BASED VERSION (comment out when moving to Godot) ===
            // TODO-GODOT: flavor text -> exploration textbox/popup
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
        /// Get the min/max number of overworld enemies that can spawn in a given location type.
        /// Safe/non-combat locations (Town, RestSite, TreasureRoom, BossRoom) never spawn wandering enemies.
        /// </summary>
        private (int min, int max) GetEncounterSpawnRange(World.LocationType type)
        {
            return type switch
            {
                World.LocationType.Forest => (2, 5),
                World.LocationType.Cave => (2, 5),
                World.LocationType.Mountain => (1, 4),
                World.LocationType.Ruins => (1, 3),
                World.LocationType.Crossroads => (1, 3),
                _ => (0, 0)
            };
        }

        /// <summary>
        /// Spawn a random number of overworld enemies in the player's current route/area.
        /// TODO-GODOT: call this whenever the player arrives at a new route/area.
        /// </summary>
        public void SpawnOverworldEncounters()
        {
            _activeOverworldEnemies.Clear();

            var currentNode = _mapManager.GetCurrentNode();
            if (currentNode == null) return;

            var (min, max) = GetEncounterSpawnRange(currentNode.Type);
            if (max <= 0) return;

            int count = _rngManager.Roll(min, max);
            for (int i = 0; i < count; i++)
            {
                var enemy = SpawnEnemy();
                _activeOverworldEnemies.Add(new World.OverworldEnemySpawn(enemy, _rngManager));
            }
        }

        /// <summary>
        /// Get the enemies currently spawned in the player's route/area.
        /// TODO-GODOT: used by the route scene to place/move a visible enemy per spawn.
        /// </summary>
        public List<World.OverworldEnemySpawn> GetActiveOverworldEnemies()
        {
            return _activeOverworldEnemies;
        }

        /// <summary>
        /// Resolve an overworld enemy's hitbox colliding with the player's hitbox into a real combat encounter.
        /// TODO-GODOT: call this from the player's Area2D collision signal when it overlaps an overworld enemy's hitbox.
        /// </summary>
        public void OnOverworldEnemyHitboxCollision(World.OverworldEnemySpawn spawn)
        {
            if (spawn == null || !_activeOverworldEnemies.Contains(spawn)) return;

            _activeOverworldEnemies.Remove(spawn);
            TriggerCombatEncounter(spawn.Enemy);
        }

        /// <summary>
        /// Trigger a combat encounter - called by Explore() or MapManager
        /// This method works for both text-based AND Godot versions
        /// </summary>
        public void TriggerCombatEncounter(Entities.Enemies.EnemyBase enemy = null)
        {
            // Use the given enemy (e.g. from an overworld spawn) or spawn a fresh one
            enemy ??= SpawnEnemy();

            // TODO-GODOT: enemy intro/description -> combat scene intro textbox
            Console.WriteLine($"A {enemy.Name} appears!\n");
            Console.WriteLine(enemy.GetDescription());

            ChangeState(GameState.Combat); // Combat always enters from Playing and returns to Playing/GameOver

            void OnCombatEnded(bool victory)
            {
                _combatManager.CombatEnded -= OnCombatEnded;

                if (victory)
                {
                    ChangeState(GameState.Playing); // Return to Playing
                }
                else
                {
                    Console.WriteLine("\nYou have been defeated...");
                    ChangeState(GameState.GameOver); // One-way to game over
                }
            }

            _combatManager.CombatEnded += OnCombatEnded;

            // CombatManager handles the actual combat (including XP and loot rewards)
            _combatManager.StartCombat(_player, new List<Entities.Enemies.EnemyBase> { enemy }, _partyManager.ActiveCompanions);
        }

        /// <summary>
        /// Trigger a loot event - called by Explore() or MapManager
        /// Works for both text-based AND Godot versions
        /// </summary>
        public void TriggerLootEvent()
        {
            // TODO-GODOT: loot event flavor text + item found messages -> loot popup
            Console.WriteLine("You found a treasure chest!");

            // Chest loot rolls through RNGManager (via the adapter) so it respects the
            // player's chosen RNG algorithm and counts toward RNG statistics, same as
            // every other roll in the game.
            var rngAdapter = new Systems.RNGManagerRandomAdapter(_rngManager);
            var lootGenerator = new Progression.LootGenerator(rngAdapter);
            var lootTable = Progression.LootTableTemplates.CreateChestLoot();
            var loot = lootGenerator.GenerateLoot(lootTable, _player.Level);

            _player.Gold += loot.Gold;
            Console.WriteLine($"You gained {loot.Gold} gold!");

            if (loot.Items.Count > 0)
            {
                foreach (var item in loot.Items)
                {
                    _player.AddToInventory(item);
                    Console.WriteLine($"Found: {item.GetDisplayName()}");
                }
            }
            else
            {
                Console.WriteLine("The chest held only gold.");
            }

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        /// <summary>
        /// Open the inventory panel - pauses the player (like Paused) since they can't
        /// move their character while browsing it.
        /// TODO-GODOT: call this when the player opens the inventory panel/hotkey.
        /// </summary>
        public void OpenInventory()
        {
            PushState(GameState.Inventory);

            // TODO-GODOT: inventory listing -> inventory panel UI
            _player.Inventory.DisplayInventory();

            Console.WriteLine($"Equipped Weapon: {(_player.EquippedWeapon?.Name ?? "None")}");
            Console.WriteLine($"Equipped Armor: {(_player.EquippedArmor?.Name ?? "None")}");
        }

        /// <summary>
        /// Close the inventory panel and return to whatever state it interrupted.
        /// TODO-GODOT: call this when the player closes the inventory panel/hotkey.
        /// </summary>
        public void CloseInventory()
        {
            PopState();
        }

        /// <summary>
        /// Open the character stats panel - pauses the player (like Paused) since they can't
        /// move their character while browsing it.
        /// TODO-GODOT: call this when the player opens the stats panel/hotkey.
        /// </summary>
        public void OpenStats()
        {
            PushState(GameState.Stats);

            // TODO-GODOT: character stats block -> stats panel UI
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
        }

        /// <summary>
        /// Close the character stats panel and return to whatever state it interrupted.
        /// TODO-GODOT: call this when the player closes the stats panel/hotkey.
        /// </summary>
        public void CloseStats()
        {
            PopState();
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
        /// <summary>
        /// Save the current game. Returns success/failure instead of blocking on a
        /// keypress, so a Godot save button/toast can react to the result directly.
        /// TODO-GODOT: call this from a save button (world HUD or pause panel).
        /// </summary>
        public bool SaveGame()
        {
            // Update statistics before saving
            _statistics.UpdatePlayTime(_player.PlayTime);
            _statistics.UpdateCurrentGold(_player.Gold);
            _statistics.UpdateCurrentLevel(_player.Level);
            _statistics.RecordGameSave();

            bool success = Data.SaveManager.SaveGame(_player, "save1", _bossManager, _mapManager, _questManager, _gameSettings, _statistics);

            Console.WriteLine(success ? "Game saved successfully!" : "Failed to save game.");

            return success;
        }

        /// <summary>
        /// Show main menu / landing screen - entry point of the game
        /// TODO-GODOT: this text-menu dispatch is a stub until Godot scene/input wiring exists.
        /// StartNewGame()/LoadGame()/QuitGame() are still valid and will be called directly
        /// by the main menu scene's New Game/Load Game/Quit buttons instead of a numbered choice.
        /// </summary>
        private void ShowMainMenu()
        {
            // === TEXT-BASED VERSION (kept for reference, not called until Godot wiring replaces it) ===
            // Console.Clear();
            // Console.WriteLine("=====================================");
            // Console.WriteLine("           RNG: THE GAME");
            // Console.WriteLine("=====================================");
            // Console.WriteLine("A turn-based RPG where chance is everything.\n");
            // Console.WriteLine("1. New Game");
            // Console.WriteLine("2. Load Game");
            // Console.WriteLine("3. Quit");
            // Console.Write("\nChoice: ");
            //
            // string choice = Console.ReadLine();
            //
            // switch (choice)
            // {
            //     case "1":
            //         StartNewGame();
            //         break;
            //     case "2":
            //         LoadGame();
            //         break;
            //     case "3":
            //         QuitGame();
            //         break;
            //     default:
            //         Console.WriteLine("Invalid choice. Try again.");
            //         break;
            // }
            // === END TEXT-BASED VERSION ===
        }

        /// <summary>
        /// Show pause menu
        /// TODO-GODOT: this text-menu dispatch is a stub until Godot scene/input wiring exists.
        /// PopState() (Resume)/SaveGame()/LoadGame()/QuitToMainMenu()/QuitGame() are still valid
        /// and will be called directly by the pause panel's buttons instead of a numbered choice.
        /// </summary>
        private void ShowPauseMenu()
        {
            // === TEXT-BASED VERSION (kept for reference, not called until Godot wiring replaces it) ===
            // Console.Clear();
            // Console.WriteLine("\n=== PAUSED ===");
            // Console.WriteLine("1. Resume");
            // Console.WriteLine("2. Save Game");
            // Console.WriteLine("3. Load Game");
            // Console.WriteLine("4. Settings (placeholder)");
            // Console.WriteLine("5. Quit to Main Menu");
            // Console.WriteLine("6. Quit Game");
            //
            // Console.Write("\nChoice: ");
            // string choice = Console.ReadLine();
            //
            // switch (choice)
            // {
            //     case "1":
            //         PopState(); // Return to previous state (Playing or Combat)
            //         break;
            //     case "2":
            //         SaveGame();
            //         // Stay in pause menu
            //         break;
            //     case "3":
            //         LoadGame();
            //         PopState(); // Clear pause state
            //         break;
            //     case "4":
            //         Menus.SettingsMenu.DisplaySettingsMenu(_gameSettings, _rngManager, isDuringSaveFile: true);
            //         break;
            //     case "5":
            //         QuitToMainMenu();
            //         break;
            //     case "6":
            //         QuitGame();
            //         break;
            //     default:
            //         Console.WriteLine("Invalid choice.");
            //         break;
            // }
            // === END TEXT-BASED VERSION ===
        }

        /// <summary>
        /// Clear the pause/interrupt state stack and return to the main menu.
        /// TODO-GODOT: call this from the pause panel's "Quit to Main Menu" button.
        /// </summary>
        public void QuitToMainMenu()
        {
            _stateStack.Clear();
            ChangeState(GameState.MainMenu);
        }

        /// <summary>
        /// Stop the game loop entirely.
        /// TODO-GODOT: call this from a Quit button (main menu or pause panel).
        /// </summary>
        public void QuitGame()
        {
            _isRunning = false;
        }

        /// <summary>
        /// Challenge a champion boss the player has found by roaming (replaces the old
        /// menu-driven boss-selection list, per the Pokemon-style Route/Town map design).
        /// TODO-GODOT: call this when the player walks into a champion boss's overworld hitbox,
        /// same trigger shape as OnOverworldEnemyHitboxCollision for regular enemies. Boss
        /// placement on the fixed map isn't designed yet, so nothing calls this today.
        /// </summary>
        public void ChallengeBoss(Entities.Enemies.Bosses.BossEnemy boss)
        {
            void OnCombatEnded(bool victory)
            {
                _combatManager.CombatEnded -= OnCombatEnded;

                if (!victory && _player.Health <= 0)
                {
                    ChangeState(GameState.GameOver);
                }
            }

            _combatManager.CombatEnded += OnCombatEnded;

            World.BossEncounter.StartBossEncounter(
                _player,
                boss,
                _bossManager,
                _combatManager,
                null); // TODO: Add companion support when implemented
        }

        /// <summary>
        /// Challenge the final boss at the Final Gate.
        /// TODO-GODOT: call this when the player reaches the Final Gate's map location
        /// (the "Tyrant's Lair" BossRoom node) instead of from a menu.
        /// </summary>
        public void ChallengeFinalBoss()
        {
            void OnCombatEnded(bool victory)
            {
                _combatManager.CombatEnded -= OnCombatEnded;

                if (victory)
                {
                    Console.WriteLine("\n🎉 YOU HAVE COMPLETED THE GAME! 🎉");
                    Console.WriteLine("You may continue playing to explore or challenge bosses again.");
                }
                else if (_player.Health <= 0)
                {
                    ChangeState(GameState.GameOver);
                }
            }

            _combatManager.CombatEnded += OnCombatEnded;

            World.BossEncounter.StartFinalBossEncounter(
                _player,
                _bossManager,
                _combatManager,
                null); // TODO: Add companion support when implemented
        }

        /// <summary>
        /// Handle game over state
        /// </summary>
        private void HandleGameOver()
        {
            // TODO-GODOT: game over summary + options -> game over screen
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