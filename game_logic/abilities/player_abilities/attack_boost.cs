using System;

namespace game_logic.abilities;

public abstract class AttackBoost : Ability
{
    public abstract float GetAttackMultiplier();
}