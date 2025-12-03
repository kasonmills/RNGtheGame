# RNGtheGame - TODO List

## Legend
- ✅ = Completed
- 🚧 = In Progress / Partially Complete
- ❌ = Not Started
- 🔴 = High Priority
- 🟡 = Medium Priority
- 🟢 = Low Priority
- 🔵 = Future/GUI Phase
- 💼 = Business/Legal

---

## HIGH PRIORITY - Core Gameplay

### 🔴 1. Equipment Leveling System
- ✅ Weapon/Armor XP tracking
- ✅ Two-stage level up (XP + Blacksmith payment)
- ✅ Blacksmith NPC implementation
- ✅ Player-choice upgrade paths (3 options per level)
- ✅ Weapon-specific upgrade ranges (data-driven)
- ✅ All 9 weapon types defined with unique scaling
- ✅ Safety checks (min damage can't exceed max)
- ✅ Save system preserves custom-upgraded weapons
- **Status**: FULLY COMPLETED!

### 🔴 2. RevivePotion System
- **File**: `game_logic/items/item_database.cs`, `consumable.cs`, `combat_manager.cs`
- **Status**: ✅ FULLY COMPLETED
- **Description**: Complete revival potion system with logic and items
- **Features**:
  - ✅ UseRevivePotion() method with level-scaled revival (1 HP at Lv1, full HP at Lv10)
  - ✅ Combat manager integration with target selection for dead allies
  - ✅ Revival speed penalty (-2 to -4 speed on revival)
  - ✅ Clears negative effects on revival
  - ✅ Three revival potion tiers:
    - Minor Revival Potion (Lv1-4): 10%-40% HP restoration (Uncommon)
    - Revival Potion (Lv5-8): 50%-80% HP restoration (Rare)
    - Greater Revival Potion (Lv9-10): 90%-100% HP restoration (Epic)

### 🔴 3. Shop Keeper Functionality
- **File**: `game_logic/entities/NPCs/shop_keeper.cs:15`
- **Status**: ❌ Not Implemented
- **Description**: ShopKeeper class exists but DisplayGoods() method is empty
- **Impact**: Players cannot buy or sell items from shop keepers
- **Needed**:
  - Display shop inventory
  - Buy/Sell transaction system
  - Price calculations with buy/sell multipliers

### 🔴 4. Enemy Defense Application
- **File**: `game_logic/combat/damage_calculator.cs:86`
- **Status**: ❌ Not Implemented
- **Description**: Enemy defense stat exists but isn't applied in damage calculations
- **Impact**: Combat balance issue - enemies may be too easy to damage

---

## MEDIUM PRIORITY - Systems & Features

### 🟡 5. Quest Event System
- **File**: `game_logic/items/quest_item.cs:68`
- **Status**: ❌ Not Implemented
- **Description**: Quest items can be used but don't trigger any quest events or progression
- **Impact**: Quest system is incomplete, quest items are just collectibles
- **Needed**:
  - Quest tracking system
  - Quest event triggers
  - Quest completion rewards
  - Quest dialogue integration

### 🟡 6. Turn Order with Speed Stats
- **File**: `game_logic/combat/combat_manager.cs:168`
- **Status**: ✅ COMPLETED
- **Description**: Dynamic turn order based on speed stats with action-based modifiers
- **Current**:
  - ✅ Speed-based turn order (highest speed acts first)
  - ✅ Action modifiers (Attack: -1 to -3, Defend: +1 to +3, Ability/Item: -1 to +1)
  - ✅ Revival speed penalty (revived entities get -2 to -4 speed)
  - ✅ Modifiers reset each round
  - ✅ Turn order displayed at start of each round

### 🟡 7. Settings Menu
- **File**: `game_logic/core/game_manager.cs:563`
- **Status**: ❌ Not Implemented
- **Description**: Settings option exists in main menu but displays "not implemented" message
- **Impact**: No way to configure game settings
- **Needed**:
  - Difficulty settings
  - RNG algorithm selection
  - Save file management
  - Game preferences

### 🟡 8. Companion/Party System Integration
- **Status**: 🚧 Partially Complete
- **Description**: Companions exist but aren't fully integrated into Player class or combat
- **Current**:
  - ✅ Companion base classes exist
  - ✅ Companion abilities implemented
  - ❌ Party management in Player class
  - ❌ Companions in combat alongside player
  - ❌ Companion XP/level up from combat
- **Impact**: Companions can't be used in actual gameplay

---

## LOW PRIORITY - Polish & Optional Features

### 🟢 9. Alternative RNG Algorithms
- **File**: `game_logic/systems/RNG_manager.cs:75-92`
- **Status**: ❌ Placeholder Only
- **Description**: Mersenne Twister and Xorshift RNG algorithms have interfaces but throw NotImplementedException
- **Impact**: Minimal - System.Random works fine for current needs
- **Note**: Optional enhancement for players who want specific RNG behaviors

### 🟢 10. Custom Map Loading
- **File**: `game_logic/world/map_manager.cs:269`
- **Status**: ❌ Not Implemented
- **Description**: LoadMap() method doesn't actually load handcrafted maps from files
- **Impact**: Minimal - procedural generation works well
- **Current**: Procedural map generation functional
- **Future**: Load predefined map layouts from JSON/XML files

### 🟢 11. Player Execute Method
- **File**: `game_logic/entities/player/player.cs:186`
- **Status**: 🚧 Placeholder
- **Description**: Abstract Execute() method from Entity has no real logic for player
- **Impact**: Minimal - method isn't actively used in current gameplay loop

---

## CONTENT CREATION

### 12. Create More Abilities
- **Status**: 🚧 Ongoing
- **Current**:
  - ✅ 4 Player abilities (Attack Boost, Critical Strike, Defense Boost, Healing)
  - ✅ 2 Enemy abilities (Poison Attack, Rage)
  - ✅ 5 Companion abilities (one per companion type)
- **Needed**: More variety for players and enemies
- **System**: ✅ Ability framework is complete and scalable

### 13. Update Logic for New Abilities
- **Status**: 🚧 Ongoing
- **Description**: As new abilities are created, ensure they integrate properly with:
  - Combat system
  - Effect system
  - Status effect display
  - AI decision-making for enemies

### 14. Expand Loot Tables
- **Status**: 🚧 Good Progress
- **Current**:
  - ✅ Item database exists with weapons, armor, consumables
  - ✅ Basic loot drop system in enemies
  - ✅ 19 weapons across 9 weapon types:
    - Swords (3): Rusty, Iron, Steel
    - Axes (2): Rusty Axe, Battle Axe
    - Maces (2): Blunt Mace, Spiked Mace
    - Spears (2): Hunting Spear, War Spear
    - Staves (2): Wooden Staff, Arcane Staff
    - Bows (2): Hunting Bow, Longbow
    - Crossbows (2): Light Crossbow, Heavy Crossbow
    - Wands (2): Apprentice Wand, Arcane Wand
    - Daggers (2): Rusty Dagger, Assassin's Blade
  - ✅ 9 armor pieces across slots/types
  - ✅ 12 consumables (health potions, food, elixirs, bombs, antidotes, revival potions)
- **Still Needed**:
  - More high-tier weapons (Epic/Legendary)
  - More armor variety
  - Set items (future)

---

## SAVE SYSTEM ENHANCEMENTS

### 15. Save Data for Selections
- **Status**: 🚧 Mostly Complete
- **Current**:
  - ✅ Player stats, inventory, equipped items saved
  - ✅ Ability level/XP saved
  - ✅ Map position saved
  - ✅ Weapon/Armor XP progress and custom stats
  - ✅ Weapon upgrade choices preserved across saves
  - ✅ ReadyForUpgrade state restoration
  - ❌ Quest progress (when quest system implemented)
  - ❌ Companion data (when companions integrated)
  - ❌ World state (defeated enemies, looted chests, etc.)

---

## FUTURE PHASE - GUI & Game Engine

### 🔵 3. Get Godot
- **Status**: ❌ Not Started
- **Phase**: GUI Development

### 🔵 4. Figure Out Sprites
- **Status**: ❌ Not Started
- **Phase**: GUI Development
- **Needed**:
  - Character sprites
  - Enemy sprites
  - Item icons
  - Environment tiles
  - UI elements

### 🔵 5. Learn Godot
- **Status**: ❌ Not Started
- **Phase**: GUI Development

### 🔵 6. Learn Game Engines
- **Status**: ❌ Not Started
- **Phase**: GUI Development
- **Goal**: Understand engine architecture for potential custom engine

### 🔵 9. Make Game Maps
- **Status**: 🚧 Partially Complete
- **Current**: ✅ Procedural map generation functional
- **Future**: Visual map design in Godot

---

## BUSINESS & LEGAL

### 💼 2. Write Game Story
- **Status**: ❌ Not Started
- **Description**: Create the narrative/lore for the game world
- **Needed**:
  - Main storyline
  - Character backstories
  - World lore
  - Quest narratives

### 💼 7. Marketing Strategy
- **Status**: ❌ Not Started
- **Phase**: Pre-Launch
- **Needed**:
  - Target audience research
  - Social media presence
  - Trailer/screenshots
  - Press kit
  - Community building

### 💼 8. Steam Publishing
- **Status**: ❌ Not Started
- **Phase**: Launch
- **Needed**:
  - Steam Direct account
  - Store page setup
  - Build preparation
  - Achievement integration

### 💼 10. Copyright Registration
- **Status**: ❌ Not Started
- **Phase**: Pre-Launch
- **Note**: Research if needed for indie game

### 💼 11. Create LLC
- **Status**: ❌ Not Started
- **Phase**: Pre-Launch
- **Note**: Consult lawyer/accountant about business structure

---

## SUMMARY BY STATUS

**Completed**: 4
- Equipment leveling system with player-choice upgrade paths
- Turn order with speed-based initiative
- Weapon/Armor save system with custom stats
- RevivePotion system with three tiers and combat integration

**High Priority** (Next to tackle): 3
1. Shop Keeper functionality
2. Enemy defense application
3. Quest event system

**Medium Priority**: 3
- Settings menu
- Companion integration
- Save system enhancements (mostly done)

**Low Priority**: 3
- Alternative RNG algorithms
- Custom map loading
- Player Execute method

**Ongoing**: 2
- Create more abilities
- Expand loot tables

**Future/GUI Phase**: 5
**Business/Legal**: 5

---

## NOTES
- Core game logic is solid and functional
- Combat system works excellently with speed-based turn order
- Equipment and ability systems are complete with deep customization
- Weapon upgrade system provides strategic player choice (9 weapon types with unique scaling)
- Save system robustly preserves all custom-upgraded equipment
- Main gaps are in NPC interactions (shops, quests) and system polish
- GUI development is separate future phase after console version is complete

## RECENT COMPLETIONS (Latest Session)
- ✅ Weapon upgrade system redesigned with player choice at each level
- ✅ All 9 weapon types defined with unique upgrade ranges:
  - **Swords**: Balanced (1-2 min, 1/1 shift, 1-3 max)
  - **Axes**: High ceiling, widens range (0-2 min, 1/2 shift, 1-4 max)
  - **Maces**: High base, low growth (0-1 min, 0-1 shift, 0-2 max)
  - **Daggers**: Small consistent growth (0-1 min, 1/1 shift, 0-2 max)
  - **Spears**: High accuracy/crit, smaller growth (0-1 min, 1/1 shift, 0-2 max)
  - **Staves**: Can miss max upgrades (1-3 min, 1/1 shift, 0-3 max)
  - **Bows**: Extreme damage range (0-1 min, 0-2 random shift, 1-7 max)
  - **Crossbows**: High accuracy, extreme range (0-1 min, 0-2 random shift, 1-7 max)
  - **Wands**: Consistency weapon, narrows range (1-3 min, 2/1 shift, 0-2 max)
- ✅ Added 8 new weapons (maces, spears, crossbows, wands)
- ✅ Safety checks prevent min damage exceeding max damage
- ✅ Data-driven design: each weapon stores its own upgrade ranges
- ✅ Save system updated to preserve custom-upgraded weapon stats
- ✅ Removed Fist from WeaponType (it's just unarmed fallback, not a weapon)
- ✅ RevivePotion system completed with three tiers based on level ranges:
  - Minor Revival Potion (Lv1-4): 10%-40% HP restoration
  - Revival Potion (Lv5-8): 50%-80% HP restoration
  - Greater Revival Potion (Lv9-10): 90%-100% HP restoration