using System;

namespace game_logic.abilities;

public abstract class DefenceBoost : Ability
{
    public abstract float GetDefenceMultiplier();
}