using System;
using System.Collections.Generic;

namespace game_files;

public class Unique : Item
{
    // this subclass is used for items that don't quite fit into the other basic categories
    public string _unqiue;

    public Unique(string name, string unique) :base(name)
    {
        _unqiue = unique;
    }
}