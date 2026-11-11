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
- [ ] **#6 Demon Prince - add his unique conditions.** He is built (2026-11-11, section 3) with the 5 existing conditions; the design is 7+ equally likely conditions, some unique to him. Each new one is one entry in `DemonPrinceMechanic.ConditionPool`. ⛔ Needs his unique conditions designed (section 2).
- [ ] **Companion → boss plumbing for Risk** - a leave-party trigger, and a way to turn the companion Risk into the Captain boss (built 2026-11-11 as a standalone boss - section 3), with his gear. ⛔ When he leaves and what happens to his gear are in section 2.
- [ ] **Princess: quest-giver → boss transition.** 🤫 She gives story quests for most of the game, then becomes the final boss (built 2026-11-11 as a standalone boss - section 3). Needs the story quests and the moment she turns. No foreshadowing text anywhere.

### Companions
- [ ] **Save the per-companion direct-control toggle** (it is lost on load today).
- [ ] **Recruitment beyond the tutorial** - a real way to recruit companions. Only Chance can be recruited today.
- [ ] **One combat decision-maker for everything the computer controls** (enemies and auto companions), returning the same kind of action the player submits. Targeting is part of it. `enemy_ai.cs` is the obvious home. (Patterns to design in section 2.)
- [ ] **Chance: human form and form switching** - comes after the Demon Prince in the story. ⛔ Details in section 2.
- [ ] **Fluke: build him** - needs a new party-wide "luck" effect (today's effects are single-target only). ⛔ Design in section 2.
- [ ] **The Mage: rework `companion_mage.cs`** into the angry wild-magic Mage. ⛔ Design in section 2, and needs the magic system.

### Combat
- [ ] **Rebalance the regular enemies for the new player** (100 HP, party damage ×2 - 2026-11-10). Goblin, Bandit and Dragon were tuned for the old 24 HP player and will now be pushovers. Do it with the enemy list (section 2), the same way as the boss sheet (`boss_stats_draft.md`).
- [ ] **Weapon screens show the un-doubled damage.** The party's weapon damage is ×2 in the damage calculation (`DamageCalculator.PartyDamageScale`), but `Weapon.GetInfo()`/`GetDisplayName()` still show the weapon's own range. Show the real range, or fold the ×2 into the item numbers once the scale is settled.
- [ ] **Enemy accuracy bonus per difficulty** - none on Normal, rising through Hard and Difficult to Unfair. (Numbers in section 2.)
- [ ] **Enemy XP and gold values become the guaranteed base reward**, with a random extra on top. Do this together with XP rework item "Random XP rewards" below. Bosses too: today a boss's fight XP is rolled from its level and its gold from the loot table, so `BossEnemy.XPValue`/`GoldValue` and the scaler's boss reward multipliers (XP x5, gold x3-5) aren't used yet - wire them in here (decided 2026-11-10, left out of the boss stat build 2026-11-11).
- [ ] **Reposition action** - a 5th combat button. Swap places with a party member. It costs the turn of whoever chose it; the partner doesn't lose a turn. Front positions are attacked more often; back positions are harder to hit. Add it to the tutorial's "try each action" list. ⛔ Needs the position layout (section 2).
- [ ] **Random names for regular enemies**, picked fresh each fight, with no repeats in one fight. Bosses keep their own names. (Name list in section 2.)
- [ ] **Fix the buff/cooldown timing** - effect timers tick at the end of the round they were applied in, so 1-turn buffs vanish before they're used. Settle it with the magic system's spell durations.
- [ ] `DamageCalculator.CalculateHealing()`'s passive healing bonus is never applied.

### XP, levels and abilities
_⛔ The numbers wait on the per-boss level target (section 2)._
- [ ] **Rework the player's XP curve** so level 40 lands near the end of the game (today the cap is reached in about 16 fights). Fix enemy XP growth at the same time.
- [ ] **Companions use exactly the player's XP system** - same curve and cap. Replace the separate curve in `companion_base.cs`.
- [ ] **Ability XP:** passives earn a small amount on every related attack; actives earn more per use. An ability should take about as many fights per level as the player does.
- [ ] **Weapons and armor gain XP from use** (hits landed for weapons, hits taken for armor). Nothing awards gear XP today.
- [ ] **Random XP rewards** - every XP reward is a guaranteed base plus a random bonus. (Size and tiers in section 2.)
- [ ] **Rescale every ability for the level-40 cap** (per-level bonus, rank names, cooldown steps, unlock levels).
- [ ] **Ability half of XP sharing** - downed and benched companions get 50% ability XP. A late recruit's ability levels match the party. (What "50%" means is in section 2.)
- [ ] **Save the XP estimate script in `tools/`** so new numbers can be checked before going into the code.

### Items, loot and magic
- [ ] **Loot picks an item of the rolled rarity** instead of relabelling a random item (a "Legendary" Rusty Sword can drop today). Components already work this way.
- [ ] **Random loot reaches the whole catalog** - `GetAllWeapons()`/`GetAllArmor()` miss most items. ⛔ Do it after the catalog trim (section 2), and build the lists from the item definitions so they can't drift.
- [ ] **A free, very weak spell for every caster** (player, Mage, Fluke) that needs no components. Its name or description must make clear it is weak because it uses none. (Each spell to design in section 2.)
- [ ] **Every spell needs components**, all drawn from the one shared pouch.
- [ ] **Combat Elixir becomes a spell** (with components), not a consumable.
- [ ] **"Elixir of Vitality" doesn't heal** - it runs the Combat Elixir code.
- [ ] **Healing potions become candy/sweets**, sold by the Shop Keeper. Replace the Minor/Health/Greater Health Potion line (`ConsumableType.HealthPotion`) in `item_database.cs`. ⛔ Names and numbers in section 2.
- [ ] **Shop:** pouch components can't be sold yet. (Buying something that won't fit no longer takes the gold - fixed 2026-11-10, see section 3.)
- [ ] **Chests and the ground keep items the player couldn't carry**, so they can come back for them. ⛔ Needs map objects.
- [ ] Inventory stacking ignores XP (two copies with different XP merge into one).
- [ ] Delete the 15 leftover "champion boss key" quest items in `item_database.cs`.

### Quests
- [ ] 🔴 **Give quest rewards to the player** (gold, XP, items). Today claiming does nothing.
- [ ] 🔴 **Wire quest progress into play** - for the boss quests and the new NPC quests only.
- [ ] **Main quests unlock in story order**, with a new "locked" quest state that is saved. (Boss quests already follow the boss order since 2026-11-11 - only the next boss's quest is shown, later ones stay `NotDiscovered`, worked out from the boss progress so nothing new is saved. This item is for the other main/story quests.)
- [ ] **Save quest item rewards** (only gold and XP are saved today).
- [ ] **NPC unlock quests** - one side quest per NPC, done in any order. The NPC is fully locked (no buying, selling or upgrading) until it's done. Build: an "unlock this NPC" quest type, a saved unlocked flag per NPC, and `OpenShop`/`OpenBlacksmith` etc. refusing a locked NPC. ⛔ Each quest's content comes from section 2.
- [ ] **New NPCs:** the magic component seller (rarer components only) and the blacksmith's apprentice (weapon enchanting, unlocked through his own quest line). There is no baker: the Shop Keeper sells the healing sweets and the common components.
- [ ] **Move quest text into data the Godot screens can show** (the console prints became `GameLog` messages 2026-11-10; quest screens still need the text as data).

### Game systems
- [ ] 🔴 **Statistics are recorded as things happen** - have systems raise events and the tracker listen. The same events feed quests.
- [ ] **Settings:** remove `ColoredText` and `UseEmojis` (code, save, tests). Hide the RNG-algorithm and auto-save settings in the MVP. Add a "setting changed" signal. Wire each kept setting.
- [ ] **Player class system out of the MVP** - no class choice and no class bonuses or armor penalties. Keep the code (`player_class.cs`, the save field) dormant.
- [ ] **Day/night state** - saved, with a "switch day/night" option at campfires, inns and rest spots, plus a random auto-flip timer that a manual switch resets. The spawn system reads it. (Timer unit and night effects in section 2.)
- [ ] **`Rest()` becomes an inn-based mechanic** (possibly the same menu as the day/night switch).
- [ ] **Save points:** only inns and marked campsites allow saving. Add the check in `SaveGame` only, so the boss auto-save still works. ⛔ Needs the map.
- [ ] **Gender selection at character creation** - cosmetic only.
- [ ] **Equipment levelling, connected:** gear XP (above), a placed Blacksmith, and the three-way weapon upgrade choice wired in (today `CompleteUpgrade()` always takes "Balanced"). (The typed Y/N and the one-second pause are out of `UpgradeWeapon`/`UpgradeArmor` - 2026-11-10.)
- [ ] **Place shops, the blacksmith and NPCs in towns** - `city.cs` becomes the home for in-town places. ⛔ Needs the fixed map.
- [ ] **Elder of the Elven Village** (NPC). ⛔ Needs the fixed map.
- [ ] **Army camp + endless "test your strength" mode** - soldiers to talk to, and a commander who starts back-to-back fights with no healing between them, until you fall. Reward on exit. (Rewards and "what losing means" in section 2.) The loop can be built before the camp.
- [ ] **Per-area enemy spawn lists**, replacing the fixed 70% Goblin / 25% Bandit / 5% Dragon roll. ⛔ Needs the enemy list and the map.
- [ ] **Encounter-size roll** (max 8 enemies per fight), set by enemy type, player level and difficulty. ⛔ Needs the enemy list.

### Code cleanup
- [ ] Delete the duplicate settings/statistics save code in `QuestSerializationHelper` (about 190 lines).
- [ ] **Keep `turn_manager.cs` and `TurnManagerTests`** (decided 2026-11-05). Combat runs its own turn queue today, but the class may be reworked to fit the new combat engine and the 8-slot rounds instead. Check that when the turn-order code is next touched.
- [ ] Rename one of the two `CombatAction` types.
- [ ] Merge the 9 identical weapon-mastery files into one class, and fix the `\\n` typo in their text. (Wait for the masteries-vs-families decision.)
- [ ] Merge the duplicate effects (Regeneration twice, Bleed/Bleeding).
- [ ] Remove dead leftovers: `GetAvailableBosses` (`DisplayBossSelectionMenu` is commented out already), `GenerateMapFromSeed`, `ShowCombatStatus`, the enemies' unused `DecideAction()`, `BossEnemy.DecideAction()`, `BossDefinitions.GetBoss()`, `[Obsolete] LevelUp()` on weapon/armor, the unused `NPC_base.AttackTarget()`.
- [ ] `quest_manager.cs` `SetQuestState()` uses reflection on a public property.
- [ ] Check the build for a flood of nullable warnings in `game_logic`. If there is one, switch the check off or clean up folder by folder.
- [ ] Decide `event_system.cs`: delete it, or make it the one central non-combat event handler (needs an in-depth review first).

### Godot frontend
- [ ] 🔴 **A shared game object (autoload)** that creates `GameManager` and keeps it alive between scenes.
- [ ] 🔴 **A scene switcher** that picks the scene from `GameManager.CurrentState`, including at start-up and after a load.
- [ ] 🔴 **Combat scene: find out why Attack does nothing** - check Godot's Output panel (`Console.Clear()` was removed, so look for a `GetNode` null error).
- [ ] **Combat scene receives its fight from `GameManager`** instead of building its own, and goes through the tutorial check during the tutorial.
- [ ] **Combat scene shows:** names (enemy name + type), damage numbers, "Critical", "Miss"/evade, health bars that move on each hit, whose turn it is, turn order and round (8 slots, skipped ones shown), all 5 action buttons (greyed when unusable), target and item picking, defend markers, status effects with time left, the companion control toggle, downed/revived combatants, an end screen per outcome with rewards, the boss name/title and attack warnings, the text log. Also every ability (`GetAbilities()` for Felicity), Boon's bolt picker (`GetBoltCounts()`), and Risk's blocked hits (`AttackBlocked`).
- [ ] **Screens:** party menu (player, inventory and companions in one, with an equip action that moves items in and out of the bag, plus use, drop and the control toggle), shop, blacksmith, quest giver, quest log, settings, statistics, main menu/new game/load, character creation, pause, save, loot pop-up, Game Over (offer a new game when there's no save), ending and credits.
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
- [ ] **Ending and credits** - the story content and credits text. Does a completed save unlock anything (e.g. a marker on the load screen)?
- [ ] **Dialogue lines** for NPCs and companions.
- [ ] **Difficulty flavour text** - the placeholder lines in `GetDifficultyDescription` are for you to reword.

### Bosses
- [ ] **Librarian-Monk:** his real name, title and description (the code has placeholders: "Librarian-Monk", "Keeper of the Library"), and where he is fought. Confirm the shield's level-1 health - 20 is my default (`LibrarianMonkMechanic.ShieldHealthAtLevel1`), which makes it 102 on Hard at level 8, about a third of his 306 HP. Confirm: a lost turn doesn't count toward his cooldown (my default). (The shield rules were decided 2026-11-11 - see `done.md`.)
- [ ] **Centipede:** should "a restack only adds the minimum duration" be the rule for every stacking damage-over-time effect (Burn, Bleed too), or only his poison? (Built for his poison only, 2026-11-11 - any effect can now set its own restack rule through `AbilityEffect.OnReapplied`.) Also his real name, title and description (placeholders in the code: "Giant House Centipede", "the Many-Legged"), and where he is fought. Confirm my defaults: stacks multiply the poison's damage (2 stacks = twice the damage each turn), and the 3-4x target lean is 3.5x (weights 7 to 2).
- [ ] **Cyclops Brothers:** their real names and text (placeholders since 2026-11-11: "Fast", the faster one, and "Slow"). What the enrage boosts, how much, for how long, and whether it stacks - my default is the existing Rage effect at his level (level 16: +43% damage, -21% defense, 3 turns; a second rage refreshes it, no stacking), and it keeps running after his brother is back up. Confirm: while one is down, his slot is skipped (the engine does this). Confirm: the fight pays both brothers' XP and gold (each rolls its own, like any two enemies) but counts as one boss beaten. Later: a combined attack with its own animation.
- [ ] **Demon Prince:** his unique conditions (names and effects) - the user is still thinking about them (2026-11-11). Built for now with the 5 existing conditions (Poisoned, Burning, Bleeding, Grappled, Pinned), so confirm or change these defaults of mine: landing a condition the target already has follows that condition's normal rule (damage over time stacks, slows refresh); strength from his level - poison uses the poison formula, burn and bleed do the middle of it each turn for 3 turns (10 at level 21), grapple and pin take 4 + 1 per 10 levels of speed for 2 turns. Also his real title and description (placeholders: "Prince of Chaos"), and where he is fought.
- [ ] **Slime Dragon:** built 2026-11-11 with my defaults (agreed for now) - 1 dragling per hit, at most 6 alive, the end-of-round turn tops them back up to 3 (exactly 3 alive = no bonus turn; counted when the round ends). Draglings fight at the party's level (his level - 5) with my stats (level 21: 120 HP, 8-17 damage, speed 21). Retune after testing. Also his real title and description (placeholders: "the Ever-Dripping"), and where he is fought.
- [ ] **Captain (Risk as a boss):** built 2026-11-11 with my defaults (agreed for now) - crit chance Normal 50%, Hard 55%, Difficult 60%, Unfair 65%, at 2.5x on every difficulty. Still open: when exactly Risk leaves the party (the why is decided - 🤫 see `done.md`); what happens to his gear.
- [ ] **Princess:** built 2026-11-11 with my defaults - confirm or change: action weights attack 50 / skip 25 / guards 25, shifting to 40 / 40 / 20 at half health or below; second-turn slowdown Normal 6-8, Hard 4-6, Difficult 2-4, Unfair 1-2 (the same at every level); a skip only goes to someone who isn't already losing a turn (otherwise she attacks); castle guards at the party's level (level 40: 260 HP, 15-31 damage, speed 20). Also her real name, title and description (placeholders: "The Princess", "Heir to the Throne").

### Companions
- [ ] **Starting gear for each companion** - should it match the story (Boon's crossbow, Risk's Royal Guard armor)? Is it better for a late recruit? Can the player swap companions' gear, and can Chance wear any at all?
- [ ] **Recruitment order** (Chance first, in the tutorial - the rest?).
- [ ] **Chance:** tune the placeholder numbers (85% accuracy, 5% crit, 80 HP + 8/level, speed 14, Aggressive AI). Will a 1-100 hitter end the Skarn fight too fast? Block the cat from gear? Human form: what does he fight with, and can he wear gear? The switch threshold - fixed, or changed by level or luck - and is it rolled before or after the attack?
- [ ] **Risk:** rename "Royal Guard's Armor"? Should better or heavier armor raise the chance?
- [ ] **Boon:** what Ice and Acid bolts do; tune the numbers (95% floor, pin -5 speed for 2 rounds, axe 75%, Fire burn, auto rules, bolt prices 6-10g); where bolts are sold or found; his combat barks; his +10% accuracy passive (wire it or drop it); his crossbow's damage, whether it can be upgraded, and whether it can be taken off him; should Precision Training push him above 95%?
- [ ] **Felicity:** confirm the defaults (Healing Circle 60% and 4-turn cooldown, the "2+ below 50%" rule, the names, debuff removal + regeneration on Divine Light). Should her healing also raise speed?
- [ ] **Fluke:** his ability; what "luck" changes in combat (hit, crit, damage rolls, enemy rolls?); his weapon; how he plays on auto; when he joins.
- [ ] **The Mage:** his name; what makes him angry, and whether it's on/off or a meter; how often a different spell comes out (does it grow with anger?); which spells can come out, and can they backfire on the party; does anger do anything else (like Rage)?; on auto, does he try to calm down?
- [ ] **Targeting patterns** for each enemy type and auto companion (most wounded, weakest defense, last attacker, spread out...).

### Combat
- [ ] **Enemy accuracy per difficulty** - the bonus per level and a ceiling (e.g. never above 95%).
- [ ] **Positions** - how many (front/back rows or a line of four), how much more often front positions are attacked, how much harder back positions are to hit, whether position affects the party's own attacks, the starting formation, ranged-weapon penalties per position, which weapons count as ranged, and melee rules.
- [ ] **Confirm the combat defaults:** speed ties broken at random; the player gets up at 1 HP if the party wins while they're down; an enemy that flees drops each loot item with a 50% chance.
- [ ] **Small numbers:** percentage bonuses round down to nothing on small hits. Use the fraction as a chance to round up, raise all the base numbers, or both? (Partly eased 2026-11-10: the player's health is 100 and the party's damage ×2, so party hits are bigger. Enemy hits are still small until the enemies are rebalanced.)
- [ ] **How long spells and buffs last** (with the magic system).
- [ ] **Combat screen timing:** queue the events and play them back, or have combat advance one turn at a time?

### Difficulty
- [ ] 🔴 **Make the harder difficulties much harder** - the wiring is built (section 3) with the old placeholder numbers: enemy health and damage 75% / 100% / 150% / 200%, XP 80% / 100% / 125% / 150%, gold 100% / 100% / 120% / 140%, party hit chance -0 / -7 / -14 / -21. The harder difficulties should be much harder and a lot more punishing - Unfair shows how punishing RNG can be when it isn't on your side (decided 2026-11-10). Set the real numbers (and whether defense, speed or crit should scale too).
- [ ] **Starting stats per difficulty** (higher on easy, lower on hard): which stats; on top of enemy scaling or instead of it; whether the gap shrinks with levels. Check it against the per-boss level target.

### XP, levels and abilities
- [ ] 🔴 **The per-boss level target** - what level the player should be at each boss. All the XP numbers are built backwards from this, after the group sizes.
- [ ] **Random XP bonus:** how big (e.g. 0-50% of the base), a flat roll or tiers (small/normal/lucky), keep today's 10% chance of +50%, and whether gear and abilities get it too.
- [ ] **XP earned past a cap** - lost, or held for a future upgrade?
- [ ] **Ability XP:** the actual numbers for passives and actives. Triggers for the passives that aren't "the player attacks": Evasion, Iron Will, Precision Training, Rallying Cry, Swift Tactics. What "50% ability XP" means for benched and downed companions.
- [ ] **Weapon masteries:** merge the 9 into weapon families (the leaning)? If yes, which families (where do spear and staff go?), and which new abilities fill the freed slots.
- [ ] **Abilities at level 1** must be worth using (today Attack Boost is +1% and Executioner is a penalty).
- [ ] **One ability per player, for good** - does that still fit?
- [ ] **Rallying Cry, Swift Tactics, Precision Training** only help companions. What do they become?
- [ ] **Confirm the defaults:** cooldown reductions at 1/4 and 3/4 of an ability's cap; old Epic loot chances merged into Rare. Should rarity still slow levelling?
- [ ] **Weapon level and stats:** a weapon's level doesn't change its stats, but an armor's does. Which way should it work?

### Items and magic
- [ ] 🔴 **Catalog trim** to about 50-60 weapons and 20-25 armor sets: how many weapon and armor types stay, the split across rarities, which items stay (cut items stay dormant), and what armor types mean without classes. Blocks the loot fixes.
- [ ] **Former Epic items and abilities** (all Rare for now, marked `TODO: was Epic`) - decide each one: Rare, Legendary or cut. That's 27 weapons, 30 armor sets, Combat Elixir, Greater Revival Potion, and the abilities Evasion, Iron Will, Swift Tactics, Precision Training and the Rogue's. There is no Legendary armor yet, so some may fill that gap.
- [ ] **Spells** - the spells themselves; each caster's free weak spell (strength, cooldown?); which spells need rare components.
- [ ] **Spell components** - rarity, description, price and buyable flag for each of the 37 (all placeholders now); loot weights and amounts; how they're grouped; area-based drops (mushrooms in forests...); the Negativity Crystal being given by an NPC or cursed onto the player.
- [ ] **Names for the other potions** (revival, antidote and so on), now that healing potions are becoming candy/sweets.

### Quests
- [ ] **Each NPC's unlock quest** - blacksmith, shopkeeper, magic component seller (more later?): what it asks and who gives it. Is unlocking the whole reward, or is there gold/XP too?
- [ ] **The Shop Keeper's candy/sweets** - their names, whether they are new items or the renamed potions, how strong each is, and what they cost.
- [ ] **The magic component seller's stock** - which of the 37 components count as "rare enough" to be exclusive to them rather than sold by the general Shop Keeper.
- [ ] **The blacksmith's apprentice's side quest line** - not a single unlock quest like the other NPCs, an actual questline. What it involves, how long it is, what unlocks enchanting at the end.
- [ ] **Weapon enchantment effects** - what the apprentice can actually grant (extra power, elemental/status effects, etc.), how many, and whether they're tied to rarity or the questline's progress.
- [ ] **Does the boss-quest giver ("Veteran Ranger") stay** as its own screen?
- [ ] **Main vs. side quests** - rework the unused `progression/quest_system.cs` into the main quest tracker? What is a main quest here?
- [ ] **Rewrite the quest text** for the current story (drop the "greatest warrior in the realm" framing).

### World and systems
- [ ] **Day/night:** what drives the timer (playtime, or a count of in-game actions such as steps or fights); the random interval range; what changes at night (enemies, numbers, aggression, shops, NPCs, the look); the starting phase; which places are switch points.
- [ ] **Enemy types:** army soldiers, palace guards, Kingdom B soldiers and wolves so far - more? Stats, behaviour, areas and group sizes for each. Do Goblin, Bandit and Dragon stay?
- [ ] **Random enemy names** - one list or per type, and how they're shown (e.g. "Tom the Goblin").
- [ ] **Army camp:** the exit reward (gold and/or a prize, by time lasted or enemies beaten) and what losing there means (not a normal Game Over).
- [ ] **Settings:** should some be global rather than per save (e.g. volume on the title screen)?

### Tutorial
- [ ] **Can the tutorial be lost?** An earlier decision says it must not be (e.g. Chance revives you once). The current code sends a loss to Game Over and you replay it. Which one?
- [ ] **The tutorial potion** does nothing visible if the player is at full health at the Item step. Should Skarn land a hit first?

### Godot
- [ ] **Combat scene layout** and a coordinate grid (x/y) for placing things on screen.
- [ ] **The 5 open questions** in section 6 of `godot_frontend_plan.md`.

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
- [ ] **Defend** lasts until the defender's next turn and halves every hit in that window.
- [ ] **One damage path** - companion hits are reduced by enemy defense; enemy hits on companions roll to hit and crit and go through armor; no hit does less than 1 damage.
- [ ] **8 turn slots** - fights with 1, 2, 4, 5 and 8 enemies give the expected slots (extras to the slowest slots, shared speed = average rounded up, empty slots skipped); speeds change between rounds after attacking or defending.
- [ ] **More than one turn slot per round** (written 2026-11-09, for the Princess). A boss whose mechanic implements `IMultiTurnMechanic` gets one opponent slot per turn, each with its own speed. No boss uses it yet, so test it with a small test mechanic (e.g. 2 turns, second turn 5 slower). Check:
  - The boss shows up twice in `TurnOrder`, the second slot with `TurnNumber` 2, and the log reads "Name (2nd turn)".
  - It acts on both turns, and `CurrentTurnNumber` is 1 and then 2.
  - Its second turn never comes before its first, even if it rolls the same or a higher speed.
  - Other enemies share the slots that are left (boss with 2 turns + 3 others: the 2 slowest others share a slot).
  - With too few slots, the extra turns are dropped so every other enemy still gets a slot.
  - `ShouldTakeExtraTurn` returning false skips the turn with a `TurnSkipped` event.
  - A poison on the boss ticks once per round, not once per turn (my default, to confirm).
  - A Defend from its first turn ends when its second turn comes.
- [ ] **Bosses reacting to events** (written 2026-11-10, for the Cyclops revive and the Slime Dragon). A boss whose mechanic implements `IReactiveMechanic` is told about each event from an action or effect tick once it has been resolved. No boss uses it yet, so test it with a small test mechanic. Check:
  - It hears an `AttackLanded`/`Damaged` when it is hit, and a `Defeated` when an ally falls.
  - Poison ticks and end-of-round effect expiry are heard too.
  - Changes a reaction makes (e.g. healing itself) show up as normal events, and can be reacted to.
  - Two reactions that set each other off stop after 10 passes.
  - A boss that is down or fled doesn't react (my default, to confirm).
  - A reaction doesn't end the boss's Defend or use up its turn.
  - A fight without a reacting boss plays exactly as before.
- [ ] **Lost turns - "skip your next turn"** (written 2026-11-10, for the Princess). A general `SkipTurnEffect`, put on a target by a boss with `CombatAction.Inflict(boss, target, new SkipTurnEffect())`. No boss uses it yet, so test it with a small test mechanic. Check:
  - Put on someone who hasn't acted yet this round, they lose this round's turn; on someone who already has, they lose next round's.
  - The lost turn does nothing (nothing is started, not even a Defend), and the log and a `TurnSkipped` event with Message "Turn lost" show it.
  - A Defend from their previous turn still ends when the lost turn comes.
  - It doesn't wear off at the end of the round, only when used up. Putting a second one on doesn't add a second lost turn.
  - Their poison/burn still ticks at the end of the lost turn (my default, to confirm).
  - A boss's end-of-round bonus turn doesn't use it up (my default, to confirm).
  - An antidote or Iron Will removes it (it counts as crowd control).
  - It never carries over to the overworld or the next fight.
  - The player can't submit an Inflict or Summon action (refused with "Only enemies can do that.").
- [ ] **Enemies joining a fight in progress** (written 2026-11-10, for the draglings and the castle guards). A boss calls them with `CombatAction.Summon(boss, enemies)` on its turn, or `context.Summon(enemy)` from a reaction or a bonus turn. Test with a small test mechanic. Check:
  - The new enemy shows in `Enemies`, the log, and an `EnemyJoined` event.
  - It doesn't act in the round it joined, and is in the turn slots from the next round (my default, to confirm). With more than 4 enemies, the slots are shared as normal.
  - It gives normal rewards when the fight is won (draglings get diminishing rewards - see below).
  - No more than 8 enemies can be in the fight at once (my reading of the encounter cap, to confirm). A refused summon logs "No one answers...".
  - The list GameManager passed into `StartCombat` is not changed.
- [ ] **End-of-round bonus turns** (written 2026-11-10, for the Cyclops revive and the Slime Dragon). A boss whose mechanic implements `IEndOfRoundTurnMechanic` is asked once each round, after every slot has been played, whether it wants a bonus turn. Test with a small test mechanic. Check:
  - If not, nothing is added - not shown in `TurnOrder`, not skipped.
  - If yes, a slot with `IsEndOfRoundTurn` is added after the filled slots, an `EndOfRoundTurnAdded` event is raised, and the boss acts before the round ends (before effect countdowns, cooldowns and next round's speeds).
  - It can revive a fallen ally through `context.FallenAllies` (setting Health) - a `Revived` event shows it, and the revived enemy is back in the slots next round.
  - It can summon, or return an action (e.g. an attack) that resolves normally. An attack on a bonus turn isn't joined.
  - It doesn't end the boss's Defend, doesn't tick its effects, and doesn't change its speed for next round (my default, to confirm). `IsEndOfRoundTurn` is true during it.
  - If the fight is already over when the round ends, no bonus turn happens (e.g. both brothers down in the same round = win).
- [ ] **Diminishing rewards** (written 2026-11-10, for farming draglings). Enemies with the same `DiminishingRewardsGroup` pay less XP and gold the more of them are beaten in one fight. Test with a fight of 15+ enemies sharing a group. Check:
  - The first 10 pay full XP and gold; the 11th pays 80%, the 12th 64%, the 13th about 51% (each 80% of the one before), and the log says so.
  - Item drops are not reduced.
  - Enemies with no group, and other groups, are not affected. The count starts again in every fight.
  - A fled one also takes the next place in line, then gets its usual half (my default, to confirm).
- [ ] **Joined attacks** (written 2026-11-10, for the Cyclops Brothers). A boss whose mechanic implements `IAttackPartnerMechanic` is asked after every enemy attack whether it joins in. Test with two bosses whose test mechanic joins each other. Check:
  - When one attacks on his own turn, the other attacks the same target straight after (an `AttackJoined` event, then the attack's own hit/miss events).
  - Abilities, Defend and other actions aren't joined. Party attacks never are.
  - The joiner's Defend stays up, his own slot still comes up this round, and his speed next round isn't changed by the joined attack.
  - If the first attack took the target down, the joiner doesn't attack (my default, to confirm).
- [ ] **Actions are checked** - a bad action (wrong turn, dead target, ability on cooldown, missing item, boss flee) is refused with an `ActionRejected` event.
- [ ] **Direct control** - combat waits for you on a directly controlled companion's turn.
- [ ] **Items in combat** - anyone can use an item on any standing party member. A revive works on a fallen companion or the fallen player, the revived member is slower the next round, and a revive can't target someone standing.
- [ ] **The fight goes on** while anyone in the party stands; if the party wins while the player is down, the player gets up at 1 HP.
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
  - Two opponent slots a round; her second is slower by the difficulty's gap (my default: Normal 6-8 ... Unfair 1-2). With only one party member standing, her second turn is skipped (`TurnSkipped`).
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
  - The boss list (`BossManager.GetBossList()`, for the Godot screen): the next boss shows its name, title, description and mechanic; every boss after it is just "Locked" (all its fields are empty); beaten bosses show their name and title (my default - they've been met).
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
  - When one falls, the other enrages (Rage effect at his level - my default) and, in a bonus turn at the end of the round, brings his brother back at 25% health (`Revived`). No limit on revives.
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
- [ ] **Difficulty changes the game** (wired 2026-11-10). `CombatManager` builds a `DifficultyScaler` whenever its `Difficulty` is set (GameManager sets it on a new game and on load); `GameManager.Difficulty` exposes it. Check:
  - Every enemy's health and damage is multiplied for the difficulty when a fight starts (Normal 75%, Hard 100%, Difficult 150%, Unfair 200%) - including bosses, the tutorial's Skarn, and enemies who join mid-fight.
  - A boss fought again after a loss is not made stronger again (applied once per enemy).
  - The fight's XP and gold get the difficulty multiplier (shown in the log), and companions share the multiplied XP.
  - The difficulty-select text now also states the enemy strength and the XP/gold numbers.
  - Tests: the 4 `GameSettingsTests` for the removed `GetDifficultyMultiplier`/`GetRewardMultiplier` are gone; 2 new `DifficultyScalerTests` cover applying difficulty to an enemy.
- [ ] **Old boss multipliers and rematches fully off** (finished 2026-11-10, code kept commented with notes on bringing it back). Check: it builds; a boss's name and info screen show no "Strength: x..." label; a new save has no boss repeat counts, and an older save that has them still loads.
- [ ] **Party accuracy by difficulty** - on Hard, an 85% weapon hits about 78% of the time; enemies are unaffected; a load keeps the penalty.
- [ ] **Combat text** (items, abilities, effects, companion level-ups) shows in the combat log.
- [ ] **Tutorial** - only the player's own actions count toward "try each action"; a refused action doesn't count; the player gets a Minor Health Potion.

### Levels
- [ ] **Level caps** - a Common weapon stops at level 10, Uncommon at 20, Rare at 30 and Legendary at 40; the player and companions stop at 40; ability info shows the cap (e.g. "Lv.3/10"); former Epic items show as [R]; saves load.

### Items and loot
- [ ] **Components** - they drop from chests and enemies into the pouch, not the bag; a rolled Rare gives a Negativity Crystal; shops only stock buyable ones; the pouch survives save/load.
- [ ] **99 cap** - a pickup that would take any stack (components, bolts, potions) over 99 adds nothing and shows the "You can only carry 99..." message.
- [ ] **Chests** - opening one adds gold and items with no console output; with a full bag the extras come back in `ItemsLeftBehind`.

### Game flow and saving
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