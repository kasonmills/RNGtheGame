using System;
using System.Collections.Generic;
using GameLogic.Items;
using GameLogic.Data;

namespace GameLogic.Entities.Player
{
    public class Player : Entity
    {
        // Player-specific Attributes (Name, Level, Health, MaxHealth inherited from Entity)
        public int Experience { get; set; }
        public int Gold { get; set; }
        public TimeSpan PlayTime { get; set; }

        // Abilities
        public List<Abilities.Ability> Abilities { get; set; }

        private Random rd = new Random();

        // Inventory
        public List<Item> Inventory { get; set; }
        public Weapon EquippedWeapon { get; set; }
        public Armor EquippedArmor { get; set; }

        // Constructor for new player
        public Player(string name) : base()
        {
            Name = name;
            Level = 1;
            Experience = 0;
            MaxHealth = 20;
            Health = MaxHealth;
            Gold = 0;
            PlayTime = TimeSpan.Zero;
            Inventory = new List<Item>();
            Abilities = new List<Abilities.Ability>();
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

        // Override Heal to add console output
        public override void Heal(int amount)
        {
            base.Heal(amount);
            Console.WriteLine($"{Name} healed for {amount} HP. Current health: {Health}/{MaxHealth}");
        }

        // Override TakeDamage to add console output
        public override void TakeDamage(int damage)
        {
            base.TakeDamage(damage);
            if (Health <= 0)
            {
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

        /// <summary>
        /// Implementation of abstract Execute method from Entity
        /// For player, this could be used for executing player actions or abilities
        /// </summary>
        public override void Execute(Entity target = null)
        {
            // Placeholder implementation
            // Can be used later for player abilities or special actions
            Console.WriteLine($"{Name} performs an action!");
        }
    }
}
