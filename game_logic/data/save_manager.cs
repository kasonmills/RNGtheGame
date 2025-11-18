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
        /// <param name="mapManager">Optional map manager to save map state</param>
        /// <returns>True if save was successful</returns>
        public static bool SaveGame(Player player, string saveSlotName, World.MapManager mapManager = null)
        {
            try
            {
                Initialize();

                // Create SaveData from player
                SaveData saveData = CreateSaveDataFromPlayer(player, mapManager);

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
            }
            else if (item is Armor armor)
            {
                serialized.ItemType = "Armor";
                serialized.Defense = armor.Defense;
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
                    // Override with saved stats if they differ
                    if (item is Weapon weapon && serialized.MinDamage.HasValue && serialized.MaxDamage.HasValue)
                    {
                        weapon.MinDamage = serialized.MinDamage.Value;
                        weapon.MaxDamage = serialized.MaxDamage.Value;
                    }
                    break;

                case "Armor":
                    // Get base armor from database
                    item = ItemDatabase.GetArmor(serialized.Name, serialized.Level);
                    // Override with saved stats if they differ
                    if (item is Armor armor && serialized.Defense.HasValue)
                    {
                        armor.Defense = serialized.Defense.Value;
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