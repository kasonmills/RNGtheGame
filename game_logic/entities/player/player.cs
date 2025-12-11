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

        // Selected Ability (permanent choice made at character creation)
        public Abilities.Ability SelectedAbility { get; set; }

        private Random rd = new Random();

        // Inventory
        public PlayerInventory Inventory { get; private set; }
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
            Speed = 10; // Base speed for player (can be modified by equipment/buffs)
            Gold = 0;
            PlayTime = TimeSpan.Zero;
            Inventory = new PlayerInventory();
            SelectedAbility = null; // Will be set during character creation
        }

        // Set the player's permanent ability (called during character creation)
        public void SetAbility(Abilities.Ability ability)
        {
            if (SelectedAbility != null)
            {
                Console.WriteLine($"Warning: Ability already set to {SelectedAbility.Name}. Ability cannot be changed.");
                return;
            }

            SelectedAbility = ability;
            Console.WriteLine($"{Name} has chosen the ability: {ability.Name}!");
        }

        // Helper method to create an ability instance from name
        private static Abilities.Ability CreateAbilityFromName(string abilityName)
        {
            switch (abilityName)
            {
                case "Attack Boost":
                    return new Abilities.PlayerAbilities.AttackBoost();
                case "Heal":
                    return new Abilities.PlayerAbilities.HealingAbility();
                case "Defense Boost":
                    return new Abilities.PlayerAbilities.DefenseBoost();
                case "Critical Strike":
                    return new Abilities.PlayerAbilities.CriticalStrike();
                case "Leadership":
                    return new Abilities.LeadershipAbility();
                case "Evasion":
                    return new Abilities.EvasionAbility();
                case "Rallying Cry":
                    return new Abilities.RallyingCryAbility();
                case "Precision Training":
                    return new Abilities.PrecisionTrainingAbility();
                case "Swift Tactics":
                    return new Abilities.SwiftTacticsAbility();
                case "Executioner":
                    return new Abilities.ExecutionerAbility();
                case "Iron Will":
                    return new Abilities.IronWillAbility();
                // Weapon Mastery Abilities
                case "Sword Mastery":
                    return new Abilities.WeaponMasteries.SwordMasteryAbility();
                case "Axe Mastery":
                    return new Abilities.WeaponMasteries.AxeMasteryAbility();
                case "Mace Mastery":
                    return new Abilities.WeaponMasteries.MaceMasteryAbility();
                case "Dagger Mastery":
                    return new Abilities.WeaponMasteries.DaggerMasteryAbility();
                case "Spear Mastery":
                    return new Abilities.WeaponMasteries.SpearMasteryAbility();
                case "Staff Mastery":
                    return new Abilities.WeaponMasteries.StaffMasteryAbility();
                case "Bow Mastery":
                    return new Abilities.WeaponMasteries.BowMasteryAbility();
                case "Crossbow Mastery":
                    return new Abilities.WeaponMasteries.CrossbowMasteryAbility();
                case "Wand Mastery":
                    return new Abilities.WeaponMasteries.WandMasteryAbility();
                default:
                    Console.WriteLine($"Warning: Unknown ability '{abilityName}'. No ability loaded.");
                    return null;
            }
        }

        // Get all available player abilities (for character creation menu)
        public static Abilities.Ability[] GetAvailableAbilities()
        {
            return new Abilities.Ability[]
            {
                new Abilities.PlayerAbilities.AttackBoost(),
                new Abilities.PlayerAbilities.HealingAbility(),
                new Abilities.PlayerAbilities.DefenseBoost(),
                new Abilities.PlayerAbilities.CriticalStrike(),
                new Abilities.LeadershipAbility(),          // Passive ability - party size
                new Abilities.EvasionAbility(),             // Passive ability - dodge damage
                new Abilities.RallyingCryAbility(),         // Passive ability - companion attack buff
                new Abilities.PrecisionTrainingAbility(),   // Passive ability - ally accuracy buff
                new Abilities.SwiftTacticsAbility(),        // Passive ability - companion speed buff
                new Abilities.ExecutionerAbility(),         // Passive ability - crit mechanics (Legendary)
                new Abilities.IronWillAbility(),            // Passive ability - status effect cleanse (Epic)
                // Weapon Mastery Abilities
                new Abilities.WeaponMasteries.SwordMasteryAbility(),    // Passive - sword bonuses
                new Abilities.WeaponMasteries.AxeMasteryAbility(),      // Passive - axe bonuses
                new Abilities.WeaponMasteries.MaceMasteryAbility(),     // Passive - mace bonuses
                new Abilities.WeaponMasteries.DaggerMasteryAbility(),   // Passive - dagger bonuses
                new Abilities.WeaponMasteries.SpearMasteryAbility(),    // Passive - spear bonuses
                new Abilities.WeaponMasteries.StaffMasteryAbility(),    // Passive - staff bonuses
                new Abilities.WeaponMasteries.BowMasteryAbility(),      // Passive - bow bonuses
                new Abilities.WeaponMasteries.CrossbowMasteryAbility(), // Passive - crossbow bonuses
                new Abilities.WeaponMasteries.WandMasteryAbility()      // Passive - wand bonuses
            };
        }

        // Load player from save data
        public static Player LoadFromSave(SaveData data)
        {
            Player player = new Player(data.PlayerName);

            // Use SaveManager to apply all save data (including inventory)
            Data.SaveManager.ApplySaveDataToPlayer(data, player);

            // Restore selected ability (not handled by ApplySaveDataToPlayer)
            if (!string.IsNullOrEmpty(data.SelectedAbilityName))
            {
                player.SelectedAbility = CreateAbilityFromName(data.SelectedAbilityName);
                if (player.SelectedAbility != null)
                {
                    player.SelectedAbility.Level = data.SelectedAbilityLevel;
                    player.SelectedAbility.Experience = data.SelectedAbilityExperience;
                    Console.WriteLine($"Ability restored: {player.SelectedAbility.Name} (Lv.{player.SelectedAbility.Level})");
                }
            }

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

            // Increase Speed every few levels (small incremental gains)
            if (Level % 3 == 0) // Every 3 levels
            {
                Speed++;
                Console.WriteLine($"{Name}'s speed increased to {Speed}!");
            }

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
            Inventory.AddItem(item);
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
