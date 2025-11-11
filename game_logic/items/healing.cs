using System;
using System.Collections.Generic;

namespace GameLogic.Items;

public class Healing : Item
{
    public Healing(string name) : base(name)
    {

    }

    public override void ModifyStat(Entity target)
    {
        int healAmount = 20;
        target.Health += healAmount;
        if (target.Health > target.MaxHealth)
        {
            target.Health = target.MaxHealth;
        }
    }
}