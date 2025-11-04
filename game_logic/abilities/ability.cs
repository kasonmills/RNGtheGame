using System;

namespace game_logic.abilities;

public abstract class Ability
{
    public abstract string get_name();
    public abstract string get_description();
    public abstract void activate(entity user, entity target);
}
