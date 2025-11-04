using System;

namespace game_files;

class Goblin : Enemy
{
    public Goblin()
    {
        Name = "Goblin";
        Description = "A small, green creature that attacks in packs.";
        Level = 1;
        Health = 10;
        MaxHealth = 10;
    }
}