using System;

namespace game_logic.abilities;

public abstract class AbilityEffect
{
    public abstract string get_effect_name();
    public abstract string get_effect_description();
    public abstract void apply_effect(Entity target);
}