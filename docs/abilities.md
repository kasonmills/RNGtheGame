# Player Abilities

Reference for the abilities the player can choose from. Numbers here match the code in `game_logic/abilities/` as of 2026-10-08.

The player picks **one** ability at character creation (`Player.SelectedAbility`). The selectable list is `Player.GetAvailableAbilities()` in `game_logic/entities/player/player.cs`; a new ability must be added there and in `Player.CreateAbilityFromName()` (used when loading a save).

Companion abilities (`game_logic/entities/NPCs/companions/`) and enemy abilities (`game_logic/abilities/enemy_abilities/`) use the same `Ability` base class but are not covered here.

---

## How abilities work

- **Active** abilities are used as a combat action and then go on cooldown.
- **Passive** abilities (`IsPassive = true`) are always on and cannot be used manually.
- Every ability has a level, and its effect grows in a straight line from its level 1 value to its max-level value (`GetScaledValue(min, max)` in `ability.cs`).
- Every ability has a rarity (Common, Uncommon, Rare, Epic, Legendary). Today rarity only changes how much XP a level costs.

### Levelling - under review

> **The level cap is planned to drop from 100 to 50, and the XP system is due for a rework once more of the game is defined.** See Bucket A item 12 in `to_do_list.md`. The code still uses a cap of 100, and the "max level" values in the tables below are the values at the cap, whatever it ends up being.

What the code does today:

- Level cap is 100 (`Ability.MaxLevel`).
- XP needed for the next level = base x 1.035^(level - 1), where base is 100 / 200 / 300 / 400 / 500 for Common / Uncommon / Rare / Epic / Legendary.
- Active abilities gain XP each time they are used in combat: 10 XP for the first use in a fight, and 15% more for each further use in the same fight. The count resets when combat ends.
- **Passive abilities never gain XP.** Nothing in the code awards it to them, so they stay at level 1.
- Active ability cooldowns shorten by 1 turn at level 25 and by 2 turns at level 75 (never below 1 turn).

---

## Active abilities

All four target the player only. Buffs last 3 turns and do not stack with themselves.

| Ability | Rarity | Cooldown | Effect | Level 1 | Max level |
|---|---|---|---|---|---|
| Attack Boost | Common | 5 turns | Increases attack damage for 3 turns | +1% | +70% |
| Heal | Common | 4 turns | Restores a random amount of HP in a range | 10-20 HP | 100-150 HP |
| Defense Boost | Common | 5 turns | Reduces damage taken for 3 turns | -1% | -60% |
| Critical Strike | Uncommon | 4 turns | Increases critical hit chance for 3 turns | +1% | +50% |

---

## Passive abilities

| Ability | Rarity | Affects | Effect | Level 1 | Max level |
|---|---|---|---|---|---|
| Rallying Cry | Rare | Companions only | Increases companion attack damage | +5% | +90% |
| Evasion | Epic | Player only | Chance to avoid an attack completely. Does not work while defending | 1% | 60% |
| Precision Training | Epic | Player and companions | Increases accuracy | +1% | +30% |
| Swift Tactics | Epic | Companions only | Increases companion speed | +2% | +50% |
| Iron Will | Epic | Player only | Chance at the end of each combat round to remove all negative status effects | 1% | 25% |
| Executioner | Legendary | Player only | Changes critical hits - see below | see below | see below |

### Executioner

Starts as a drawback and turns into a strength as it levels. It replaces the normal 1.5x critical hit multiplier.

| | Level 1 | Max level |
|---|---|---|
| Critical hit chance | -10% | +10% |
| Critical hit multiplier | 1.1x | 3.0x |

Because passives cannot level today, Executioner is currently stuck at its level 1 drawback.

---

## Weapon masteries

Nine passive abilities, one per weapon type: Sword, Axe, Mace, Dagger, Spear, Staff, Bow, Crossbow, Wand. All are Rare and identical apart from the weapon they need. The bonuses only apply while the player has a weapon of the matching type equipped.

| Bonus | Level 1 | Max level |
|---|---|---|
| Accuracy | +1% | +40% |
| Attack damage | +1% | +70% |
| Critical hit chance | +1% | +25% |

Code: `game_logic/abilities/weapon_masteries/`, applied in `game_logic/combat/damage_calculator.cs`.

---

## Not in the MVP

**Leadership** (Rare, passive) - adds companion slots to the party as it levels. It is out of scope for the MVP and has been disabled, not deleted: it is commented out of both lists in `player.cs`, while `leadership_ability.cs` and the party-size check in `party_manager.cs` are still in place. Party size is fixed at 4 (player plus 3 companions) for the MVP.
