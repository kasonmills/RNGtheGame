using System;
using System.Collections.Generic;

namespace GameLogic.Items;

public class Weapon : Item
{
    // this subclass is used for weapons and their levels

    // each weapon will have values set for the level of the weapon, the damage range the chance of a critical hit and the accuracy of said weapon
    public int MinDamage;
    public int MaxDamage;
    public int WeaponLvl;
    public int CritChance;

    public int Accuracy;

    public Weapon(string name, int minDamage, int maxDamage, int Wlevel, int critChance, int accuracy) : base(name)
    {
        MinDamage = minDamage;
        MaxDamage = maxDamage;
        WeaponLvl = Wlevel;
        CritChance = critChance;
        Accuracy = accuracy;
    }

    protected bool AttackSuccess()
    {
        return false;
    }

    protected int AttackDamage()
    {
        return 0;
    }
}