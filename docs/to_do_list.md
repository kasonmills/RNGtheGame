# RNGtheGame - To-Do List

**Four sections:**
1. **Ready to build** - fully decided, just needs coding.
2. **Needs creative input** - a decision, design or numbers from you come first.
3. **Built, needs testing** - written but not yet built and played. Each item says what to check.
4. **Other** - non-code work, and ideas parked until after the MVP.

**Rules**
- A feature with parts in several states is split across sections under the same name (e.g. "Boon"), so search for the name to see all of it.
- When an item is finished (built **and** tested) or fully resolved, move it to `done.md`.
- The reasoning, code findings and history behind each item are in `code_review_notes.md`. `(notes: ...)` points to the matching place there.
- 🔴 = high priority. ⛔ = blocked by something else (says what).

---

# 1. Ready to build

### Bosses - strength
Nothing left here - the hand-set boss stats are built (section 3). Boss rewards are with the XP rework ("Enemy XP and gold values become the guaranteed base reward", Combat).

### Bosses - one by one
- [ ] **#6 Demon Prince - add his 6 unique conditions** (designed 2026-11-19 - full rules in `done.md`). He is built (2026-11-11, section 3) with the 5 existing conditions. Each new one is one entry in `DemonPrinceMechanic.ConditionPool`, making 11, all equally likely. All of them can be cured.
  - **Hexed** (-15% hit, 3 turns) and **Weakened** (-25% damage, 3 turns): new stat debuffs.
  - **Soul Siphon**: damage over time that heals him by what it deals.
  - **Hellfire**: a burn that blocks all healing on the target while it lasts (needs a "can't be healed" check in every heal path).
  - **Possessed**: new engine behaviour. The target's next turn is taken over by a normal attack on a random standing ally, never themselves.
  - **Polymorph**: the target becomes a chicken (Normal 1-2 / Hard 2-3 / Difficult 2-4 / Unfair 3-5 turns). It keeps its HP, but its stats become the chicken's (peck 1-3 + level/4, defense 0, speed 15), and gear does nothing. Only Peck or Defend can be chosen.
- [ ] **Companion → boss plumbing for Risk** - a leave-party trigger built as a hook a story moment will call (it goes between boss #7 and #8 - decided 2026-11-19), and a way to turn the companion Risk into the Captain boss (built 2026-11-11 as a standalone boss - section 3). On leaving: gear the player equipped on him goes back to the bag; his own Royal Guard armor leaves with him.
- [ ] **Princess: quest-giver → boss transition.** 🤫 She gives story quests for most of the game, then becomes the final boss (built 2026-11-11 as a standalone boss - section 3). Needs the story quests and the moment she turns. No foreshadowing text anywhere.

### Companions
- [ ] **Save the per-companion direct-control toggle** (it is lost on load today).
- [ ] **Recruitment beyond the tutorial** - a real way to recruit companions. Only Chance can be recruited today. Order (decided 2026-11-19): Chance (tutorial), Fiery the Mage, Boon, Risk, Felicity, Fluke.
- [ ] **One combat decision-maker for everything the computer controls** (enemies and auto companions), returning the same kind of action the player submits. Targeting is part of it. `enemy_ai.cs` is the obvious home. Patterns (decided 2026-11-19 - assignments in `done.md`): Random, Most wounded, Weakest defense, Last attacker, Biggest threat. Each follows its pattern about 70% of the time, 30% random.
- [ ] **Companion gear rules** (decided 2026-11-19): companions join with their signature gear at their join level, with pre-rolled upgrades. Everything can be swapped, but moving a signature item off its owner shows a "may not be as effective" warning. Chance can't wear gear in cat form. Risk's block chance gets +0 / +3 / +6 / +10% from his armor's rarity (still capped at 70%).
- [ ] **Boon's changes** (decided 2026-11-19): Ice bolt (25% freeze = `SkipTurnEffect`), Acid bolt (defense about -30% for 3 turns). The Shop Keeper sells Piercing and Pinning only; Fire, Ice and Acid come from chests and drops. Drop his unwired +10% accuracy passive, with 95% as his ceiling. His crossbow has Uncommon-level damage and upgrades like any weapon.
- [ ] **Chance: human form and form switching** - comes after the Demon Prince in the story (decided 2026-11-19): in human form he uses a weapon and gear like the player (gear does nothing while he's a cat). After each attack, a fixed 20% chance flips his form for next time. Save his current form.
- [ ] **Fluke: build him** (decided 2026-11-19): a passive party aura (placeholder "Lucky Aura") - each party hit, crit and damage roll has about a 10% → 30% chance (with his level) to be rolled twice, keeping the better. Needs a new party-wide effect (today's effects are single-target only). Any weapon, simple auto. He can cast luck spells; any other spell is a 50/50 fizzle (turn and components wasted) or backfire on him.
- [ ] **Fiery the Mage: rework `companion_mage.cs`** into the angry wild-magic Mage - now a woman named Fiery (decided 2026-11-19 - full rules in `done.md`): a 0-100 anger meter; wild-spell chance = half her anger; above 75 anger, about 1 in 4 wild spells backfires on the party; anger adds up to +50% spell damage; on auto she Defends to calm down above 75. She starts with Fireball, Frost Lance, Chain Lightning, Poison Cloud and Mage Blast (decided 2026-11-20). She is the second recruit, so she needs the magic system early. ⛔ Needs the magic system.

### Combat
- [ ] **Enemy roster leftovers** (after the rebalance build, 2026-11-24 - see section 3):
  - **The Imp's other conditions:** it only burns today. Add Hexed and Weakened (weaker versions) to `EnemyRoster.Imp` when the Demon Prince's new conditions are built.
  - **Component favour:** spiders and beetles drop bug components, and Fungusoars mushroom components, at 2x weight - with "Component drops" (Items, loot and magic).
  - **Base XP and gold:** `StandardEnemy.StandardXPPerLevel` (50) and `StandardGoldPerLevel` (12) are placeholders, times each enemy's XP weight - set them with the XP rework.
- [ ] **Weapon screens show the un-doubled damage.** The party's weapon damage is ×2 in the damage calculation (`DamageCalculator.PartyDamageScale`), but `Weapon.GetInfo()`/`GetDisplayName()` still show the weapon's own range. Show the real range, or fold the ×2 into the item numbers once the scale is settled.
- [ ] **Enemy XP and gold values become the guaranteed base reward**, with a random extra on top. Do this together with XP rework item "Random XP rewards" below. Bosses too: today a boss's fight XP is rolled from its level and its gold from the loot table, so `BossEnemy.XPValue`/`GoldValue` and the scaler's boss reward multipliers (XP x5, gold x3-5) aren't used yet - wire them in here (decided 2026-11-10, left out of the boss stat build 2026-11-11).
- [ ] **Formation leftovers** (the rows and Reposition are built - section 3, "Formation, rows and Reposition"):
  - **Spells:** spells ignore rows - the spell build must not add the -10 or the melee cut.
  - **Auto companions and rows:** `enemy_roster.md` says auto companions pick back-row enemies less often. Today they always go for the most wounded enemy, so add the 2:1 weighting with "One combat decision-maker" (Companions).
  - **Godot:** the formation view in the party menu (`PartyManager.SetFormationSlot`), the rows on the combat screen (`CombatManager.GetSlot`/`GetRow`), and the Reposition button (target a party member or an empty slot - `GetValidTargets(actor, ActionType.Reposition)` and `GetFreeSlots()`).
  - **Tutorial text:** Reposition is now one of the actions to try in the Skarn fight; its hint text comes with the tutorial writing.
- [ ] `DamageCalculator.CalculateHealing()`'s passive healing bonus is never applied. Do it with "Rescale every ability for the level-40 cap" (XP, levels and abilities), since the bonus numbers change there. `Player.Heal` is the likely hook (check for heals that set `Health` directly).

### Tutorial
- [ ] **Chance revives the player once** (decided 2026-11-23): the first time the player goes down in the Skarn fight, Chance revives them (with a short line) and the fight goes on. A second fall loses, and the tutorial is replayed (today any loss goes to Game Over).
- [ ] **The potion step comes after Skarn's first hit on the player** (decided 2026-11-23), so using the potion has a visible effect.

### XP, levels and abilities
_The per-boss level target is decided (2026-11-19 - table in `done.md`): party levels 1 / 5 / 10 / 15 / 20 / 25 / 30 at bosses #1-#8, 35-38 at the final boss, about 8-12 regular fights between bosses._
- [ ] **Rework the player's XP curve** to that target: about 5 levels per boss, and around 35-38 at the final boss for a player who doesn't grind (today the cap is reached in about 16 fights). Fix enemy XP growth at the same time.
- [ ] **Companions use exactly the player's XP system** - same curve and cap. Replace the separate curve in `companion_base.cs`.
- [ ] **Ability XP** (rules decided 2026-11-19 - see `done.md`): passives earn a small amount on every related trigger, and an active use is worth 5x a passive trigger. XP per use is flat (drop the +15% repeat climb in `GetCombatScaledExperience`). Every rarity costs the same per level (drop `GetBaseXPByRarity`). Set the numbers so an ability takes about 2 fights a level, like the player. Triggers count every chance: Evasion when attacked, Iron Will every round with a debuff.
- [ ] **Rallying Cry, Swift Tactics and Precision Training become party-wide**, player included (decided 2026-11-19). They earn XP on every party attack (Rallying Cry, Precision Training) or every party turn (Swift Tactics) while active.
- [ ] **Weapons and armor gain XP from use** (hits landed for weapons, hits taken for armor). Nothing awards gear XP today.
- [ ] **Random XP rewards** - every player/companion XP reward is a guaranteed base plus a tier roll: most fights +0-10%, about 1 in 5 "Good fight!" +25%, about 1 in 20 "Lucky!" +50% (decided 2026-11-19). It replaces the 10%-chance +50% in `CalculateXPReward`. Ability and gear XP don't get it.
- [ ] **Rescale every ability for the level-40 cap** (per-level bonus, rank names, cooldown steps, unlock levels). Level 1 starts at about 1/3 of the ability's max power (decided 2026-11-19), and Executioner starts with a small bonus instead of a penalty.
- [ ] **Build the 5 new abilities** (designed 2026-11-19 - numbers in `done.md`): Gambler's Strike (active, rolled 0x-3x damage), Second Chance (reroll a miss), Lifesteal, Riposte (counter while defending), Bulwark (party-wide damage reduction; with Defend and Defense Boost the total is capped at 75%). Offer them at character creation.
- [ ] **Ability half of XP sharing** - downed and benched companions' abilities get half of what the player's ability earned that fight (decided 2026-11-19). A late recruit's ability levels match the party.
- [ ] **Save the XP estimate script in `tools/`** so new numbers can be checked before going into the code.

### Items, loot and magic
- [ ] **Loot picks an item of the rolled rarity** instead of relabelling a random item (a "Legendary" Rusty Sword can drop today). Components already work this way.
- [ ] **Apply the catalog trim** (decided 2026-11-19 - list in `catalog_trim.md`, reviewed and final 2026-11-24). Steps:
  - mark every item not on the list dormant (no drops, no shop stock)
  - Robe and Shield go dormant
  - one speed cost per armor type for everyone (Cloth 0%, Leather -5%, Chainmail -10%, Plate -20%), replacing the class-based `GetArmorSpeedPenalty`
  - remove the `TODO: was Epic` markers: Evasion, Iron Will, Precision Training and Swift Tactics stay Rare; the Greater Revival Potion becomes Legendary; the 4 promoted armor sets become Legendary
  - add Royal Guard Mail (new signature Uncommon chainmail, about 30 defense) as Risk's starting armor
  - raise Boon's Crossbow to Uncommon-level damage: 10-20, keeping its 95% accuracy and 5% crit (decided 2026-11-24)
- [ ] **Random loot reaches the whole catalog** - `GetAllWeapons()`/`GetAllArmor()` miss most items. ⛔ Do it with the catalog trim, and build the lists from the item definitions so they can't drift.
- [ ] **Mage Blast - the free, very weak spell** (decided 2026-11-19): no components, never misses, low damage (about half a basic weapon hit at the same level), can't crit, no cooldown. Its name or description must make clear it is weak because it uses none. Fiery and Fluke always have it; the player only gets it once the magic component seller is unlocked (their side quest completed).
- [ ] **Every spell needs components**, all drawn from the one shared pouch. Each spell needs 3 unique components, none shared with another spell (decided 2026-11-20 - recipes in `spell_list.md`). Some spells also need a 4th from a named group ("any bug"), and the player picks which one is spent (a pick prompt handed back for Godot; on auto, spend whichever one the party has most of). The 4th doesn't change the effect. Spells scale with the caster's level and don't level up themselves. Only the big spells have cooldowns (placeholders: Doom 4 turns, Rekindle 5, Chain Lightning 2, Poison Cloud 2 - updated 2026-11-24). **Rekindle** (added 2026-11-24) revives a fallen ally at 25% max HP with the revival potion's speed penalty - reuse the revive path from `UseRevivePotion`. The spell list is final (2026-11-24): 11 spells plus Mage Blast. Spells aimed at enemies roll to hit with the caster's accuracy (one roll per enemy, components spent even on a miss); heals and buffs on allies always land. Damage spells can crit with the caster's crit chance. A spell without its components is greyed out, and the menu lists what's missing. All decided 2026-11-20.
- [ ] **Permanent stat potions** (decided 2026-11-20 - see `done.md`): crafted by the magic component seller. The player brings the potion's 4 components and pays her a crafting fee, and she makes it. They're never shop stock. Each one gives the player or a companion a small permanent boost to Max HP, Speed, damage, defense, crit or accuracy, with a cap per stat per character. Build: a crafting option at her shop (check the components and gold, take them, give the potion), a permanent-bonus stat on each character for damage, defense, crit and accuracy (added on top of gear, saved), a count of potions drunk per stat (saved, for the cap), the potion items, and a "this character has reached the cap" message handed back for Godot. Recipes are in `spell_list.md`. Boosts stay with the character, so they leave with Risk. **Numbers (decided 2026-11-23):** per potion +10 Max HP, +1 Speed, +2% damage, +2 defense, +1% crit, +1% accuracy; cap 10 potions per stat per character, Speed 5 (accuracy still never above 95%). Fee: 100 gold + 50 for each potion of that stat the character has already drunk (the fee depends on who will drink it, so the craft needs a target). Names: Vigor / Swiftness / Might / Iron / Keen / Eagle-Eye Draught.
- [ ] **Combat Elixir becomes a spell** (with components), not a consumable.
- [ ] **Spell scrolls and town spell NPCs** (decided 2026-11-20 - see `done.md`): the player learns spells from scrolls, either bought or given by an NPC. Each town has one spell NPC. It offers up to 3 spells picked from the ones the player doesn't know yet, or only the ones that remain if fewer than 3 are left, so there's always a new spell to get. The player gets one of them at random, and can read every offered description first. The offer and the pick are rolled once per save, so reloading can't change them. Scrolls teach the player only. A bought scroll costs more for each spell the player already knows (placeholder: 100 gold, plus 100 per spell known - decided 2026-11-20). Build: a known-spells list on the player (saved), a scroll item that teaches a spell, and the NPC offer and roll (saved per NPC). Whether each NPC sells the scroll or gives it is set per NPC with the fixed map.
- [ ] **Healing potions become candy/sweets**, sold by the Shop Keeper (decided 2026-11-23). Rename the Minor/Health/Greater Health Potion line (`ConsumableType.HealthPotion`) in `item_database.cs`, and heal a % of the target's max HP instead of a flat amount:
  - **Honey Drop** - 20%, 15 gold
  - **Sugar Plum** - 40%, 40 gold
  - **Rock Candy** - 75%, 100 gold
  - **Golden Toffee** - 100%, Rare, never sold (drops only), sells back for 125. It replaces the **Elixir of Vitality**, which also fixes its bug (it runs the Combat Elixir code and doesn't heal).
  - **Bread and Cooked Meat go dormant** (no drops, no stock).
  - The tutorial's Minor Health Potion becomes a Honey Drop.
  - The revival potions and the Antidote keep their names.
- [ ] **Component data** (decided 2026-11-23) in `spell_component.cs`: mushrooms, berries and herbs stay Common, buyable, 5 gold - sold by the Shop Keeper. Bugs, lucky and unlucky charms stay Uncommon at 20 gold, sold only by the magic component seller (today they're not buyable at all, so the flag needs to say which trader). The Negativity Crystal stays Rare and is never sold. Add the **Phoenix Ember** (41st component, Rare, drops only, never sold - Rekindle's key component, added 2026-11-24); a rolled Rare component drop picks between the crystal and the ember. Everything can still drop. Keep the 7 groups (crystal, lucky, unlucky, mushrooms, berries, bugs, herbs) - the "any bug" slots use them.
- [ ] **Component drops** (decided 2026-11-23): a component drop gives 1-3 of a Common or 1 of an Uncommon. Each map area favours one or two groups (2x drop weight, e.g. mushrooms in forests). ⛔ The area favour needs the fixed map; the amounts can be done now.
- [ ] **The first Negativity Crystal is given by an NPC** as a story or quest reward (decided 2026-11-23), so Doom isn't locked behind luck. After that it stays a Rare drop. ⛔ Which NPC comes with the fixed map.
- [ ] **Shop:** pouch components can't be sold yet. (Buying something that won't fit no longer takes the gold - fixed 2026-11-10, see section 3.)
- [ ] **Chests and the ground keep items the player couldn't carry**, so they can come back for them. ⛔ Needs map objects.
- [ ] Inventory stacking ignores XP (two copies with different XP merge into one).
- [ ] **Dropped weapons come with pre-rolled upgrades** (decided 2026-11-19): a weapon created at level N gets N-1 upgrades on random paths. Armor keeps `UpdateStatsForLevel`.
- [ ] Delete the 15 leftover "champion boss key" quest items in `item_database.cs`.

### Quests
- [ ] **Wire quest progress for the new NPC unlock quests**, once those exist (section 1 Quests, "NPC unlock quests" below) - boss quests are done (section 3).
- [ ] **Main quests unlock in story order**, with a new "locked" quest state that is saved. (Boss quests already follow the boss order since 2026-11-11 - only the next boss's quest is shown, later ones stay `NotDiscovered`, worked out from the boss progress so nothing new is saved. This item is for the other main/story quests.)
- [ ] **Save quest item rewards** (only gold and XP are saved today).
- [ ] **NPC unlock quests** - one side quest per NPC, done in any order. The NPC is fully locked (no buying, selling or upgrading) until it's done. Build: an "unlock this NPC" quest type, a saved unlocked flag per NPC, and `OpenShop`/`OpenBlacksmith` etc. refusing a locked NPC. Completing the magic component seller's quest also unlocks Mage Blast for the player (decided 2026-11-19). Each NPC gives their own quest. **Content (decided 2026-11-24):**
  - **Shop Keeper:** goblins keep raiding his supply cart. Win 3 goblin fights near town. Thank-you: 3 Honey Drops.
  - **Blacksmith:** bandits stole his hammer. Beat a bandit group with a named leader (a regular Bandit with a set name, not a boss). Thank-you: the first weapon or armor upgrade is free.
  - **Magic component seller:** bring her samples - 1 mushroom, 1 bug and 1 lucky charm (any from each group; they are handed over). It teaches the player what components are. Thank-you: Mage Blast.
  - Each also pays normal side-quest XP and gold.
  - All three are early-game quests, so the shops open quickly.
- [ ] **The blacksmith's apprentice - a 4-step questline** (decided 2026-11-24). One step opens after each of bosses #2, #4, #6 and #7, so it runs alongside the story:
  1. Find a rare ore in a cave (a fight).
  2. Bring components for a test enchant.
  3. Escort him to an old forge (a fight on the route).
  4. Retrieve his master's lost notes from a dangerous area.

  Each finished step unlocks a tier of enchanting (next item). Locations come with the fixed map.
- [ ] **Weapon enchanting at the apprentice** (decided 2026-11-24):
  - **One enchant per weapon.** Re-enchanting replaces it.
  - **Cost:** gold (rising with the weapon's rarity) plus 2-3 fitting components (e.g. Flame takes Kellycones and Rubeetles).
  - **Enchants:**
    - Flame: 15% chance to burn
    - Frost: 15% chance to slow
    - Venom: 15% chance to poison
    - Storm: 10% chance to also hit a 2nd enemy
    - Keen: +5% crit
    - True: +5% accuracy
    - Vampiric: heals 10% of the damage dealt
  - **Tiers by questline step:**
    - Step 1: Flame and Keen
    - Step 2: Frost and True
    - Step 3: Venom and Storm
    - Step 4: Vampiric, and every enchant gets stronger (e.g. 15% → 25%). Weapons already enchanted upgrade for free.
  - **Saving:** the enchant is saved on the weapon.
  - Vampiric overlaps the new Lifesteal ability - if both are on, they add up (my default, to retune).
  - Component recipes per enchant and the gold prices are placeholders, to set with the build.
- [ ] **Fold the Veteran Ranger into the quest log** (decided 2026-11-24): boss quests appear on their own when the next boss is revealed (already the rule), and are claimed from the quest log, with no giver to visit. The Ranger can stay as a dialogue-only NPC for lore. Retire the boss-quest part of `quest_giver.cs` (comment it out, as with the other console menus).
- [ ] **Main and side quests** (decided 2026-11-24): one `QuestManager` with a Main/Side category on each quest. Main = boss quests and the Princess's story quests, unlocked in story order. Side = NPC unlock quests, the apprentice line and everything else. The unused `progression/quest_system.cs` goes dormant (kept, like the other parked code).
- [ ] **New NPCs:** the magic component seller (rarer components only, and she crafts the permanent stat potions for a fee - see Items, loot and magic) and the blacksmith's apprentice (weapon enchanting, unlocked through his own quest line). There is no baker: the Shop Keeper sells the healing sweets and the common components.
- [ ] **Move quest text into data the Godot screens can show** (the console prints became `GameLog` messages 2026-11-10; quest screens still need the text as data).

### Game systems
- [ ] 🔴 **Statistics are recorded as things happen** - have systems raise events and the tracker listen. The same events feed quests.
- [ ] **Settings:** remove `ColoredText` and `UseEmojis` (code, save, tests). Hide the RNG-algorithm and auto-save settings in the MVP. Add a "setting changed" signal. Wire each kept setting. **Split global and per-save** (decided 2026-11-24):
  - **Global** (in their own settings file next to the saves, also used on the title screen): audio volume, text speed, display options (turn order, combat log, damage numbers, enemy stats) and confirmations.
  - **Per save:** difficulty (locked at creation) and any gameplay setting.
  - An old save's settings seed the global file the first time it's loaded, if the global file doesn't exist yet. Check `GameSettingsTests` and the save tests when this changes.
- [ ] **Player class system out of the MVP** - no class choice and no class bonuses or armor penalties. Keep the code (`player_class.cs`, the save field) dormant.
- [ ] **Day/night leftovers** (the clock is built - section 3, "Day and night"):
  - **Night spawn lists:** each area uses its night list at night - with "Per-area enemy spawn lists" below. ⛔ Needs the map.
  - **The manual switch only at inns and save campsites:** `GameManager.SwitchDayNight()` works anywhere in the world today. Put it in the rest menu with "`Rest()` becomes an inn-based mechanic" below. ⛔ Needs the map.
  - **Godot:** call `GameManager.AdvanceClock(delta)` every frame, tint the map darker at night (`DayPhaseChanged`), and show `LastRefusalReason` when a shop or the blacksmith is closed.
- [ ] **`Rest()` becomes an inn-based mechanic** - the same rest menu as the day/night switch (decided 2026-11-23). Inns heal fully; save campsites switch day/night and save, but don't heal.
- [ ] **Save points:** only inns and marked campsites allow saving. Add the check in `SaveGame` only, so the boss auto-save still works. ⛔ Needs the map.
- [ ] **Gender selection at character creation** - cosmetic only.
- [ ] **Equipment levelling, connected:** gear XP (above), a placed Blacksmith, and the three-way weapon upgrade choice wired in (today `CompleteUpgrade()` always takes "Balanced"). (The typed Y/N and the one-second pause are out of `UpgradeWeapon`/`UpgradeArmor` - 2026-11-10.)
- [ ] **Place shops, the blacksmith and NPCs in towns** - `city.cs` becomes the home for in-town places. ⛔ Needs the fixed map.
- [ ] **Elder of the Elven Village** (NPC). ⛔ Needs the fixed map.
- [ ] **Army camp leftovers** (the wave challenge is built - section 3, "The army camp challenge"):
  - **The camp itself:** a place on the fixed map with soldiers to talk to and a commander who starts the challenge (`GameManager.StartArmyCampChallenge` works anywhere in the world today). ⛔ Needs the fixed map.
  - **Free spells:** `FightRules.ComponentsAreFree` is set for the waves, but nothing reads it yet. The spell build must check it, including the "greyed out without components" check. Casters still only cast spells they know; cooldowns, the to-hit roll and Fiery's wild magic work as normal.
  - **Godot:** a lineup picker before the attempt, the between-waves screen (pot, Next wave, Leave), Surrender in place of the Flee button, and the payout screen.
- [ ] **Per-area enemy spawn lists**, replacing the fixed 70% Goblin / 25% Bandit / 5% Dragon roll. The Dragon only appears in one or two mountain areas; loyal army soldiers are never random encounters (only deserters are). ⛔ Needs the map (the roster is final in `enemy_roster.md`, with a day/night lean per family).
- [ ] **Encounter-size roll** (max 8 enemies per fight, decided 2026-11-23): each enemy has a group range (in `enemy_roster.md`, e.g. wolves 2-5, Dragon always 1). The maximum goes up by 1 every 10 player levels, and by +1 on Difficult / +2 on Unfair. The touched enemy's family fills the group, and about 1 in 4 fights adds one or two others from the area's spawn list. The group ranges are final (roster reviewed 2026-11-24). ⛔ The mixing needs the spawn lists.

### Code cleanup
- [ ] Delete the duplicate settings/statistics save code in `QuestSerializationHelper` (about 190 lines).
- [ ] **Keep `turn_manager.cs` and `TurnManagerTests`** (decided 2026-11-05). Combat runs its own turn queue today, but the class may be reworked to fit the new combat engine and the 8-slot rounds instead. Check that when the turn-order code is next touched.
- [ ] Rename one of the two `CombatAction` types.
- [ ] **Merge the 9 weapon-mastery files into 4 family masteries** in one class (decided 2026-11-19): Blades (sword, dagger), Heavy (axe, mace, spear), Ranged (bow, crossbow), Arcane (staff, wand). Fix the `\\n` typo in their text. Old saves with a single-weapon mastery need mapping to its family.
- [ ] Merge the duplicate effects (Regeneration twice, Bleed/Bleeding).
- [ ] Remove dead leftovers: `GetAvailableBosses` (`DisplayBossSelectionMenu` is commented out already), `GenerateMapFromSeed`, `ShowCombatStatus`, the enemies' unused `DecideAction()`, `BossEnemy.DecideAction()`, `BossDefinitions.GetBoss()`, `[Obsolete] LevelUp()` on weapon/armor, the unused `NPC_base.AttackTarget()`.
- [ ] `quest_manager.cs` `SetQuestState()` uses reflection on a public property.
- [ ] Check the build for a flood of nullable warnings in `game_logic`. If there is one, switch the check off or clean up folder by folder.
- [ ] Decide `event_system.cs`: delete it, or make it the one central non-combat event handler (needs an in-depth review first).

### Godot frontend
- [ ] **Combat scene receives its fight from `GameManager`** instead of building its own, and goes through the tutorial check during the tutorial.
- [ ] **Combat scene plays the events back in order** (decided 2026-11-23): the backend runs every computer-controlled turn in one go, and the screen queues the events and animates them one after another. It only shows the player's turn once the queue is empty.
- [ ] **Combat scene shows:** names (enemy name + type), damage numbers, "Critical", "Miss"/evade, health bars that move on each hit, whose turn it is, turn order and round (8 slots, skipped ones shown), all 5 action buttons (greyed when unusable; in the army camp waves the Flee button becomes Surrender), target and item picking, defend markers, status effects with time left, the companion control toggle, downed/revived combatants, an end screen per outcome with rewards, the boss name/title and attack warnings, the text log. Also every ability (`GetAbilities()` for Felicity), Boon's bolt picker (`GetBoltCounts()`), and Risk's blocked hits (`AttackBlocked`).
- [ ] **Screens:** party menu (player, inventory and companions in one, with an equip action that moves items in and out of the bag, plus use, drop and the control toggle), shop, blacksmith, quest giver, quest log, settings, statistics, main menu/new game/load, character creation, pause, save, loot pop-up, Game Over (offer a new game when there's no save), ending and credits.
- [ ] **World map overview** (decided 2026-11-24): a view-only screen in the pause menu showing the route/town graph. Visited places are named, unvisited ones show "???". No fast travel in the MVP. Needs the backend to track visited map nodes (saved). ⛔ Needs the fixed map.
- [ ] **Completed saves get a "Completed" marker** on the load screen, plus a statistics summary (decided 2026-11-24 - `IsGameCompleted` already exists). Nothing else unlocks in the MVP.
- [ ] **Pixel-art project settings** (decided 2026-11-24): a low base resolution (e.g. 640x360), scaled up in whole steps, with nearest-neighbour filtering, set in `project.godot`.
- [ ] Set the project's main scene once a main menu exists.
- [ ] Rewrite `MapController.cs` for the touch-to-fight overworld spawns.
- [ ] The combat log treats `[...]` as formatting. Fix it when combat text moves to events.

### Unit tests
_The backend tests were reworked against the current code on 2026-11-04, and they pass._
- [ ] `QuestManagerTests` - ⛔ switched off until the quest changes above (NPC unlock quests, story-order unlocking, rewards) are built, then rewrite it against them.
- [ ] New tests: bosses, shop, weapon/armor upgrades.
- [ ] Frontend (Godot) tests.
- [ ] A CI pipeline to run the tests.

---

# 2. Needs creative input

### Story and world
- [ ] 🔴 **The fixed map** - routes and towns (a route/town graph with bounded areas). Town and route names, where each boss lives, which campsites are save points, and where each NPC lives. This blocks placing anything in the world.
  - **Waiting on the user's drawing** (decided 2026-11-24). Rules already set: a main path with optional side areas branching off. Bosses live in their own locations, not in towns. Every visited area stays reachable on foot (no one-way cut-offs), and new areas open as bosses fall. The size and each boss's spot come with the drawing.
  - **The drawing should mark:** town and route names; how they connect; each boss's location; the side areas (including the one or two Dragon mountains); the campsites that are save points; where the army camp, the royal palace and the Elven Village are; and, if known, which town has which shops, spell NPC and quest NPCs. The spawn lists, component favour per area and day/night lists can be drafted from it afterwards.
- [ ] **Ending and credits** - the story content and credits text. (A completed save only gets a load-screen marker - decided 2026-11-24.)
- [ ] **Dialogue lines** for NPCs and companions.
- [ ] **Difficulty flavour text** - the placeholder lines in `GetDifficultyDescription` are for you to reword.

### Bosses
_All the boss mechanics and numbers are settled (2026-11-19 - see `done.md`). What's left is names, text and places._
- [ ] **Librarian-Monk:** his real name, title and description (placeholders: "Librarian-Monk", "Keeper of the Library"), and where he is fought.
- [ ] **Centipede:** his real name, title and description (placeholders: "Giant House Centipede", "the Many-Legged"), and where he is fought.
- [ ] **Cyclops Brothers:** their real names and text (placeholders: "Fast", the faster one, and "Slow"), and where they are fought. Later: a combined attack with its own animation.
- [ ] **Demon Prince:** his real title and description (placeholder: "Prince of Chaos"), and where he is fought. Real names for his new conditions if the placeholders don't stick.
- [ ] **Slime Dragon:** built 2026-11-11 with my defaults (agreed for now) - 1 dragling per hit, at most 6 alive, the end-of-round turn tops them back up to 3 (exactly 3 alive = no bonus turn; counted when the round ends). Draglings fight at the party's level (his level - 5) with my stats (level 21: 120 HP, 8-17 damage, speed 21). Retune after testing. Also his real title and description (placeholders: "the Ever-Dripping"), and where he is fought.
- [ ] **Captain (Risk as a boss):** built 2026-11-11 with my defaults (agreed for now) - crit chance Normal 50%, Hard 55%, Difficult 60%, Unfair 65%, at 2.5x on every difficulty. Still open: write the story moment (cutscene or quest step) between boss #7 and #8 where he leaves (the why, the timing and his gear are decided - 🤫 see `done.md`).
- [ ] **Princess:** her real name, title and description (placeholders: "The Princess", "Heir to the Throne"). Her mechanics are settled (see `done.md`).

### Companions
_All the companion mechanics are settled (2026-11-19 - see `done.md`), and Fiery's starting spells too (2026-11-20). What's left:_
- [ ] **Names and text for the writing pass:** Boon's combat barks; a new name for "Royal Guard's Armor" 🤫; a real name for Fluke's "Lucky Aura".

### Items and magic
- [ ] **Item names for the writing pass:** give the kept Legendaries (and any other working names) original names that fit this world - Excalibur, Gae Bolg, Failnaught, Ragnarok's Edge, Reality Warp Wand, Ballista Prime, Cosmos Staff, The Peacemaker and so on. The keep-list itself is final (2026-11-24).
- [ ] **Spell NPCs: sell or give?** With the fixed map, decide whether each town's spell NPC sells the scroll or gives it. Their offers are random, so they need no list. (The spell list and all recipes are final - 2026-11-24.)
- [ ] **Component descriptions** for the writing pass - all 41 (with the new Phoenix Ember) need one - today they say only "A mushroom.", "A bug." and so on. (Rarity, price, who sells them, groups and drops are decided 2026-11-23 - see `done.md`.)

### Quests
- [ ] **Names for the writing pass:** the bandit leader in the Blacksmith's quest, the apprentice, his master, and the quest titles.
- [ ] **Rewrite the quest text** for the current story (drop the "greatest warrior in the realm" framing).

### World and systems
- [ ] **A name for "Kingdom B"** (the enemy kingdom). Its soldiers and archers show as "Name the Kingdom B Soldier" until then (`enemy_roster.md`).

### Godot
- [ ] **Combat scene layout** and a coordinate grid (x/y) for placing things on screen.
- [ ] **One scene per area, or one per area type filled from data?** (the last open question in section 6 of `godot_frontend_plan.md` - the others were decided 2026-11-24). Decide it with the fixed map.

---

# 3. Built, needs testing

### Companions
- [ ] **Chance** - the tutorial gives you Chance; his hits land anywhere from his level to 100 (crits can go higher); the bottom rises as he levels; no extra level bonus on top.
- [ ] **Risk** - about 15% of hits on him do no damage (about 30% while defending); 0 without armor; a blocked hit still applies its effect (e.g. Skarn's grapple); the chance rises on even levels and caps at 70%; magic attacks (`IsMagical`) are never blocked.
- [ ] **Boon** - recruiting him adds 5 Piercing + 5 Pinning bolts; each special shot uses one; Piercing ignores armor; Pinning slows; choosing an empty type throws an axe; he almost never misses with his own crossbow on any difficulty; bolts survive save/load; the auto rules pick sensible bolts.
- [ ] **Felicity** - heals only when someone is under 50%; heals the most wounded (herself too); heals every other round at most; the party heal appears at level 20 and hits everyone; both cooldowns tick.
- [ ] **Companions can be created** (their starting gear names now exist in the item database).
- [ ] **Roster and bench** - recruit 4 and the 4th is benched; bench, field and swap work and refuse bad moves; Risk's max-health bonus comes off when benched and back on when fielded; save/load keeps the roster, marks, levels, XP, health, gear, ability levels and Risk's bonus.
- [ ] **XP sharing** - a won fight gives fighters full XP, and downed and benched companions half; a late recruit joins at the player's level minus 1.

### Combat
- [ ] **Endings** - win, loss, party fled and opponent fled each end correctly; fleeing no longer leads to Game Over; enemies can flee on their own; a mixed ending pays 100% per beaten enemy and 50% per fled one; no fleeing from bosses.
- [ ] **Defend** lasts until the defender's next turn and halves every hit in that window. (Since 2026-11-24 the halving is done inside `DamageCalculator.CalculateAttack` - `targetDefending` - so it counts towards the 75% cap below.)
- [ ] **Armor is a % reduction** (built 2026-11-24): a party member's armor cuts each hit by defense / (defense + 100) (`DamageCalculator.GetArmorReduction`). Armor, Defense Boost and Defend multiply together and cut at most 75% (`MaxDamageReduction` - Bulwark joins it when built). The consumables' flat Resistance comes after the %. A piercing bolt skips armor, Defense Boost and Resistance, but Defend still halves it. Enemy defense stays flat, then Defend halves it. A hit always does at least 1. Check in play: Common plate (25) takes about 20% off; full defense + Defend never goes past 75%. Not built yet: the Iron Draught's +2 defense (it adds to the armor number before the %, with the stat potions). Tests: the armor test now expects 18-19 from a 20 hit; new tests for the formula and the cap.
- [ ] **Fractions round up by chance** (built 2026-11-24): `RNGManager.RoundByChance` - the leftover fraction is the chance to round up (5.25 → 6 a quarter of the time); a whole number uses no roll. Used for every % on damage in `DamageCalculator` (attack boost, rage, crits, Rallying Cry, passive abilities, armor and Defense Boost, Defend), the boss charge multiplier in `ProcessAttack`, `CalculateDamage` and `CalculateHealing`. Left as they were: the difficulty stat scaling (already rounds up), the Librarian-Monk's shield (rounds down, as decided), reward shares, and accuracy/crit chance numbers. New tests for the helper.
- [ ] **Formation, rows and Reposition** (built 2026-11-25 from the rules decided 2026-11-23 - `combat/formation.cs`):
  - **Slots:** 4 slots, 0-1 front and 2-3 back (`Entity.FormationSlot`, saved for the player and each companion). The party menu sets them with `PartyManager.SetFormationSlot`, which swaps with whoever is there. A recruit fills the first empty slot, ranged users at the back first. Benching clears the slot, and a swap gives the newcomer the old slot. Old saves load with no slots and are placed (`FixFormation`).
  - **Fights:** each fight copies the formation, and anyone without a slot is placed the same way. Reposition changes only the fight's copy.
  - **Targeting:** an enemy's random pick (the 30%, and Random enemies) counts the front row twice and the back row once. Pattern picks ignore rows. When everyone is in the same row, it's the old even roll, so those fights use the same random numbers.
  - **Hit penalty:** a plain attack on anyone behind their front row gets -10 hit chance (`CalculateAttack`'s new `hitChanceModifier`, the hit chance never below 1). This counts for both sides, so it covers archers and imps too.
  - **Melee cut:** a party member's melee attack from behind the front row does 70% damage (spear 90%, ranged 100%). It's applied after defense with the boss multiplier, rounded by chance, and the log says so. Ranged means bow, crossbow, staff or wand. Unarmed and cat-form Chance are melee.
  - **Reposition:** `CombatAction.Reposition(actor, slot)`, party only, and it costs only the mover's turn. The member in that slot, standing or down, swaps into the mover's old slot. Raises `Repositioned`. It's added to the tutorial's "try each action" list.
  - **My defaults:**
    - A back row with nobody standing in front counts as the front, for the -10 and the melee cut too, not just for targeting.
    - Bosses ignore rows completely: always front, no 2:1 in their random picks, and no -10 on their attacks.
    - Back-row enemies (archers, imps) count as ranged, so enemies never get the melee cut.
    - Abilities (Boon's bolts, Power Strike and so on) ignore rows, as spells, heals and items do.
    - The computer never repositions.
  - **Check in play:** the mage and Felicity start at the back with their staves, so enemies hit them less often.
  - **Tests:** the new `FormationTests`. No existing test changed. Fights with a back-row member now roll differently, but no existing test has one standing. `ActionType.Reposition` and `CombatEventType.Repositioned` were added at the end of their lists.
- [ ] **Everyone down at a win gets up at 1 HP** (built 2026-11-24): the player and every downed companion, after the XP is handed out, so a downed companion still gets half.
- [ ] **Nothing flees in a boss fight** (built 2026-11-24): a Cautious enemy never picks Flee while a boss is in the fight, and `ProcessFlee` refuses any enemy flee there ("stands their ground!", the turn is spent). The party's flee was already refused.
- [ ] **Difficulty numbers** (decided 2026-11-23, built 2026-11-24). Hard is the 1x baseline.

  | | Normal | Hard | Difficult | Unfair |
  |---|---|---|---|---|
  | Enemy HP | 75% | 100% | 175% | 275% |
  | Enemy damage | 75% | 100% | 150% | 225% |
  | Enemy accuracy | +0 | +5 | +10 | +15 (never above 95%) |
  | Enemy crit | +0 | +0 | +5% | +10% |
  | Enemy speed | +0 | +0 | +2 | +4 |
  | Enemy defense | +0 | +0 | +20% | +40% |
  | Party hit chance | -0 | -5 | -10 | -15 |
  | Party Max HP | 120% | 100% | 90% | 80% |
  | XP | 80% | 100% | 130% | 160% |
  | Gold | 100% | 100% | 125% | 150% |
  | Loot quality | 0.8x | 1x | 1.3x | 1.6x (unchanged) |

  - **Enemies:** `DifficultyScaler` now has separate health and damage multipliers. `ApplyDifficultyToEnemy` also applies the accuracy (an enemy already above 95% keeps its number), crit, speed and defense (rounded up), once per enemy - bosses and mid-fight joiners too. The Librarian-Monk's shield follows the health multiplier. The Captain keeps his own crit table (set after the difficulty, so his number wins), and the Centipede's and the Princess's per-difficulty numbers are unchanged.
  - **Party Max HP:** `Entity.MaxHealthMultiplier`. Applied to the player on a new game (`GameStartup.CreateNewGame`), copied from the player when a companion is recruited (`PartyManager.RecruitCompanion`, before the catch-up levels), and restored without rescaling on load (the saved max health already has it). Every level-up gain is scaled by it. Flat bonuses aren't scaled: Risk's max-health bonus, and the Vigor Draught's +10 when it's built (my default). Saves made before this have unscaled max health; only their future level-ups are scaled.
  - **Unfair bad luck:** `DamageCalculator.RollWithLuck` - on Unfair, 25% of the party's attack hit, crit and damage rolls are rolled twice and the worse kept. It takes a good-luck chance too, for Fluke's aura (`GetGoodLuckChance`, 0 until he's built). If both trigger on one roll they cancel (my default). Spells don't use it yet.
  - **The tutorial is capped at Hard:** `DifficultyScaler.GetTutorialDifficulty`. `GameManager.BeginEagleBearEncounter` sets it for the Skarn fight and puts the save's difficulty back when the fight ends. The party's max health stays the save's own.
  - **Difficulty-select text** (`GameStartup.GetDifficultyDescription`) is built from the new numbers. Its flavour lines are still placeholders (section 2).
  - **Check in play:** Skarn on Unfair (Hard numbers); a new Normal game starts at 120 HP; a companion recruited on Unfair joins at 80%; save, load and level up keep the %.
  - **Tests:** updated `DifficultyScalerTests`, `BossStatsTests`, `LibrarianMonkTests` and `LevelingSystemTests` to the new numbers; new `DifficultyPartyTests` (party side), plus enemy-bonus, accuracy-cap, defense and tutorial-cap tests.
- [ ] **The regular enemies rebalanced** (built 2026-11-24 from `enemy_roster.md`). Every regular enemy is `StandardEnemy` times its `EnemyProfile` (`EnemyRoster`): health 20 + 6/level, damage (2 + level/2)-(5 + level), defense level/4, speed 10 + level/4, 75% accuracy, 5% crit. Health and damage round to nearest, defense rounds down.
  - **New:** Wolf, Slime, Big Slime, Spider, Giant Beetle, Fungusoar, Imp, Deserter, Army Soldier (the roster's "Deserter / Army Soldier" row, split because only the deserter flees), Kingdom B Soldier, Kingdom B Archer. **Retuned:** Goblin, Bandit, Dragon (keeps the Boss type for boss loot; it isn't a boss fight), Castle Guard (damage up to the standard). The Dragling keeps its built stats.
  - **Flee lines:** below its `FleeBelowHealth` an enemy tries to flee - goblins and imps every turn, the rest 30% of their turns (`FleeAttemptChance`) - then `FleeChance` decides. Never in a boss fight. Wild draglings can now flee (below 30%); the Slime Dragon's can't. The old per-class `DecideAction` flee code (never used by combat) is gone.
  - **Targeting:** each enemy follows its pattern 70% of the time, otherwise random (`CombatManager.SelectEnemyTarget`): Random, Most wounded, Weakest defense (least armor), Last attacker, Biggest threat (most damage done this fight). With no answer yet (nobody has hit it) the pick is random. Bosses keep Most wounded, so they behave as before.
  - **Hit effects** (`EnemyBase.RollOnHitEffect`, rolled when it attacks, lands only on a hit): wolf Bleeding 15%, slime Slowed (-2 speed, 2 turns - new `SlowedEffect`) 20%, spider poison 20%, Fungusoar Spore Roar poison 25%, imp weak Burning 20%, Dragon fire breath Burning 30%. Strengths are my defaults (the poison formula, and half or all of its middle for burn/bleed).
  - **Big Slime split:** when beaten it brings in 2 Slimes at its level (`GetSpawnsOnDefeat`), like a summon (8-enemy cap, they act next round). All slimes share the "Slime" diminishing-rewards group.
  - **Behaviour:** deserters and both Kingdom B soldiers are `Defensive` (defend more when wounded); the Dragon is `Tactical`; the rest `Aggressive` or `Balanced`.
  - **Not in the fights yet:** the new enemies don't spawn until the per-area spawn lists exist (the overworld still rolls Goblin / Bandit / Dragon). Leftovers are in section 1 ("Enemy roster leftovers").
  - **Check in play:** a level-1 Goblin has 18 HP and runs when low; wolves make you bleed; a Big Slime's split; a beetle shrugging off normal hits.
  - **Tests:** the 2 Goblin tests in `DifficultyScalerTests` use the new Goblin (18 HP); new `EnemyRosterTests` (the sheet's worked numbers, flee lines, targeting, the split in a fight).
- [ ] **Random names for regular enemies** (built 2026-11-25 from the lists in `enemy_roster.md`). Every regular enemy gets a name from its family's list when a fight starts, and so does every enemy that joins mid-fight (split slimes, draglings, the Princess's guards). It shows as "Grik the Goblin" (`EnemyBase.SetGivenName`; `TypeName` keeps "Goblin", `GivenName` "Grik"). No name repeats in one fight; if a family's list runs out (only possible with many joiners), a name not worn by a standing enemy is reused. Bosses keep their own names. The lists are in `EnemyNames`.
  - Names use their own random numbers (`CombatManager.NameRng`), so naming never changes a fight's rolls.
  - Statistics and kill quests (not wired yet) should count by `TypeName`, not `Name`.
  - "Kingdom B" reads badly in "Valka the Kingdom B Soldier" - it changes with the kingdom's name (section 2).
  - **Check in play:** names in the combat log, no repeats in a big fight, fresh names on a retry.
  - **Tests:** new in `EnemyRosterTests` (the name format, every list has 8+ unique names, 8 goblins all different, split slimes get fresh names, the fallback).
- [ ] **Effects and cooldowns count the owner's own turns** (fixed 2026-11-25). Before, every effect and cooldown counted down at the end of each round, including the round it started in, so a 1-turn buff was gone before it could be used and a 3-turn cooldown only blocked 2 turns. Now:
  - An effect is applied and counts down at the end of each of its target's turns (`Entity.ProcessTurnEndEffects`), except the turn it landed or was re-applied in (`AbilityEffect.AppliedThisTurn`). A 3-turn buff cast on yourself lasts your next 3 turns; a 3-turn poison from an enemy hurts at the end of your next 3 turns.
  - A cooldown counts down at the end of each of its owner's turns, except the turn the ability was used in. Attack Boost's 5-turn cooldown blocks exactly the next 5 turns.
  - A boss with 2 turns counts once per round (its first turn), as before. A downed character's effects and cooldowns don't count down until they're back up.
  - The round's end now only does the speed changes and the Iron Will cleanse. The effect lists say "turns" instead of "rounds".
  - **Check in play:** spell buffs (3 turns) and Doom (2) once spells exist; Boon's Pinned; the Cyclops' Rage; poison and burn counts.
  - **Tests (effects):** new `EffectTimingTests`. Seeded fights where a poison or burn lands after the target's turn now tick it once more, so a long seeded fight with damage over time can roll differently - the existing tests found none that depend on it.
- [ ] **One damage path** - companion hits are reduced by enemy defense; enemy hits on companions roll to hit and crit and go through armor; no hit does less than 1 damage.
- [ ] **8 turn slots** - fights with 1, 2, 4, 5 and 8 enemies give the expected slots (extras to the slowest slots, shared speed = average rounded up, empty slots skipped); speeds change between rounds after attacking or defending.
- [ ] **More than one turn slot per round** (written 2026-11-09, for the Princess). A boss whose mechanic implements `IMultiTurnMechanic` gets one opponent slot per turn, each with its own speed. No boss uses it yet, so test it with a small test mechanic (e.g. 2 turns, second turn 5 slower). Check:
  - The boss shows up twice in `TurnOrder`, the second slot with `TurnNumber` 2, and the log reads "Name (2nd turn)".
  - It acts on both turns, and `CurrentTurnNumber` is 1 and then 2.
  - Its second turn never comes before its first, even if it rolls the same or a higher speed.
  - Other enemies share the slots that are left (boss with 2 turns + 3 others: the 2 slowest others share a slot).
  - With too few slots, the extra turns are dropped so every other enemy still gets a slot.
  - `ShouldTakeExtraTurn` returning false skips the turn with a `TurnSkipped` event.
  - A poison on the boss ticks once per round, not once per turn (confirmed 2026-11-23).
  - A Defend from its first turn ends when its second turn comes.
- [ ] **Bosses reacting to events** (written 2026-11-10, for the Cyclops revive and the Slime Dragon). A boss whose mechanic implements `IReactiveMechanic` is told about each event from an action or effect tick once it has been resolved. No boss uses it yet, so test it with a small test mechanic. Check:
  - It hears an `AttackLanded`/`Damaged` when it is hit, and a `Defeated` when an ally falls.
  - Poison ticks and end-of-round effect expiry are heard too.
  - Changes a reaction makes (e.g. healing itself) show up as normal events, and can be reacted to.
  - Two reactions that set each other off stop after 10 passes.
  - A boss that is down doesn't react (confirmed 2026-11-23 - bosses never flee).
  - A reaction doesn't end the boss's Defend or use up its turn.
  - A fight without a reacting boss plays exactly as before.
- [ ] **Lost turns - "skip your next turn"** (written 2026-11-10, for the Princess). A general `SkipTurnEffect`, put on a target by a boss with `CombatAction.Inflict(boss, target, new SkipTurnEffect())`. No boss uses it yet, so test it with a small test mechanic. Check:
  - Put on someone who hasn't acted yet this round, they lose this round's turn; on someone who already has, they lose next round's.
  - The lost turn does nothing (nothing is started, not even a Defend), and the log and a `TurnSkipped` event with Message "Turn lost" show it.
  - A Defend from their previous turn still ends when the lost turn comes.
  - It doesn't wear off at the end of the round, only when used up. Putting a second one on doesn't add a second lost turn.
  - Their poison/burn still ticks at the end of the lost turn (confirmed 2026-11-23).
  - A boss's end-of-round bonus turn doesn't use it up (confirmed 2026-11-23).
  - An antidote or Iron Will removes it (it counts as crowd control).
  - It never carries over to the overworld or the next fight.
  - The player can't submit an Inflict or Summon action (refused with "Only enemies can do that.").
- [ ] **Enemies joining a fight in progress** (written 2026-11-10, for the draglings and the castle guards). A boss calls them with `CombatAction.Summon(boss, enemies)` on its turn, or `context.Summon(enemy)` from a reaction or a bonus turn. Test with a small test mechanic. Check:
  - The new enemy shows in `Enemies`, the log, and an `EnemyJoined` event.
  - It doesn't act in the round it joined, and is in the turn slots from the next round (confirmed 2026-11-23). With more than 4 enemies, the slots are shared as normal.
  - It gives normal rewards when the fight is won (draglings get diminishing rewards - see below).
  - No more than 8 enemies can be in the fight at once (confirmed 2026-11-23). A refused summon logs "No one answers...".
  - The list GameManager passed into `StartCombat` is not changed.
- [ ] **End-of-round bonus turns** (written 2026-11-10, for the Cyclops revive and the Slime Dragon). A boss whose mechanic implements `IEndOfRoundTurnMechanic` is asked once each round, after every slot has been played, whether it wants a bonus turn. Test with a small test mechanic. Check:
  - If not, nothing is added - not shown in `TurnOrder`, not skipped.
  - If yes, a slot with `IsEndOfRoundTurn` is added after the filled slots, an `EndOfRoundTurnAdded` event is raised, and the boss acts before the round ends (before effect countdowns, cooldowns and next round's speeds).
  - It can revive a fallen ally through `context.FallenAllies` (setting Health) - a `Revived` event shows it, and the revived enemy is back in the slots next round.
  - It can summon, or return an action (e.g. an attack) that resolves normally. An attack on a bonus turn isn't joined.
  - It doesn't end the boss's Defend, doesn't tick its effects, and doesn't change its speed for next round (confirmed 2026-11-23). `IsEndOfRoundTurn` is true during it.
  - If the fight is already over when the round ends, no bonus turn happens (e.g. both brothers down in the same round = win).
- [ ] **Diminishing rewards** (written 2026-11-10, for farming draglings). Enemies with the same `DiminishingRewardsGroup` pay less XP and gold the more of them are beaten in one fight. Test with a fight of 15+ enemies sharing a group. Check:
  - The first 10 pay full XP and gold; the 11th pays 80%, the 12th 64%, the 13th about 51% (each 80% of the one before), and the log says so.
  - Item drops are not reduced.
  - Enemies with no group, and other groups, are not affected. The count starts again in every fight.
  - A fled one also takes the next place in line, then gets its usual half (confirmed 2026-11-23).
- [ ] **Joined attacks** (written 2026-11-10, for the Cyclops Brothers). A boss whose mechanic implements `IAttackPartnerMechanic` is asked after every enemy attack whether it joins in. Test with two bosses whose test mechanic joins each other. Check:
  - When one attacks on his own turn, the other attacks the same target straight after (an `AttackJoined` event, then the attack's own hit/miss events).
  - Abilities, Defend and other actions aren't joined. Party attacks never are.
  - The joiner's Defend stays up, his own slot still comes up this round, and his speed next round isn't changed by the joined attack.
  - If the first attack took the target down, the joiner doesn't attack (confirmed 2026-11-23).
- [ ] **Actions are checked** - a bad action (wrong turn, dead target, ability on cooldown, missing item, boss flee) is refused with an `ActionRejected` event.
- [ ] **Direct control** - combat waits for you on a directly controlled companion's turn.
- [ ] **Items in combat** - anyone can use an item on any standing party member. A revive works on a fallen companion or the fallen player, the revived member is slower the next round, and a revive can't target someone standing.
- [ ] **The fight goes on** while anyone in the party stands; if the party wins while the player is down, the player gets up at 1 HP. (Extended to downed companions 2026-11-23 - see section 1, Combat.)
- [ ] **Bosses** have stats (Skarn about 160 HP, 16-30 damage - probably too strong, note it); the old 15%/50% multipliers are off; a beaten boss can't be challenged again; boss fights include the party.
- [ ] **Stronger player and party** (2026-11-10, first pass - to be tested and adjusted). Check:
  - A new player starts at 100 HP and speed 10, with no class bonus (the Warrior +20% that made 24 is commented out - classes are out of the MVP).
  - Each level adds 10 + level/5 HP, the same as the companions (it was +1-6).
  - The player's and every weapon-using companion's hits are doubled (weapon roll + level bonus, ×2 - `DamageCalculator.PartyDamageScale`), unarmed too. Chance's cat-form hit (his level to 100) is not.
  - Test changed: `CalculatePlayerAttackDamage_DamageRange_WithinExpectedBounds` now expects the ×2 range.
- [ ] **#7 Slime Dragon - spawns draglings** (built 2026-11-11). Boss #7 in `BossDefinitions` (`SlimeDragonMechanic`), plus a new regular enemy, `Dragling`. Check:
  - Every attack that lands on him brings in a dragling (from the next round), up to 6 alive.
  - When a round ends with 0-2 draglings alive, he takes a bonus turn and spawns enough to make 3. With 3 or more alive, no bonus turn.
  - The fight never goes over 8 enemies.
  - Beating him stops the spawning, and the fight goes on until every dragling is down.
  - Draglings are at the party's level (his - 5), fast and weak; after 10 in one fight, each one's XP and gold drop by 20% (compounding).
  - New tests: `SlimeDragonAndCaptainTests`. No existing test changed.
- [ ] **#9 Princess - the final boss** (built 2026-11-11, standalone for now). 🤫 Last in `BossDefinitions` (`FinalBossMechanic`), plus a new regular enemy, `CastleGuard`. Every boss now exists. Check:
  - Two opponent slots a round; her second is slower by the difficulty's gap (confirmed 2026-11-23: Normal 6-8 ... Unfair 1-2). With only one party member standing, her second turn is skipped (`TurnSkipped`).
  - Each turn: attack a random target, force a skip (the target loses their next turn - `SkipTurnEffect`), or call two castle guards (only when none of hers are alive; uses her turn). At half health or below she leans on skips.
  - Her two actions in a round never go to the same person.
  - A lost turn does nothing at all; a Defend still ends on time (the engine's lost-turn rules).
  - Castle guards are at the party's level, with normal rewards.
  - Her quest (the final quest) only appears once she is the next boss, and nothing before that names her.
  - The final-battle path (`ChallengeFinalBoss`) starts her fight.
  - New tests: `FinalBossTests`. No existing test changed.
- [ ] **Only the next boss is shown** (decided and built 2026-11-11). Fixes both spoilers (the final quest and "Defeat Risk" showing at game start). Check:
  - A new game shows only Skarn's boss quest. Every later boss quest, and the final quest, is hidden (`NotDiscovered`) - not even the boss's name shows anywhere.
  - Beating a boss (the tutorial fight, or any boss fight) shows the next boss's quest (`QuestManager.UpdateBossQuestVisibility`, also run after loading a save).
  - The final quest only appears once the final boss is the next boss.
  - A save from before this rule: later boss quests that were shown but never accepted are hidden again; an accepted one is kept.
  - The boss list (`BossManager.GetBossList()`, for the Godot screen): the next boss shows its name, title, description and mechanic; every boss after it is just "Locked" (all its fields are empty); beaten bosses show their name and title (confirmed 2026-11-23 - they've been met).
  - Godot screens must never list `NotDiscovered` quests.
  - Removed: `BossDefinitions.GetBossListSummary()` (commented out - it listed every boss by name; nothing used it).
  - New tests: `BossVisibilityTests`. No existing test changed.
- [ ] **#8 Captain of the Royal Guard - Risk as a boss** (built 2026-11-11, standalone for now). Boss #8 in `BossDefinitions` as "Risk, Captain of the Royal Guard" (`CaptainMechanic`). Check:
  - His crit chance is 50% / 55% / 60% / 65% (Normal / Hard / Difficult / Unfair), set when the fight starts, and every crit is 2.5x.
  - He is the fastest boss so far (speed 24 at level 32) with low base damage (21-37 at level 32, Hard) - his average hit is about 1.8x that with the crits.
  - Nothing in his fight text says why he left the party.
  - His quest stays hidden until he is the next boss (see "Only the next boss is shown").
  - New tests: in `SlimeDragonAndCaptainTests`.
- [ ] **#6 Demon Prince - a random condition on every hit** (built 2026-11-11, existing conditions only). He is boss #6 in `BossDefinitions` (`DemonPrinceMechanic`). Check:
  - Every one of his turns is an attack, and every hit puts one of Poisoned, Burning, Bleeding, Grappled or Pinned on the target, each about 1 in 5. Nothing on a miss.
  - A hit that does 0 damage still lands its condition (new `CombatAction.EffectEvenWithoutDamage`). Every other attack still needs to do damage for its effect to land.
  - Several conditions can sit on one target at once; a repeat stacks (damage over time) or refreshes (slows).
  - Strength from his level: at 21, poison 7-14 a turn, burn and bleed 10 a turn for 3 turns, grapple and pin -6 speed for 2 turns.
  - His low damage (4-8 at level 1 - the sheet) feels balanced against the conditions.
  - He gets a normal boss quest (only the final boss, last in the list, gets the final quest).
  - New tests: `DemonPrinceTests`. No existing test changed.
- [ ] **#4/#5 Cyclops Brothers - always attack together** (built 2026-11-11). One entry in `BossDefinitions`: "Fast" (placeholder name, speed 9) is the boss, "Slow" (speed 7) is his `PartnerBoss`, and `BossEncounter.StartBossEncounter` brings in both. Each has a `CyclopsBrotherMechanic`. Check:
  - Only the two of them in the fight. Fast always acts before Slow.
  - Whenever one attacks on his own turn, the other attacks the same target straight after (`AttackJoined`). Defend and abilities aren't joined. Joining doesn't end his Defend or use up his own turn.
  - When one falls, the other enrages (Rage effect at his level - confirmed 2026-11-23) and, in a bonus turn at the end of the round, brings his brother back at 25% health (`Revived`). No limit on revives.
  - Both down in the same round wins the fight. It counts as one boss beaten (`DefeatBoss` only for Fast - the partner isn't in the boss list), and pays both brothers' XP and gold.
  - New tests: `CyclopsBrothersTests`. No existing test changed.
- [ ] **#3 Giant House Centipede - poison bite** (built 2026-11-11). He is boss #3 in `BossDefinitions` (`GiantCentipedeMechanic`, `CentipedePoisonEffect`). Check:
  - Every one of his turns is a bite. The poison chance is rolled fresh each bite from the band - Normal 25-35%, Hard 35-45%, Difficult 45-55%, Unfair 55-65% - and only lands if the bite hits.
  - The poison lasts a rolled 2-3 / 3-4 / 3-5 / 4-6 turns (Normal / Hard / Difficult / Unfair), and its damage per turn comes from his level (the existing poison formula - 5-10 at level 12).
  - A second bite on someone he's already poisoned adds a stack and only the band's minimum turns (Hard: +3), not a full new roll. Each stack adds the damage again (2 stacks = 10-20 at level 12).
  - He goes for party members who aren't poisoned about 3.5x as often, but still bites the poisoned ones sometimes.
  - Other poisons (`PoisonedEffect`) and every other effect keep the old restack rule (a stack, back to full time) - the rule moved into `AbilityEffect.OnReapplied` without changing it.
  - New tests: `GiantCentipedeTests`. No existing test changed.
- [ ] **#2 Librarian-Monk - shield** (built 2026-11-11). He is boss #2 in `BossDefinitions` (`LibrarianMonkMechanic`, `ShieldEffect`). Check:
  - The fight starts with his shield up (log line and `EffectApplied "Shield"` before round 1 - the pre-fight cutscene shows him raising it).
  - Every hit on him while it's up is halved, rounded down; the shield takes the rest from its own health (`ShieldBlocked` event) and shatters at 0 (`EffectRemoved "Shield"`). A hit bigger than what the shield has left only has that much blocked. Poison and other damage over time aren't blocked.
  - Shield health: 20 at level 1, grown like boss health, then the difficulty (Hard at level 8: 102; Normal 77, Unfair 204). Every raise is a full shield.
  - Cooldown: 3 turns from a raise. The pre-fight raise counts as his turn 1, so turn 4 is his first chance to raise it again. When the cooldown is over and the shield is down, he raises it 40% / 50% / 60% / 70% of the time (Normal / Hard / Difficult / Unfair), which uses his turn; otherwise he attacks.
  - New tests: `LibrarianMonkTests`. No existing test changed.
- [ ] **Hand-set boss stats** (approved 2026-11-10, built 2026-11-11). Every boss's level-1 stats are in `BossStatSheet` (`boss_stat_sheet.cs`), keyed by boss id - all 9, though only Skarn exists in `BossDefinitions` so far; the others pick up their row when they are built. `BossEnemy.InitializeStats` grows them to the fight level with `DifficultyScaler.BossHealthAtLevel`/`BossDamageAtLevel`/`BossStatAtLevel` (health x(1 + 10% a level) x3, damage x(1 + 8% a level) x1.5, defense and speed + growth a level), and the difficulty is applied once on top. A boss with no row falls back to the old shared formula. Check:
  - Skarn in the tutorial on Hard: 231 HP, 15-28 damage, defense 1, speed 12, 72% accuracy. Does the fight last about 5 rounds, and does his stoop (2.5x, 38-70) make Defend worth learning?
  - The same fight on Normal (174 HP) and Unfair (462 HP, 30-56 damage).
  - Losing and retrying doesn't make him stronger.
  - New tests: `BossStatsTests` (growth formulas, the sheet, Skarn's numbers, difficulty applied once). No existing test changed.
- [ ] **Boss level follows the party** (2026-11-10). When a boss fight starts, the boss is rebuilt at the fighting party's plain average level (rounded up) + 5, capped at 45 (`LevelCaps.MaxBossLevel`); Skarn keeps his fixed level (`HasFixedLevel`). Difficulty is applied on top. Check:
  - A level-10 player with companions at 8 and 9 faces a level-14 boss (average 9, rounded up, + 5); benched companions don't count.
  - A max-level party faces a level-45 boss, never higher.
  - Skarn is level 2 in the tutorial whatever the party's level.
  - Losing to a boss and trying again: the boss comes back at full health, at the current party's level, with no effects left on it, and the difficulty isn't applied twice.
  - Poison and rage keep climbing past level 40 for bosses up to 45 (poison 13-26 per turn, rage +87% damage at 45).
  - Test changed: `GetEffectiveEnemyLevel_CapsAt100` is now `..._Boss_CapsAt45`; new `GetBossLevel_IsPartyLevelPlusFive_CappedAt45`.
- [ ] **One difficulty list** (2026-11-10) - `DifficultyScaler` and the XP multiplier now use `DifficultyLevel` (Normal / Hard / Difficult / Unfair) instead of their own lists; numbers unchanged, Hard is the 1x baseline. The party level is a plain average now (the weighted one is commented out). Check: it builds; `DifficultyScalerTests` and `LevelingSystemTests` pass (updated: names, two party-level answers, the "Fair Fight" danger label - the general danger labels Manageable / Fair Fight / Tough / Dangerous are placeholders to reword).
- [ ] **No console output anywhere** (2026-11-10). Every `Console.WriteLine`/`Write`/`ReadKey`/`ReadLine`/`Clear` in the repo is gone from live code. Check:
  - It builds. Nothing in `game_logic` prints; messages outside a fight come through `GameLog.MessageLogged` (nothing subscribes yet - an autoload can show them as pop-ups), and fight text through `CombatMessage`.
  - Commented out, with a note on each (nothing called them; the Godot screens replace them): the three `menus/` files (whole file), the console menus in `shop_keeper.cs`, `blacksmith.cs` and `quest_giver.cs`, `Weapon.GetPlayerUpgradeChoice`, `PlayerInventory.DisplayInventory`, `QuestManager.DisplayQuestLog`/`DisplayActiveQuestTracker`, `Quest.DisplayQuestInfo`, `QuestReward.DisplayRewards`, `MapManager.DisplayMap`, `BossEncounter.DisplayBossSelectionMenu`, `HandleGameOver`'s menu, and the final-boss intro and victory printout (the victory hook could be left behind - that cleanup item is resolved).
  - `ShopKeeper.BuyItem`/`SellItem`, `Blacksmith.UpgradeWeapon`/`UpgradeArmor` and `GameManager.Rest` no longer wait for a key press or a typed Y/N, and the upgrade's one-second pause is gone - the screen confirms before calling them.
  - Buying an item that doesn't fit (full bag, or a stack over 99) is refused and takes no gold.
  - The combat log doesn't repeat damage or healing lines (`Player.Heal`/`TakeDamage` messages are commented out).
  - `CombatManagerTests.CalculateXPReward_BonusOccursAtExpectedRate` now counts "Bonus XP!" from `CombatMessage` instead of capturing the console (the console-capture helpers were removed).
- [ ] **Difficulty changes the game** (wired 2026-11-10 - the numbers below are the old placeholders; the real ones were built 2026-11-24, see "Difficulty numbers" above in this section). `CombatManager` builds a `DifficultyScaler` whenever its `Difficulty` is set (GameManager sets it on a new game and on load); `GameManager.Difficulty` exposes it. Check:
  - Every enemy's health and damage is multiplied for the difficulty when a fight starts (Normal 75%, Hard 100%, Difficult 150%, Unfair 200%) - including bosses, the tutorial's Skarn, and enemies who join mid-fight.
  - A boss fought again after a loss is not made stronger again (applied once per enemy).
  - The fight's XP and gold get the difficulty multiplier (shown in the log), and companions share the multiplied XP.
  - The difficulty-select text now also states the enemy strength and the XP/gold numbers.
  - Tests: the 4 `GameSettingsTests` for the removed `GetDifficultyMultiplier`/`GetRewardMultiplier` are gone; 2 new `DifficultyScalerTests` cover applying difficulty to an enemy.
- [ ] **Old boss multipliers and rematches fully off** (finished 2026-11-10, code kept commented with notes on bringing it back). Check: it builds; a boss's name and info screen show no "Strength: x..." label; a new save has no boss repeat counts, and an older save that has them still loads.
- [ ] **Party accuracy by difficulty** - on Hard, an 85% weapon hits about 80% of the time (-0 / -5 / -10 / -15 since 2026-11-24); enemies are unaffected by it (they get their own accuracy bonus); a load keeps the penalty.
- [ ] **Combat text** (items, abilities, effects, companion level-ups) shows in the combat log.
- [ ] **Tutorial** - only the player's own actions count toward "try each action"; a refused action doesn't count; the player gets a Minor Health Potion.

### Levels
- [ ] **Level caps** - a Common weapon stops at level 10, Uncommon at 20, Rare at 30 and Legendary at 40; the player and companions stop at 40; ability info shows the cap (e.g. "Lv.3/10"); former Epic items show as [R]; saves load.

### Items and loot
- [ ] **Components** - they drop from chests and enemies into the pouch, not the bag; a rolled Rare gives a Negativity Crystal; shops only stock buyable ones; the pouch survives save/load.
- [ ] **99 cap** - a pickup that would take any stack (components, bolts, potions) over 99 adds nothing and shows the "You can only carry 99..." message.
- [ ] **Chests** - opening one adds gold and items with no console output; with a full bag the extras come back in `ItemsLeftBehind`.

### Quests
- [ ] **Boss quest progress and reward claiming** (built 2026-11-14 - `QuestManager.NotifyBossDefeated`, `GameManager.ClaimQuestReward`). Check in a real run:
  - Winning the tutorial Skarn fight, a regular boss fight (`ChallengeBoss`), or the final fight (`ChallengeFinalBoss`) completes that boss's quest (`BossDefeatQuest`/`FinalBossQuest`) - the final quest also needs `Accept()` first, since it requires acceptance.
  - Claiming a completed boss quest (`GameManager.ClaimQuestReward(questId)`) adds its gold and XP to the player, and any reward items to the bag (no live quest grants items today, so that path is covered by test only, not by play).
  - Claiming before the boss is beaten, claiming twice, or claiming an unknown quest id all return false and give nothing.
  - New tests: `GameManagerTests`. No existing test changed.

### Game flow and saving
- [ ] **The army camp challenge** (built 2026-11-25 from the rules decided 2026-11-23 - `ArmyCampChallenge`, the camp itself waits for the map):
  - **Starting:** `GameManager.StartArmyCampChallenge(companions)` with up to 3 recruited companions - the fixed lineup, the player always in it. It goes into Combat for wave 1 and stays in Combat between waves (so the day/night clock rolls once, when the attempt ends).
  - **Waves:** wave 1 is 1 Army Soldier at the lineup's average level (rounded - my reading of "the party's level", like a boss fight); each wave adds one (max 8), +1 level every 3 waves.
  - **The pot:** each wave beaten adds that fight's XP and gold, worked out as normal (difficulty included) but not given out. Every 5th wave adds a prize: a random weapon or armor piece at the wave's level - wave 5 Uncommon, 10 Rare, 15 and later Legendary (my defaults). Soldiers drop no normal loot here.
  - **Between waves:** `ArmyCampNextWave()` or `ArmyCampLeave()`. Leaving keeps the whole pot.
  - **Surrender:** a new combat action (`CombatAction.Surrender`, outcome `Surrendered`) - no roll, ends the attempt at once, keeps half the pot. Normal Flee and every item are refused in the waves.
  - **Losing:** never a Game Over - half the pot, then back to the world.
  - **Half:** XP and gold round down; of the prize items, the best half is kept, rounded down (my default - one prize alone is lost).
  - **Fallen stay down:** no "back on their feet" between waves; a downed companion sits out the next waves; the player is always in the fight but can't act while down.
  - **The payout:** the player and every lineup companion get the whole XP amount (my default - benched companions get none); the gold goes to the bag; prizes that don't fit come back as `ItemsLeftBehind`. Then everyone in the lineup is fully healed, revived and cleared of effects.
  - **Best wave:** `StatisticsTracker.ArmyCampBestWave`, saved per save.
  - **Fight rules:** behind it is the new `FightRules` (`combat/fight_rules.cs`), passed to `StartCombat`: flee, items and surrender allowed or not, rewards paid or held back, the win revive, a fixed lineup, free components. Normal fights use the defaults, which change nothing.
  - **Check in play:** a few waves in a row; surrendering; losing; the heal afterwards; the best wave after a load.
  - **Tests:** new `ArmyCampTests`. No existing test changed (`StartCombat` got an optional `rules` parameter at the end; `CombatOutcome` and `ActionType` got one new value each).
- [ ] **Day and night** (built 2026-11-25 from the user's design in `done.md`). `DayNightCycle` (in `systems/`), owned by `GameManager.DayNight`:
  - **The countdown:** 10 real minutes per phase; a new game starts at day. It only runs while the player is in the world (`GameManager.AdvanceClock`, called each frame by Godot).
  - **The roll:** when a fight, a conversation or a cutscene ends for good, -60 to +60 seconds (every whole number) is added to what's left. Pausing mid-fight, or the intro going straight into the tutorial fight, doesn't roll until the real end. Menus, shops, the bag and the pause screen stop the clock with no roll. If a roll takes it to 0 or below, the phase flips straight away.
  - **Every flip** starts a full 10 minutes (my default - a few seconds of overshoot aren't carried over).
  - **Manual switch:** `GameManager.SwitchDayNight()` flips it and starts a full countdown (anywhere in the world for now - see section 1, "Day/night leftovers").
  - **Saving:** the phase and the time left are saved (`SaveData.IsNight`, `DayNightSecondsLeft`). Saves made before load at day with a full countdown.
  - **Night enemies:** an enemy spawned at night (`DayNightCycle.MakeNightEnemy`) gets +10% HP and damage (rounded up), +20 aggression (my reading of "one step" - aggression is 0-100, capped at 100) and +20% XP and gold (`EnemyBase.RewardMultiplier`). It stays a night enemy if day comes before it is fought. Bosses are never night enemies. The difficulty goes on top.
  - **Closed at night:** `OpenShop` and `OpenBlacksmith` refuse at night, with `LastRefusalReason` = "It's closed for the night. Come back in the morning." (placeholder text). Inns don't exist yet.
  - The clock uses its own random numbers (`DayNightCycle.Rng`), so it never changes other rolls.
  - **Check in play:** the clock stopping in menus and fights; a fight's roll; night enemies hitting harder and paying more; the shop closed at night; save and load keep the time.
  - **Tests:** new `DayNightTests`. No existing test changed (the save methods got an optional `dayNight` parameter at the end).
- [ ] **New game wipes the old one** - start a game, recruit, fight, then start a new game: no companions until Chance joins, clean statistics, no leftover enemies, Back/Pause doesn't return to the old game. The same after loading another slot. Quitting mid-fight then starting a new game: the old fight's end does nothing. A failed load keeps the current game. Volume and text speed survive a new game.
- [ ] **Tutorial and saving** - saving during the intro or the Skarn fight fails with the message; after winning it works; a loaded game can save straight away.
- [ ] **Boss auto-save** - challenging a boss saves first; losing and loading lands right before the boss; a brand-new game's first auto-save goes to an empty slot; a failed write still starts the fight and sets `LastSaveError`.
- [ ] **Defeat → last save** - losing a regular fight goes back to the last save point; losing to a boss goes back to right before it; losing the tutorial gives no "Load Last Save".
- [ ] **Difficulty descriptions** show only what really changes: enemy strength, XP and gold, and the hit-chance penalty, all read from the code.
- [ ] **Intro → tutorial** - a new game lands in `Cutscene`; after `FinishIntro()` it is `Tutorial` on top of `Combat`; it drops to `Combat` after the fourth different action; winning goes to `Playing`; Flee is refused all fight.
- [ ] **Screen states** - each Open from an allowed state pushes its state, and Close returns to the one before (e.g. Paused → Settings → Paused → Combat); Open from any other state returns false; the save screen won't open during the tutorial; `CurrentShop` etc. are set while open and null after.
- [ ] **Ending** - winning the final fight → `Ending` → `Credits` → `MainMenu`; a Finish call in the wrong state does nothing; after a win the slot loads to just before the final boss with `IsGameCompleted` true, and a later save keeps the flag; a failed save prints nothing and sets `LastSaveError`.
- [ ] **Godot access** - `CurrentState`, `Player`, `Party`, `Map`, `Quests`, `Settings` etc. are readable; `LoadGame` returns true/false.
- [ ] **Quests** - a new game's quest log shows only the boss quests.
- [ ] **Console program** - `dotnet run` in the repo root does nothing and exits (its two lines are commented out too, 2026-11-10).

### Godot frontend
- [ ] **The `Game` and `SceneRouter` autoloads** (built 2026-11-14, against `docs/godot_frontend_plan.md` sections 2/3 - compiles, not yet run in the actual editor). `Game.cs` creates the one `GameManager` on `_Ready()` and exposes it plus shortcuts (`Game.Player`, `Game.Party`, etc. - section 3's list). `SceneRouter.cs` has the GameState -> scene table from the plan's first draft, reacts to `GameManager.StateChanged`, and `boot.tscn` (new main scene, replaces nothing since there wasn't one) holds the `ScreenLayer`/`OverlayLayer` it hands over on `_Ready()`. Check in the editor:
  - The project boots to `boot.tscn` with no errors in the Output panel, and the console shows `SceneRouter: no scene registered for fixed state MainMenu.` (expected - `main_menu.tscn` doesn't exist yet) rather than a crash.
  - Pushing/popping overlay states (once any overlay scene exists to test with) adds to `OverlayLayer` and removes on close, while whatever's in `ScreenLayer` stays untouched underneath.
  - Opening a second overlay on top of a first (e.g. `Paused` → `PartyMenu`), then closing just the top one, correctly reveals the first again instead of closing both or misreading the pop.
  - A fixed-state change (e.g. once `main_menu.tscn` exists and a "New Game" button calls `StartNewGame`) clears any stray overlays and swaps `ScreenLayer`'s contents.
  - Not built yet, deliberately: every actual screen scene (the table just logs "no scene yet" for each), the `Tutorial` overlay's scene path (not decided - see section 2), and `Notifier.cs` (still empty, untouched by this).

---

# 4. Other

### Story
- [ ] **Write the game story** (ongoing, mostly outside the repo): main storyline, character backstories, world lore, quest narratives.

### Art and learning
- [ ] Sprites: characters, enemies, item icons, environment tiles, UI.
- [ ] Learn Godot; learn how game engines work (for a possible custom engine later).

### Docs and tools
- [ ] Delete the stray root `coding_sessions.json` once you have checked it holds nothing you need (the `.gitignore` additions for it and `__pycache__/` were committed 2026-11-10).
- [ ] Add a note to the top of `player_class_system.md` saying it is a post-MVP reference.
- [ ] Rewrite the README's "Key Features" and "Current Status" sections (they overstate what works) when the game is closer to done.
- [ ] ⏸ Rewrite `SRS.md` from the finished game - only once the game is complete.

### Business and legal
- [ ] Marketing: target audience, social media, trailer and screenshots, press kit, community.
- [ ] Steam: Steam Direct account, store page, build prep, achievements.
- [ ] Copyright registration (research whether it's needed).
- [ ] Create an LLC (talk to a lawyer/accountant).

### Parked until after the MVP (code kept dormant - don't build or delete)
- New Game+ ghost mechanic and secret boss.
- Leadership ability.
- Player classes.
- Boss rematches and the 15%/50% boss multipliers.
- Multiple save slots and timed auto-save.
- Several RNG algorithms at once (the empty Mersenne Twister/Xorshift placeholders stay).
- The old 19 job-board side quests and the job board.
- The Rogue companion.