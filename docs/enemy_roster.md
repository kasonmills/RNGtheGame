# Enemy roster

**Status: final, reviewed 2026-11-24** (drafted 2026-11-23). The stats, level bands, specials, AI patterns, day/night lean and name lists below are approved. All numbers are placeholders, to retune after playtesting. This sheet is the "Rebalance the regular enemies" item (section 1, Combat). Still open: a real name for "Kingdom B".

**Decided so far (2026-11-23)**
- **Goblin, Bandit and Dragon stay.** The Dragon becomes a rare late-game enemy found only in one or two mountain areas, never a random 5% roll.
- **Families:** goblins, bandits, wolves, slimes (with wild draglings), bugs, **Fungusoars** (mushroom dinosaurs), imps, the player's own army (deserters on the routes, sparring soldiers at the army camp), Kingdom B's army, castle guards and the dragon.
- **One enemy per family, two for the big families** - 15 in all.
- **Castle Guards** are one enemy: a regular enemy in and around the palace, and the Princess's summon.
- **Army soldiers:** loyal soldiers are never random encounters. The player fights them as sparring opponents at the army camp, and as **deserters** (hostile, roaming the routes).
- **Group mix:** the touched enemy's family fills the group. About 1 in 4 fights adds one or two others from the area's spawn list.
- **Group sizes** (in the table below): the maximum goes up by 1 every 10 player levels, and by +1 on Difficult / +2 on Unfair. A fight never has more than 8 enemies.
- **Names:** each family has its own name list, shown as "Name the Type" ("Grik the Goblin"). No repeats in one fight. Bosses keep their own names.

---

## The standard enemy

Every enemy is the standard enemy times its multipliers. The standard is the built Castle Guard's health, with a little more damage (my picks):

| Stat | Standard | Level 1 | Level 20 | Level 40 |
|---|---|---|---|---|
| Health | 20 + 6 per level | 26 | 140 | 260 |
| Damage | (2 + level/2) - (5 + level) | 2-6 | 12-25 | 22-45 |
| Defense | level / 4 | 0 | 5 | 10 |
| Speed | 10 + level / 4 | 10 | 15 | 20 |
| Accuracy | 75% | | | |
| Crit | 5% | | | |

Why: at level 1 the player does about 10 a hit (Rusty Sword 3-7, x2), so a standard enemy takes about 3 of the player's hits. A level-20 player (about 330 HP) facing 3 standard enemies takes about 55 a round before armor, so a fight lasts 4-5 rounds. The difficulty multipliers (section 1, "Difficulty numbers") go on top of this.

**XP weight** is a multiplier on whatever base XP the XP rework sets for a regular enemy (gold follows it). It isn't a number of its own yet.

---

## The roster

| # | Enemy | Family | Levels | Group | HP x | Dmg x | Def x | Speed | Acc | Crit | XP weight |
|---|---|---|---|---|---|---|---|---|---|---|---|
| 1 | **Goblin** | Goblins | 1-12 | 1-4 | 0.7 | 0.8 | 0.5 | +2 | 75% | 5% | 0.7 |
| 2 | **Bandit** | Bandits | all game | 1-3 | 1.0 | 1.0 | 1.0 | +3 | 80% | 25% | 1.2 |
| 3 | **Wolf** | Wolves | 1-20 | 2-5 | 0.8 | 0.9 | 0.5 | +4 | 80% | 10% | 0.9 |
| 4 | **Slime** | Slimes | 5-25 | 2-5 | 1.4 | 0.6 | 0.5 | -3 | 70% | 0% | 0.8 |
| 5 | **Big Slime** | Slimes | 15-35 | 1 in a slime group | 2.5 | 0.9 | 0.75 | -4 | 70% | 0% | 1.8 |
| 6 | **Wild Dragling** | Slimes | 20-30 | in slime groups | as built | as built | as built | as built | 70% | 5% | 0.7 |
| 7 | **Spider** | Bugs | 5-25 | 2-5 | 0.8 | 0.9 | 0.5 | +2 | 80% | 10% | 1.0 |
| 8 | **Giant Beetle** | Bugs | 15-35 | 1-3 | 1.8 | 1.0 | 2.5 | -3 | 70% | 5% | 1.5 |
| 9 | **Fungusoar** | Fungusoars | 8-30 | 1-2 | 2.5 | 1.3 | 1.0 | -2 | 70% | 5% | 2.0 |
| 10 | **Imp** | Imps | 18-30 | 2-5 | 0.6 | 0.8 | 0.5 | +4 | 80% | 10% | 1.0 |
| 11 | **Deserter** / **Army Soldier** | Own army | 10-40 | 1-3 | 1.1 | 1.0 | 1.5 | +0 | 80% | 10% | 1.1 |
| 12 | **Kingdom B Soldier** | Kingdom B | 20-40 | 1-3 | 1.3 | 1.1 | 2.0 | +0 | 80% | 10% | 1.3 |
| 13 | **Kingdom B Archer** | Kingdom B | 20-40 | joins soldier groups | 0.8 | 1.1 | 1.0 | +1 | 90% | 15% | 1.2 |
| 14 | **Castle Guard** | Castle guards | 30-40 (palace) | 1-3 | 1.0 | 1.0 | 1.0 | +0 | 75% | 5% | 1.0 |
| 15 | **Dragon** | Dragon | 30-40 (mountains) | always 1 | 4.0 | 2.0 | 3.0 | -2 | 85% | 20% | 5.0 |

Level bands are the player levels where each enemy shows up, set with the fixed map (the per-boss level target is in `done.md`). The enemy fights at the area's level, like today.

### Worked numbers (Hard, before difficulty)

| Enemy | Level 1 | Level 20 | Level 40 |
|---|---|---|---|
| Goblin | 18 HP, 2-5 | 98 HP, 10-20 | - |
| Bandit | 26 HP, 2-6 | 140 HP, 12-25 | 260 HP, 22-45 |
| Wolf | 21 HP, 2-5 | 112 HP, 11-22 | - |
| Slime | 36 HP, 1-4 | 196 HP, 7-15 | - |
| Big Slime | - | 350 HP, 11-22 | 650 HP, 20-40 (at 35: about 570 HP) |
| Spider | 21 HP, 2-5 | 112 HP, 11-22 | - |
| Giant Beetle | - | 252 HP, 12-25, def 12 | 468 HP, 22-45, def 25 |
| Fungusoar | 65 HP, 3-8 (from 8) | 350 HP, 16-32 | - |
| Imp | - | 84 HP, 10-20 | - |
| Deserter / Army Soldier | - | 154 HP, 12-25, def 7 | 286 HP, 22-45, def 15 |
| Kingdom B Soldier | - | 182 HP, 13-28, def 10 | 338 HP, 24-50, def 20 |
| Kingdom B Archer | - | 112 HP, 13-28 | 208 HP, 24-50 |
| Castle Guard | - | - | 260 HP, 22-45 |
| Dragon | - | - | 1040 HP, 44-90, def 30 |

(The built Castle Guard does 2 + level/3 to 5 + 2·level/3. Moving it to the standard raises its damage a little - 15-31 → 22-45 at level 40.)

---

## Behaviour, AI and specials

The AI patterns are the ones decided for companions (2026-11-19): **Random, Most wounded, Weakest defense, Last attacker, Biggest threat** - each one followed about 70% of the time, 30% random. They need the shared combat decision-maker (section 1, Companions).

| Enemy | Flees? | AI pattern | Special |
|---|---|---|---|
| Goblin | Cowardly: tries below 25% HP | Random | - |
| Bandit | May try below 40% HP | Most wounded | High crit (sneak attacks) |
| Wolf | May try below 20% HP | Most wounded | 15% chance a bite causes Bleeding |
| Slime | Never (mindless) | Random | 20% chance a hit slows (-2 speed, 2 turns) |
| Big Slime | Never | Random | Splits into 2 Slimes when beaten (a summon - limited by the 8-enemy cap, the new Slimes join next round) |
| Wild Dragling | May try below 30% HP | Random | Fast and weak. The Slime Dragon's spawns never flee (decided 2026-11-23) |
| Spider | May try below 20% HP | Weakest defense | 20% chance a bite poisons |
| Giant Beetle | Never | Last attacker | Very high defense - piercing bolts and spells are the answer |
| Fungusoar | Never | Biggest threat | Spore Roar: 25% chance a hit poisons. Mushroom components drop at 2x weight |
| Imp | Tries below 25% HP | Random | 20% chance a hit adds a small condition: Burning, Hexed or Weakened (the Demon Prince's new conditions, weaker) |
| Deserter | May try below 30% HP | Biggest threat | Defends more often when wounded |
| Army Soldier | Never (sparring) | Biggest threat | Army camp only. Its rewards come from the camp's exit reward, not normal loot |
| Kingdom B Soldier | May try below 20% HP | Biggest threat | Defends more often when wounded |
| Kingdom B Archer | May try below 30% HP | Weakest defense | High accuracy. Ranged, back row (no melee penalty) |
| Castle Guard | Never in the Princess fight; may try below 20% HP elsewhere | Last attacker | - |
| Dragon | Never | Biggest threat | Fire breath: 30% chance a hit burns |

**Rows (decided 2026-11-23):** the Kingdom B Archer and the Imp fight from the back row. Every other enemy is in the front row. Bosses ignore rows. Attacks on the back row get -10 hit chance. A back-row enemy is picked less often by auto companions and is harder to hit, so archers and imps need dealing with on purpose.

"May try" = the existing per-enemy flee chance (`FleeChance`, 30 until tuned) is rolled when it tries.

---

## Day and night (draft)

Each area has a day and a night spawn list (decided 2026-11-23). Night enemies get +10% HP and damage, one step more aggression, and +20% XP and gold. My suggestion for which families lean which way, to use when the per-area lists are set with the fixed map:

| Mostly day | Both | Mostly night |
|---|---|---|
| Goblins, slimes, deserters, Kingdom B army | Bandits, bugs, Fungusoars, castle guards | Wolves, imps, the Dragon |

---

## Names (per family)

At least 8 per family, so an 8-enemy fight never repeats one. Shown as "Name the Type".

| Family | Names |
|---|---|
| Goblins | Grik, Snag, Nib, Wort, Skab, Gub, Rizzle, Muck, Fizz, Krag |
| Bandits | Knuckles, Scar, Two-Toes, Lefty, Rook, Patch, Mad Mags, Sly Sal, Crow, Dirk |
| Own army (deserters, army soldiers) | Tom, Bob, Will, Ned, Hugh, Martha, Greta, Ivy, Bram, Cole |
| Kingdom B army | Anselm, Dorek, Valka, Ilsa, Torvin, Mira, Brannoc, Zelde, Kasimir, Oda |
| Castle guards | Gerald, Percy, Edmund, Agnes, Mabel, Oswin, Cedric, Winifred, Rupert, Hilda |
| Wolves | Fang, Ash, Grey, Howl, Shade, Thorn, Rill, Snarl, Ember, Frost |
| Slimes (and draglings) | Gloop, Blorp, Wobble, Squish, Jiggle, Ooze, Plop, Goober, Burble, Splat |
| Bugs | Skitter, Click, Nibbles, Chitter, Itsy, Crunch, Buzz, Dot, Pincer, Lacy |
| Fungusoars | Rex, Morel, Truffle, Porcini, Enoki, Shiitake, Puffball, Chanterelle, Button, Toadstool |
| Imps | Snik, Pox, Grub, Cinder, Hex, Nettle, Jinx, Smudge, Wick, Gnash |
| Dragon | Emberclaw, Ashwing, Cindermaw, Scorchtail, Smoulder, Blazefang, Pyra, Kindle |

"Kingdom B" is a placeholder until the kingdom has a name - "Valka the Kingdom B Soldier" reads badly, so the type name changes with it.

---

## Notes

- **Bugs and components:** spiders and beetles drop bug components at 2x weight, like the Fungusoar's mushrooms. (My suggestion, on top of the area favour.)
- **The Big Slime's split** uses the summon the Slime Dragon already uses. It needs the 8-enemy cap and diminishing rewards to stop farming - Slimes could share the dragling rewards group.
- **The Imp's conditions** reuse Burning (already built) and two of the Demon Prince's new ones (Hexed, Weakened), so they come for free once he's built.
- **Wild draglings** fight with the built `Dragling` stats, but need a behaviour that can flee (section 1, Combat).
