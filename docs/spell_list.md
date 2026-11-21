# Spell list (draft for review)

**Status: draft, 2026-11-19 - for the user to review and edit.** The structure is decided (see `done.md`, "Spells - who casts and how recipes work"). The spells, recipes and numbers below are my suggestions. All numbers are placeholders, to retune after playtesting.

**Decided so far**
- About 10 spells in the MVP, plus the free **Mage Blast** (no components - see `done.md`).
- **Casters:** Fiery, the player (once the magic component seller is unlocked) and Fluke. **Fluke can only cast luck spells** - any other spell fails or backfires on him.
- **Recipes: 3 unique components per spell** (decided 2026-11-20 - replaces the earlier "one key component plus group filler"). No component is used by two spells, so 30 of the 40 are used and 10 stay free (see Notes). Rare components only for the big spells.
- **Some spells need a 4th component, chosen by the player** from a group the spell names ("any bug"). It's only a cost and doesn't change the effect. Any component in the group counts, even one another recipe uses.
- **On auto**, the 4th-slot pick spends whichever component in the group the party has most of.
- **Cooldowns on the big spells only:** Doom (placeholder 4 turns) and Chain Lightning (placeholder 2). The rest can be cast every turn if the components are there.
- **Spells scale with the caster's level** ("a basic hit for the caster's level"). They don't level up themselves.
- **Durations:** buffs and debuffs last 3 turns, and Doom 2.
- **Hitting:** spells aimed at enemies roll to hit with the caster's accuracy (one roll per enemy). A miss still spends the components. Heals and buffs on allies always land. Mage Blast never misses.
- **Crits:** damage spells use the caster's crit chance. Mage Blast can't crit.
- **Missing components:** the spell is greyed out, and the menu shows what's missing.
- **Scroll prices** rise with each spell the player knows (placeholder: 100 gold, plus 100 per spell known).
- **Fiery starts with** Fireball, Frost Lance, Chain Lightning, Poison Cloud and Mage Blast. Fluke knows the 3 luck spells. The player learns from scrolls.
- **Permanent stat potions** (bottom of this file) need 4 components each and can share components with spells. The magic component seller crafts them for a fee.
- **How spells are learned** (decided 2026-11-20): from scrolls, bought or given by NPCs. Each town has one spell NPC. It offers up to 3 spells the player doesn't know yet (fewer if fewer are left). The player gets one of them at random, but can read all the descriptions first. It's rolled once per save. Scrolls teach the player only. Fiery has her own starting set, and Fluke knows the luck spells.

"A basic hit" means an average weapon hit for the caster's level (the same yardstick as Mage Blast, which does about half of one). Durations are 3 turns unless noted (decided 2026-11-20) - they depend on the buff-timing fix (section 1, Combat).

---

## Damage

| Spell | Recipe | Effect |
|---|---|---|
| **Fireball** | Rubeetles + Kellycones + Roseberries | One enemy, about 1.5x a basic hit. 30% chance to burn. |
| **Frost Lance** | Sapphider + Min + Blueberries | One enemy, about 1.2x a basic hit. Slows it (-4 speed for 3 turns). |
| **Chain Lightning** | Amberbees + Crystacricket + Draviteflies + *any bug* | Up to 3 enemies, about 0.7x a basic hit each. |
| **Poison Cloud** | Poisonshroom + Berylflies + Spotshroom + *any mushroom* | Poisons every enemy (the poison formula at the caster's level). |

## Support

| Spell | Recipe | Effect |
|---|---|---|
| **Mend** | Haleshroom + Honeyberries + Appleberries | Heals one ally about 25% of their max HP. |
| **Combat Elixir** | Strengthshroom + Shieldshroom + Laurel Scales | One ally: +20% damage and +20% defense. Replaces the Combat Elixir consumable (to-do, section 1). |
| **Purify** | Fairyshroom + Lavenjade + Mossmary | Removes every negative effect from one ally, including the Demon Prince's chicken. |

## Luck (Fluke can cast these)

| Spell | Recipe | Effect |
|---|---|---|
| **Fortune's Favor** | Horseshoe + Ladybug + Bamboo + *any lucky charm* | Whole party: +10% crit and +5% hit chance. |
| **Ill Omen** | Black Cat Fur + Broken Mirror + Opalant | One enemy: -15% hit chance and -10% crit. |
| **Doom** (rare) | Negativity Crystal + Raven Feather + Blackberries + *any unlucky charm* | For 2 rounds, every enemy roll is rolled twice and the worse one is kept. The big spell. |

*Italic* = the player's-choice 4th slot. I gave it to the 3 spells that hit or help more than one target, plus Doom. The single-target spells stay at 3.

## Permanent stat potions (crafted by the magic component seller)

The player brings the 4 components and pays her crafting fee. She doesn't sell these as shop stock. Components can be shared with spells. The amounts and caps are placeholders, as are the names.

| Potion (placeholder) | Recipe | Boost (placeholder) |
|---|---|---|
| **Vigor Draught** - Max HP | Haleshroom + Honeyberries + Melonberries + Forest Fern | +5 Max HP |
| **Swiftness Draught** - Speed | Berylflies + Nanaberries + Kombuthyme + Fish | +1 Speed |
| **Might Draught** - damage | Strengthshroom + Rubeetles + Roseberries + Acorns | +1% damage |
| **Iron Draught** - defense | Shieldshroom + Flatshroom + Laurel Scales + Jade | +1 defense |
| **Keen Draught** - crit | Ladybug + Crystacricket + Emerald Fronds + Bamboo | +1% crit |
| **Eagle-Eye Draught** - accuracy | Sapphider + Myrtle Needles + Blueberries + Spotshroom | +1% accuracy |

Why: healing mushroom and sweet fruit for health; fish are quick; oak (acorns) and strength for damage; jade is a guardian stone and the flat mushroom a shield; ladybug luck and a sharp crystal bug for crits; needles for precision and blueberries for eyesight for accuracy.

The potions use all 10 components no spell uses (Acorns, Fish, Jade, Flatshroom, Nanaberries, Melonberries, Forest Fern, Emerald Fronds, Kombuthyme, Myrtle Needles), so **every one of the 40 components now has a use**. Each potion has at least one component that only potions use.

---

## Notes

- **There are 40 components** (`spell_component.cs`): 1 crystal, 6 lucky, 3 unlucky, 7 mushrooms, 7 berries, 7 bugs and 9 herbs.
- **Why these matches** (my picks, change freely):
  - Fireball: ruby-red bug, pine cones burn, red berries.
  - Frost Lance: sapphire-blue spider, mint is cold, blue berries.
  - Chain Lightning: amber holds static, crystal conducts, dravite (tourmaline) makes electricity under pressure.
  - Poison Cloud: the poison mushroom, a green fly, the spotted toadstool.
  - Mend: "hale" means healthy, honey soothes, an apple a day.
  - Combat Elixir: strength plus shield, with laurels for victory.
  - Purify: fairy magic, with lavender and rosemary as cleansing herbs.
  - Fortune's Favor: classic lucky charms.
  - Ill Omen: classic bad luck, and opals are said to be unlucky.
  - Doom: the crystal, the raven as a death omen, and the folk tale that blackberries are cursed after Michaelmas.
- **The 10 components no spell uses** (Acorns, Fish, Jade, Flatshroom, Nanaberries, Melonberries, Forest Fern, Emerald Fronds, Kombuthyme, Myrtle Needles) all go into the potions.
- **The luck spells use all 3 unlucky components and 3 of the 6 lucky ones as fixed picks.** The other lucky ones can still fill Fortune's Favor's 4th slot.
- **Demand:** Bamboo, Ladybug, Rubeetles, Roseberries, Strengthshroom, Shieldshroom, Haleshroom, Honeyberries, Berylflies, Crystacricket, Laurel Scales, Sapphider, Blueberries and Spotshroom are each in one spell and one potion, so they'll be used most. That's worth keeping in mind for their drop rates and prices.
- **Fiery's wild magic** (her rules in `done.md`): when it triggers, a random other spell from this list comes out instead, paid for with the components of the spell she chose. Doom coming out by accident is the jackpot. Above 75 anger, about 1 in 4 wild spells backfires on the party.
- **Fluke:** his luck spells fit his Lucky Aura. A non-luck spell is a 50/50 gamble (decided 2026-11-19): half the time it fizzles (turn and components wasted), half the time it backfires on him (a damage spell hurts him, a buff goes to an enemy).
- **Ill Omen and the Demon Prince's Hexed** do similar things (lower hit chance). That's fine, but they could share one effect in the code.
