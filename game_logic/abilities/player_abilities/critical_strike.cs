using System;

namespace game_logic.abilities;

public abstract class CriticalStrike : Ability
{
    public abstract float GetCriticalChance();
}