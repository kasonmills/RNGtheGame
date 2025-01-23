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

    // Inventory
    public List<Item> Inventory;
    public Weapon EquippedWeapon;
    public Armor EquippedArmor;

    // Constructor
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

    // Methods

    /// <summary>
    /// Adds experience to the player and levels up if the experience threshold is met.
    /// </summary>
    public void AddExperience(int exp)
    {
        Experience += exp;
        Console.WriteLine($"{Name} gained {exp} experience!");

        while (Experience >= ExperienceToLevelUp())
        {
            Experience -= ExperienceToLevelUp();
            LevelUp();
        }
    }

    /// <summary>
    /// Levels up the player, increasing stats and health.
    /// </summary>
    private void LevelUp()
    {
        Level++;
        MaxHealth += 10;
        Health = MaxHealth; // Restore health on level up

        Console.WriteLine($"{Name} leveled up to level {Level}!");
    }

    /// <summary>
    /// Calculates the experience needed to level up.
    /// </summary>
    private int ExperienceToLevelUp()
    {
        return Level * 100; // Example: Level 1 -> 100 EXP, Level 2 -> 200 EXP, etc.
    }

    /// <summary>
    /// Heals the player by a specific amount.
    /// </summary>
    public void Heal(int amount)
    {
        Health = Math.Min(Health + amount, MaxHealth);
        Console.WriteLine($"{Name} healed for {amount} health. Current health: {Health}/{MaxHealth}");
    }

    /// <summary>
    /// Takes damage and reduces health.
    /// </summary>
    public void TakeDamage(int damage)
    {
        Health -= damage;
        if (Health <= 0)
        {
            Health = 0;
            Console.WriteLine($"{Name} has been defeated!");
        }
        else
        {
            Console.WriteLine($"{Name} took {damage} damage. Current health: {Health}/{MaxHealth}");
        }
    }

    /// <summary>
    /// Adds an item to the player's inventory.
    /// </summary>
    public void AddToInventory(Item item)
    {
        Inventory.Add(item);
        Console.WriteLine($"{item.Name} has been added to your inventory.");
    }

    /// <summary>
    /// Equips a weapon, replacing the current weapon if one is already equipped.
    /// </summary>
    public void EquipWeapon(Weapon weapon)
    {
        EquippedWeapon = weapon;
        Console.WriteLine($"{Name} equipped {weapon.Name}.");
    }

    /// <summary>
    /// Equips armor, replacing the current armor if one is already equipped.
    /// </summary>
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