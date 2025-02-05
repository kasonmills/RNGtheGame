using System;
using System.Collections.Generic;

namespace game_files;

public class Armor : Item
{
    // this subclass is for use with armor items and their levels
    public int Defense;
    public string Armtype;
    public int ArmLevel;

    public Armor(string name, int defense, string armtype, int armLevel)
    {
        Defense = defense;
        Armtype = armtype;
        ArmLevel = armLevel;
    }
}