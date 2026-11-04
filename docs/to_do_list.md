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

### Bosses - shared engine work
_Several bosses need the same engine changes. Build each one once._
- [ ] **A combatant who acts in more than one turn slot per round** - needed for the Cyclops Brothers and the Princess.
- [ ] **A boss mechanic that can react to events** (being hit, an ally falling) - `IBossMechanic` can only choose the boss's own turn today. Needed for the Cyclops revive and the Slime Dragon.
- [ ] **Enemies joining a fight already in progress** - needed for the Slime Dragon's draglings and the Princess's guards.
- [ ] **A "skip your next turn" effect** - general, so it can be reused. The combat screen shows that the turn was skipped.
- [ ] **An extra turn added at the end of a round** - needed for the Cyclops revive.

### Bosses - strength
- [ ] 🔴 **Hand-set level-1 stats for every boss** (health, damage, defense, accuracy, crit, speed, rewards) to replace the shared stopgap formula. Then tune Skarn against the player's starting stats. (notes: Bucket A #11)
- [ ] **A boss's level = the party's average level + 5**, set when the fight starts, applied through `DifficultyScaler`. Count only the fighting party. (Open details in section 2.)
- [ ] **Finish switching off the old boss multipliers and rematches** (keep the code, add comments on how to bring it back). Remaining: `TimesDefeated` saving, the console printout in `ApplyStrengthScaling()`, and the `[Strength: x1.15]` name label.

### Bosses - one by one
- [ ] **#2 Librarian-Monk - shield.** He raises a shield that reduces the damage of incoming attacks. The shield has its own HP, drained by the amount it blocks. It breaks at 0, and he has a 3-turn cooldown before he can raise it again. Needs a new `IBossMechanic` and a damage-pipeline hook. (Open details in section 2.)
- [ ] **#3 Giant House Centipede - poison bite.**
  - Every normal bite has a chance to poison. Chance by difficulty, rolled fresh each bite: Normal 25-35%, Hard 35-45%, Difficult 45-55%, Unfair 55-65%.
  - Duration by difficulty, also rolled: Normal 2-3 turns, Hard 3-4, Difficult 3-5, Unfair 4-6.
  - Damage per turn comes from his level (existing `PoisonedEffect` formula).
  - Poison stacks, but a restack only adds the band's minimum duration (it doesn't refresh to a full roll).
  - Targeting: favours unpoisoned targets about 3-4x, but doesn't strictly avoid poisoned ones.
- [ ] **#4/#5 Cyclops Brothers - always attack together.**
  - They take 2 opponent slots, and the other 2 stay empty. When one brother attacks, the other joins in on the same target. Only attacks are joined; defend and abilities are done alone.
  - One brother is faster than the other. A brother's Defend ends on his own next turn, and joining his brother's attack doesn't count as his turn.
  - When one falls, the other enrages and, in a bonus turn at the end of the round, revives him at 25% health. There is no limit on revives. Both down in the same round = win.
  - XP and rewards only once both are beaten (`DefeatBoss` called once).
- [ ] **#6 Demon Prince - a random condition on every hit.** He picks from 7+ equally likely conditions, some unique to him. Nothing on a miss. A 0-damage hit still applies it. His attack power is lower to balance this. Several conditions can sit on one target at once. ⛔ Needs his unique conditions designed (section 2).
- [ ] **#7 Slime Dragon - spawns draglings whenever hit.** ⛔ Needs the reactive hook and mid-fight joining above.
- [ ] **#8 Captain of the Royal Guard (Risk as a boss)** - very high crit chance (about 55%, higher on harder difficulties) and a very high crit multiplier.
- [ ] **Companion → boss plumbing for Risk** - a leave-party trigger, and a way to turn a `CompanionBase` into a matching `BossEnemy`.
- [ ] **#9 Princess (final boss).** 🤫 She is a hidden twist: no foreshadowing text anywhere.
  - Two turns per round in two opponent slots. Her second turn's speed = her speed minus a random amount: the gap is wide on easy difficulties (about -7 on Normal) and narrow on hard ones (about -1 on Unfair).
  - Each turn she picks one of: **force a skip** (a random opponent loses their next turn), **attack** (random target), or **call two castle guards** (only when no guards are alive, uses her turn).
  - Her two actions in a round never hit the same person. If only one opponent is left, her second turn is skipped.
  - A skipped turn does nothing at all. A Defend still ends when the skipped turn would have come.
  - Castle guards: a new enemy type with normal rewards (farming them is accepted).
  - Quest-giver → boss transition (she gives story quests for most of the game).
  - 🔴 **Spoiler fix:** `GameStartup.InitializeQuests()` reveals the final quest at game start. Reveal it only once she is unlocked.

### Companions
- [ ] **Save the per-companion direct-control toggle** (it is lost on load today).
- [ ] **Recruitment beyond the tutorial** - a real way to recruit companions. Only Chance can be recruited today.
- [ ] **One combat decision-maker for everything the computer controls** (enemies and auto companions), returning the same kind of action the player submits. Targeting is part of it. `enemy_ai.cs` is the obvious home. (Patterns to design in section 2.)
- [ ] **Chance: human form and form switching** - comes after the Demon Prince in the story. ⛔ Details in section 2.
- [ ] **Fluke: build him** - needs a new party-wide "luck" effect (today's effects are single-target only). ⛔ Design in section 2.
- [ ] **The Mage: rework `companion_mage.cs`** into the angry wild-magic Mage. ⛔ Design in section 2, and needs the magic system.

### Combat
- [ ] **Enemy accuracy bonus per difficulty** - none on Normal, rising through Hard and Difficult to Unfair. (Numbers in section 2.)
- [ ] **Enemy XP and gold values become the guaranteed base reward**, with a random extra on top. Do this together with XP rework item "Random XP rewards" below.
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
- [ ] **Shop:** pouch components can't be sold yet, and buying something that won't fit still takes the gold. Check first and refuse the sale.
- [ ] **Chests and the ground keep items the player couldn't carry**, so they can come back for them. ⛔ Needs map objects.
- [ ] Inventory stacking ignores XP (two copies with different XP merge into one).
- [ ] Delete the 15 leftover "champion boss key" quest items in `item_database.cs`.

### Quests
- [ ] 🔴 **Give quest rewards to the player** (gold, XP, items). Today claiming does nothing.
- [ ] 🔴 **Wire quest progress into play** - for the boss quests and the new NPC quests only.
- [ ] **Main quests unlock in story order**, with a new "locked" quest state that is saved.
- [ ] **Save quest item rewards** (only gold and XP are saved today).
- [ ] **NPC unlock quests** - one side quest per NPC, done in any order. The NPC is fully locked (no buying, selling or upgrading) until it's done. Build: an "unlock this NPC" quest type, a saved unlocked flag per NPC, and `OpenShop`/`OpenBlacksmith` etc. refusing a locked NPC. ⛔ Each quest's content comes from section 2.
- [ ] **New NPCs:** the baker (sells sweets that heal) and the magic component seller.
- [ ] **Move quest text out of console prints** into data the Godot screens can show.

### Game systems
- [ ] 🔴 **Difficulty actually changes the game.** Merge the three separate difficulty lists into the one players pick (Normal / Hard / Difficult / Unfair). Wire `DifficultyScaler` into `GameManager` and `CombatManager`. Wire the XP difficulty multiplier. Then remove the unused `GameSettings` multipliers. (notes: "Difficulty selection has zero mechanical effect")
- [ ] 🔴 **Statistics are recorded as things happen** - have systems raise events and the tracker listen. The same events feed quests.
- [ ] **Settings:** remove `ColoredText` and `UseEmojis` (code, save, tests). Hide the RNG-algorithm and auto-save settings in the MVP. Add a "setting changed" signal. Wire each kept setting.
- [ ] 🔴 **No console output anywhere** - finish the conversion. Left: `player.cs`, `NPC_base.cs`, outside-fight events (level up, item added, gear equipped, quest progress, boss defeated) as events for Godot pop-ups, then the rest folder by folder. `menus/` files are reference-only and are skipped.
- [ ] **Player class system out of the MVP** - no class choice and no class bonuses or armor penalties. Keep the code (`player_class.cs`, the save field) dormant.
- [ ] **Day/night state** - saved, with a "switch day/night" option at campfires, inns and rest spots, plus a random auto-flip timer that a manual switch resets. The spawn system reads it. (Timer unit and night effects in section 2.)
- [ ] **`Rest()` becomes an inn-based mechanic** (possibly the same menu as the day/night switch).
- [ ] **Save points:** only inns and marked campsites allow saving. Add the check in `SaveGame` only, so the boss auto-save still works. ⛔ Needs the map.
- [ ] **Gender selection at character creation** - cosmetic only.
- [ ] **Equipment levelling, connected:** gear XP (above), a placed Blacksmith, and the three-way weapon upgrade choice wired in (today `CompleteUpgrade()` always takes "Balanced"). Take the typed Y/N and the one-second pause out of `UpgradeWeapon`/`UpgradeArmor`.
- [ ] **Place shops, the blacksmith and NPCs in towns** - `city.cs` becomes the home for in-town places. ⛔ Needs the fixed map.
- [ ] **Elder of the Elven Village** (NPC). ⛔ Needs the fixed map.
- [ ] **Army camp + endless "test your strength" mode** - soldiers to talk to, and a commander who starts back-to-back fights with no healing between them, until you fall. Reward on exit. (Rewards and "what losing means" in section 2.) The loop can be built before the camp.
- [ ] **Per-area enemy spawn lists**, replacing the fixed 70% Goblin / 25% Bandit / 5% Dragon roll. ⛔ Needs the enemy list and the map.
- [ ] **Encounter-size roll** (max 8 enemies per fight), set by enemy type, player level and difficulty. ⛔ Needs the enemy list.

### Code cleanup
- [ ] Delete the duplicate settings/statistics save code in `QuestSerializationHelper` (about 190 lines).
- [ ] `turn_manager.cs` is unused: delete it together with `TurnManagerTests`.
- [ ] Rename one of the two `CombatAction` types.
- [ ] Merge the 9 identical weapon-mastery files into one class, and fix the `\\n` typo in their text. (Wait for the masteries-vs-families decision.)
- [ ] Merge the duplicate effects (Regeneration twice, Bleed/Bleeding).
- [ ] Remove dead leftovers: `DisplayBossSelectionMenu`/`GetAvailableBosses`, `GenerateMapFromSeed`, `ShowCombatStatus`, the enemies' unused `DecideAction()`, `BossEnemy.DecideAction()`, `BossDefinitions.GetBoss()`, `[Obsolete] LevelUp()` on weapon/armor, the unused `NPC_base.AttackTarget()`.
- [ ] `BossEncounter.StartFinalBossEncounter` attaches a console victory printout that can be left behind (same bug the wipe fixed elsewhere).
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

### Unit tests - ⛔ on hold until most of this list is built
- [ ] Full rework of the backend tests (5 files are switched off and `CombatManagerTests` is broken), plus frontend tests, new tests (bosses, shop, upgrades, save), and a CI pipeline.

---

# 2. Needs creative input

### Story and world
- [ ] 🔴 **The fixed map** - routes and towns (a route/town graph with bounded areas). Town and route names, where each boss lives, which campsites are save points, and where each NPC lives. This blocks placing anything in the world.
- [ ] **Ending and credits** - the story content and credits text. Does a completed save unlock anything (e.g. a marker on the load screen)?
- [ ] **Dialogue lines** for NPCs and companions.
- [ ] **Difficulty flavour text** - the placeholder lines in `GetDifficultyDescription` are for you to reword.

### Bosses
- [ ] **Boss strength:** a plain average of the fighting party, or the player-weighted one in `DifficultyScaler`? Keep `DifficultyScaler`'s extra boss multipliers (health x3, etc.) on top of hand-set stats? Can a boss go above level 40?
- [ ] **Librarian-Monk:** how much the shield reduces per hit (flat or %); the shield's HP and what it scales with (level, difficulty, both); whether the cooldown starts on break or on raise; whether raising it costs his turn; when the AI raises it.
- [ ] **Centipede:** should "a restack only adds the minimum duration" be the rule for every stacking damage-over-time effect (Burn, Bleed too), or only his poison?
- [ ] **Cyclops Brothers:** their names (which also sets who is faster); what the enrage boosts, how much, for how long, and whether it stacks; their stats. Confirm: while one is down, his slot is skipped. Later: a combined attack with its own animation.
- [ ] **Demon Prince:** his unique conditions (names and effects); which existing ones join the pool (Poisoned, Burn, Bleed, Grappled, Pinned); what happens when he lands a condition the target already has (stack, refresh or reroll); how much lower his attack is; what sets each condition's strength and duration.
- [ ] **Slime Dragon:** does beating the main dragon end the fight, or must every dragling be cleared? How many draglings per hit, and is there a cap?
- [ ] **Captain (Risk as a boss):** crit multiplier and crit chance per difficulty; why and when Risk leaves the party; what happens to his gear.
- [ ] **Princess:** her action weights and how they shift with her health; the exact second-turn slowdown per difficulty and level; the castle guards' stats.

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
- [ ] **Small numbers:** percentage bonuses round down to nothing on small hits. Use the fraction as a chance to round up, raise all the base numbers, or both?
- [ ] **How long spells and buffs last** (with the magic system).
- [ ] **Combat screen timing:** queue the events and play them back, or have combat advance one turn at a time?

### Difficulty
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
- [ ] **Potion names** (may be reworked).

### Quests
- [ ] **Each NPC's unlock quest** - blacksmith, shopkeeper, component seller, baker (more later?): what it asks and who gives it. Is unlocking the whole reward, or is there gold/XP too?
- [ ] **The baker's sweets** - new items or renamed potions? How strong, and what they cost.
- [ ] **The component seller's stock.**
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
- [ ] **Actions are checked** - a bad action (wrong turn, dead target, ability on cooldown, missing item, boss flee) is refused with an `ActionRejected` event.
- [ ] **Direct control** - combat waits for you on a directly controlled companion's turn.
- [ ] **Items in combat** - anyone can use an item on any standing party member. A revive works on a fallen companion or the fallen player, the revived member is slower the next round, and a revive can't target someone standing.
- [ ] **The fight goes on** while anyone in the party stands; if the party wins while the player is down, the player gets up at 1 HP.
- [ ] **Bosses** have stats (Skarn about 160 HP, 16-30 damage - probably too strong, note it); the old 15%/50% multipliers are off; a beaten boss can't be challenged again; boss fights include the party.
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
- [ ] **Difficulty descriptions** show only what really changes (the hit-chance penalty).
- [ ] **Intro → tutorial** - a new game lands in `Cutscene`; after `FinishIntro()` it is `Tutorial` on top of `Combat`; it drops to `Combat` after the fourth different action; winning goes to `Playing`; Flee is refused all fight.
- [ ] **Screen states** - each Open from an allowed state pushes its state, and Close returns to the one before (e.g. Paused → Settings → Paused → Combat); Open from any other state returns false; the save screen won't open during the tutorial; `CurrentShop` etc. are set while open and null after.
- [ ] **Ending** - winning the final fight → `Ending` → `Credits` → `MainMenu`; a Finish call in the wrong state does nothing; after a win the slot loads to just before the final boss with `IsGameCompleted` true, and a later save keeps the flag; a failed save prints nothing and sets `LastSaveError`.
- [ ] **Godot access** - `CurrentState`, `Player`, `Party`, `Map`, `Quests`, `Settings` etc. are readable; `LoadGame` returns true/false.
- [ ] **Quests** - a new game's quest log shows only the boss quests.
- [ ] **Console program** - `dotnet run` in the repo root prints the two lines and exits.

---

# 4. Other

### Story
- [ ] **Write the game story** (ongoing, mostly outside the repo): main storyline, character backstories, world lore, quest narratives.

### Art and learning
- [ ] Sprites: characters, enemies, item icons, environment tiles, UI.
- [ ] Learn Godot; learn how game engines work (for a possible custom engine later).

### Docs and tools
- [ ] Commit the `.gitignore` additions (root tracker files, `__pycache__/`), then delete the stray root `coding_sessions.json` once you've checked it holds nothing you need.
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