# Boss Stat Sheet (approved 2026-11-10, in the code 2026-11-11)

Each boss's **level-1 stats**, the numbers every boss grows from. **Approved 2026-11-10 and in the code 2026-11-11:** the numbers live in `game_logic/entities/enemies/bosses/boss_stat_sheet.cs` - change them there. This file keeps the reasoning behind them.

**Version 2.** Redone the same day against the new player: 100 HP at level 1, +10 + level/5 a level, and the party's weapon damage doubled (Chance's cat-form 1-100 unchanged). Version 1 was tuned against the old 24 HP player.

🤫 Boss #9 is a hidden twist. This file is a design doc, never game text.

---

## How a boss's stats are built in a fight

A boss fights at the **party's average level + 5** (max 45). Skarn keeps a fixed level 2. At that level:

| Stat | Formula | Where it comes from |
|---|---|---|
| Health | level-1 HP × (1 + 10% per level above 1) × **3** | `DifficultyScaler` level growth, boss multiplier |
| Damage (min and max) | level-1 damage × (1 + 8% per level above 1) × **1.5** | same |
| Defense | level-1 defense + (growth per level × levels above 1) | new - set per boss below |
| Speed | level-1 speed + (growth per level × levels above 1) | new - set per boss below |
| Accuracy, crit chance, crit multiplier | fixed per boss | enemy accuracy per difficulty is still to come (to-do) |

Then the difficulty multiplies health and damage: Normal 75%, Hard 100%, Difficult 150%, Unfair 200%. **All numbers below are for Hard.**

---

## The draft

### Level-1 stats (what you edit)

| Boss | Role | HP | Damage | Accuracy | Crit | Crit × | Defense (+ per level) | Speed (+ per level) |
|---|---|---|---|---|---|---|---|---|
| #1 Skarn | Tutorial teacher | 70 | 9-17 | 72% | 5% | 1.5 | 1 (+0) | 12 (+0) |
| #2 Librarian-Monk | Slow wall (his shield is extra) | 60 | 6-11 | 80% | 5% | 1.5 | 2 (+0.25) | 8 (+0.2) |
| #3 Giant House Centipede | Fast, the poison does the work | 55 | 6-10 | 85% | 5% | 1.5 | 1 (+0.2) | 14 (+0.25) |
| #4/#5 Cyclops Brothers (each) | A pair - every attack lands twice | 65 | 4-7 | 75% | 8% | 1.5 | 3 (+0.25) | 9 / 7 (+0.2) |
| #6 Demon Prince | Weak hits, a condition on every hit | 85 | 4-8 | 85% | 5% | 1.5 | 2 (+0.25) | 13 (+0.25) |
| #7 Slime Dragon | Big hits, slow (draglings are extra) | 85 | 11-19 | 75% | 5% | 1.5 | 3 (+0.3) | 8 (+0.2) |
| #8 Captain of the Royal Guard | Glass cannon, crits | 70 | 4-7 | 90% | 55% | 2.5 | 2 (+0.2) | 15 (+0.3) |
| #9 Final boss | Two turns, guards | 125 | 8-14 | 85% | 10% | 1.5 | 3 (+0.3) | 14 (+0.3) |

**Why each one:**
- **Skarn** is tuned so the tutorial lasts about 5 rounds. The player needs at least 4 turns to try every action. His normal hit takes 5 to drop the player. His stoop attack (2.5×) does 38-70, which is why the tutorial teaches Defend.
- **Librarian-Monk**: low damage and slow, because his shield already makes him the long fight. His HP doesn't count the shield, so a strong shield may mean less HP here. (2026-11-11: the shield is 20 health at level 1, grown the same way as his health and scaled by the difficulty - 102 on Hard at level 8.)
- **Centipede**: fast and accurate, with low hits, because the stacking poison is meant to be the danger.
- **Cyclops**: each hit is about half a solo boss's. Both attack on each brother's turn, so the party takes 4 hits a round. Each brother has a solo boss's HP, because the party has to bring both down in the same round.
- **Demon Prince**: the lowest damage for his level, as decided, because every hit also lands a condition.
- **Slime Dragon**: hard hits but slow and clumsy. The draglings add pressure on top.
- **Captain**: the highest speed and low base damage. A 55% chance at 2.5× crits makes his average hit about 1.8× what the table shows (on harder difficulties the to-do wants even more crit).
- **Final boss**: the highest HP, because the fight should feel like the last one. Two turns a round and the guards make the damage count twice.

### What that becomes in the fight (Hard)

Fight levels are **my assumption**: the per-boss level target is still open (section 2). I spread the bosses evenly to level 40.

The numbers match the code's rounding (health and damage rounded up, defense and speed to the nearest, halves up). The second Cyclops brother is speed 10 at level 16.

| Boss | Boss level | HP | Damage | Defense | Speed | Rounds to win (with Chance) | Rounds (without Chance) | Player HP | Hits to drop the player (crits counted) |
|---|---|---|---|---|---|---|---|---|---|
| #1 Skarn | 2 | 231 | 15-28 | 1 | 12 | 4.9 | 20.5 | 100 | 5 |
| #2 Librarian-Monk | 8 | 306 | 15-26 | 4 | 9 | 5.7 | 14.8 | 120 | 6 |
| #3 Centipede | 12 | 347 | 17-29 | 3 | 17 | 4.9 | 8.3 | 163 | 7 (+ poison) |
| #4/#5 Cyclops (each) | 16 | 488 | 14-24 | 7 | 12 | 5.2 | 6.9 | 209 | 11 (4 hits a round) |
| #6 Demon Prince | 21 | 765 | 16-32 | 7 | 18 | 6.3 | 7.3 | 271 | 12 (+ conditions) |
| #7 Slime Dragon | 26 | 893 | 50-86 | 11 | 13 | 6.5 | 7.1 | 338 | 5 |
| #8 Captain | 32 | 861 | 21-37 | 8 | 24 | 5.0 | 5.1 | 425 | 9 |
| #9 Final boss | 45 | 2025 | 55-95 | 16 | 27 | 9.2 | 8.7 | 638 | 9 (twice a round) |

For comparison, party speeds: player 10 (+1 every 3 levels), Chance 14, Boon 13, Felicity 11, Risk 7.

**What I assumed about the party** (to check):
- **Player:** 100 HP at level 1, +10 + level/5 a level (the same as the companions).
- **Hits:** a weapon hit averages about 5 at level 1 before the ×2, growing about 0.75 a level from upgrades and better finds, +1 per 2 levels, then ×2. 63% hit chance on Hard (a 70% weapon − 7).
- **Party size:** the player plus 1 companion for Skarn, 2 for the Monk and Centipede, 3 from then on.
- **Chance:** the "with Chance" column has him in the party; he hits for his level to 100 at 85%.
- Armor isn't counted, and neither are abilities, items or healing.

---

## Things the numbers turned up

1. **Chance still decides the first three fights.** With the party's damage doubled, the party does about the same damage with or without him from boss #4 on. But Skarn takes 4 times longer without him, and the Monk 2.5 times. The draft is tuned **with** him, which is safe for Skarn (Chance is always there in the tutorial). His numbers are worth checking against bosses #2 and #3 when they are tested.
2. **Boss rewards don't come from these stats.** Fight XP is rolled from the enemy's level (`CalculateXPReward`, level × 30-70) and gold comes from the loot table, not the boss's `XPValue`/`GoldValue`. The scaler's boss reward multipliers (XP ×5, gold ×3-5) aren't wired into either. Rewards are left to the XP rework, which also waits on the per-boss level target.
3. **Difficulty is applied only once.** The scaler's old `ScaleBossHealth`/`ScaleBossDamage` multiply by the difficulty too, so the build uses new difficulty-free methods (`BossHealthAtLevel`, `BossDamageAtLevel`, `BossStatAtLevel`) and the fight applies the difficulty once on top (done 2026-11-11).
4. **Regular enemies were tuned for the old player.** A Goblin hits for 2-5 and has 15 HP, so ordinary fights will now be much easier (100 HP player, doubled damage). They need the same kind of pass.

## Open for you
- ~~Every number in the level-1 table.~~ Approved 2026-11-10.
- The per-boss level target (section 2). If a boss is fought at a different level than assumed, its fight numbers move, but the level-1 numbers can stay.
- How many rounds a boss fight should last on Hard (the draft aims at about 5 early, 6-9 late).
- Finding 1, and finding 4 (regular enemies).
