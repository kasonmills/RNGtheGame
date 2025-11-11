using System;
using System.Collections.Generic;

namespace GameLogic.Items;

public class Armor : Item
{
    // this subclass is for use with armor items and their levels
    public int Defense;
    public string Armtype;
    public int ArmLevel;

    public Armor(string name, int defense, string armtype, int armLevel) :base(name)
    {
        Defense = defense;
        Armtype = armtype;
        ArmLevel = armLevel;
    }
}