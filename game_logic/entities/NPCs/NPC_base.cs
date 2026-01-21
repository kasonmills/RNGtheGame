using System;
using System.Collections.Generic;
using GameLogic.Items;
using GameLogic.Systems;

namespace GameLogic.Entities.NPCs
{
    /// <summary>
    /// Types of NPCs in the game
    /// </summary>
    public enum NPCType
    {
        Generic,        // Non-interactive background NPCs
        Merchant,       // Shop vendors
        QuestGiver,     // NPCs that give quests
        Companion,      // Party members that travel with player (leveled)
        Trainer,        // NPCs that teach abilities or provide upgrades
        Innkeeper,      // Rest/healing NPCs
        Blacksmith,     // Weapon/armor upgrades
        Guard,          // Town guards
        Citizen         // Interactive townspeople
    }

    /// <summary>
    /// Base class for all NPCs
    /// Supports both leveled (companions) and non-leveled NPCs
    /// </summary>
    public class NPCBase : Entity
    {
        // NPC Identity
        public NPCType Type { get; set; }
        public string Greeting { get; set; }
        public List<string> Dialogues { get; set; }

        // Companion-specific properties (only used if Type == Companion)
        public bool IsCompanion => Type == NPCType.Companion;
        public bool InParty { get; set; }
        public Weapon EquippedWeapon { get; set; }
        public Armor EquippedArmor { get; set; }

        // Shop-specific properties (only used if Type == Merchant)
        public List<Item> ShopInventory { get; set; }
        public int BuyPriceMultiplier { get; set; } = 100;  // Percent (100 = normal price)
        public int SellPriceMultiplier { get; set; } = 50;  // Percent (50 = half price when selling)

        /// <summary>
        /// Constructor for non-leveled NPCs
        /// </summary>
        public NPCBase(string name, NPCType type, string greeting = null)
        {
            Name = name;
            Type = type;
            Level = 0; // Non-leveled NPCs have level 0
            Greeting = greeting ?? $"Hello, I'm {name}.";
            Dialogues = new List<string>();
            ShopInventory = new List<Item>();
            InParty = false;
        }

        /// <summary>
        /// Constructor for leveled companions
        /// </summary>
        public NPCBase(string name, int level, int health, int maxHealth) : this(name, NPCType.Companion)
        {
            Level = level;
            Health = health;
            MaxHealth = maxHealth;
            Speed = 10; // Default speed - should be overridden by specific companion types
            Description = "A trusted companion who fights alongside you.";
        }

        /// <summary>
        /// Make the NPC speak
        /// </summary>
        public void Speak(string message)
        {
            Console.WriteLine($"{Name}: \"{message}\"");
        }

        /// <summary>
        /// Greet the player
        /// </summary>
        public void Greet()
        {
            Speak(Greeting);
        }

        /// <summary>
        /// Add dialogue options
        /// </summary>
        public void AddDialogue(string dialogue)
        {
            Dialogues.Add(dialogue);
        }

        /// <summary>
        /// Get a random dialogue
        /// </summary>
        public string GetRandomDialogue(Systems.RNGManager rng)
        {
            if (Dialogues.Count == 0)
            {
                return Greeting;
            }

            int index = rng.Roll(0, Dialogues.Count - 1);
            return Dialogues[index];
        }

        /// <summary>
        /// Add companion to party
        /// </summary>
        public void JoinParty()
        {
            if (!IsCompanion)
            {
                Console.WriteLine($"{Name} cannot join the party (not a companion type).");
                return;
            }

            InParty = true;
            Console.WriteLine($"{Name} joined the party!");
        }

        /// <summary>
        /// Remove companion from party
        /// </summary>
        public void LeaveParty()
        {
            InParty = false;
            Console.WriteLine($"{Name} left the party.");
        }

        /// <summary>
        /// Level up a companion
        /// </summary>
        public virtual void LevelUp()
        {
            if (!IsCompanion)
            {
                Console.WriteLine($"{Name} cannot level up (not a companion).");
                return;
            }

            Level++;

            // Scale stats with level (similar to player)
            int healthIncrease = 10 + (Level / 5); // Slightly less than player
            MaxHealth += healthIncrease;
            Health = MaxHealth; // Full heal on level up

            Console.WriteLine($"{Name} leveled up to Level {Level}!");
            Console.WriteLine($"Max Health increased by {healthIncrease} (now {MaxHealth})");
        }

        /// <summary>
        /// Get companion's combat stats display
        /// </summary>
        public string GetCompanionStatsDisplay()
        {
            if (!IsCompanion)
            {
                return $"{Name} - {Type}";
            }

            string weaponInfo = EquippedWeapon != null ? EquippedWeapon.Name : "Unarmed";
            string armorInfo = EquippedArmor != null ? EquippedArmor.Name : "None";

            return $"{Name} (Lv.{Level})\n" +
                   $"HP: {Health}/{MaxHealth}\n" +
                   $"Weapon: {weaponInfo}\n" +
                   $"Armor: {armorInfo}\n" +
                   $"Status: {(InParty ? "In Party" : "Available")}";
        }

        /// <summary>
        /// Execute NPC-specific behavior
        /// Required by Entity base class
        /// </summary>
        public override void Execute(Entity target = null)
        {
            // NPC behavior varies by type
            switch (Type)
            {
                case NPCType.Companion:
                    if (InParty && target != null)
                    {
                        // Companions attack enemies when in party
                        AttackTarget(target, new RNGManager());
                    }
                    break;

                case NPCType.Merchant:
                    Console.WriteLine($"{Name} stands ready to trade.");
                    break;

                case NPCType.QuestGiver:
                    Console.WriteLine($"{Name} has a quest for you.");
                    break;

                default:
                    // Generic NPCs just exist
                    Console.WriteLine($"{Name} is here.");
                    break;
            }
        }

        /// <summary>
        /// Companion attacks a target (for combat)
        /// </summary>
        public void AttackTarget(Entity target, RNGManager rng)
        {
            if (!IsCompanion || !InParty)
            {
                Console.WriteLine($"{Name} cannot attack (not an active companion).");
                return;
            }

            Console.WriteLine($"\n{Name} attacks {target.Name}!");

            // Calculate accuracy
            int accuracy = GetCompanionAccuracy();
            int hitRoll = rng.Roll(1, 100);

            if (hitRoll > accuracy)
            {
                Console.WriteLine($"{Name}'s attack missed!");
                return;
            }

            // Calculate damage
            int damage = GetCompanionDamage(rng);

            // Apply level scaling
            int levelBonus = Level / 2;
            damage += levelBonus;

            // Check for critical hit (5% base chance)
            int critChance = GetCompanionCritChance();
            int critRoll = rng.Roll(1, 100);

            if (critRoll <= critChance)
            {
                Console.WriteLine("CRITICAL HIT!");
                damage = (int)(damage * 1.5f);
            }

            // Deal damage
            target.TakeDamage(damage);
            Console.WriteLine($"{Name} dealt {damage} damage to {target.Name}!");
        }

        /// <summary>
        /// Get companion's base damage
        /// </summary>
        private int GetCompanionDamage(RNGManager rng)
        {
            if (EquippedWeapon != null)
            {
                // Armed: Use weapon damage (80% effectiveness compared to player)
                int minDamage = (int)(EquippedWeapon.MinDamage * 0.8);
                int maxDamage = (int)(EquippedWeapon.MaxDamage * 0.8);
                return rng.Roll(Math.Max(1, minDamage), Math.Max(1, maxDamage));
            }
            else
            {
                // Unarmed: Weak damage
                return rng.Roll(1, 3);
            }
        }

        /// <summary>
        /// Get companion's accuracy
        /// </summary>
        private int GetCompanionAccuracy()
        {
            if (EquippedWeapon != null)
            {
                // Slightly lower accuracy than player (90%)
                return (int)(EquippedWeapon.Accuracy * 0.9);
            }
            else
            {
                // Unarmed: Lower accuracy
                return 55;
            }
        }

        /// <summary>
        /// Get companion's critical hit chance
        /// </summary>
        private int GetCompanionCritChance()
        {
            if (EquippedWeapon != null)
            {
                // Lower crit chance than player (75%)
                return (int)(EquippedWeapon.CritChance * 0.75);
            }
            else
            {
                // Unarmed: Minimal crit chance
                return 2;
            }
        }

        /// <summary>
        /// Display NPC info
        /// </summary>
        public string GetInfo()
        {
            string typeInfo = IsCompanion ? $"Companion (Lv.{Level})" : Type.ToString();
            return $"{Name} - {typeInfo}\n{Description ?? ""}";
        }

        /// <summary>
        /// Equip weapon for companion
        /// </summary>
        public void EquipWeapon(Weapon weapon)
        {
            if (!IsCompanion)
            {
                Console.WriteLine($"{Name} cannot equip weapons (not a companion).");
                return;
            }

            EquippedWeapon = weapon;
            Console.WriteLine($"{Name} equipped {weapon.Name}!");
        }

        /// <summary>
        /// Equip armor for companion
        /// </summary>
        public void EquipArmor(Armor armor)
        {
            if (!IsCompanion)
            {
                Console.WriteLine($"{Name} cannot equip armor (not a companion).");
                return;
            }

            EquippedArmor = armor;
            Console.WriteLine($"{Name} equipped {armor.Name}!");
        }
    }
}
