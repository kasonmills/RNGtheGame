# Settings System Implementation

## Overview
Implemented a comprehensive settings menu system that allows players to customize their gameplay experience. Settings are saved with the game file and persist across save/load cycles.

## Files Created

### 1. game_logic/systems/game_settings.cs
**Purpose**: Core settings data structure

**Features**:
- Display Settings (turn order, combat log detail, damage calculations, enemy stats)
- Gameplay Settings (auto-save, confirmations)
- RNG Settings (algorithm selection, statistics tracking)
- Accessibility Settings (colored text, emojis, text speed)
- Audio Settings (placeholder for future GUI implementation)
- Difficulty Settings (immutable after save creation)

**Key Methods**:
- `ResetToDefaults()` - Resets adjustable settings (not difficulty)
- `GetDifficultyMultiplier()` - Returns enemy stat multiplier based on difficulty
- `GetRewardMultiplier()` - Returns gold/XP reward multiplier based on difficulty
- `Clone()` - Creates a deep copy of settings

### 2. game_logic/menus/settings_menu.cs
**Purpose**: Interactive settings menu UI

**Submenus**:
1. **Display Settings**
   - Toggle turn order display
   - Toggle detailed combat log
   - Toggle damage calculation display
   - Toggle enemy stat block

2. **Gameplay Settings**
   - Toggle auto-save
   - Adjust auto-save interval (1-60 minutes)
   - Toggle flee confirmation
   - Toggle item consumption confirmation

3. **RNG Settings**
   - Switch RNG algorithm (currently only System.Random available)
   - Toggle statistics tracking
   - Reset statistics counter

4. **Accessibility Settings**
   - Toggle colored text
   - Toggle emojis
   - Adjust text speed (0=Instant, 1-5=Slow)

5. **Audio Settings** (Placeholder for GUI)
   - Sound effects toggle and volume
   - Music toggle and volume

6. **View Difficulty** (Read-only during save file)
   - Displays current difficulty
   - Shows stat multipliers
   - Explains difficulty levels

## Difficulty System

### Difficulty Levels
1. **Easy**
   - Enemy Stats: 75%
   - Rewards: 80%
   - For learning the game

2. **Normal** (Default)
   - Enemy Stats: 100%
   - Rewards: 100%
   - Balanced gameplay

3. **Hard**
   - Enemy Stats: 150%
   - Rewards: 130%
   - For experienced players

4. **Very Hard**
   - Enemy Stats: 200%
   - Rewards: 150%
   - Extreme challenge

### Important: Difficulty is Immutable
- Difficulty is selected ONCE at save file creation
- Cannot be changed after the save file is created
- This prevents players from:
  - Grinding on easy difficulty then switching to hard for rewards
  - Switching to easy for difficult sections
  - Gaming the system by changing difficulty mid-game

## Save/Load Integration

### SaveData.cs Additions
Added settings fields to SaveData class:
- All display settings
- All gameplay settings
- All RNG settings
- All accessibility settings
- All audio settings
- Difficulty level

### SaveManager.cs Additions
**Modified `SaveGame()` method**:
- Added `GameSettings gameSettings` parameter
- Saves all settings to SaveData before serialization

**Added `LoadSettingsFromSaveData()` method**:
- Reconstructs GameSettings from SaveData
- Returns default settings if saveData is null

## GameManager Integration

### Fields Added
- `private GameSettings _gameSettings;`

### Methods Modified

**`InitializeSystems()`**:
- Initializes `_gameSettings` with default values

**`StartNewGame()`**:
- Calls `SelectDifficulty()` to set immutable difficulty
- Difficulty selection happens before character creation

**`LoadGame()`**:
- Loads settings from SaveData
- Applies RNG settings to RNGManager
- Settings restored before game state

**`SaveGame()`**:
- Passes `_gameSettings` to SaveManager.SaveGame()

**Pause Menu (case "4")**:
- Replaced placeholder with `SettingsMenu.DisplaySettingsMenu()`
- Passes `isDuringSaveFile: true` to prevent difficulty changes

### New Methods Added

**`SelectDifficulty()`**:
- Displays difficulty selection menu
- Shows stat/reward multipliers for each level
- Warns that choice is permanent
- Sets `_gameSettings.Difficulty`
- Called during new game creation only

## Usage Flow

### New Game
1. Player starts new game
2. SelectDifficulty() is called
3. Player chooses difficulty (1-4)
4. Difficulty is stored in `_gameSettings`
5. Player creates character
6. Settings are saved with the game

### Loading Game
1. SaveData is loaded
2. Settings are extracted via `LoadSettingsFromSaveData()`
3. RNG settings are applied to RNGManager
4. Player continues with their saved settings

### Accessing Settings
1. Player opens pause menu (during gameplay)
2. Selects "Settings" option
3. Can view/modify all settings except difficulty
4. Changes are immediately applied
5. Changes saved on next save

## Settings Persistence

Settings are saved in the JSON save file alongside player data:
```json
{
  "PlayerName": "Hero",
  "Level": 10,
  ...
  "Difficulty": "Normal",
  "ShowTurnOrderAtStartOfRound": true,
  "AutoSaveEnabled": true,
  "AutoSaveIntervalMinutes": 5,
  ...
}
```

## Future Enhancements

### Audio Implementation (GUI Phase)
- Currently settings exist but have no effect (console app)
- Will be connected to audio system in GUI version

### Additional RNG Algorithms
- Framework supports multiple algorithms
- Currently only System.Random implemented
- Can add Mersenne Twister, Xorshift, etc.

### Additional Settings Ideas
- Combat animation speed
- Auto-accept quests
- Quest notification preferences
- Screen shake intensity (GUI)
- Colorblind mode options

## Technical Notes

### Why Difficulty is Immutable
1. **Game Balance**: Prevents exploitation of difficulty switching
2. **Quest Integrity**: Quest rewards are scaled once at creation based on difficulty
3. **Boss Scaling**: Boss stats are calculated with difficulty in mind
4. **Achievement Value**: Completing on Hard means something

### Settings Architecture
- **GameSettings**: Data model (pure data)
- **SettingsMenu**: UI layer (displays and modifies settings)
- **GameManager**: Integration layer (owns settings, passes to systems)
- **SaveManager**: Persistence layer (saves/loads settings)

This separation of concerns makes the system:
- Easy to test
- Easy to modify
- Easy to extend
- Easy to maintain

## Testing Checklist

- [ ] Create new game, select each difficulty level
- [ ] Verify difficulty is shown in pause menu as read-only
- [ ] Change display settings, verify they persist on save/load
- [ ] Change gameplay settings, verify they persist on save/load
- [ ] Toggle RNG statistics tracking, verify it works
- [ ] Change accessibility settings, verify they persist
- [ ] Reset to defaults, verify all settings reset except difficulty
- [ ] Old save files load with default settings
- [ ] New save files preserve selected difficulty

## Integration Points

The settings system integrates with:
- **RNGManager**: Statistics tracking and algorithm selection
- **SaveManager**: Full save/load support
- **GameManager**: Central ownership and state management
- **Combat System** (Future): Will use ShowDamageCalculations, etc.
- **Enemy Generation** (Future): Will use difficulty multipliers
- **Reward System** (Future): Will use reward multipliers

## Summary

The settings system provides:
✅ Comprehensive customization options
✅ Immutable difficulty selection per save file
✅ Full save/load persistence
✅ Clean separation of concerns
✅ Extensible architecture for future features
✅ User-friendly menu interface
✅ Integration with existing game systems