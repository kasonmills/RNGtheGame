using System;
using GameLogic.Entities.Player;
using GameLogic.World;
using GameLogic.Systems;
using GameLogic.Combat;
using GameLogic.Data;
using GameLogic.Quests;
using GameLogic.Abilities;

namespace GameLogic.Core
{
    /// <summary>
    /// Bundle of the long-lived systems built once at boot.
    /// </summary>
    public class GameSystems
    {
        public RNGManager RngManager { get; set; }
        public GameSettings GameSettings { get; set; }
        public StatisticsTracker Statistics { get; set; }
        public MapManager MapManager { get; set; }
        public CombatManager CombatManager { get; set; }
    }

    /// <summary>
    /// Bundle of a play session (new or loaded).
    /// </summary>
    public class GameStartupResult
    {
        public bool Success { get; set; } = true;
        public Player Player { get; set; }
        public Entities.Enemies.Bosses.BossManager BossManager { get; set; }
        public QuestManager QuestManager { get; set; }
        public QuestGiver QuestGiver { get; set; }
        public GameSettings GameSettings { get; set; }
        public StatisticsTracker Statistics { get; set; }
    }

    /// <summary>
    /// Boots the game's long-lived systems and builds/loads a play session.
    /// Stateless - every method takes what it needs as parameters, so each
    /// is independently testable without a GameManager.
    /// </summary>
    public class GameStartup
    {
        /// <summary>
        /// Initialize all long-lived game systems
        /// </summary>
        public GameSystems InitializeSystems()
        {
            var rngManager = new RNGManager();
            var gameSettings = new GameSettings(); // Initialize with default settings
            var statistics = new StatisticsTracker(); // Initialize statistics tracker
            SaveManager.Initialize(); // Initialize static SaveManager
            var mapManager = new MapManager();
            var combatManager = new CombatManager(rngManager);

            return new GameSystems
            {
                RngManager = rngManager,
                GameSettings = gameSettings,
                Statistics = statistics,
                MapManager = mapManager,
                CombatManager = combatManager
            };
        }

        /// <summary>
        /// Start a brand new game
        /// </summary>
        public GameStartupResult CreateNewGame(RNGManager rngManager, MapManager mapManager, GameSettings gameSettings)
        {
            Console.Clear();
            // TODO-GODOT: landing/title screen text -> main menu scene
            Console.WriteLine("=== Welcome to RNG: The Game ===\n");

            // Get player name
            Console.Write("Enter your character's name: ");
            string playerName = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(playerName))
            {
                playerName = "Hero";
            }

            // Create new player
            var player = new Player(playerName);

            Console.WriteLine($"\nWelcome, {player.Name}!");

            // Select difficulty (once per save file, immutable)
            gameSettings.Difficulty = SelectDifficulty();

            // Let player choose starting ability
            player.SetAbility(ChooseStartingAbility());

            // Register the boss roster (currently empty pending the 9-boss redesign).
            // TODO: no final-boss assignment happens here anymore - SelectRandomFinalBoss()
            // is being retired since the final boss will be a fixed, specific boss once the
            // roster/order redesign lands, not a random pick.
            var bossManager = new Entities.Enemies.Bosses.BossManager();
            var bosses = Entities.Enemies.Bosses.BossDefinitions.GetAllChampionBosses();
            bossManager.RegisterBosses(bosses.ToArray());

            Console.WriteLine("\nYour bosses await your challenge!\n");

            // Initialize quest system
            var questManager = new QuestManager();
            InitializeQuests(questManager, bossManager, rngManager);
            var questGiver = new QuestGiver("Veteran Ranger", questManager, bossManager);
            Console.WriteLine("Quests are now available at the Job Board and Quest Giver!\n");

            Console.WriteLine("\nYour adventure begins...\n");

            // Generate starting map
            mapManager.GenerateNewMap();

            return new GameStartupResult
            {
                Player = player,
                BossManager = bossManager,
                QuestManager = questManager,
                QuestGiver = questGiver,
                GameSettings = gameSettings
            };
        }

        /// <summary>
        /// Load an existing saved game
        /// </summary>
        public GameStartupResult LoadGame(string saveSlot, RNGManager rngManager, MapManager mapManager)
        {
            SaveData saveData = SaveManager.LoadGame(saveSlot);

            if (saveData == null)
            {
                return new GameStartupResult { Success = false };
            }

            var player = Player.LoadFromSave(saveData);

            // Load game settings from save data
            var gameSettings = SaveManager.LoadSettingsFromSaveData(saveData);

            // Load statistics from save data
            var statistics = SaveManager.LoadStatisticsFromSaveData(saveData);

            // Apply RNG settings
            rngManager.SetStatisticsTracking(gameSettings.RngStatisticsTracking);
            if (!string.IsNullOrEmpty(gameSettings.RngAlgorithm))
            {
                rngManager.SwitchAlgorithm(gameSettings.RngAlgorithm);
            }

            // Load boss manager from save data
            var bossManager = SaveManager.LoadBossManager(saveData, rngManager);

            // Initialize quest system and reconstruct quests from save
            var questManager = new QuestManager();
            if (saveData.Quests != null && saveData.Quests.Count > 0)
            {
                // Reconstruct quests with their original RNG values from save
                QuestSerializationHelper.ReconstructQuests(questManager, saveData.Quests, bossManager);
                if (!string.IsNullOrEmpty(saveData.ActiveQuestId))
                {
                    questManager.SetActiveQuestId(saveData.ActiveQuestId);
                }
            }
            else
            {
                // Fallback: If no quests in save (old save file), initialize new quests
                InitializeQuests(questManager, bossManager, rngManager);
            }
            var questGiver = new QuestGiver("Veteran Ranger", questManager, bossManager);

            // Map layout is fixed, so just regenerate it and restore the player's position
            mapManager.GenerateNewMap();
            mapManager.SetCurrentNode(saveData.CurrentMapNodeId);

            Console.WriteLine($"Welcome back, {player.Name}!");

            return new GameStartupResult
            {
                Player = player,
                BossManager = bossManager,
                QuestManager = questManager,
                QuestGiver = questGiver,
                GameSettings = gameSettings,
                Statistics = statistics
            };
        }

        /// <summary>
        /// Let the player select difficulty (immutable after creation)
        /// </summary>
        private DifficultyLevel SelectDifficulty()
        {
            // TODO-GODOT: difficulty options + descriptions below -> difficulty-select screen (radio buttons + info panel)
            Console.WriteLine("=== Select Difficulty ===");
            Console.WriteLine("This choice is permanent for this save file and cannot be changed!\n");

            Console.WriteLine("1. Normal");
            Console.WriteLine("   - Enemies have 75% stats");
            Console.WriteLine("   - Rewards are 80% of normal");
            Console.WriteLine("   - Recommended for learning the game\n");

            Console.WriteLine("2. Hard (Recommended)");
            Console.WriteLine("   - Balanced gameplay");
            Console.WriteLine("   - Standard enemies and rewards");
            Console.WriteLine("   - The intended experience\n");

            Console.WriteLine("3. Difficult");
            Console.WriteLine("   - Enemies have 150% stats");
            Console.WriteLine("   - Rewards are 130% of normal");
            Console.WriteLine("   - For experienced players\n");

            Console.WriteLine("4. Unfair");
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

            DifficultyLevel difficulty = choice switch
            {
                1 => DifficultyLevel.Normal,
                2 => DifficultyLevel.Hard,
                3 => DifficultyLevel.Difficult,
                4 => DifficultyLevel.Unfair,
                _ => DifficultyLevel.Hard
            };

            Console.WriteLine($"\nDifficulty set to: {difficulty}");
            Console.WriteLine("Remember: This cannot be changed for this save file!\n");

            return difficulty;
        }

        /// <summary>
        /// Let the player choose their permanent starting ability
        /// </summary>
        private Ability ChooseStartingAbility()
        {
            // TODO-GODOT: ability list + descriptions below -> ability-select screen (cards/list + info panel)
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

            return abilities[choice - 1];
        }

        /// <summary>
        /// Initialize all quests for the game (ONLY called on new game creation)
        /// </summary>
        private void InitializeQuests(QuestManager questManager, Entities.Enemies.Bosses.BossManager bossManager, RNGManager rngManager)
        {
            // Create boss defeat quests for each boss (except final boss)
            foreach (var boss in bossManager.AllBosses.Values)
            {
                if (boss.BossId != bossManager.FinalBossId)
                {
                    // Boss quests: 100 gold + 50 XP per quest (no RNG - these are fixed)
                    var bossQuest = new BossDefeatQuest(boss.BossId, boss.Name, 100, 50);
                    bossQuest.Discover(); // Make available immediately
                    questManager.RegisterQuest(bossQuest);
                }
            }

            // Create final boss quest (no RNG - this is fixed)
            var finalBoss = bossManager.GetFinalBoss();
            if (finalBoss != null)
            {
                var finalQuest = new FinalBossQuest(finalBoss.BossId, finalBoss.Name);
                finalQuest.Discover(); // Make available immediately
                questManager.RegisterQuest(finalQuest);
            }

            // Create level progression quests (no RNG - fixed milestones)
            int[] levelMilestones = { 5, 10, 15, 20, 25 };
            foreach (int level in levelMilestones)
            {
                // Add RNG to rewards: ±20% variation
                int baseGold = level * 50;
                int baseXP = level * 25;
                int gold = ApplyRewardVariation(rngManager, baseGold, 0.2);
                int xp = ApplyRewardVariation(rngManager, baseXP, 0.2);

                var levelQuest = new LevelQuest(level, gold, xp);
                levelQuest.Discover();
                questManager.RegisterQuest(levelQuest);
            }

            // Create enemy kill quests with RNG on requirements and rewards
            // Tier 1: 10 enemies (±6 = 4-16 range)
            int tier1Kills = ApplyRequirementVariation(rngManager, 10, 6);
            int tier1Gold = ApplyRewardVariation(rngManager, 75, 0.3);  // ±30% variation
            int tier1XP = ApplyRewardVariation(rngManager, 40, 0.3);
            var enemyQuest1 = new EnemyKillQuest("kill_tier1_enemies", "Novice Hunter", tier1Kills, tier1Gold, tier1XP);
            enemyQuest1.Discover();
            questManager.RegisterQuest(enemyQuest1);

            // Tier 2: 25 enemies (±6 = 19-31 range)
            int tier2Kills = ApplyRequirementVariation(rngManager, 25, 6);
            int tier2Gold = ApplyRewardVariation(rngManager, 150, 0.3);
            int tier2XP = ApplyRewardVariation(rngManager, 75, 0.3);
            var enemyQuest2 = new EnemyKillQuest("kill_tier2_enemies", "Experienced Hunter", tier2Kills, tier2Gold, tier2XP);
            enemyQuest2.Discover();
            questManager.RegisterQuest(enemyQuest2);

            // Tier 3: 50 enemies (±6 = 44-56 range)
            int tier3Kills = ApplyRequirementVariation(rngManager, 50, 6);
            int tier3Gold = ApplyRewardVariation(rngManager, 300, 0.3);
            int tier3XP = ApplyRewardVariation(rngManager, 150, 0.3);
            var enemyQuest3 = new EnemyKillQuest("kill_tier3_enemies", "Master Hunter", tier3Kills, tier3Gold, tier3XP);
            enemyQuest3.Discover();
            questManager.RegisterQuest(enemyQuest3);

            // Create gold collection quests with RNG on requirements and rewards
            // Tier 1: 500 gold (±6 * 25 = ±150 = 350-650 range)
            int tier1GoldReq = ApplyRequirementVariation(rngManager, 500, 6 * 25);
            int tier1GoldReward = ApplyRewardVariation(rngManager, 100, 0.3);
            int tier1GoldXP = ApplyRewardVariation(rngManager, 50, 0.3);
            var goldQuest1 = new GoldCollectionQuest(tier1GoldReq, tier1GoldReward, tier1GoldXP);
            goldQuest1.Discover();
            questManager.RegisterQuest(goldQuest1);

            // Tier 2: 1000 gold (±150 = 850-1150 range)
            int tier2GoldReq = ApplyRequirementVariation(rngManager, 1000, 150);
            int tier2GoldReward = ApplyRewardVariation(rngManager, 200, 0.3);
            int tier2GoldXP = ApplyRewardVariation(rngManager, 100, 0.3);
            var goldQuest2 = new GoldCollectionQuest(tier2GoldReq, tier2GoldReward, tier2GoldXP);
            goldQuest2.Discover();
            questManager.RegisterQuest(goldQuest2);

            // Tier 3: 2500 gold (±300 = 2200-2800 range)
            int tier3GoldReq = ApplyRequirementVariation(rngManager, 2500, 300);
            int tier3GoldReward = ApplyRewardVariation(rngManager, 500, 0.3);
            int tier3GoldXP = ApplyRewardVariation(rngManager, 250, 0.3);
            var goldQuest3 = new GoldCollectionQuest(tier3GoldReq, tier3GoldReward, tier3GoldXP);
            goldQuest3.Discover();
            questManager.RegisterQuest(goldQuest3);

            // Create weapon upgrade quests with RNG on requirements and rewards
            // Tier 1: Level 5 (±2 = 3-7 range)
            int tier1WeaponLevel = ApplyRequirementVariation(rngManager, 5, 2);
            int tier1WeaponGold = ApplyRewardVariation(rngManager, 150, 0.3);
            int tier1WeaponXP = ApplyRewardVariation(rngManager, 75, 0.3);
            var weaponQuest1 = new WeaponUpgradeQuest(tier1WeaponLevel, tier1WeaponGold, tier1WeaponXP);
            weaponQuest1.Discover();
            questManager.RegisterQuest(weaponQuest1);

            // Tier 2: Level 10 (±3 = 7-13 range)
            int tier2WeaponLevel = ApplyRequirementVariation(rngManager, 10, 3);
            int tier2WeaponGold = ApplyRewardVariation(rngManager, 300, 0.3);
            int tier2WeaponXP = ApplyRewardVariation(rngManager, 150, 0.3);
            var weaponQuest2 = new WeaponUpgradeQuest(tier2WeaponLevel, tier2WeaponGold, tier2WeaponXP);
            weaponQuest2.Discover();
            questManager.RegisterQuest(weaponQuest2);

            // Create equipment quests with RNG on requirements and rewards
            // Tier 1: Level 3 (±1 = 2-4 range)
            int tier1EquipLevel = ApplyRequirementVariation(rngManager, 3, 1);
            int tier1EquipGold = ApplyRewardVariation(rngManager, 100, 0.3);
            int tier1EquipXP = ApplyRewardVariation(rngManager, 50, 0.3);
            var equipQuest1 = new EquipmentQuest(tier1EquipLevel, tier1EquipGold, tier1EquipXP);
            equipQuest1.Discover();
            questManager.RegisterQuest(equipQuest1);

            // Tier 2: Level 5 (±2 = 3-7 range)
            int tier2EquipLevel = ApplyRequirementVariation(rngManager, 5, 2);
            int tier2EquipGold = ApplyRewardVariation(rngManager, 200, 0.3);
            int tier2EquipXP = ApplyRewardVariation(rngManager, 100, 0.3);
            var equipQuest2 = new EquipmentQuest(tier2EquipLevel, tier2EquipGold, tier2EquipXP);
            equipQuest2.Discover();
            questManager.RegisterQuest(equipQuest2);

            // Create challenge quests with RNG on rewards
            int challenge1Gold = ApplyRewardVariation(rngManager, 200, 0.3);
            int challenge1XP = ApplyRewardVariation(rngManager, 100, 0.3);
            var challenge1 = new ChallengeQuest("flawless_victory", "Flawless Victory", "Win a battle without taking any damage. Perfect defense and timing are key!", ChallengeType.FlawlessVictory, challenge1Gold, challenge1XP);
            challenge1.Discover();
            questManager.RegisterQuest(challenge1);

            int challenge2Gold = ApplyRewardVariation(rngManager, 150, 0.3);
            int challenge2XP = ApplyRewardVariation(rngManager, 75, 0.3);
            var challenge2 = new ChallengeQuest("crit_master", "Critical Master", "Land 5 critical hits in a single battle. Show your mastery of precision strikes!", ChallengeType.CriticalMaster, challenge2Gold, challenge2XP);
            challenge2.Discover();
            questManager.RegisterQuest(challenge2);

            int challenge3Gold = ApplyRewardVariation(rngManager, 175, 0.3);
            int challenge3XP = ApplyRewardVariation(rngManager, 90, 0.3);
            var challenge3 = new ChallengeQuest("survivor", "Survivor", "Win a battle with less than 10% health remaining. Live on the edge!", ChallengeType.Survivor, challenge3Gold, challenge3XP);
            challenge3.Discover();
            questManager.RegisterQuest(challenge3);

            // Win streak: 5 battles (±2 = 3-7 range)
            int winStreakReq = ApplyRequirementVariation(rngManager, 5, 2);
            int challenge4Gold = ApplyRewardVariation(rngManager, 250, 0.3);
            int challenge4XP = ApplyRewardVariation(rngManager, 125, 0.3);
            // Update description dynamically based on RNG requirement
            string winStreakDesc = $"Win {winStreakReq} battles in a row without fleeing. Prove your consistency!";
            var challenge4 = new ChallengeQuest("win_streak", "Undefeated", winStreakDesc, ChallengeType.WinStreak, challenge4Gold, challenge4XP, winStreakReq);
            challenge4.Discover();
            questManager.RegisterQuest(challenge4);
        }

        /// <summary>
        /// Apply RNG variation to quest requirements (±variance)
        /// </summary>
        private int ApplyRequirementVariation(RNGManager rngManager, int baseValue, int maxVariance)
        {
            int variation = rngManager.Roll(-maxVariance, maxVariance);
            return Math.Max(1, baseValue + variation); // Ensure minimum of 1
        }

        /// <summary>
        /// Apply percentage-based RNG variation to rewards
        /// </summary>
        private int ApplyRewardVariation(RNGManager rngManager, int baseValue, double variancePercent)
        {
            int maxVariance = (int)(baseValue * variancePercent);
            int variation = rngManager.Roll(-maxVariance, maxVariance);
            return Math.Max(1, baseValue + variation); // Ensure minimum of 1
        }
    }
}
