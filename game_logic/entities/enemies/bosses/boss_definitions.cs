using System;
using System.Collections.Generic;

namespace GameLogic.Entities.Enemies.Bosses
{
    /// <summary>
    /// Defines the bosses in the game, in fixed story order.
    /// The original 15-boss roster (procedural-map era) was pulled from here on 2026-09-11
    /// after the scope was reduced to a fixed, story-driven map with specific spawn locations.
    /// That original roster is archived at docs/boss_ideas_archive.md for inspiration - none
    /// of it is wired into the active game right now.
    /// The confirmed roster is 9 bosses total; all 9 bosses exist below (#4/#5, the Cyclops Brothers, are one entry)
    /// - most names and text are still placeholders.
    /// </summary>
    public static class BossDefinitions
    {
        /// <summary>
        /// Create and return the bosses, in fixed story order. Order matters: BossManager
        /// derives its order-based unlock progression (defeat boss N-1 to unlock boss N) and
        /// its final-boss identity (always the last entry) directly from this list's order.
        /// All 9 bosses are here (the Cyclops Brothers are one entry). See docs/boss_ideas_archive.md
        /// for the retired 15-boss list this replaced.
        /// </summary>
        public static List<BossEnemy> GetAllBosses()
        {
            return new List<BossEnemy>
            {
                // Boss #1 - the tutorial fight (see TutorialManager, game_logic/core/tutorial_manager.cs).
                // Level/mechanic are a first-pass balance choice - easy to retune after playtesting.
                // MechanicType is flavor/display text only - Mechanic is the real implementation
                // (see EagleBearMechanic, game_logic/entities/enemies/bosses/boss_mechanic.cs).
                new BossEnemy(
                    bossId: BossStatSheet.Skarn,
                    name: "Skarn",
                    title: "the Eagle Bear",
                    description: "A monstrous fusion of eagle and bear - taloned wings and a crushing, "
                        + "furred bulk - that struck from the treeline while the princess's escort was ambushed.",
                    level: 2,
                    mechanicType: BossMechanicType.MultiPhase)
                {
                    Mechanic = new EagleBearMechanic(),
                    // The tutorial boss keeps his own level - every later boss follows the party
                    // (average level + 5) - decided 2026-11-10
                    HasFixedLevel = true
                },

                // Boss #2 - the Librarian-Monk. Fights at the party's average level + 5 (the
                // level here is only his starting value). Shield mechanic: LibrarianMonkMechanic.
                // PLACEHOLDER text: his name, title and description aren't decided yet (to-do
                // section 2), and neither is where he is fought (needs the fixed map).
                new BossEnemy(
                    bossId: BossStatSheet.LibrarianMonk,
                    name: "Librarian-Monk",
                    title: "Keeper of the Library",
                    description: "A robed monk who guards the library's shelves, hiding behind a shimmering shield.",
                    level: 8,
                    mechanicType: BossMechanicType.Defensive)
                {
                    Mechanic = new LibrarianMonkMechanic(),
                    UniqueAbilityDescription = "Raises a shield that halves every hit until it breaks, and raises it again."
                },

                // Boss #3 - the Giant House Centipede. Fights at the party's average level + 5.
                // Poison bite mechanic: GiantCentipedeMechanic.
                // PLACEHOLDER text: his name, title and description aren't decided yet (to-do
                // section 2), and neither is where he is fought (needs the fixed map).
                new BossEnemy(
                    bossId: BossStatSheet.Centipede,
                    name: "Giant House Centipede",
                    title: "the Many-Legged",
                    description: "A house centipede grown to monstrous size, fast on its countless legs, with a venomous bite.",
                    level: 12,
                    mechanicType: BossMechanicType.Standard)
                {
                    Mechanic = new GiantCentipedeMechanic(),
                    UniqueAbilityDescription = "Every bite may poison. Poison stacks, and he goes after those not poisoned yet."
                },

                // Bosses #4/#5 - the Cyclops Brothers, one fight and one place in the boss order.
                // Fast (the faster brother, speed 9) is the boss in the list; Slow (speed 7) comes
                // with him as his PartnerBoss. Both follow the party's level. Mechanic:
                // CyclopsBrotherMechanic (one copy each).
                // PLACEHOLDER names and text: "Fast" and "Slow" stand in until their real names
                // are picked (decided 2026-11-11 - the real names come later).
                new BossEnemy(
                    bossId: BossStatSheet.CyclopsBrotherOne,
                    name: "Fast",
                    title: "the Elder Cyclops Brother",
                    description: "One of two hulking cyclops brothers who never fight apart.",
                    level: 16,
                    mechanicType: BossMechanicType.Standard)
                {
                    Mechanic = new CyclopsBrotherMechanic(),
                    UniqueAbilityDescription = "Always attacks together with his brother, and brings him back if he falls.",
                    PartnerBoss = new BossEnemy(
                        bossId: BossStatSheet.CyclopsBrotherTwo,
                        name: "Slow",
                        title: "the Younger Cyclops Brother",
                        description: "One of two hulking cyclops brothers who never fight apart.",
                        level: 16,
                        mechanicType: BossMechanicType.Standard)
                    {
                        Mechanic = new CyclopsBrotherMechanic(),
                        UniqueAbilityDescription = "Always attacks together with his brother, and brings him back if he falls."
                    }
                },

                // Boss #6 - the Demon Prince (chaos-magic themed). Fights at the party's average
                // level + 5. Mechanic: DemonPrinceMechanic - a random condition on every hit, from
                // the existing conditions until his unique ones are designed.
                // PLACEHOLDER title and description (to-do section 2), and where he is fought.
                new BossEnemy(
                    bossId: BossStatSheet.DemonPrince,
                    name: "Demon Prince",
                    title: "Prince of Chaos",
                    description: "A demon prince steeped in chaos magic. Every blow he lands leaves something behind.",
                    level: 21,
                    mechanicType: BossMechanicType.Standard)
                {
                    Mechanic = new DemonPrinceMechanic(),
                    UniqueAbilityDescription = "Every hit leaves a random condition on its target."
                },

                // Boss #7 - the Slime Dragon. Fights at the party's average level + 5. Mechanic:
                // SlimeDragonMechanic - a dragling for every hit he takes (at most 6 alive), topped
                // back up to 3 at the end of a round. Beating him stops the spawning, but every
                // dragling still has to be beaten to win.
                // PLACEHOLDER title and description (to-do section 2), and where he is fought.
                new BossEnemy(
                    bossId: BossStatSheet.SlimeDragon,
                    name: "Slime Dragon",
                    title: "the Ever-Dripping",
                    description: "A hulking dragon of living slime. Every wound splits off another squirming dragling.",
                    level: 26,
                    mechanicType: BossMechanicType.Summoner)
                {
                    Mechanic = new SlimeDragonMechanic(),
                    UniqueAbilityDescription = "Every hit on him spawns a dragling, and he keeps at least 3 around."
                },

                // Boss #8 - the Captain of the Royal Guard (Risk as a boss). Fights at the party's
                // average level + 5. Mechanic: CaptainMechanic - very high crit chance by difficulty
                // at 2.5x. A standalone boss for now: turning the companion Risk into this boss
                // (leave-party trigger, his gear) is a separate to-do. Never show WHY he leaves in
                // game text (design docs only).
                new BossEnemy(
                    bossId: BossStatSheet.Captain,
                    name: "Risk",
                    title: "Captain of the Royal Guard",
                    description: "The captain of the royal guard, quick and deadly - his blows find the weak spots.",
                    level: 32,
                    mechanicType: BossMechanicType.Berserker)
                {
                    Mechanic = new CaptainMechanic(),
                    UniqueAbilityDescription = "Strikes fast, and most of his hits are critical."
                },

                // Boss #9 - the final boss: the Princess, a hidden twist - no foreshadowing text
                // anywhere. Always last in this list (BossManager treats the last entry as the
                // final boss). Fights at the party's average level + 5 (max 45). Mechanic:
                // FinalBossMechanic - two turns a round, forced skips, castle guards.
                // PLACEHOLDER name, title and description (to-do section 2). Her information is
                // only ever shown once she is the next boss (BossManager.GetBossList).
                new BossEnemy(
                    bossId: BossStatSheet.FinalBoss,
                    name: "The Princess",
                    title: "Heir to the Throne",
                    description: "The princess, crowned in cold resolve, with the royal guard at her command.",
                    level: 45,
                    mechanicType: BossMechanicType.Speed)
                {
                    Mechanic = new FinalBossMechanic(),
                    UniqueAbilityDescription = "Acts twice a round, can freeze you in place, and calls her castle guards."
                }
            };
        }

        /// <summary>
        /// Get a specific boss by ID
        /// </summary>
        public static BossEnemy GetBoss(string bossId)
        {
            var bosses = GetAllBosses();
            return bosses.Find(b => b.BossId == bossId);
        }

        // Commented out 2026-11-11: it lists every boss by name, which breaks the rule that only
        // the next boss's information is shown (nothing called it). Use BossManager.GetBossList().
        // /// <summary>
        // /// Get boss names and levels for display
        // /// </summary>
        // public static string GetBossListSummary()
        // {
        //     var bosses = GetAllBosses();
        //     string summary = "═══ BOSSES ═══\n\n";
        //
        //     foreach (var boss in bosses)
        //     {
        //         summary += $"• {boss.Name} (Lv.{boss.Level}) - {boss.MechanicType}\n";
        //     }
        //
        //     return summary;
        // }
    }
}
