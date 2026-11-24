# RNGtheGame - Done

What is finished. The other half of `to_do_list.md`: that file is what is still to do, this file is what is already done.

**How this file works**
- When an entry in `to_do_list.md` is completed, it is **moved** here - cut from that file, pasted into this one - with the date it was moved.
- "Done" means built **and** working in the game. Code that is written but not connected to the game, or not yet built and tested, stays in `to_do_list.md`.
- Also moved here (rule widened 2026-10-15): anything **resolved with nothing left to do** - a settled decision or question, a deleted file, a finished cleanup.
- If follow-up work on a finished item is still planned, the entry says where in `to_do_list.md` that work is logged.
- Newest entries go at the top of their section.
- Entries keep the number they had in `to_do_list.md`, so older notes that say "item 7" still make sense.

---

## Decisions resolved

### Fixed map - who draws it, shape and backtracking (section 2, Story and world)
- **Moved here**: 2026-11-24
- ✅ **Decided 2026-11-24**:
  - **The user draws the map**, and it gets built from the drawing (not a drafted text graph).
  - **Shape:** a main path with optional side areas branching off.
  - **Bosses aren't found in towns.** Each boss's location is marked on the drawing. The size comes with the drawing too.
  - **Backtracking:** every visited area stays reachable on foot, and new areas open as bosses fall. No one-way cut-offs, so `ConnectToOneWay` stays unused for now.
- **Still open**: the map itself (section 2, Story and world - 🔴, waiting on the drawing).

### Enemy roster reviewed (section 2, World and systems)
- **Moved here**: 2026-11-24
- ✅ **Decided 2026-11-24**: `enemy_roster.md` is final as drafted:
  - the standard enemy (HP 20 + 6 per level, damage (2 + level/2) to (5 + level), defense level/4, speed 10 + level/4, 75% accuracy, 5% crit)
  - the 15 enemies' multipliers and level bands (as placeholders, to retune after playtesting)
  - flee rules, AI patterns and specials, including the Big Slime's split, the Fungusoar's Spore Roar and the imps' conditions
  - the day/night lean per family
  - the name lists per family.
- **Still open**: a name for "Kingdom B" (section 2, World and systems).
- **Follow-up**: the build is "Rebalance the regular enemies" (section 1, Combat). It breaks 2 `DifficultyScalerTests` that hard-code the old level-1 Goblin. The encounter-size roll and the spawn lists wait on the map.

### Spell list reviewed - Poison Cloud cooldown, Rekindle added (section 2, Items and magic)
- **Moved here**: 2026-11-24
- ✅ **Decided 2026-11-24**:
  - **The spell list, effects and recipes in `spell_list.md` are final**, along with the 6 stat potion recipes. The numbers stay placeholders for playtesting.
  - **Poison Cloud gets a 2-turn cooldown** - its poison stacks on every enemy, so casting it every turn would snowball.
  - **Ill Omen and the Demon Prince's Hexed share one effect** in the code.
  - **New 11th spell, Rekindle:** revives a fallen ally at 25% HP, with a 5-turn cooldown. The recipe is Phoenix Ember + Melonberries + Forest Fern. It's learned from scrolls like any spell. Fiery's wild magic can pull it, but Fluke can't cast it. Until now the revival potions were the only way to revive (Felicity only heals - I wrongly said she revived during the review).
  - **New 41st component, the Phoenix Ember** - Rare, drops only, never sold.
- **Removed from `to_do_list.md` section 2**: the spell list review. Still open: whether each town's spell NPC sells or gives its scroll (with the fixed map).
- **Follow-up**: the builds are in section 1 (Items, loot and magic: "Every spell needs components", "Component data").

### Catalog keep-list reviewed, armor becomes % reduction (section 2, Items and magic)
- **Moved here**: 2026-11-24
- ✅ **Decided 2026-11-24**:
  - **The keep-list in `catalog_trim.md` is final as drafted** - 54 weapons and 24 armor sets. Swaps can still happen after playtesting.
  - **Armor defense becomes a % reduction:** defense / (defense + 100), capped at 75% together with Defend, Defense Boost and Bulwark. Found during the review: the flat subtraction would have made the catalog's armor (Common plate 25, Legendary 103) shrug off most enemies. Enemy defense stays flat.
  - **Boon's Crossbow: 10-20 damage**, 95% accuracy, 5% crit.
  - **Names stay as working names.** The Legendaries get original names in the writing pass.
- **Removed from `to_do_list.md` section 2**: "Review the catalog keep-list" (replaced by the writing-pass names item).
- **Follow-up**: "Apply the catalog trim" and "Random loot reaches the whole catalog" are unblocked (section 1, Items, loot and magic). The armor change is in section 1, Combat.

### Settings, ending unlock, world map, art style (section 2, World and systems, Story and Godot)
- **Moved here**: 2026-11-24
- ✅ **Decided 2026-11-24**:
  - **Settings split:** global (their own file, also used on the title screen) for audio, text speed, display options and confirmations. Per save for difficulty and anything gameplay-related.
  - **A completed save gets a "Completed" marker** on the load screen and a statistics summary. Nothing else unlocks in the MVP (NG+ stays parked).
  - **World map overview:** a simple view-only screen in the pause menu - visited places named, unvisited ones "???". No fast travel in the MVP.
  - **Art style: pixel art** - a low base resolution scaled up in whole steps, nearest-neighbour filtering.
  - **Godot folder layout:** feature folders, as built on 2026-10-28 (settled).
- **Removed from `to_do_list.md` section 2**: "Settings: should some be global", the unlock question in "Ending and credits", and questions 1, 2 and 5 from the Godot plan's section 6. Still open: one scene per area or per area type (with the fixed map).
- **Follow-up**: the builds are in `to_do_list.md` section 1 (Game systems: Settings; Godot frontend: world map overview, Completed marker, pixel-art settings).

### NPC quests, the apprentice and enchanting (section 2, Quests)
- **Moved here**: 2026-11-24
- ✅ **Decided 2026-11-24**:
  - **NPC unlock quests**, each given by the NPC themselves and all early in the game:
    - **Shop Keeper:** win 3 goblin fights near town (they raid his supply cart).
    - **Blacksmith:** beat a bandit group with a named leader (they stole his hammer).
    - **Magic component seller:** bring her 1 mushroom, 1 bug and 1 lucky charm. It doubles as the components tutorial.
  - **Unlock rewards:** the NPC opens, plus normal side-quest XP and gold, plus a one-off thank-you. The Shop Keeper gives 3 Honey Drops, the Blacksmith's first upgrade is free, and the seller teaches Mage Blast.
  - **The apprentice's questline has 4 steps**, one after each of bosses #2, #4, #6 and #7: find a rare ore, bring components for a test enchant, escort him to an old forge, retrieve his master's lost notes. Each step unlocks a tier of enchanting.
  - **Enchanting:**
    - **Enchants:** elemental (Flame burn, Frost slow, Venom poison, Storm hits a 2nd enemy) and stat runes (Keen +crit, True +accuracy, Vampiric heals on hit).
    - **Slots and cost:** one enchant per weapon. It costs gold plus 2-3 fitting components.
    - **Tiers:** step 1 Flame and Keen, step 2 Frost and True, step 3 Venom and Storm, step 4 Vampiric. Step 4 also makes every enchant stronger.
  - **The Veteran Ranger folds into the quest log** - boss quests appear and are claimed there. The Ranger can stay as a lore NPC.
  - **Main vs side:** one `QuestManager` with a Main/Side category. Main = boss quests and the Princess's story quests. Side = everything else. `quest_system.cs` goes dormant.
- **Removed from `to_do_list.md` section 2**: "Each NPC's unlock quest", "The blacksmith's apprentice's side quest line", "Weapon enchantment effects", "Does the boss-quest giver stay", "Main vs. side quests". Still open there: rewriting the quest text, and the names for the writing pass.
- **Follow-up**: the builds are in `to_do_list.md` section 1 (Quests).

### Day/night and the army camp (section 2, World and systems)
- **Moved here**: 2026-11-23
- ✅ **Decided 2026-11-23** (the timer is the user's own design - it replaces the open "playtime or action counter" question from 2026-11-03):
  - **A real-time countdown, 10 minutes per phase**, starting at day on a new game.
  - **Battles, dialogue and cutscenes pause the countdown.** When one ends, a roll of **-60 to +60 seconds** (every whole number possible) is added to what's left, and then it resumes. It stays mostly predictable, with some randomness.
  - **Menus, shops, inventory and the pause screen pause it too, but with no roll**, so it can't be rerolled by opening the bag.
  - **A roll that takes it to 0 or below flips the phase** as soon as play resumes.
  - **Manual switch at inns and save campsites**, in the same rest menu as `Rest()`. It flips the phase and starts a full new countdown. The phase and the time left are saved.
  - **At night:** each area uses its night spawn list; night enemies are tougher (+10% HP and damage, one step more aggression) and pay more (+20% XP and gold); town shops and the blacksmith close (inns stay open); the map is tinted darker.
  - **Army camp waves:** Army Soldiers only. Wave 1 is one soldier at the party's level, each wave adds one (max 8), and +1 level every 3 waves.
  - **Army camp rewards:** each wave beaten adds its XP and gold to a pot, with a prize item every 5th wave. Leaving between waves keeps it all. Leaving while enemies are still standing keeps only half, the same as losing (clarified by the user). **Mid-wave leaving is a separate "Surrender" action** (placeholder name, or "Retire"), which replaces the Flee button in the camp. It doesn't use the flee mechanic at all: it always works and ends the attempt with half the pot.
  - **Losing at the army camp is never a Game Over** - the party keeps half the pot. The best wave is saved.
  - **Fixed lineup, no items, no revives** (added by the user): the player picks a starting lineup of 4 and sees how far those 4 can go - no swapping with the bench. No items can be used. A member who falls stays down for the rest of the attempt.
  - **Everyone is fully healed when the challenge ends**, whether the party left or lost.
  - **Spells are free in the army camp waves** (added by the user): casting doesn't spend components, but each caster can still only cast the spells they already know. It's a place to test any mix of party members against the army.
- **Removed from `to_do_list.md` section 2**: "Day/night" and "Army camp".
- **Follow-up**: the builds are in `to_do_list.md` section 1 (Game systems). A draft of which families come out by day or night is in `enemy_roster.md`.

### Positions - formation, rows, melee and ranged (section 2, Combat)
- **Moved here**: 2026-11-23
- ✅ **Decided 2026-11-23**:
  - **Party layout: 2 front, 2 back.** The player sets the formation in the party menu, and it's saved. Every fight starts with it. A new recruit fills the first empty slot, ranged users at the back first.
  - **Front-row members are targeted twice as often** as back-row ones. If the front row is empty, the back row counts as the front.
  - **AI patterns win over rows:** when an enemy follows its pattern (about 70%), it ignores rows. The 2:1 weighting applies to its random picks and to "Random" enemies.
  - **Attacks on the back row get -10 hit chance**, on either side.
  - **Ranged:** bow, crossbow, staff, wand - no penalty in either row. **Melee:** sword, dagger, axe, mace, spear, unarmed, and cat-form Chance - -30% damage from the back row (spear only -10%, for its reach).
  - **Spells, heals and items ignore rows** (no penalty, no -10).
  - **Enemies have rows too, set by type:** archers and imps at the back, everyone else at the front. The same rules apply when the party attacks them. **Bosses ignore rows.**
  - **Reposition** swaps with any party member (a downed one too) or moves into an empty slot. It costs the chooser's turn only (decided earlier).
- **Removed from `to_do_list.md` section 2**: "Positions".
- **Follow-up**: the build is "Formation and rows" and "Reposition action" in `to_do_list.md` section 1 (Combat). The combat screen layout (section 2, Godot) needs to show the two rows.

### Enemy roster - families, groups and names (section 2, World and systems)
- **Moved here**: 2026-11-23
- ✅ **Decided 2026-11-23**:
  - **Goblin, Bandit and Dragon stay.** The Dragon becomes a rare late-game enemy found only in one or two mountain areas.
  - **Families:** goblins, bandits, wolves, slimes (with wild draglings), bugs/spiders, imps (lesser demons), **Fungusoars** (the user's own idea: mushroom dinosaurs, not humanoid mushroom folk), the player's own army, Kingdom B's army, castle guards and the dragon.
  - **One enemy per family, two for the big families** (e.g. Slime + Big Slime, Spider + Giant Beetle, Kingdom B Soldier + Archer) - about 15.
  - **The player's own army:** loyal soldiers are never random encounters. They are sparring opponents at the army camp, and **deserters** roam the routes as hostile enemies.
  - **Castle Guards are one enemy** - regular in and around the palace, and the Princess's summon.
  - **Group mix:** the touched enemy's family fills the group. About 1 in 4 fights adds one or two others from the area's spawn list.
  - **Group sizes:** packs (wolves, slimes, bugs, imps) 2-5; goblins 1-4; bandits, deserters and soldiers 1-3; Fungusoar 1-2; Dragon always 1. The maximum goes up by 1 every 10 player levels, and by +1 on Difficult / +2 on Unfair, never above 8.
  - **Random names per family**, shown as "Name the Type" ("Grik the Goblin").
- **Removed from `to_do_list.md` section 2**: "Enemy types" and "Random enemy names", replaced by "Review the enemy roster".
- **Follow-up**: the stats, specials, AI patterns and name lists are drafted in `enemy_roster.md` for review. The builds are in section 1 (Combat: rebalance and random names; Game systems: spawn lists and encounter size).

### Difficulty numbers (section 2, Difficulty and Combat)
- **Moved here**: 2026-11-23
- ✅ **Decided 2026-11-23** - the harder difficulties are much harder, and Unfair bends the dice against the player. Hard stays the 1x baseline:

  | | Normal | Hard | Difficult | Unfair |
  |---|---|---|---|---|
  | Enemy HP | 75% | 100% | 175% | 275% |
  | Enemy damage | 75% | 100% | 150% | 225% |
  | Enemy accuracy | +0 | +5 | +10 | +15 (never above 95%) |
  | Enemy crit | +0 | +0 | +5% | +10% |
  | Enemy speed | +0 | +0 | +2 | +4 |
  | Enemy defense | +0 | +0 | +20% | +40% |
  | Party hit chance | -0 | -5 | -10 | -15 (softened from -7 / -14 / -21) |
  | Party Max HP | 120% | 100% | 90% | 80% |
  | XP | 80% | 100% | 130% | 160% |
  | Gold | 100% | 100% | 125% | 150% |
  | Loot quality | 0.8x | 1x | 1.3x | 1.6x (kept) |

  - **Unfair bad luck:** about 1 in 4 of the party's hit, crit and damage rolls are rolled twice and the worse is kept - Fluke's Lucky Aura in reverse.
  - **Starting stats per difficulty = Party Max HP** (player and companions), on top of the enemy scaling. It's a %, so the gap lasts at every level.
  - **The tutorial is capped at Hard**, so a new player isn't walled before learning the controls (Normal stays Normal).
- **Removed from `to_do_list.md` section 2**: "Make the harder difficulties much harder", "Starting stats per difficulty", "Enemy accuracy per difficulty".
- **Follow-up**: the build is "Difficulty numbers" in `to_do_list.md` section 1 (Combat), which lists the tests that will need new numbers.

### Potions, sweets and components (section 2, Items and magic and Quests)
- **Moved here**: 2026-11-23
- ✅ **Decided 2026-11-23**:
  - **Permanent stat potions - amounts and caps:** per potion +10 Max HP, +1 Speed, +2% damage, +2 defense, +1% crit, +1% accuracy. Each character can drink at most **10 of each**, but only **5 Speed** potions (so at most +100 HP, +5 Speed, +20% damage, +20 defense, +10% crit, +10% accuracy). Accuracy still never goes above the 95% ceiling.
  - **Crafting fee rises per potion drunk:** 100 gold, plus 50 for each potion of that stat the character has already drunk (the 10th costs 550).
  - **Names:** Vigor, Swiftness, Might, Iron, Keen and Eagle-Eye Draught (can still be reworded in the writing pass).
  - **Healing sweets heal a % of max HP**, so they stay useful at every level: Honey Drop 20% (15 gold), Sugar Plum 40% (40 gold), Rock Candy 75% (100 gold). They are the 3 health potions renamed.
  - **Golden Toffee** - a rare 4th sweet, 100% heal, drops only (sells back for 125). It replaces the Elixir of Vitality.
  - **Bread and Cooked Meat go dormant** - the sweets replace them.
  - **The other potions keep their names** (Minor / Revival / Greater Revival Potion, Antidote) - with the healing ones now candy, "potion" means revive or cure.
  - **Who sells components:** the Shop Keeper sells mushrooms, berries and herbs (Common, 5 gold). The magic component seller sells bugs, lucky and unlucky charms (Uncommon, 20 gold). The Negativity Crystal is never sold. Everything can still drop.
  - **The Negativity Crystal:** an NPC gives the first one as a story or quest reward, then it's a Rare drop.
  - **Groups and drops:** keep the 7 groups (crystal, lucky, unlucky, mushrooms, berries, bugs, herbs). Each map area favours one or two groups (2x drop weight). A drop gives 1-3 of a Common or 1 of an Uncommon.
- **Removed from `to_do_list.md` section 2**: "Permanent stat potions", "Names for the other potions", "The Shop Keeper's candy/sweets", "The magic component seller's stock". "Spell components" is cut down to its descriptions (writing pass). The stat potion recipes go with the spell list review.
- **Follow-up**: the build items are in `to_do_list.md` section 1 (Items, loot and magic).

### Combat defaults, rounding, screen timing, tutorial, boss-fight fleeing (section 2 Combat and Tutorial, section 3)
- **Moved here**: 2026-11-23
- ✅ **Decided 2026-11-23**:
  - **The three combat defaults are confirmed:** speed ties are broken at random; if the party wins while anyone in it is down, **everyone who is down gets up at 1 HP** - companions too, not just the player (clarified 2026-11-23; they still get half XP for being down at the end); an enemy that flees drops each of its loot items with a 50% chance (gold and XP are halved exactly).
  - **Small numbers: fractions become a chance to round up.** +10% on a 5-damage hit is 5.5, so it lands as 5 (50%) or 6 (50%). Applies wherever a percentage is applied to damage, defense or healing. Bigger base numbers aren't the fix (the player's 100 HP and the party's ×2 damage stay as they are).
  - **Combat screen timing: queue and play back.** The backend keeps running every computer-controlled turn in one go. The Godot screen queues the events and animates them in order, and only shows the player's turn once the queue is empty.
  - **The tutorial can't be lost the first time.** The first time the player goes down in the Skarn fight, Chance revives them (with a short line) and the fight goes on. A second fall loses, and the player replays the tutorial.
  - **The tutorial's potion step comes after Skarn has landed a hit on the player**, so using the potion has a visible effect.
  - **Nothing flees in a boss fight**, party or enemies. Bosses never flee, and neither do the Slime Dragon's draglings, the Princess's castle guards or the Cyclops partner. This is part of the boss mechanics.
  - **Draglings are also a wild overworld enemy.** Outside the boss fight they use their normal flee chance.
  - **The engine defaults in section 3 are confirmed as written:**
    - poison on a two-turn boss ticks once per round
    - the Princess's second-turn gap depends on difficulty (Normal 6-8 … Unfair 1-2), and it's skipped when only one party member is standing
    - a downed boss doesn't react (bosses never flee)
    - poison or burn still ticks at the end of a lost turn
    - a boss's end-of-round bonus turn doesn't use up a lost turn
    - an enemy that joins mid-fight acts from the next round
    - at most 8 enemies at once ("No one answers..." when refused)
    - a bonus turn doesn't end Defend, tick effects or change speed
    - a fled enemy still takes its place in the diminishing-rewards line
    - a joining brother doesn't attack a target that's already down
    - the surviving Cyclops brother's Rage is at his own level
    - beaten bosses show their name and title in the boss list
- **Follow-up**: the build items are in `to_do_list.md` (section 1: Combat, Tutorial, Godot frontend).

### Spell rules - cooldowns, scaling, durations, Fiery's starting spells (section 2, Companions and Combat)
- **Moved here**: 2026-11-20
- ✅ **Decided 2026-11-20**:
  - **Cooldowns on the big spells only.** Normal spells can be cast every turn if the components are there. Doom and Chain Lightning get a cooldown (placeholders: Doom 4 turns, Chain Lightning 2).
  - **Spells scale with the caster's level.** Damage and heals are measured against "a basic hit for the caster's level". Spells don't level up themselves.
  - **Durations:** spell buffs and debuffs last 3 turns, and Doom lasts 2. This depends on the effect-timer fix, so a buff isn't gone before it's used.
  - **Fiery starts with the damage spells:** Fireball, Frost Lance, Chain Lightning and Poison Cloud, plus Mage Blast. Her wild magic can pull any spell on the list.
  - **On auto, a 4th-slot ("any bug") pick spends whichever component in the group the party has most of.**
  - **Spells aimed at enemies roll to hit with the caster's accuracy**, like a weapon attack. A miss still spends the components. Heals and buffs on allies always land. Spells that hit several enemies roll for each one. Mage Blast still never misses.
  - **Damage spells can crit** with the caster's crit chance. Mage Blast still can't.
  - **No components, no spell:** it's greyed out in the menu, which shows what's missing.
  - **Scroll prices rise with each spell the player already knows** (placeholder: 100 gold, plus 100 for each spell known).
- **Removed from `to_do_list.md` section 2**: "Fiery's spells" (Companions) and "How long spells and buffs last" (Combat).

### Permanent stat potions from the magic component seller (new idea)
- **Moved here**: 2026-11-20
- ✅ **Decided 2026-11-20**:
  - **The magic component seller makes potions that give a small permanent stat boost** (e.g. +1 Speed for good). She doesn't stock them. The player brings her the potion's **4 components** and pays a fee for her crafting, and she makes it. Her shop itself only sells components.
  - **Potion recipes can share components with spells.**
  - **Stats they can raise:** Max HP, Speed, damage, defense, crit and accuracy. Max HP and Speed already exist on every character. The other four need a new permanent-bonus stat on each character, added on top of gear.
  - **Cap per stat:** each character can gain at most a set amount per stat from potions, so gold can't buy unlimited power.
  - **Anyone in the party can drink them.** The boost stays with that character, so it leaves with Risk if he had any.
- **Follow-up**: the build is in `to_do_list.md` (section 1, Items, loot and magic). The amounts, caps and crafting fees are in section 2. The recipes are drafted in `spell_list.md`.

### Spell recipes - 3 unique components per spell (section 2, Items and magic)
- **Moved here**: 2026-11-20
- ✅ **Decided 2026-11-20**: every spell needs **3 unique components**, none shared with another spell. This replaces "one key component plus group filler" from 2026-11-19. With 10 spells that uses 30 of the 40 components.
  - **Some spells need a 4th component, chosen by the player** from a group the spell names (e.g. "any bug"). It's only a flexible cost and doesn't change what the spell does. Any component in the group counts, including ones other recipes use.
- **Follow-up**: the drafted matches are in `spell_list.md` for the user's review.

### How spells are learned (section 2, Items and magic)
- **Moved here**: 2026-11-20
- ✅ **Decided 2026-11-20**:
  - **Spells are learned from scrolls.** Some are bought, some are given by NPCs.
  - **One spell NPC per town.** Each offers up to 3 spells, picked from the spells **the player doesn't know yet**. The player can get only **one** of them from that NPC, and **which one is random**.
  - **There's always a new spell to get.** When fewer than 3 unknown spells are left, the NPC offers only the ones that remain.
  - **The random pick is rolled once per save**, so reloading can't change it.
  - **The player can read every offered spell's description beforehand**, but can't choose which one they get.
  - **Scrolls teach the player only.** Fiery has her own starting set of spells, and Fluke knows the luck spells from the start. Fiery's wild magic can still pull any spell on the list.
  - **Missed spells aren't lost.** A spell not picked stays unknown, so a later NPC can offer it again.
- **Follow-up**: the build is in `to_do_list.md` (section 1, Items, loot and magic). Still open in section 2: Fiery's starting spells and the spell list review.

### Spells - who casts and how recipes work (section 2, Items and magic)
- **Moved here**: 2026-11-19
- ✅ **Decided 2026-11-19**:
  - **Casters**:
    - **Fiery** from the start.
    - **The player** once the magic component seller is unlocked (the same gate as Mage Blast).
    - **Fluke**, luck spells only. Any other spell is a 50/50 gamble on him: half the time it fizzles (turn and components wasted), half the time it backfires (a damage spell hurts him, a buff goes to an enemy).
  - **About 10 spells in the MVP**, plus Mage Blast.
  - **Recipes are mostly by group**: one key component plus filler from a group (e.g. 1 Rubeetles + any 2 berries). Rare components only for the big spells.
  - **The spell list** is drafted in `spell_list.md` (10 spells: 4 damage, 3 support, 3 luck) and waits for the user's review.
- **Still open** (`to_do_list.md`, section 2): the list review, and how spells are learned - the user wants to settle that after the spells.

### Catalog trim and former-Epic items (section 2, Items and magic)
- **Moved here**: 2026-11-19
- ✅ **Decided 2026-11-19**:
  - **Weapons**: all 9 types stay, 6 each = 54 (2 Common, 2 Uncommon, 1 Rare, 1 Legendary per type). Signature items (Boon's Crossbow) come on top.
  - **Armor**: 4 weight types, the same for everyone now that classes are out - Cloth (no speed cost), Leather (-5% speed), Chainmail (-10%), Plate (-20%), with defense rising in that order (placeholder numbers). Robe merges into Cloth, and Shield goes dormant (it doesn't fit the single armor slot). 6 per type = 24, the same rarity split as weapons.
  - **The keep-list** is drafted in `catalog_trim.md` and waits for the user's review. Cut items stay in the code, dormant.
  - **Former-Epic items**: the keep-list settles the weapons and armor - every former-Epic weapon is cut, and 4 former-Epic armor sets are promoted to fill the empty Legendary armor tier. Evasion, Iron Will, Precision Training and Swift Tactics stay **Rare**. The Greater Revival Potion becomes **Legendary**. The Combat Elixir stays Rare until it becomes a spell.
  - **Royal Guard Mail** (placeholder name): a new signature Uncommon chainmail (about 30 defense) as Risk's starting armor, replacing the Rusty Chainmail Set on him. It's the one that leaves with him. ("Royal Guard's Armor" was only ever the name of his block ability.)
- **Follow-up still in `to_do_list.md`**: the keep-list review (section 2); applying the trim (section 1, Items, loot and magic).

### Mage Blast - the free, component-free spell (section 2, Items and magic)
- **Moved here**: 2026-11-19
- ✅ **Decided 2026-11-19**: the free weak spell is **Mage Blast** (a generic placeholder name, in the spirit of a "magic missile"). It never misses, does low damage (about half a basic weapon hit at the same level), can't crit, has no cooldown and needs no components. Its name or description makes clear it is weak because it uses none.
  - **Fiery and Fluke** always have it.
  - **The player** can't use it until they have unlocked the magic component seller by completing that NPC's side quest.
- **Follow-up still in `to_do_list.md`**: the build (section 1, Items, loot and magic); the component seller's unlock quest (section 2, Quests).

### Companions (section 2, Companions)
- **Moved here**: 2026-11-19
- ✅ **Decided 2026-11-19**:
  - **Starting gear**: each companion joins with their story/signature gear (Boon's crossbow, Risk's Royal Guard armor) at their join level, with pre-rolled upgrades like dropped weapons, so a late recruit isn't a downgrade.
  - **Swapping gear**: the player can swap everything on every companion. Moving a signature item off its owner shows a warning that it may not be as effective on someone else - the freedom is the player's, and the game recommends leaving it where it is.
  - **Chance can't wear gear in cat form.**
  - **Recruitment order**: Chance (tutorial), Fiery the Mage, Boon, Risk, Felicity, Fluke.
  - **Chance**: the placeholder numbers stay (85% accuracy, 5% crit, 80 HP + 8 a level, speed 14), to retune after playtesting. In human form (after the Demon Prince) he uses a weapon and gear like the player, and his gear does nothing while he's a cat. Form switch: a fixed 20% chance, rolled **after** each attack, to flip form for next time, so the current attack is always the form shown.
  - **Risk**: better armor raises his block chance a little - +0 / +3 / +6 / +10% for Common / Uncommon / Rare / Legendary armor, still capped at 70%. Renaming "Royal Guard's Armor" waits for the writing pass.
  - **Felicity**: the defaults are confirmed (Healing Circle 60% of a Divine Light heal on a 4-turn cooldown; on auto it's used when 2+ allies are below 50%; Divine Light removes debuffs and adds regeneration). Her heals don't raise speed.
  - **Boon**:
    - **Ice bolt**: a 25% chance to freeze the target (it loses its next turn - `SkipTurnEffect`).
    - **Acid bolt**: the target's defense drops (about -30%) for 3 turns.
    - The other numbers stay as placeholders (95% floor, Pinning -5 speed for 2 rounds, axe throw 75%, Fire burns, bolts 6-10 gold, the auto rules).
    - Bolt sources: the Shop Keeper sells Piercing and Pinning; Fire, Ice and Acid only come from chests and drops.
    - His unwired +10% accuracy passive is dropped, and 95% is his ceiling - nothing pushes him above it.
    - His crossbow has Uncommon-level damage and levels and upgrades at the blacksmith like any weapon.
  - **Fluke**: his ability is a passive party aura (placeholder name **Lucky Aura**). Each party hit, crit and damage roll has a chance (about 10% → 30% with his level) to be rolled twice, keeping the better result. He uses any weapon like the player, with a simple auto (he attacks, and the aura works on its own). He joins last.
  - **Fiery the Mage** (the companion Mage is now a woman named Fiery):
    - **Anger** is a 0-100 meter shown on screen. It rises when she's hit, when an ally goes down, or when her spell misses. It falls a little each round she isn't hit, and a lot when she Defends.
    - **Wild magic**: the chance a different spell comes out is half her anger (0% calm, 50% at max).
    - **Backfire**: only above 75 anger, about 1 in 4 wild spells hits a random party member instead (a damage spell hurts them, a heal heals an enemy).
    - Anger also raises her spell damage, up to +50% at max. On auto she Defends to calm down once she's above 75.
    - Which spells can come out waits on the spell design.
  - **Targeting patterns**: Random, Most wounded, Weakest defense, Last attacker, Biggest threat (a front-row pattern later, with positions). Each follows its pattern about 70% of the time and picks at random 30%.
    - Enemies: Goblin Random, Bandit Most wounded, Dragon Biggest threat, Dragling Random, Castle guard Last attacker. New enemy types get theirs when designed. Bosses keep their own mechanics.
    - Auto companions: Chance Random, Fiery Last attacker, Boon Biggest threat, Risk Most wounded, Felicity Weakest defense (when not healing), Fluke Random.
- **Follow-up still in `to_do_list.md`**: the builds (section 1, Companions); Fiery's spell list (section 2, Items and magic - Spells); names and text for the writing pass (section 2, Companions).

### Abilities and gear levels (section 2, XP, levels and abilities)
- **Moved here**: 2026-11-19
- ✅ **Decided 2026-11-19**:
  - **Weapon masteries merge into 4 families**: Blades (sword, dagger), Heavy (axe, mace, spear), Ranged (bow, crossbow), Arcane (staff, wand). Each family has 2-3 weapon types' worth of drops to draw on.
  - **Level-1 abilities start at about 1/3 of their max power** (e.g. Attack Boost +15% at level 1, growing to +50% at its cap). Executioner starts with a small bonus, not a penalty. This applies to every ability.
  - **One ability per player, chosen once and kept for good** - still fits.
  - **Defaults confirmed**: cooldown reductions at 1/4 and 3/4 of an ability's cap; the old Epic loot chances merged into Rare. (Whether rarity slows levelling was settled the same day: it doesn't - see "XP rewards, caps and ability XP".)
  - **Weapon level and stats**: a weapon that drops at level N comes with N-1 upgrades already rolled (random paths). After that the player picks the path at the blacksmith as now. Armor keeps its level formula.
  - **5 new abilities fill the 5 freed slots** (19 in all again). Placeholder numbers, to retune after playtesting:
    - **Gambler's Strike** (active, Uncommon, 3-turn cooldown): an attack with a rolled damage multiplier, 0x / 0.5x / 1x / 1.5x / 2x / 3x. It averages about 1.1x at level 1, and levels shift the odds toward the high end (about 1.6x average at max).
    - **Second Chance** (passive, Common): a missed attack has a 15% → 40% chance to reroll.
    - **Lifesteal** (passive, Rare): heal 5% → 20% of the damage dealt.
    - **Riposte** (passive, Uncommon): when hit while defending, a 25% → 60% chance to counter with a normal attack.
    - **Bulwark** (passive, Legendary, placeholder name - the user's idea): the whole party takes 10% → 60% less damage. It multiplies with Defend and Defense Boost (each applies to what's left), but the total reduction is capped at 75%.
- **Follow-up still in `to_do_list.md`**: the builds (section 1 - "XP, levels and abilities", "Items, loot and magic", "Code cleanup").

### XP rewards, caps and ability XP (section 2, XP, levels and abilities)
- **Moved here**: 2026-11-19
- ✅ **Decided 2026-11-19**:
  - **Random XP bonus - tiers.** On top of the guaranteed base: most fights +0-10%, about 1 in 5 "Good fight!" +25%, about 1 in 20 "Lucky!" +50% (about +11% on average). This replaces today's 10% chance of +50%. It applies to player and companion XP only. Ability and gear XP don't get it.
  - **XP past a cap is lost** - the player at 40, an ability or a piece of gear at its rarity's cap. Nothing is banked.
  - **Ability XP amounts.** A passive earns a small amount on every related trigger, and an active use is worth 5x one passive trigger (e.g. 2 XP and 10 XP). The XP needed per level is set so either kind takes about 2 fights a level (the player's pace - see the per-boss level target).
  - **Flat XP per active use.** Today's +15% climb for repeat uses in one fight is dropped.
  - **The same XP cost per level for every rarity** (the 100 / 200 / 300 / 500 bases go). A higher rarity is a longer road only because its cap is higher (10 / 20 / 30 / 40).
  - **Passive triggers count every chance, not just successes**: Evasion earns whenever its owner is attacked, Iron Will every round its owner has a debuff, whether or not it works.
  - **"50% ability XP"** for downed and benched companions means half of what the player's ability earned that fight.
  - **Rallying Cry, Swift Tactics and Precision Training become party-wide**, player included. They earn XP on every party attack (Rallying Cry, Precision Training) or every party turn (Swift Tactics) while active.
- **Follow-up still in `to_do_list.md`**: the builds (section 1, "XP, levels and abilities").

### The per-boss level target (section 2, XP, levels and abilities)
- **Moved here**: 2026-11-19
- ✅ **Decided 2026-11-19**: an even curve, about 5 party levels per boss. A boss still fights at the party's average level + 5, so this sets pacing (the XP numbers), not boss strength.

  | Boss | Party level | Boss level |
  |---|---|---|
  | #1 Skarn | 1 | 2 (fixed) |
  | #2 Librarian-Monk | 5 | 10 |
  | #3 Centipede | 10 | 15 |
  | #4/5 Cyclops Brothers | 15 | 20 |
  | #6 Demon Prince | 20 | 25 |
  | #7 Slime Dragon | 25 | 30 |
  | #8 Captain | 30 | 35 |
  | #9 Princess 🤫 | 35-38 | 40-43 |

  - A normal (non-grinding) player is around level 35-38 at the final boss, not capped. Grinding (e.g. the army camp) can still reach 40 before her.
  - About 8-12 regular fights between two bosses, so roughly 70-85 regular fights in the whole game.
- **Follow-up still in `to_do_list.md`**: the XP rework (section 1, "XP, levels and abilities"); the fight table in `boss_stats_draft.md` needs redoing at these levels.

### Boss defaults confirmed (section 2, Bosses)
- **Moved here**: 2026-11-19
- ✅ **Decided 2026-11-19**: every default built on 2026-11-11 for these bosses stays as it is (no code change):
  - **Librarian-Monk**: the shield's level-1 health is 20. A lost turn doesn't count toward his shield cooldown.
  - **Centipede**: "a restack only adds the band's minimum turns" applies to his poison only. Burn, Bleed and the regular Poisoned keep the old rule (a stack, back to full time). Each stack adds the full damage again. He targets unpoisoned party members 3.5x as often (weights 7 to 2).
  - **Cyclops Brothers**: the enrage is the existing Rage effect at his level, and a second rage refreshes it with no stacking. It keeps running after his brother is revived. While one brother is down his slot is skipped. The fight pays both brothers' XP and gold but counts as one boss beaten.
  - **Demon Prince**: landing a condition the target already has follows that condition's normal rule (damage over time stacks, slows refresh). His strength numbers stay as built, to retune after playtesting.
  - **Princess** 🤫: action weights attack 50 / skip 25 / guards 25, shifting to 40 / 40 / 20 at half health or below. Her second turn is slower by Normal 6-8, Hard 4-6, Difficult 2-4, Unfair 1-2. A skip only goes to someone who isn't already losing a turn (otherwise she attacks). Castle guards are called two at a time, only when none are alive, at the party's level.
- **Follow-up still in `to_do_list.md`**: names, titles, descriptions and locations (section 2); testing (section 3).

### Demon Prince - his unique conditions (section 2, Bosses)
- **Moved here**: 2026-11-19
- ✅ **Decided 2026-11-19**: 6 unique conditions join his 5 existing ones (Poisoned, Burning, Bleeding, Grappled, Pinned), so there are 11 in all, each equally likely on a hit. Names and numbers are placeholders, to retune after playtesting. They scale with his level only, except Polymorph's duration, which goes by difficulty. Every cure (antidote, Iron Will, Felicity's Divine Light debuff removal) removes all of them, including turning a chicken back.
  - **Hexed**: -15% hit chance for 3 turns.
  - **Weakened**: -25% damage dealt for 3 turns.
  - **Soul Siphon**: damage each turn (his burn/bleed number, 10 at level 21) for 3 turns, and he heals by whatever it actually deals.
  - **Hellfire**: burns at his burn number for 3 turns, and the target can't be healed by anything while it lasts.
  - **Possessed**: on the target's next turn, they make a normal attack (their own damage) on a random standing party member, never themselves. Then it ends.
  - **Polymorph** (the user's idea): the target becomes a chicken for a rolled number of turns: Normal 1-2, Hard 2-3, Difficult 2-4, Unfair 3-5. The chicken keeps its current and max HP, but everything else is the chicken's own: peck damage 1-3 + level/4 (6-8 at level 21), defense 0, speed 15. Gear and weapon do nothing while polymorphed. The chicken can only Peck or Defend (no abilities, items or fleeing).
- **Follow-up still in `to_do_list.md`**: the build (section 1, "#6 Demon Prince").

### Captain of the Royal Guard - when Risk leaves and his gear (section 2, Bosses) 🤫
- **Moved here**: 2026-11-19
- ✅ **Decided 2026-11-19**: Risk leaves at a story moment (a cutscene or quest step) between boss #7 (Slime Dragon) and boss #8 (him). When he leaves, any gear the player equipped on him comes back to the bag. His own starting gear (the Royal Guard armor) leaves with him, since he fights in it as the boss.
- **Follow-up still in `to_do_list.md`**: the leave-party plumbing (section 1); the story moment itself needs writing (section 2).

### Only the next boss's information is shown (boss list and boss quests)
- **Moved here**: 2026-11-11
- ✅ **Decided 2026-11-11**: the boss list and boss quests show information only for the next boss the player has yet to defeat. A boss that comes later in the game shows no information at all - just that it is locked or inaccessible for now. This also settles the old spoiler worry about the final quest showing at game start.
- **Follow-up still in `to_do_list.md`**: built the same day, testing in section 3.

### Captain of the Royal Guard - why Risk leaves the party (section 2, Bosses) 🤫
- **Moved here**: 2026-11-11
- ✅ **Decided 2026-11-11**: Risk was told to keep an eye on the party for the princess. He only joined the party to keep her informed of what they were doing. 🤫 This ties into the hidden final-boss twist - design docs only, never game text or foreshadowing.
- **Follow-up still in `to_do_list.md`**: when exactly he leaves, his gear, his crit numbers (section 2); the build (section 1).

### Slime Dragon - does beating him end the fight? (section 2, Bosses)
- **Moved here**: 2026-11-11
- ✅ **Decided 2026-11-11**: beating the dragon means he stops spawning draglings, but the fight doesn't officially end until every enemy is defeated - the draglings left still have to be beaten.
- **Follow-up still in `to_do_list.md`**: the dragling numbers (section 2); the build (section 1).

### Cyclops Brothers - who is faster (section 2, Bosses)
- **Moved here**: 2026-11-11
- ✅ **Decided 2026-11-11**: their real names aren't picked yet, so they use placeholder names; one is "Fast", and he is the faster brother.
- **Follow-up still in `to_do_list.md`**: the real names, the enrage details and the rewards question (section 2); testing (section 3).

### Librarian-Monk's shield rules (section 2, Bosses)
- **Moved here**: 2026-11-11
- ✅ **Decided 2026-11-11**: the shield cuts each hit in half, rounded down. Its health is a set amount at level 1 that scales with level and difficulty. The 3-turn cooldown starts when it is raised. Raising it costs his turn. He starts every fight with it raised (shown in the pre-fight cutscene), and that starts the cooldown on his turn 1. Whenever his turn comes and it isn't up (and the cooldown is over), he has a chance to raise it: Normal 40%, Hard 50%, Difficult 60%, Unfair 70% - he's a bit of a weakling.
- **Follow-up still in `to_do_list.md`**: built 2026-11-11, testing in section 3; his name and text, and the shield's level-1 health number, in section 2.

### Bucket A #1. Gender selection
- **Moved here**: 2026-10-15
- ✅ **Resolved (2026-10-05)** Character-creation Gender selection has no stat/mechanical difference between genders - purely cosmetic/narrative.
- **Follow-up still in `to_do_list.md`**: building the selection itself (Tier 4).

### Bucket A #6. Princess (final boss) mechanic
- **Moved here**: 2026-10-15
- ✅ **Resolved (2026-10-14)** Princess (final boss) - her unique mechanic is decided, and it replaces the earlier "randomly use one of the other bosses' mechanics" idea.
- **Follow-up still in `to_do_list.md`**: the full design, its open points and the build work are under Tier 2 #9.

### Fight continues while a party member stands (`combat/` review)
- **Moved here**: 2026-10-15
- ✅ **Decided 2026-10-14**: when the player falls and companions are still standing, the fight continues. Lost = the whole party is down.
- **Follow-up still in `to_do_list.md`**: written in the "Combat code pass" under `game_logic/combat/`, not built or tested yet.

### Flee chance replaces the flat 30% (`combat/` review)
- **Moved here**: 2026-10-15
- ✅ **Decided 2026-10-14**: the flat 30% flee chance for everything is replaced by a chance per enemy type, with no fleeing from bosses.
- **Follow-up still in `to_do_list.md`**: written in the "Combat code pass" under `game_logic/combat/`, not built or tested yet.

## Code Cleanup

### Backend test rework
- **Moved here**: 2026-11-05
- ✅ **Done (2026-11-04)** The backend tests were reworked against the current code, and they pass. DamageCalculator, Player, SaveManager and TurnManager tests are switched back on, `CombatManagerTests` is rewritten, and gold-reward tests moved to the new `LootGeneratorTests.cs`. Two bugs were found and fixed along the way: a double-counted RNG roll statistic, and a party-level penalty in `DifficultyScaler` that cancelled itself.
- **Follow-up still in `to_do_list.md`**: `QuestManagerTests` (switched off until the quest changes are built), new tests, frontend tests and CI, all under "Unit tests".

### Condense the to-do list (planned 2026-11-03)
- **Moved here**: 2026-11-04
- ✅ `to_do_list.md` was rewritten as short checklists in four sections: ready to build, needs creative input, built and needs testing, and other. The old long-form text, with its reasoning, code findings and history, was moved mostly unchanged to `docs/code_review_notes.md`. The old status summary at the end was dropped as out of date.

### Order-based boss progression (Tier 0)
- **Moved here**: 2026-10-15
- ✅ **Done (2026-09-30)** Remove the Champion Key/Final Gate system, replace with order-based progression (defeating boss N-1 unlocks boss N). `BossManager` now has `IsBossUnlocked`/`GetNextBoss`/`GetFinalBoss`/`IsFinalBoss` derived from registration order instead of keys/randomness.
- ✅ **Done (2026-09-30)** "Champion" → "Boss" terminology cleanup across the boss/quest system (`boss_manager.cs`, `boss_enemy.cs`, `boss_definitions.cs`, `boss_encounter.cs`, `final_boss_quest.cs`, `boss_defeat_quest.cs`, `quest_giver.cs`).

### Deleted files (code review, 2026-10-09)
- **Moved here**: 2026-10-15
- `world/dungeon.cs` - **DELETED 2026-10-09**. Dungeons are out of scope, including for future versions. Nothing referenced it.
- `data/game_data.cs` - **DELETED 2026-10-09** (362 lines). `GameDatabase` was an older, second item/enemy/companion/ability database, unused even by tests and superseded by `ItemDatabase` and the real factories. Its entries in `RNGTheGame.csproj` and the README tree were removed too.
- `data/constants.cs` - **DELETED 2026-10-09** (11 lines). Nothing used it; the same numbers are defined where they are needed elsewhere in the code, and a separate constants file is not wanted.

### README project tree
- **Moved here**: 2026-10-15
- ✅ The README's "Project Structure" tree listed a `world/location_type.cs` that does not exist and had not been checked against the other folders. Brought up to date 2026-10-13 and kept current since.
- **Follow-up still in `to_do_list.md`**: the rest of the README (features, status, test count) still overstates what works (root files review).

## GUI & Game Engine

### Combat scene - the dead Attack button
- **Moved here**: 2026-11-14
- ✅ **Confirmed fixed by the user 2026-11-14**: the combat scene's Attack button now works. The likely cause (logged in `code_review_notes.md`) was `CombatManager.ShowCombatIntro()` calling `Console.Clear()`, which throws with no real console window - true inside Godot - stopping the fight before it started. Knox's "no console output" work (2026-11-10) removed every `Console.Clear()`/`Console.ReadKey()` call from `game_logic/`, and the user has now tested it directly in Godot and confirmed Attack responds correctly.

### 3. Get Godot
- **Moved here**: 2026-10-13
- **Status**: ✅ Done (installed, Godot 4.7.2 mono build)
- **Phase**: GUI Development

---

## Session History (2025-12-05) - what was written, NOT current status
- **Moved here**: 2026-10-15

> **Read this as history only.** These entries record code that was written in that session. Several of the systems are not connected to the game, and the boss key system has since been removed. For current status use the numbered items and SUMMARY BY STATUS above.

### Boss Progression System (key/gate version - REMOVED 2026-09-30)
- ✅ Full combat integration with boss scaling and key drops
- ✅ BossManager integrated into GameManager
- ✅ Boss encounter system with warnings for repeat fights
- ✅ Final gate key consumption (10 keys required)
- ✅ Final gate unlock bug fixed (prevents save/load exploit)
- ✅ Dual scaling system:
  - 15% stronger per unique boss defeated
  - 50% stronger per repeat defeat of same boss
- ✅ Diminishing returns on key drops (100% → 50% → 25% → 12.5%...)
- ✅ Save system preserves boss progression, defeats, and repeat counts
- ✅ Boss combat verified - uses same combat system as regular enemies

### Quest System (code written - not connected, see item 6)
- ✅ Quest base architecture (Quest, QuestObjective, QuestReward, QuestManager)
- ✅ 8 quest types with 34+ total quests:
  - 14 Boss defeat quests
  - 1 Final boss quest (special: requires acceptance)
  - 5 Level progression quests
  - 3 Enemy kill quests
  - 3 Gold collection quests
  - 2 Weapon upgrade quests
  - 2 Equipment quests
  - 4 Challenge quests
- ✅ **Quest Giver NPC**: "Veteran Ranger" for all boss quests
- ✅ **Job Board Menu**: Browse, accept, track, and claim all other quests
- ✅ **RNG Quest Generation**:
  - Job board quests randomized once per save file
  - Requirements: ±1-6 variance depending on quest type
  - Rewards: ±20-30% variance for gold and XP
  - Boss quests remain fixed (100g + 50 XP) for consistency
- ✅ **Quest Persistence**:
  - Complete quest data saved (requirements, rewards, progress, state)
  - ReconstructQuests() rebuilds with original RNG values on load
  - Each save file has unique quest challenges
- ✅ Retroactive completion (progress tracks before acceptance)
- ✅ Active quest tracking with ★ marker
- ✅ Quest log with detailed progress display
- ✅ Menu integration (Options 8, 9, 10 in main game loop)

### Settings Menu System (code written - mostly not connected, see item 8)
- ✅ **GameSettings Class**: Complete settings data structure
- ✅ **Settings Categories**:
  - Display settings (turn order, combat log, damage calculations, enemy stats)
  - Gameplay settings (auto-save, confirmations)
  - RNG settings (algorithm selection, statistics tracking)
  - Accessibility settings (colored text, emojis, text speed)
  - Audio settings (placeholder for GUI)
  - Difficulty settings (Normal/Hard/Difficult/Unfair)
- ✅ **Difficulty System**:
  - 4 difficulty levels affecting enemy stats (75%-200%) and rewards (80%-150%)
  - Immutable after save file creation (prevents exploitation)
  - Selected at new game creation before character name
- ✅ **SettingsMenu**: Interactive menu with 6 submenus
- ✅ **Full Save/Load Support**: All settings persist in save file
- ✅ **Integration**: Accessible from pause menu (option 4)
- ✅ **Reset to Defaults**: Option to reset all adjustable settings

### Statistics Tracking System (code written - not connected, see item 9)
- ✅ **StatisticsTracker Class**: Comprehensive tracking system
- ✅ **Statistics Categories**:
  - Combat (battles, damage, kills, bosses, win streaks)
  - Economic (gold flow, purchases, sales)
  - Equipment (upgrades, levels, weapon usage)
  - Item usage (consumables, abilities)
  - Exploration (shops, NPCs, quests)
  - Progression (level, XP)
  - Achievements (flawless victories, close calls, perfect crits)
  - Miscellaneous (play time, sessions, version)
- ✅ **StatisticsMenu**: 8 categorized viewing pages + summary overview
- ✅ **Calculated Stats**: Win rate, averages, favorites, highlights
- ✅ **Dictionary Tracking**: Top kills by enemy, weapons used, consumables used
- ✅ **Full Save/Load Support**: All statistics persist in save file
- ✅ **Integration**: Accessible from main menu (option 11)
- ✅ **Ready for Game System Integration**: All tracking methods implemented

### Previous Session Completions
- ✅ Weapon upgrade system with player choice at each level (9 weapon types)
- ✅ RevivePotion system with three tiers
- ✅ ShopKeeper system with 3-layer dynamic pricing
- ✅ Dual restock system (time + combat based)
