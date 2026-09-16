using System;

namespace GameLogic.World
{
    /// <summary>
    /// Wraps one enemy currently spawned in the player's overworld route/area.
    /// This is engine-agnostic data/logic only - no physics or hitboxes yet.
    /// TODO-GODOT: the route/area scene should place one visible enemy per active
    /// spawn, move it around, and drive combat via the collision hook below.
    /// </summary>
    public class OverworldEnemySpawn
    {
        private const int AggressionVarianceMax = 15; // +/- variance applied to the enemy's baseline AggressionLevel
        private const float BaseDetectionRange = 3.0f; // world units, tunable once Godot scenes exist
        private const float DetectionRangeVarianceMax = 1.0f;

        public Entities.Enemies.EnemyBase Enemy { get; }
        public int EffectiveAggression { get; }
        public float DetectionRange { get; }
        public bool IsCharging { get; private set; }

        public OverworldEnemySpawn(Entities.Enemies.EnemyBase enemy, Systems.RNGManager rng)
        {
            Enemy = enemy;

            int aggressionVariance = rng.Roll(-AggressionVarianceMax, AggressionVarianceMax);
            EffectiveAggression = Math.Clamp(enemy.AggressionLevel + aggressionVariance, 0, 100);

            // More aggressive enemies notice the player from farther away, plus its own random variance
            float aggressionBonus = (EffectiveAggression / 100f) * DetectionRangeVarianceMax;
            float rangeVariance = rng.Range(-DetectionRangeVarianceMax, DetectionRangeVarianceMax);
            DetectionRange = Math.Max(0.5f, BaseDetectionRange + aggressionBonus + rangeVariance);
        }

        /// <summary>
        /// Pure decision logic for whether this enemy has decided to charge the player.
        /// TODO-GODOT: call every tick/frame from the route scene with the live distance
        /// between this spawn's position and the player's position. Once IsCharging is
        /// true, the route scene should move this enemy toward the player until they collide.
        /// </summary>
        public bool EvaluateCharge(float distanceToPlayer, Systems.RNGManager rng)
        {
            if (IsCharging)
            {
                return true;
            }

            if (distanceToPlayer <= DetectionRange && rng.RollPercentage(EffectiveAggression))
            {
                IsCharging = true;
            }

            return IsCharging;
        }
    }
}
