using System;

namespace game;

class Director
{
    /*
    this class will direct the whole ordeal of the game. I want this to be the nexus of all things involved.
    */
    MapManager mapManager;
    Player player;
    CommandManager commandManager;
    UIManager uiManager;
    SaveManager saveManager;
}