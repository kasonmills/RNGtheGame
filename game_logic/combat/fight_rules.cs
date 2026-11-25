namespace GameLogic.Combat
{
    /// <summary>
    /// Special rules for one fight, passed to CombatManager.StartCombat. A normal fight uses
    /// the defaults (FightRules.Normal), which change nothing. The army camp's waves use
    /// FightRules.ArmyCampWave (rules decided 2026-11-23).
    /// </summary>
    public class FightRules
    {
        /// <summary>The party can choose Flee (bosses refuse it anyway).</summary>
        public bool FleeAllowed { get; init; } = true;

        /// <summary>The party can use items from the bag.</summary>
        public bool ItemsAllowed { get; init; } = true;

        /// <summary>The party can choose Surrender (ends the fight at once, outcome Surrendered).</summary>
        public bool SurrenderAllowed { get; init; }

        /// <summary>
        /// The XP, gold and loot are given out when the fight is won. False: the XP and gold are
        /// still worked out and put in the CombatResult, but nobody gets them and no items
        /// drop - the caller decides what to do with them (the army camp's pot).
        /// </summary>
        public bool PayRewards { get; init; } = true;

        /// <summary>Everyone down gets back up at 1 HP when the fight is won (decided 2026-11-23).</summary>
        public bool ReviveOnWin { get; init; } = true;

        /// <summary>
        /// The companions passed to StartCombat are the fighters, as given (up to 3, the ones
        /// still standing), whether or not they are in the party; nobody sits out for half XP.
        /// For a lineup picked before the fight (the army camp).
        /// </summary>
        public bool FixedLineup { get; init; }

        /// <summary>
        /// Casting spells doesn't spend components (the army camp, decided 2026-11-23). Nothing
        /// reads this yet - the spell build must check it, including the "greyed out without
        /// components" check.
        /// </summary>
        public bool ComponentsAreFree { get; init; }

        /// <summary>A normal fight.</summary>
        public static FightRules Normal => new FightRules();

        /// <summary>
        /// One wave of the army camp challenge: no Flee (Surrender instead), no items, the
        /// rewards go to the pot, the fallen stay down, the lineup is fixed, spells are free.
        /// </summary>
        public static FightRules ArmyCampWave => new FightRules
        {
            FleeAllowed = false,
            ItemsAllowed = false,
            SurrenderAllowed = true,
            PayRewards = false,
            ReviveOnWin = false,
            FixedLineup = true,
            ComponentsAreFree = true
        };
    }
}
