using System;
using System.IO;
using System.Text.Json;
using System.Linq;
using GameLogic.Entities.Player;
using GameLogic.Items;
using GameLogic.Quests;
using GameLogic.Systems;

namespace GameLogic.Data
{
    /// <summary>
    /// Handles saving and loading game data to/from disk
    /// Uses JSON serialization for human-readable save files
    /// </summary>
    public static class SaveManager
    {
        // Save file location
        private static readonly string SaveDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "RNGtheGame",
            "Saves"
        );

        private const string SaveFileExtension = ".json";
        private const string AutoSaveFileName = "autosave";
        private const string QuickSaveFileName = "quicksave";

        /// <summary>
        /// Initialize save directory (create if doesn't exist)
        /// </summary>
        public static void Initialize()
        {
            if (!Directory.Exists(SaveDirectory))
            {
                Directory.CreateDirectory(SaveDirectory);
                Console.WriteLine($"Created save directory: {SaveDirectory}");
            }
        }

        /// <summary>
        /// Save the current game state to a file
        /// </summary>
        /// <param name="player">The player to save</param>
        /// <param name="saveSlotName">Name of the save slot (e.g., "save1", "autosave")</param>
        /// <param name="bossManager">Boss manager containing boss progression state</param>
        /// <param name="mapManager">Optional map manager to save map state</param>
        /// <param name="questManager">Quest manager containing quest progression state</param>
        /// <param name="gameSettings">Game settings to save</param>
        /// <param name="statistics">Statistics tracker to save</param>
        /// <returns>True if save was successful</returns>
        public static bool SaveGame(Player player, string saveSlotName, Progression.BossManager bossManager = null, World.MapManager mapManager = null, QuestManager questManager = null, GameSettings gameSettings = null, StatisticsTracker statistics = null)
        {
            try
            {
                Initialize();

                // Create SaveData from player
                SaveData saveData = CreateSaveDataFromPlayer(player, mapManager);

                // Save boss progression data
                if (bossManager != null)
                {
                    saveData.DefeatedBossIds = new System.Collections.Generic.List<string>(bossManager.DefeatedBossIds);
                    saveData.FinalBossId = bossManager.FinalBossId;
                    saveData.BossesDefeated = bossManager.BossesDefeated;
                    saveData.FinalGateUnlocked = bossManager.FinalGateUnlocked;

                    // Save individual boss repeat counts
                    saveData.BossTimesDefeated = new System.Collections.Generic.Dictionary<string, int>();
                    foreach (var boss in bossManager.AllBosses.Values)
                    {
                        if (boss.TimesDefeated > 0)
                        {
                            saveData.BossTimesDefeated[boss.BossId] = boss.TimesDefeated;
                        }
                    }
                }

                // Save quest progression data
                if (questManager != null)
                {
                    saveData.Quests = QuestSerializationHelper.SerializeQuests(questManager);
                    saveData.ActiveQuestId = questManager.ActiveQuestId;
                }

                // Save game settings
                if (gameSettings != null)
                {
                    saveData.Difficulty = gameSettings.Difficulty;
                    saveData.ShowTurnOrderAtStartOfRound = gameSettings.ShowTurnOrderAtStartOfRound;
                    saveData.ShowDetailedCombatLog = gameSettings.ShowDetailedCombatLog;
                    saveData.ShowDamageCalculations = gameSettings.ShowDamageCalculations;
                    saveData.ShowEnemyStatBlock = gameSettings.ShowEnemyStatBlock;
                    saveData.AutoSaveEnabled = gameSettings.AutoSaveEnabled;
                    saveData.AutoSaveIntervalMinutes = gameSettings.AutoSaveIntervalMinutes;
                    saveData.ConfirmFleeAction = gameSettings.ConfirmFleeAction;
                    saveData.ConfirmConsumeItem = gameSettings.ConfirmConsumeItem;
                    saveData.RngAlgorithm = gameSettings.RngAlgorithm;
                    saveData.RngStatisticsTracking = gameSettings.RngStatisticsTracking;
                    saveData.ColoredText = gameSettings.ColoredText;
                    saveData.UseEmojis = gameSettings.UseEmojis;
                    saveData.TextSpeed = gameSettings.TextSpeed;
                    saveData.SoundEffectsEnabled = gameSettings.SoundEffectsEnabled;
                    saveData.SoundEffectsVolume = gameSettings.SoundEffectsVolume;
                    saveData.MusicEnabled = gameSettings.MusicEnabled;
                    saveData.MusicVolume = gameSettings.MusicVolume;
                }

                // Save statistics
                if (statistics != null)
                {
                    SaveStatisticsToSaveData(saveData, statistics);
                }

                // Serialize to JSON
                string json = JsonSerializer.Serialize(saveData, new JsonSerializerOptions
                {
                    WriteIndented = true // Pretty print for readability
                });

                // Write to file
                string filePath = GetSaveFilePath(saveSlotName);
                File.WriteAllText(filePath, json);

                Console.WriteLine($"Game saved successfully to: {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving game: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Load a game from a save file
        /// </summary>
        /// <param name="saveSlotName">Name of the save slot to load</param>
        /// <returns>SaveData object, or null if load failed</returns>
        public static SaveData LoadGame(string saveSlotName)
        {
            try
            {
                string filePath = GetSaveFilePath(saveSlotName);

                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"Save file not found: {filePath}");
                    return null;
                }

                // Read from file
                string json = File.ReadAllText(filePath);

                // Deserialize from JSON
                SaveData saveData = JsonSerializer.Deserialize<SaveData>(json);

                Console.WriteLine($"Game loaded successfully from: {filePath}");
                return saveData;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading game: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Create a SaveData object from the current player state
        /// </summary>
        private static SaveData CreateSaveDataFromPlayer(Player player, World.MapManager mapManager = null)
        {
            SaveData data = new SaveData
            {
                PlayerName = player.Name,
                PlayerClass = player.Class.ToString(),  // Save class as string
                Level = player.Level,
                Experience = player.Experience,
                Health = player.Health,
                MaxHealth = player.MaxHealth,
                Gold = player.Gold,
                SaveDate = DateTime.Now,
                PlayTime = player.PlayTime
            };

            // Save map state if provided
            if (mapManager != null)
            {
                data.MapSeed = mapManager.GetMapSeed();
                data.CurrentMapNodeId = mapManager.GetCurrentNodeId();
            }

            // Save equipped items (both new format and legacy format)
            if (player.EquippedWeapon != null)
            {
                data.EquippedWeapon = SerializeItem(player.EquippedWeapon);
                data.EquippedWeaponName = player.EquippedWeapon.Name; // Legacy support
            }

            if (player.EquippedArmor != null)
            {
                data.EquippedArmor = SerializeItem(player.EquippedArmor);
                data.EquippedArmorName = player.EquippedArmor.Name; // Legacy support
            }

            // Save selected ability
            if (player.SelectedAbility != null)
            {
                data.SelectedAbilityName = player.SelectedAbility.Name;
                data.SelectedAbilityLevel = player.SelectedAbility.Level;
                data.SelectedAbilityExperience = player.SelectedAbility.Experience;
            }

            // Save inventory with full item details
            if (player.Inventory != null && player.Inventory.Items != null)
            {
                foreach (var item in player.Inventory.Items)
                {
                    data.InventoryItems.Add(SerializeItem(item));
                }
            }

            return data;
        }

        /// <summary>
        /// Convert an Item to a SerializedItem for saving
        /// </summary>
        private static SerializedItem SerializeItem(Item item)
        {
            var serialized = new SerializedItem
            {
                Name = item.Name,
                Level = item.Level,
                Quantity = item.Quantity
            };

            // Set type and type-specific properties
            if (item is Weapon weapon)
            {
                serialized.ItemType = "Weapon";
                serialized.MinDamage = weapon.MinDamage;
                serialized.MaxDamage = weapon.MaxDamage;
                serialized.CritChance = weapon.CritChance;
                serialized.Accuracy = weapon.Accuracy;
                serialized.Experience = weapon.Experience;
                serialized.ExperienceToNextLevel = weapon.ExperienceToNextLevel;
                serialized.ReadyForUpgrade = weapon.ReadyForUpgrade;
            }
            else if (item is Armor armor)
            {
                serialized.ItemType = "Armor";
                serialized.Defense = armor.Defense;
                serialized.ArmorExperience = armor.Experience;
                serialized.ArmorExperienceToNextLevel = armor.ExperienceToNextLevel;
                serialized.ArmorReadyForUpgrade = armor.ReadyForUpgrade;
            }
            else if (item is Consumable consumable)
            {
                serialized.ItemType = "Consumable";
                serialized.ConsumableType = consumable.Type.ToString();
                serialized.EffectPower = consumable.EffectPower;
            }
            else if (item is QuestItem)
            {
                serialized.ItemType = "QuestItem";
            }
            else
            {
                serialized.ItemType = "Item";
            }

            return serialized;
        }

        /// <summary>
        /// Convert a SerializedItem back to an Item
        /// </summary>
        private static Item DeserializeItem(SerializedItem serialized)
        {
            if (serialized == null)
                return null;

            Item item;

            switch (serialized.ItemType)
            {
                case "Weapon":
                    // Get base weapon from database
                    item = ItemDatabase.GetWeapon(serialized.Name, serialized.Level);
                    // Override with saved stats (preserves custom upgrade choices)
                    if (item is Weapon weapon)
                    {
                        if (serialized.MinDamage.HasValue)
                            weapon.MinDamage = serialized.MinDamage.Value;
                        if (serialized.MaxDamage.HasValue)
                            weapon.MaxDamage = serialized.MaxDamage.Value;
                        if (serialized.CritChance.HasValue)
                            weapon.CritChance = serialized.CritChance.Value;
                        if (serialized.Accuracy.HasValue)
                            weapon.Accuracy = serialized.Accuracy.Value;
                        if (serialized.Experience.HasValue)
                            weapon.Experience = serialized.Experience.Value;

                        // Restore ReadyForUpgrade state if needed
                        weapon.RestoreUpgradeReadyState();
                    }
                    break;

                case "Armor":
                    // Get base armor from database
                    item = ItemDatabase.GetArmor(serialized.Name, serialized.Level);
                    // Override with saved stats (preserves custom upgrade choices)
                    if (item is Armor armor)
                    {
                        if (serialized.Defense.HasValue)
                            armor.Defense = serialized.Defense.Value;
                        if (serialized.ArmorExperience.HasValue)
                            armor.Experience = serialized.ArmorExperience.Value;

                        // Restore ReadyForUpgrade state if needed
                        armor.RestoreUpgradeReadyState();
                    }
                    break;

                case "Consumable":
                    // Get consumable with quantity
                    item = ItemDatabase.GetConsumable(serialized.Name, serialized.Level, serialized.Quantity);
                    break;

                case "QuestItem":
                    item = ItemDatabase.GetQuestItem(serialized.Name);
                    break;

                default:
                    // Fallback to GetItemByName
                    item = ItemDatabase.GetItemByName(serialized.Name, serialized.Level, serialized.Quantity);
                    break;
            }

            // Set quantity if it wasn't set by the database call
            if (item != null && item.Quantity != serialized.Quantity)
            {
                item.Quantity = serialized.Quantity;
            }

            return item;
        }

        /// <summary>
        /// Apply SaveData to a player object
        /// </summary>
        public static void ApplySaveDataToPlayer(SaveData saveData, Player player)
        {
            player.Name = saveData.PlayerName;

            // Restore player class (parse string to enum)
            if (!string.IsNullOrEmpty(saveData.PlayerClass))
            {
                if (Enum.TryParse<PlayerClass>(saveData.PlayerClass, out PlayerClass playerClass))
                {
                    player.Class = playerClass;
                }
                else
                {
                    Console.WriteLine($"Warning: Invalid player class '{saveData.PlayerClass}'. Defaulting to Warrior.");
                    player.Class = PlayerClass.Warrior;
                }
            }

            player.Level = saveData.Level;
            player.Experience = saveData.Experience;
            player.Health = saveData.Health;
            player.MaxHealth = saveData.MaxHealth;
            player.Gold = saveData.Gold;
            player.PlayTime = saveData.PlayTime;

            // Restore equipped weapon (try new format first, fallback to legacy)
            if (saveData.EquippedWeapon != null)
            {
                player.EquippedWeapon = DeserializeItem(saveData.EquippedWeapon) as Weapon;
            }
            else if (!string.IsNullOrEmpty(saveData.EquippedWeaponName))
            {
                // Legacy support
                player.EquippedWeapon = ItemDatabase.GetWeapon(saveData.EquippedWeaponName);
            }

            // Restore equipped armor (try new format first, fallback to legacy)
            if (saveData.EquippedArmor != null)
            {
                player.EquippedArmor = DeserializeItem(saveData.EquippedArmor) as Armor;
            }
            else if (!string.IsNullOrEmpty(saveData.EquippedArmorName))
            {
                // Legacy support
                player.EquippedArmor = ItemDatabase.GetArmor(saveData.EquippedArmorName);
            }

            // Restore inventory with full item properties
            player.Inventory.Clear();
            if (saveData.InventoryItems != null)
            {
                foreach (var serializedItem in saveData.InventoryItems)
                {
                    Item item = DeserializeItem(serializedItem);
                    if (item != null)
                    {
                        player.Inventory.AddItem(item);
                    }
                }
            }

            // Note: Ability restoration is now handled in Player.LoadFromSave()
            // No need to restore ability here
        }

        /// <summary>
        /// Auto-save the game (convenience method)
        /// </summary>
        public static bool AutoSave(Player player, World.MapManager mapManager = null)
        {
            return SaveGame(player, AutoSaveFileName, mapManager);
        }

        /// <summary>
        /// Quick-save the game (convenience method)
        /// </summary>
        public static bool QuickSave(Player player, World.MapManager mapManager = null)
        {
            return SaveGame(player, QuickSaveFileName, mapManager);
        }

        /// <summary>
        /// Load the auto-save
        /// </summary>
        public static SaveData LoadAutoSave()
        {
            return LoadGame(AutoSaveFileName);
        }

        /// <summary>
        /// Load the quick-save
        /// </summary>
        public static SaveData LoadQuickSave()
        {
            return LoadGame(QuickSaveFileName);
        }

        /// <summary>
        /// Check if a save file exists
        /// </summary>
        public static bool SaveExists(string saveSlotName)
        {
            string filePath = GetSaveFilePath(saveSlotName);
            return File.Exists(filePath);
        }

        /// <summary>
        /// Delete a save file
        /// </summary>
        public static bool DeleteSave(string saveSlotName)
        {
            try
            {
                string filePath = GetSaveFilePath(saveSlotName);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    Console.WriteLine($"Save file deleted: {filePath}");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting save: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get list of all save files
        /// </summary>
        public static string[] GetAllSaveFiles()
        {
            Initialize();

            if (!Directory.Exists(SaveDirectory))
            {
                return Array.Empty<string>();
            }

            string[] files = Directory.GetFiles(SaveDirectory, $"*{SaveFileExtension}");

            // Remove path and extension, return just the names
            for (int i = 0; i < files.Length; i++)
            {
                files[i] = Path.GetFileNameWithoutExtension(files[i]);
            }

            return files;
        }

        /// <summary>
        /// Get save file metadata (date, playtime) without loading full save
        /// </summary>
        public static SaveFileInfo GetSaveFileInfo(string saveSlotName)
        {
            try
            {
                string filePath = GetSaveFilePath(saveSlotName);
                if (!File.Exists(filePath))
                {
                    return null;
                }

                // Read and deserialize just to get metadata
                string json = File.ReadAllText(filePath);
                SaveData saveData = JsonSerializer.Deserialize<SaveData>(json);

                return new SaveFileInfo
                {
                    SlotName = saveSlotName,
                    PlayerName = saveData.PlayerName,
                    Level = saveData.Level,
                    SaveDate = saveData.SaveDate,
                    PlayTime = saveData.PlayTime,
                    FilePath = filePath
                };
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Get the full path for a save file
        /// </summary>
        private static string GetSaveFilePath(string saveSlotName)
        {
            return Path.Combine(SaveDirectory, saveSlotName + SaveFileExtension);
        }

        /// <summary>
        /// Load and reconstruct BossManager from save data
        /// </summary>
        /// <param name="data">SaveData containing boss progression</param>
        /// <param name="rng">RNG manager for random final boss selection</param>
        /// <returns>Reconstructed BossManager</returns>
        public static Progression.BossManager LoadBossManager(SaveData data, Systems.RNGManager rng)
        {
            var bossManager = new Progression.BossManager();

            // Register all 15 champion bosses
            var allBosses = Progression.BossDefinitions.GetAllChampionBosses();
            bossManager.RegisterBosses(allBosses.ToArray());

            // Restore final boss selection
            if (!string.IsNullOrEmpty(data.FinalBossId))
            {
                bossManager.SetFinalBoss(data.FinalBossId);
                var finalBoss = bossManager.GetFinalBoss();
                if (finalBoss != null)
                {
                    Console.WriteLine($"Final Boss: {finalBoss.Name}");
                }
            }
            else
            {
                // Fallback: select random final boss if not set
                bossManager.SelectRandomFinalBoss(rng);
            }

            // Restore defeated boss list
            if (data.DefeatedBossIds != null)
            {
                foreach (var bossId in data.DefeatedBossIds)
                {
                    var boss = bossManager.GetBoss(bossId);
                    if (boss != null)
                    {
                        boss.MarkDefeated();
                        bossManager.AddDefeatedBoss(bossId);
                    }
                }
            }

            // Restore boss defeat counts
            bossManager.SetBossesDefeated(data.BossesDefeated);
            bossManager.SetFinalGateUnlocked(data.FinalGateUnlocked);

            // Restore individual boss repeat counts
            if (data.BossTimesDefeated != null)
            {
                foreach (var kvp in data.BossTimesDefeated)
                {
                    var boss = bossManager.GetBoss(kvp.Key);
                    if (boss != null)
                    {
                        boss.TimesDefeated = kvp.Value;
                    }
                }
            }

            Console.WriteLine($"Boss Progress: {data.BossesDefeated}/{Progression.BossManager.TOTAL_BOSSES} defeated");
            return bossManager;
        }
    }

    /// <summary>
    /// Metadata about a save file (for displaying save list)
    /// </summary>
    public class SaveFileInfo
    {
        public string SlotName { get; set; }
        public string PlayerName { get; set; }
        public int Level { get; set; }
        public DateTime SaveDate { get; set; }
        public TimeSpan PlayTime { get; set; }
        public string FilePath { get; set; }

        public override string ToString()
        {
            return $"{SlotName}: {PlayerName} (Lv.{Level}) - {SaveDate:g} - Playtime: {PlayTime:hh\\:mm\\:ss}";
        }
    }

    // === Quest Serialization Helper Methods ===
    public static class QuestSerializationHelper
    {
        /// <summary>
        /// Serialize all quests from QuestManager (saves complete quest data including requirements and rewards)
        /// </summary>
        public static System.Collections.Generic.List<SerializedQuest> SerializeQuests(QuestManager questManager)
        {
            var serializedQuests = new System.Collections.Generic.List<SerializedQuest>();

            foreach (var quest in questManager.AllQuests.Values)
            {
                var serializedQuest = new SerializedQuest
                {
                    QuestId = quest.QuestId,
                    QuestName = quest.QuestName,
                    Description = quest.Description,
                    QuestState = quest.State.ToString(),
                    QuestType = quest.GetType().Name,
                    GoldReward = quest.Reward.Gold,
                    XPReward = quest.Reward.Experience
                };

                // Serialize objectives (includes requirements)
                foreach (var objective in quest.Objectives)
                {
                    serializedQuest.Objectives.Add(new SerializedQuestObjective
                    {
                        Description = objective.Description,
                        CurrentProgress = objective.CurrentProgress,
                        RequiredProgress = objective.RequiredProgress
                    });
                }

                // Save additional data for specific quest types
                if (quest is GoldCollectionQuest goldQuest)
                {
                    serializedQuest.TotalGoldEarned = goldQuest.GetTotalGoldEarned();
                }

                serializedQuests.Add(serializedQuest);
            }

            return serializedQuests;
        }

        /// <summary>
        /// Reconstruct quests from serialized data (rebuilds quests with original RNG values)
        /// </summary>
        public static void ReconstructQuests(QuestManager questManager, System.Collections.Generic.List<SerializedQuest> serializedQuests, Progression.BossManager bossManager)
        {
            if (serializedQuests == null || questManager == null)
                return;

            foreach (var savedQuest in serializedQuests)
            {
                Quest quest = null;

                // Reconstruct quest based on type
                switch (savedQuest.QuestType)
                {
                    case "BossDefeatQuest":
                        // Extract boss ID from quest ID
                        string bossId = savedQuest.QuestId.Replace("boss_defeat_", "");
                        var boss = bossManager.GetBoss(bossId);
                        if (boss != null)
                        {
                            quest = new BossDefeatQuest(bossId, boss.Name, savedQuest.GoldReward, savedQuest.XPReward);
                        }
                        break;

                    case "FinalBossQuest":
                        var finalBoss = bossManager.GetFinalBoss();
                        if (finalBoss != null)
                        {
                            quest = new FinalBossQuest(finalBoss.BossId, finalBoss.Name);
                        }
                        break;

                    case "LevelQuest":
                        // Extract level from first objective's required progress
                        if (savedQuest.Objectives.Count > 0)
                        {
                            int targetLevel = savedQuest.Objectives[0].RequiredProgress;
                            quest = new LevelQuest(targetLevel, savedQuest.GoldReward, savedQuest.XPReward);
                        }
                        break;

                    case "EnemyKillQuest":
                        // Extract kill count from first objective
                        if (savedQuest.Objectives.Count > 0)
                        {
                            int killCount = savedQuest.Objectives[0].RequiredProgress;
                            quest = new EnemyKillQuest(savedQuest.QuestId, savedQuest.QuestName, killCount, savedQuest.GoldReward, savedQuest.XPReward);
                        }
                        break;

                    case "GoldCollectionQuest":
                        // Extract gold requirement from first objective
                        if (savedQuest.Objectives.Count > 0)
                        {
                            int goldRequired = savedQuest.Objectives[0].RequiredProgress;
                            var goldQuest = new GoldCollectionQuest(goldRequired, savedQuest.GoldReward, savedQuest.XPReward);
                            if (savedQuest.TotalGoldEarned.HasValue)
                            {
                                goldQuest.SetTotalGoldEarned(savedQuest.TotalGoldEarned.Value);
                            }
                            quest = goldQuest;
                        }
                        break;

                    case "WeaponUpgradeQuest":
                        // Extract level from first objective
                        if (savedQuest.Objectives.Count > 0)
                        {
                            int weaponLevel = savedQuest.Objectives[0].RequiredProgress;
                            quest = new WeaponUpgradeQuest(weaponLevel, savedQuest.GoldReward, savedQuest.XPReward);
                        }
                        break;

                    case "EquipmentQuest":
                        // Extract level from description (stored in quest name pattern)
                        if (savedQuest.QuestName.Contains("Level "))
                        {
                            string levelStr = savedQuest.QuestName.Replace("Equip Full Level ", "").Replace("+ Gear", "").Trim();
                            if (int.TryParse(levelStr, out int equipLevel))
                            {
                                quest = new EquipmentQuest(equipLevel, savedQuest.GoldReward, savedQuest.XPReward);
                            }
                        }
                        break;

                    case "ChallengeQuest":
                        // Determine challenge type from quest ID
                        ChallengeType challengeType = ChallengeType.FlawlessVictory;
                        int customReq = 0;

                        if (savedQuest.QuestId.Contains("flawless"))
                            challengeType = ChallengeType.FlawlessVictory;
                        else if (savedQuest.QuestId.Contains("crit"))
                            challengeType = ChallengeType.CriticalMaster;
                        else if (savedQuest.QuestId.Contains("survivor"))
                            challengeType = ChallengeType.Survivor;
                        else if (savedQuest.QuestId.Contains("win_streak"))
                        {
                            challengeType = ChallengeType.WinStreak;
                            if (savedQuest.Objectives.Count > 0)
                                customReq = savedQuest.Objectives[0].RequiredProgress;
                        }

                        quest = new ChallengeQuest(savedQuest.QuestId, savedQuest.QuestName, savedQuest.Description, challengeType, savedQuest.GoldReward, savedQuest.XPReward, customReq);
                        break;
                }

                if (quest != null)
                {
                    // Restore quest state
                    if (Enum.TryParse<QuestState>(savedQuest.QuestState, out QuestState state))
                    {
                        quest.State = state;
                    }

                    // Restore objective progress
                    for (int i = 0; i < savedQuest.Objectives.Count && i < quest.Objectives.Count; i++)
                    {
                        quest.Objectives[i].SetProgress(savedQuest.Objectives[i].CurrentProgress);
                    }

                    // Register the reconstructed quest
                    questManager.RegisterQuest(quest);
                }
            }
        }

        /// <summary>
        /// Load settings from save data into a GameSettings object
        /// </summary>
        public static GameSettings LoadSettingsFromSaveData(SaveData saveData)
        {
            if (saveData == null)
                return new GameSettings();

            var settings = new GameSettings
            {
                Difficulty = saveData.Difficulty,
                ShowTurnOrderAtStartOfRound = saveData.ShowTurnOrderAtStartOfRound,
                ShowDetailedCombatLog = saveData.ShowDetailedCombatLog,
                ShowDamageCalculations = saveData.ShowDamageCalculations,
                ShowEnemyStatBlock = saveData.ShowEnemyStatBlock,
                AutoSaveEnabled = saveData.AutoSaveEnabled,
                AutoSaveIntervalMinutes = saveData.AutoSaveIntervalMinutes,
                ConfirmFleeAction = saveData.ConfirmFleeAction,
                ConfirmConsumeItem = saveData.ConfirmConsumeItem,
                RngAlgorithm = saveData.RngAlgorithm,
                RngStatisticsTracking = saveData.RngStatisticsTracking,
                ColoredText = saveData.ColoredText,
                UseEmojis = saveData.UseEmojis,
                TextSpeed = saveData.TextSpeed,
                SoundEffectsEnabled = saveData.SoundEffectsEnabled,
                SoundEffectsVolume = saveData.SoundEffectsVolume,
                MusicEnabled = saveData.MusicEnabled,
                MusicVolume = saveData.MusicVolume
            };

            return settings;
        }

        /// <summary>
        /// Save statistics to save data
        /// </summary>
        public static void SaveStatisticsToSaveData(SaveData saveData, StatisticsTracker stats)
        {
            if (saveData == null || stats == null)
                return;

            // Combat statistics
            saveData.TotalBattlesFought = stats.TotalBattlesFought;
            saveData.BattlesWon = stats.BattlesWon;
            saveData.BattlesLost = stats.BattlesLost;
            saveData.BattlesFled = stats.BattlesFled;
            saveData.TotalDamageDealt = stats.TotalDamageDealt;
            saveData.TotalDamageTaken = stats.TotalDamageTaken;
            saveData.HighestDamageDealt = stats.HighestDamageDealt;
            saveData.CriticalHitsLanded = stats.CriticalHitsLanded;
            saveData.TotalPlayerDeaths = stats.TotalPlayerDeaths;
            saveData.EnemiesKilled = stats.EnemiesKilled;
            saveData.KillsByEnemyType = new Dictionary<string, int>(stats.KillsByEnemyType);
            saveData.BossesDefeatedCount = stats.BossesDefeated;
            saveData.BossAttemptsCount = stats.BossAttempts;
            saveData.BossDefeatsDict = new Dictionary<string, int>(stats.BossDefeats);

            // Economic statistics
            saveData.TotalGoldEarned = stats.TotalGoldEarned;
            saveData.TotalGoldSpent = stats.TotalGoldSpent;
            saveData.ItemsBought = stats.ItemsBought;
            saveData.ItemsSold = stats.ItemsSold;
            saveData.TotalValueBought = stats.TotalValueBought;
            saveData.TotalValueSold = stats.TotalValueSold;
            saveData.MostExpensivePurchase = stats.MostExpensivePurchase;
            saveData.MostExpensivePurchaseName = stats.MostExpensivePurchaseName;

            // Equipment statistics
            saveData.WeaponUpgradesPerformed = stats.WeaponUpgradesPerformed;
            saveData.ArmorUpgradesPerformed = stats.ArmorUpgradesPerformed;
            saveData.HighestWeaponLevel = stats.HighestWeaponLevel;
            saveData.HighestArmorLevel = stats.HighestArmorLevel;
            saveData.WeaponsUsedDict = new Dictionary<string, int>(stats.WeaponsUsed);
            saveData.TotalEquipmentChanges = stats.TotalEquipmentChanges;

            // Item usage statistics
            saveData.ConsumablesUsed = stats.ConsumablesUsed;
            saveData.ConsumablesByTypeDict = new Dictionary<string, int>(stats.ConsumablesByType);
            saveData.HealingPotionsUsed = stats.HealingPotionsUsed;
            saveData.RevivalPotionsUsed = stats.RevivalPotionsUsed;
            saveData.AbilitiesActivated = stats.AbilitiesActivated;

            // Exploration statistics
            saveData.ShopsVisited = stats.ShopsVisited;
            saveData.NPCsInteracted = stats.NPCsInteracted;
            saveData.QuestsCompletedCount = stats.QuestsCompleted;
            saveData.QuestsAcceptedCount = stats.QuestsAccepted;
            saveData.QuestsClaimedCount = stats.QuestsClaimed;

            // Progression statistics
            saveData.HighestLevelReached = stats.HighestLevelReached;
            saveData.TotalExperienceEarned = stats.TotalExperienceEarned;
            saveData.LevelsGained = stats.LevelsGained;

            // Miscellaneous
            saveData.GameStartDate = stats.GameStartDate;
            saveData.GameSessions = stats.GameSessions;
            saveData.GamesSaved = stats.GamesSaved;
            saveData.GamesLoaded = stats.GamesLoaded;
            saveData.GameVersion = stats.GameVersion;

            // Achievements
            saveData.CurrentWinStreak = stats.CurrentWinStreak;
            saveData.LongestWinStreak = stats.LongestWinStreak;
            saveData.FlawlessVictories = stats.FlawlessVictories;
            saveData.CloseCallVictories = stats.CloseCallVictories;
            saveData.PerfectCritRuns = stats.PerfectCritRuns;
        }

        /// <summary>
        /// Load statistics from save data
        /// </summary>
        public static StatisticsTracker LoadStatisticsFromSaveData(SaveData saveData)
        {
            if (saveData == null)
                return new StatisticsTracker();

            var stats = new StatisticsTracker();

            // Combat statistics
            stats.TotalBattlesFought = saveData.TotalBattlesFought;
            stats.BattlesWon = saveData.BattlesWon;
            stats.BattlesLost = saveData.BattlesLost;
            stats.BattlesFled = saveData.BattlesFled;
            stats.TotalDamageDealt = saveData.TotalDamageDealt;
            stats.TotalDamageTaken = saveData.TotalDamageTaken;
            stats.HighestDamageDealt = saveData.HighestDamageDealt;
            stats.CriticalHitsLanded = saveData.CriticalHitsLanded;
            stats.TotalPlayerDeaths = saveData.TotalPlayerDeaths;
            stats.EnemiesKilled = saveData.EnemiesKilled;
            if (saveData.KillsByEnemyType != null)
                stats.KillsByEnemyType = new Dictionary<string, int>(saveData.KillsByEnemyType);
            stats.BossesDefeated = saveData.BossesDefeatedCount;
            stats.BossAttempts = saveData.BossAttemptsCount;
            if (saveData.BossDefeatsDict != null)
                stats.BossDefeats = new Dictionary<string, int>(saveData.BossDefeatsDict);

            // Economic statistics
            stats.TotalGoldEarned = saveData.TotalGoldEarned;
            stats.TotalGoldSpent = saveData.TotalGoldSpent;
            stats.ItemsBought = saveData.ItemsBought;
            stats.ItemsSold = saveData.ItemsSold;
            stats.TotalValueBought = saveData.TotalValueBought;
            stats.TotalValueSold = saveData.TotalValueSold;
            stats.MostExpensivePurchase = saveData.MostExpensivePurchase;
            stats.MostExpensivePurchaseName = saveData.MostExpensivePurchaseName ?? "None";

            // Equipment statistics
            stats.WeaponUpgradesPerformed = saveData.WeaponUpgradesPerformed;
            stats.ArmorUpgradesPerformed = saveData.ArmorUpgradesPerformed;
            stats.HighestWeaponLevel = saveData.HighestWeaponLevel;
            stats.HighestArmorLevel = saveData.HighestArmorLevel;
            if (saveData.WeaponsUsedDict != null)
                stats.WeaponsUsed = new Dictionary<string, int>(saveData.WeaponsUsedDict);
            stats.TotalEquipmentChanges = saveData.TotalEquipmentChanges;

            // Item usage statistics
            stats.ConsumablesUsed = saveData.ConsumablesUsed;
            if (saveData.ConsumablesByTypeDict != null)
                stats.ConsumablesByType = new Dictionary<string, int>(saveData.ConsumablesByTypeDict);
            stats.HealingPotionsUsed = saveData.HealingPotionsUsed;
            stats.RevivalPotionsUsed = saveData.RevivalPotionsUsed;
            stats.AbilitiesActivated = saveData.AbilitiesActivated;

            // Exploration statistics
            stats.ShopsVisited = saveData.ShopsVisited;
            stats.NPCsInteracted = saveData.NPCsInteracted;
            stats.QuestsCompleted = saveData.QuestsCompletedCount;
            stats.QuestsAccepted = saveData.QuestsAcceptedCount;
            stats.QuestsClaimed = saveData.QuestsClaimedCount;

            // Progression statistics
            stats.HighestLevelReached = saveData.HighestLevelReached;
            stats.TotalExperienceEarned = saveData.TotalExperienceEarned;
            stats.LevelsGained = saveData.LevelsGained;
            stats.CurrentLevel = saveData.Level; // From player data

            // Miscellaneous
            if (saveData.GameStartDate != DateTime.MinValue)
                stats.GameStartDate = saveData.GameStartDate;
            stats.TotalPlayTime = saveData.PlayTime;
            stats.GameSessions = saveData.GameSessions;
            stats.GamesSaved = saveData.GamesSaved;
            stats.GamesLoaded = saveData.GamesLoaded;
            stats.GameVersion = saveData.GameVersion ?? "1.0.0";
            stats.CurrentGold = saveData.Gold;

            // Achievements
            stats.CurrentWinStreak = saveData.CurrentWinStreak;
            stats.LongestWinStreak = saveData.LongestWinStreak;
            stats.FlawlessVictories = saveData.FlawlessVictories;
            stats.CloseCallVictories = saveData.CloseCallVictories;
            stats.PerfectCritRuns = saveData.PerfectCritRuns;

            return stats;
        }
    }
}