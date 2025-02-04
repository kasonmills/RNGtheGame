using System;
using System.Collections.Generic;

namespace game_files;

// Manages the player's abilities, including adding, applying, and removing effects.
public class PlayerAbilities
{
    public List<Ability> Abilities; // List of all abilities in the game
    private Player _playerStats; // Reference to player stats
    private Weapon _equippedWeapon;
    private Armor _equippedArmor;

    // Constructor
    public PlayerAbilities(Player playerStats, Weapon equippedWeapon, Armor equippedArmor)
    {
        // this would be defining one unique ability note a whole list of them... But I will need a list of all the abilites that are possible but only one is active for the player
        //ability = List<Ability>("");
        _playerStats = playerStats;
        _equippedWeapon = equippedWeapon;
        _equippedArmor = equippedArmor;
    }

    // this method lets the player select what ability they want.
    public void select_ability(Ability ability)
    {
        switch (ability)
        {
            // I haven't decided how many abilites they can choose from but I know there will be at least two..
            case " ": // I am keeping it blank for now because I need to work on this more.
            {
                break;
            }
            default:
            {
                break;
            }
        }
    }

    /// Uses an ability if it's available (i.e., not on cooldown).
    public void apply_ability(string abilityName)
    {
        Ability ability = ActiveAbilities.Find(a => a.Name == abilityName);
        if (ability == null)
        {
            Console.WriteLine("Ability not found!");
            return;
        }
    }

    /// <summary>
    /// Removes an ability effect if it has a temporary duration.
    /// </summary>
    public void RemoveAbilityEffect(string abilityName)
    {
        Ability ability = ActiveAbilities.Find(a => a.Name == abilityName);
        if (ability != null)
        {
            ability.RemoveEffect(_playerStats, _equippedWeapon, _equippedArmor);
            Console.WriteLine($"{ability.Name} effect removed.");
        }
    }

    /// <summary>
    /// Reduces cooldowns on all abilities at the end of the turn.
    /// </summary>
    public void ReduceCooldowns()
    {
        foreach (var ability in ActiveAbilities)
        {
            if (ability.CurrentCooldown > 0)
            {
                ability.CurrentCooldown--;
            }
        }
    }
}

/// <summary>
/// Base class for all player abilities.
/// </summary>
public abstract class Ability
{
    public string Name;
    public string Description;
    public int Cooldown;
    public int CurrentCooldown;

    // Constructor
    public Ability(string name, string description, int cooldown)
    {
        Name = name;
        Description = description;
        Cooldown = cooldown;
        CurrentCooldown = 0;
    }

    /// <summary>
    /// Applies the ability's effect.
    /// </summary>
    public abstract void ApplyEffect(PlayerStats playerStats, Weapon weapon, Armor armor);

    /// <summary>
    /// Removes the ability's effect (for temporary buffs).
    /// </summary>
    public abstract void RemoveEffect(PlayerStats playerStats, Weapon weapon, Armor armor);
}

/// <summary>
/// Example: A temporary buff that increases player attack.
/// </summary>
public class AttackBoost : Ability
{
    private int attackIncrease;

    public AttackBoost() : base("Attack Boost", "Temporarily increases attack power by 5.", 3)
    {
        attackIncrease = 5;
    }

    public override void ApplyEffect(PlayerStats playerStats, Weapon weapon, Armor armor)
    {
        playerStats.Strength += attackIncrease;
        Console.WriteLine($"{Name} applied! Strength increased by {attackIncrease}.");
    }

    public override void RemoveEffect(PlayerStats playerStats, Weapon weapon, Armor armor)
    {
        playerStats.Strength -= attackIncrease;
        Console.WriteLine($"{Name} effect removed! Strength returned to normal.");
    }
}

/// <summary>
/// Example: A healing ability that restores health.
/// </summary>
public class HealingAbility : Ability
{
    private int healAmount;

    public HealingAbility() : base("Healing Light", "Restores 20 HP.", 2)
    {
        healAmount = 20;
    }

    public override void ApplyEffect(PlayerStats playerStats, Weapon weapon, Armor armor)
    {
        playerStats.Heal(healAmount);
        Console.WriteLine($"{Name} used! Restored {healAmount} HP.");
    }

    public override void RemoveEffect(PlayerStats playerStats, Weapon weapon, Armor armor)
    {
        // Healing is a one-time effect, so nothing to remove
    }
}

