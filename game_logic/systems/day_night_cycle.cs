using System;
using GameLogic.Entities.Enemies;

namespace GameLogic.Systems
{
    public enum DayPhase
    {
        Day,
        Night
    }

    /// <summary>
    /// Day and night (designed by the user 2026-11-23 - see docs/done.md, "Day/night and the
    /// army camp"). A real-time countdown of 10 minutes per phase; a new game starts at day.
    /// - The clock only runs while the player is in the world (GameManager.AdvanceClock).
    /// - Battles, dialogue and cutscenes pause it, and when one ends a roll of -60 to +60
    ///   seconds is added to what's left (RollAfterPause). Menus, shops, the bag and the pause
    ///   screen pause it with no roll, so the clock can't be rerolled by opening the bag.
    /// - If a roll takes it to 0 or below, the phase flips as play resumes.
    /// - Inns and save campsites can switch the phase by hand (SwitchPhase), which starts a
    ///   full new countdown.
    /// - The phase and the time left are saved.
    /// Every flip starts a full 10 minutes (my default - no carry-over of a few seconds).
    /// No console output: listen to PhaseChanged (e.g. for the darker map tint in Godot).
    /// </summary>
    public class DayNightCycle
    {
        /// <summary>Length of each phase in real seconds (10 minutes).</summary>
        public const double PhaseLengthSeconds = 600;

        /// <summary>The roll after a battle, dialogue or cutscene: -60 to +60 seconds.</summary>
        public const int PauseRollSeconds = 60;

        // ----- Night enemies (decided 2026-11-23; the numbers are placeholders) -----

        /// <summary>Night enemies have +10% health and damage.</summary>
        public const double NightStrengthMultiplier = 1.10;

        /// <summary>Night enemies pay +20% XP and gold.</summary>
        public const double NightRewardMultiplier = 1.20;

        /// <summary>
        /// "One step more aggression": aggression is 0-100, so a step is +20 (my default -
        /// the steps were never defined), capped at 100.
        /// </summary>
        public const int NightAggressionStep = 20;

        public DayPhase Phase { get; private set; } = DayPhase.Day;

        /// <summary>Real seconds left until the phase flips.</summary>
        public double SecondsLeft { get; private set; } = PhaseLengthSeconds;

        public bool IsNight => Phase == DayPhase.Night;

        /// <summary>Fired with the new phase every time it flips (on its own or by hand).</summary>
        public event Action<DayPhase> PhaseChanged;

        /// <summary>
        /// The random numbers for the after-pause roll only, so the clock never changes the
        /// game's other rolls. Tests can set a seeded one.
        /// </summary>
        public RNGManager Rng
        {
            get => _rng;
            set => _rng = value ?? new RNGManager();
        }
        private RNGManager _rng = new RNGManager();

        public DayNightCycle()
        {
        }

        /// <summary>
        /// Restore a saved clock. A missing or out-of-range time (an old save) starts a full
        /// countdown.
        /// </summary>
        public DayNightCycle(DayPhase phase, double? secondsLeft)
        {
            Phase = phase;
            SecondsLeft = secondsLeft.HasValue && secondsLeft.Value > 0 && secondsLeft.Value <= PhaseLengthSeconds
                ? secondsLeft.Value
                : PhaseLengthSeconds;
        }

        /// <summary>
        /// Real time passing while the player is in the world. Flips the phase when the
        /// countdown reaches 0. GameManager.AdvanceClock only calls this while Playing.
        /// </summary>
        public void Advance(double seconds)
        {
            if (seconds <= 0 || double.IsNaN(seconds)) return;

            SecondsLeft -= seconds;
            if (SecondsLeft <= 0)
            {
                Flip();
            }
        }

        /// <summary>
        /// A battle, dialogue or cutscene has ended: add -60 to +60 seconds (every whole number
        /// possible) to what's left. If that takes it to 0 or below, the phase flips now, as play
        /// resumes. Returns the roll.
        /// </summary>
        public int RollAfterPause()
        {
            int roll = _rng.Roll(-PauseRollSeconds, PauseRollSeconds);
            SecondsLeft = Math.Min(SecondsLeft + roll, PhaseLengthSeconds + PauseRollSeconds);
            if (SecondsLeft <= 0)
            {
                Flip();
            }
            return roll;
        }

        /// <summary>
        /// Switch day/night by hand (at an inn or a save campsite, in the rest menu): flips the
        /// phase and starts a full new countdown.
        /// </summary>
        public void SwitchPhase()
        {
            Flip();
        }

        private void Flip()
        {
            Phase = Phase == DayPhase.Day ? DayPhase.Night : DayPhase.Day;
            SecondsLeft = PhaseLengthSeconds;
            PhaseChanged?.Invoke(Phase);
        }

        /// <summary>
        /// Make an enemy spawned at night a night enemy: +10% health and damage (rounded up),
        /// one step more aggression, and +20% XP and gold (EnemyBase.RewardMultiplier). Only
        /// once per enemy. Bosses are never night enemies. The difficulty still goes on top
        /// when the fight starts.
        /// </summary>
        public static void MakeNightEnemy(EnemyBase enemy)
        {
            if (enemy == null || enemy.IsNightEnemy || enemy is Entities.Enemies.Bosses.BossEnemy) return;

            enemy.IsNightEnemy = true;

            enemy.MaxHealth = ScaleUp(enemy.MaxHealth, NightStrengthMultiplier);
            enemy.Health = enemy.MaxHealth;
            enemy.MinDamage = ScaleUp(enemy.MinDamage, NightStrengthMultiplier);
            enemy.MaxDamage = Math.Max(enemy.MinDamage, ScaleUp(enemy.MaxDamage, NightStrengthMultiplier));

            enemy.AggressionLevel = Math.Min(100, enemy.AggressionLevel + NightAggressionStep);
            enemy.RewardMultiplier *= NightRewardMultiplier;
        }

        private static int ScaleUp(int value, double multiplier)
        {
            return (int)Math.Ceiling(Math.Round(value * multiplier, 6));
        }
    }
}
