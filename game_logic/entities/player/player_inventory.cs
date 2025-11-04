using System;
using System.Collections.Generic;

namespace game_files;

public class PlayerInventory
{
    public List<Item> Items;

    public PlayerInventory()
    {
        Items = new List<Item>();
    }

    public void AddItem(Item item)
    {
        Items.Add(item);
        Console.WriteLine($"{item.Name} has been added to your inventory.");
    }

    public void RemoveItem(Item item)
    {
        if (Items.Remove(item))
        {
            Console.WriteLine($"{item.Name} has been removed from your inventory.");
        }
        else
        {
            Console.WriteLine($"{item.Name} is not in your inventory.");
        }
    }

    public void DisplayInventory()
    {
        Console.WriteLine("Inventory:");
        foreach (var item in Items)
        {
            Console.WriteLine($"- {item.Name}: {item.Description}");
        }
    }   
}