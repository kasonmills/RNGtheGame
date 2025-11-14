# RNGtheGame

A turn-based RPG where RNG (Random Number Generation) is a core mechanic, built with C# and designed for future Godot integration.

## Project Structure

```
RNGtheGame/
│
├── game_logic/                          # Pure C# game logic (no Godot dependencies)
│   ├── core/
│   │   ├── game_manager.cs              # Main game coordinator
│   │   └── game_state.cs                # Enum: MainMenu, Playing, Combat, Paused, GameOver
│   │
│   ├── entities/                        # All game entities
│   │   ├── entity.cs                    # Base class (Entity) - shared by Player/Enemy/NPC
│   │   │                                # Provides: Health, ActiveEffects, HasEffect<T>(), etc.
│   │   ├── player/
│   │   │   ├── player.cs                # Player class (inherits Entity)
│   │   │   │                            # Adds: Experience, Gold, Inventory, Equipment
│   │   │   └── player_inventory.cs      # Inventory management helper
│   │   │
│   │   ├── enemies/
│   │   │   ├── enemy_base.cs            # EnemyBase class (inherits Entity)
│   │   │   │                            # Adds: Combat stats, AI, loot tables
│   │   │   ├── enemy_ai.cs              # Enemy AI behavior logic
│   │   │   └── enemy_types/
│   │   │       ├── goblin.cs            # Goblin enemy
│   │   │       ├── dragon.cs            # Dragon enemy
│   │   │       └── bandit.cs            # Bandit enemy
│   │   │
│   │   └── NPCs/
│   │       ├── npc_base.cs              # Base NPC class
│   │       ├── shop_keeper.cs           # Shopkeeper NPC
│   │       └── companions/
│   │           ├── companion_base.cs    # Base companion class
│   │           ├── companion_warrior.cs
│   │           ├── companion_mage.cs
│   │           ├── companion_rogue.cs
│   │           ├── companion_healer.cs
│   │           └── companion_ranger.cs
│   │
│   ├── combat/
│   │   ├── combat_manager.cs            # Orchestrates combat flow
│   │   ├── turn_manager.cs              # Handles turn order
│   │   ├── damage_calculator.cs         # Calculates damage/defense/crits
│   │   └── combat_action.cs             # Combat action data structure
│   │
│   ├── abilities/
│   │   ├── ability.cs                   # Base ability class
│   │   ├── ability_effect.cs            # Base effect class (buffs/debuffs/DoT)
│   │   ├── player_abilities/
│   │   │   ├── attack_boost.cs          # Attack power buff
│   │   │   ├── healing_ability.cs       # Healing ability
│   │   │   ├── critical_strike.cs       # Crit chance buff
│   │   │   └── defence_boost.cs         # Defense buff
│   │   └── enemy_abilities/
│   │       ├── poison_attack.cs         # Poison DoT effect
│   │       └── rage.cs                  # Rage buff (damage up, defense down)
│   │
│   ├── items/
│   │   ├── item.cs                      # Base item class
│   │   ├── weapon.cs                    # Weapon with damage/accuracy/crit
│   │   ├── armor.cs                     # Armor with defense
│   │   ├── consumable.cs                # Potions, food, etc.
│   │   ├── quest_item.cs                # Special quest items
│   │   └── item_database.cs            # Item factory/database
│   │
│   ├── world/
│   │   ├── map_manager.cs               # Handles map generation/navigation
│   │   ├── map_node.cs                  # Individual map locations
│   │   ├── location_type.cs             # Enum: City, Dungeon, Wilderness, Boss
│   │   ├── city.cs                      # City data and shops
│   │   └── dungeon.cs                   # Dungeon data and encounters
│   │
│   ├── progression/
│   │   ├── leveling_system.cs           # XP curves, level requirements, stat scaling
│   │   │                                # Also: Enemy scaling, item costs, rewards
│   │   ├── loot_table.cs                # Loot drop rate logic
│   │   └── quest_system.cs              # Quest tracking and completion
│   │
│   ├── systems/
│   │   ├── RNG_manager.cs               # All randomness: rolls, dice, weighted selection
│   │   │                                # Provides reproducible RNG with seeds
│   │   ├── difficulty_scaler.cs         # Scales difficulty based on party level
│   │   │                                # Handles party composition and enemy scaling
│   │   └── event_system.cs              # Event/messaging system
│   │
│   └── data/                            # Data containers and persistence
│       ├── save_data.cs                 # Player save file structure (JSON)
│       ├── save_manager.cs              # Save/load system (static class)
│       ├── game_data.cs                 # Global game data
│       └── constants.cs                 # Magic numbers and game constants
│
├── tests/                               # Unit tests (xUnit framework)
│   ├── RNGManagerTests.cs               # 40+ tests for RNG system
│   ├── LevelingSystemTests.cs           # 78 tests for leveling/XP/scaling
│   ├── DifficultyScalerTests.cs         # 93 tests for difficulty scaling
│   ├── DamageCalculatorTests.cs         # 27 tests for damage calculations
│   ├── TurnManagerTests.cs              # 43 tests for turn management
│   ├── CombatManagerTests.cs            # 22 tests for combat orchestration
│   ├── SaveManagerTests.cs              # 38 tests for save/load system
│   └── PlayerTests.cs                   # 48 tests for player functionality
│   │
│   └── Total: 411 comprehensive unit tests
│
├── GodotIntegration/                    # Godot-specific code (future)
│   ├── Scenes/
│   │   ├── Main.tscn
│   │   ├── Combat.tscn
│   │   ├── Map.tscn
│   │   └── UI.tscn
│   │
│   ├── Scripts/                         # Godot C# scripts that use game_logic
│   │   ├── PlayerController.cs          # Links Player.cs to Godot node
│   │   ├── EnemyController.cs
│   │   ├── CombatSceneController.cs
│   │   └── MapController.cs
│   │
│   ├── UI/
│   │   ├── UIManager.cs
│   │   ├── MainMenu.cs
│   │   ├── CombatHUD.cs
│   │   ├── InventoryUI.cs
│   │   └── HealthBar.cs
│   │
│   └── Resources/
│       ├── Sprites/
│       ├── Audio/
│       ├── Fonts/
│       └── Prefabs/
│
├── Program.cs                           # Console app entry point (for testing)
├── .gitignore
├── RNGtheGame.csproj                    # C# project file
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

# Run console version
dotnet run
```

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