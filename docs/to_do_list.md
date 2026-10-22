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

**Finished work lives in `done.md` (rule added 2026-10-13).** When an entry here is completed - built and working in the game - it is moved out of this file into `docs/done.md`. This file holds only what is still to do. An entry with several parts moves when all of them are done; until then, finished parts stay ticked (✅) inside the entry. Item numbers are not reused, so gaps in the numbering (2, 4, 7) are entries that have moved.

---

## ⭐ ACTIVE FOCUS: STORY-DRIVEN FEATURE ROADMAP (added 2026-09-28)
_This is the living plan for turning the current codebase into what the story needs. It supersedes/extends item 5 (Boss Key Progression System) and item 10 (Replace Procedural Map) below - see those for original context, both are cross-referenced from here._

**How this is organized:**
- **Bucket A** - story/creative decisions still needed from the designer. This is the actual bottleneck right now - most "code work" below is a mechanical consequence of these, not independent work.
- **Bucket B** - confirmed features, sorted into tiers by dependency (foundational first), not by size. Tackling Tier 0 first collapses the most downstream uncertainty.

### Bucket A: Open story decisions
1. ✅ **Resolved (2026-10-05)** Character-creation Gender selection has no stat/mechanical difference between genders - purely cosmetic/narrative.
2. Librarian-Monk boss - unique mechanic not yet designed.
3. ✅ **Resolved (2026-10-16)** Giant House Centipede boss - unique mechanic decided (venomous bite, chance-based poison on every attack). Full design under Tier 2 #3 below.
4. Cyclops Brothers boss - unique mechanic not yet designed.
5. Demon Prince boss - mechanic loosely Chaos-magic themed, not finalized.
6. ✅ **Resolved (2026-10-14)** Princess (final boss) - her unique mechanic is decided, and it replaces the earlier "randomly use one of the other bosses' mechanics" idea. Full design under Tier 2 #9 below.
7. Slime Dragon fight - does defeating just the main dragon end the fight, or must every spawned dragling be cleared too?
8. ✅ **Resolved (2026-10-05)** Army interaction section - an optional, non-story area: an army camp preparing to go to war. The player can talk to the soldiers (dialogue only), or talk to one of the commanders, who offers to have his soldiers "test your strength" - the entry point into the endless combat mode (see Tier 4). Still open: the exact reward structure (gold and/or a prize, scaled by how long the player lasted and/or how many enemies they defeated).
9. Day/night cycle - what actually drives the clock (real time, in-game turns/actions, travel count, etc.)?
10. Which of the existing companions (Warrior/Mage/Ranger/Rogue/Healer) survive as-is, get adjusted, or get fully replaced? (Warrior → Captain of the Royal Guard is the one confirmed swap so far.)
11. Boss difficulty scaling - now that boss order is fixed, what should the difficulty curve look like? The current automatic scaling (15% per boss defeated, 50% per repeat fight) was designed for the old any-order key system (see item 5 below).
    - **What the code does today (checked 2026-10-14):** regular enemies follow the player's level - `GameManager.SpawnEnemy()` makes each one the player's level -1 to +2, and their stats come from that level. **Bosses do not follow the player.** Each boss has a fixed level written into `BossDefinitions` (Skarn is level 2), and at the start of a boss fight `BossManager.ApplyBossScaling()` multiplies its health, damage and defense by 1.15 for every boss already beaten and by 1.5 for every time this boss has been beaten before. Difficulty changes nothing for either (see "Difficulty selection has zero mechanical effect"). `DifficultyScaler` has party-level scaling and a "bosses count as +5 levels" rule, but the game never uses it.
    - The boss multipliers currently multiply zero, because a boss's base stats are never set (`BossEnemy.InitializeStats()` is never called - to-do 1 under `game_logic/entities/`).
    - With the boss order now fixed, "15% per boss beaten" and "each boss has its own fixed level" do the same job twice - boss #5 is always fought after 4 others. Decide which one sets boss strength, whether bosses should also follow the player's level, and how difficulty applies to bosses.
    - **Decided 2026-10-14 - bosses follow the party's level, plus 5.** Two to-dos:
      1. **Give every boss its own level-1 base stats.** Each boss gets a hand-set set of starting numbers (health, damage, defense, accuracy, crit, speed, rewards) describing it at level 1, instead of every boss sharing the one formula in `BossEnemy.InitializeStats()`. Those numbers are what all scaling starts from. This also fixes Skarn's 0 health and damage (to-do 1 under `game_logic/entities/`).
      2. **Wire in the difficulty scaler so a boss's level is set when the fight starts: the party's average level + 5.** The boss grows from its level-1 stats to that level. Purpose: the fight stays a challenge even if the player goes and grinds - when the party gets stronger, the boss does too. Difficulty is applied on top through the same scaler (the "Difficulty selection has zero mechanical effect" item covers wiring it in).
    - **What already exists for this (checked 2026-10-14):** `DifficultyScaler` (`game_logic/systems/difficulty_scaler.cs`, not used by the game) has most of the pieces - a party-level calculation, boss versions of the health/damage/XP/gold scaling, and `GetEffectiveEnemyLevel()`, which already says "bosses are party level + 5" but is only used for display. Points to settle when wiring it in:
      - **"Average" is not a plain average there.** `CalculatePartyLevel()` counts the player as half the total and splits the other half between the companions, and rounds up. Decide: a plain average of everyone in the fighting party, or the player-weighted one. Either way it should count only the **fighting** party, not companions who are recruited but sitting out (to-do 7 under `core/`).
      - **Its boss scaling already multiplies on top** (boss health x3, damage x1.5, XP x5, gold x3-5 over a regular enemy at the same level). With hand-set level-1 stats per boss, those extra multipliers may be redundant - decide whether to keep them.
      - **Its difficulty names do not match the game's.** It uses Easy/Normal/Hard/Nightmare at 0.75x/1x/1.5x/2x; the game's chosen difficulty is Normal/Hard/Difficult/Unfair (`GameSettings`). One set has to go (already logged under `game_logic/systems/`).
      - **The level cap**: party level + 5 can pass the cap (100 today, 50 planned under #12). Cap the boss's level or let bosses go above it.
    - **Decided 2026-10-14 - for the MVP, take out the old boss multipliers and the rematch system, but keep the code for later.** Both are post-MVP ideas - possibly a later update or DLC once the game is out. Handle it the same way as Leadership: switch it off without deleting it, and leave a comment where it was switched off saying how to bring it back.
      - **Old multipliers**: `BossEnemy.ApplyStrengthScaling()` (+15% per boss beaten, +50% per repeat of the same boss, plus the matching reward increases) and `BossManager.ApplyBossScaling()` / `STRENGTH_SCALING_PER_BOSS` / `GetCurrentStrengthMultiplier()`. Stop `CombatManager.StartCombat()` calling it. Boss strength then comes only from the party-level + 5 rule above.
      - **Rematches**: a beaten boss cannot be fought again in the MVP. Pieces to switch off: the repeat-fight warning in `world/boss_encounter.cs`, the `TimesDefeated` count on `BossEnemy` and its saving in `save_data.cs` / `save_manager.cs`, and anything that lets a defeated boss be challenged again. Keep each boss's "defeated" flag - the order-based unlocking depends on it.
      - Also drop the console printout `ApplyStrengthScaling()` makes when it runs, and the `[Strength: x1.15]` label in the boss's display name.
12. **Level cap and XP system review (decided 2026-10-08: cap drops from 100 to 50; XP rework deliberately deferred)** - the game is now about 8 bosses with roughly 5 combat encounters between each, not the 10+ encounters and 15 bosses the levelling was built for, so level 100 is out of reach in a normal playthrough. Plan: halve the level cap to 50 for abilities, weapons and armor. Do NOT rework XP yet - wait until more of the game is defined (map routes, encounter counts, boss difficulty curve in #11) so it can be scaled against real numbers. Not started in code. Things to cover when this is picked up:
    - Where the cap lives: `Ability.MaxLevel` (`game_logic/abilities/ability.cs`), `Weapon.MaxLevel`, `Armor.MaxLevel`, plus the enemy abilities that set `MaxLevel = 100` or divide by 99 (`rage.cs`, `poison_attack.cs`). Decide whether player/enemy levels follow the same cap.
    - Hard-coded level thresholds that assume 100: ability cooldown reductions at levels 25 and 75, milestone levels 10/25/50/75/100 in `leveling_system.cs`, Leadership's 25/50/75/100 (post-MVP).
    - Ability effects scale linearly from level 1 to `MaxLevel`, so changing the cap alone keeps the same top-end values but doubles the gain per level - confirm that is wanted.
    - **Level cap 50 for the MVP re-confirmed 2026-10-14** (abilities, and gear up to Legendary).
    - **Rarity-based level caps for gear - wanted, NOT in the code (checked 2026-10-14).** The plan: each rarity has its own cap, for example Common 20, Rare 30, and so on up to 50 for Legendary. Today `Weapon.MaxLevel` and `Armor.MaxLevel` are a single fixed 100 for every rarity, and abilities are 100 too. What **is** in the code is the other half: rarity sets how much XP each level costs (base 100 Common / 200 Uncommon / 300 Rare / 400 Epic / 500 Legendary / 600 Mythic, x1.035 per level) for weapons, armor and abilities. To do: replace the fixed cap with a cap per rarity, and decide the numbers for Uncommon and Epic (and whether Mythic stays in the MVP).
    - **XP estimate run 2026-10-14** (a small script reproducing the current formulas, 1,000 simulated playthroughs; script not saved in the repo). Results with the code as it is:
      - **The player reaches level 50 far too fast: about 16 fights** with one enemy per fight (13-21), about 9 with two, about 7 with three. Level 10 comes after about 7 fights, level 30 after about 12. The planned game is about 40 fights, so the player would hit the cap before the third boss. Cause: an enemy's XP grows in a straight line with its level (level x 30-70, so about 2,500 XP per kill at level 49), while the XP the player needs grows only 3.5% per level (626 XP for level 49 to 50). Enemies are spawned at the player's level, so every level-up makes the next kill worth more - by the 40s one kill gives several levels at once.
      - **Companions use a different, much steeper curve** (`companion_base.cs`: 100 x 1.15 per level, 81,940 XP for level 49 to 50, cap 100) but get the same XP as the player, so they fall far behind: after 40 single-enemy fights a companion who fought every one is around level 35 while the player is at 50.
      - **Abilities are the opposite - nearly impossible to level.** Ability XP comes only from using it in a fight (10 XP for the first use, +15% for each further use in the same fight). Reaching level 50 takes about 1,256 fights for a Common ability used once per fight, 3,768 for Rare, 6,281 for Legendary - still 188 to 938 fights at five uses per fight. Passives get no XP at all (already logged).
      - **Weapons and armor get no XP at all** - nothing in the game calls their `GainExperience` (already logged), so their curve cannot be tested in play.
    - **What this means for the rework:** the player's curve needs to be much steeper, or enemy XP much flatter (or both), and the two need to be set against the same target. Set the target first - roughly what level the player should be at each boss of the ~40-fight game - then build the curves backwards. The player, companions, abilities and gear should be tuned together, and the encounter-size work (to-do 13 under `core/`) multiplies XP per fight, so settle group sizes before final numbers. Boss XP was not included in the estimate and adds on top.
    - **XP rework to-dos (decided 2026-10-14 - bigger job than first thought).** Numbers still wait for the per-boss level target and group sizes above, but the work itself is confirmed:
      1. **Rework the player's XP curve** so level 50 lands near the end of the game, not after ~16 fights. Fix the enemy-XP side at the same time (today it grows in a straight line with level while the requirement grows 3.5% per level).
      2. **Raise ability XP** so an ability needs roughly the same number of fights to level as the player does (today 1,256+ fights to reach 50). Either give more XP per use or lower the requirement - decide which when the numbers are set.
      3. **Give weapons and armor XP.** Nothing awards it today (`GainExperience` is never called). Decide the source - a share of each fight's XP to the equipped weapon and armor (simplest), or XP from use (hits landed for weapons, hits taken for armor). Either way their curve should feel like the player's and the abilities': a similar number of fights per level, scaled by rarity.
      4. **Random XP rewards: a base amount plus a random amount on top.** Every XP reward is a guaranteed base plus a random bonus rolled on top of it (fits the "RNG is the core mechanic" idea). This matches combat to-do 3 under `game_logic/combat/` (each enemy's `XPValue` becomes the guaranteed base, a roll is added on top) - do them as one job. Still to design: how big the random part is (for example 0-50% of the base), whether the bonus is a flat roll or a few tiers (small/normal/lucky), whether to keep today's 10% chance of +50%, and whether gear and ability XP get the same random top-up.
      5. **Level cap per rarity for weapons, armor and abilities.** Replace the fixed `MaxLevel = 100` (`weapon.cs`, `armor.cs`, `ability.cs`) with a cap set by rarity - Common 20, Rare 30, up to Legendary 50. Pick the Uncommon and Epic numbers, decide whether Mythic stays in the MVP, and save/load the cap correctly. Decide what happens to XP earned past the cap (lost, or held for a future rarity upgrade).
      6. **Companions use exactly the player's XP system (decided 2026-10-14).** Same XP curve, same level cap of 50, and the same base-plus-random XP rewards - whatever is built for the player applies to companions too. Today they have their own steep curve (`companion_base.cs`: 100 x 1.15 per level, cap 100); replace it with the player's rather than keeping a second copy, so the two can never drift apart again.
      7. **Save the estimate script in `tools/`** (or rewrite it) so the new numbers can be checked before they go into the code.
    - XP curve: currently base XP by rarity (100/200/300/400/500) x 1.035 per level. Set a target first (what level should a typical player reach by the final boss?) and build the curve backwards from the expected number of fights.
    - XP sources: only active abilities gain XP today (per use in combat). Passive abilities, weapons and armor are never awarded XP anywhere in the code, so they cannot level at all - the "Equipment Leveling System" below is complete except for this.
    - Whether rarity should still slow levelling (Legendary currently needs 5x the XP of Common).
    - **Ability redesign - idea under consideration, NOT decided (2026-10-12).** Intent: every ability should be directly about combat and make combat more dynamic and engaging for the player. Direction being thought about: no always-on passives - turn the passive abilities (and possibly the weapon masteries) into abilities the player **activates during a battle**, lasting for the rest of that battle or until their effect comes into play (e.g. the next hit, the next dodge). **Before any decision: review every ability one by one** (the 4 active, 6 passive, 9 masteries, the 5 companion abilities and the 2 unused enemy abilities). Things this would also answer or change:
        - XP for passives: if every ability is activated, every ability gains XP from use, which removes the "passives are stuck at level 1" problem (see `game_logic/abilities/` in the code review).
        - Starting strength: abilities need to be worth using at level 1 (today Attack Boost is +1% and Executioner starts as a penalty).
        - One ability per player, chosen once and never changed - check that still fits if abilities are the main source of combat variety.
        - Combat has to check cooldowns and "already active this battle" before letting an ability be used (it checks nothing today).
        - Three abilities only affect companions (Rallying Cry, Swift Tactics, and partly Precision Training) - decide what they become.
13. **Player class system - OUT OF THE MVP, post-MVP idea (decided 2026-10-09)** - the idea is kept, but it is not part of the MVP. The code has five classes (Warrior, Rogue, Mage, Ranger, Paladin), each with stat bonuses and armor bonuses/penalties (`game_logic/entities/player/player_class.cs`, described in `docs/player_class_system.md`). **To do for the MVP:** take the class system out of play without deleting it, the same way Leadership was handled - no class choice at character creation, and no class bonuses or armor penalties applied. Today every `Player` is created with a class (default Warrior), `player.cs` applies it, and the class is written to and read from the save file (`save_data.cs`, `save_manager.cs`), so those are the places to check. Knock-on question for the MVP: the six armor types exist mainly to serve the classes - decide what armor types mean without classes. **Post-MVP:** revisit the design alongside companions (#10) and ability/level balance (#12). Keep `player_class.cs` and `docs/player_class_system.md` as the reference for that.
14. **Armor types and the size of the weapon and armor lists - may be reduced (noted 2026-10-09)** - as the MVP scope shrinks, the six armor types and the large item catalog (207 weapons across 9 weapon types, 180 armor sets across 6 armor types in `game_logic/items/item_database.cs`) may need cutting down. The adjusted numbers are not known yet - they depend on the reduced scope (8 boss fights, smaller map, level cap 50, no classes in the MVP). Settle the numbers before doing other work on the catalog: fixing which items random loot can reach, creating the missing Legendary armor, and the nine weapon masteries (one per weapon type) all depend on them.
    - **MVP target sizes (decided 2026-10-14):** roughly **20-25 unique armor sets** and **50-60 unique weapons**, counted across all weapon/armor types and all rarities together. That is about a quarter of the weapons and an eighth of the armor in the catalog today. Still to settle: how many weapon types and armor types the MVP keeps (fewer types means more items per type), how the items split across rarities, which existing items are kept and which are cut, and what happens to the nine weapon masteries if weapon types are dropped. Cut items should be kept dormant rather than deleted unless decided otherwise.
15. **Difficulty shown through the player's starting stats - DECIDED (idea 2026-10-12, decided 2026-10-14).** On easier difficulties the player starts with higher stats; on harder difficulties, lower. **Why (the user's reasoning):** an easier game means less grinding to get strong, because you start from a stronger point; on a harder game your base stats are lower, so you need the levels. It also makes the gap between difficulties obvious from the first fight. Numbers and details not worked out yet. To settle: which stats (starting health only, or speed and damage too); whether it replaces or adds to scaling the enemies (`difficulty_scaler.cs` scales enemy health, damage, XP and gold and is the kept difficulty system - see `game_logic/systems/` in the code review); whether the gap stays fixed or shrinks as the player levels; and that difficulty is locked once a save is created, so the starting stats are set once at character creation. Goes with the player starting-stats review (to-do 2 under `game_logic/entities/` in the code review) and Bucket A #11. Also check it against the XP rework (#12): the per-boss level targets should allow for players on hard difficulties needing more levels.
16. **Enemy accuracy: a base per enemy, raised by difficulty (requested 2026-10-14) - half built.** Checked in the code 2026-10-14:
    - **Built:** every enemy has its own base accuracy (`Accuracy` in `enemy_base.cs`, a 1-100 hit chance) - Goblin 75, Bandit 80, Dragon 85, and bosses 75 + half their level (`boss_enemy.cs`). `CalculateEnemyAttackDamage()` in `damage_calculator.cs` rolls 1-100 against it.
    - **Not built:** difficulty never changes it. Nothing in `difficulty_scaler.cs`, `GameSettings` or combat touches enemy accuracy, and difficulty has no effect on anything yet (see "Difficulty selection has zero mechanical effect").
    - **To do:** add an accuracy bonus per difficulty (none on Normal, rising through Hard and Difficult to Unfair) on top of each enemy's base, applied when the enemy is created for a fight. Decide the numbers and whether there is a ceiling (for example never above 95-100%, so Unfair still leaves room to dodge). It has to work with the Evasion ability and the reposition action (standing farther back lowers enemy accuracy - core/ to-do 18).
    - **Found 2026-10-14 - enemy attacks on companions never miss.** `combat_manager.cs` (around line 873) works out damage to a companion by hand: no accuracy roll, no crits, `Missed = false` every time. Only attacks on the player roll against `Accuracy`. Route companion targets through the same hit check (and the difficulty bonus) so accuracy means the same thing for everyone.

### Bucket B: Confirmed work, dependency-ordered

**Tier 0 - foundational, unblocks the most downstream work:**
- ✅ **Done (2026-09-30)** Remove the Champion Key/Final Gate system, replace with order-based progression (defeating boss N-1 unlocks boss N). `BossManager` now has `IsBossUnlocked`/`GetNextBoss`/`GetFinalBoss`/`IsFinalBoss` derived from registration order instead of keys/randomness.
- ✅ **Done (2026-09-30)** "Champion" → "Boss" terminology cleanup across the boss/quest system (`boss_manager.cs`, `boss_enemy.cs`, `boss_definitions.cs`, `boss_encounter.cs`, `final_boss_quest.cs`, `boss_defeat_quest.cs`, `quest_giver.cs`).
- ⏳ **Still pending, separate pass** Fixed/hand-drawn map (replacing the placeholder `GenerateFixedMap()`, see item 10 below) - blocks *placing* any new boss/NPC in the actual explorable world (Elder of the Elven Village, every new boss's encounter location). Each boss's combat mechanic can still be designed/built/tested independently of map placement. Needs real location design first (village/route names, where each boss lives) - deliberately not started yet.

**Tier 1 - companion system consolidation (do as one pass, not piecemeal - they all touch the same roster):**
- Decide the final companion roster (Bucket A #10).
- Build Fluke (luck companion) - needs a new team-wide buff/debuff concept ("luck"), since today's effect system (`AbilityEffect`) is single-target only.
- Swap Warrior → Captain of the Royal Guard (pending Bucket A #10 confirmation).
- Build the companion→boss transition plumbing (a leave-party trigger + a bridge from a `CompanionBase` instance to a matching `BossEnemy` instance) - needed for the Captain's late-game turn.
- **Player control of companions in combat - per-companion toggle (decided 2026-10-12).** For each companion the player chooses between two modes, and can switch at any time: **direct control** (the player picks that companion's action on its turn, the same way they pick their own) or **auto** (the computer decides, as it does today). ❌ Not started. What it touches:
  - Today every companion is always automatic: `CombatManager.AdvanceTurns()` only stops and waits when the turn belongs to the player; companions go through `ExecuteCompanionTurn()`, which asks `CompanionBase.DecideCombatAction()`.
  - `CompanionBase` needs a per-companion setting (controlled by player / auto), and it needs saving with the companion.
  - `CombatManager` has to stop and wait on a controlled companion's turn too, and tell the screen **whose** turn it is - `IsWaitingForPlayerAction` is a plain yes/no today. `SubmitPlayerAction()` and the action code behind it assume the actor is the player in several places (attack damage, item use, the ability used) and need to work for a companion.
  - **Decided 2026-10-12 - what a directly controlled companion can do**: Attack, its unique ability, Defend, **use items** (from one shared party bag - the player's inventory; companions do not carry their own, confirmed 2026-10-12) and **flee**. Which enemy it targets is the player's choice (auto mode picks the lowest-health enemy).
  - **Decided 2026-10-12 - fleeing is always a whole-party action.** If the player or any companion chooses Flee, the whole party attempts to flee, not just that one member. Today `ProcessFlee` is written for the player only. To settle when building: whose stats set the chance of escape, and whether a failed attempt costs only that member's turn.
  - **Decided 2026-10-12 - the toggle can be changed at any time**: in the middle of a fight, and outside combat from the party menu (see the "Party menu" row in the screen table under `game_logic/menus/`).
  - New work this adds: item use in combat is written around the player (`consumable.Use(_player)` always applies the effect to the player) - it needs a user and a target so a companion can use an item, and so an item can be used **on** a companion.
  - Screens: the combat scene shows the action buttons for whichever party member is acting, plus the toggle.
- General companion recruitment UI (beyond the one scripted tutorial method) - already deferred; now clearly needed since Fluke and any roster changes need a real recruit flow, not just the tutorial's one-off `RecruitStarterCompanion()`.

**Tier 2 - new bosses, in confirmed story order (corrected 2026-09-30, supersedes the earlier tentative order below this list used to describe):**
- #1 Skarn the Eagle Bear - ✅ done (tutorial fight).
- #2 Librarian-Monk (mechanic TBD)
- #3 Giant House Centipede - **unique mechanic decided 2026-10-16:**
  1. **Every normal bite has a chance to poison the target** - not a separate cooldown-gated ability like the existing generic `PoisonAttack`, the proc rides on its regular attack.
  2. **Poison chance scales with difficulty, rolled fresh per bite within a band** (not a flat number, so even Unfair never guarantees it):
     - Normal 25-35%, Hard 35-45%, Difficult 45-55%, Unfair 55-65%.
  3. **Poison duration also scales with difficulty, same banded-random approach:**
     - Normal 2-3 turns, Hard 3-4, Difficult 3-5, Unfair 4-6.
  4. **Poison damage-per-turn is driven by the centipede's level, not difficulty** - reuses the existing level-scaled formula in `PoisonAttack`/`PoisonedEffect` (`game_logic/abilities/enemy_abilities/poison_attack.cs`). Split of responsibilities: difficulty controls how _likely and persistent_ the poison is, level controls how much it _hurts_.
  5. **Stacking, with a capped duration add:** repeated procs on an already-poisoned target stack (existing `PoisonedEffect.CanStack`), but a restack adds only the _difficulty band's minimum_ duration to whatever time is left, instead of refreshing to a fresh full roll - stops a target getting locked into poison forever while still letting damage compound. See the engine note below - this is not how stacking currently works for any effect.
  6. **Weighted targeting, not strict priority:** the centipede strongly favors biting targets that aren't poisoned yet (discussed as roughly 3-4x the selection weight of an already-poisoned target), but it's a weight, not a hard rule - an already-poisoned target can still get bitten again. A strict "poison everyone before double-dipping" rule was considered and rejected: since the proc isn't guaranteed, it could get stuck hammering one unpoisoned target over and over waiting for the roll, ignoring everyone else, which read as too robotic.
  - **Engine note (found 2026-10-16):** today, restacking _any_ `CanStack` effect (`entity.cs` `AddEffect`) calls `RefreshDuration()`, which resets `Duration` to the new application's full `MaxDuration` - there is no "add" behavior anywhere. Implementing rule 5 needs a small override. Open question: should "restack adds only the minimum instead of refreshing" become the rule for every stacking DoT (`PoisonedEffect`, `BurnEffect`, `BleedEffect`, `BleedingEffect` - all four already stack this way today), or should it be bespoke to the centipede's poison only? Logged for a decision under `game_logic/abilities/` in the code review below.
- #4/#5 Cyclops Brothers (mechanic TBD) - already fits the multi-enemy combat engine (two bosses, one fight) with no new architecture needed for that part.
- #6 Demon Prince (Chaos-magic themed, TBD)
- #7 Slime Dragon - reactive "spawns draglings whenever hit" mechanic. Needs `IBossMechanic` to grow a new on-damage/reactive hook (today it only supports "decide my own turn"), and `CombatManager` to support enemies joining a fight already in progress - a real engine change, bigger than a typical new-boss addition.
- #8 Captain of the Royal Guard - signature is very high crit chance + crit multiplier; may need crit multiplier to become per-enemy configurable (unconfirmed whether it's currently a fixed global value - check `damage_calculator.cs`).
- #9 Princess (final boss) - Tier 0's order-based progression is already in place, so this boss needs no key/gate work - just registering her last in `BossDefinitions.GetAllBosses()`. She no longer borrows other bosses' mechanics, but her guard summon needs the same "enemies join mid-fight" engine change as the Slime Dragon (#7), so build that first.
  - **Unique mechanic - decided 2026-10-14:**
    1. **Two turns every round, taking two of the four opponent turn slots** (to-do 9 under `game_logic/combat/`). Her first turn happens at her normal speed. Her second turn comes later in the same round: its speed is her speed **minus a random amount**, set by **her level and the game's difficulty**.
       - **The gap is wide on easy difficulties and narrow on hard ones.** On the easier settings the party is much more likely to get turns in between her two; on the harder settings her turns come close together. Rough guide: about **-7** speed for the second turn on Normal, about **-1** on Unfair (Hard and Difficult in between). Exact ranges and how her level shifts them are still to set.
    2. **Three kinds of action.** On each of her turns she picks one:
       - **Force a skip** - she picks one of her opponents **at random** and that opponent loses their **next possible turn**: if they have not acted yet this round, they lose this round's turn; if they already have, they lose their turn in the next round.
       - **A normal attack** - target also chosen **at random**.
       - **Call two castle guards** - two guards join the fight on her side and take the **other two opponent turn slots**. Restricted: she can only use it when **no guards are alive** - once both are killed she can call two more, replacing them. She never has more than two guards at once (unlike the Slime Dragon, who keeps adding draglings).
       - A round can be any mix of these: two skips, two attacks, an attack and a skip, a summon and something else.
    - **Random targets on purpose (decided 2026-10-14).** Both her skip and her attack pick a random opponent. This is a deliberate exception to the rule that targeting is part of the combat decision-maker (to-do 5 under `game_logic/entities/`): it keeps the skip fair, so she cannot keep locking the party's strongest member out of the fight.
    - **Her two actions in a round hit two different opponents (decided 2026-10-14).** Two skips, two attacks, or a skip and an attack never land on the same person in one round. This applies to every combination, including two attacks. **If only one opponent is left, her second turn is skipped (decided 2026-10-14)** - so a lone survivor is never caught in a loop of losing every turn while she and two guards keep attacking.
    - **Weighted odds that shift with her health (decided 2026-10-14, numbers later).** The chance of each action is not even, and it changes as she loses health. Weights to be worked out later, including how the summon fits in.
    - **The castle guards are a new enemy type** - no guard enemy exists in the code. They need their own stats and use the shared combat decision-maker like any other enemy. While no guards are alive, their two slots show as skipped.
  - **Also decided 2026-10-14:**
    - **Calling the guards uses up one of her turns.**
    - **The guards give their normal rewards and can be farmed** - accepted, since this is the last fight of the game and there is little left to spend the rewards on.
    - **A skipped turn does nothing at all.** Anything the skipped character would have done or started on that turn is lost - including starting a Defend.
    - **Defend still ends on time.** A Defend from the character's previous turn ends when their next turn would have come, even if that turn is skipped. So a skip never stretches a Defend into a second round. This is the general Defend rule (to-do 2 under `game_logic/combat/`) and applies to any future skip effect, not just hers.
  - **Still open (2026-10-14):**
    - The action weights and how they change with her health.
    - The exact second-turn slowdown per difficulty and per level.
  - **What it needs from the code** (found 2026-10-14 by reading, nothing built):
    - **Two turns in one round is new.** `CombatManager` gives every combatant exactly one turn per round (`BeginRound` builds one entry per combatant). It needs a way for one combatant to appear twice in the turn order, each entry with its own speed. The 8-slot rework touches the same code, so do them together.
    - **"Skip your next turn" does not exist yet.** No effect in the code makes anyone lose a turn: the Warrior's Shield Bash "Stunned" only reduces damage (and nothing reads even that - logged under `game_logic/entities/`), and Skarn's grapple slows its target. Needed: a "skips next turn" effect, and `CombatManager` checking it before letting anyone act and showing that the turn was skipped. Build it as a general effect so other bosses or abilities can reuse it.
    - **Enemies joining a fight already in progress** - the castle guards need the same engine change as the Slime Dragon's draglings (#7 above): `CombatManager` fixes its list of enemies when the fight starts. Build it once for both.
    - Her two actions in one round must avoid each other's target, so her mechanic has to remember who she targeted on her first turn until the round ends.
    - `IBossMechanic.DecideAction` (`boss_mechanic.cs`, used by Skarn) already decides one turn at a time, so the action choice fits the existing shape; it needs to know which of her two turns it is deciding only if the rules end up treating them differently.
    - The combat screen has to show a forced skip on the target and both of her turns in the turn order (items 7 and 11 in "Combat scene - what the screen has to show").

  **Important (confirmed 2026-09-30):** the Princess being the final boss/antagonist is a twist that must stay hidden until the very end - she spends most of the game as a quest-giver sending the player on story quests. When building her: (1) don't add any flavor text elsewhere that foreshadows her before the reveal; (2) she needs both a quest-giver presence (early/mid-game) and a `BossEnemy` instance (late-game) - same shape as the Captain of the Royal Guard's companion→boss transition in Tier 1, but for a quest-giver→boss transition instead; (3) `GameStartup.InitializeQuests()` currently calls `finalQuest.Discover()` unconditionally at game start for whoever the final boss is - **this will spoil her identity immediately** once she's boss #9 unless that's changed to only discover the final quest once she's actually unlocked (see the `SPOILER RISK` comment left in `game_logic/core/game_startup.cs`).

**Tier 3 - large parallel systems (big, but don't block or get blocked by the boss/companion work above):**
- Magic/spellcasting + component system - foundational for player build diversity, but nothing else on this list strictly requires it to exist first, so it can proceed in parallel. Fits well as a `SpellAbility : Ability` subtype (reusing the existing ability/combat pipeline) with components as a new `ItemCategory` that slots naturally into the already-unified loot table system. **Decided 2026-10-12**: the Combat Elixir (attack + defense boost) becomes a spell cast with components here, not a consumable item. How long spells and buffs last must be designed as part of this - see to-do 8 under `game_logic/combat/` in the code review.

**Tier 4 - smaller/independent additions (can slot in anytime):**
- Character-creation Gender selection - cosmetic/narrative only, no stat differences (Bucket A #1, resolved).
- Elder of the Elven Village (NPC) - blocked on the fixed map existing (needs an Elven Village location to live at).
- Army camp (optional, non-story area) + endless "test your strength" mode (scoped 2026-10-05, Bucket A #8 - these were two separate items, now one feature since the camp is how the player reaches the endless mode):
  - Camp: soldiers camping and preparing for war, each with dialogue. Placing it in the world is blocked on the fixed map, same as the Elder.
  - Commander NPC: talking to him offers the challenge, which throws the player into back-to-back combat encounters with no healing between fights, until they fall.
  - Reward on exit: gold and/or a prize based on how long the player lasted and/or how many enemies they defeated (exact structure still open).
  - The combat loop itself is mostly additive and can be built/tested before the camp exists. "No healing between fights" already falls out for free from how `Player.Health` persists across separate fights today. Needs deciding at build time: what losing here means (it shouldn't be a normal game over, since the point is to last as long as possible).
- Day/night cycle affecting enemy spawns - needs its own clock mechanism (Bucket A #9); may interact with the already-flagged-broken difficulty-scaling gap (see the "Difficulty selection has zero mechanical effect" item under Code Cleanup below).
- `Rest()` → inn-based, location-gated mechanic (unrelated to the story list above, but already an agreed-on change - carried over so it doesn't get lost).

**Deferred - post-MVP (not needed to ship the MVP; revisit once there's time for features beyond it):**
- NG+ ghost mechanic + secret ghost-only boss (deferred 2026-10-05, was Tier 3) - fun, but not required for the MVP. Notes kept for when it's revisited: needs New Game+ save tracking, a death-flow branch point (offer the ghost choice instead of an immediate game over), and a new "weapon enchantment" concept that doesn't exist yet. Enemies don't deal damage via `Weapon` objects today (just flat `MinDamage`/`MaxDamage` stats), so "only enchanted weapons can hurt a ghost" needs a new tag-based system, not a literal weapon check.

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
- **Status**: 🚧 Built, NOT working in the game (status corrected 2026-10-09 - previously "FULLY COMPLETED")
- **Not done**:
  - ❌ Weapons and armor are never awarded XP - nothing in the game calls their XP methods, so gear never becomes ready for an upgrade
  - ❌ The Blacksmith is never created or placed anywhere in the game, so the payment/upgrade-choice step cannot be reached
  - ❌ Level cap and XP curve are under review (Bucket A #12)

### 🔴 3. Shop Keeper Functionality
- **File**: `game_logic/entities/NPCs/shop_keeper.cs`
- **Status**: 🚧 Built, NOT reachable in the game (status corrected 2026-10-09 - previously "FULLY COMPLETED")
- **Not done**:
  - ❌ No shop is ever created or placed anywhere in the game - the `ShopKeeper` class is complete but there is no instance for the player to visit (needs shops on the map/towns and a Godot UI)
  - ❌ Shop visits, purchases and sales are not recorded in statistics
- **Description**: Shop system with dynamic pricing (the features below exist in the class)
- **Features**:
  - ✅ Full buy/sell menu system with item browsing
  - ✅ Tier-based shops (1-5) with better prices at higher tiers
  - ✅ Dynamic 3-layer price variability system:
    - **Layer 1**: Shop personality (0.85-1.15x, permanent per shop)
      - Generous shops have cheaper buy prices, pay more when buying from player
      - Greedy shops have expensive buy prices, pay less when buying from player
    - **Layer 2**: Market conditions (0.95-1.05x, changes per restock)
      - Buyer's market vs Seller's market
      - Affects both buy and sell prices
    - **Layer 3**: Per-item variation (±3% for shop items, ±2% for player sales)
      - Tiny haggling/negotiation variation per transaction
  - ✅ Personality-based dialogue (generous/fair/greedy merchants)
  - ✅ RestockShop() generates level-appropriate inventory
  - ✅ Can't sell quest items or equipped gear (with warnings)
  - ✅ Full shop info display explaining pricing system
  - ✅ Dual restock system (prevents shop scumming):
    - **Time-based**: 25±(1-5) minutes since last restock
    - **Combat-based**: 4±(1-2) combat encounters since last restock
    - Whichever condition is met first triggers restock
    - Requirements re-randomized after each restock
    - Initial stock provided at shop creation
  - ✅ Restock tracking display shows time and combat remaining
  - ✅ Combat integration complete:
    - Victory counts as 1 encounter, resets flee counter
    - First flee counts as 1 encounter (no penalty)
    - Consecutive flees add +5 min and +1 encounter to requirements
    - Prevents shop scumming via flee spam

---

## MEDIUM PRIORITY - Systems & Features

### 🔴 5. Boss Progression System - ORDER-BASED (key/gate system removed 2026-09-30)
> **See the "STORY-DRIVEN FEATURE ROADMAP" section above for the current plan** - the confirmed 9-boss roster (with mechanics) lives there as Tier 2 work.
- **Files**: `game_logic/entities/enemies/bosses/boss_enemy.cs`, `game_logic/entities/enemies/bosses/boss_manager.cs`, `game_logic/entities/enemies/bosses/boss_definitions.cs`, `game_logic/entities/enemies/bosses/boss_mechanic.cs`, `game_logic/world/boss_encounter.cs`
- **Status**: 🚧 Framework complete, roster being built one boss at a time
- **How it works now**: Bosses unlock in a fixed order (defeating boss N-1 unlocks boss N), derived from registration order in `BossDefinitions`. There are no champion keys, no final gate, and no random final boss. The original 15-boss roster is archived in `docs/boss_ideas_archive.md` for inspiration only.
- **Still in the code from the old any-order design** (see "Still needed" below):
  - Progression scaling: every boss is 15% stronger per unique boss already defeated (`BossManager.STRENGTH_SCALING_PER_BOSS`)
  - Repeat penalty: a boss is 50% stronger per previous defeat of that same boss (`BossManager.REPEAT_PENALTY_PER_DEFEAT`), multiplied with the progression scaling
  - Reward scaling: gold/XP +5% per unique boss defeated and +10% per repeat (`BossEnemy.ApplyStrengthScaling`)
  - Boss progress, defeats and repeat counts are saved/loaded
- **Still needed**:
  - **Redesign boss difficulty scaling for the fixed order** - the 15%-per-boss and repeat-penalty rules were built for fighting bosses in any order, with key farming to discourage. With a set order, each boss's difficulty can be authored directly, so decide whether any automatic scaling (and any repeat penalty) is still wanted (Bucket A #11)
  - Build the remaining bosses in the roster (Tier 2 above)
  - Tie boss encounters to the fixed map's spawn locations (depends on the map work below)

### 🟡 6. Quest Event System
- **Files**: `game_logic/quests/`, `game_logic/entities/npcs/quest_giver.cs`, `game_logic/menus/job_board.cs`
- **Status**: 🚧 Built, NOT working in the game (status corrected 2026-10-09 - previously "FULLY COMPLETED")
- **Not done** (details in "Quests, statistics and settings are built but not connected" under Code Cleanup):
  - ❌ Quest progress is never updated - no game event (kill, gold, level up, boss defeat, weapon upgrade, equipment change) reaches the quests
  - ❌ Quest Giver and Job Board cannot be opened (waiting for the Godot UI)
  - ❌ Quest list is out of date: it was written for 14 champion bosses and the longer game; boss quests now follow the current roster, and the Job Board set needs resizing for the current scope
  - ❌ Final boss quest is revealed at game start, which would spoil the ending (see the note in `game_startup.cs`)
- **Description**: Quest system with two quest hubs and RNG variety
- **Features built** (the code exists; ✅ here means written, not working in play):
  - ✅ Quest base classes (Quest, QuestObjective, QuestReward)
  - ✅ QuestManager for centralized tracking and active quest management
  - ✅ 8 quest types:
    - BossDefeatQuest (14 quests - one per champion boss)
    - FinalBossQuest (1 quest - requires acceptance before completion)
    - LevelQuest (5 quests - reach level milestones)
    - EnemyKillQuest (3 quests - defeat X enemies with RNG variance)
    - GoldCollectionQuest (3 quests - earn total gold with RNG variance)
    - WeaponUpgradeQuest (2 quests - upgrade weapon to level X with RNG variance)
    - EquipmentQuest (2 quests - equip full gear set with RNG variance)
    - ChallengeQuest (4 quests - flawless victory, crit master, survivor, win streak)
  - ✅ **Quest Giver NPC** for boss-related quests (fixed rewards: 100g + 50 XP)
  - ✅ **Job Board menu** for all other quests (randomized requirements & rewards)
  - ✅ Quest states: NotDiscovered → Available → Accepted → Completed → Claimed
  - ✅ **Retroactive completion**: Progress tracks even if quest not accepted (except final boss)
  - ✅ **Active quest tracking**: Focus on one quest with ★ marker
  - ✅ Quest log display with progress tracking
  - ✅ **RNG quest generation** (once per save file):
    - Job board quest requirements randomized (±1-6 variance)
    - Job board quest rewards randomized (±20-30% variance)
    - Rewards scale with difficulty naturally
  - ✅ **Complete save/load support**:
    - Quest data serialized with requirements and rewards
    - ReconstructQuests() rebuilds quests with original RNG values
    - No re-randomization on load - quests stay consistent per save
  - ❌ GameManager menu access (the old console menu options 8, 9, 10 are commented out)

### 🟡 8. Settings Menu
- **File**: `game_logic/systems/game_settings.cs`, `game_logic/menus/settings_menu.cs`
- **Status**: 🚧 Built, mostly NOT connected (status corrected 2026-10-09 - previously "FULLY COMPLETED")
- **Not done**:
  - ❌ Only the two RNG settings are read by the game. Display, auto-save, confirmation, accessibility and audio settings are stored and saved but change nothing
  - ❌ Difficulty selection has no effect on gameplay (see "Difficulty selection has zero mechanical effect" under Code Cleanup)
  - ❌ The settings menu is console-only and needs a Godot UI
- **Description**: Settings system with multiple categories
- **Features built** (✅ here means the setting exists and is saved, not that it does anything):
  - ✅ Display settings (turn order, combat log detail, damage calculations, enemy stats)
  - ✅ Gameplay settings (auto-save, confirmations)
  - ✅ RNG settings (algorithm selection, statistics tracking)
  - ✅ Accessibility settings (colored text, emojis, text speed)
  - ✅ Audio settings (placeholder for GUI version)
  - ✅ Difficulty selection (Normal/Hard/Difficult/Unfair - immutable after save creation)
  - ✅ Full save/load support
  - ✅ Reset to defaults option
  - ✅ Accessible from pause menu

### 🟡 9. Statistics/Records Page
- **Files**: `game_logic/systems/statistics_tracker.cs`, `game_logic/menus/statistics_menu.cs`
- **Status**: 🚧 Built, NOT connected (status corrected 2026-10-09 - previously "FULLY COMPLETED")
- **Not done**:
  - ❌ Almost nothing is recorded. The only calls are at save time (play time, current gold, current level, save count). Battles, damage, kills, bosses, purchases, upgrades, item and ability use, quests and NPC visits are never recorded, so the pages below show zeros
  - ❌ The statistics menu is console-only and needs a Godot UI
- **Description**: Statistics tracking system for gameplay metrics
- **Features built** (✅ here means the tracker can store and display it, not that the game records it):
  - ✅ Combat statistics (battles, damage, kills, bosses, streaks)
  - ✅ Economic statistics (gold flow, purchases, sales)
  - ✅ Equipment statistics (upgrades, levels, weapon usage)
  - ✅ Item usage statistics (consumables, abilities)
  - ✅ Exploration statistics (shops, NPCs, quests)
  - ✅ Progression statistics (level, XP)
  - ✅ Achievement statistics (flawless victories, close calls, perfect crits)
  - ✅ 8 categorized viewing menus
  - ✅ Summary overview page
  - ✅ Calculated statistics (win rate, averages, favorites)
  - ✅ Full save/load support with dictionaries
  - ✅ Accessible from main menu (option 11)

### 🟡 9. Companion/Party System Integration
- **Status**: 🚧 Mostly complete (entry corrected 2026-10-09 - it previously said companions were not in combat)
- **Description**: Companions fight alongside the player
- **Current**:
  - ✅ Companion base classes exist
  - ✅ Companion abilities implemented
  - ✅ Party management (`PartyManager`, owned by `GameManager`; fixed party of 4 for the MVP)
  - ✅ Companions in combat: own turns in the speed-based turn order, AI picks attack / ability / defend by AI style (Aggressive, Defensive, Balanced, Supportive, Strategic - `CompanionBase.DecideCombatAction`), enemies can target them
  - ✅ Companion XP from combat (every living companion gets the full fight XP)
  - ❌ Recruiting: only the tutorial's starter companion is ever recruited (`GameManager.RecruitStarterCompanion`) - no way to gain the others yet
  - ❌ Never play-tested end to end (companion death, revive mid-combat, boss fights with companions, all companions down)
- **Depends on**: Bucket A #10 (which companions survive into the story)

---

## CODE CLEANUP / REFACTORING
_Tracking spots found while going through the codebase to build a better understanding of it and remove what isn't necessary._

### 🟡 Finish the docs/ folder cleanup (started 2026-10-08)
- **Status**: 🚧 Nearly done - boss docs (6 files down to 1), ability docs (8 files down to `abilities.md`), item docs (3 removed) and the six "implementation complete" write-ups (removed; unfinished work moved into this file) are done
- **Goal**: Get `docs/` down to a small set of files that reflect the current scope. For each group: read it against the current code, keep what is still true, merge or delete the rest.
- **Still to review**:
  - `player_class_system.md` - kept as it is, as the reference for a post-MVP feature (Bucket A #13). It describes code that is not part of the MVP; add a note at the top of the file saying so.

### 🟡 Folder-by-folder code review (started 2026-10-09)
_Big-picture pass over every folder to see where the code is at before the Godot frontend. Findings are logged here only - no code is changed during the review. Fixing/wiring happens in later sessions._

**Progress**: ✅ `systems/` · ✅ `world/` · ✅ `data/` · ✅ `progression/` · ✅ `quests/` · ✅ `menus/` · 🚧 `items/` (first pass logged, not discussed) · 🚧 `abilities/` (first pass logged; redesign idea noted under Bucket A #12) · ✅ `entities/` (to-dos logged) · ✅ `combat/` (to-dos logged) · ✅ `core/` (to-dos logged) · 🚧 `godot_project/` (was `godot_integration/`), root files and `tests/` (first pass logged, not discussed)

#### `game_logic/systems/`
- `RNG_manager.cs` - in use everywhere, healthy. **Decided 2026-10-09: the built-in system RNG algorithm is enough for the MVP.** Post-MVP idea: a system that uses several RNG algorithms at once (see Low Priority #9). Notes for then:
  - The two placeholder algorithms (`MersenneTwisterAlgorithm`, `XorshiftAlgorithm`) are empty shells that are never registered and throw "not implemented". Leave them as markers for the post-MVP work; they are harmless as long as they stay unregistered.
  - The manager is built to run one algorithm at a time and switch between them. Running several at once would need a design change, not just filling in the placeholders.
  - The settings menu offers an RNG algorithm choice, but for the MVP there is only one option - hide that setting in the Godot settings panel until there is a real choice.
- `game_settings.cs` - in use, but most settings are not read by anything (see the "built but not connected" item below). **Decided 2026-10-09 - which settings carry into the Godot settings panel:**
  - Drop `ColoredText` and `UseEmojis` - they only controlled terminal output (also remove them from the save data, `Clone()`, `ResetToDefaults()`, the console settings menu and `GameSettingsTests`).
  - Keep `TextSpeed` (for dialogue/story text that types out).
  - Keep the display toggles (turn order, detailed combat log, damage calculations, enemy stat block) as HUD toggles, the confirmations (flee, item use) as popups, and the audio settings.
  - `AutoSave` (and its interval): keep the setting in the code but **hide it in the MVP settings panel** - there is no auto-saving in the MVP (see `save_manager.cs` under `game_logic/data/` below).
  - None of the kept settings is read by anything yet - each still needs wiring.
- `statistics_tracker.cs` - **staying in the MVP.** Intended behaviour: every statistic updates the moment the thing happens, so the stats page is always accurate. Actual: about 30 recording methods exist, only 4 are ever called, all inside `SaveGame()`. Needs wiring into combat, shops, blacksmith, player, items, abilities and quests. Suggested approach: have the systems raise events (as `CombatManager` already does for Godot) and let the tracker subscribe - the same mechanism would also drive quest progress.
- **Godot hook-ups for settings and statistics are missing** (the two classes themselves are fine for Godot: plain data, no console code):
  - `GameManager` keeps both as private fields with no public access, so a Godot scene has no way to get at them.
  - There are no `OpenSettings()`/`CloseSettings()` or `OpenStatistics()`/`CloseStatistics()` methods like the ones that exist for inventory, character stats and pause. `GameState` has a `Settings` state that nothing uses and no state for the statistics page. (`OpenStats()` is the character sheet, not the statistics page.)
  - The only screens for them are `menus/settings_menu.cs` and `menus/statistics_menu.cs`, which are console menus (typed input, printed text) and cannot be used in Godot - each needs a Godot panel that reads/writes the same data.
  - Neither class announces changes. A statistics panel can simply read the numbers when opened, but settings need a "setting changed" signal so things like volume apply immediately.
  - Settings live inside the save file, so there are no settings before a game is loaded (e.g. volume on the title screen). Decide whether some settings should be global rather than per save.
- `difficulty_scaler.cs` - **KEEP (decided 2026-10-09).** This is the intended difficulty system. It is not connected to the game yet (only `tests/DifficultyScalerTests.cs` calls it), and that is expected for now: tuning it needs the Godot frontend and full playthroughs/simulations, so it will see little use until closer to the MVP. Before it can be tuned it still has to be wired in:
  - Nothing creates a `DifficultyScaler` - `GameManager`/`CombatManager` need one, and enemy health, damage, XP, gold and loot need to pass through it.
  - Its difficulty names (Easy/Normal/Hard/Nightmare) do not match the ones the player picks at character creation (Normal/Hard/Difficult/Unfair in `game_settings.cs`) - the two need to become one list.
  - Once it is the real system, the unused multiplier methods in `GameSettings` and the XP difficulty enum in `leveling_system.cs` become the duplicates to remove (see "Difficulty selection has zero mechanical effect" below).
- `event_system.cs` - **NEEDS AN IN-DEPTH REVIEW in a later session - do not delete yet (decided 2026-10-09).** Not used by anything today, tests included (552 lines). What it contains now: random world events (random encounter, merchant visit, treasure find, weather) with trigger chances, rarity, level limits, cooldowns and durations.
  - **Original intent**: a helper to `GameManager` for random events and spawning - e.g. laying out a city, placing quest givers around the map.
  - **What changed**: much of that is no longer random. Towns and areas will have set locations for the quest giver, blacksmith, magic shop and so on.
  - **Question for the review**: is this another case of a mechanic built twice with only one copy in use (compare `GameManager.Explore`/loot events, `OverworldEnemySpawn`, `MapNode.AvailableEvents`)? Or should it become the one central handler for every non-combat event, with event code that is currently scattered across separate files moved into it? Combat keeps its own event handling in `CombatManager`.
  - **Possible use (idea, 2026-10-09)**: the day/night cycle could be run from here. It fits what the file already does - events with durations that start, expire and can be checked as "currently active". Ties in with Bucket A #9 (what drives the clock).
  - **Outcome to reach**: either remove it, or reshape it into that central non-combat event handler.

#### `game_logic/world/`
- **Intended design (stated 2026-10-09)**: `map_manager.cs` owns the map, and every other file in this folder is a helper to it, so the game can always tell (and later display) which area the player is in and where things are relative to the player's position. Two levels of map node:
  - **Overworld nodes** - a city, a boss encounter, or a route/area between cities and other locations.
  - **Smaller nodes inside a city or camp** - the spawn point of the inn, a shop, the blacksmith and possibly quest givers. These are handled by, or connected to, `city.cs`.
- **Gap against that design**: only the overworld level exists. A town today is one `MapNode` with text labels in `AvailableEvents` ("Shop", "Rest"); nothing models the places inside it. This is the same gap that leaves the shop keeper, blacksmith and quest giver with nowhere to be placed (items 1, 3 and 6).
- `map_manager.cs`, `map_node.cs` - in use. The map itself is still the placeholder (`GenerateFixedMap()`: 10 made-up nodes such as "Haven Village", "The Tyrant's Lair"), not the story's routes and towns - already tracked as Low Priority #10 / Tier work. `LoadMap()` is a stub that just regenerates the map; `GenerateMapFromSeed()` is left over from the procedural map and is never called; the map seed is still written into every save file but nothing reads it back to rebuild a map.
- `boss_encounter.cs` - in use. `DisplayBossSelectionMenu()` and `GetAvailableBosses()` are leftovers from the removed Champion menu and are no longer called.
- `overworld_enemy_spawn.cs` - in use, logic only, waiting on Godot scenes.
- `map_node.cs` location types (`LocationType`: Town, Forest, Cave, Ruins, Mountain, Crossroads, BossRoom, TreasureRoom, RestSite) come from the old node-crawl design. Revisit them when the routes-and-towns map is built; `GameManager.GetEncounterSpawnRange()` depends on them.
- `city.cs` - **KEEP (decided 2026-10-09)**. Today it is a 16-line stub (name, population, location type) that nothing uses. It is the planned home for the in-town nodes described above and needs building out and connecting to `MapManager`/`MapNode`.
- `dungeon.cs` - **DELETED 2026-10-09**. Dungeons are out of scope, including for future versions. Nothing referenced it.
- `README.md` project tree is out of date: it lists a `world/location_type.cs` that does not exist (the enum lives in `map_node.cs`) and has not been checked against the other folders.

#### `game_logic/data/`
- `save_data.cs` - in use, healthy.
- `save_manager.cs` - in use.
  - **Save slots and auto-save - decided 2026-10-09:** the MVP has **one save slot and no auto-saving**. Post-MVP plan: multiple save slots (likely 3) and maybe auto-save. The game already saves to one fixed slot ("save1"), which matches the MVP. `AutoSave`, `QuickSave`, `LoadAutoSave`, `LoadQuickSave`, `DeleteSave`, `GetAllSaveFiles`, `GetSaveFileInfo` and `SaveExists` are only called by tests - leave them in place as the base for the post-MVP slots, and do not wire them for the MVP.
  - **Duplicate code - TO DO, delete it (decided 2026-10-09):** `LoadSettingsFromSaveData`, `SaveStatisticsToSaveData` and `LoadStatisticsFromSaveData` exist twice, once in `SaveManager` and an identical copy pasted into `QuestSerializationHelper` (about 190 lines). Despite the class name, the copied code is about settings and statistics, not quests. Only the `SaveManager` copies are called; remove the copies.
  - **Quests and saving - intended behaviour (stated 2026-10-09):** quest progress must update the moment something changes (example: quest is "get a weapon to level 10" and the weapon goes from 7 to 8 - the quest tracker updates right then). The save manager's only job is to read the current quest state from the quest manager when a save is called. The save side already works that way (`SaveGame` calls `QuestSerializationHelper.SerializeQuests(questManager)`); the missing half is the live updating, tracked in "Quests, statistics and settings are built but not connected to the game" below. When that is wired, check that no quest progress is calculated at save time.
- `game_data.cs` - **DELETED 2026-10-09** (362 lines). `GameDatabase` was an older, second item/enemy/companion/ability database, unused even by tests and superseded by `ItemDatabase` and the real factories. Its entries in `RNGTheGame.csproj` and the README tree were removed too.
- `constants.cs` - **DELETED 2026-10-09** (11 lines). Nothing used it; the same numbers are defined where they are needed elsewhere in the code, and a separate constants file is not wanted.

#### `game_logic/progression/`
- `loot_table.cs` - in use. This is the live loot system for enemy drops, chests and shop stock. **To do (2026-10-09): find out and fix why random loot only reaches part of the item database instead of every item** - tracked in "Random loot only draws from a fraction of the item catalog" below.
- `leveling_system.cs` - **not used by the game** (346 lines, only its test file `tests/LevelingSystemTests.cs` calls it). It was meant to be the single place for XP curves, item upgrade costs, enemy stat scaling by level, XP penalties for under-levelled enemies and the XP difficulty multiplier. Instead the player, abilities, weapons, armor and enemies each carry their own separate formulas. **KEEP (decided 2026-10-09) - it will likely stay but needs a significant rework**, to be done as part of the XP review (Bucket A #12), not before. Its current numbers disagree with the live ones (for example ability XP: base 50 growing 20% per level here, against base 100-500 growing 3.5% per level in `ability.cs`), and its `XpDifficultyLevel` is one of the three difficulty definitions that need unifying (see `difficulty_scaler.cs`).
- `quest_system.cs` - **not used by anything** (576 lines). A complete second quest system; the game uses `game_logic/quests/`. **Do NOT delete - idea under consideration (2026-10-09), see next point.**
- **Main story quests vs side quests (idea, 2026-10-09)**: the game needs a clear distinction between the two. Proposed split: rework `progression/quest_system.cs` into the **main quest tracker**, and make `game_logic/quests/` the **side quest** system, with every quest explicitly designated as one category or the other.
  - Where the code is today: `game_logic/quests/` (the live system) has no main/side designation at all. `quest_system.cs` already has a `QuestType` enum that includes `Main` and `Side`, plus objective types the live system lacks (talk to NPC, reach location, escort, use item).
  - To settle when this is planned: what a main quest is in this game (the boss order and story beats?), how the two trackers share one quest log and one save format, and how the final boss quest is handled without revealing the twist (see the spoiler risk in item 6).
  - Compare the two systems properly when the review reaches `game_logic/quests/`.

#### `game_logic/quests/` (first pass 2026-10-09 - 12 files, about 1,200 lines)
- `quest.cs`, `quest_objective.cs`, `quest_reward.cs` - the base pieces (a quest, one objective with a progress count, a reward of gold/XP/items). In use. Quest states: NotDiscovered -> Available -> Accepted -> Completed -> Claimed.
- `quest_manager.cs` - in use: created at game start, saved and loaded. Holds every quest, the one "active" (tracked) quest, and accept/complete/claim. `DisplayQuestLog()` and `DisplayActiveQuestTracker()` are console screens that need Godot versions. `SetQuestState()` uses reflection to set a property that is already public - tidy up.
- Eight quest types, all created at game start in `game_startup.cs` (`InitializeQuests`): `boss_defeat_quest.cs` (one per boss), `final_boss_quest.cs` (1), `level_quest.cs` (levels 5/10/15/20/25), `enemy_kill_quest.cs` (3), `gold_collection_quest.cs` (3), `weapon_upgrade_quest.cs` (2), `equipment_quest.cs` (2), `challenge_quest.cs` (4).
- **Already logged**: no quest ever progresses, and the Quest Giver and Job Board cannot be opened (see the item below); the final boss quest is revealed at game start (item 6).
- **New - quest rewards are never given to the player.** Claiming a quest marks it Claimed and prints the reward, but nothing adds the gold, XP or items to the player. Both `quest_giver.cs` and `job_board.cs` carry the comment "Actual reward application (gold, XP) happens in GameManager", and `GameManager` has no such code.
- **New - item rewards are not saved.** `QuestReward` can hold items, but the save file only stores a quest's gold and XP reward. No quest uses item rewards today.
- **New - every quest is handed out at game start.** All quests are created and made available immediately, and their random requirement/reward values are rolled once per save. Nothing ties a quest to a place, an NPC or a point in the story, and there is no quest order or prerequisite.
- **New - nothing would feed the challenge quests even after wiring.** Flawless victory, 5 critical hits in one battle, win under 10% health and win streak need per-battle counts that combat does not report. `ChallengeType.MultiKill` and `PerfectBlock` are defined but no quest uses them.
- **New - quests that depend on unfinished systems**: the weapon upgrade quests need gear that can level and a reachable blacksmith (item 1); the level quests stop at 25 and need checking against the level cap change (Bucket A #12); the gold quest only counts gold correctly if every source of gold reports to it.
- **New - story text is out of date**: quest descriptions and the final quest's completion text talk about "the greatest warrior in the realm" / "champion" framing from the old design, and `FinalBossQuest` prints its ending straight to the console.
- **Comparison with `progression/quest_system.cs` (for the main quest / side quest idea above)**:
  - What only `quest_system.cs` has: a main/side quest type; prerequisites (quests that must be finished first); a minimum level to accept; a named quest giver and a named turn-in NPC per quest; optional objectives; repeatable quests; abandoning a quest; a cap on active quests; rewards that unlock an area, a companion or an ability; one general progress method (`UpdateQuestProgress(type, target, amount)`); and `TurnInQuest`, which actually takes the player as input to hand over the reward.
  - What only `game_logic/quests/` has: save/load support, randomised requirements and rewards, the "discovered" step, progress that counts before a quest is accepted, the tracked active quest, the challenge quests, and tests (`tests/QuestManagerTests.cs`, 36 tests).
  - Reading: the unused system is the better fit for story quests (order, NPCs, unlocks); the live one is the better fit for job-board style side quests. That supports the proposed split, but the two have different quest, objective and reward classes with the same names, so they cannot simply run side by side - they need a shared base or one common quest log.

- **TO DOs decided 2026-10-09 for the quest system** (all ❌ Not Started; the work happens in later sessions):
  1. **Give quest rewards to the player.** Claiming a quest must add its gold to the player's gold, its XP to the player's experience and its items to the player's inventory. Wire this through `GameManager` so it works the same from the Quest Giver, the Job Board and any Godot screen.
  2. **Wire the whole quest system to `GameManager`** so quests progress and complete during play (the event wiring described in the item below).
  3. **Job board contents vary by city or town.** Build a system that decides which side quests appear on which town's job board, replacing "every quest available everywhere from the start". Depends on towns existing as real places (`game_logic/world/` - `city.cs`).
  4. **Main story quests unlock in story order.** The next main quest stays hidden until the previous one has been beaten. This also fixes the final boss quest being visible from the start (spoiler risk in item 6). Goes with the main quest / side quest split logged under `game_logic/progression/`.
  5. **Challenge quests read from statistics.** Wire the challenge quests to the statistics tracker so they track and update properly. Statistics must therefore record per-battle facts as they happen (damage taken, critical hits, blocks, enemies defeated in the fight, win streak) - today it records almost nothing (see `statistics_tracker.cs` under `game_logic/systems/`).
  6. **Flesh out quests so they read real game information.** Connect the quest system to the inventory and to the save/statistics data that already track the game's state - weapon levels, armor levels, ability levels, gold, player level and so on - so a quest checks the real value instead of keeping its own separate count.
  7. **Rework the quest text and prepare it for Godot.** Rewrite names, descriptions and completion text for the current story (drop the "greatest warrior in the realm" framing), and move text that is printed straight to the console (`DisplayQuestInfo`, `DisplayQuestLog`, `DisplayActiveQuestTracker`, `QuestReward.DisplayRewards`, the ending text in `FinalBossQuest.OnCompleted`) to data a Godot screen can show.
  8. **Save data for quests and rewards.** Item rewards must be saved (today only a quest's gold and XP reward are), and the inventory must save properly once reward items land in it. The save must record each quest as completed, in progress, not started or **locked**. Today the save stores each quest's state and objective progress, but there is no "locked" state - `NotDiscovered` is the closest and nothing sets a quest back to it. Add the locked state together with to-do 4.

#### `game_logic/menus/` (first pass 2026-10-09 - 3 files, about 1,250 lines)
- All three files are **console screens**: they print text and wait for typed input (433 console calls between them), so none can run in Godot. Each has one entry method, and the only calls to those are in the commented-out console menu in `game_manager.cs` - **nothing reaches any of them today**. No tests.
- `job_board.cs` (378 lines) - side quest screen: view available quests, accept, view active, claim rewards, set the tracked quest. Shows every quest except boss and final boss quests. Carries two gaps already logged under `game_logic/quests/`: rewards are displayed but never given, and the board is the same everywhere (not per town).
- `settings_menu.cs` (504 lines) - edits `GameSettings` in five groups: display, gameplay, RNG, accessibility, audio (labelled "Placeholder"), plus view difficulty and reset to defaults. Its accessibility group is Colored Text, Use Emojis and Text Speed - the first two are being dropped. It offers the RNG algorithm choice and the auto-save interval, both to be hidden for the MVP.
- `statistics_menu.cs` (364 lines) - shows the statistics in eight pages: combat, economic, equipment, item usage, exploration, progression, achievements, summary. Almost every number would read zero today because statistics are not recorded during play.
- **Value of these files**: the screen logic cannot be reused, but each one is a ready-made list of what its Godot screen has to show and let the player do (menu structure, groupings, labels). Use them as the specification when planning the Godot frontend, then remove them once the Godot screens exist.
- **DECIDED 2026-10-12 - keep the three files, as reference only.** They stay in `game_logic/menus/` untouched and are not wired to anything. Each screen is rebuilt from scratch in the Godot frontend when that time comes, using these files as the list of what the screen must show and do. No new files get added to this folder.
- **Other console screens live outside this folder** (quest giver, shop keeper, blacksmith, quest log, boss screens and more) - to be listed as each folder is reviewed.
- **Screens the MVP needs that have no menu file (checked 2026-10-12)** - the three files here cover only the job board, settings and statistics:
  - **Combat** - no menu file, and none needed in `game_logic`. `CombatManager` runs the fight and waits for one call, `SubmitPlayerAction()`, with Attack / Ability / Item / Defend / Flee. The screen is the Godot combat scene, which today has Attack, Defend and Flee buttons only. Missing on the screen: Ability and Item buttons, an item picker, and target picking for allies (revive). Missing in `CombatManager`: a way for the screen to ask whose turn it is, the turn order, and which abilities and items are usable right now.
  - **Inventory** - no menu and no navigation. `GameManager.OpenInventory()` prints the item list and the equipped weapon and armor names to the console, and that is all. The pieces exist (`PlayerInventory` lists, sorts and filters items; `Player.EquipWeapon`/`EquipArmor`; `Item.Use`), but nothing lets the player pick an item and equip, use or drop it. Equipping does not move the old item back into the inventory or take the new one out.
  - **Party / equipment view** - does not exist. `GameManager.OpenStats()` prints the player's own stats, ability, weapon and armor; companions are not shown anywhere. `CompanionBase.GetDetailedInfo()` returns a text block per companion but nothing calls it for a screen. There is no way to change a companion's gear. `GameManager` does not expose the player or the party to a Godot scene (only `Combat` and `Tutorial` are public).
  - **Also without a menu file**: main menu / new game / load, character creation (name, difficulty, ability, starting weapon), pause, shop, blacksmith, quest giver, quest log, loot / reward popup, game over, ending. Shop, blacksmith and quest giver have console screens inside their NPC classes; the rest are stubs or commented out in `game_manager.cs`.
  - **DECIDED 2026-10-12 - no more menu files in `game_logic/menus/`, and nothing is built twice.** The rule for every screen:
    - `game_logic` owns the **information and the rules** - what the player has, what an action does, whether it is allowed. It never prints and never waits for typed input.
    - `godot_project/UI/` owns the **screen** - drawing it, and turning clicks into calls to `game_logic`. It holds no game rules and keeps no copy of the data.
    - Each screen gets one small "helper" entry point in `game_logic` that tells the Godot screen where its information comes from and which actions it can call. Suggested home: `GameManager`, next to the existing `OpenInventory()`/`CloseInventory()` pairs, so every screen opens the same way. To build - none of these helpers exist yet in a form Godot can use.
  - **Where each screen pulls its information from (noted 2026-10-12 so it can be implemented properly).** There is no "player database": the live data sits on the `Player` object and the managers that `GameManager` holds. `GameManager` keeps all of them private today except `Combat` and `Tutorial`, so the first job for every row below is giving the screen a way to reach its source.

    | Screen | Information comes from | Actions it calls | Missing today |
    |---|---|---|---|
    | Combat | `CombatManager` (`Player`, `Enemies`, `Combatants`, `CombatMessage`, `CombatEnded`) | `SubmitPlayerAction()` | Whose turn, turn order, usable abilities and items; Ability and Item buttons on the scene |
    | **Party menu** (player + inventory + companion information - **one menu, decided 2026-10-12**) | `Player` (stats, `EquippedWeapon`, `EquippedArmor`, `SelectedAbility`), `Player.Inventory` (`GetAllItems`, `GetWeapons`, `GetArmor`, `GetConsumables`, `SortInventory`), `PartyManager.ActiveCompanions` (each companion's stats, weapon, armor, ability) | Swap weapon or armor on the player **and on any companion**; use an item; drop an item; the per-companion control toggle (direct control / auto - see Tier 1) | One "equip" action that works for the player or a companion and moves the old item back into the inventory (today `EquipWeapon`/`EquipArmor` exist for the player only and do not touch the inventory); use and drop actions; the toggle; the existing pieces print to the console. Replaces the separate `OpenInventory()` and `OpenStats()` screens |
    | Shop | `ShopKeeper` (stock, prices, restock state) | its buy and sell methods | A shop placed in the world; buy/sell without the console screen inside `shop_keeper.cs` |
    | Blacksmith | `Blacksmith` plus the player's weapons and armor | `UpgradeWeapon`, `UpgradeArmor` | A blacksmith placed in the world; the upgrade-path choice (see `items/` below); console screen inside `blacksmith.cs` |
    | Quest giver, job board, quest log | `QuestManager` | accept, claim, set tracked quest | Rewards are never given; console screens in `quest_giver.cs`, `job_board.cs`, `quest_manager.cs` |
    | Settings | `GameSettings` | set a value, reset to defaults | Public access from `GameManager`; a "setting changed" signal |
    | Statistics | `StatisticsTracker` | none (read only) | Public access from `GameManager`; almost nothing is recorded |
    | Main menu, new game, load, save | `SaveManager`, `GameManager.StartNewGame`, `LoadGame`, `SaveGame`, `GetStarterWeaponChoices` | same | Already close - these take inputs instead of asking for them |
    | Pause, game over, ending | `GameManager` state (`StateChanged`) | `PushState`/`PopState`, `QuitToMainMenu` | Screens only |
    | Loot / reward popup | result of `TriggerLootEvent()` and the end of combat | none | Loot and rewards are printed, not returned, so a popup has nothing to show |

  - This table is the starting point for the scene list in "Plan the Godot frontend file structure" below.
- **Likely cause of the combat scene's dead Attack button (found 2026-10-12, not confirmed on a build)**: `CombatManager.ShowCombatIntro()` calls `Console.Clear()` (line 182; also lines 1150 and 1235) before the first log line. `Console.Clear()` throws an error when there is no real console window, which is the case inside Godot. That would stop `StartCombat()` before the fight begins: nothing in the log, and every button press ignored because combat is never waiting for the player. Matches both symptoms in the "Combat scene MVP" entry below.

#### `game_logic/items/` (first pass 2026-10-12 - 6 files, about 6,700 lines, 5,400 of them `item_database.cs`)
_Found by reading the code only - nothing was built or run._
- `item.cs` - base class, in use. Never used: `IsTradeable` (nothing reads it), `ModifyStat()` (every item type has to implement it, all four leave it empty, nothing calls it), the `Material` and `Treasure` item categories (no class, nothing creates them), and the `Mythic` rarity (no item in the database is Mythic).
- `weapon.cs`, `armor.cs` - in use for damage and defense. The XP/upgrade half is the "built, not connected" system already logged in item 1. New points:
  - **The three-way weapon upgrade choice cannot be reached, even through the Blacksmith.** `Weapon.GetPlayerUpgradeChoice()` has no callers. `blacksmith.cs` calls `weapon.CompleteUpgrade()` with no arguments, which always applies the default "Balanced" path (+1 min, +1 max). Item 1 lists "Player-choice upgrade paths" as ✅ - it is written but not wired.
  - **A weapon's level does not change its stats; an armor's does.** A weapon created at level 20 has level-1 damage, shows as "+20", and asks for level-20 XP and gold to upgrade. Armor gains 1% defense per level. Random loot creates gear at the player's/area's level, so this shows up as soon as loot drops. Decide which behaviour is wanted (goes with Bucket A #12).
  - The XP-needed and upgrade-cost code is written twice (once in each file), and the rarity-prefix code (`[U]`, `[R]`...) four times across the item classes. Both files have an `[Obsolete] LevelUp()` that nothing calls. Both set `MaxLevel = 100` (Bucket A #12).
  - Weapons and armor stack up to 99. The inventory matches on name, level and stats but not on XP, so two copies with different XP merge into one.
- `consumable.cs` - in use in combat (`CombatManager.ProcessItem`). 12 consumables exist.
  - **"Elixir of Vitality" does not heal.** Its text says it greatly restores health, but every Elixir runs the Combat Elixir code (attack and defense boost for 1 turn).
  - Five consumable types have working code but no item: Attack Potion, Defense Potion, Critical Potion, Healing Potion, Buff Potion.
  - Food restores half its listed power, at once (the comment says "over time"). Bread is listed at 20 and heals 10.
  - **Item use will not show in the Godot combat log.** `Use()` and `UseRevivePotion()` print straight to the console instead of going through the combat log, so healing, buffs and revives would happen silently in the combat scene.
  - `LevelUp()` is never called - a consumable's level only comes from how it was created.
- `quest_item.cs` - nothing in the game gives the player a quest item; the only caller of `GetQuestItem()` is save loading. `Use()` ends in a TODO.
- `item_database.cs` - in use (starting weapons, loot, shop stock, save loading). 207 weapons, 180 armor sets, 12 consumables, 20 quest items.
  - **Leftover from the removed key system**: 15 "champion boss key" quest items (lines 5686-5792), with text about the Final Gate. Nothing creates them.
  - Never called: `GetAllQuestItems()`, `GetHealthPotion()`, and the three-argument `GetWeapon()`/`GetArmor()` overloads.
  - The cause of the loot gap already logged ("Random loot only draws from a fraction of the item catalog"): the item definitions and the `GetAllWeapons()`/`GetAllArmor()` lists are two hand-kept copies. Defining each item once and building the lists from that would stop them drifting apart again.
- **New - random loot relabels an item's rarity without changing the item** (`loot_table.cs`, lines 215 and 221). Loot picks a random item, then overwrites its rarity with a separately rolled one. A Rusty Sword can come out labelled Legendary (or Mythic) with Rusty Sword stats - and a Legendary sell price, XP requirement and upgrade cost. The rolled rarity should choose which item drops instead.
- **New - every companion asks the item database for gear that does not exist, which throws an error.** `GetWeapon()`/`GetArmor()` throw when a name is not found, and none of these names are in the database: "rusty chainmail" (Warrior - the database has "Rusty Chainmail Set"), "cracked staff" (Mage, Healer), "linen robes" (Healer), "tattered cloth" (Mage), "old bow" (Ranger), "worn dagger" (Rogue), "worn leather" (Ranger, Rogue). The tutorial's starter companion is the Warrior, so `GameManager.RecruitStarterCompanion()` should fail as soon as it is called. To confirm on a build - this fits the "never play-tested end to end" note under Companion/Party System Integration.
- **Console output**: 71 console calls in this folder (weapon 35, consumable 30, armor 4, quest item 2), including `Weapon.GetPlayerUpgradeChoice()`, which waits for typed input. All need to become data or events a Godot screen can show.
- **Tests**: none for this folder. Items only appear as props in `DamageCalculatorTests`, `PlayerTests` and `SaveManagerTests`.
- **Depends on decisions not made yet**: catalog size and armor types (Bucket A #14), level cap (Bucket A #12).
- `items/` has been presented but not discussed.

#### `game_logic/abilities/` (first pass 2026-10-12 - 26 files, about 2,700 lines)
_Found by reading the code only - nothing was built or run._
- **How abilities work today**: the player picks **one** ability at character creation and can never change it (`Player.SetAbility`). 19 are offered: 4 active (Attack Boost, Heal, Defense Boost, Critical Strike), 6 passive (Evasion, Rallying Cry, Precision Training, Swift Tactics, Executioner, Iron Will) and 9 weapon masteries. Leadership is switched off, as decided. Each companion has one ability of its own, and those five live in the companion files under `entities/`, not in this folder.
- `ability.cs` - base class, in use.
  - **Combat does not check whether an ability can be used.** `CombatManager.ProcessAbility()` runs whatever it is handed: an ability still on cooldown fires anyway, and a passive one prints a message and costs the turn. `Ability.CanUse()` exists for this and nothing calls it. The combat screen needs this check to grey out the button (already on the list of things `CombatManager` cannot tell a screen).
  - Never called: `CanUse()`, `TickCooldown()` and `ReduceCooldown()` (the last two are the same method twice - combat lowers the cooldown number directly). `TargetType` (self, single enemy, all allies...) is set on every ability and combat never reads it.
  - Levelling up throws away any XP beyond what the level needed.
  - Level cap of 100 is built in here (`MaxLevel`, cooldown reductions at 25 and 75) and in the text of most abilities ("Level 100", `Level < 100`, a message at level 51 in Executioner, `/ 99` in `rage.cs`) - Bucket A #12.
- **Passive abilities and masteries are stuck at level 1, where they do almost nothing or do harm.** Only active abilities gain XP (10 per use), as already logged under Bucket A #12. What that means in play: a mastery gives +1% accuracy, damage and crit; Evasion gives its level-1 value; and **Executioner, the Legendary choice, is a permanent downgrade** - -10% crit chance and a 1.1x crit multiplier against the normal 1.5x - because it is designed to start as a penalty and grow out of it. 15 of the 19 choices are affected.
- **Active abilities start very weak too**: Attack Boost at level 1 is +1% damage for 3 turns on a 5-turn cooldown; Heal is 10-20 HP, less than a Minor Health Potion (25). The numbers assume a long climb to level 100. To settle with the level cap and XP review (Bucket A #12).
- **Ability and status-effect text will not show in the Godot combat log** - same problem as items. 113 console calls in this folder: using an ability, an effect being applied, ticking each turn (poison damage, regeneration) and wearing off all print to the console, not through the combat log.
- `weapon_masteries/` - nine files that are the same file with the weapon name changed (same numbers: up to +40% accuracy, +70% damage, +25% crit). `damage_calculator.cs` has a nine-way type check to find which one the player has. Could be one class that takes a weapon type. **All nine have a typo in their description text** (`\\n` instead of `\n`), so the info block would show the characters "\n" instead of line breaks. How many masteries are needed depends on how many weapon types survive (Bucket A #14).
- `player_abilities/` - the 4 active abilities, in use. All four only ever target the user. Their boost effects are read by `damage_calculator.cs`.
- Passive abilities (`evasion`, `executioner`, `iron_will`, `precision_training`, `rallying_cry`, `swift_tactics`) - in use, each wired by a "does the player have this ability" check spread across `damage_calculator.cs`, `combat_manager.cs` and `companion_base.cs`. Rallying Cry and Swift Tactics only do something for companions, so they are worthless until the player has one.
- `enemy_abilities/` - **no enemy has an ability.** `poison_attack.cs` and `rage.cs` are complete, and combat checks whether an enemy has a special ability, but nothing ever gives one to an enemy (`EnemyBase.SpecialAbility` is never set). "2 Enemy abilities ✅" under Content Creation means written, not used. `grapple.cs` is only the Grappled slow effect, used by the Eagle Bear boss - in use.
- `consumable_effects.cs` - **nothing in this file can happen in the game today.** Strength Boost, Resistance and Regeneration are only created by the Buff Potion type, which has no item (see `items/`). Haste, Burn and Bleed are never created by anything, and Haste has no effect even if applied.
- **Same effect written twice**: Regeneration exists here and again in `companion_healer.cs`; Bleed exists here and as "Bleeding" in `companion_rogue.cs`. The base class `AbilityEffect` has two ways to create an effect that take duration and strength in opposite order, which is how the two Regeneration copies ended up with their numbers the other way round from each other.
- **Stacking DoTs always refresh duration to the new roll, never add (found 2026-10-16, needed for the centipede boss - Tier 2 #3 above).** `Entity.AddEffect()` (`entities/entity.cs`) handles every `CanStack` effect the same way: increment `StackCount`, then `RefreshDuration()` sets `Duration` back to the newly-applied instance's full `MaxDuration`. There's no "add" option anywhere. All four existing stacking DoTs work this way today: `PoisonedEffect` (here), `BurnEffect` and `BleedEffect` (`consumable_effects.cs`), `BleedingEffect` (`companion_rogue.cs`). The centipede's poison needs a restack to add only its difficulty band's minimum duration instead of refreshing to a fresh roll. **Decide:** make that the rule for every stacking DoT (one shared change in `AbilityEffect`/`Entity.AddEffect`), or give the centipede's poison its own override and leave the generic refresh-on-stack behavior as-is for Burn/Bleed.
- **For the companion control decision (Tier 1)**: active abilities are written for "whoever uses it", so a directly controlled companion using its ability should work. Passive abilities are all tied to the player's one chosen ability.
- **Tests**: none for this folder.
- `abilities/` has been presented. Outcome so far: the ability redesign idea logged under Bucket A #12 (not decided).

#### `game_logic/entities/` (first pass 2026-10-12 - 24 files, about 4,900 lines)
_Found by reading the code only - nothing was built or run. The three items marked "confirm on a build" look like real breakages._

**Bosses (`enemies/bosses/`)**
- **The tutorial boss has no stats - confirm on a build.** `BossEnemy` sets its health, damage, defense, accuracy, speed and rewards in `InitializeStats()`, and nothing ever calls it for a boss (the three regular enemy types call it in their own constructors; `BossEnemy`'s constructor does not, and neither does `BossDefinitions`, `GameStartup` or `TutorialManager`). Skarn would enter the fight with 0 health and 0 damage. The scaling step multiplies those zeros, so it does not fix it.
- **Once that is fixed the numbers need a first tuning pass.** With stats set, a level 2 boss has 160 health and hits for 16-30 (2.5x that on the stoop attack). The player starts at about 20 health.
- `boss_enemy.cs` - the boss-order scaling and repeat penalty are already logged (Bucket A #11). New: the reward scaling multiplies gold and XP again every time a fight starts, so it compounds across repeat attempts. The 12 `BossMechanicType` values are display text only; Skarn's says "changes strategy at 75%, 50%, 25% health", which is not what his real mechanic does. `DecideAction()` here is never used.
- `boss_mechanic.cs` (Skarn's grapple and stoop) and `boss_manager.cs` (order-based unlocking) - in use and clean. `IBossMechanic` only lets a boss choose its own turn; the Slime Dragon's "react when hit" need is already logged in Tier 2.
- `boss_definitions.cs` - 1 of 9 bosses. `GetBoss()` and `GetBossListSummary()` here are never called (the game uses `BossManager.GetBoss()`).

**Regular enemies (`enemies/`)**
- Three enemy types exist: Goblin, Bandit, Dragon. They are placeholders, not the story's enemies. `GameManager.SpawnEnemy()` picks 70% Goblin, 25% Bandit, **5% Dragon** - a boss-type enemy (125 health, 13-26 damage at level 1) as a random encounter for a 20-health player.
- **Each enemy's own decision code is never used.** Goblin, Bandit and Dragon each have a `DecideAction()` (goblins flee at low health, dragons breathe fire...). Combat ignores it and decides from the enemy's `Behavior` setting in `CombatManager.DetermineEnemyAction()`. The `EnemyAction` list (attack, defend, use ability, flee) is unused with it.
- Never used: `enemy_ai.cs` (an empty class), `EnemyBase.LootTable` and `GetLootDrops()` (loot comes from the loot table system instead), `GetCombatStatus()`. No enemy has a special ability (logged under `abilities/`).

**Companions (`NPCs/companions/`)**
- **Every companion fails to create - confirm on a build** (logged under `items/`: the gear names they ask for are not in the item database).
- **Two of the five companion abilities do nothing beyond their damage.** The Warrior's Shield Bash applies "Stunned - damage reduced by 20%+" and the Ranger's ability applies "Marked - takes 15% more damage". Nothing in the damage code ever checks for either effect.
- **Companion attacks skip the damage rules.** `CompanionBase.AttackEnemy()` rolls weapon damage and applies it directly: enemy defense is ignored, and so are any effects on the enemy. It is handed the damage calculator and does not use it.
- **Companions are not saved.** Nothing in `game_logic/data/` mentions companions, so the party is lost on load (already a ❌ line under Save System item 15 - noting that it is a full gap, not a partial one). The new control toggle (Tier 1) would be lost the same way.
- No way to remove a companion: `LeaveParty()` and `RemovePassiveBonus()` exist and are never called (Tier 1 already lists the leave-party trigger).
- Companion levelling uses its own curve (100 XP, +15% per level) with a comment saying it matches the player's (120 XP, +3.5% per level). Level cap of 100 written in. Goes with Bucket A #12. **Decided 2026-10-14: companions use exactly the player's XP system (XP rework to-do 6).**
- `NPCBase.EquipWeapon()`/`EquipArmor()` exist for companions - the starting point for the party menu's "swap gear on a companion". Like the player's, they do not touch the inventory.
- `party_manager.cs` - small and clean. Fixed party of 4, with the dormant Leadership check left in place as decided.

**Player (`player/`)**
- `player.cs` - in use. Starts at 20 health (before the class bonus), gains a random 1-6 health per level using its own random number source instead of the game's `RNGManager`. No level cap on the player.
- **Most of the class system is already not applied.** `GetEffectiveDefense()`, `GetEffectiveAccuracy()` and `GetEffectiveAbilityPower()` are never called, and combat calls the general speed method, not the player's class-aware one. What a class actually does today: the health and speed bonus when the character is created (everyone is a Warrior by default), and the bonus/penalty messages printed when armor is equipped. Useful for Bucket A #13: taking classes out of the MVP is a smaller job than expected - those two spots plus the save file.
- `player_inventory.cs` - in use. Holds 100 items; listing, sorting and filtering are fine as the data source for the party menu. `DisplayInventory()` and `AddItem()` print to the console.
- `player_class.cs` - post-MVP reference, left alone (Bucket A #13).

**Shop keeper, blacksmith, quest giver (`NPCs/`)**
- `shop_keeper.cs` (658 lines), `blacksmith.cs` (380), `quest_giver.cs` (265) - the rules and the console screen are mixed together in each (294 console calls between them, typed input, a one-second pause for effect in the blacksmith). The rule parts are reusable as they are: `BuyItem`, `SellItem`, `CalculateBuyPrice`, `CalculateSellPrice`, `RestockShop`, `GetUpgradableWeapons`, `GetUpgradableArmor`. `UpgradeWeapon`/`UpgradeArmor` have the typed "Y/N" confirmation inside them and need that pulled out. The menu parts get rebuilt in Godot, same as `menus/`.
- No shop keeper or blacksmith is ever created (already logged). The quest giver is created at game start but cannot be opened.
- `NPC_base.cs` - every NPC carries companion fields and shop fields whether it uses them or not. Its `AttackTarget()` is an older copy of the companion attack and is never used. `NPCType` already has `Innkeeper`, unused - relevant to the planned inn-based rest (Tier 4).

**Across the folder**
- `entity.cs` - base for player, enemies and NPCs, in use. `Execute()` must be written by every entity type and is a placeholder in all of them; only a test calls it. Re-applying an effect that is already active only resets its timer - a stronger version of the same effect is thrown away.
- Two different things are both named `CombatAction` (a list in `entities/`, a class in `combat/`), which forces long-hand names in `boss_mechanic.cs`. Rename one.
- **Console output**: about 430 console calls in this folder. Taking damage, healing, equipping, levelling up, effects starting and ending and companion attacks all print to the console instead of the combat log.
- **Tests**: `PlayerTests` only. Nothing for enemies, bosses, companions, the party, the shop or the blacksmith (already on the missing-tests list).

- **TO DOs decided 2026-10-12 from the `entities/` review** (all ❌ Not Started; the work happens in later sessions):
  1. **Give Skarn (and every future boss) real stats.** Nothing calls `BossEnemy.InitializeStats()`, so the tutorial boss has 0 health and 0 damage. Fix where bosses are created so stats are always set, then tune Skarn against the player's starting stats (see to-do 2).
  2. **Review the player's starting stats.** The player starts at about 20 health; a level 2 boss built from the current formula has 160 health and hits for 16-30. Tune the two together. Tied to the difficulty idea in Bucket A #15.
  3. **Random names for regular enemies.** Every time the player enters combat against anything that is not a boss, each enemy gets a random name (Tom, Bob...), picked fresh for that encounter. Bosses keep their own names. This also fixes a real problem: two goblins in one fight are both just "Goblin" in the combat log today. To settle when building: the list of names (one shared list, or per enemy type), how the name is shown (e.g. "Tom the Goblin", so the player still sees what it is), and no repeated name within one fight. Natural place for it: where the encounter's enemies are created (`GameManager.SpawnEnemy()` / the overworld spawn), keeping the enemy type separate from the display name.
  4. **Wire companion attacks into the real damage rules.** Companion attacks must use their equipped weapon's damage through `DamageCalculator` like the player's do - weapon damage, accuracy and crit, enemy defense, and effects on the attacker and the target. Today `CompanionBase.AttackEnemy()` rolls the weapon's damage and applies it directly. Needed regardless of which companions survive the roster rework (Bucket A #10); the individual companion abilities that do nothing today (Shield Bash's stun, the Ranger's mark) are **not** a priority, since those companions are likely to be reworked.
  5. **One combat decision-maker ("AI") for everything the computer controls.** Enemies and companions both need their actions chosen by the computer; companions can have it switched off per companion (the control toggle, Tier 1), enemies cannot. Today it is in three disconnected places: `CombatManager.DetermineEnemyAction()` decides for enemies from a general behaviour setting; each enemy type's own `DecideAction()` is written and never used; and `CompanionBase.DecideCombatAction()` returns a bare number (0/1/2) that `CombatManager` then acts on. Build one decision step that combat asks for any computer-controlled combatant, returning the same kind of action the player submits, with the per-enemy and per-companion behaviour plugged into it. `enemy_ai.cs` is an empty class today and is the obvious home. Do this together with the control toggle work, since both change the same part of `CombatManager`.
     - **Targeting is part of the decision (added 2026-10-12).** Who a combatant attacks is governed by a targeting behaviour inside the decision-maker, not by a random pick. Each enemy type (and each companion on auto) gets a targeting pattern alongside its action pattern - for example: go for the most wounded, go for the weakest defense, focus whoever hit it last, protect or avoid a particular party member, spread attacks out. Randomness can be one ingredient of a pattern, not the rule. Today there are two fixed rules that apply to everyone: enemies go for the most wounded party member 70% of the time and pick at random otherwise (`CombatManager.SelectEnemyTarget()`), and companions always go for the lowest-health enemy (`SelectCompanionAttackTarget()`). Both move into the decision-maker and become per-type behaviours. Enemies sharing a turn slot each choose their own target this way (to-do 9 under `game_logic/combat/`). The patterns themselves still need designing.
  6. **Companions that fail to create** (gear names missing from the item database - see `items/`): fix the names so a companion can exist at all. Small, and it blocks testing to-dos 4 and 5.
- Godot-side to-dos from this review are logged under "FUTURE PHASE - GUI & Game Engine": more enemy types, replacing the console output, and rebuilding the shop keeper / blacksmith / quest giver screens.
- `entities/` has been presented and discussed.

#### `game_logic/combat/` (first pass 2026-10-12 - 4 files, about 1,750 lines)
_Found by reading the code only - nothing was built or run._
- **How a fight runs today**: `CombatManager.StartCombat()` takes the player, a list of enemies and the companions. Each round, everyone is sorted by speed and acts once. The computer plays enemies and companions; combat stops and waits only on the player's turn, until `SubmitPlayerAction()` is called. Victory is every enemy dead; defeat is the player at 0 health. The structure is sound and is the right base for the Godot combat scene.

**Rules that do not work the way they read**
- **Fleeing from a regular enemy ends the game - confirm on a build.** A successful flee ends combat with "not a victory". `GameManager.TriggerCombatEncounter()` treats anything that is not a victory as a defeat and goes to Game Over. The boss fights check the player's health first and do not have this problem. Combat needs to report three outcomes (won, lost, fled), not two.
- **An enemy that flees ends the fight as if the player had fled.** `ProcessFlee()` is written for the player only. Enemies with the "Cautious" behaviour can choose Flee; the result would be "You escaped, but gained no rewards" and the fight over. No enemy uses that behaviour today, so it cannot happen yet - fix it as part of the whole-party flee rule (Tier 1).
- **Defend usually protects against nothing.** Defending halves damage until the start of the next round, when every defend is cleared. So it only covers enemies that act *after* the defender in the same round. A player slower than the enemies (starting speed 10 against a goblin's 12) gets no protection from Defend at all, only next round's speed bonus. The message says "Next incoming attack will deal reduced damage!". Decide what Defend should cover (until the defender's next turn is the usual answer).
- **One-turn buffs wear off before they can be used.** Effect timers count down at the end of every round, including the round the effect was applied in. A buff with a duration of 1 - Attack, Defense and Critical potions at levels 1-5, and the Combat Elixir - is gone before the player's next attack. A 3-turn ability buff (Attack Boost) covers 2 attacks, because using it takes the first turn. Cooldowns lose a turn the same way.
- **Enemy XP and gold values are never used.** Every enemy and boss sets `XPValue` and `GoldValue`; combat ignores both. XP is rolled as 30-70 x the enemy's level, and gold comes from the loot tables. A boss's "massive XP, generous gold" and its reward scaling therefore change nothing.
- **Small percentage bonuses round down to nothing.** Damage is whole numbers and starts in single digits, so +1% or +10% of a 5-damage hit is still 5. This is a second reason the low-level abilities do nothing (see `abilities/` and the ability redesign idea in Bucket A #12).
- The passive healing bonus for Heal and Defense Boost (`DamageCalculator.CalculateHealing()`) is never applied - only tests call it.

**What the companion and party decisions run into here** (Tier 1 control toggle, whole-party flee, shared party bag, and to-dos 4 and 5 under `entities/`)
- Companions do not go through the action pipeline the player uses. Their turn is handled separately in `CompanionTurn()`, and `ProcessAttack()` does nothing if the attacker is a companion. A directly controlled companion's Attack would be silently ignored until this is unified.
- `DamageCalculator` has exactly two cases: player attacks enemy, and enemy attacks player. Both are written for the `Player` type. An enemy hitting a companion uses a short separate formula with **no hit roll and no critical hit** - it never misses. Wiring companions into the real damage rules means making these two methods work for any attacker and any target.
- **The fight ends the moment the player falls, even if companions are still standing.** Revive potions therefore only ever work on companions. With directly controlled companions this needs a decision: does the fight continue while any party member is up, so a companion could revive the player?
- Item use applies every ordinary consumable to the player (already logged), and removes used-up items from the player's inventory - correct for a shared party bag.
- Boss fights other than the tutorial are started without companions (`ChallengeBoss` and `ChallengeFinalBoss` pass none, with a "TODO: add companion support" note).
- The critical hit multiplier is a fixed 1.5 written in three separate places (player, enemy, companion). Answers the open question in Tier 2 for the Captain of the Royal Guard: it needs to become a per-attacker value.

**What the combat screen cannot get from `CombatManager`**
- The log is one stream of text lines. The screen cannot tell a damage number from a heading, so it cannot show floating damage, flash a health bar on a hit or play a sound for a critical. Combat needs to report *what happened* (who hit whom, for how much, miss, critical, effect applied) alongside the text.
- Not available to a screen at all: the round number, the turn order (only written into the log as text), who is defending, whose turn it is, which abilities and items can be used, and the reward summary (XP, gold, items) at the end.
- `SubmitPlayerAction()` checks nothing: a dead or missing target, an ability on cooldown or an item the player does not have are all accepted. A submission made when combat is not waiting is dropped silently.
- `Console.Clear()` in three places - the likely cause of the dead Attack button (logged under `menus/` and "Combat scene MVP").

**Leftovers**
- `turn_manager.cs` (about 200 lines) is not used. `CombatManager` creates one and sets it up, then runs its own turn queue; the file's own comment calls it kept "for backwards compatibility". `TurnManagerTests` tests this unused class.
- `ShowCombatStatus()` is never called. `DamageCalculator.CalculateDamage()` is only called by tests.
- Flee is a flat 30% chance for everything, with no stat involved. **Replaced 2026-10-14 - see "Combat code pass" below.**
- Every living companion receives the full fight XP (already noted); a companion that is down at the end gets none.
- Loot is added straight into the inventory; if it is full the item is lost with only a console line.
- Nothing is reported to statistics or quests from combat (already logged).
- **Tests**: `CombatManagerTests` is known-broken since the rework (already logged); `DamageCalculatorTests` covers the two existing damage cases.

- **TO DOs decided 2026-10-12 from the `combat/` review** (all ❌ Not Started; significant changes to combat, to be made in later sessions):
  1. **Combat needs more than two endings.** Today it reports only "victory" or "not a victory". The endings it needs:
     - **Won** - full rewards (as today).
     - **Lost** - Game Over (as today).
     - **Opponent fled, or a draw** (NEW) - the player and companions receive **50% of the XP and loot** they would have had for a win, then the game returns to normal overworld play.
     - **The party fled** - returns to overworld play. Must stop leading to Game Over (the current bug in `GameManager.TriggerCombatEncounter()`). _Assumed, to confirm: no rewards for the party's own flee, as the current "You escaped, but gained no rewards" message says._
     - To settle when building: what counts as a draw (not defined yet); and when only some of the enemies flee and the rest are defeated, whether the fled ones pay 50% and the defeated ones 100%.
     - Enemy fleeing must become its own thing - `ProcessFlee()` treats any flee as the player's today.
  2. **Defend lasts until the defender's next turn** - the same rule for the player, companions and enemies. It halves every attack that lands on them in that window: the rest of the current round and the start of the next, so one Defend can cover two or more hits when several enemies attack. Replaces "cleared at the start of every round". **Added 2026-10-14**: if the defender's next turn is skipped (the Princess's forced skip, or any later skip effect), the Defend still ends at the moment that turn would have come - a skip never extends it.
  3. **Enemy XP and gold values become the guaranteed base reward.** Each enemy's `XPValue` and `GoldValue` is the minimum the player gets for defeating it. The XP roll (30-70 x level today) and the gold from the loot table are added on top as the random extra. Check the totals when wiring this - the current rolls were sized as the whole reward, not a bonus. Bosses' reward scaling starts to matter once this is in. **Base plus random top-up confirmed for all XP rewards 2026-10-14 - design and do together with Bucket A #12, XP rework to-do 4.**
  4. **Companions go through the same action pipeline as the player.** `CombatManager` must stop handling companion turns on a separate path and must stop ignoring companion attacks, since the player can take control of any companion's choices (control toggle, Tier 1). One pipeline for player, companion and enemy actions; the decision-maker (to-do 5 under `entities/`) only chooses the action when the computer is in control.
  5. **Companion damage works exactly like the player's** - rolled from the weapon they hold, with accuracy, crit, the target's defense and active effects (same as to-do 4 under `entities/`). `DamageCalculator` becomes "any attacker against any target" instead of its two player-only cases. This also gives enemy attacks on companions a hit roll and a crit roll.
  6. **Boss fights include the party.** The first boss fight has one companion (the tutorial already passes the party in). `ChallengeBoss` and `ChallengeFinalBoss` pass no companions today and need the same.
  7. **Critical hit multiplier: 1.5x by default, changed only when something says so** (the Captain of the Royal Guard's signature, the Executioner ability). Make it a value on the attacker, defaulting to 1.5, instead of the number written in three places.
  8. **Combat Elixir is leaving the item list** - it will be reworked into the magic system as a spell that needs components (Tier 3), not an elixir. When the magic system is designed, **how long spells and buffs last must be settled with it**, including the timing problem found here: effect timers count down at the end of the round they were applied in, so a 1-turn effect is gone before it can be used.
  9. **Eight turn slots per round - 4 for the party, 4 for the opponents (decided 2026-10-12).** Not in the code at all today: every combatant gets its own turn each round, so a fight against 10 enemies plays out 10 enemy turns.
     - A round always has 8 turn slots. The party has 4: the player and up to three companions, one slot each. The opponents have the other 4, **however many enemies there are**.
     - **More enemies than slots**: enemies are paired up or grouped into a slot and take their turn at the same time. Each still does its own full action and its own damage - nothing is reduced, the turns just happen together. The point is to keep a round short.
     - **Fewer than 4 on a side**: the unused slots are labelled as skipped and combat moves straight on to the next slot.
     - **Slot order is still decided by speed** - that is not changing (decided 2026-10-12).
     - **Enemies are grouped mainly by speed** (decided 2026-10-12). Known complication to work out in the code: speed changes every round with the action each enemy took (attack slows, defend speeds up), so enemies in one slot that chose different actions drift apart and the groups have to be rebuilt at the start of every round. Regrouping each round is the preferred behaviour in most cases.
     - **Enemies sharing a slot each pick their own target** (decided 2026-10-12). **Corrected the same day**: the choice is made by the combat decision-maker's targeting behaviour, not by plain randomness - see to-do 5 under `game_logic/entities/`.
     - **How the grouping works (decided 2026-10-12)**: the **fastest enemies keep a slot to themselves; the slowest are the ones paired up**. A shared slot takes its place in the turn order at the **average speed of its members**. Example: 5 enemies with speeds 14, 14, 14, 10 and 6 - the three at 14 each get their own slot, and the 10 and the 6 share the fourth slot with an effective speed of 8.
     - **Larger groups (decided 2026-10-12)**: spread the enemies **evenly across the 4 slots, and give any extras to the slowest slots first**. 8 enemies: two in each slot. 9 enemies: the slowest slot has three, the three faster slots keep two each. 10 enemies: the two slowest slots have three each. The 5-enemy example above is the same rule (one each, the extra goes to the slowest slot). Enemies are sorted by speed first, so the fastest fill the fastest slots - confirmed 2026-10-12, unless a different way of separating them is designed later.
     - **Rounding (decided 2026-10-12)**: when a slot's average speed is not a whole number, always round **up**.
     - **Planned maximum of 8 enemies per encounter (2026-10-13)**, so in practice no slot holds more than two; the 9 and 10 examples above only apply if that maximum is raised. See to-do 13 under `core/`.
     - To work out in the code: ties in speed (enemies with equal speed, and a shared slot tying with a party member).
     - Touches: the round and turn-queue code in `CombatManager` (`BeginRound`, `AdvanceTurns`), the party size (fixed at 4 for the MVP, which matches; the dormant Leadership ability raises it above 4 and would not fit), the combat decision-maker (to-do 5 under `entities/`), and the screen's turn-order display, which shows 8 slots including the skipped ones (item 7 in "Combat scene - what the screen has to show").
- **Combat code pass (2026-10-14) - written, NOT built or run yet (no local build; build and test on the other machine).** Getting combat ready for the Godot test scene and in line with the decisions above:
  - **New decisions this session:**
    - **The fight goes on while anyone in the party is standing.** Lost = the whole party is down. A fallen player's turn is skipped until revived.
    - **Party flee: no rewards. No draw in the MVP. Mixed endings pay per enemy** (defeated 100%, fled 50%, the fight counts as won). Only "every enemy fled" is the 50% "opponent fled" ending.
    - **Flee chance is per enemy type** (`EnemyBase.FleeChance`, 30 until each enemy is tuned). One number per enemy, used both ways: an enemy fleeing rolls its own chance; the party fleeing a group rolls the **lowest** chance among the enemies still standing (the hardest one to escape decides). **No fleeing from boss fights.** A party member fleeing takes the whole party.
    - **Companions on auto never flee** - they fight until the party wins or they go down. Only a directly controlled companion can choose Flee.
    - **Bombs are no longer consumable items.** Combat refuses them. **To do:** remove the bomb items from `item_database.cs` and the `Bomb` consumable type (kept for now so nothing else breaks).
    - `ShowCombatIntro` removed - the screen reads names, levels, health and effects from `CombatManager.Party` / `Enemies` at any time, so name labels and the bottom portrait panel can stay up all fight.
  - **Built in this pass:**
    - To-do 1 (endings): `CombatOutcome` Won / Lost / PartyFled / OpponentFled and a `CombatResult` (XP, gold, items, defeated and fled enemies, levels gained) through the new `CombatFinished` event. Fleeing no longer leads to Game Over. Enemies can flee on their own. The old `CombatEnded(bool)` still fires (true for won / opponent fled) so the current Godot scene keeps working.
    - To-do 2 (defend until the defender's next turn), including the skip rule.
    - To-dos 4 and 5 (one pipeline, one damage path): `DamageCalculator.CalculateAttack()` handles any attacker against any target with the same steps; companions go through the same action pipeline as the player; enemy attacks on companions now roll to hit and crit and go through armor. Companion attacks now go through enemy defense too (they ignored it before), so companions hit a little softer.
    - To-do 6: boss fights include the party (`ChallengeBoss`, `ChallengeFinalBoss`).
    - To-do 7: `Entity.CritMultiplier` (default 1.5) replaces the three hard-coded 1.5s; Executioner still overrides it for the player.
    - To-do 9: the 8 turn slots per round (4 party, 4 opponents, grouping by speed, extras to the slowest slots, average speed rounded up, empty slots skipped). `CombatManager.TurnOrder` exposes them.
    - What the screen needs: `CombatEventOccurred` (one event per hit, miss, evade, damage, heal, defeat, revive, defend, effect, ability, item, flee, turn and round), `CurrentActor`, `CurrentRound`, `IsDefending()`, `HasFled()`, `GetValidTargets()`, `CanUseAbility()`, `GetUsableItems()`, `GetPartyFleeChance()`. `SubmitPlayerAction()` now checks the action (turn, target, cooldown, item in the bag, no boss flee) and returns false with an `ActionRejected` event instead of accepting anything.
    - Companion direct control: `SetDirectControl()` - combat waits for the player on that companion's turn. Not saved yet.
    - Items: any party member can use a consumable from the shared bag on any standing party member (`Consumable.UseOn`); revives on fallen party members.
    - Boss switches from Bucket A #11: the 15%/50% multipliers are no longer applied, the repeat-fight warning is off, and a beaten boss can't be challenged again (code kept, commented). **Bosses now have stats** - they had 0 health before (nothing called `InitializeStats`), so every boss fight was an instant win. Stopgap: the generic boss formula at the boss's fixed level. Skarn (level 2) comes out at 160 HP and 16-30 damage, which is probably far too strong for a level-1 tutorial - check on the first test.
    - Bug fixed on the way: Divine Light (healer companion) healed the enemy it was "targeting"; support abilities now go to the most wounded ally.
    - Removed: the `Console.Clear()` calls in combat and boss encounters, and the `Console.ReadKey()` after the final victory screen (it would freeze Godot).
    - Tutorial: only the player's actions count toward "try each action"; a refused action no longer counts as tried.
  - **My defaults, to confirm:**
    - Speed ties (between enemies, and between slots) are broken at random.
    - If the party wins while the player is down, the player gets back up with 1 HP. A companion who is down at the end stays down and gets no XP (as before).
    - An enemy that flees drops each of its loot items with a 50% chance (gold and XP are halved exactly).
  - **Still to do in combat:** reposition action (layout not decided); the Princess's skip-turn and two-turns-per-round; enemies joining mid-fight; buff/cooldown timing (with the magic system); enemy accuracy by difficulty (Bucket A #16); XP numbers and the `XPValue` base reward (to-do 3, with the XP rework); rewriting `CombatManagerTests.cs` for the new engine; saving the companion control toggle.
- **Ideas noted, not decided (2026-10-12)**:
  - **Percentage bonuses that do not always round down.** Instead of dropping the fraction, use it as a chance to round up: +10% on a 5-damage hit is 5.5, so the hit lands as 5 or 6. Adds a little randomness and keeps the ability's strength curve meaningful at low numbers. Applies wherever a percentage is applied to damage, defense or healing.
  - **Or (also) raise the whole numbers** - bigger health and damage values so the effect of an ability is easier for the player to see. Goes with the player starting-stats review (to-do 2 under `entities/`) and the ability redesign (Bucket A #12).
- ~~**Still open from this review**: whether the fight continues when the player falls but companions are still standing.~~ **Decided 2026-10-14: it continues** - see "Combat code pass" above.
- Godot-side to-dos from this review are logged under "FUTURE PHASE - GUI & Game Engine" in "Combat scene - what the screen has to show".
- `combat/` has been presented and discussed.

#### `game_logic/core/` (first pass 2026-10-12 - 4 files, about 1,250 lines)
_Found by reading the code only - nothing was built or run._
- **What this folder is**: `game_manager.cs` is the hub that owns every system and the game's current state; `game_startup.cs` builds a new game or loads a save; `tutorial_manager.cs` runs the opening tutorial; `game_state.cs` is the list of states. The half-converted shape is as expected: the methods Godot will call exist and are marked `TODO-GODOT`, and the old console menus are commented out beside them.

**Tutorial (`tutorial_manager.cs`)**
- **The tutorial can get stuck - confirm on a build.** The Skarn fight makes the player try Attack, Ability, Item and Defend once each before any can be repeated. A new player starts with **no items** (`CreateNewGame` gives a weapon and an ability only), so the Item step cannot be done properly, and after one Attack, one Ability and one Defend every further action is refused. The only way through today is submitting "use item" with no item, which combat accepts and logs as an error. Fix: give the player a starting item (a health potion fits the lesson), and/or drop a step when it is not possible.
- **The Ability step teaches nothing for most choices.** 15 of the 19 abilities are passive; "using" one prints a message and wastes the turn. Goes with the ability redesign idea (Bucket A #12) - if every ability is activated in battle, this step works for all of them.
- The tutorial rule only looks at what kind of action was submitted, not who it was for. With directly controlled companions (Tier 1), the companion's actions do **not** count toward the lesson (decided 2026-10-13, to-do 3 below).
- Flee is refused during the tutorial with a story line - a good pattern. Under the whole-party flee rule the same refusal must apply to a companion choosing Flee.
- Otherwise this file is the cleanest in the folder: no console calls, messages go out through an event (`TutorialMessage`). It is the model for how the rest should report things to the frontend.

**New game and load (`game_startup.cs`, `GameManager.StartNewGame`/`LoadGame`)**
- **Things carry over from one game to the next in the same run.** The party is created once when the program starts and never cleared, so starting a new game or loading a save keeps the companions from the previous session. Statistics are only replaced on load, so a second new game continues the old numbers. The list of spawned overworld enemies and the paused-state stack are not cleared either.
- **The tutorial is not part of the save.** Loading always goes straight to normal play, and `SaveGame()` can be called at any moment, including mid-tutorial and mid-combat. Decide when saving is allowed (ties to the inn-based rest in Tier 4) and record tutorial completion in the save.
- Losing the tutorial fight leads to Game Over, whose first option is "Load Last Save" - there is no save yet at that point.
- A new player starts with a weapon, an ability, 0 gold, no armor and no items. Part of the starting-stats review (to-do 2 under `entities/`).
- `CreateNewGame` creates every player as the default class (Warrior) - one of the two places to change when classes come out of the MVP (Bucket A #13).
- The difficulty descriptions shown at character creation promise effects that do not exist ("Enemies have 75% stats" and so on) - difficulty changes nothing today (already logged). Also the naming: "Normal" is described as the easy setting and "Hard" as "the intended experience". Settle the names together with Bucket A #15.
- About 160 lines of `game_startup.cs` are the quest list written out by hand (`InitializeQuests`). It belongs with the quests, and it is rewritten anyway by the quest to-dos (story-order unlocking, per-town job boards).

**Game manager (`game_manager.cs`)**
- **A regular encounter is always exactly one enemy.** `TriggerCombatEncounter()` starts every fight with a single enemy; the only fight with more than one is the Godot test scene's two goblins. Combat itself handles several. The 8-turn-slot design (to-do 9 under `combat/`) assumes groups, so something has to decide how many enemies an encounter has - per area, per overworld spawn, or both. Add to the "more enemy types" work.
- **The chest/loot event stops and waits for a key press** (`TriggerLootEvent()` ends in `Console.ReadKey()`), which fails without a console. It also prints what was found instead of handing it back, so a loot popup has nothing to show. It is described in the code as working for Godot; it does not yet.
- **A Godot scene cannot ask what state the game is in.** There is a `StateChanged` event but no way to read the current state, and (already logged) no access to the player, party, map, quests, settings or statistics - only `Combat` and `Tutorial` are reachable.
- `Run()` is the old console main loop. In Godot the engine owns the loop and nothing should call it; if it were called it would spin forever, because the main menu, game loop and pause menu it calls are now empty. `HandleGameOver()` is still a live console menu with typed input. `Explore()` and `Rest()` are private and only reachable from the commented-out menu - `Rest()` is being replaced by the inn (Tier 4).
- `ChallengeBoss()` and `ChallengeFinalBoss()` have no callers (waiting on boss placement on the map). When wired: neither switches the game into the Combat state the way regular and tutorial fights do; both pass no companions (to-do 6 under `combat/`); and if the boss turns out to be locked, the "fight ended" listener they attached stays attached and would fire on the next unrelated fight.
- **There is no ending.** Beating the final boss prints two lines to the console and play continues. No end-of-game state or screen exists. Needs designing with the story.
- Fleeing a regular fight leads to Game Over (logged under `combat/`).
- The save slot name "save1" is written in two places - fine for the one-slot MVP.

**Game states (`game_state.cs`)**
- 13 states. Never used: `Shopping`, `Dialogue`, `Cutscene`, `Settings`, `Loading`. Missing for screens that are now planned: the party menu (replaces `Inventory` and `Stats`, per the one-menu decision), statistics, quest log / job board, blacksmith, and an ending. Tidy the list once the Godot scene list is written.

**Across the folder**
- 96 console calls, counting the ones inside the commented-out menus.
- **Tests**: none for this folder. `GameStartup` was written to be testable on its own (it takes everything it needs as inputs) and has no tests yet.

**To-dos and decisions from discussing `core/` (2026-10-13)**
  1. **Give the player a healing item for the tutorial fight.** Add it to the inventory when the tutorial fight is set up (`GameManager.BeginEagleBearEncounter`), so the Item step has something real to use. Use the existing "Minor Health Potion" (decided 2026-10-13; potion names may be reworked later, not decided). To settle when building: if the player is at full health when the Item step comes up, the potion does nothing visible, so the lesson may need Skarn to land a hit first.
  2. **The tutorial must not be losable.** Likely form: the first companion revives the player once. Depends on two things that are not true today: the fight has to continue when the player falls while a companion is standing (it ends at once - open question under `combat/`), and companions carry no items (shared party bag), so this is either the companion using a "Minor Revival Potion" from the shared bag or a one-off scripted revive that needs no item. Also removes the "Load Last Save" problem at Game Over, because the tutorial never reaches Game Over.
  3. **A companion's actions do not count toward the "try each action" lesson (decided 2026-10-13).** Only the player's own actions tick the list. `TrySubmitAction` has to check who the action is for.
  4. **Ability step** - no separate to-do; handled by the ability redesign (Bucket A #12).
  5. **Starting a new game wipes everything from the previous game.** Player stats, inventory, the party (every companion gone until recruited again), statistics, quests, boss progress, the spawned overworld enemies and the paused-state stack. Loading a save does the same wipe first and then fills in from the file. Today the party and (on a new game) the statistics survive.
  6. **Statistics start fresh for every new save file.** Part of to-do 5, listed separately so it is checked on its own: a new file shows a clean statistics page.
  7. **Recruited versus active party members.** Today there is one list (`PartyManager.ActiveCompanions`) and everyone in it fights. Needed: a roster of everyone recruited, each marked as **in the fighting party** (appears in combat and takes a party slot, max 3 companions) or **recruited but not fighting**. The party menu moves companions between the two. The roster and the marks must be saved - companions are not saved at all today.
  8. **Keeping things up to date between saves.** No new system should be needed for this: the game already holds the live player, party and statistics in memory the whole time it is running, and a save is only a copy of that written to disk. What is missing is (a) the game actually recording into the statistics as things happen (logged under "Quests, statistics and settings are built but not connected"), (b) the roster from to-do 7, and (c) the wipe from to-do 5. With those three the statistics screen and the party are always current, saved or not. Revisit only if "return to just before the fight" is chosen in to-do 11 - that one does need a snapshot taken when a fight starts.
  9. **Save that the tutorial is complete.** Add it to the save file so loading never sends the player back through the tutorial. If saving is impossible until the tutorial is over (to-do 10), the flag is simply always true in a save - still worth storing so the rule is not hidden.
  10. **Saving is only allowed in certain places (reminder 2026-10-13 - already the design).** The player has to go out of their way to save. Nothing in the code enforces it: `SaveGame()` works at any moment, including mid-combat and mid-tutorial. **Decided 2026-10-13: save points are the only way to save. A save point is an inn or one of a set of specific campsites, clearly marked for the player.** Needed: a "can the player save here?" check tied to the map (inns and the marked campsites - the campsites have to be placed when the fixed map is designed), and the Godot save option only offered there. **Auto-save is out of the MVP** (possible post-MVP feature): the auto-save and quick-save code and the auto-save setting stay dormant and must not be offered in the settings screen.
  11. **Open - where a defeat sends the player (not decided 2026-10-13):** back to the last save point, or back to just before the fight began. The second needs a snapshot taken at the start of every fight (see to-do 8) and makes save points matter much less.
  12. **Rewrite the difficulty descriptions to fit the current names** (Normal / Hard / Difficult / Unfair, renamed 2026-09-10). The text in `game_startup.cs` lines 208-211 was written for the old names and promises stat and reward percentages that are not applied. Goes with "Difficulty selection has zero mechanical effect" and Bucket A #15.
  13. **Review how encounters are sized, then add an encounter-size roll if needed.** Checked 2026-10-13: the overworld spawn work randomises **how many enemies are walking around an area** (`SpawnOverworldEncounters`, a range per location type such as 1 to 3) and each one's aggression and detection range. Each of those walking enemies is exactly **one** enemy when the fight starts (`OnOverworldEnemyHitboxCollision` passes the single enemy to `TriggerCombatEncounter`). Nothing randomises the number of enemies inside a fight, so the roll does need adding. **Planned maximum: 8 enemies in any encounter** (may change with difficulty or other things not yet decided). With 4 opponent slots that is at most two enemies per slot; the 9- and 10-enemy grouping examples under `combat/` only matter if the maximum is raised. **Decided 2026-10-13**: the two numbers are separate on purpose. The enemies walking around an area and the enemies in a fight are not directly related; the model is *Mario & Luigi: Dream Team* - touch one enemy on the map, the scene changes to combat, and the combat scene shows however many enemies that encounter has. **Encounter size depends on three things, all of which affect it: the enemy type, the player's level and the difficulty.** To design: the actual numbers per enemy type and how level and difficulty shift them; and whether the group is all one enemy type or the touched enemy plus other types from the area's spawn list (ties to "Add more enemy types").
  14. **Godot picks its scene from the game state.** Two parts. First the missing piece: Godot cannot read the current state today - there is a `StateChanged` event only - so add a public "current state" value on `GameManager`. Then one place on the Godot side (a scene switcher) that holds the list "this state shows this scene" and changes scene whenever the state changes, including on start-up and after a load.
  15. **Chests hand their contents to Godot and put them in the inventory, with no console.** `TriggerLootEvent()` returns what was found (items and gold), adds it to the inventory, and neither prints nor waits for a key. Decide what happens when the inventory is full.
  16. **Ending cutscene and credits.** Beating the final boss moves the game into an ending state that plays the ending and then the credits, then returns to the main menu. Needs an ending state (none exists) and the story content.
  17. **Properly implement the five unused game states** - `Shopping`, `Dialogue`, `Cutscene`, `Settings`, `Loading`. For each: what enters it, what leaves it, whether it is an overlay that resumes the previous state or a fixed state, and which Godot scene it shows (to-do 14). `Cutscene` is needed by to-do 16. Add the missing ones at the same time (party menu, statistics, quest log / job board, blacksmith, ending).
  18. **New combat action: reposition / formation change (decided 2026-10-13).** The player can swap battle positions with a companion, and a companion can swap places with another party member. Combat has no idea of position today, so this needs the position system first (the coordinate-grid layout still to be specified under "Combat scene MVP"). **Decided 2026-10-13: swapping uses up the turn of whoever chose it. Position changes how enemies treat a party member: closer to the enemy side means more likely to be attacked; farther away means harder to hit - enemies get reduced accuracy against them.** This plugs into two existing to-dos: the "who do I attack" choice is the combat decision-maker's targeting (to-do 5 under `entities/`), which now has to weigh position, and the accuracy penalty goes into the generalised hit roll (to-do 5 under `combat/`). To settle: how many positions there are (a front and a back row, or a line of four), the actual numbers (how much more likely, how much less accurate), whether position also affects the party's own attacks or enemy positions, and the starting formation and whether it is set in the party menu. **Also decided 2026-10-13: the partner who is swapped with does not lose their turn - they just move. Reposition is a regular combat action (a fifth button beside Attack, Ability, Item and Defend) and a fifth action in the tutorial's "try each action" lesson** - add it to `RequiredActionTypes` in `tutorial_manager.cs` and to the action buttons in "Combat scene - what the screen has to show". The tutorial fight has one companion, so there is always someone to swap with.
- `core/` has been presented and discussed.

#### `godot_project/` (first pass 2026-10-13 - 2 scenes, 3 scripts with code, 9 empty scripts)
_Found by reading the files only - nothing was built or opened in Godot._
- **What this folder is**: the real Godot project. Only the combat test scene has anything in it.
- **Nine script files are completely empty** (0 lines): `UI/UIManager.cs`, `UI/HUD/CombatHUD.cs`, `HealthBar.cs`, `InventoryHUD.cs`, `UI/menus/MainMenu.cs`, `PauseMenu.cs`, `SettingsMenu.cs`, `scripts/EnemyController.cs`, `PlayerController.cs`. They are name placeholders carried over from `godot_integration/`. Their names already disagree with decisions made since (one party menu instead of an inventory screen; the health bar lives inside `EntitySlot`). **Decided 2026-10-13: leave everything in `godot_project/` as it is.** A full planning session for the Godot frontend comes first ("Plan the Godot frontend file structure and file contents" under FUTURE PHASE); what stays and what goes is decided once that design is final.
- **The project has no starting scene.** `project.godot` sets no main scene, so pressing Play (F5) asks which scene to run; only "run this scene" (F6) works. Fine while there is one test scene; set it when a main menu exists.
- **Nothing connects Godot to `GameManager`.** There is no single shared game object (Godot calls this an autoload) that creates the `GameManager` and keeps it alive between scenes. Every screen needs it; it is the first building block, and the home for the scene switcher (to-do 14 under `core/`).
- **The combat scene builds its own fight and bypasses `GameManager`.** `CombatSceneController` creates its own player ("Hero" with a Rusty Sword), two goblins and its own `CombatManager`. Right for a first test; it means the scene knows nothing about the real party, inventory, game state, tutorial rules or what happens after the fight. When the shared game object exists, the scene should receive the fight from `GameManager` and submit actions through the tutorial check during the tutorial.
- **The dead Attack button - probable cause (logged under `combat/`, not confirmed).** `CombatManager.StartCombat` calls `Console.Clear()` before its first message (`combat_manager.cs` line 182). Without a console window that call fails, which would stop the fight before it starts and leave the log empty - both symptoms from one cause. Check Godot's Output panel for the error.
- **The scene has Attack, Defend and Flee only** - no Ability or Item button, no way to start another fight when one ends, and nothing from the "what the screen has to show" list. It redraws health only after the whole chain of computer turns has finished (the timing issue already logged).
- **Combat text is shown as formatted text.** The log treats square brackets as formatting codes, so any message containing them (boss labels such as "[BOSS]") may lose that part. Goes away when combat sends structured events instead of finished sentences.
- **`MapController.cs` is out of date.** It starts a fight when the player steps on a map square marked as having an enemy - the old chance-per-square model that the overworld spawn work replaced (enemies walk around and a fight starts on touch, `OnOverworldEnemyHitboxCollision`). It is also never given a `GameManager`, so it does nothing. Rewrite with the map scene.
- `EntitySlot` is a reasonable reusable piece (name, health bar, hover details, select). Its comment says a fight can have "1-8 allies"; the party is 4.

#### Root files and `tests/` (first pass 2026-10-13)
_Found by reading the files only._
- **The console program can no longer run the game.** `Program.cs` calls `GameManager.Run()`, the old console loop, which now spins forever because the menus it calls are empty (logged under `core/`). `Program.cs` and the root `RNGTheGame.csproj` exist only for that. **Decided 2026-10-13: keep `Program.cs`, but cut it down** - it is no longer the main way into the game. To do: stop it calling `Run()`, and settle what the small version does (for example start a new game and run one test fight, or just print that the game is played through Godot). The root `RNGTheGame.csproj` stays with it. Update the README's "dotnet run" instructions to say what it now does.
- **Five of the eleven test files are switched off** in `tests/GameLogicTests.csproj` because they no longer compile against the current code: `DamageCalculatorTests`, `PlayerTests`, `QuestManagerTests`, `TurnManagerTests`, `SaveManagerTests`. The note beside them says "see conversation/memory" for the reasons, which is not a place anyone else can read - the reasons should be written down here as each is repaired. A sixth, `CombatManagerTests`, compiles but is known-broken. So only five files really test anything today: RNG, leveling, difficulty scaler, game settings, statistics tracker. Two of those five cover systems the game does not use yet (difficulty scaler) or barely feeds (statistics). "Unit tests still missing" above lists 11 files without saying this.
- `TurnManagerTests` tests `turn_manager.cs`, which combat does not use (logged under `combat/`). Delete both together or revive both with the 8-slot turn order.
- **`README.md` is out of date** - it marks whole systems as implemented (difficulty scaling, "comprehensive testing") that this review found unconnected or switched off, and its folder listing predates the Godot project and the `GameLogic.csproj` split. `SRS.md` is the original 2025 requirements document and describes a different game in places (weapon toggle key, gameplay speed keys). Neither was part of the docs clean-up of 2026-10-08. **SRS decided 2026-10-13: keep it, leave it alone for now, and rewrite it once the game is finished so it describes the final product** - a properly documented SRS for this project's history and a detailed reference for the second game. Logged as its own to-do under BUSINESS & LEGAL. **README decided 2026-10-13: its "Project Structure" listing is kept in line with the real file structure** - brought up to date the same day, and to be updated whenever files are added, moved or removed. The rest of the README (the "Key Features Implemented" and "Current Status" sections, the "411 unit tests" figure, "dotnet run") was left as it was and still overstates what works; rewrite those when the game is closer to done.
- **Combat positions** (how many there are, front/back row or a line of four) - put off by the user on 2026-10-13, to be addressed later (to-do 18 under `core/`).
- The three projects and the solution file are set up correctly as far as reading can tell: `GameLogic` is a library with no Godot dependency, and the console program, the tests and the Godot project each reference it. Small naming inconsistency: `RNGTheGame.csproj` versus `RNGtheGame.sln`.
- `game_logic` has strict "may be empty" checking switched on while most of the code was written without it, so a build probably prints a large number of warnings that bury real ones. Confirm on a build; if so, either switch the check off for that project or clean the warnings up folder by folder.
- **The folder-by-folder review is complete (2026-10-13).** `items/` and `abilities/` were presented but never discussed in detail.

### 🔴 Quests, statistics and settings are built but not connected to the game (found 2026-10-09)
- **Status**: ❌ Not Started
- **Description**: Items 6, 8 and 9 above were marked FULLY COMPLETED until 2026-10-09. The classes, menus and save/load are all there, but the game never feeds them:
  - **Quests never progress.** Nothing outside `game_logic/quests/` calls the progress methods (`OnEnemyDefeated`, `OnGoldEarned`, `UpdateLevel`, `OnBossDefeated`, `OnFinalBossDefeated`, `OnWeaponUpgraded`, `CheckEquipment`, `UpdateChallengeProgress`, `QuestManager.CheckQuestCompletion`), and git history shows they never had callers. Killing enemies, earning gold, levelling up or beating a boss does not move any quest forward. They need to be called from combat (`combat_manager.cs`), the player (gold, level, equipment), the blacksmith and the boss encounter.
  - **Quest Giver and Job Board cannot be opened.** Their calls sit in the commented-out console menu in `game_manager.cs`, waiting for the Godot UI.
  - **Statistics record almost nothing.** The only calls are at save time (play time, current gold, current level, save count). Battles, damage, kills, shop visits, purchases, upgrades, item and ability use, quest and NPC interactions are never recorded. The tracker has the methods; they need calling from `combat_manager.cs`, `shop_keeper.cs`, `blacksmith.cs`, `player.cs`, `consumable.cs`, `ability.cs`, `quest_manager.cs`, `quest_giver.cs` and `job_board.cs`.
  - **Most settings do nothing.** Only the RNG algorithm and RNG statistics-tracking settings are read (`game_startup.cs`). The display, auto-save, confirmation, accessibility and audio settings are stored and saved but nothing reads them. Difficulty is covered by its own item below.
- **Quest content to revisit for the current scope**: boss quests are generated one per boss from the roster (fine), but the Job Board set (5 level quests, 3 kill, 3 gold, 2 weapon upgrade, 2 equipment, 4 challenge) and its requirement numbers were sized for the longer game - e.g. "upgrade a weapon to level 10" while gear cannot currently level at all (Bucket A #12).

### 🟡 Unit tests still missing (found 2026-10-09)
- **Status**: ❌ Not Started
- **Description**: `tests/` has 11 test files (RNG, leveling, difficulty scaler, damage calculator, turn manager, combat manager, save manager, player, statistics tracker, game settings, quest manager). Planned but never written:
  - `BossManagerTests` (now for the order-based unlock, not keys)
  - `ShopKeeperTests` (pricing, restock, buy/sell)
  - Weapon/armor upgrade tests (XP, blacksmith payment, upgrade choices)
  - `SaveManagerTests` updates for settings, statistics, quest reconstruction and boss progress
  - Cross-system integration tests
  - A CI pipeline to run the tests (there is no `.github/workflows/`)
- **Also**: `CombatManagerTests.cs` is known-broken since the combat engine rework and needs rewriting against the new event-driven `CombatManager`.

### 🔴 Random loot only draws from a fraction of the item catalog (found 2026-10-09)
- **Goal (2026-10-09)**: random loot should be able to draw from the whole item database. Find out why it does not, then fix it.
- **Files**: `game_logic/items/item_database.cs` (`GetAllWeapons`, `GetAllArmor`, `GetRandomWeapon`, `GetRandomArmor`), `game_logic/progression/loot_table.cs`
- **Status**: ❌ Not Started
- **Description**: `GetWeapon()` defines 207 weapons and `GetArmor()` defines 180 armor sets, but random loot picks from the `GetAllWeapons()` / `GetAllArmor()` lists, and those were never brought up to date:
  - `GetAllWeapons()` still lists only the original 19 weapons, so the other 188 (every Epic and Legendary weapon included) can never come from a random drop.
  - `GetAllArmor()` lists 150 armor sets and leaves out all 30 Epic ones.
- **Also open**: the armor plan called for 30 Legendary armor sets (5 per armor type) that were never created - there is no Legendary armor in the game. Decide whether they are still wanted at the reduced scope.
- **Check first**: the catalog itself may be reduced for the MVP (Bucket A #14). Settle those numbers before bringing the lists up to date, so the work is not done for items that get cut.

### 🟢 Ignore stray generated files
- **Status**: ❌ Not Started
- **Description**: `coding_sessions.json` at the repo root (written when `tools/git_time_tracker.py` is run from the root instead of from `tools/`) and `tools/__pycache__/` show up as untracked files. Add both to `.gitignore` and delete the stray root copy.

### 🟢 Leadership ability disabled for the MVP (2026-10-08)
- **Files**: `game_logic/entities/player/player.cs`, `game_logic/abilities/leadership_ability.cs`, `game_logic/entities/NPCs/companions/party_manager.cs`
- **Status**: ⏸ Parked until after the MVP
- **Description**: Leadership is commented out of `Player.GetAvailableAbilities()` and `Player.CreateAbilityFromName()`. The ability class and the party-size check in `PartyManager` are left in place, unused. To bring it back, uncomment those two spots. Not yet compiled or tested since the change.

### 🟢 Investigate progression/quest_system.cs and progression/loot_table.cs
- **Files**: `game_logic/progression/quest_system.cs`, `game_logic/progression/loot_table.cs`
- **Status**: ❌ Not Started
- **Description**: Both files only contain a couple of enums (`QuestType`/`QuestStatus` in quest_system.cs, `LootSourceType` in loot_table.cs) in the `GameLogic.Progression` namespace. Check whether these duplicate/overlap with enums already defined in `game_logic/quests/` (which has its own `quest.cs`, `quest_manager.cs`, etc.) and whether they should be merged or removed.
- **Correction (2026-10-09)**: neither file is "just enums". `loot_table.cs` is the live loot system (`LootTable`, `LootGenerator`, `LootTableTemplates`) and should stay. `quest_system.cs` is a complete second quest system (576 lines: `Quest`, `QuestObjective`, `QuestReward`, `QuestSystem`, `QuestTemplates`) that nothing else references - the game uses `game_logic/quests/` instead - so it is the candidate for removal. **Update 2026-10-09: not being removed for now** - it may become the main story quest tracker (see `game_logic/progression/` in the folder-by-folder code review).
- **Context**: Bosses were just moved out of `progression/` into `game_logic/entities/enemies/bosses/` (namespace `GameLogic.Entities.Enemies.Bosses`) since `BossManager`/`BossDefinitions` were content/entity-related, not progression logic. `quest_system.cs` and `loot_table.cs` look like they might be similar leftover misplacement.

### 🔴 Difficulty selection has zero mechanical effect on gameplay
- **Files**: `game_logic/systems/game_settings.cs`, `game_logic/systems/difficulty_scaler.cs`, `game_logic/progression/leveling_system.cs`, `game_logic/menus/settings_menu.cs`
- **Status**: ❌ Not Started
- **Description**: Found while renaming the difficulty tiers (2026-09-10). There are actually THREE separate, disconnected "difficulty" concepts in the codebase:
  1. `GameLogic.Systems.DifficultyLevel` (game_settings.cs) - the one actually chosen at new-game creation (`GameManager.SelectDifficulty`), now named Normal/Hard/Difficult/Unfair. `GameSettings.GetDifficultyMultiplier()`/`GetRewardMultiplier()` exist but are **only ever read by the settings menu display** - never applied to actual enemy stats or rewards anywhere in combat/loot code.
  2. `GameLogic.Progression.XpDifficultyLevel` (leveling_system.cs) - **renamed 2026-09-10** from the confusing same-named `DifficultyLevel` to disambiguate from #1. Intentionally distinct from the main difficulty enum - it's meant to scale XP gain specifically (harder difficulty = modestly more XP, not a 1:1 stat multiplier), now using values Normal=0.8x/Hard=1.0x/Difficult=1.25x/Unfair=1.5x (adopted from `DifficultyScaler.ScaleEnemyXP`'s curve). `LevelingSystem.GetDifficultyMultiplier()` is still only ever called from unit tests, not live game code - **still needs to be wired into the actual XP-award path** (likely `combat_manager.cs`, wherever `enemy.XPValue` is granted to the player) using `_gameSettings.Difficulty` mapped to the corresponding `XpDifficultyLevel` tier.
  3. `GameLogic.Systems.GameDifficulty` (difficulty_scaler.cs) - yet another distinct enum (Easy/Normal/Hard/Nightmare) feeding the whole `DifficultyScaler` class (health/damage/XP/gold scaling, elite/boss scaling, loot quality) - a fully-built system that is **never instantiated anywhere outside its own test file**. `GameManager` has no `DifficultyScaler` field at all.
  - **Impact**: Picking Normal/Hard/Difficult/Unfair at character creation currently does nothing to actual enemy difficulty - it's cosmetic only. `DifficultyScaler` looks like the intended real implementation but was never wired in.
  - **Next step**: Decide whether to wire `DifficultyScaler` into `GameManager`/`CombatManager` (and delete the redundant `Progression.DifficultyLevel` + `GameSettings` multiplier methods), or remove `DifficultyScaler` if the flat `GameSettings` multipliers are meant to be the real mechanism (in which case those need to actually be applied in combat/loot code).

---

## LOW PRIORITY - Polish & Optional Features

### 🟢 9. Alternative RNG Algorithms
- **File**: `game_logic/systems/RNG_manager.cs:75-92`
- **Status**: ❌ Placeholder Only
- **Description**: Mersenne Twister and Xorshift RNG algorithms have interfaces but throw NotImplementedException
- **Impact**: Minimal - System.Random works fine for current needs
- **Note**: Optional enhancement for players who want specific RNG behaviors

### 🔴 10. Replace Procedural Map with Fixed Story Map
> **See the "STORY-DRIVEN FEATURE ROADMAP" section above (2026-09-28) for the current plan** - this is Tier 0 foundational work there, blocking where new bosses/NPCs get placed in the world.
- **File**: `game_logic/world/map_manager.cs`
- **Status**: ❌ Not Started (scope change, 2026-09-11)
- **Description**: Scope reduced - the map is no longer randomized. It needs to become a fixed, story-driven map with specific spawn locations (tied to the new 8-boss roster above), replacing `GenerateNewMap()`/`GenerateMapFromSeed()`'s current procedural "Slay the Spire"-style linear generation.
- **Impact**: This changes how saves store map state (`MapSeed` regeneration won't make sense for a fixed map) and how boss encounters are triggered (map location instead of free-select Champion menu).
- **Not started yet** - needs design input (how many locations, what's at each, story beats) before implementation.

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

### 🔴 Plan the Godot frontend file structure and file contents (added 2026-10-09)
- **Status**: 🚧 Superseded by starting implementation directly (2026-10-10) - the full written plan below was never produced; building started iteratively instead. Still worth doing once the MVP combat scene settles, so structure stops being invented ad hoc screen-by-screen.
- **Phase**: Planning - do this BEFORE any Godot implementation, so the thinking is finished separately from the building
- **Goal**: a written plan of every folder and file the Godot frontend will have, and what each file contains and is responsible for. Implementation then follows the plan instead of inventing structure as it goes.
- **Output**: one planning document in `docs/` (for example `docs/godot_frontend_plan.md`)
- **What the plan needs to cover**:
  - **Folder layout** of the Godot project: scenes, scripts, UI, and art/audio assets (sprites, tiles, map images, icons, fonts, music, sound effects). Where the Godot project sits relative to `game_logic/`, and how the C# game logic is included in it.
  - **Asset storage**: `game_logic/` holds code only. Images such as a world-map picture belong in a Godot assets folder, not in `game_logic/world/`. No assets folder exists yet.
  - **Scene list**: one entry per screen or area - main menu, character creation/tutorial, overworld areas (routes, towns, camps), combat, boss fights, shop, blacksmith, quest giver/job board, inventory, character sheet, settings, statistics, pause, save/load, game over, ending.
  - **For each file**: its purpose, which `game_logic` classes it talks to, which `GameManager` Open/Close methods and events (`StateChanged`, `CombatMessage`, `CombatEnded`) it uses, and what the player sees and can do on it.
  - **Map display**: how `MapManager`/`MapNode` data (including each node's `PositionX`/`PositionY`) is drawn, whether each area is its own scene, and whether the MVP needs a world-map overview screen. Depends on the two-level node design logged under `game_logic/world/` in the code review.
  - **Gaps the plan will expose in `game_logic`**: hook-ups the frontend needs that do not exist yet (settings and statistics access is already logged in the code review; expect more).
- **Current state (2026-10-10)**: `godot_integration/`'s placeholder files are gone - folded into a real, committed Godot project at `godot_project/` (`project.godot` + `RNGtheGameGodot.csproj`, referencing a newly-split `game_logic/GameLogic.csproj`). First scene exists: `godot_project/scenes/combat_scene.tscn`, driving the real `CombatManager`. See the new entry directly below for its current issues.
- **Best done after**: the folder-by-folder code review (so the plan is based on what the code really offers) and the Bucket A decisions that change which screens exist.

### 🔴 Combat scene MVP - buttons not wired up correctly, layout needs rework (added 2026-10-10)
- **Status**: 🚧 In Progress - first version runs in-editor (F6) but isn't actually usable yet
- **What exists**: `godot_project/scenes/combat_scene.tscn` + `CombatSceneController.cs` - one `Player` vs. two `Goblin`s, `EntitySlot` components (mini HP bar + placeholder hitbox + hover/select popup) in dynamic rows, driving the real `CombatManager` (`StartCombat`/`SubmitPlayerAction`/`CombatMessage`/`CombatEnded`).
- **Bug**: clicking the Attack button does nothing visible - no damage, no log update. Not yet root-caused. First thing to check next session: Godot's Output/Errors panel for a printed exception - `_Ready()`/`StartNewFight()` may be throwing partway through (a NullReferenceException from a failed `GetNode` lookup is the prime suspect), which could also explain why the combat log never displays anything either - one root cause may explain both symptoms.
- **Layout**: current vertical stack (title / enemy row / party row / log / buttons) needs replacing with a layout the user actually wants - not yet specified, ask at the start of the next session rather than guessing.
- **Wanted next**: a coordinate-grid reference system (x/y) overlaid on or documented for the combat scene, so the user can specify "put X at (3, 5)" instead of describing positions in words - removes the back-and-forth guessing game over layout changes. Design not started.

### 🔴 Combat scene - what the screen has to show (added 2026-10-12)
- **Status**: ❌ Not Started (the first combat scene exists - see "Combat scene MVP" above - and shows almost none of this yet)
- **Description**: the list of things the Godot combat screen must display and react to. Where each one sits on the screen is not decided - positions get set with the coordinate-grid layout system wanted in "Combat scene MVP".
- **Each of these is its own to-do:**
  1. ❌ **Names** - the player's name, each companion's name and each enemy's name, shown with their slot. Enemy names are the random per-encounter names (to-do 3 under `game_logic/entities/`), shown with the enemy type.
  2. ❌ **Damage numbers** - a number appears on the target when an attack lands. Decide where (over the target's slot is usual) and how it differs for damage and healing.
  3. ❌ **"Critical hit"** - the words appear when a hit is critical. Decide where and how it looks next to the damage number.
  4. ❌ **"Miss"** - the word appears when an attack misses, for both sides: the party's attacks and the enemies'. Also needed for an evaded attack (Evasion ability).
  5. ❌ **Health bars move when a hit lands** - for both sides, at the moment of the hit and not only when the whole turn has finished. The same for healing and for damage from effects such as poison.
  6. ❌ **Whose turn it is** - mark the party member or enemy currently acting, and show the action buttons for the party member the player is choosing for (the player, or a directly controlled companion).
  7. ❌ **Turn order and round number** - the round's 8 turn slots (4 party, 4 opponents), with unused slots shown as skipped and several enemies shown together when they share a slot (to-do 9 under `game_logic/combat/`).
  8. ❌ **Action buttons** - Attack, Ability, Item, Defend, Flee. Ability and Item are missing today. Grey out what cannot be used (ability on cooldown, no usable items).
  9. ❌ **Picking a target** - an enemy for attacks; a party member for healing items and revives; an item from the shared party bag.
  10. ❌ **Defending** - a marker on anyone who is defending, until their next turn (to-do 2 under `game_logic/combat/`).
  11. ❌ **Status effects** - what is on each combatant (poisoned, grappled, boosted) and how long is left.
  12. ❌ **Companion control toggle** - switch each companion between direct control and auto during the fight (Tier 1).
  13. ❌ **Defeated combatants** - show a fallen enemy or party member as down, and bring a revived one back.
  14. ❌ **How the fight ended** - a different screen for won, lost, the party fled, and opponent fled / draw (to-do 1 under `game_logic/combat/`), with the reward summary: XP, gold, items, level ups.
  15. ❌ **Boss fights** - the boss's name and title, and the warning messages before its big attacks (Skarn's stoop).
  16. ❌ **Combat log** - keep the text log as a readable history alongside the visual feedback.
- **Update 2026-10-14**: the `game_logic` side below is now written (not yet built) - see "Combat code pass" under `game_logic/combat/` for the events and methods the screen can use. The scene itself still needs wiring to them.
- **Needed from `game_logic` first**: `CombatManager` only sends lines of text, so the screen cannot tell a damage number from a heading. It has to report what happened - who acted, on whom, hit / miss / critical, how much, which effect - as it happens, one event at a time, so items 2-5 can be shown at the right moment. It also has to expose whose turn it is, the turn order, the round number, who is defending, what can be used, and the end-of-fight result. Listed under `game_logic/combat/` in the code review ("What the combat screen cannot get").
- **Timing**: today `CombatManager` runs every computer-controlled turn in one go, before the screen gets control back. For hits, numbers and health bars to play one after another, the screen has to queue the events and play them in order, or combat has to advance one turn at a time. Decide which when this is built.

### 🔴 Replace all console output with output the Godot frontend can show (added 2026-10-12)
- **Status**: ❌ Not Started
- **Description**: `game_logic` prints straight to the console in about 1,290 places, and a Godot screen never sees any of it. Count by folder (2026-10-12): `entities` 436, `menus` 433, `abilities` 113, `core` 96, `items` 71, `world` 68, `quests` 37, `progression` 12, `data` 10, `systems` 8, `combat` 4. The 433 in `menus/` are reference-only files and do not need converting, which leaves about 855.
- **Goal**: every one is removed and replaced by something the frontend is wired to - none left printing.
- **Three kinds, handled differently**:
  - **Things that happen in a fight** (damage taken, healing, an ability used, an effect starting, ticking or ending, a companion attacking, an item used): send them through the combat log, the way `CombatManager.Log()` / `CombatMessage` already does for the lines `CombatManager` writes itself. This is the biggest group and the one the combat scene needs first.
  - **Things that happen outside a fight** (level up, item added to the inventory, gear equipped, quest progress, boss defeated): raise an event the frontend can show as a popup or message. The same events are what the statistics tracker and the quests need to listen to (see "Quests, statistics and settings are built but not connected").
  - **Whole console screens** (typed input, "press any key", menus): not converted line by line - rebuilt as Godot screens (see the entry below and `game_logic/menus/` in the code review).
- **Also remove**: the three `Console.Clear()` calls in `combat_manager.cs`, the likely cause of the combat scene's dead Attack button (see "Combat scene MVP" above), and waits such as `Console.ReadKey()` and the blacksmith's one-second pause.
- **Order**: combat first (it unblocks the combat scene), then the rest folder by folder.

### 🔴 Rebuild the shop keeper, blacksmith and quest giver screens in Godot (added 2026-10-12)
- **Status**: ❌ Not Started
- **Description**: `shop_keeper.cs`, `blacksmith.cs` and `quest_giver.cs` each mix the rules with a console menu. Keep the rules in `game_logic` and rebuild each menu as a Godot screen, following the rule decided under `game_logic/menus/` (logic in `game_logic`, screen in `godot_project/UI/`, nothing built twice).
- **Per screen**:
  - **Shop**: stock list with prices, buy, sell, the player's gold, restock information. Rules to keep: `BuyItem`, `SellItem`, `CalculateBuyPrice`, `CalculateSellPrice`, `RestockShop`. Console parts to drop: `ShowShop`, `DisplayGoods`, `BuyMenu`, `SellMenu`, `ShowShopInfo`, `Interact`.
  - **Blacksmith**: list of gear ready to upgrade, cost, confirm, and for weapons the three-way upgrade path choice (never wired - see `game_logic/items/`). Rules to keep: `GetUpgradableWeapons`, `GetUpgradableArmor`, the cost calculation. `UpgradeWeapon`/`UpgradeArmor` need the typed "Y/N" question and the pause taken out of them. Console parts to drop: `ShowServices`, `UpgradeWeaponMenu`, `UpgradeArmorMenu`, `ShowPriceGuide`, `Interact`.
  - **Quest giver**: available, active and finished boss quests; accept; claim reward. The whole of `QuestGiver.Interact()` is a console menu. Depends on the quest to-dos (rewards actually given, story-order unlocking) under `game_logic/quests/`.
- **Depends on**: a shop keeper and a blacksmith actually existing somewhere in the world (none is ever created today), which depends on towns (`city.cs`, the fixed map).

### 🔴 Add more enemy types (added 2026-10-12)
- **Status**: ❌ Not Started
- **Description**: there are three regular enemies today (Goblin, Bandit, Dragon), picked by one fixed roll for the whole game (70% / 25% / 5%). More enemy types are planned, some limited to a specific area or part of the world. The chance of each enemy per area will be worked out later.
- **`game_logic` side**: new enemy classes; a per-area list of which enemies can appear and how often, replacing the single fixed roll in `GameManager.SpawnEnemy()` (the map node already carries a location type; areas arrive with the routes-and-towns map); each type's behaviour plugged into the shared combat decision-maker (to-do 5 under `game_logic/entities/`); random names per encounter (to-do 3 there).
- **Godot side**: a sprite and an overworld/combat appearance for each enemy type, and showing the random name with the enemy type in the combat scene.
- **Note**: the Dragon being a 5% random encounter is known and accepted for now; it gets sorted out when the per-area lists exist.

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

### 💼 Rewrite the SRS to match the finished game (added 2026-10-13)
- **File**: `SRS.md`
- **Status**: ⏸ Waiting until the game is complete
- **Description**: the SRS is the original 2025 requirements document and no longer matches the game. Do not edit it piecemeal. When the game is finished, rewrite it from the final product so the project has a properly documented, well-made SRS - for the record, and as a detailed reference when starting the second game.
- **Helps later**: decisions are already dated in this file; keeping that habit gives the rewrite its source material.

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

_Corrected 2026-10-09 after checking each item against the code. "Built, not connected" means the classes exist but the game never uses them - these are NOT done._

**Completed**: see `docs/done.md` (turn order, RevivePotion system, enemy defense application, Godot installed - moved 2026-10-13). The weapon/armor save system with custom stats also works; it is one part of item 15, which is not finished, so it stays here.

**Built, not connected** (do not skip these): 5
- Equipment leveling - gear never gains XP, Blacksmith never placed in the game (item 1)
- ShopKeeper - no shop exists in the game to visit (item 3)
- Quest system - quest progress is never updated (item 6)
- Settings - most settings do nothing, difficulty has no effect (item 8)
- Statistics - almost nothing is recorded (item 9)

**In progress**: 3
- Boss progression - order-based framework done, roster and difficulty scaling open (item 5)
- Companion integration - in combat, but only the tutorial companion can be recruited
- Save system enhancements (mostly done)

**Removed**: the Boss Key Progression System (15 bosses, keys, final gate) was replaced by order-based unlocking on 2026-09-30

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
- Equipment and ability classes are written, but levelling is not working in play: gear and passive abilities never gain XP (Bucket A #12)
- Weapon upgrade system provides strategic player choice (9 weapon types with unique scaling) - not reachable until the Blacksmith is placed in the game
- Save system robustly preserves all custom-upgraded equipment
- Main gaps: systems that are built but not connected to the game (shops, blacksmith, quests, statistics, settings) - see SUMMARY BY STATUS
- The console menus are stubbed out; these systems need Godot UI and map placement to become reachable

## SESSION HISTORY (2025-12-05) - what was written, NOT current status

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
