using System;

namespace game_logic.abilities;

public abstract class HealingAbility : Ability
{
    public abstract float GetHealingAmount();
}