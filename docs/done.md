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
