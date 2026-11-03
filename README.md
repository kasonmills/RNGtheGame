# RNGtheGame

A turn-based RPG where RNG (Random Number Generation) is a core mechanic, built with C# and designed for future Godot integration.

## Project Structure

```
RNGtheGame/
│
├── game_logic/                          # Pure C# game rules (no Godot dependencies)
│   ├── GameLogic.csproj                 # Library project - referenced by the console app, tests and Godot
│   │
│   ├── core/
│   │   ├── game_manager.cs              # Hub: owns every system and the current game state
│   │   ├── game_startup.cs              # Builds a new game or loads a save
│   │   ├── game_state.cs                # Enum of game states
│   │   └── tutorial_manager.cs          # Opening tutorial (story text, first boss fight rules)
│   │
│   ├── entities/
│   │   ├── entity.cs                    # Base class shared by Player/Enemy/NPC
│   │   ├── player/
│   │   │   ├── player.cs                # Player (stats, gold, equipment, ability)
│   │   │   ├── player_class.cs          # Player classes (post-MVP, dormant)
│   │   │   ├── player_inventory.cs      # Inventory (100 slots)
│   │   │   └── component_pouch.cs       # Spell component pouch (no slots, 99 per component)
│   │   │
│   │   ├── enemies/
│   │   │   ├── enemy_base.cs            # Base enemy
│   │   │   ├── enemy_ai.cs              # Enemy decision-making
│   │   │   ├── enemy_types/
│   │   │   │   ├── goblin.cs
│   │   │   │   ├── bandit.cs
│   │   │   │   └── dragon.cs
│   │   │   └── bosses/
│   │   │       ├── boss_definitions.cs  # Boss roster
│   │   │       ├── boss_enemy.cs        # One shared boss class
│   │   │       ├── boss_manager.cs      # Boss order and defeat tracking
│   │   │       └── boss_mechanic.cs     # Per-boss unique combat mechanics
│   │   │
│   │   └── NPCs/
│   │       ├── NPC_base.cs              # Base NPC
│   │       ├── shop_keeper.cs
│   │       ├── blacksmith.cs
│   │       ├── quest_giver.cs
│   │       └── companions/
│   │           ├── companion_base.cs
│   │           ├── companion_chance.cs      # Chance, the first companion (cat form)
│   │           ├── companion_risk.cs        # Risk, Captain of the Royal Guard (reworked from the Warrior)
│   │           ├── companion_mage.cs
│   │           ├── companion_rogue.cs
│   │           ├── companion_felicity.cs    # Felicity, the Healer (reworked from the Healer)
│   │           ├── companion_boon.cs        # Boon, the Sharpshooter (reworked from the Ranger)
│   │           └── party_manager.cs     # Party roster and size rules
│   │
│   ├── combat/
│   │   ├── combat_manager.cs            # Runs a fight (non-blocking, event-driven, multi-enemy)
│   │   ├── combat_action.cs             # One combat action (attack, ability, item, defend, flee)
│   │   ├── combat_event.cs              # What a combat screen reads: events, outcome, result, turn slots
│   │   ├── damage_calculator.cs         # Damage, defense, accuracy, crits
│   │   └── turn_manager.cs              # Older turn-order code (not used by combat_manager)
│   │
│   ├── abilities/
│   │   ├── ability.cs                   # Base ability
│   │   ├── ability_effect.cs            # Base effect (buffs/debuffs/damage over time)
│   │   ├── consumable_effects.cs
│   │   ├── evasion_ability.cs
│   │   ├── executioner_ability.cs
│   │   ├── iron_will_ability.cs
│   │   ├── leadership_ability.cs        # Disabled for the MVP
│   │   ├── precision_training_ability.cs
│   │   ├── rallying_cry_ability.cs
│   │   ├── swift_tactics_ability.cs
│   │   ├── player_abilities/
│   │   │   ├── attack_boost.cs
│   │   │   ├── critical_strike.cs
│   │   │   ├── defence_boost.cs
│   │   │   └── healing_ability.cs
│   │   ├── weapon_masteries/            # One file per weapon type (axe, bow, crossbow, dagger,
│   │   │                                # mace, spear, staff, sword, wand)
│   │   └── enemy_abilities/
│   │       ├── grapple.cs
│   │       ├── poison_attack.cs
│   │       └── rage.cs
│   │
│   ├── items/
│   │   ├── item.cs                      # Base item
│   │   ├── weapon.cs
│   │   ├── armor.cs
│   │   ├── consumable.cs                # Potions and other usable items
│   │   ├── ammunition.cs                # Special crossbow bolts (Boon's special shots)
│   │   ├── spell_component.cs           # Spell components (crystals, lucky/unlucky items, mushrooms, fruits, bugs, herbs)
│   │   ├── quest_item.cs
│   │   └── item_database.cs             # Every item definition
│   │
│   ├── world/
│   │   ├── map_manager.cs               # Map generation and navigation
│   │   ├── map_node.cs                  # One map location
│   │   ├── city.cs
│   │   ├── boss_encounter.cs
│   │   └── overworld_enemy_spawn.cs     # Enemies that roam an area and start a fight on contact
│   │
│   ├── progression/
│   │   ├── leveling_system.cs           # XP curves, level requirements, stat scaling
│   │   ├── loot_table.cs                # Loot tables and loot generation
│   │   └── quest_system.cs              # Second, unused quest system (kept for now)
│   │
│   ├── quests/
│   │   ├── quest.cs                     # Base quest
│   │   ├── quest_manager.cs
│   │   ├── quest_objective.cs
│   │   ├── quest_reward.cs
│   │   └── *_quest.cs                   # Quest types: boss_defeat, final_boss, enemy_kill,
│   │                                    # gold_collection, level, equipment, weapon_upgrade, challenge
│   │
│   ├── menus/                           # Old console menus, kept as reference only
│   │   ├── job_board.cs
│   │   ├── settings_menu.cs
│   │   └── statistics_menu.cs
│   │
│   ├── systems/
│   │   ├── RNG_manager.cs               # All randomness (seeded, reproducible)
│   │   ├── difficulty_scaler.cs
│   │   ├── event_system.cs
│   │   ├── game_log.cs                  # One channel for text messages (fed into the combat log during a fight)
│   │   ├── game_settings.cs
│   │   └── statistics_tracker.cs
│   │
│   └── data/
│       ├── save_data.cs                 # Save file structure (JSON)
│       └── save_manager.cs              # Save/load
│
├── godot_project/                       # The Godot frontend - structure from docs/godot_frontend_plan.md;
│   │                                    #   most .cs files are empty placeholders until each screen is built
│   ├── project.godot
│   ├── RNGtheGameGodot.csproj           # References game_logic/GameLogic.csproj
│   ├── autoload/                        # Game.cs, SceneRouter.cs, Notifier.cs
│   ├── scenes/
│   │   ├── boot/                        # Boot.cs
│   │   ├── main_menu/                   # MainMenu.cs, NewGamePanel.cs, LoadPanel.cs
│   │   ├── tutorial/                    # IntroStory.cs
│   │   ├── world/                       # World.cs, WorldHud.cs, WorldMap.cs, MapController.cs (out of date)
│   │   │   ├── areas/                   # Area.cs; routes/, towns/, camps/ (empty)
│   │   │   ├── actors/                  # PlayerActor.cs, OverworldEnemy.cs, NpcActor.cs
│   │   │   └── interactables/           # Chest.cs, SavePoint.cs, AreaExit.cs, BossTrigger.cs
│   │   ├── combat/
│   │   │   ├── combat_scene.tscn        # The combat test scene (works)
│   │   │   ├── CombatSceneController.cs
│   │   │   ├── CombatResult.cs
│   │   │   └── components/              # entity_slot.tscn + EntitySlot.cs (work); ActionBar, TurnOrderBar,
│   │   │                                #   AbilityPicker, ItemPicker, TargetPicker, FloatingText,
│   │   │                                #   CompanionToggle, BossBanner, CombatLog
│   │   ├── overlays/
│   │   │   ├── pause/                   # PauseMenu.cs
│   │   │   ├── party/                   # PartyMenu.cs, MemberTab.cs, InventoryTab.cs, RosterTab.cs
│   │   │   ├── quests/                  # QuestLog.cs, QuestGiverScreen.cs, JobBoardScreen.cs
│   │   │   ├── shop/                    # ShopScreen.cs
│   │   │   ├── blacksmith/              # BlacksmithScreen.cs
│   │   │   ├── dialogue/                # DialogueBox.cs
│   │   │   ├── settings/                # SettingsMenu.cs
│   │   │   ├── statistics/              # StatisticsScreen.cs
│   │   │   └── save/                    # SaveScreen.cs
│   │   ├── popups/                      # LootPopup.cs, Toast.cs, ConfirmDialog.cs
│   │   ├── endings/                     # GameOver.cs, Ending.cs, Credits.cs
│   │   └── debug/                       # DebugGridOverlay.cs (works), TestFight.cs
│   ├── ui/
│   │   ├── theme/                       # (empty)
│   │   └── components/                  # HealthBar.cs, ItemSlot.cs, ItemTooltip.cs, StatRow.cs
│   └── assets/                          # (empty) sprites/, tiles/, maps/, ui/, fonts/, audio/
│
├── tests/                               # Unit tests (xUnit)
│   ├── GameLogicTests.csproj            # Lists the test files currently switched off
│   ├── xunit.runner.json
│   ├── RNGManagerTests.cs
│   ├── LevelingSystemTests.cs
│   ├── DifficultyScalerTests.cs
│   ├── GameSettingsTests.cs
│   ├── StatisticsTrackerTests.cs
│   ├── CombatManagerTests.cs            # Needs rewriting for the current combat engine
│   ├── DamageCalculatorTests.cs         # Switched off - no longer compiles
│   ├── PlayerTests.cs                   # Switched off - no longer compiles
│   ├── QuestManagerTests.cs             # Switched off - no longer compiles
│   ├── SaveManagerTests.cs              # Switched off - no longer compiles
│   └── TurnManagerTests.cs              # Switched off - no longer compiles
│
├── docs/
│   ├── to_do_list.md                    # What is still to do, and the decision log
│   ├── done.md                          # What is finished (entries move here from to_do_list.md)
│   ├── godot_frontend_plan.md           # Planned folder/file structure of the Godot frontend
│   ├── abilities.md
│   ├── boss_ideas_archive.md
│   └── player_class_system.md
│
├── tools/
│   └── git_time_tracker.py              # Development hours estimate from git history
│
├── .githooks/                           # Shared git hooks (post-merge, pre-push)
├── .vscode/
├── Program.cs                           # Console entry point - only says the game is played through Godot
├── RNGTheGame.csproj                    # Console project
├── RNGtheGame.sln                       # Solution: console app, game_logic, tests, Godot project
├── SRS.md                               # Original requirements document (to be rewritten at the end)
├── .gitignore
└── README.md
```

## Key Features Implemented

### ✅ Entity System
- **Entity base class** provides shared functionality (health, effects, damage)
- **Player** inherits from Entity with experience, gold, inventory, and equipment
- **EnemyBase** inherits from Entity with combat stats and AI
- **Effect system** for buffs/debuffs (AttackBoost, DefenseBoost, Rage, Poison, etc.)

### ✅ Combat System
- **Turn-based combat** with turn manager
- **Damage calculator** with critical hits, armor reduction, accuracy checks
- **Combat manager** orchestrates the full combat flow
- **Ability effects** can be applied during combat

### ✅ RNG System
- **Seeded RNG** for reproducible randomness
- **Comprehensive methods**: Roll, RollDice, NextBool, weighted selection
- **Collection operations**: Shuffle, SelectRandom, SelectRandomUnique
- **Statistics tracking** for debugging

### ✅ Progression System
- **Leveling System** with exponential XP curves
- **Enemy scaling** based on level
- **Item upgrade costs** and material requirements
- **Ability ranking** system (Novice → Legendary)

### ✅ Difficulty Scaling
- **Party-based scaling** considers player + companions
- **Difficulty modes**: Easy, Normal, Hard, Nightmare
- **Boss/Elite modifiers** (3x health for bosses, etc.)
- **Loot quality scaling** based on difficulty

### ✅ Data Persistence
- **Save/load system** using JSON serialization
- **Auto-save and quick-save** functionality
- **Save file management** (list, delete, get info)
- **Play time tracking**

### ✅ Comprehensive Testing
- **411 unit tests** across all systems
- **Statistical validation** for randomness (1000+ iterations)
- **Edge case coverage** and error handling
- **Isolated test environments** (temporary save directories)

## Current Status

**Phase**: Core Game Logic Implementation (Console-based)
- ✅ Entity system with inheritance
- ✅ Combat system (damage, turns, actions)
- ✅ RNG and difficulty scaling
- ✅ Save/load functionality
- ✅ Comprehensive unit tests
- 🚧 Map/world navigation (stub implementation)
- 🚧 Full item/loot system
- 🚧 Quest system
- ⏳ Godot integration (planned)

## Running Tests

```bash
# Run all tests
dotnet test

# Run specific test file
dotnet test --filter "ClassName=RNGManagerTests"

# Run with verbose output
dotnet test -v detailed
```

## Building

```bash
# Build the project
dotnet build

# The console program only prints that the game is played through Godot
dotnet run
```

To play, open `godot_project/` in Godot.

## Design Principles

1. **Separation of Concerns**: Game logic is completely independent of UI/Godot
2. **Entity Inheritance**: All entities (Player, Enemy, NPC) share common base functionality
3. **Testability**: All systems are unit tested with comprehensive coverage
4. **Reproducibility**: RNG uses seeds for debugging and testing
5. **Scalability**: Difficulty scales based on party composition and level
6. **Data Integrity**: Save/load system preserves all player state

## Future Plans

- Complete map/world navigation system
- Implement full quest system
- Add more enemy types and abilities
- Integrate with Godot for visual presentation
- Add multiplayer/co-op support
- Implement achievement system
