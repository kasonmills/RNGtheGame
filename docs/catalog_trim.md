# Catalog trim - keep-list (draft for review)

**Status: draft, 2026-11-19 - for the user to review and edit.** Once approved, the build is in `to_do_list.md` (section 1, Items, loot and magic). Items not on this list stay in `item_database.cs` but dormant (no drops, no shop stock), so they can come back later.

**The shape** (decided 2026-11-19 - see `done.md`, "Catalog trim"):
- **Weapons:** all 9 types stay, 6 each = **54**: 2 Common, 2 Uncommon, 1 Rare, 1 Legendary per type. Boon's Crossbow is a signature item on top of that (not a drop).
- **Armor:** 4 weight types, 6 each = **24**: 2 Common, 2 Uncommon, 1 Rare, 1 Legendary per type. Robe and Shield go dormant. Speed cost (placeholders): Cloth 0%, Leather -5%, Chainmail -10%, Plate -20%.

**How I picked**
- Every item the code refers to by name stays (marked 🔒): the four starting weapons (`game_startup.cs`) and the companions' starting gear.
- The two Commons and the two Uncommons are spread low/high, so there's an upgrade within each tier.
- The one Rare per type is a middle regular-Rare item. **Every former-Epic weapon (marked `was Epic` in the code) is cut** - they sat between Rare and Legendary, a tier that no longer exists.
- Armor has no Legendary items today, so 4 former-Epic armor sets are **promoted to Legendary** to fill that gap (marked ⬆).
- Names tied to other franchises are left out (see the bottom of this file).

---

## Weapons (54)

Stats are at level 1: damage, crit %, accuracy %.

### Sword (Blades)
| Rarity | Item | Damage | Crit | Acc |
|---|---|---|---|---|
| Common | Rusty Sword 🔒 (starting weapon) | 3-7 | 5 | 70 |
| Common | Iron Sword | 5-12 | 8 | 75 |
| Uncommon | Steel Sword | 10-20 | 10 | 80 |
| Uncommon | Bastard Sword | 13-22 | 10 | 78 |
| Rare | Flamberge | 16-30 | 13 | 83 |
| Legendary | Excalibur | 32-58 | 26 | 96 |

### Dagger (Blades)
| Rarity | Item | Damage | Crit | Acc |
|---|---|---|---|---|
| Common | Rusty Dagger 🔒 (starting weapon) | 2-5 | 15 | 80 |
| Common | Stiletto | 3-6 | 17 | 82 |
| Uncommon | Kris | 5-9 | 22 | 83 |
| Uncommon | Main Gauche | 6-11 | 21 | 86 |
| Rare | Fang of the Serpent | 10-19 | 29 | 86 |
| Legendary | Bloodletter | 24-45 | 44 | 92 |

### Axe (Heavy)
| Rarity | Item | Damage | Crit | Acc |
|---|---|---|---|---|
| Common | Hand Axe | 5-10 | 12 | 69 |
| Common | Bearded Axe | 6-12 | 11 | 67 |
| Uncommon | Double-Bladed Axe | 13-23 | 15 | 69 |
| Uncommon | Battle Axe | 12-25 | 15 | 70 |
| Rare | Thunderstrike Axe | 17-36 | 19 | 73 |
| Legendary | Ragnarok's Edge | 31-61 | 28 | 81 |

### Mace (Heavy)
| Rarity | Item | Damage | Crit | Acc |
|---|---|---|---|---|
| Common | Wooden Club | 6-12 | 15 | 62 |
| Common | Flanged Mace | 8-13 | 16 | 61 |
| Uncommon | Spiked Mace | 10-18 | 20 | 58 |
| Uncommon | Maul | 13-21 | 20 | 56 |
| Rare | Meteor Hammer | 18-32 | 26 | 63 |
| Legendary | The Peacemaker | 31-59 | 37 | 70 |

### Spear (Heavy)
| Rarity | Item | Damage | Crit | Acc |
|---|---|---|---|---|
| Common | Hunting Spear | 5-11 | 16 | 85 |
| Common | Pike | 6-12 | 17 | 86 |
| Uncommon | War Spear | 8-16 | 18 | 88 |
| Uncommon | Naginata | 10-18 | 20 | 89 |
| Rare | Celestial Halberd | 16-29 | 23 | 91 |
| Legendary | Gae Bolg | 28-54 | 35 | 99 |

### Bow (Ranged)
| Rarity | Item | Damage | Crit | Acc |
|---|---|---|---|---|
| Common | Hunting Bow 🔒 (starting weapon) | 4-9 | 10 | 75 |
| Common | Yew Bow | 5-11 | 12 | 78 |
| Uncommon | Longbow | 9-17 | 12 | 80 |
| Uncommon | Elven Bow | 10-18 | 14 | 82 |
| Rare | Windcaller Bow | 15-32 | 19 | 85 |
| Legendary | Failnaught | 29-64 | 31 | 95 |

### Crossbow (Ranged)
| Rarity | Item | Damage | Crit | Acc |
|---|---|---|---|---|
| Common | Light Crossbow | 6-12 | 5 | 85 |
| Common | Arbalest | 7-14 | 6 | 87 |
| Uncommon | Heavy Crossbow | 10-20 | 7 | 88 |
| Uncommon | Dwarven Crossbow | 12-22 | 9 | 90 |
| Rare | Dragonfire Crossbow | 16-34 | 12 | 93 |
| Legendary | Ballista Prime | 32-67 | 23 | 98 |
| *Signature* | Boon's Crossbow 🔒 (Uncommon, not a drop) | 6-12 | 5 | 95 |

Boon's Crossbow is Uncommon but has Common-level damage today (6-12). The decision is Uncommon-level damage, so it needs raising when it's built (e.g. to about 10-20, like the Heavy Crossbow).

### Staff (Arcane)
| Rarity | Item | Damage | Crit | Acc |
|---|---|---|---|---|
| Common | Wooden Staff 🔒 (starting weapon, Fiery) | 2-6 | 3 | 85 |
| Common | Quarterstaff 🔒 (Felicity) | 4-8 | 5 | 87 |
| Uncommon | Battle Staff | 6-14 | 7 | 90 |
| Uncommon | Moonstone Staff | 8-16 | 8 | 93 |
| Rare | Stormcaller Staff | 12-26 | 11 | 95 |
| Legendary | Cosmos Staff | 27-54 | 21 | 100 |

### Wand (Arcane)
| Rarity | Item | Damage | Crit | Acc |
|---|---|---|---|---|
| Common | Apprentice Wand | 4-7 | 8 | 82 |
| Common | Rowan Wand | 4-8 | 8 | 82 |
| Uncommon | Arcane Wand | 8-12 | 10 | 85 |
| Uncommon | Crystal Wand | 9-13 | 12 | 87 |
| Rare | Thunderwood Wand | 14-20 | 18 | 90 |
| Legendary | Reality Warp Wand | 29-44 | 31 | 100 |

---

## Armor (24)

Defense at level 1.

### Cloth (light, no speed cost)
| Rarity | Item | Defense |
|---|---|---|
| Common | Tattered Cloth Set 🔒 (Fiery) | 3 |
| Common | Rough Linen Set 🔒 (Felicity) | 5 |
| Uncommon | Durable Linen Set | 14 |
| Uncommon | Reinforced Silk Garb | 18 |
| Rare | Windwalker's Vestments | 28 |
| Legendary | Voidweave Vestments ⬆ (was Epic) | 50 |

### Leather (-5% speed)
| Rarity | Item | Defense |
|---|---|---|
| Common | Worn Leather Armor 🔒 (Boon) | 9 |
| Common | Studded Leather Armor | 15 |
| Uncommon | Hardened Leather Set | 18 |
| Uncommon | Riveted Leather Armor | 27 |
| Rare | Shadowstep Armor | 39 |
| Legendary | Wyvern Leather Set ⬆ (was Epic) | 65 |

### Chainmail (-10% speed)
| Rarity | Item | Defense |
|---|---|---|
| Common | Rusty Chainmail Set 🔒 (Risk) | 15 |
| Common | Soldier's Chainmail | 22 |
| Uncommon | Knight's Chainmail | 34 |
| Uncommon | Masterwork Chainmail | 40 |
| Rare | Mithril Chain Set | 52 |
| Legendary | Titan's Chainmail ⬆ (was Epic) | 77 |
| *Signature* | Royal Guard Mail (new, placeholder name - Uncommon, not a drop) | about 30 |

Royal Guard Mail is new (decided 2026-11-19): Risk's signature starting armor, replacing the Rusty Chainmail Set on him. It's the armor that leaves with him and gets the signature-item swap warning. Its name gets fixed in the writing pass, together with his "Royal Guard's Armor" block ability 🤫. The Rusty Chainmail Set stays in the list as a normal Common.

### Plate (-20% speed)
| Rarity | Item | Defense |
|---|---|---|
| Common | Battered Plate Set | 25 |
| Common | Foot Soldier's Plate | 30 |
| Uncommon | Knight's Plate Armor | 43 |
| Uncommon | Masterwork Plate Set | 55 |
| Rare | General's War Plate | 78 |
| Legendary | Dreadnought Plate ⬆ (was Epic) | 103 |

---

## Notes

- **Names left out on purpose** (tied to other franchises): Elder Wand and Deathstick (Harry Potter), Frostmourne's Grip (Warcraft), Jarnbjorn and Stormbreaker Sword (Marvel), Hellsing, Staff of the Magi and Dragon Lance (D&D). Mjolnir was skipped too, as it's now strongly tied to Marvel. Names from mythology and history (Excalibur, Gae Bolg, Failnaught, Ragnarok) are fine. Some cut items with these names could come back under new names.
- **Starting weapon choice:** the four starting weapons (sword, bow, dagger, staff) are all weak Commons, as they should be. No starting axe, mace, spear, crossbow or wand - unchanged.
- **The Rogue** (post-MVP) uses Rusty Dagger and Worn Leather Armor, which both stay.
- **Plate's two Commons (25 and 30)** out-defend every Uncommon Cloth and Leather set. That's the speed trade working as intended, but worth checking in play.
