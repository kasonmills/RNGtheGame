using System;
using System.Collections.Generic;
using GameLogic.Items;
using GameLogic.Data;
using GameLogic.Systems;

namespace GameLogic.Entities.Player
{
    public class Player : Entity
    {
        // Player-specific Attributes (Name, Level, Health, MaxHealth inherited from Entity)
        public int Experience { get; set; }
        public int Gold { get; set; }
        public TimeSpan PlayTime { get; set; }

        // Player Class (permanent choice made at character creation)
        public PlayerClass Class { get; set; }

        // Selected Ability (permanent choice made at character creation)
        public Abilities.Ability SelectedAbility { get; set; }

        private Random rd = new Random();

        // Inventory
        public PlayerInventory Inventory { get; private set; }
        public Weapon EquippedWeapon { get; set; }
        public Armor EquippedArmor { get; set; }

        // Constructor for new player
        public Player(string name, PlayerClass playerClass = PlayerClass.Warrior) : base()
        {
            Name = name;
            Class = playerClass;
            Level = 1;
            Experience = 0;

            // Base stats. Health raised from 20 to 100 (decided 2026-11-10) so the player is on the
            // same footing as the companions and the harder difficulties have room to lower it -
            // first pass, to be tested and adjusted.
            int baseHealth = 100;
            int baseSpeed = 10;

            // MVP: no class bonuses (player classes are out of the MVP). The Warrior default's
            // +20% health was turning 20 into 24. Kept for a possible update:
            // MaxHealth = (int)(baseHealth * (1.0 + PlayerClassInfo.GetHealthBonus(playerClass)));
            // Speed = (int)(baseSpeed * (1.0 + PlayerClassInfo.GetSpeedBonus(playerClass)));
            MaxHealth = baseHealth;
            Health = MaxHealth;
            Speed = baseSpeed;

            Gold = 0;
            PlayTime = TimeSpan.Zero;
            Inventory = new PlayerInventory();
            SelectedAbility = null; // Will be set during character creation

            GameLog.Write($"{Name} created as {PlayerClassInfo.GetClassName(playerClass)}!");
            GameLog.Write($"Base Health: {MaxHealth} | Base Speed: {Speed}");
        }

        // Set the player's permanent ability (called during character creation)
        public void SetAbility(Abilities.Ability ability)
        {
            if (SelectedAbility != null)
            {
                GameLog.Write($"Warning: Ability already set to {SelectedAbility.Name}. Ability cannot be changed.");
                return;
            }

            SelectedAbility = ability;
            GameLog.Write($"{Name} has chosen the ability: {ability.Name}!");
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
                // Leadership is out of scope for the MVP - disabled, not deleted (re-enable here and in GetAvailableAbilities)
                // case "Leadership":
                //     return new Abilities.LeadershipAbility();
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
                    GameLog.Write($"Warning: Unknown ability '{abilityName}'. No ability loaded.");
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
                // new Abilities.LeadershipAbility(),       // Passive ability - party size (out of scope for the MVP - disabled, not deleted)
                new Abilities.EvasionAbility(),             // Passive ability - dodge damage
                new Abilities.RallyingCryAbility(),         // Passive ability - companion attack buff
                new Abilities.PrecisionTrainingAbility(),   // Passive ability - ally accuracy buff
                new Abilities.SwiftTacticsAbility(),        // Passive ability - companion speed buff
                new Abilities.ExecutionerAbility(),         // Passive ability - crit mechanics (Legendary)
                new Abilities.IronWillAbility(),            // Passive ability - status effect cleanse (Rare - was Epic)
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
                    player.SelectedAbility.Level = Math.Clamp(data.SelectedAbilityLevel, 1, player.SelectedAbility.MaxLevel);
                    player.SelectedAbility.Experience = data.SelectedAbilityExperience;
                    GameLog.Write($"Ability restored: {player.SelectedAbility.Name} (Lv.{player.SelectedAbility.Level})");
                }
            }

            return player;
        }

        /// <summary>
        /// Highest level the player can reach (overall level cap, decided 2026-10-22)
        /// </summary>
        public const int MaxLevel = LevelCaps.MaxLevel;

        // Adds experience and handles level ups
        public void AddExperience(int exp)
        {
            Experience += exp;
            GameLog.Write($"{Name} gained {exp} experience!");

            while (Level < MaxLevel && Experience >= ExperienceToLevelUp())
            {
                Experience -= ExperienceToLevelUp();
                LevelUp();
            }
        }

        // Level up the player
        private void LevelUp()
        {
            Level++;
            // Same health growth as the companions (NPCBase.LevelUp) - decided 2026-11-10 with the
            // 100 HP start; was a random +1 to +6
            MaxHealth += 10 + (Level / 5);
            Health = MaxHealth; // Full heal on level up

            // Increase Speed every few levels (small incremental gains)
            if (Level % 3 == 0) // Every 3 levels
            {
                Speed++;
                GameLog.Write($"{Name}'s speed increased to {Speed}!");
            }

            GameLog.Write($"{Name} leveled up to level {Level}!");
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

        // Heal and TakeDamage only added console lines. Those are commented out (2026-11-10):
        // in a fight they would repeat what combat already reports (Damaged/Healed/Defeated
        // events and the combat log), and items and Rest() write their own lines.
        public override void Heal(int amount)
        {
            base.Heal(amount);
            // GameLog.Write($"{Name} healed for {amount} HP. Current health: {Health}/{MaxHealth}");
        }

        public override void TakeDamage(int damage)
        {
            base.TakeDamage(damage);
            // if (Health <= 0)
            // {
            //     GameLog.Write($"{Name} has been defeated!");
            // }
            // else
            // {
            //     GameLog.Write($"{Name} took {damage} damage. Current health: {Health}/{MaxHealth}");
            // }
        }

        // Add item to inventory
        public bool AddToInventory(Item item)
        {
            return Inventory.AddItem(item);
        }

        // Equip weapon
        public void EquipWeapon(Weapon weapon)
        {
            EquippedWeapon = weapon;
            GameLog.Write($"{Name} equipped {weapon.Name}.");
        }

        // Equip armor
        public void EquipArmor(Armor armor)
        {
            EquippedArmor = armor;

            // Show class compatibility warnings/bonuses
            double speedPenalty = PlayerClassInfo.GetArmorSpeedPenalty(Class, armor.Type);
            double defensePenalty = PlayerClassInfo.GetArmorDefensePenalty(Class, armor.Type);
            double abilityPowerPenalty = PlayerClassInfo.GetArmorAbilityPowerPenalty(Class, armor.Type);
            double accuracyPenalty = PlayerClassInfo.GetArmorAccuracyPenalty(Class, armor.Type);

            GameLog.Write($"{Name} equipped {armor.Name}.");

            // Display any penalties or bonuses
            if (speedPenalty < 0)
                GameLog.Write($"  ⚠️  Speed penalty: {speedPenalty * 100:F0}% (armor too heavy for {PlayerClassInfo.GetClassName(Class)})");
            if (defensePenalty < 0)
                GameLog.Write($"  ⚠️  Defense penalty: {defensePenalty * 100:F0}% (armor not optimal for {PlayerClassInfo.GetClassName(Class)})");
            else if (defensePenalty > 0)
                GameLog.Write($"  ✅ Defense bonus: +{defensePenalty * 100:F0}% ({PlayerClassInfo.GetClassName(Class)} proficiency)");
            if (abilityPowerPenalty < 0)
                GameLog.Write($"  ⚠️  Ability Power penalty: {abilityPowerPenalty * 100:F0}% (armor interferes with magic)");
            else if (abilityPowerPenalty > 0)
                GameLog.Write($"  ✅ Ability Power bonus: +{abilityPowerPenalty * 100:F0}% (enhanced magical channeling)");
            if (accuracyPenalty < 0)
                GameLog.Write($"  ⚠️  Accuracy penalty: {accuracyPenalty * 100:F0}% (armor interferes with precision)");
        }

        /// <summary>
        /// Get effective defense including class bonuses/penalties
        /// </summary>
        public int GetEffectiveDefense()
        {
            if (EquippedArmor == null)
                return 0;

            int baseDefense = EquippedArmor.Defense;
            double classDefenseBonus = PlayerClassInfo.GetDefenseBonus(Class);
            double armorDefensePenalty = PlayerClassInfo.GetArmorDefensePenalty(Class, EquippedArmor.Type);

            // Apply class base bonus and armor compatibility
            double totalDefenseMultiplier = 1.0 + classDefenseBonus + armorDefensePenalty;
            return (int)(baseDefense * totalDefenseMultiplier);
        }

        /// <summary>
        /// Get effective speed including class bonuses and armor penalties
        /// </summary>
        public int GetEffectiveSpeed()
        {
            // Start with base speed (which already includes class speed bonus from constructor)
            int effectiveSpeed = Speed;

            // Apply armor speed penalty if armor is equipped
            if (EquippedArmor != null)
            {
                double armorSpeedPenalty = PlayerClassInfo.GetArmorSpeedPenalty(Class, EquippedArmor.Type);
                effectiveSpeed = (int)(effectiveSpeed * (1.0 + armorSpeedPenalty));
            }

            return Math.Max(1, effectiveSpeed); // Minimum speed of 1
        }

        /// <summary>
        /// Get effective ability power based on class and armor
        /// </summary>
        public double GetEffectiveAbilityPower()
        {
            double baseAbilityPower = 1.0;
            double classAbilityPowerBonus = PlayerClassInfo.GetAbilityPowerBonus(Class);

            double totalMultiplier = 1.0 + classAbilityPowerBonus;

            // Apply armor ability power penalty if armor is equipped
            if (EquippedArmor != null)
            {
                double armorAbilityPowerPenalty = PlayerClassInfo.GetArmorAbilityPowerPenalty(Class, EquippedArmor.Type);
                totalMultiplier += armorAbilityPowerPenalty;
            }

            return baseAbilityPower * totalMultiplier;
        }

        /// <summary>
        /// Get effective accuracy based on class and armor
        /// </summary>
        public double GetEffectiveAccuracy()
        {
            double baseAccuracy = 1.0;
            double classAccuracyBonus = PlayerClassInfo.GetAccuracyBonus(Class);

            double totalMultiplier = 1.0 + classAccuracyBonus;

            // Apply armor accuracy penalty if armor is equipped
            if (EquippedArmor != null)
            {
                double armorAccuracyPenalty = PlayerClassInfo.GetArmorAccuracyPenalty(Class, EquippedArmor.Type);
                totalMultiplier += armorAccuracyPenalty;
            }

            return baseAccuracy * totalMultiplier;
        }

        /// <summary>
        /// Implementation of abstract Execute method from Entity
        /// For player, this could be used for executing player actions or abilities
        /// </summary>
        public override void Execute(Entity target = null)
        {
            // Placeholder implementation
            // Can be used later for player abilities or special actions
            GameLog.Write($"{Name} performs an action!");
        }
    }
}
