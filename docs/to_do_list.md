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
- **Status**: COMPLETED!

### 🔴 2. RevivePotion Logic
- **File**: `game_logic/items/consumable.cs:209`
- **Status**: ❌ Not Implemented
- **Description**: RevivePotion exists but has no logic - needs to handle player death/revival mechanics
- **Impact**: RevivePotion consumable type is currently useless

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
- **File**: `game_logic/combat/turn_manager.cs:44`
- **Status**: 🚧 Basic Implementation
- **Description**: Combat uses simple alternating turns; could use speed/agility stats for dynamic turn order
- **Impact**: Combat is less strategic without speed-based initiative
- **Current**: Simple alternating turns (player -> enemy -> player)
- **Future**: Speed-based turn queue system

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
- **Status**: 🚧 Partially Complete
- **Current**:
  - ✅ Item database exists with weapons, armor, consumables
  - ✅ Basic loot drop system in enemies
  - 🚧 Could use more item variety
- **Needed**:
  - More weapons (currently ~10)
  - More armor pieces
  - More consumables
  - Rare/legendary items
  - Set items (future)

---

## SAVE SYSTEM ENHANCEMENTS

### 15. Save Data for Selections
- **Status**: 🚧 Partially Complete
- **Current**:
  - ✅ Player stats, inventory, equipped items saved
  - ✅ Ability level/XP saved
  - ✅ Map position saved
  - ❌ Weapon/Armor XP progress (NEW - needs adding)
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

**Completed**: 1
- Equipment leveling system with blacksmith

**High Priority** (Next to tackle): 4
1. RevivePotion logic
2. Shop Keeper functionality
3. Enemy defense application
4. Quest event system

**Medium Priority**: 4
- Turn order with speed stats
- Settings menu
- Companion integration
- Save system enhancements

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
- Combat system works well
- Equipment and ability systems are complete
- Main gaps are in NPC interactions (shops, quests) and system polish
- GUI development is separate future phase after console version is complete