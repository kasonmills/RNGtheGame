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

        // The final boss has been beaten on this save (kept on every later save)
        private bool _gameCompleted;
        // The opening tutorial has been won - saving is refused until then (core/ to-do 9)
        private bool _tutorialCompleted;
        // The slot this game was last loaded from or saved to - the pre-boss auto-save, the
        // final-boss win and "Load Last Save" all use it. Null for a new game until its first save.
        private string _currentSaveSlot;
        // The game exactly as it was when the final boss fight started; written to the save,
        // marked completed, if the fight is won
        private Data.SaveData _preFinalBossSnapshot;

        // What to do when the current fight ends (set by WatchFightEnd). Only one fight runs at
        // a time, so only one handler is kept - and a reset can always take it off again.
        private Action<CombatResult> _fightEndHandler;

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

        // === Read access for Godot screens ===
        // Screens read and act on these directly and keep no copy of the data (frontend rule
        // decided 2026-10-12). Player, Quests, QuestGiver and Bosses are null until
        // StartNewGame() or a successful LoadGame().

        /// <summary>
        /// The state the game is in right now. Read this on start-up and after a load to pick
        /// the scene; listen to StateChanged for every change after that.
        /// </summary>
        public GameState CurrentState => _currentState;

        /// <summary>
        /// The player: stats, equipped gear, ability, gold and inventory (Player.Inventory).
        /// </summary>
        public Player Player => _player;

        /// <summary>
        /// The party: every recruited companion (RecruitedCompanions), each fighting or benched.
        /// </summary>
        public Entities.NPCs.Companions.PartyManager Party => _partyManager;

        /// <summary>
        /// The world map: nodes, connections and the player's current node.
        /// </summary>
        public MapManager Map => _mapManager;

        /// <summary>
        /// Every quest, the tracked quest, and accept/complete/claim.
        /// </summary>
        public QuestManager Quests => _questManager;

        /// <summary>
        /// The NPC who hands out the boss quests.
        /// </summary>
        public QuestGiver QuestGiver => _questGiver;

        /// <summary>
        /// The bosses, which ones are beaten, and which one is next in the story order.
        /// </summary>
        public Entities.Enemies.Bosses.BossManager Bosses => _bossManager;

        /// <summary>
        /// The current settings. Replaced by the save's settings on LoadGame(), so read it
        /// through this property each time instead of keeping a reference.
        /// </summary>
        public GameSettings Settings => _gameSettings;

        /// <summary>
        /// The numbers for this save's difficulty - enemy strength, XP and gold multipliers,
        /// danger ratings (e.g. for a "Deadly" tag on a boss). Always matches Settings.Difficulty;
        /// replaced on a new game and on LoadGame(), so read it through this property each time.
        /// </summary>
        public DifficultyScaler Difficulty => _combatManager.DifficultyScaler;

        /// <summary>
        /// The statistics for this save file (read only on screen). Replaced on LoadGame(),
        /// same as Settings.
        /// </summary>
        public StatisticsTracker Statistics => _statistics;

        /// <summary>
        /// Whether the final boss has been beaten on this save. A completed save loads to just
        /// before the final boss, so the player can keep playing or fight it again.
        /// </summary>
        public bool IsGameCompleted => _gameCompleted;

        /// <summary>
        /// Whether the opening tutorial (intro + the Skarn fight) has been won. Saving is
        /// refused until it has, so the save screen can grey out its button while this is false.
        /// Always true after a load - a save only exists once the tutorial is done.
        /// </summary>
        public bool IsTutorialCompleted => _tutorialCompleted;

        /// <summary>
        /// Why the last save failed, for a pop-up (null if it worked). Set by SaveGame() and by
        /// the save written when the final boss is beaten.
        /// </summary>
        public string LastSaveError { get; private set; }

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
            ResetPlaythrough();

            var result = _gameStartup.CreateNewGame(playerName, difficulty, ability, startingWeapon, _rngManager, _mapManager, _gameSettings);

            _player = result.Player;
            _bossManager = result.BossManager;
            _questManager = result.QuestManager;
            _questGiver = result.QuestGiver;
            _combatManager.Difficulty = difficulty; // Harder difficulties lower the party's hit chance

            ChangeState(GameState.Cutscene); // Opening intro; FinishIntro() then starts the tutorial fight
            _isRunning = true;
        }

        /// <summary>
        /// End the opening intro cutscene: the starter companion joins and the tutorial fight
        /// against Skarn begins.
        /// TODO-GODOT: call this when the intro scene finishes (or is skipped).
        /// </summary>
        public void FinishIntro()
        {
            RecruitStarterCompanion();
            BeginEagleBearEncounter();
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
        /// Begin the tutorial's first boss fight (Skarn, the Eagle Bear). The game goes into
        /// Combat with the Tutorial state pushed on top while the lessons run; once every
        /// action has been tried the overlay pops back to plain Combat, and the end of the
        /// fight goes to Playing (decided 2026-10-28).
        /// Normally reached through FinishIntro().
        /// TODO-GODOT: during this fight, submit actions via GameManager.Instance.Tutorial.TrySubmitAction(...)
        /// instead of Combat.SubmitPlayerAction(...) directly.
        /// </summary>
        public void BeginEagleBearEncounter()
        {
            var eagleBear = _tutorialManager.CreateEagleBearEncounter(_bossManager);

            // A new player starts with no items, so give one for the tutorial's "use an item"
            // step (core/ to-do 1, decided 2026-10-13)
            _player.Inventory.AddItem(Items.ItemDatabase.GetConsumable("minor health potion"));

            ChangeState(GameState.Combat);
            PushState(GameState.Tutorial);

            void OnLessonsFinished()
            {
                _tutorialManager.ActionLessonsFinished -= OnLessonsFinished;
                if (_currentState == GameState.Tutorial)
                {
                    PopState(); // Back to plain Combat - the fight carries on
                }
            }

            void OnCombatFinished(CombatResult result)
            {
                _tutorialManager.ActionLessonsFinished -= OnLessonsFinished;

                // The fight can end before every lesson is done (e.g. Chance lands the last hit)
                if (_currentState == GameState.Tutorial)
                {
                    PopState();
                }

                // Tutorial complete (or lost) - the tutorial refuses Flee, so only win/lose happen.
                // Only a win finishes it; after a loss the player must play it again.
                if (result.Outcome == CombatOutcome.Won)
                {
                    _tutorialCompleted = true;
                }

                ChangeState(result.Outcome == CombatOutcome.Lost ? GameState.GameOver : GameState.Playing);
            }

            _tutorialManager.ActionLessonsFinished += OnLessonsFinished;
            WatchFightEnd(OnCombatFinished);

            _combatManager.StartCombat(_player, new List<Entities.Enemies.EnemyBase> { eagleBear }, _partyManager.RecruitedCompanions, _bossManager);
        }

        /// <summary>
        /// Load a saved game from the given slot. The previous playthrough is wiped first
        /// (ResetPlaythrough), then everything is filled in from the file. Returns false if
        /// there is no save there or it could not be read - then nothing is wiped and the game
        /// stays where it was; the load screen shows the message.
        /// Data.SaveManager.SaveExists()/GetSaveFileInfo() list what is in each slot.
        /// </summary>
        public bool LoadGame(string saveSlotName = "save1")
        {
            // Read the save into a fresh map first - the current game is only wiped once the
            // file has been read, so a bad save never costs the player the game they're in
            var loadedMap = new MapManager();
            var result = _gameStartup.LoadGame(saveSlotName, _rngManager, loadedMap);

            if (result.Success)
            {
                ResetPlaythrough(loadedMap);

                _player = result.Player;
                _gameSettings = result.GameSettings;
                _combatManager.Difficulty = _gameSettings.Difficulty; // Harder difficulties lower the party's hit chance
                _statistics = result.Statistics;
                _bossManager = result.BossManager;
                _questManager = result.QuestManager;
                _questGiver = result.QuestGiver;
                _gameCompleted = result.GameCompleted;
                // A save can only be made after the tutorial, so a loaded game never goes back
                // through it (SaveData.TutorialCompleted is always true; even a hand-edited false
                // is ignored, so it can't lock the player out of saving)
                _tutorialCompleted = true;
                _partyManager.RestoreRoster(result.Companions);
                _currentSaveSlot = saveSlotName;

                ChangeState(GameState.Playing);
                _isRunning = true;
                return true;
            }

            // Nothing was wiped and the state is left alone, so the player stays where they
            // were (the main menu, or the game they loaded from)
            return false;
        }

        /// <summary>
        /// Wipe everything that belongs to a playthrough, so nothing from the previous game
        /// carries into the next one (core/ to-do 5, decided 2026-11-03). Both StartNewGame()
        /// and LoadGame() run this first - a load then fills everything back in from the file.
        /// Kept on purpose: the player's preferences in Settings (volume, text speed and so on)
        /// carry over to a new game; only Difficulty is set per save. A load still takes the
        /// save's preferences, since the save is the only place they are stored for now.
        /// Systems are replaced with new objects rather than cleared, so anything added to
        /// them later is reset too without having to remember it here.
        /// </summary>
        private void ResetPlaythrough(MapManager map = null)
        {
            // A fight left running (e.g. quit to the main menu mid-fight) is dropped, and its
            // end handler taken off so it can't fire on the next game's first fight
            StopWatchingFightEnd();
            _combatManager.AbandonCombat();

            // The playthrough itself - set again by the new game or the load
            _player = null;
            _bossManager = null;
            _questManager = null;
            _questGiver = null;

            _partyManager = new Entities.NPCs.Companions.PartyManager();
            _statistics = new StatisticsTracker();
            _tutorialManager = new TutorialManager();
            _mapManager = map ?? new MapManager();
            _activeOverworldEnemies = new List<World.OverworldEnemySpawn>();
            _stateStack = new Stack<GameState>();
            CurrentShop = null;
            CurrentBlacksmith = null;
            CurrentDialogueNpc = null;

            Entities.NPCs.ShopKeeper.ClearAllShops();
            _rngManager.ResetStatistics();

            _gameCompleted = false;
            _tutorialCompleted = false;
            _preFinalBossSnapshot = null;
            _currentSaveSlot = null;
            LastSaveError = null;
        }

        /// <summary>
        /// Run the given handler once, when the fight about to start ends. Replaces any
        /// handler still waiting from an earlier fight.
        /// </summary>
        private void WatchFightEnd(Action<CombatResult> onFinished)
        {
            StopWatchingFightEnd();

            void Handler(CombatResult result)
            {
                StopWatchingFightEnd();
                onFinished(result);
            }

            _fightEndHandler = Handler;
            _combatManager.CombatFinished += _fightEndHandler;
        }

        private void StopWatchingFightEnd()
        {
            if (_fightEndHandler != null)
            {
                _combatManager.CombatFinished -= _fightEndHandler;
                _fightEndHandler = null;
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
        /// The action methods below (Explore, Rest, SaveGame, and the Open/Close pairs for the
        /// party menu, quest giver, quest log and statistics) are still valid and will be called by
        /// player movement/interaction signals (walking onto an encounter tile, pressing an interact
        /// key near an NPC, opening a UI panel, etc.) instead of a numbered console choice.
        /// Boss challenges no longer go through a menu at all - the player finds
        /// bosses by roaming, same as regular overworld encounters.
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
            // Console.WriteLine("7. ⚔️  Boss Challenges (Boss Fights)");
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
            GameLog.Write("\nYou venture deeper into the unknown...\n");
            
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
                GameLog.Write("You find nothing of interest...");
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
            GameLog.Write($"A {enemy.Name} appears!\n");
            GameLog.Write(enemy.GetDescription());

            ChangeState(GameState.Combat); // Combat always enters from Playing and returns to Playing/GameOver

            // Won, party fled or the enemy fled - back to the overworld. Only a loss
            // (the whole party down) is Game Over.
            WatchFightEnd(result => ChangeState(result.Outcome == CombatOutcome.Lost ? GameState.GameOver : GameState.Playing));

            // CombatManager handles the actual combat (including XP and loot rewards)
            _combatManager.StartCombat(_player, new List<Entities.Enemies.EnemyBase> { enemy }, _partyManager.RecruitedCompanions);
        }

        /// <summary>
        /// Open a treasure chest - called by Explore() or MapManager. Rolls the chest loot
        /// table, adds the gold and every item that fits to the bag, and returns what
        /// happened so the frontend can show a loot popup. Items that don't fit (bag full)
        /// come back in ItemsLeftBehind and stay in the chest.
        /// TODO-GODOT: show the returned loot in a popup; keep ItemsLeftBehind on the chest
        /// so the player can come back for them.
        /// </summary>
        public Progression.LootResult TriggerLootEvent()
        {
            // Chest loot rolls through RNGManager (via the adapter) so it respects the
            // player's chosen RNG algorithm and counts toward RNG statistics, same as
            // every other roll in the game.
            var rngAdapter = new Systems.RNGManagerRandomAdapter(_rngManager);
            var lootGenerator = new Progression.LootGenerator(rngAdapter);
            var lootTable = Progression.LootTableTemplates.CreateChestLoot();
            var loot = lootGenerator.GenerateLoot(lootTable, _player.Level);

            _player.Gold += loot.Gold;

            foreach (var item in loot.Items.ToList())
            {
                // All or nothing - anything that doesn't fit (bag full, or a stack would go
                // over 99) stays in the chest; the inventory's ItemNotAdded event tells the player
                if (!_player.AddToInventory(item))
                {
                    loot.Items.Remove(item);
                    loot.ItemsLeftBehind.Add(item);
                }
            }

            return loot;
        }

        // ------------------------------------------------------------------
        // Overlay screens (core/ to-do 17, 2026-11-03)
        // ------------------------------------------------------------------
        // Every screen below is an overlay: Open pushes its state on top of the current one,
        // Close pops back to exactly where the player was. Each Open only works from the
        // states listed on it and returns false otherwise (nothing changes), so a screen can
        // never open in the wrong place - e.g. a shop in the middle of a fight. Each Close only
        // works while that screen is the one on top. None of them print anything.
        // TODO-GODOT: SceneRouter opens/closes the matching overlay scene on StateChanged.

        /// <summary>
        /// The shopkeeper whose shop is open (null when no shop is open).
        /// </summary>
        public ShopKeeper CurrentShop { get; private set; }

        /// <summary>
        /// The blacksmith whose screen is open (null when it isn't open).
        /// </summary>
        public Blacksmith CurrentBlacksmith { get; private set; }

        /// <summary>
        /// The NPC being talked to (null when no conversation is open).
        /// </summary>
        public NPCBase CurrentDialogueNpc { get; private set; }

        private bool OpenOverlay(GameState overlay, params GameState[] allowedFrom)
        {
            if (Array.IndexOf(allowedFrom, _currentState) < 0)
            {
                return false;
            }

            PushState(overlay);
            return true;
        }

        private bool CloseOverlay(GameState overlay)
        {
            if (_currentState != overlay)
            {
                return false;
            }

            PopState();
            return true;
        }

        /// <summary>
        /// Pause the game. Opens from the world, a fight or the tutorial fight.
        /// TODO-GODOT: call this from the pause key/button.
        /// </summary>
        public bool OpenPauseMenu() => OpenOverlay(GameState.Paused, GameState.Playing, GameState.Combat, GameState.Tutorial);

        /// <summary>
        /// Resume - back to whatever was paused.
        /// </summary>
        public bool ClosePauseMenu() => CloseOverlay(GameState.Paused);

        /// <summary>
        /// The party menu: the one menu for the player, inventory and companions (decided
        /// 2026-10-12; replaces the old Inventory and Stats screens). Opens from the world or
        /// the pause menu - not during a fight, which has its own buttons. Reads Player,
        /// Player.Inventory and Party.
        /// TODO-GODOT: call this from the party menu key/button.
        /// </summary>
        public bool OpenPartyMenu() => OpenOverlay(GameState.PartyMenu, GameState.Playing, GameState.Paused);

        public bool ClosePartyMenu() => CloseOverlay(GameState.PartyMenu);

        /// <summary>
        /// The quest log (every quest, the tracked quest). Opens from the world or the pause
        /// menu. Reads Quests.
        /// </summary>
        public bool OpenQuestLog() => OpenOverlay(GameState.QuestLog, GameState.Playing, GameState.Paused);

        public bool CloseQuestLog() => CloseOverlay(GameState.QuestLog);

        /// <summary>
        /// The statistics page for this save (read only). Opens from the world or the pause
        /// menu. Reads Statistics.
        /// </summary>
        public bool OpenStatistics() => OpenOverlay(GameState.Statistics, GameState.Playing, GameState.Paused);

        public bool CloseStatistics() => CloseOverlay(GameState.Statistics);

        /// <summary>
        /// The settings menu. Opens from the main menu or the pause menu. Reads and changes
        /// Settings.
        /// </summary>
        public bool OpenSettings() => OpenOverlay(GameState.Settings, GameState.MainMenu, GameState.Paused);

        public bool CloseSettings() => CloseOverlay(GameState.Settings);

        /// <summary>
        /// The save screen. Opens from the world only, and only once the tutorial is won.
        /// TODO: also require standing at a save point (an inn or a marked campsite, core/
        /// to-do 10) once the fixed map places them. Calls SaveGame(slot).
        /// TODO-GODOT: call this when the player uses a save point.
        /// </summary>
        public bool OpenSaveScreen()
        {
            if (!_tutorialCompleted)
            {
                return false;
            }

            return OpenOverlay(GameState.SaveScreen, GameState.Playing);
        }

        public bool CloseSaveScreen() => CloseOverlay(GameState.SaveScreen);

        /// <summary>
        /// Talk to an NPC. Opens from the world. The screen reads CurrentDialogueNpc.
        /// TODO-GODOT: call this when the player interacts with an NPC who only talks.
        /// The NPC's lines aren't designed yet (frontend plan: "source to design").
        /// </summary>
        public bool OpenDialogue(NPCBase npc)
        {
            if (npc == null || !OpenOverlay(GameState.Dialogue, GameState.Playing))
            {
                return false;
            }

            CurrentDialogueNpc = npc;
            return true;
        }

        public bool CloseDialogue()
        {
            if (!CloseOverlay(GameState.Dialogue)) return false;
            CurrentDialogueNpc = null;
            return true;
        }

        /// <summary>
        /// Open a shopkeeper's shop. Opens from the world. The screen reads CurrentShop.
        /// TODO-GODOT: call this when the player interacts with a shopkeeper.
        /// </summary>
        public bool OpenShop(ShopKeeper shop)
        {
            if (shop == null || !OpenOverlay(GameState.Shopping, GameState.Playing))
            {
                return false;
            }

            CurrentShop = shop;
            return true;
        }

        public bool CloseShop()
        {
            if (!CloseOverlay(GameState.Shopping)) return false;
            CurrentShop = null;
            return true;
        }

        /// <summary>
        /// Open a blacksmith's upgrade screen. Opens from the world. The screen reads
        /// CurrentBlacksmith.
        /// TODO-GODOT: call this when the player interacts with a blacksmith.
        /// </summary>
        public bool OpenBlacksmith(Blacksmith blacksmith)
        {
            if (blacksmith == null || !OpenOverlay(GameState.Blacksmith, GameState.Playing))
            {
                return false;
            }

            CurrentBlacksmith = blacksmith;
            return true;
        }

        public bool CloseBlacksmith()
        {
            if (!CloseOverlay(GameState.Blacksmith)) return false;
            CurrentBlacksmith = null;
            return true;
        }

        /// <summary>
        /// The quest giver's screen (the boss quests). Opens from the world. Reads QuestGiver
        /// and Quests.
        /// TODO-GODOT: call this when the player talks to the quest giver.
        /// </summary>
        public bool OpenQuestGiver() => OpenOverlay(GameState.QuestGiver, GameState.Playing);

        public bool CloseQuestGiver() => CloseOverlay(GameState.QuestGiver);

        // No job board in the MVP (decided 2026-11-03): each shop/NPC hands out its own unlock
        // quest, so there is no board to open.

        /// <summary>
        /// Rest to restore health
        /// </summary>
        private void Rest()
        {
            GameLog.Write("You take a moment to rest...");

            int healAmount = _player.MaxHealth / 2;
            _player.Heal(healAmount);

            GameLog.Write("You feel refreshed!");
        }

        /// <summary>
        /// Save the current game to the given slot. Returns success/failure instead of
        /// printing or blocking on a keypress, so the save screen can show the result.
        /// TODO-GODOT: call this from the save screen at a save point (saving is only
        /// allowed at save points - to-do 10 under core/; only the tutorial part of that is
        /// enforced so far). Refused, with LastSaveError set, until the tutorial is won.
        /// With no slot given it saves to this game's current slot (or a free one if this
        /// game has never been saved), so it can't overwrite another playthrough by accident.
        /// </summary>
        public bool SaveGame(string saveSlotName = null)
        {
            if (!_tutorialCompleted)
            {
                LastSaveError = "You can't save until the tutorial is finished.";
                return false;
            }

            return WriteSave(saveSlotName ?? _currentSaveSlot ?? FindFreeSaveSlot());
        }

        /// <summary>
        /// The automatic save made right before every boss fight (decided 2026-11-03) - the
        /// only place the game saves by itself; everywhere else the player has to find a save
        /// point. Writes to this game's current slot (a free one if it has never been saved),
        /// so losing the fight and loading the last save puts the player right before the boss.
        /// Skipped during the tutorial (no saving until it's won). Returns false and sets
        /// LastSaveError if the save failed - the fight still goes ahead.
        /// </summary>
        private bool AutoSaveBeforeBoss()
        {
            if (!_tutorialCompleted)
            {
                return false;
            }

            return WriteSave(_currentSaveSlot ?? FindFreeSaveSlot());
        }

        /// <summary>
        /// Write the game to a slot with no save-point check - shared by SaveGame() and the
        /// pre-boss auto-save. A successful write makes it this game's current slot.
        /// </summary>
        private bool WriteSave(string saveSlotName)
        {
            // Update statistics before saving
            _statistics.UpdatePlayTime(_player.PlayTime);
            _statistics.UpdateCurrentGold(_player.Gold);
            _statistics.UpdateCurrentLevel(_player.Level);
            _statistics.RecordGameSave();

            bool saved = Data.SaveManager.SaveGame(_player, saveSlotName, out string error, _bossManager, _mapManager, _questManager, _gameSettings, _statistics, _gameCompleted, _partyManager);
            LastSaveError = error;
            if (saved)
                _currentSaveSlot = saveSlotName;
            return saved;
        }

        /// <summary>
        /// The first "saveN" slot with nothing in it, for a game that has never been saved.
        /// TODO-GODOT: if the save screen lets the player pick a slot for a new game, pass that
        /// to SaveGame() instead.
        /// </summary>
        private static string FindFreeSaveSlot()
        {
            int number = 1;
            while (Data.SaveManager.SaveExists($"save{number}"))
            {
                number++;
            }
            return $"save{number}";
        }

        /// <summary>
        /// "Load Last Save" on the Game Over screen: reload this game's current slot - the last
        /// save point it was saved at, or right before the boss if a boss fight was lost (every
        /// defeat goes back to the last save, decided 2026-11-03). Returns false if this game
        /// has no save yet (e.g. losing the tutorial) - the screen should then offer a new game
        /// or the main menu instead.
        /// TODO-GODOT: call this from the Game Over screen.
        /// </summary>
        public bool LoadLastSave()
        {
            if (_currentSaveSlot == null)
            {
                return false;
            }

            return LoadGame(_currentSaveSlot);
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
            CurrentShop = null;
            CurrentBlacksmith = null;
            CurrentDialogueNpc = null;
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
            if (!World.BossEncounter.CanStartBossEncounter(boss, _bossManager, out string reason))
            {
                // TODO-GODOT: show the reason (locked / already beaten) as a message
                GameLog.Write(reason);
                return;
            }

            // Auto-save right before the fight (decided 2026-11-03).
            // TODO-GODOT: if this returns false, show LastSaveError in a pop-up
            AutoSaveBeforeBoss();

            ChangeState(GameState.Combat);

            WatchFightEnd(result => ChangeState(result.Outcome == CombatOutcome.Lost ? GameState.GameOver : GameState.Playing));

            World.BossEncounter.StartBossEncounter(
                _player,
                boss,
                _bossManager,
                _combatManager,
                _partyManager.RecruitedCompanions);
        }

        /// <summary>
        /// Challenge the final boss at the Final Gate.
        /// TODO-GODOT: call this when the player reaches the Final Gate's map location
        /// (the "Tyrant's Lair" BossRoom node) instead of from a menu.
        /// </summary>
        public void ChallengeFinalBoss()
        {
            if (!World.BossEncounter.CanStartBossEncounter(_bossManager.GetFinalBoss(), _bossManager, out string reason))
            {
                // TODO-GODOT: show the reason as a message
                GameLog.Write(reason);
                return;
            }

            // Auto-save right before the fight, like every boss (decided 2026-11-03) - a loss
            // and "Load Last Save" puts the player back here.
            // TODO-GODOT: if this returns false, show LastSaveError in a pop-up
            AutoSaveBeforeBoss();

            // Snapshot the game as it is right now, before the fight changes anything. A win
            // writes this to the save marked completed, so loading it puts the player back
            // here - free to keep playing or fight the final boss again (decided 2026-10-30).
            _preFinalBossSnapshot = Data.SaveManager.CreateSaveData(_player, _bossManager, _mapManager, _questManager, _gameSettings, _statistics, gameCompleted: true, party: _partyManager);

            ChangeState(GameState.Combat);

            void OnCombatFinished(CombatResult result)
            {
                if (result.Outcome == CombatOutcome.Lost)
                {
                    _preFinalBossSnapshot = null;
                    ChangeState(GameState.GameOver);
                    return;
                }

                if (result.Outcome == CombatOutcome.Won)
                {
                    _gameCompleted = true;
                    // TODO-GODOT: if this fails, show LastSaveError in a pop-up on the ending screen
                    // The pre-boss auto-save normally set the slot; if it failed and this game
                    // was never saved, use a free slot rather than another playthrough's
                    _currentSaveSlot ??= FindFreeSaveSlot();
                    Data.SaveManager.WriteSaveData(_preFinalBossSnapshot, _currentSaveSlot, out string error);
                    LastSaveError = error;
                    _preFinalBossSnapshot = null;

                    // Ending, then credits (FinishEnding / FinishCredits), then the main menu
                    ChangeState(GameState.Ending);
                    return;
                }

                _preFinalBossSnapshot = null;
                ChangeState(GameState.Playing);
            }

            WatchFightEnd(OnCombatFinished);

            World.BossEncounter.StartFinalBossEncounter(
                _player,
                _bossManager,
                _combatManager,
                _partyManager.RecruitedCompanions);
        }

        /// <summary>
        /// Text for the ending scene. PLACEHOLDER - the story content isn't written yet.
        /// </summary>
        public string GetEndingText()
        {
            return "The final battle is won. (Ending not written yet.)";
        }

        /// <summary>
        /// Text for the credits roll. PLACEHOLDER - to be filled in before release.
        /// </summary>
        public string GetCreditsText()
        {
            return "RNG the Game\n\n(Credits not written yet.)";
        }

        /// <summary>
        /// The ending has finished playing - roll the credits.
        /// TODO-GODOT: call this when the ending scene finishes or the player skips it.
        /// </summary>
        public void FinishEnding()
        {
            if (_currentState != GameState.Ending)
                return;

            ChangeState(GameState.Credits);
        }

        /// <summary>
        /// The credits have finished - back to the main menu.
        /// TODO-GODOT: call this when the credits scene finishes or the player skips it.
        /// </summary>
        public void FinishCredits()
        {
            if (_currentState != GameState.Credits)
                return;

            QuitToMainMenu();
        }

        /// <summary>
        /// Handle game over state
        /// </summary>
        private void HandleGameOver()
        {
            // TODO-GODOT: the Game Over screen shows the summary (level, gold) and its buttons
            // call LoadLastSave() / ChangeState(GameState.MainMenu) directly. The old console
            // menu is commented out (2026-11-10 - no console output); kept for reference:
            // Console.WriteLine("\n=== GAME OVER ===");
            // Console.WriteLine($"You reached level {_player.Level}");
            // Console.WriteLine($"You collected {_player.Gold} gold");
            //
            // Console.WriteLine("\n1. Load Last Save");
            // Console.WriteLine("2. Return to Main Menu");
            // Console.WriteLine("3. Quit");
            //
            // Console.Write("\nChoice: ");
            // string choice = Console.ReadLine();
            //
            // switch (choice)
            // {
            //     case "1":
            //         if (!LoadLastSave()) ChangeState(GameState.MainMenu);
            //         break;
            //     case "2":
            //         ChangeState(GameState.MainMenu);
            //         break;
            //     case "3":
            //         _isRunning = false;
            //         break;
            //     default:
            //         Console.WriteLine("Invalid choice.");
            //         HandleGameOver(); // Ask again
            //         break;
            // }
        }

    }
}