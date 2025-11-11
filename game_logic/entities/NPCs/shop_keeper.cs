using System;

namespace GameLogic.Entities.NPCs;

    public class ShopKeeper : NPCBase
{
    public ShopKeeper(string name, int level) : base(name, level)
    {

    }

    public void DisplayGoods()
    {
        Speak("Welcome to my shop! Here are the goods I have for sale.");
        // Logic to display goods would go here
    }
}
