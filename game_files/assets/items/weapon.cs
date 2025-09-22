using System;
using System.Collections.Generic;

namespace game_files;

public class Weapon : Item
{
    // this subclass is used for weapons and their levels
    public int MinDamage;
    public int MaxDamage;
    public int WeaponLvl;

    public Weapon(string name, int minDamage, int maxDamage, int Wlevel) :base(name)
    {
        MinDamage = minDamage;
        MaxDamage = maxDamage;
        WeaponLvl = Wlevel;
    }
}