using GameLogic.Systems;

namespace GameLogic.Entities.NPCs.Companions
{
    /// <summary>
    /// Chance's two forms. He is a cat until the Demon Prince (boss #6); human form and
    /// switching between the two come later and aren't built yet.
    /// </summary>
    public enum ChanceForm
    {
        Cat,
        Human
    }

    /// <summary>
    /// Chance - the player's first companion, recruited in the tutorial (added 2026-10-21)
    /// Cat form: no weapon or armor; his attack rolls evenly from his level up to 100
    /// (level 1: 1-100, level 15: 15-100, level 30: 30-100)
    /// Unique Ability: none yet - form switching arrives after the Demon Prince
    /// </summary>
    public class CompanionChance : CompanionBase
    {
        /// <summary>
        /// Top of the cat's damage roll - it never changes; only the bottom rises with his level
        /// </summary>
        public const int CatMaxDamage = 100;

        public ChanceForm Form { get; private set; } = ChanceForm.Cat;

        public CompanionChance(int startingLevel = 1)
            : base("Chance", startingLevel, CompanionAIStyle.Aggressive)
        {
            Description = "A black cat with an uncanny sense of timing.";
            Greeting = "*The black cat stares at you, then falls in beside you as if it had always been there.*";

            AddDialogue("*Chance flicks his tail.*");
            AddDialogue("*Chance's ears go flat.*");
            AddDialogue("*Chance stretches, unbothered.*");

            // Starting stats - placeholder numbers, to be tuned
            MaxHealth = 80 + (startingLevel * 8);
            Health = MaxHealth;
            Speed = 14 + (startingLevel / 3); // Cats are quick
        }

        /// <summary>
        /// Lowest the cat can roll: his level, capped at the top of the range
        /// </summary>
        public int CatMinDamage => System.Math.Max(1, System.Math.Min(Level, CatMaxDamage));

        /// <summary>
        /// Cat form: an even roll from his level up to 100, with no extra level bonus on top.
        /// Human form is not built yet and falls back to the normal weapon roll.
        /// </summary>
        public override int RollAttackDamage(RNGManager rng)
        {
            if (Form == ChanceForm.Cat)
            {
                return rng.Roll(CatMinDamage, CatMaxDamage);
            }
            return base.RollAttackDamage(rng);
        }

        // Cat form fights with claws - placeholder numbers, to be tuned
        public override int AttackAccuracy => Form == ChanceForm.Cat ? 85 : base.AttackAccuracy;
        public override int AttackCritChance => Form == ChanceForm.Cat ? 5 : base.AttackCritChance;

        /// <summary>
        /// Chance levels up - his cat damage floor rises by 1 every level
        /// </summary>
        public override void LevelUp()
        {
            base.LevelUp(); // Handle health and base stats

            if (Level % 3 == 0)
            {
                Speed++;
                GameLog.Write($"{Name} got quicker! Speed increased to {Speed}!");
            }

            if (Form == ChanceForm.Cat)
            {
                GameLog.Write($"{Name}'s claws now hit for {CatMinDamage}-{CatMaxDamage}.");
            }
        }
    }
}
