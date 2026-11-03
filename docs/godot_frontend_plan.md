# Godot frontend plan - file structure (wireframe)

_Started 2026-10-28. This is the **shape** of `godot_project/`: every folder, every planned file and one line on what each is for. It is not a build guide - the screens get designed and built in their own sessions. **The folders and C# files now exist (2026-10-28)**: the old files were moved as in section 5 and every new `.cs` file is empty. `.tscn` files are created as each screen is built. Folders with nothing in them yet hold a `.gitkeep` so git keeps them._

---

## 1. Ground rules

- **`game_logic/` owns information and rules; `godot_project/` owns the screen** (decided 2026-10-12). A Godot script draws things, turns clicks and key presses into calls on `game_logic`, and keeps no copy of game data and no game rules.
- **One way in to the game**: every scene reaches the game through the `Game` autoload (section 3), never by building its own `GameManager`, `Player` or `CombatManager`. The debug fight launcher is the one exception (section 2, `debug/`).
- **Organise by feature, not by file type.** A scene and its script sit side by side in the folder for that screen (`scenes/combat/combat_scene.tscn` + `CombatScene.cs`), instead of all scenes in one folder and all scripts in another. When you work on the shop you open one folder.
- **Naming**: folders and `.tscn` files in `snake_case`; C# scripts in `PascalCase` matching the class name (Godot requires the C# file name to match the class). Namespaces follow folders: `RNGtheGame.Godot.Scenes.Combat`, `RNGtheGame.Godot.UI.Components`, ...
- **Two kinds of screen, matching the `GameState` convention**:
  - **Fixed screens** (main menu, world, combat, game over, ending) replace the whole screen - `ChangeState` on the backend, a scene change on the Godot side.
  - **Overlays** (pause, party menu, shop, settings, ...) open on top of whatever is showing and return to it - `PushState`/`PopState` on the backend, a scene added to the overlay layer on the Godot side. The world stays loaded underneath so the player comes back to exactly where they were.

---

## 2. Folder tree

```
godot_project/
├── project.godot                       # Main scene = scenes/boot/boot.tscn; autoloads registered here
├── RNGtheGameGodot.csproj              # References ../game_logic/GameLogic.csproj
│
├── autoload/                           # Always-loaded singletons (Godot "autoloads")
│   ├── Game.cs                         # Creates and holds the one GameManager; every scene reaches the game through it
│   ├── SceneRouter.cs                  # GameState -> scene table; swaps fixed screens, opens/closes overlays on StateChanged
│   └── Notifier.cs                     # Listens to non-combat game events (level up, item gained, quest progress, saved) and shows toasts/popups
│
├── scenes/
│   ├── boot/
│   │   ├── boot.tscn                   # Project's main scene: empty root holding the screen layer + overlay layer
│   │   └── Boot.cs                     # Waits for Game to be ready, then asks SceneRouter for the main menu
│   │
│   ├── main_menu/
│   │   ├── main_menu.tscn / MainMenu.cs            # New game, load, settings, quit
│   │   ├── new_game_panel.tscn / NewGamePanel.cs   # Character creation: name, difficulty, ability, starter weapon -> StartNewGame()
│   │   └── load_panel.tscn / LoadPanel.cs          # Save file list from SaveManager -> LoadGame()
│   │
│   ├── tutorial/
│   │   └── intro_story.tscn / IntroStory.cs        # Intro text, companion recruitment, hands off to the Skarn fight (which uses scenes/combat/)
│   │
│   ├── world/                                      # Everything for walking around (GameState.Playing)
│   │   ├── world.tscn / World.cs                   # Holds the current area, the player and the HUD; swaps areas when the player leaves through an exit
│   │   ├── world_hud.tscn / WorldHud.cs            # On-screen info while exploring: area name, gold, tracked quest
│   │   ├── world_map.tscn / WorldMap.cs            # Overview of routes/towns drawn from MapNode PositionX/Y (MVP need still open - see section 6)
│   │   ├── areas/
│   │   │   ├── Area.cs                             # Base script every area uses: its MapNode id, spawn points, exits; calls SpawnOverworldEncounters() on enter
│   │   │   ├── routes/route_01.tscn ...            # One scene per route (bounded walkable area, Pokemon-style)
│   │   │   ├── towns/town_xxx.tscn ...             # One scene per town (inn, shop, blacksmith, quest giver placed inside)
│   │   │   └── camps/camp_xxx.tscn ...             # Marked campsites (save points)
│   │   ├── actors/
│   │   │   ├── player_actor.tscn / PlayerActor.cs          # Movement, interact key, hitbox -> OnOverworldEnemyHitboxCollision()
│   │   │   ├── overworld_enemy.tscn / OverworldEnemy.cs    # Wraps one OverworldEnemySpawn: wander, EvaluateCharge() each tick, chase
│   │   │   └── npc_actor.tscn / NpcActor.cs                # Any NPC in the world; on interact opens its screen (shop, blacksmith, quest giver, dialogue)
│   │   └── interactables/
│   │       ├── chest.tscn / Chest.cs               # On open -> TriggerLootEvent() -> loot popup
│   │       ├── save_point.tscn / SavePoint.cs      # Inn bed / campfire; the only place saving is offered
│   │       ├── area_exit.tscn / AreaExit.cs        # Edge of an area; leads to the connected MapNode
│   │       └── boss_trigger.tscn / BossTrigger.cs  # Where a boss is met -> ChallengeBoss()
│   │
│   ├── combat/                                     # GameState.Combat (also used for the tutorial and boss fights)
│   │   ├── combat_scene.tscn / CombatScene.cs      # Builds the screen from GameManager.Combat; queues combat events and plays them in order
│   │   ├── components/
│   │   │   ├── entity_slot.tscn / EntitySlot.cs            # One combatant: sprite/placeholder, name, health bar, status icons, defend marker, down state
│   │   │   ├── turn_order_bar.tscn / TurnOrderBar.cs       # Round number + the 8 turn slots (skipped slots, shared slots)
│   │   │   ├── action_bar.tscn / ActionBar.cs              # Attack / Ability / Item / Defend / Reposition / Flee, greyed out when unusable
│   │   │   ├── ability_picker.tscn / AbilityPicker.cs      # Lists the acting member's abilities (Felicity has several; Boon's bolt type)
│   │   │   ├── item_picker.tscn / ItemPicker.cs            # Usable items from the shared party bag
│   │   │   ├── TargetPicker.cs                             # Target selection mode: enemy for attacks, ally for heals/revives
│   │   │   ├── floating_text.tscn / FloatingText.cs        # Damage / heal numbers, "Critical", "Miss", "Blocked" over a slot
│   │   │   ├── companion_toggle.tscn / CompanionToggle.cs  # Direct control / auto per companion
│   │   │   ├── boss_banner.tscn / BossBanner.cs            # Boss name + title, big-attack warnings (Skarn's stoop)
│   │   │   └── combat_log.tscn / CombatLog.cs              # Scrolling text history
│   │   └── combat_result.tscn / CombatResult.cs    # Won / lost / party fled / opponent fled, with XP, gold, items, level ups
│   │
│   ├── overlays/                                   # PushState/PopState screens, drawn over the current screen
│   │   ├── pause/pause_menu.tscn / PauseMenu.cs                # Resume, settings, quit to main menu, quit (save only at save points)
│   │   ├── party/
│   │   │   ├── party_menu.tscn / PartyMenu.cs                  # The one menu for player + inventory + companions (decided 2026-10-12); tabs below
│   │   │   ├── member_tab.tscn / MemberTab.cs                  # One party member: stats, ability, equipped weapon/armor, equip swap
│   │   │   ├── inventory_tab.tscn / InventoryTab.cs            # Shared bag: sort/filter, use, drop, equip onto a chosen member
│   │   │   └── roster_tab.tscn / RosterTab.cs                  # Recruited vs fighting companions, control toggle, formation (when positions exist)
│   │   ├── quests/
│   │   │   ├── quest_log.tscn / QuestLog.cs                    # All quests, set tracked quest
│   │   │   ├── quest_giver_screen.tscn / QuestGiverScreen.cs   # Boss quests: accept, claim
│   │   │   └── (no job board in the MVP - decided 2026-11-03; NPCs give their own unlock quests)
│   │   ├── shop/shop_screen.tscn / ShopScreen.cs               # Stock + prices, buy, sell, player gold
│   │   ├── blacksmith/blacksmith_screen.tscn / BlacksmithScreen.cs  # Upgradable gear, cost, weapon upgrade-path choice
│   │   ├── dialogue/dialogue_box.tscn / DialogueBox.cs         # NPC / story text box (GameState.Dialogue)
│   │   ├── settings/settings_menu.tscn / SettingsMenu.cs       # Settings decided 2026-10-09 (RNG choice and auto-save hidden)
│   │   ├── statistics/statistics_screen.tscn / StatisticsScreen.cs  # Read-only stats from StatisticsTracker
│   │   └── save/save_screen.tscn / SaveScreen.cs               # Opened from a save point: pick slot, confirm, result
│   │
│   ├── popups/                                     # Small, short-lived pieces shown by Notifier or by a screen
│   │   ├── loot_popup.tscn / LootPopup.cs          # What a chest or reward gave
│   │   ├── toast.tscn / Toast.cs                   # One-line message ("Level up!", "Game saved")
│   │   └── confirm_dialog.tscn / ConfirmDialog.cs  # Yes/no for drop item, quit, overwrite save
│   │
│   ├── endings/
│   │   ├── game_over.tscn / GameOver.cs            # Defeat screen -> load / main menu
│   │   ├── ending.tscn / Ending.cs                 # Ending cutscene -> FinishEnding() -> credits
│   │   └── credits.tscn / Credits.cs               # Credits roll -> FinishCredits() -> main menu
│   │
│   └── debug/
│       ├── debug_grid_overlay.tscn / DebugGridOverlay.cs   # The x/y coordinate grid for placing things
│       └── test_fight.tscn / TestFight.cs                  # Today's standalone fight (own player + goblins), kept for quick combat testing
│
├── ui/                                         # Shared pieces used by more than one screen
│   ├── theme/
│   │   └── main_theme.tres                     # Fonts, colours, button and panel styles for the whole game
│   └── components/
│       ├── health_bar.tscn / HealthBar.cs      # Bar + numbers; used by EntitySlot, party menu, HUD
│       ├── item_slot.tscn / ItemSlot.cs        # Icon + name + count; inventory, shop, loot, item picker
│       ├── item_tooltip.tscn / ItemTooltip.cs  # Hover details for an item (stats, rarity, price)
│       ├── stat_row.tscn / StatRow.cs          # "Label .... value" line for stats screens
│       └── panel_frame.tscn                    # Standard window frame + close button for overlays
│
└── assets/                                     # Art and sound only - no code, nothing from game_logic
    ├── sprites/
    │   ├── player/                             # Player character (one per starter weapon if needed)
    │   ├── companions/                         # Garrick, Felicity, Chance, Risk, Boon, ...
    │   ├── enemies/                            # Goblin, bandit, dragon, ...
    │   ├── bosses/                             # Skarn and the other champions
    │   └── npcs/                               # Shopkeeper, blacksmith, quest giver, townsfolk
    ├── tiles/                                  # Tilesets for routes, towns, camps
    ├── maps/                                   # World-map overview art
    ├── ui/
    │   ├── icons/items/                        # Weapon, armor, consumable, ammunition icons
    │   ├── icons/abilities/
    │   ├── icons/status/                       # Poisoned, grappled, boosted, defending, ...
    │   └── frames/                             # Panel and button art used by the theme
    ├── fonts/
    └── audio/
        ├── music/
        └── sfx/
```

---

## 3. How the pieces connect

```
project starts
   └─ boot.tscn  (main scene: ScreenLayer + OverlayLayer)
        └─ Game (autoload)        creates GameManager
        └─ SceneRouter (autoload) subscribes to GameManager.StateChanged
               │
               ├─ fixed state  -> replace whatever is in ScreenLayer
               └─ overlay state-> add the overlay to OverlayLayer (push) / remove it (pop)
```

- **`Game`** - the only place a `GameManager` is created. Exposes it (`Game.Manager`) plus shortcuts the screens need (player, party, quests, settings, statistics - see section 6 for what the backend must expose first).
- **`SceneRouter`** - the one table that says "this state shows this scene" (to-do 14 under `core/`). No screen changes scenes by itself; screens call the backend (`OpenX()`, `PushState`, `ChangeState`) and the router reacts to `StateChanged`. Also runs on start-up and after a load.
- **`Notifier`** - turns non-combat events (level up, item added, quest progress, game saved) into toasts and popups, so individual screens don't each have to.
- **Combat screen** listens to `CombatManager`'s events and queues them, so hits, numbers and health bars play one after another (the timing question in "Combat scene - what the screen has to show").

### GameState -> scene table (first draft)

| GameState | Kind | Scene |
|---|---|---|
| `MainMenu` | fixed | `scenes/main_menu/main_menu.tscn` |
| `Cutscene` | fixed | `scenes/tutorial/intro_story.tscn` for the opening intro (calls `FinishIntro()` when done) |
| `Ending` | fixed | `scenes/endings/ending.tscn` - shows `GetEndingText()`, calls `FinishEnding()` when done (added 2026-10-30) |
| `Credits` | fixed | `scenes/endings/credits.tscn` - shows `GetCreditsText()`, calls `FinishCredits()` when done -> main menu (added 2026-10-30) |
| `Tutorial` | overlay | Tutorial layer over the combat scene (lesson hints, `TutorialMessage`). Pushed on top of `Combat` when the Skarn fight starts, popped back to `Combat` once every action has been tried (decided 2026-10-28) |
| `Playing` | fixed | `scenes/world/world.tscn` |
| `Combat` | fixed | `scenes/combat/combat_scene.tscn` |
| `GameOver` | fixed | `scenes/endings/game_over.tscn` |
| `Paused` | overlay | `scenes/overlays/pause/pause_menu.tscn` - `OpenPauseMenu`/`ClosePauseMenu`, from Playing, Combat or Tutorial |
| `PartyMenu` | overlay | `scenes/overlays/party/party_menu.tscn` (one menu; replaced the old `Inventory` + `Stats` states) - `OpenPartyMenu`/`ClosePartyMenu`, from Playing or Paused |
| `QuestLog` | overlay | `scenes/overlays/quests/quest_log.tscn` - `OpenQuestLog`/`CloseQuestLog`, from Playing or Paused |
| `Statistics` | overlay | `scenes/overlays/statistics/statistics_screen.tscn` - `OpenStatistics`/`CloseStatistics`, from Playing or Paused |
| `Settings` | overlay | `scenes/overlays/settings/settings_menu.tscn` - `OpenSettings`/`CloseSettings`, from MainMenu or Paused |
| `SaveScreen` | overlay | `scenes/overlays/save/save_screen.tscn` - `OpenSaveScreen`/`CloseSaveScreen`, from Playing after the tutorial (save-point check to come) |
| `Dialogue` | overlay | `scenes/overlays/dialogue/dialogue_box.tscn` - `OpenDialogue(npc)`/`CloseDialogue`, from Playing; reads `CurrentDialogueNpc` |
| `Shopping` | overlay | `scenes/overlays/shop/shop_screen.tscn` - `OpenShop(shop)`/`CloseShop`, from Playing; reads `CurrentShop` |
| `Blacksmith` | overlay | `scenes/overlays/blacksmith/blacksmith_screen.tscn` - `OpenBlacksmith(smith)`/`CloseBlacksmith`, from Playing; reads `CurrentBlacksmith` |
| `QuestGiver` | overlay | `scenes/overlays/quests/quest_giver_screen.tscn` - `OpenQuestGiver`/`CloseQuestGiver`, from Playing |

`Loading` was removed (2026-11-03): if a loading screen is ever needed, Godot shows it itself; the game logic never waits in a loading state. There is no job board in the MVP (decided 2026-11-03): each shop/NPC hands out its own unlock quest, so `job_board_screen.tscn` is not needed. Every Open returns false and changes nothing when called from a state not listed; every Close only works while its screen is on top.

---

## 4. Scene list (one line each)

| Screen | Folder | Talks to in `game_logic` |
|---|---|---|
| Main menu | `main_menu/` | `GameManager.StartNewGame`, `LoadGame`, `QuitGame` |
| Character creation | `main_menu/new_game_panel` | `GetStarterWeaponChoices`, `Player.GetAvailableAbilities`, `GameStartup.GetDifficultyDescription` |
| Load game | `main_menu/load_panel` | `SaveManager` |
| Intro + recruit | `tutorial/` | `TutorialManager` texts, `RecruitStarterCompanion`, `BeginEagleBearEncounter` |
| World / areas | `world/` | `MapManager`, `MapNode`, `SpawnOverworldEncounters`, `GetActiveOverworldEnemies`, `OnOverworldEnemyHitboxCollision` |
| World map | `world/world_map` | `MapManager` nodes and connections |
| Combat | `combat/` | `CombatManager` events + `SubmitPlayerAction` (via `Tutorial.TrySubmitAction` during the tutorial) |
| Combat result | `combat/combat_result` | `CombatFinished` / `CombatResult` |
| Pause | `overlays/pause/` | `PushState`/`PopState`, `QuitToMainMenu`, `QuitGame` |
| Party menu | `overlays/party/` | `Player`, `Player.Inventory`, `PartyManager` |
| Quest log / giver | `overlays/quests/` | `QuestManager` |
| Shop | `overlays/shop/` | `ShopKeeper` buy/sell/prices |
| Blacksmith | `overlays/blacksmith/` | `Blacksmith` upgrade methods |
| Dialogue | `overlays/dialogue/` | NPC text (source to design) |
| Settings | `overlays/settings/` | `GameSettings` |
| Statistics | `overlays/statistics/` | `StatisticsTracker` |
| Save | `overlays/save/` | `SaveGame`, save-point check |
| Loot popup | `popups/loot_popup` | result of `TriggerLootEvent` |
| Game over | `endings/game_over` | `GameManager` state |
| Ending | `endings/ending` | `GetEndingText`, `FinishEnding` |
| Credits | `endings/credits` | `GetCreditsText`, `FinishCredits` |

---

## 5. Where today's `godot_project/` files go

**Done 2026-10-28.** Each file moved together with its `.uid` file, and the three `res://` paths in the combat scene were updated by hand (`combat_scene.tscn`, `entity_slot.tscn`, and the `GD.Load` in `CombatSceneController.cs`). The old `scripts/` and `UI/` folders are gone.

| Was | Now | Still to do |
|---|---|---|
| `scenes/combat_scene.tscn`, `scripts/CombatSceneController.cs` | `scenes/combat/combat_scene.tscn` + `CombatSceneController.cs` | Rename to `CombatScene.cs` and take the fight from `GameManager`; the self-built fight moves to `debug/TestFight.cs` |
| `scenes/entity_slot.tscn`, `scripts/EntitySlot.cs` | `scenes/combat/components/` | - |
| `scripts/DebugGridOverlay.cs` | `scenes/debug/DebugGridOverlay.cs` | - |
| `scripts/PlayerController.cs` (empty) | `scenes/world/actors/PlayerActor.cs` | - |
| `scripts/EnemyController.cs` (empty) | `scenes/world/actors/OverworldEnemy.cs` | - |
| `scripts/MapController.cs` (out of date) | `scenes/world/MapController.cs` | Delete once `World.cs` + `areas/Area.cs` are written (spawn/hitbox model) |
| `UI/UIManager.cs` (empty) | `autoload/SceneRouter.cs` | - |
| `UI/HUD/HealthBar.cs` (empty) | `ui/components/HealthBar.cs` | - |
| `UI/HUD/CombatHUD.cs` (empty) | `scenes/combat/components/ActionBar.cs` | - |
| `UI/HUD/InventoryHUD.cs` (empty) | `scenes/overlays/party/InventoryTab.cs` | - |
| `UI/menus/MainMenu.cs` (empty) | `scenes/main_menu/MainMenu.cs` | - |
| `UI/menus/PauseMenu.cs` (empty) | `scenes/overlays/pause/PauseMenu.cs` | - |
| `UI/menus/SettingsMenu.cs` (empty) | `scenes/overlays/settings/SettingsMenu.cs` | - |

**First time the project is opened in Godot after the move**: run the combat scene (F6) once to check the links held. If Godot reports a missing script, re-point it in the Inspector. From now on, move files **inside the Godot editor** (FileSystem dock) so it updates references itself.

---

## 6. Open questions and backend gaps this plan exposes

**Open (decide before or during the build sessions):**
1. **Folder layout** - feature folders (this plan) or keep the current `scenes/` + `scripts/` + `UI/` split by file type?
2. **World map overview** - needed for the MVP, or is walking between areas enough?
3. **One scene per area, or one scene per area _type_ filled from data?** Hand-built scenes fit the fixed map; it depends on the fixed-map design that is still pending.
4. ✅ **Tutorial intro - decided 2026-10-28**: its own scene in the `Cutscene` state; the tutorial itself is an overlay state on top of `Combat`.
5. **Pixel-art vs higher-res art** - decides the viewport/stretch settings in `project.godot` and the asset folder contents.

**`game_logic` hook-ups the frontend needs (most already logged in `to_do_list.md`):**
- 🧪 **Written 2026-10-28, needs testing**: `GameManager.CurrentState`, and read access to `Player`, `Party`, `Map`, `Quests`, `QuestGiver`, `Bosses`, `Settings`, `Statistics`. `LoadGame(slot)` returns true/false and `SaveGame(slot)` takes a slot, neither prints. `SaveManager` is static, so the load screen calls `SaveManager.SaveExists()` / `GetSaveFileInfo()` directly.
- **Missing `GameState` values**: party menu, statistics, quest log, quest giver / job board, blacksmith, save screen (to-do 17 under `core/`). `Ending` and `Credits` added 2026-10-30. 🧪 **All added 2026-11-03, needs testing** - see the state table above.
- **Current area**: which `MapNode` the player is in and where they entered from, so `World` can load the right area scene and spawn point.
- **Can-save-here check** tied to save points (to-do 10 under `core/`).
- **Loot returned, not printed** (to-do 15 under `core/`).
- **Non-combat events** for `Notifier` (level up, item gained, quest progress) - "Replace all console output" under BACKEND WORK THE FRONTEND IS WAITING ON.
