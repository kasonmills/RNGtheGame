using System;
using System.Collections.Generic;
using GameLogic.Entities.Enemies;

namespace GameLogic.Progression
{
    /// <summary>
    /// Defines all 15 champion bosses in the game
    /// Each boss has unique mechanics, appearance, and drops a specific key
    /// </summary>
    public static class BossDefinitions
    {
        /// <summary>
        /// Create and return all 15 champion bosses
        /// </summary>
        public static List<BossEnemy> GetAllChampionBosses()
        {
            var bosses = new List<BossEnemy>();

            // Boss 1: Flame Warden
            var flameWarden = new BossEnemy(
                bossId: "flame_warden",
                name: "Ignis the Eternal",
                title: "Flame Warden, Guardian of the Eternal Pyre",
                description: "An ancient fire elemental bound to protect the sacred flames. Its body blazes with heat that could melt steel, and its fury burns hotter with each passing moment.",
                keyId: "flame_warden_key",
                level: 15,
                mechanicType: BossMechanicType.Enrage
            );
            flameWarden.InitializeStats(15);
            bosses.Add(flameWarden);

            // Boss 2: Frost Tyrant
            var frostTyrant = new BossEnemy(
                bossId: "frost_tyrant",
                name: "Glacius the Merciless",
                title: "Frost Tyrant, Lord of the Frozen Wastes",
                description: "A cruel ice giant who brings eternal winter wherever he treads. His frozen heart knows no mercy, and his touch brings death by ice.",
                keyId: "frost_tyrant_key",
                level: 18,
                mechanicType: BossMechanicType.Defensive
            );
            frostTyrant.InitializeStats(18);
            bosses.Add(frostTyrant);

            // Boss 3: Thunder Lord
            var thunderLord = new BossEnemy(
                bossId: "thunder_lord",
                name: "Voltarion the Stormbringer",
                title: "Thunder Lord, Master of Lightning",
                description: "Commands the very storms themselves. Lightning crackles around his form, and thunder announces his presence. His attacks strike with the speed and fury of a tempest.",
                keyId: "thunder_lord_key",
                level: 20,
                mechanicType: BossMechanicType.Speed
            );
            thunderLord.InitializeStats(20);
            bosses.Add(thunderLord);

            // Boss 4: Shadow Reaper
            var shadowReaper = new BossEnemy(
                bossId: "shadow_reaper",
                name: "Umbra the Unseen",
                title: "Shadow Reaper, Harvester of Souls",
                description: "Born from the void between light and dark. It exists partially in shadow, making it difficult to strike. Those who fall to it are consumed by darkness.",
                keyId: "shadow_reaper_key",
                level: 16,
                mechanicType: BossMechanicType.Counter
            );
            shadowReaper.InitializeStats(16);
            bosses.Add(shadowReaper);

            // Boss 5: Stone Guardian
            var stoneGuardian = new BossEnemy(
                bossId: "stone_guardian",
                name: "Terrak the Unbreaking",
                title: "Stone Guardian, Protector of Ancient Ruins",
                description: "An enormous construct of living stone, built in an age long forgotten. Nearly impervious to damage, it has stood vigil for millennia.",
                keyId: "stone_guardian_key",
                level: 17,
                mechanicType: BossMechanicType.Defensive
            );
            stoneGuardian.InitializeStats(17);
            bosses.Add(stoneGuardian);

            // Boss 6: Serpent Queen
            var serpentQueen = new BossEnemy(
                bossId: "serpent_queen",
                name: "Venara the Viperous",
                title: "Serpent Queen, Empress of Poison",
                description: "A massive serpent whose venom can dissolve stone. Her scales are impenetrable, and her bite brings certain death. Legends say she has lived for over a thousand years.",
                keyId: "serpent_queen_key",
                level: 19,
                mechanicType: BossMechanicType.Draining
            );
            serpentQueen.InitializeStats(19);
            bosses.Add(serpentQueen);

            // Boss 7: Iron Colossus
            var ironColossus = new BossEnemy(
                bossId: "iron_colossus",
                name: "Ferrum the Indestructible",
                title: "Iron Colossus, War Machine of the Ancients",
                description: "A towering mechanical giant forged in a forgotten war. Its iron plating has never been pierced, and its strength could topple mountains.",
                keyId: "iron_colossus_key",
                level: 22,
                mechanicType: BossMechanicType.Berserker
            );
            ironColossus.InitializeStats(22);
            bosses.Add(ironColossus);

            // Boss 8: Blood Knight
            var bloodKnight = new BossEnemy(
                bossId: "blood_knight",
                name: "Sanguis the Crimson",
                title: "Blood Knight, Champion of the Crimson Covenant",
                description: "A warrior who made a pact with dark forces. His armor is stained with the blood of thousands, and he grows stronger with each life he takes.",
                keyId: "blood_knight_key",
                level: 21,
                mechanicType: BossMechanicType.Draining
            );
            bloodKnight.InitializeStats(21);
            bosses.Add(bloodKnight);

            // Boss 9: Arcane Archon
            var arcaneArchon = new BossEnemy(
                bossId: "arcane_archon",
                name: "Mystara the All-Knowing",
                title: "Arcane Archon, Mistress of Pure Magic",
                description: "Pure magical energy given form and consciousness. She wields the fundamental forces of magic itself, bending reality to her will.",
                keyId: "arcane_archon_key",
                level: 24,
                mechanicType: BossMechanicType.MultiPhase
            );
            arcaneArchon.InitializeStats(24);
            bosses.Add(arcaneArchon);

            // Boss 10: Plague Bearer
            var plagueBearer = new BossEnemy(
                bossId: "plague_bearer",
                name: "Pestilus the Festering",
                title: "Plague Bearer, Herald of Disease",
                description: "A twisted creature born from pandemic and suffering. Its very presence spreads corruption and decay. The ground rots where it walks.",
                keyId: "plague_bearer_key",
                level: 20,
                mechanicType: BossMechanicType.Summoner
            );
            plagueBearer.InitializeStats(20);
            bosses.Add(plagueBearer);

            // Boss 11: Sky Sovereign
            var skySovereign = new BossEnemy(
                bossId: "sky_sovereign",
                name: "Aethon the Celestial",
                title: "Sky Sovereign, Ruler of the Heavens",
                description: "A divine being that dwells among the clouds. Its wings span the sky, and it can call down celestial judgment upon its enemies.",
                keyId: "sky_sovereign_key",
                level: 23,
                mechanicType: BossMechanicType.Speed
            );
            skySovereign.InitializeStats(23);
            bosses.Add(skySovereign);

            // Boss 12: Abyssal Horror
            var abyssalHorror = new BossEnemy(
                bossId: "abyssal_horror",
                name: "Leviathan the Depths",
                title: "Abyssal Horror, Terror from the Deep",
                description: "An ancient sea monster from the darkest oceanic trenches. Its tentacles can crush ships, and its maw could swallow a whale whole.",
                keyId: "abyssal_horror_key",
                level: 25,
                mechanicType: BossMechanicType.Regeneration
            );
            abyssalHorror.InitializeStats(25);
            bosses.Add(abyssalHorror);

            // Boss 13: Solar Phoenix
            var solarPhoenix = new BossEnemy(
                bossId: "solar_phoenix",
                name: "Solara the Reborn",
                title: "Solar Phoenix, Eternal Flame of Rebirth",
                description: "A legendary phoenix that has died and been reborn countless times. Its flames burn with the intensity of the sun, and it cannot truly be killed.",
                keyId: "solar_phoenix_key",
                level: 26,
                mechanicType: BossMechanicType.Regeneration
            );
            solarPhoenix.InitializeStats(26);
            bosses.Add(solarPhoenix);

            // Boss 14: Void Dragon
            var voidDragon = new BossEnemy(
                bossId: "void_dragon",
                name: "Nihilus the Eternal Void",
                title: "Void Dragon, Devourer of Dimensions",
                description: "A dragon that exists between dimensions. It can phase in and out of reality, making it nearly impossible to land a hit. Its breath erases matter from existence.",
                keyId: "void_dragon_key",
                level: 28,
                mechanicType: BossMechanicType.MultiPhase
            );
            voidDragon.InitializeStats(28);
            bosses.Add(voidDragon);

            // Boss 15: Time Keeper
            var timeKeeper = new BossEnemy(
                bossId: "time_keeper",
                name: "Chronos the Ageless",
                title: "Time Keeper, Guardian of the Eternal Flow",
                description: "The embodiment of time itself. It can manipulate the flow of time, reversing wounds and aging enemies to dust. To fight it is to fight inevitability.",
                keyId: "time_keeper_key",
                level: 30,
                mechanicType: BossMechanicType.TimeLimit
            );
            timeKeeper.InitializeStats(30);
            bosses.Add(timeKeeper);

            return bosses;
        }

        /// <summary>
        /// Get a specific boss by ID
        /// </summary>
        public static BossEnemy GetBoss(string bossId)
        {
            var bosses = GetAllChampionBosses();
            return bosses.Find(b => b.BossId == bossId);
        }

        /// <summary>
        /// Get boss names and levels for display
        /// </summary>
        public static string GetBossListSummary()
        {
            var bosses = GetAllChampionBosses();
            string summary = "═══ CHAMPION BOSSES ═══\n";
            summary += "Defeat any 10 to unlock the Final Gate\n\n";

            foreach (var boss in bosses)
            {
                summary += $"• {boss.Name} (Lv.{boss.Level}) - {boss.MechanicType}\n";
            }

            return summary;
        }
    }
}