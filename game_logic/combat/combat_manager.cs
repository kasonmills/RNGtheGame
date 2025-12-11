using System;
using System.Collections.Generic;
using GameLogic.Entities.Player;
using GameLogic.Entities.Enemies;
using GameLogic.Systems;
using Enemy = GameLogic.Entities.Enemies.EnemyBase;

namespace GameLogic.Combat
{
    /// <summary>
    /// Orchestrates combat encounters between player and enemies
    /// Delegates to specialized managers for turn order, damage, and actions
    /// Uses a round-based system where each entity acts once per round
    /// </summary>
    public class CombatManager
    {
        private RNGManager _rngManager;
        private TurnManager _turnManager;
        private DamageCalculator _damageCalculator;
        private Progression.BossManager _bossManager;

        private Player _player;
        private Enemy _enemy;
        private bool _combatActive;

        // Round-based combat tracking
        private int _currentRound;
        private List<Entity> _allCombatants;
        private HashSet<Entity> _entitiesActedThisRound;

        // Universal defend tracking - any entity can defend
        private Dictionary<Entity, bool> _defendingEntities;

        public CombatManager(RNGManager rngManager)
        {
            _rngManager = rngManager;
            _turnManager = new TurnManager();
            _damageCalculator = new DamageCalculator(rngManager);
            _defendingEntities = new Dictionary<Entity, bool>();
            _allCombatants = new List<Entity>();
            _entitiesActedThisRound = new HashSet<Entity>();
            _currentRound = 0;
        }

        /// <summary>
        /// Start a combat encounter
        /// </summary>
        /// <param name="player">The player character</param>
        /// <param name="enemy">The enemy to fight</param>
        /// <param name="companions">List of active companions (can be null or empty)</param>
        /// <param name="bossManager">Optional boss manager for boss encounters</param>
        /// <returns>True if player won, False if player lost</returns>
        public bool StartCombat(Player player, Enemy enemy, List<Entities.NPCs.Companions.CompanionBase> companions = null, Progression.BossManager bossManager = null)
        {
            _player = player;
            _enemy = enemy;
            _combatActive = true;
            _bossManager = bossManager;

            // If this is a boss fight, apply strength scaling
            if (_enemy is BossEnemy boss && _bossManager != null)
            {
                _bossManager.ApplyBossScaling(boss);
            }

            // Setup combatants list
            _allCombatants.Clear();
            _allCombatants.Add(_player);

            // Add companions that are in the party
            if (companions != null)
            {
                foreach (var companion in companions)
                {
                    if (companion.InParty && companion.IsAlive())
                    {
                        _allCombatants.Add(companion);
                    }
                }
            }

            _allCombatants.Add(_enemy);

            // Clear tracking structures
            _defendingEntities.Clear();
            _entitiesActedThisRound.Clear();
            _currentRound = 1;

            // Initialize turn manager
            _turnManager.InitializeCombat(player, enemy);

            // Display combat start
            ShowCombatIntro();

            // Main round-based combat loop
            while (_combatActive && _player.Health > 0 && _enemy.Health > 0)
            {
                // Start a new round
                StartNewRound();

                // Each entity gets one turn per round
                foreach (var entity in _allCombatants)
                {
                    // Skip if entity is dead
                    if (!entity.IsAlive())
                        continue;

                    // Skip if entity already acted this round
                    if (_entitiesActedThisRound.Contains(entity))
                        continue;

                    // Execute entity's turn
                    ExecuteEntityTurn(entity);

                    // Mark entity as having acted
                    _entitiesActedThisRound.Add(entity);

                    // Check for combat end after each action
                    if (_enemy.Health <= 0)
                    {
                        return HandleVictory();
                    }
                    else if (_player.Health <= 0)
                    {
                        return HandleDefeat();
                    }

                    // Show status between turns if combat is still active
                    if (_combatActive && entity.IsAlive())
                    {
                        ShowCombatStatus();
                    }

                    // If player or enemy fled, exit
                    if (!_combatActive)
                    {
                        return false;
                    }
                }

                // End of round - process end-of-round effects
                EndRound();
            }

            // Fled from combat or something else
            return false;
        }

        /// <summary>
        /// Display combat introduction
        /// </summary>
        private void ShowCombatIntro()
        {
            Console.Clear();
            Console.WriteLine("=".PadRight(50, '='));
            Console.WriteLine($"    COMBAT: {_player.Name} vs {_enemy.Name}");
            Console.WriteLine("=".PadRight(50, '='));

            // Show party composition
            Console.WriteLine($"\nYour Party:");
            Console.WriteLine($"  {_player.Name} (Level {_player.Level}) - HP: {_player.Health}/{_player.MaxHealth}");

            foreach (var entity in _allCombatants)
            {
                if (entity is Entities.NPCs.Companions.CompanionBase companion && entity != _player && entity != _enemy)
                {
                    Console.WriteLine($"  {companion.Name} (Level {companion.Level}) - HP: {companion.Health}/{companion.MaxHealth}");
                }
            }

            Console.WriteLine($"\nEnemy:");
            Console.WriteLine($"  {_enemy.Name} (Level {_enemy.Level}) - HP: {_enemy.Health}/{_enemy.MaxHealth}");
            Console.WriteLine();
        }

        /// <summary>
        /// Get the effective speed for an entity, accounting for Swift Tactics if applicable
        /// </summary>
        private int GetEntityEffectiveSpeed(Entity entity)
        {
            // Check if entity is a companion
            if (entity is Entities.NPCs.Companions.CompanionBase companion)
            {
                // Use companion's version that applies Swift Tactics
                return companion.GetEffectiveSpeed(_player);
            }

            // For player and enemies, use base GetEffectiveSpeed()
            return entity.GetEffectiveSpeed();
        }

        /// <summary>
        /// Start a new round - reset tracking and display round info
        /// </summary>
        private void StartNewRound()
        {
            Console.WriteLine("\n" + "=".PadRight(50, '='));
            Console.WriteLine($"           ROUND {_currentRound}");
            Console.WriteLine("=".PadRight(50, '='));

            // Clear entities that acted this round
            _entitiesActedThisRound.Clear();

            // Sort combatants by effective speed (base speed + modifier + Swift Tactics) - highest to lowest
            // Entities with higher speed act first in the turn order
            _allCombatants.Sort((a, b) =>
            {
                int speedA = GetEntityEffectiveSpeed(a);
                int speedB = GetEntityEffectiveSpeed(b);
                return speedB.CompareTo(speedA);
            });

            // Display turn order for this round
            Console.WriteLine("\nTurn Order (by Speed):");
            foreach (var entity in _allCombatants)
            {
                if (entity.IsAlive())
                {
                    int effectiveSpeed = GetEntityEffectiveSpeed(entity);
                    string modifierText = entity.SpeedModifier != 0 ? $" ({entity.Speed}{(entity.SpeedModifier > 0 ? "+" : "")}{entity.SpeedModifier})" : "";

                    // Show Swift Tactics bonus for companions
                    if (entity is Entities.NPCs.Companions.CompanionBase && _player.SelectedAbility is Abilities.SwiftTacticsAbility swiftTactics)
                    {
                        int baseSpeed = entity.Speed + entity.SpeedModifier;
                        int speedBonus = effectiveSpeed - baseSpeed;
                        if (speedBonus > 0)
                        {
                            modifierText += $" [+{speedBonus} Swift Tactics]";
                        }
                    }

                    Console.WriteLine($"  {entity.Name} (Speed: {effectiveSpeed}{modifierText})");
                }
            }
            Console.WriteLine();

            // Reset defend status for all entities at start of new round
            foreach (var entity in _allCombatants)
            {
                if (_defendingEntities.ContainsKey(entity))
                {
                    _defendingEntities[entity] = false;
                }
            }
        }

        /// <summary>
        /// Execute a turn for the given entity
        /// </summary>
        private void ExecuteEntityTurn(Entity entity)
        {
            Console.WriteLine($"\n{entity.Name}'s Turn!");

            // Process status effects at start of turn
            entity.ProcessEffects(_rngManager);

            // Check if entity died from effects
            if (!entity.IsAlive())
            {
                Console.WriteLine($"{entity.Name} was defeated by status effects!");
                return;
            }

            // Determine and execute action based on entity type
            if (entity == _player)
            {
                PlayerTurn();
            }
            else if (entity == _enemy)
            {
                EnemyTurn();
            }
            else if (entity is Entities.NPCs.Companions.CompanionBase companion)
            {
                CompanionTurn(companion);
            }
        }

        /// <summary>
        /// Calculate speed modifiers for next round based on actions taken this round
        /// Attack: -1 to -3 speed (slower - recovering from offensive action)
        /// Defend: +1 to +3 speed (faster - defensive stance allows quick reactions)
        /// UseAbility: -1 to +1 speed (variable - depends on the ability)
        /// UseItem: -1 to +1 speed (variable - similar to abilities)
        /// </summary>
        private void CalculateSpeedModifiers()
        {
            Console.WriteLine("\n--- Speed Adjustments for Next Round ---");

            foreach (var entity in _allCombatants)
            {
                if (!entity.IsAlive())
                    continue;

                int modifier = 0;

                switch (entity.LastAction)
                {
                    case Entities.CombatAction.Attack:
                        // Attacking makes you slower next round (-1 to -3)
                        modifier = -_rngManager.Roll(1, 3);
                        break;

                    case Entities.CombatAction.Defend:
                        // Defending makes you faster next round (+1 to +3)
                        modifier = _rngManager.Roll(1, 3);
                        break;

                    case Entities.CombatAction.UseAbility:
                    case Entities.CombatAction.UseItem:
                        // Abilities/items have variable effect (-1 to +1)
                        modifier = _rngManager.Roll(-1, 1);
                        break;

                    case Entities.CombatAction.None:
                        // No action taken (fled or failed to act)
                        modifier = 0;
                        break;
                }

                entity.SpeedModifier = modifier;

                // Display speed change
                if (modifier != 0)
                {
                    string changeText = modifier > 0 ? $"+{modifier}" : modifier.ToString();
                    string actionText = entity.LastAction.ToString();
                    Console.WriteLine($"{entity.Name}'s speed {changeText} from {actionText}");
                }
            }
        }

        /// <summary>
        /// End the current round - process end-of-round effects
        /// </summary>
        private void EndRound()
        {
            Console.WriteLine("\n--- End of Round ---");

            // Reset speed modifiers from previous round before calculating new ones
            foreach (var entity in _allCombatants)
            {
                if (entity.IsAlive())
                {
                    entity.ResetSpeedModifier();
                }
            }

            // Calculate speed modifiers for next round based on actions taken
            CalculateSpeedModifiers();

            // Tick down effect durations for all entities
            foreach (var entity in _allCombatants)
            {
                if (entity.IsAlive())
                {
                    entity.TickEffectDurations();
                }
            }

            // Check for Iron Will passive ability cleanse (after effects tick, before next round)
            if (_player.IsAlive() && _player.SelectedAbility is Abilities.IronWillAbility ironWill)
            {
                // Check if player has any negative effects to cleanse
                if (_player.ActiveEffects.Any(e =>
                    e.Type == EffectType.Debuff ||
                    e.Type == EffectType.DamageOverTime ||
                    e.Type == EffectType.CrowdControl))
                {
                    bool cleansed = ironWill.TryCleanseEffects(_player, _rngManager);
                    if (cleansed)
                    {
                        Console.WriteLine($"\n{_player.Name}'s Iron Will activated! All negative effects cleansed!");
                    }
                }
            }

            // Reduce ability cooldowns for all entities at end of round
            if (_player.SelectedAbility != null && _player.SelectedAbility.CurrentCooldown > 0)
            {
                _player.SelectedAbility.CurrentCooldown--;
            }

            if (_enemy.SpecialAbility != null && _enemy.SpecialAbility.CurrentCooldown > 0)
            {
                _enemy.SpecialAbility.CurrentCooldown--;
            }

            // Reduce companion ability cooldowns
            foreach (var entity in _allCombatants)
            {
                if (entity is Entities.NPCs.Companions.CompanionBase companion)
                {
                    if (companion.UniqueAbility != null && companion.UniqueAbility.CurrentCooldown > 0)
                    {
                        companion.UniqueAbility.CurrentCooldown--;
                    }
                }
            }

            // Increment round counter
            _currentRound++;

            Console.WriteLine("\nPress any key to continue to next round...");
            Console.ReadKey();
        }

        /// <summary>
        /// Handle the player's turn
        /// </summary>
        private void PlayerTurn()
        {
            Console.WriteLine($"Your HP: {_player.Health}/{_player.MaxHealth}");
            Console.WriteLine($"Enemy HP: {_enemy.Health}/{_enemy.MaxHealth}");

            // Get player's action choice
            CombatAction action = GetPlayerAction();

            // Process the action
            ProcessAction(action);
        }

        /// <summary>
        /// Get the player's chosen action
        /// </summary>
        private CombatAction GetPlayerAction()
        {
            Console.WriteLine("\nChoose your action:");
            Console.WriteLine("1. Attack");
            Console.WriteLine("2. Use Active Ability");
            Console.WriteLine("3. Use Item");
            Console.WriteLine("4. Defend");
            Console.WriteLine("5. Try to Flee");

            Console.Write("\nChoice: ");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    return CombatAction.Attack(_player, _enemy);
                case "2":
                    // Use player's selected ability
                    if (_player.SelectedAbility != null)
                    {
                        // Check if ability is on cooldown
                        if (_player.SelectedAbility.CurrentCooldown > 0)
                        {
                            Console.WriteLine($"\n{_player.SelectedAbility.Name} is on cooldown for {_player.SelectedAbility.CurrentCooldown} more rounds!");
                            Console.WriteLine("Attacking instead.");
                            return CombatAction.Attack(_player, _enemy);
                        }

                        return CombatAction.UseAbility(_player, _enemy, _player.SelectedAbility);
                    }
                    else
                    {
                        Console.WriteLine("\nYou don't have an ability selected! Attacking instead.");
                        return CombatAction.Attack(_player, _enemy);
                    }
                case "3":
                    // Use item from inventory
                    if (_player.Inventory.Items.Count == 0)
                    {
                        Console.WriteLine("\nYour inventory is empty! Attacking instead.");
                        return CombatAction.Attack(_player, _enemy);
                    }

                    // Show inventory
                    Console.WriteLine("\nYour Inventory:");
                    for (int i = 0; i < _player.Inventory.Items.Count; i++)
                    {
                        Console.WriteLine($"{i + 1}. {_player.Inventory.Items[i].Name}");
                    }
                    Console.WriteLine($"{_player.Inventory.Items.Count + 1}. Cancel");

                    Console.Write("\nSelect item: ");
                    string itemChoice = Console.ReadLine();

                    if (int.TryParse(itemChoice, out int itemIndex) && itemIndex > 0 && itemIndex <= _player.Inventory.Items.Count)
                    {
                        var item = _player.Inventory.Items[itemIndex - 1];

                        // Check if it's a revival potion - needs target selection
                        if (item is Items.Consumable consumable && consumable.Type == Items.ConsumableType.RevivePotion)
                        {
                            // Get dead allies to revive
                            var deadAllies = new List<Entity>();
                            foreach (var entity in _allCombatants)
                            {
                                if (!entity.IsAlive() && entity != _enemy)
                                {
                                    deadAllies.Add(entity);
                                }
                            }

                            if (deadAllies.Count == 0)
                            {
                                Console.WriteLine("\nNo one needs reviving! Choose a different item.");
                                return GetPlayerAction(); // Return to menu
                            }

                            // Show dead allies
                            Console.WriteLine("\nWho do you want to revive?");
                            for (int i = 0; i < deadAllies.Count; i++)
                            {
                                Console.WriteLine($"{i + 1}. {deadAllies[i].Name}");
                            }
                            Console.WriteLine($"{deadAllies.Count + 1}. Cancel");

                            Console.Write("\nSelect target: ");
                            string targetChoice = Console.ReadLine();

                            if (int.TryParse(targetChoice, out int targetIndex) && targetIndex > 0 && targetIndex <= deadAllies.Count)
                            {
                                var target = deadAllies[targetIndex - 1];
                                return CombatAction.UseItem(_player, item, target);
                            }
                            else
                            {
                                Console.WriteLine("Cancelled.");
                                return GetPlayerAction(); // Return to menu
                            }
                        }

                        // Regular item (no target needed)
                        return CombatAction.UseItem(_player, item);
                    }
                    else
                    {
                        Console.WriteLine("Invalid choice.");
                        return GetPlayerAction(); // Return to menu
                    }
                case "4":
                    return CombatAction.Defend(_player);
                case "5":
                    return CombatAction.Flee(_player);
                default:
                    Console.WriteLine("Invalid choice. Attacking by default.");
                    return CombatAction.Attack(_player, _enemy);
            }
        }

        /// <summary>
        /// Handle the enemy's turn
        /// Uses AI to decide action based on behavior and situation
        /// </summary>
        private void EnemyTurn()
        {
            // Determine action based on enemy behavior and situation
            CombatAction action = DetermineEnemyAction();

            ProcessAction(action);
        }

        /// <summary>
        /// Handle a companion's turn
        /// Uses companion AI to decide action
        /// </summary>
        private void CompanionTurn(Entities.NPCs.Companions.CompanionBase companion)
        {
            Console.WriteLine($"{companion.Name}'s HP: {companion.Health}/{companion.MaxHealth}");

            // Use companion AI to decide action: 0 = Attack, 1 = Ability, 2 = Defend
            int actionDecision = companion.DecideCombatAction(_enemy, _rngManager);

            switch (actionDecision)
            {
                case 1: // Use Ability
                    if (companion.UniqueAbility != null && !companion.UniqueAbility.IsOnCooldown())
                    {
                        Console.WriteLine($"{companion.Name} uses {companion.UniqueAbility.Name}!");
                        companion.UniqueAbility.Execute(companion, _enemy, _rngManager);
                        companion.LastAction = Entities.CombatAction.UseAbility;
                    }
                    else
                    {
                        // Fallback to attack if ability not available
                        Console.WriteLine($"{companion.Name} attacks!");
                        companion.AttackEnemy(_enemy, _damageCalculator, _rngManager, _player);
                        companion.LastAction = Entities.CombatAction.Attack;
                    }
                    break;

                case 2: // Defend
                    Console.WriteLine($"{companion.Name} takes a defensive stance!");
                    _defendingEntities[companion] = true;
                    companion.LastAction = Entities.CombatAction.Defend;
                    break;

                case 0: // Attack
                default:
                    Console.WriteLine($"{companion.Name} attacks!");
                    companion.AttackEnemy(_enemy, _damageCalculator, _rngManager, _player);
                    companion.LastAction = Entities.CombatAction.Attack;
                    break;
            }
        }

        /// <summary>
        /// Determine enemy action based on AI behavior
        /// </summary>
        private CombatAction DetermineEnemyAction()
        {
            // Calculate health percentage
            float healthPercent = (float)_enemy.Health / _enemy.MaxHealth;

            // Check if enemy has a special ability and can use it
            bool canUseAbility = _enemy.SpecialAbility != null &&
                                 _enemy.SpecialAbility.CurrentCooldown <= 0;

            // Select target - enemies can target player or companions
            Entity target = SelectEnemyTarget();

            // Decision logic based on behavior type
            switch (_enemy.Behavior)
            {
                case Entities.Enemies.EnemyBehavior.Aggressive:
                    // Always attack, use ability when available
                    if (canUseAbility && _rngManager.Roll(1, 100) <= 70)
                    {
                        return CombatAction.UseAbility(_enemy, target, _enemy.SpecialAbility);
                    }
                    return CombatAction.Attack(_enemy, target);

                case Entities.Enemies.EnemyBehavior.Defensive:
                    // Defend when low on health
                    if (healthPercent < 0.3f && _rngManager.Roll(1, 100) <= 60)
                    {
                        return CombatAction.Defend(_enemy);
                    }
                    if (canUseAbility && _rngManager.Roll(1, 100) <= 40)
                    {
                        return CombatAction.UseAbility(_enemy, target, _enemy.SpecialAbility);
                    }
                    return CombatAction.Attack(_enemy, target);

                case Entities.Enemies.EnemyBehavior.Tactical:
                    // Use abilities strategically
                    if (canUseAbility && _rngManager.Roll(1, 100) <= 80)
                    {
                        return CombatAction.UseAbility(_enemy, target, _enemy.SpecialAbility);
                    }
                    if (healthPercent < 0.25f && _rngManager.Roll(1, 100) <= 50)
                    {
                        return CombatAction.Defend(_enemy);
                    }
                    return CombatAction.Attack(_enemy, target);

                case Entities.Enemies.EnemyBehavior.Berserker:
                    // Always attack, rarely defend, use abilities aggressively
                    if (canUseAbility && _rngManager.Roll(1, 100) <= 90)
                    {
                        return CombatAction.UseAbility(_enemy, target, _enemy.SpecialAbility);
                    }
                    return CombatAction.Attack(_enemy, target);

                case Entities.Enemies.EnemyBehavior.Cautious:
                    // Defend frequently when low health, flee if very low
                    if (healthPercent < 0.15f && _rngManager.Roll(1, 100) <= 30)
                    {
                        return CombatAction.Flee(_enemy);
                    }
                    if (healthPercent < 0.4f && _rngManager.Roll(1, 100) <= 70)
                    {
                        return CombatAction.Defend(_enemy);
                    }
                    if (canUseAbility && _rngManager.Roll(1, 100) <= 50)
                    {
                        return CombatAction.UseAbility(_enemy, target, _enemy.SpecialAbility);
                    }
                    return CombatAction.Attack(_enemy, target);

                case Entities.Enemies.EnemyBehavior.Balanced:
                default:
                    // Mix of all actions
                    if (healthPercent < 0.3f && _rngManager.Roll(1, 100) <= 40)
                    {
                        return CombatAction.Defend(_enemy);
                    }
                    if (canUseAbility && _rngManager.Roll(1, 100) <= 60)
                    {
                        return CombatAction.UseAbility(_enemy, target, _enemy.SpecialAbility);
                    }
                    return CombatAction.Attack(_enemy, target);
            }
        }

        /// <summary>
        /// Select a target for the enemy to attack
        /// Prioritizes low health targets and wounded allies
        /// </summary>
        private Entity SelectEnemyTarget()
        {
            // Get all alive allies (player + companions)
            var aliveAllies = new List<Entity> { _player };
            foreach (var entity in _allCombatants)
            {
                if (entity is Entities.NPCs.Companions.CompanionBase companion && companion.IsAlive())
                {
                    aliveAllies.Add(companion);
                }
            }

            if (aliveAllies.Count == 1)
            {
                // Only player alive, target them
                return _player;
            }

            // Tactical enemy behavior - 70% chance to target wounded allies
            if (_rngManager.Roll(1, 100) <= 70)
            {
                // Find the most wounded ally
                Entity mostWounded = _player;
                float lowestHealthPercent = (float)_player.Health / _player.MaxHealth;

                foreach (var ally in aliveAllies)
                {
                    float healthPercent = (float)ally.Health / ally.MaxHealth;
                    if (healthPercent < lowestHealthPercent)
                    {
                        lowestHealthPercent = healthPercent;
                        mostWounded = ally;
                    }
                }

                return mostWounded;
            }
            else
            {
                // 30% chance to target randomly
                int randomIndex = _rngManager.Roll(0, aliveAllies.Count - 1);
                return aliveAllies[randomIndex];
            }
        }

        /// <summary>
        /// Process a combat action
        /// </summary>
        private void ProcessAction(CombatAction action)
        {
            switch (action.Type)
            {
                case ActionType.Attack:
                    ProcessAttack(action);
                    break;
                case ActionType.UseAbility:
                    ProcessAbility(action);
                    break;
                case ActionType.UseItem:
                    ProcessItem(action);
                    break;
                case ActionType.Defend:
                    ProcessDefend(action);
                    break;
                case ActionType.Flee:
                    ProcessFlee(action);
                    break;
            }
        }

        /// <summary>
        /// Process an attack action
        /// </summary>
        private void ProcessAttack(CombatAction action)
        {
            Console.WriteLine($"\n{action.Actor.Name} attacks {action.Target.Name}!");

            // Track action for speed modifier calculation
            action.Actor.LastAction = Entities.CombatAction.Attack;

            DamageResult result;

            // Calculate damage based on attacker and target types
            if (action.Actor == _player)
            {
                // Player attacking enemy
                result = _damageCalculator.CalculatePlayerAttackDamage(_player, _enemy);

                // Check if player is unarmed
                if (_player.EquippedWeapon == null)
                {
                    Console.WriteLine("(Fighting unarmed - find a weapon!)");
                }

                // Apply defense reduction if enemy is defending
                if (_defendingEntities.ContainsKey(_enemy) && _defendingEntities[_enemy])
                {
                    int damageReduction = result.FinalDamage / 2;
                    result.FinalDamage -= damageReduction;
                    Console.WriteLine($"{_enemy.Name}'s defensive stance reduced damage by {damageReduction}!");
                }
            }
            else if (action.Actor is Entities.NPCs.Companions.CompanionBase)
            {
                // Companion already handles their own attack in CompanionTurn
                // This shouldn't be reached, but handle it anyway
                return;
            }
            else
            {
                // Enemy attacking player or companion
                if (action.Target == _player)
                {
                    result = _damageCalculator.CalculateEnemyAttackDamage(_enemy, _player);
                }
                else if (action.Target is Entities.NPCs.Companions.CompanionBase companion)
                {
                    // Calculate enemy damage to companion
                    int baseDamage = _rngManager.Roll(_enemy.MinDamage, _enemy.MaxDamage);
                    int companionDefense = companion.EquippedArmor != null ? companion.EquippedArmor.Defense : 0;
                    int finalDamage = Math.Max(1, baseDamage - companionDefense);

                    result = new DamageResult
                    {
                        RawDamage = baseDamage,
                        DamageReduced = baseDamage - finalDamage,
                        FinalDamage = finalDamage,
                        Missed = false,
                        IsCritical = false
                    };
                }
                else
                {
                    result = _damageCalculator.CalculateEnemyAttackDamage(_enemy, _player);
                }

                // Check if target is defending (for evasion purposes)
                bool isDefending = _defendingEntities.ContainsKey(action.Target) && _defendingEntities[action.Target];

                // Apply defense reduction if target is defending
                if (isDefending)
                {
                    int damageReduction = result.FinalDamage / 2;
                    result.FinalDamage -= damageReduction;
                    Console.WriteLine($"{action.Target.Name}'s defensive stance reduced damage by {damageReduction}!");
                }

                // Check for Evasion passive ability (only for player, only when not defending)
                if (action.Target == _player && !isDefending && _player.SelectedAbility is Abilities.EvasionAbility evasion)
                {
                    if (evasion.ShouldEvade(_rngManager, isDefending))
                    {
                        Console.WriteLine($"{_player.Name} evaded the attack! No damage taken!");
                        result.FinalDamage = 0;
                    }
                }
            }

            // Display result
            if (result.Missed)
            {
                Console.WriteLine("The attack missed!");
            }
            else
            {
                if (result.IsCritical)
                {
                    Console.WriteLine("CRITICAL HIT!");
                }

                if (result.DamageReduced > 0)
                {
                    // Display appropriate message based on who was attacked
                    if (action.Target == _player)
                    {
                        Console.WriteLine($"Armor blocked {result.DamageReduced} damage!");
                    }
                    else
                    {
                        Console.WriteLine($"Defense blocked {result.DamageReduced} damage!");
                    }
                }

                // Apply damage (will be 0 if evaded)
                if (result.FinalDamage > 0)
                {
                    action.Target.TakeDamage(result.FinalDamage);
                    Console.WriteLine($"{action.Target.Name} took {result.FinalDamage} damage!");
                }
            }
        }

        /// <summary>
        /// Process an ability action
        /// </summary>
        private void ProcessAbility(CombatAction action)
        {
            // Track action for speed modifier calculation
            action.Actor.LastAction = Entities.CombatAction.UseAbility;

            if (action.Ability != null)
            {
                // Execute the ability
                action.Ability.Execute(action.Actor, action.Target, _rngManager);
            }
            else
            {
                Console.WriteLine("Error: No ability provided!");
            }
        }

        /// <summary>
        /// Process an item action
        /// </summary>
        private void ProcessItem(CombatAction action)
        {
            // Track action for speed modifier calculation
            action.Actor.LastAction = Entities.CombatAction.UseItem;

            if (action.Item != null)
            {
                // Check if it's a consumable (most combat items)
                if (action.Item is Items.Consumable consumable)
                {
                    // Special handling for combat consumables
                    if (consumable.Type == Items.ConsumableType.Bomb)
                    {
                        // Apply bomb damage to enemy
                        Console.WriteLine($"\n{action.Actor.Name} throws a {consumable.Name}!");
                        _enemy.TakeDamage(consumable.EffectPower);
                        Console.WriteLine($"The explosion dealt {consumable.EffectPower} damage to {_enemy.Name}!");
                        consumable.RemoveFromStack(1);
                    }
                    else if (consumable.Type == Items.ConsumableType.RevivePotion)
                    {
                        // Revival potion requires a target
                        if (action.Target == null)
                        {
                            Console.WriteLine($"\nError: Revival potion requires a target!");
                            return;
                        }

                        // Use the revival potion on the target
                        bool success = consumable.UseRevivePotion(action.Actor, action.Target);

                        if (success)
                        {
                            // Apply revival penalty: revived entities are disoriented and slower
                            int speedPenalty = _rngManager.Roll(2, 4);
                            action.Target.SpeedModifier = -speedPenalty;
                            Console.WriteLine($"{action.Target.Name} is disoriented from revival! Speed -{speedPenalty} for this round.");

                            // Add revived entity back to combat tracking if they were removed
                            if (!_entitiesActedThisRound.Contains(action.Target))
                            {
                                // Entity was revived - they haven't acted this round yet
                                Console.WriteLine($"{action.Target.Name} can act again this round!");
                            }
                        }
                    }
                    else
                    {
                        // Use consumable normally (healing, etc.)
                        consumable.Use(_player);
                    }

                    // Remove from inventory if stack is empty
                    if (consumable.Quantity <= 0)
                    {
                        _player.Inventory.RemoveItem(consumable);
                    }
                }
                else
                {
                    // For non-consumable items, just use them
                    action.Item.Use(_player);
                }
            }
            else
            {
                Console.WriteLine("Error: No item provided!");
            }
        }

        /// <summary>
        /// Process a defend action
        /// </summary>
        private void ProcessDefend(CombatAction action)
        {
            Console.WriteLine($"\n{action.Actor.Name} takes a defensive stance!");

            // Track action for speed modifier calculation
            action.Actor.LastAction = Entities.CombatAction.Defend;

            // Set defending flag for any entity
            _defendingEntities[action.Actor] = true;

            if (action.Actor == _player)
            {
                Console.WriteLine("Next incoming attack will deal reduced damage!");
            }
            else
            {
                Console.WriteLine($"{action.Actor.Name} braces for impact!");
            }
        }

        /// <summary>
        /// Process a flee action
        /// </summary>
        private void ProcessFlee(CombatAction action)
        {
            int fleeChance = 30; // Base 30% chance
            int fleeRoll = _rngManager.Roll(1, 100);

            if (fleeRoll <= fleeChance)
            {
                Console.WriteLine($"\n{action.Actor.Name} successfully fled from combat!");
                Console.WriteLine("You escaped, but gained no rewards...");
                Console.WriteLine($"Remaining HP: {_player.Health}/{_player.MaxHealth}");

                // Reset combat usage for abilities
                if (_player.SelectedAbility != null)
                {
                    _player.SelectedAbility.ResetCombatUsage();
                }

                // Notify all shops about flee (may apply penalties for consecutive flees)
                Entities.NPCs.ShopKeeper.NotifyAllShopsOfFlee();

                _combatActive = false;

                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
            }
            else
            {
                Console.WriteLine($"\n{action.Actor.Name} failed to escape!");
                Console.WriteLine("Turn wasted!");
            }
        }

        /// <summary>
        /// Show current combat status
        /// </summary>
        private void ShowCombatStatus()
        {
            Console.WriteLine("\n" + "-".PadRight(50, '-'));
            Console.WriteLine($"Player HP: {_player.Health}/{_player.MaxHealth}");

            // Show companion HP if any are in combat
            foreach (var entity in _allCombatants)
            {
                if (entity is Entities.NPCs.Companions.CompanionBase companion && entity != _player && entity != _enemy)
                {
                    string status = companion.IsAlive() ? $"{companion.Health}/{companion.MaxHealth}" : "DEFEATED";
                    Console.WriteLine($"{companion.Name} HP: {status}");
                }
            }

            Console.WriteLine($"Enemy HP: {_enemy.Health}/{_enemy.MaxHealth}");
            Console.WriteLine("-".PadRight(50, '-'));

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        /// <summary>
        /// Handle player victory
        /// </summary>
        private bool HandleVictory()
        {
            Console.Clear();
            Console.WriteLine("\n" + "=".PadRight(50, '='));
            Console.WriteLine("           VICTORY!");
            Console.WriteLine("=".PadRight(50, '='));

            // Calculate rewards
            int xpGained = CalculateXPReward();
            int goldGained = CalculateGoldReward();

            Console.WriteLine($"\n{_enemy.Name} has been defeated!");
            Console.WriteLine($"\nYou gained {xpGained} XP!");
            Console.WriteLine($"You gained {goldGained} gold!");

            // Apply rewards to player
            _player.AddExperience(xpGained);
            _player.Gold += goldGained;

            // Share XP with companions
            foreach (var entity in _allCombatants)
            {
                if (entity is Entities.NPCs.Companions.CompanionBase companion && companion.IsAlive())
                {
                    companion.GainExperience(xpGained);
                }
            }

            // Roll for loot drops
            var lootDrops = _enemy.GetLootDrops(_rngManager);
            if (lootDrops.Count > 0)
            {
                Console.WriteLine("\n--- LOOT DROPS ---");
                foreach (var item in lootDrops)
                {
                    _player.AddToInventory(item);
                    Console.WriteLine($"Obtained: {item.GetDisplayName()}");
                }
            }
            else
            {
                Console.WriteLine("\nNo items dropped.");
            }

            // Handle boss defeats
            if (_enemy is BossEnemy defeatedBoss && _bossManager != null)
            {
                _bossManager.DefeatBoss(defeatedBoss.BossId);
            }

            // Reset combat usage for abilities
            if (_player.SelectedAbility != null)
            {
                _player.SelectedAbility.ResetCombatUsage();
            }

            // Reset companion abilities
            foreach (var entity in _allCombatants)
            {
                if (entity is Entities.NPCs.Companions.CompanionBase companion)
                {
                    if (companion.UniqueAbility != null)
                    {
                        companion.UniqueAbility.ResetCombatUsage();
                    }
                }
            }

            // Notify all shops that a combat encounter occurred (for restock tracking)
            Entities.NPCs.ShopKeeper.NotifyAllShopsOfCombat();

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();

            return true; // Player won
        }

        /// <summary>
        /// Handle player defeat
        /// </summary>
        private bool HandleDefeat()
        {
            Console.Clear();
            Console.WriteLine("\n" + "=".PadRight(50, '='));
            Console.WriteLine("           DEFEAT");
            Console.WriteLine("=".PadRight(50, '='));

            Console.WriteLine($"\n{_player.Name} has been defeated by {_enemy.Name}...");
            Console.WriteLine("\nGAME OVER");

            // Reset combat usage for abilities
            if (_player.SelectedAbility != null)
            {
                _player.SelectedAbility.ResetCombatUsage();
            }

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();

            return false; // Player lost
        }

        /// <summary>
        /// Calculate XP reward with randomness
        /// </summary>
        private int CalculateXPReward()
        {
            int minXP = _enemy.Level * 30;
            int maxXP = _enemy.Level * 70;
            
            int baseXP = _rngManager.Roll(minXP, maxXP);
            
            // 10% chance for bonus XP
            int bonusRoll = _rngManager.Roll(1, 100);
            if (bonusRoll <= 10)
            {
                int bonus = baseXP / 2;
                Console.WriteLine($"Bonus XP! +{bonus}");
                baseXP += bonus;
            }
            
            return baseXP;
        }

        /// <summary>
        /// Calculate gold reward with randomness
        /// </summary>
        private int CalculateGoldReward()
        {
            int minGold = _enemy.Level * 3;
            int maxGold = _enemy.Level * 20;
            
            int gold = _rngManager.Roll(minGold, maxGold);
            
            // 5% chance for treasure bonus
            int treasureRoll = _rngManager.Roll(1, 100);
            if (treasureRoll <= 5)
            {
                Console.WriteLine("The enemy dropped extra gold!");
                gold *= 2;
            }
            
            return gold;
        }
    }
}