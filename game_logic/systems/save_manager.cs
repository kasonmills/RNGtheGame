using System;
using System.IO;
using System.Text.Json;
using GameLogic.Data;

namespace GameLogic.Systems
{
    /// <summary>
    /// Handles saving and loading game data to/from disk
    /// Uses JSON format for easy debugging and cross-platform compatibility
    /// </summary>
    public class SaveManager
    {
        private const string SAVE_FOLDER = "Saves";
        private const string SAVE_FILE_NAME = "savegame.json";
        private readonly string _saveFilePath;

        // JSON serialization options for readable output
        private readonly JsonSerializerOptions _jsonOptions;

        public SaveManager()
        {
            // Create Saves folder if it doesn't exist
            if (!Directory.Exists(SAVE_FOLDER))
            {
                Directory.CreateDirectory(SAVE_FOLDER);
            }

            _saveFilePath = Path.Combine(SAVE_FOLDER, SAVE_FILE_NAME);

            // Configure JSON to be human-readable
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };
        }

        /// <summary>
        /// Save game data to disk
        /// </summary>
        /// <param name="saveData">The game data to save</param>
        /// <returns>True if save was successful</returns>
        public bool SaveGame(SaveData saveData)
        {
            try
            {
                // Update save timestamp
                saveData.SaveDate = DateTime.Now;

                // Serialize to JSON
                string jsonString = JsonSerializer.Serialize(saveData, _jsonOptions);

                // Write to file
                File.WriteAllText(_saveFilePath, jsonString);

                Console.WriteLine($"Game saved successfully to: {_saveFilePath}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save game: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Load game data from disk
        /// </summary>
        /// <returns>SaveData if successful, null if failed</returns>
        public SaveData LoadGame()
        {
            try
            {
                // Check if save file exists
                if (!File.Exists(_saveFilePath))
                {
                    Console.WriteLine("No save file found.");
                    return null;
                }

                // Read file
                string jsonString = File.ReadAllText(_saveFilePath);

                // Deserialize from JSON
                SaveData saveData = JsonSerializer.Deserialize<SaveData>(jsonString, _jsonOptions);

                Console.WriteLine($"Game loaded successfully from: {_saveFilePath}");
                Console.WriteLine($"Last saved: {saveData.SaveDate}");

                return saveData;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load game: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Check if a save file exists
        /// </summary>
        /// <returns>True if save file exists</returns>
        public bool SaveFileExists()
        {
            return File.Exists(_saveFilePath);
        }

        /// <summary>
        /// Delete the current save file
        /// </summary>
        /// <returns>True if deletion was successful</returns>
        public bool DeleteSave()
        {
            try
            {
                if (File.Exists(_saveFilePath))
                {
                    File.Delete(_saveFilePath);
                    Console.WriteLine("Save file deleted.");
                    return true;
                }
                else
                {
                    Console.WriteLine("No save file to delete.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to delete save: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get info about the save file without loading it
        /// </summary>
        /// <returns>Save info string, or null if no save exists</returns>
        public string GetSaveInfo()
        {
            try
            {
                if (!File.Exists(_saveFilePath))
                {
                    return null;
                }

                // Quick peek at save data
                string jsonString = File.ReadAllText(_saveFilePath);
                SaveData saveData = JsonSerializer.Deserialize<SaveData>(jsonString, _jsonOptions);

                return $"Character: {saveData.PlayerName} | Level: {saveData.Level} | " +
                       $"Saved: {saveData.SaveDate:g}";
            }
            catch
            {
                return "Save file corrupted";
            }
        }

        // === FUTURE ENHANCEMENT: Multiple Save Slots ===
        // Uncomment and expand these when you want multiple save files

        /*
        /// <summary>
        /// Save to a specific slot
        /// </summary>
        public bool SaveGame(SaveData saveData, int slot)
        {
            string slotPath = Path.Combine(SAVE_FOLDER, $"save_slot_{slot}.json");
            // ... similar to SaveGame() but use slotPath
        }

        /// <summary>
        /// Load from a specific slot
        /// </summary>
        public SaveData LoadGame(int slot)
        {
            string slotPath = Path.Combine(SAVE_FOLDER, $"save_slot_{slot}.json");
            // ... similar to LoadGame() but use slotPath
        }

        /// <summary>
        /// Get list of all save slots with their info
        /// </summary>
        public List<(int slot, string info)> GetAllSaves()
        {
            // Scan SAVE_FOLDER for all save files
            // Return list of available saves
        }
        */
    }
}