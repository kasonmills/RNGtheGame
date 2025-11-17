using System;
using System.IO;
using System.Text.Json;
using GameLogic.Entities.Player;
using GameLogic.Items;

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
        /// <returns>True if save was successful</returns>
        public static bool SaveGame(Player player, string saveSlotName)
        {
            try
            {
                Initialize();

                // Create SaveData from player
                SaveData saveData = CreateSaveDataFromPlayer(player);

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
        private static SaveData CreateSaveDataFromPlayer(Player player)
        {
            SaveData data = new SaveData
            {
                PlayerName = player.Name,
                Level = player.Level,
                Experience = player.Experience,
                Health = player.Health,
                MaxHealth = player.MaxHealth,
                Gold = player.Gold,
                SaveDate = DateTime.Now,
                PlayTime = player.PlayTime
            };

            // Save equipped items
            if (player.EquippedWeapon != null)
            {
                data.EquippedWeaponName = player.EquippedWeapon.Name;
            }

            if (player.EquippedArmor != null)
            {
                data.EquippedArmorName = player.EquippedArmor.Name;
            }

            // Save selected ability
            if (player.SelectedAbility != null)
            {
                data.SelectedAbilityName = player.SelectedAbility.Name;
                data.SelectedAbilityLevel = player.SelectedAbility.Level;
                data.SelectedAbilityExperience = player.SelectedAbility.Experience;
            }

            // Save inventory
            if (player.Inventory != null && player.Inventory.Items != null)
            {
                foreach (var item in player.Inventory.Items)
                {
                    data.InventoryItems.Add(item.Name);
                }
            }

            return data;
        }

        /// <summary>
        /// Apply SaveData to a player object
        /// </summary>
        public static void ApplySaveDataToPlayer(SaveData saveData, Player player)
        {
            player.Name = saveData.PlayerName;
            player.Level = saveData.Level;
            player.Experience = saveData.Experience;
            player.Health = saveData.Health;
            player.MaxHealth = saveData.MaxHealth;
            player.Gold = saveData.Gold;
            player.PlayTime = saveData.PlayTime;

            // Restore equipped weapon
            if (!string.IsNullOrEmpty(saveData.EquippedWeaponName))
            {
                player.EquippedWeapon = ItemDatabase.GetWeapon(saveData.EquippedWeaponName);
            }

            // Restore equipped armor
            if (!string.IsNullOrEmpty(saveData.EquippedArmorName))
            {
                player.EquippedArmor = ItemDatabase.GetArmor(saveData.EquippedArmorName);
            }

            // Restore inventory
            player.Inventory.Clear();
            foreach (var itemName in saveData.InventoryItems)
            {
                // Try to get item from database
                Item item = ItemDatabase.GetItemByName(itemName, 1, 1);
                if (item != null)
                {
                    player.Inventory.AddItem(item);
                }
            }

            // Note: Ability restoration is now handled in Player.LoadFromSave()
            // No need to restore ability here
        }

        /// <summary>
        /// Auto-save the game (convenience method)
        /// </summary>
        public static bool AutoSave(Player player)
        {
            return SaveGame(player, AutoSaveFileName);
        }

        /// <summary>
        /// Quick-save the game (convenience method)
        /// </summary>
        public static bool QuickSave(Player player)
        {
            return SaveGame(player, QuickSaveFileName);
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
}