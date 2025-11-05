using System;
using System.Collections.Generic;
using GameLogic.Items;
using GameLogic.Data;

namespace GameLogic.Entities.Player
{
    public class Player // Remove ": Entity" for now until we build Entity.cs
    {
        // Player Attributes
        public string Name;
        public int Level;
        public int Experience;
        public int Health;
        public int MaxHealth;
        public int Gold;
        private Random rd = new Random();

        // Inventory
        public List<Item> Inventory = new List<Item>();
        public Weapon EquippedWeapon;
        public Armor EquippedArmor;

        // Constructor for new player
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

        // Load player from save data
        public static Player LoadFromSave(SaveData data)
        {
            Player player = new Player(data.PlayerName);
            player.Level = data.Level;
            player.Experience = data.Experience;
            player.MaxHealth = data.MaxHealth;
            player.Health = data.Health;
            player.Gold = data.Gold;
            
            // TODO: Reconstruct inventory from item names
            // For now, leave inventory empty until we build ItemDatabase
            player.Inventory = new List<Item>();
            
            return player;
        }

        // Adds experience and handles level ups
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

        // Level up the player
        private void LevelUp()
        {
            Level++;
            MaxHealth += rd.Next(1, 7);
            Health = MaxHealth; // Full heal on level up
            Console.WriteLine($"{Name} leveled up to level {Level}!");
        }

        // Calculate XP needed for next level
        private int ExperienceToLevelUp()
        {
            double expCalc = 120;
            for (int i = 1; i < Level; i++)
            {
                expCalc *= 1.035;
            }
            return Convert.ToInt32(expCalc);
        }

        // Heal the player (FIXED - actually increases health now!)
        public void Heal(int amount)
        {
            Health += amount;
            if (Health > MaxHealth)
            {
                Health = MaxHealth;
            }
            Console.WriteLine($"{Name} healed for {amount} HP. Current health: {Health}/{MaxHealth}");
        }

        // Take damage
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

        // Add item to inventory
        public void AddToInventory(Item item)
        {
            Inventory.Add(item);
            Console.WriteLine($"{item.Name} has been added to your inventory.");
        }

        // Equip weapon
        public void EquipWeapon(Weapon weapon)
        {
            EquippedWeapon = weapon;
            Console.WriteLine($"{Name} equipped {weapon.Name}.");
        }

        // Equip armor
        public void EquipArmor(Armor armor)
        {
            EquippedArmor = armor;
            Console.WriteLine($"{Name} equipped {armor.Name}.");
        }
    }
}
