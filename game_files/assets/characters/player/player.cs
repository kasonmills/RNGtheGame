using System;
using System.Collections.Generic;

namespace game_files.assets.characters.player;

/*
this class is the players file. It has everything to do with the player and every aspect related to them.
*/
public class Player
{
    // Define Player Attributes
    public string Name;
    public int Level;
    public int Experience;
    public int Health;
    public int MaxHealth;
    public int Gold;
    Random rd = new Random();

    // Inventory
    public List<Item> Inventory;
    public Weapon EquippedWeapon;
    public Armor EquippedArmor;

    // define the player from the save file
    public Player(string name)
    {
        Name = name;
        Level = 1;
        Experience = 0;
        MaxHealth = 20;
        Health = MaxHealth;
        Gold = 0;

        Inventory = new List<Item>();
    }

    // Adds experience to the player and levels up if the experience threshold is met.
    public void AddExperience(int exp, int level)
    {
        Experience += exp;
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
        MaxHealth += rd.Next(1,7);
        Health = MaxHealth; // Restore health on level up

        Console.WriteLine($"{Name} leveled up to level {Level}!");
    }

    // Calculates the experience needed to level up.
    private int ExperienceToLevelUp(int currentLVL)
    {
        int i = 1;
        double expCalc = 100;
        while (i < currentLVL)
        {
            expCalc *= 1.02;
            i++;
        }
        int expNeeded = Convert.ToInt32(expCalc);
        return expNeeded;
    }

    /// Heals the player by a specific amount.
    public void Heal(int amount)
    {
        Health = Math.Min(Health + amount, MaxHealth);
        Console.WriteLine($"{Name} healed for {amount} health. Current health: {Health}/{MaxHealth}");
    }

    /// Takes damage and reduces health.
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

// Base Item Class (Extend for Weapons, Armor, etc.)
public abstract class Item
{
    public string Name;
}

public class Weapon : Item
{
    public int MinDamage;
    public int MaxDamage;

    public Weapon(string name, int minDamage, int maxDamage)
    {
        Name = name;
        MinDamage = minDamage;
        MaxDamage = maxDamage;
    }
}

public class Armor : Item
{
    public int Defense;

    public Armor(string name, int defense)
    {
        Name = name;
        Defense = defense;
    }
}