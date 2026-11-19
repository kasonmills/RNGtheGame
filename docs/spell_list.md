# Spell list (draft for review)

**Status: draft, 2026-11-19 - for the user to review and edit.** The structure is decided (see `done.md`, "Spells - who casts and how recipes work"). The spells, recipes and numbers below are my suggestions. All numbers are placeholders, to retune after playtesting.

**Decided so far**
- About 10 spells in the MVP, plus the free **Mage Blast** (no components - see `done.md`).
- **Casters:** Fiery, the player (once the magic component seller is unlocked) and Fluke. **Fluke can only cast luck spells** - any other spell fails or backfires on him.
- **Recipes are mostly by group:** one key component, plus filler from a group ("any 2 berries"). Rare components only for the big spells.
- **How spells are learned** is still open - decide it after this list.

"A basic hit" means an average weapon hit for the caster's level (the same yardstick as Mage Blast, which does about half of one). Durations are 3 turns unless noted - they depend on the buff-timing fix (section 1, Combat).

---

## Damage

| Spell | Recipe | Effect |
|---|---|---|
| **Fireball** | 1 Rubeetles + any 2 berries | One enemy, about 1.5x a basic hit. 30% chance to burn. |
| **Frost Lance** | 1 Sapphider + any 2 herbs | One enemy, about 1.2x a basic hit. Slows it (-4 speed for 2 turns). |
| **Chain Lightning** | 1 Amberbees + any 2 berries | Up to 3 enemies, about 0.7x a basic hit each. |
| **Poison Cloud** | 1 Poisonshroom + 1 Berylflies + any 1 herb | Poisons every enemy (the poison formula at the caster's level). |

## Support

| Spell | Recipe | Effect |
|---|---|---|
| **Mend** | 1 Haleshroom + any 2 berries | Heals one ally about 25% of their max HP. |
| **Combat Elixir** | 1 Strengthshroom + 1 Shieldshroom + any 1 herb | One ally: +20% damage and +20% defense. Replaces the Combat Elixir consumable (to-do, section 1). |
| **Purify** | 1 Fairyshroom + any 1 herb | Removes every negative effect from one ally, including the Demon Prince's chicken. |

## Luck (Fluke can cast these)

| Spell | Recipe | Effect |
|---|---|---|
| **Fortune's Favor** | any 1 lucky charm + any 1 berry | Whole party: +10% crit and +5% hit chance. |
| **Ill Omen** | any 1 unlucky charm + any 1 herb | One enemy: -15% hit chance and -10% crit. |
| **Doom** (rare) | 1 Negativity Crystal + any 1 unlucky charm + any 1 bug | For 2 rounds, every enemy roll is rolled twice and the worse one is kept. The big spell. |

---

## Notes

- **Component groups used:** berries (7), herbs (9), lucky charms (Ladybug, Horseshoe, Bamboo, Acorns, Fish, Jade), unlucky charms (Black Cat Fur, Broken Mirror, Raven Feather), bugs (7), plus named mushrooms and bugs as key components.
- **Not a key component anywhere yet:** Spotshroom, Flatshroom, Crystacricket, Draviteflies, Opalant. The bugs still count as "any bug" for Doom. These could key future spells, or become filler.
- **Fiery's wild magic** (her rules in `done.md`): when it triggers, a random other spell from this list comes out instead, paid for with the components of the spell she chose. Doom coming out by accident is the jackpot. Above 75 anger, about 1 in 4 wild spells backfires on the party.
- **Fluke:** his luck spells fit his Lucky Aura. A non-luck spell is a 50/50 gamble (decided 2026-11-19): half the time it fizzles (turn and components wasted), half the time it backfires on him (a damage spell hurts him, a buff goes to an enemy).
- **Ill Omen and the Demon Prince's Hexed** do similar things (lower hit chance). That's fine, but they could share one effect in the code.
