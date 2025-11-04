using System;
using System.Collections.Generic;

namespace game_files;

/*
this class is the players file. It has everything to do with the player and every aspect related to them.
*/
public class Player : Entity
{
    // Define Player Attributes
    public string Name;
    public int Level;
    public int Experience;
    public int Health;
    public int MaxHealth;
    public int Gold;
    Random rd = new Random();

    // call a new instance of the play ability so that will affect the players stats as needed.

    public int Savefile;

    // Inventory
    public List<Item> Inventory = new List<Item>();
    public Weapon EquippedWeapon;
    public Armor EquippedArmor;

    // define the player from the save file, I will also need a flag that checks to see if it a new game or not
    public Player(string name)
    {
        Name = name;
        Level = 1;
        Experience = 0;
        MaxHealth = 20;
        Health = MaxHealth;
        Gold = 0;
        Inventory.Clear();
    }

    public static Player LoadFromSave(SaveData data)
    {
        Player player = new Player(data.Name);
        player.Level = data.Level;
        player.Experience = data.Experience;
        player.MaxHealth = data.MaxHealth;
        player.Health = data.Health;
        player.Gold = data.Gold;
        player.Inventory = data.Inventory;
        return player;
    }

    // Adds experience to the player and levels up if the experience threshold is met.
    public void AddExperience(int exp, int level)
    {
        Experience += exp;
        // I will keep lines like these in here for testing purposes
        Console.WriteLine($"{Name} gained {exp} experience!");

        while (Experience >= ExperienceToLevelUp(level))
        {
            Experience -= ExperienceToLevelUp(level);
            LevelUp();
        }
    }

    // Levels up the player, increasing stats and health.
    private void LevelUp()
    {
        Level++;
        MaxHealth += rd.Next(1, 7); // this range may change 1/30/25
        Health = MaxHealth; // Restore health on level up

        // this line is here for testing purposes
        Console.WriteLine($"{Name} leveled up to level {Level}!");
    }

    // Calculates the experience needed to level up.
    private int ExperienceToLevelUp(int currentLVL)
    {
        int i = 1;
        double expCalc = 120;
        while (i < currentLVL)
        {
            expCalc *= 1.035;
            i++;
        }
        int expNeeded = Convert.ToInt32(expCalc);
        return expNeeded;
    }

    // Heals the player by a specific amount.
    public void Heal(int amount)
    {
        // I need to have an method that will increase the health of the player but it needs to work with all healing methods 1/30/25
        // this will likely need a switch case to change the text based on what kind of healing they are using.
        Console.WriteLine($"{Name} healed for {amount} health. Current health: {Health}/{MaxHealth}");
    }

    // Takes damage and reduces health.
    public void TakeDamage(int damage)
    {
        Health -= damage;
        if (Health <= 0)
        {
            Console.WriteLine($"{Name} has been defeated!");
        }
        else
        {
            Console.WriteLine($"{Name} took {damage} damage. Current health: {Health}/{MaxHealth}");
        }
    }

    // Adds an item to the player's inventory.
    public void AddToInventory(Item item)
    {
        Inventory.Add(item);
        Console.WriteLine($"{item.Name} has been added to your inventory.");
    }

    // Equips a weapon, replacing the current weapon if one is already equipped.
    public void EquipWeapon(Weapon weapon)
    {
        EquippedWeapon = weapon;
        Console.WriteLine($"{Name} equipped {weapon.Name}.");
    }

    // Equips armor, replacing the current armor if one is already equipped.
    public void EquipArmor(Armor armor)
    {
        EquippedArmor = armor;
        Console.WriteLine($"{Name} equipped {armor.Name}.");
    }
}
