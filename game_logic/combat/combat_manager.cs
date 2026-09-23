using System;
using System.Collections.Generic;
using System.Linq;
using GameLogic.Abilities;
using GameLogic.Entities;
using GameLogic.Entities.Player;
using GameLogic.Entities.Enemies;
using GameLogic.Entities.Enemies.Bosses;
using GameLogic.Entities.NPCs;
using GameLogic.Entities.NPCs.Companions;
using GameLogic.Systems;
using Enemy = GameLogic.Entities.Enemies.EnemyBase;

namespace GameLogic.Combat
{
    /// <summary>
    /// Orchestrates combat encounters between the player (plus companions) and one or
    /// more enemies. Delegates to specialized managers for turn order, damage, and actions.
    /// Uses a round-based system where each entity acts once per round.
    ///
    /// Non-blocking/resumable: StartCombat() sets up the fight and advances AI turns
    /// automatically, pausing (IsWaitingForPlayerAction) whenever it's the player's turn.
    /// The caller (e.g. a Godot combat scene) calls SubmitPlayerAction() to resume.
    /// Subscribe to CombatMessage for narrative log lines and CombatEnded for the outcome.
    /// </summary>
    public class CombatManager
    {
        private RNGManager _rngManager;
        private TurnManager _turnManager;
        private DamageCalculator _damageCalculator;
        private BossManager _bossManager;

        private Player _player;
        private List<Enemy> _enemies;
        private bool _combatActive;

        // Round-based combat tracking
        private int _currentRound;
        private List<Entity> _allCombatants;
        private HashSet<Entity> _entitiesActedThisRound;
        private Queue<Entity> _turnQueue;

        // Universal defend tracking - any entity can defend
        private Dictionary<Entity, bool> _defendingEntities;

        /// <summary>
        /// Fired for narrative combat-log lines (attacks, abilities, items, round events, rewards).
        /// TODO-GODOT: subscribe to this to drive a combat-log UI.
        /// </summary>
        public event Action<string> CombatMessage;

        /// <summary>
        /// Fired exactly once per combat, with true = victory, false = defeat or fled.
        /// TODO-GODOT: subscribe to this to know when to leave the combat scene.
        /// </summary>
        public event Action<bool> CombatEnded;

        /// <summary>
        /// True whenever combat is paused waiting for SubmitPlayerAction().
        /// </summary>
        public bool IsWaitingForPlayerAction { get; private set; }

        public Player Player => _player;
        public IReadOnlyList<Enemy> Enemies => _enemies;
        public IReadOnlyList<Entity> Combatants => _allCombatants;

        public CombatManager(RNGManager rngManager)
        {
            _rngManager = rngManager;
            _turnManager = new TurnManager();
            _damageCalculator = new DamageCalculator(rngManager);
            _defendingEntities = new Dictionary<Entity, bool>();
            _allCombatants = new List<Entity>();
            _entitiesActedThisRound = new HashSet<Entity>();
            _turnQueue = new Queue<Entity>();
            _enemies = new List<Enemy>();
            _currentRound = 0;
        }

        /// <summary>
        /// Write a narrative combat-log line - keeps console text-testing working
        /// while also notifying any subscriber (e.g. a Godot combat-log UI).
        /// </summary>
        private void Log(string message)
        {
            Console.WriteLine(message);
            CombatMessage?.Invoke(message);
        }

        /// <summary>
        /// Start a combat encounter against one or more enemies. Resolves AI turns
        /// automatically and pauses (IsWaitingForPlayerAction = true) whenever it's the
        /// player's turn - call SubmitPlayerAction() to resume. Subscribe to CombatEnded
        /// for the outcome.
        /// </summary>
        /// <param name="player">The player character</param>
        /// <param name="enemies">The enemy or enemies to fight</param>
        /// <param name="companions">List of active companions (can be null or empty)</param>
        /// <param name="bossManager">Optional boss manager for boss encounters</param>
        public void StartCombat(Player player, List<Enemy> enemies, List<Entity> companions = null, BossManager bossManager = null)
        {
            _player = player;
            _enemies = enemies;
            _combatActive = true;
            _bossManager = bossManager;

            // If any of these are bosses, apply strength scaling
            foreach (var enemy in _enemies)
            {
                if (enemy is BossEnemy boss && _bossManager != null)
                {
                    _bossManager.ApplyBossScaling(boss);
                }
            }

            // Setup combatants list
            _allCombatants.Clear();
            _allCombatants.Add(_player);

            // Add companions that are in the party
            if (companions != null)
            {
                foreach (var companion in companions)
                {
                    // Cast to NPCBase to access InParty property
                    var npcCompanion = companion as NPCBase;
                    if (npcCompanion != null && npcCompanion.InParty && companion.IsAlive())
                    {
                        _allCombatants.Add(companion);
                    }
                }
            }

            _allCombatants.AddRange(_enemies);

            // Clear tracking structures
            _defendingEntities.Clear();
            _entitiesActedThisRound.Clear();
            _currentRound = 1;

            // Initialize turn manager (vestigial - kept for compatibility, not used to drive turns)
            _turnManager.InitializeCombat(player, _enemies.FirstOrDefault());

            // Display combat start
            ShowCombatIntro();

            BeginRound();
            AdvanceTurns();
        }

        /// <summary>
        /// Submit the player's chosen action (built via CombatAction.Attack/UseAbility/UseItem/Defend/Flee -
        /// picking which enemy to target when there's more than one) while IsWaitingForPlayerAction is
        /// true. Resolves it, applies the player's own end-of-turn effects, then automatically advances
        /// through any remaining AI turns until the next player decision point or combat end.
        /// TODO-GODOT: call this from the combat scene when the player confirms an action.
        /// </summary>
        public void SubmitPlayerAction(CombatAction action)
        {
            if (!IsWaitingForPlayerAction) return;

            IsWaitingForPlayerAction = false;

            ProcessAction(action);

            if (FinishTurn(_player)) return;

            AdvanceTurns();
        }

        /// <summary>
        /// Display combat introduction
        /// </summary>
        private void ShowCombatIntro()
        {
            Console.Clear();
            Log("=".PadRight(50, '='));
            Log($"    COMBAT: {_player.Name} vs {string.Join(", ", _enemies.Select(e => e.Name))}");
            Log("=".PadRight(50, '='));

            // Show party composition
            Log($"\nYour Party:");
            Log($"  {_player.Name} (Level {_player.Level}) - HP: {_player.Health}/{_player.MaxHealth}");

            foreach (var entity in _allCombatants)
            {
                if (entity is Entities.NPCs.Companions.CompanionBase companion && entity != _player)
                {
                    Log($"  {companion.Name} (Level {companion.Level}) - HP: {companion.Health}/{companion.MaxHealth}");
                }
            }

            Log($"\nEnemies:");
            foreach (var enemy in _enemies)
            {
                Log($"  {enemy.Name} (Level {enemy.Level}) - HP: {enemy.Health}/{enemy.MaxHealth}");
            }
            Log("");
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
        /// Begin a new round - sort turn order, fill the turn queue, and reset per-round tracking.
        /// </summary>
        private void BeginRound()
        {
            Log("\n" + "=".PadRight(50, '='));
            Log($"           ROUND {_currentRound}");
            Log("=".PadRight(50, '='));

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
            Log("\nTurn Order (by Speed):");
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

                    Log($"  {entity.Name} (Speed: {effectiveSpeed}{modifierText})");
                }
            }
            Log("");

            // Reset defend status for all entities at start of new round
            foreach (var entity in _allCombatants)
            {
                if (_defendingEntities.ContainsKey(entity))
                {
                    _defendingEntities[entity] = false;
                }
            }

            // Fill the turn queue in speed order (dead entities are queued too, matching
            // the old behavior of skipping them at the point their turn comes up - this
            // preserves "revived mid-round" semantics: an entity gets to act this round
            // only if their queue position hasn't been reached yet)
            _turnQueue.Clear();
            foreach (var entity in _allCombatants)
            {
                _turnQueue.Enqueue(entity);
            }
        }

        /// <summary>
        /// Resolve entities' turns automatically (AI-controlled) until the queue is
        /// empty (round ends, next round begins), combat ends, or it's the player's
        /// turn - at which point this returns and waits for SubmitPlayerAction().
        /// </summary>
        private void AdvanceTurns()
        {
            while (true)
            {
                if (_turnQueue.Count == 0)
                {
                    EndRound();
                    if (!_combatActive)
                    {
                        CombatEnded?.Invoke(false);
                        return;
                    }

                    _currentRound++;
                    BeginRound();
                    continue;
                }

                var entity = _turnQueue.Dequeue();

                if (!entity.IsAlive() || _entitiesActedThisRound.Contains(entity))
                {
                    continue;
                }

                if (entity == _player)
                {
                    Log($"\n{entity.Name}'s Turn!");
                    Log($"Your HP: {_player.Health}/{_player.MaxHealth}");
                    Log($"Enemies: {string.Join(", ", _enemies.Where(e => e.IsAlive()).Select(e => $"{e.Name} ({e.Health}/{e.MaxHealth})"))}");

                    IsWaitingForPlayerAction = true;
                    return;
                }

                ExecuteEntityTurn(entity);

                if (FinishTurn(entity)) return;
            }
        }

        /// <summary>
        /// Apply an entity's end-of-turn status effects (poison/bleed/etc. tick here now,
        /// not at turn start), mark them as having acted, and check whether combat ended
        /// as a result. Returns true if combat ended (caller should stop advancing turns).
        /// </summary>
        private bool FinishTurn(Entity entity)
        {
            _entitiesActedThisRound.Add(entity);

            entity.ProcessEffects(_rngManager);

            if (!entity.IsAlive())
            {
                Log($"{entity.Name} succumbed to a status effect!");
            }

            return CheckForCombatEnd();
        }

        /// <summary>
        /// Check whether combat has ended (victory/defeat/fled) and, if so, resolve
        /// rewards/cleanup and fire CombatEnded.
        /// </summary>
        private bool CheckForCombatEnd()
        {
            if (_enemies.All(e => !e.IsAlive()))
            {
                HandleVictory();
                CombatEnded?.Invoke(true);
                return true;
            }

            if (_player.Health <= 0)
            {
                HandleDefeat();
                CombatEnded?.Invoke(false);
                return true;
            }

            if (!_combatActive)
            {
                CombatEnded?.Invoke(false);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Execute a turn for the given (non-player) entity
        /// </summary>
        private void ExecuteEntityTurn(Entity entity)
        {
            Log($"\n{entity.Name}'s Turn!");

            if (entity is Enemy enemy)
            {
                EnemyTurn(enemy);
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
            Log("\n--- Speed Adjustments for Next Round ---");

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
                    Log($"{entity.Name}'s speed {changeText} from {actionText}");
                }
            }
        }

        /// <summary>
        /// End the current round - process end-of-round effects (duration countdown/expiry
        /// only - the effect damage/healing itself is applied at end-of-turn via FinishTurn())
        /// </summary>
        private void EndRound()
        {
            Log("\n--- End of Round ---");

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

            // Tick down effect durations for all entities (duration only - effects were
            // already applied at each entity's own end-of-turn this round)
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
                        Log($"\n{_player.Name}'s Iron Will activated! All negative effects cleansed!");
                    }
                }
            }

            // Reduce ability cooldowns for all entities at end of round
            if (_player.SelectedAbility != null && _player.SelectedAbility.CurrentCooldown > 0)
            {
                _player.SelectedAbility.CurrentCooldown--;
            }

            foreach (var enemy in _enemies)
            {
                if (enemy.SpecialAbility != null && enemy.SpecialAbility.CurrentCooldown > 0)
                {
                    enemy.SpecialAbility.CurrentCooldown--;
                }
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
        }

        /// <summary>
        /// Handle a specific enemy's turn
        /// Uses AI to decide action based on that enemy's behavior and situation
        /// </summary>
        private void EnemyTurn(Enemy enemy)
        {
            // Determine action based on enemy behavior and situation
            CombatAction action = DetermineEnemyAction(enemy);

            ProcessAction(action);
        }

        /// <summary>
        /// Handle a companion's turn
        /// Uses companion AI to decide action
        /// </summary>
        private void CompanionTurn(Entity companion)
        {
            Log($"{companion.Name}'s HP: {companion.Health}/{companion.MaxHealth}");

            // Cast to CompanionBase to access companion-specific methods
            var companionBase = companion as CompanionBase;
            if (companionBase == null)
            {
                // Fallback: basic attack if not a proper companion
                Log($"{companion.Name} attacks!");
                companion.LastAction = Entities.CombatAction.Attack;
                return;
            }

            // Pick which enemy this companion engages (lowest-HP living enemy)
            Enemy target = SelectCompanionAttackTarget();
            if (target == null)
            {
                // No living enemies to act against (shouldn't happen if combat is still active)
                return;
            }

            // Use companion AI to decide action: 0 = Attack, 1 = Ability, 2 = Defend
            int actionDecision = companionBase.DecideCombatAction(target, _rngManager);

            switch (actionDecision)
            {
                case 1: // Use Ability
                    if (companionBase.UniqueAbility != null && !companionBase.UniqueAbility.IsOnCooldown())
                    {
                        Log($"{companionBase.Name} uses {companionBase.UniqueAbility.Name}!");
                        companionBase.UniqueAbility.Execute(companionBase, target, _rngManager);
                        companion.LastAction = Entities.CombatAction.UseAbility;
                    }
                    else
                    {
                        // Fallback to attack if ability not available
                        Log($"{companionBase.Name} attacks!");
                        companionBase.AttackEnemy(target, _damageCalculator, _rngManager, _player);
                        companion.LastAction = Entities.CombatAction.Attack;
                    }
                    break;

                case 2: // Defend
                    Log($"{companionBase.Name} takes a defensive stance!");
                    _defendingEntities[companion] = true;
                    companion.LastAction = Entities.CombatAction.Defend;
                    break;

                case 0: // Attack
                default:
                    Log($"{companionBase.Name} attacks!");
                    companionBase.AttackEnemy(target, _damageCalculator, _rngManager, _player);
                    companion.LastAction = Entities.CombatAction.Attack;
                    break;
            }
        }

        /// <summary>
        /// Pick which enemy a companion should attack/target this turn - the lowest-HP
        /// living enemy (mirrors SelectEnemyTarget()'s "prioritize wounded targets" heuristic).
        /// </summary>
        private Enemy SelectCompanionAttackTarget()
        {
            Enemy target = null;
            float lowestHealthPercent = float.MaxValue;

            foreach (var enemy in _enemies)
            {
                if (!enemy.IsAlive()) continue;

                float healthPercent = (float)enemy.Health / enemy.MaxHealth;
                if (target == null || healthPercent < lowestHealthPercent)
                {
                    lowestHealthPercent = healthPercent;
                    target = enemy;
                }
            }

            return target;
        }

        /// <summary>
        /// Determine an enemy's action based on its AI behavior
        /// </summary>
        private CombatAction DetermineEnemyAction(Enemy enemy)
        {
            // Calculate health percentage
            float healthPercent = (float)enemy.Health / enemy.MaxHealth;

            // Check if enemy has a special ability and can use it
            bool canUseAbility = enemy.SpecialAbility != null &&
                                 enemy.SpecialAbility.CurrentCooldown <= 0;

            // Select target - enemies can target player or companions
            Entity target = SelectEnemyTarget();

            // Decision logic based on behavior type
            switch (enemy.Behavior)
            {
                case Entities.Enemies.EnemyBehavior.Aggressive:
                    // Always attack, use ability when available
                    if (canUseAbility && _rngManager.Roll(1, 100) <= 70)
                    {
                        return CombatAction.UseAbility(enemy, enemy.SpecialAbility, target);
                    }
                    return CombatAction.Attack(enemy, target);

                case Entities.Enemies.EnemyBehavior.Defensive:
                    // Defend when low on health
                    if (healthPercent < 0.3f && _rngManager.Roll(1, 100) <= 60)
                    {
                        return CombatAction.Defend(enemy);
                    }
                    if (canUseAbility && _rngManager.Roll(1, 100) <= 40)
                    {
                        return CombatAction.UseAbility(enemy, enemy.SpecialAbility, target);
                    }
                    return CombatAction.Attack(enemy, target);

                case Entities.Enemies.EnemyBehavior.Tactical:
                    // Use abilities strategically
                    if (canUseAbility && _rngManager.Roll(1, 100) <= 80)
                    {
                        return CombatAction.UseAbility(enemy, enemy.SpecialAbility, target);
                    }
                    if (healthPercent < 0.25f && _rngManager.Roll(1, 100) <= 50)
                    {
                        return CombatAction.Defend(enemy);
                    }
                    return CombatAction.Attack(enemy, target);

                case Entities.Enemies.EnemyBehavior.Berserker:
                    // Always attack, rarely defend, use abilities aggressively
                    if (canUseAbility && _rngManager.Roll(1, 100) <= 90)
                    {
                        return CombatAction.UseAbility(enemy, enemy.SpecialAbility, target);
                    }
                    return CombatAction.Attack(enemy, target);

                case Entities.Enemies.EnemyBehavior.Cautious:
                    // Defend frequently when low health, flee if very low
                    if (healthPercent < 0.15f && _rngManager.Roll(1, 100) <= 30)
                    {
                        return CombatAction.Flee(enemy);
                    }
                    if (healthPercent < 0.4f && _rngManager.Roll(1, 100) <= 70)
                    {
                        return CombatAction.Defend(enemy);
                    }
                    if (canUseAbility && _rngManager.Roll(1, 100) <= 50)
                    {
                        return CombatAction.UseAbility(enemy, enemy.SpecialAbility, target);
                    }
                    return CombatAction.Attack(enemy, target);

                case Entities.Enemies.EnemyBehavior.Balanced:
                default:
                    // Mix of all actions
                    if (healthPercent < 0.3f && _rngManager.Roll(1, 100) <= 40)
                    {
                        return CombatAction.Defend(enemy);
                    }
                    if (canUseAbility && _rngManager.Roll(1, 100) <= 60)
                    {
                        return CombatAction.UseAbility(enemy, enemy.SpecialAbility, target);
                    }
                    return CombatAction.Attack(enemy, target);
            }
        }

        /// <summary>
        /// Select a target for an attacking enemy
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
            Log($"\n{action.Actor.Name} attacks {action.Target.Name}!");

            // Track action for speed modifier calculation
            action.Actor.LastAction = Entities.CombatAction.Attack;

            DamageResult result;

            // Calculate damage based on attacker and target types
            if (action.Actor == _player)
            {
                // Player attacking a specific enemy (action.Target, not a shared field - there
                // can be several enemies now)
                var targetEnemy = (Enemy)action.Target;
                result = _damageCalculator.CalculatePlayerAttackDamage(_player, targetEnemy);

                // Check if player is unarmed
                if (_player.EquippedWeapon == null)
                {
                    Log("(Fighting unarmed - find a weapon!)");
                }

                // Apply defense reduction if the targeted enemy is defending
                if (_defendingEntities.ContainsKey(targetEnemy) && _defendingEntities[targetEnemy])
                {
                    int damageReduction = result.FinalDamage / 2;
                    result.FinalDamage -= damageReduction;
                    Log($"{targetEnemy.Name}'s defensive stance reduced damage by {damageReduction}!");
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
                // A specific enemy (action.Actor) attacking the player or a companion
                var attackingEnemy = (Enemy)action.Actor;

                if (action.Target == _player)
                {
                    result = _damageCalculator.CalculateEnemyAttackDamage(attackingEnemy, _player);
                }
                else if (action.Target is Entities.NPCs.Companions.CompanionBase companion)
                {
                    // Calculate enemy damage to companion
                    int baseDamage = _rngManager.Roll(attackingEnemy.MinDamage, attackingEnemy.MaxDamage);
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
                    result = _damageCalculator.CalculateEnemyAttackDamage(attackingEnemy, _player);
                }

                // Check if target is defending (for evasion purposes)
                bool isDefending = _defendingEntities.ContainsKey(action.Target) && _defendingEntities[action.Target];

                // Apply defense reduction if target is defending
                if (isDefending)
                {
                    int damageReduction = result.FinalDamage / 2;
                    result.FinalDamage -= damageReduction;
                    Log($"{action.Target.Name}'s defensive stance reduced damage by {damageReduction}!");
                }

                // Check for Evasion passive ability (only for player, only when not defending)
                if (action.Target == _player && !isDefending && _player.SelectedAbility is Abilities.EvasionAbility evasion)
                {
                    if (evasion.ShouldEvade(_rngManager, isDefending))
                    {
                        Log($"{_player.Name} evaded the attack! No damage taken!");
                        result.FinalDamage = 0;
                    }
                }
            }

            // Display result
            if (result.Missed)
            {
                Log("The attack missed!");
            }
            else
            {
                if (result.IsCritical)
                {
                    Log("CRITICAL HIT!");
                }

                if (result.DamageReduced > 0)
                {
                    // Display appropriate message based on who was attacked
                    if (action.Target == _player)
                    {
                        Log($"Armor blocked {result.DamageReduced} damage!");
                    }
                    else
                    {
                        Log($"Defense blocked {result.DamageReduced} damage!");
                    }
                }

                // Apply damage (will be 0 if evaded)
                if (result.FinalDamage > 0)
                {
                    action.Target.TakeDamage(result.FinalDamage);
                    Log($"{action.Target.Name} took {result.FinalDamage} damage!");
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
                Log("Error: No ability provided!");
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
                        // Bombs need a specific enemy target now that there can be several
                        var targetEnemy = action.Target as Enemy;
                        if (targetEnemy == null)
                        {
                            Log("Error: Bomb requires a target enemy!");
                            return;
                        }

                        Log($"\n{action.Actor.Name} throws a {consumable.Name}!");
                        targetEnemy.TakeDamage(consumable.EffectPower);
                        Log($"The explosion dealt {consumable.EffectPower} damage to {targetEnemy.Name}!");
                        consumable.RemoveFromStack(1);
                    }
                    else if (consumable.Type == Items.ConsumableType.RevivePotion)
                    {
                        // Revival potion requires a target
                        if (action.Target == null)
                        {
                            Log($"\nError: Revival potion requires a target!");
                            return;
                        }

                        // Use the revival potion on the target
                        bool success = consumable.UseRevivePotion(action.Actor, action.Target);

                        if (success)
                        {
                            // Apply revival penalty: revived entities are disoriented and slower
                            int speedPenalty = _rngManager.Roll(2, 4);
                            action.Target.SpeedModifier = -speedPenalty;
                            Log($"{action.Target.Name} is disoriented from revival! Speed -{speedPenalty} for this round.");

                            // Add revived entity back to combat tracking if they were removed
                            if (!_entitiesActedThisRound.Contains(action.Target))
                            {
                                // Entity was revived - they haven't acted this round yet
                                Log($"{action.Target.Name} can act again this round!");
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
                Log("Error: No item provided!");
            }
        }

        /// <summary>
        /// Process a defend action
        /// </summary>
        private void ProcessDefend(CombatAction action)
        {
            Log($"\n{action.Actor.Name} takes a defensive stance!");

            // Track action for speed modifier calculation
            action.Actor.LastAction = Entities.CombatAction.Defend;

            // Set defending flag for any entity
            _defendingEntities[action.Actor] = true;

            if (action.Actor == _player)
            {
                Log("Next incoming attack will deal reduced damage!");
            }
            else
            {
                Log($"{action.Actor.Name} braces for impact!");
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
                Log($"\n{action.Actor.Name} successfully fled from combat!");
                Log("You escaped, but gained no rewards...");
                Log($"Remaining HP: {_player.Health}/{_player.MaxHealth}");

                // Reset combat usage for abilities
                if (_player.SelectedAbility != null)
                {
                    _player.SelectedAbility.ResetCombatUsage();
                }

                // Notify all shops about flee (may apply penalties for consecutive flees)
                Entities.NPCs.ShopKeeper.NotifyAllShopsOfFlee();

                _combatActive = false;
            }
            else
            {
                Log($"\n{action.Actor.Name} failed to escape!");
                Log("Turn wasted!");
            }
        }

        /// <summary>
        /// Show current combat status
        /// </summary>
        private void ShowCombatStatus()
        {
            Log("\n" + "-".PadRight(50, '-'));
            Log($"Player HP: {_player.Health}/{_player.MaxHealth}");

            // Show companion HP if any are in combat
            foreach (var entity in _allCombatants)
            {
                if (entity is Entities.NPCs.Companions.CompanionBase companion && entity != _player)
                {
                    string status = companion.IsAlive() ? $"{companion.Health}/{companion.MaxHealth}" : "DEFEATED";
                    Log($"{companion.Name} HP: {status}");
                }
            }

            foreach (var enemy in _enemies)
            {
                string status = enemy.IsAlive() ? $"{enemy.Health}/{enemy.MaxHealth}" : "DEFEATED";
                Log($"{enemy.Name} HP: {status}");
            }

            Log("-".PadRight(50, '-'));
        }

        /// <summary>
        /// Handle player victory - rewards aggregate across every defeated enemy
        /// </summary>
        private void HandleVictory()
        {
            Console.Clear();
            Log("\n" + "=".PadRight(50, '='));
            Log("           VICTORY!");
            Log("=".PadRight(50, '='));

            int totalXP = 0;
            int totalGold = 0;
            var allLootItems = new List<Items.Item>();
            var lootGenerator = new Progression.LootGenerator(new RNGManagerRandomAdapter(_rngManager));

            foreach (var enemy in _enemies)
            {
                Log($"\n{enemy.Name} has been defeated!");

                totalXP += CalculateXPReward(enemy);

                // Roll for loot (gold + items) from the unified loot table system
                var lootTable = Progression.LootTableTemplates.CreateForEnemyType(enemy.Type);
                var loot = lootGenerator.GenerateLoot(lootTable, _player.Level, areaLevel: enemy.Level);
                totalGold += loot.Gold;
                allLootItems.AddRange(loot.Items);

                // Bosses additionally roll their Champion Key with diminishing returns on repeats
                if (enemy is BossEnemy boss)
                {
                    allLootItems.AddRange(boss.GetLootDrops(_rngManager));
                }

                // Handle boss defeats
                if (enemy is BossEnemy defeatedBoss && _bossManager != null)
                {
                    _bossManager.DefeatBoss(defeatedBoss.BossId);
                }
            }

            Log($"\nYou gained {totalXP} XP!");
            _player.AddExperience(totalXP);

            // Share XP with companions
            foreach (var entity in _allCombatants)
            {
                if (entity is Entities.NPCs.Companions.CompanionBase companion && companion.IsAlive())
                {
                    companion.GainExperience(totalXP);
                }
            }

            _player.Gold += totalGold;
            Log($"You gained {totalGold} gold!");

            if (allLootItems.Count > 0)
            {
                Log("\n--- LOOT DROPS ---");
                foreach (var item in allLootItems)
                {
                    _player.AddToInventory(item);
                    Log($"Obtained: {item.GetDisplayName()}");
                }
            }
            else
            {
                Log("\nNo items dropped.");
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
        }

        /// <summary>
        /// Handle player defeat
        /// </summary>
        private void HandleDefeat()
        {
            Console.Clear();
            Log("\n" + "=".PadRight(50, '='));
            Log("           DEFEAT");
            Log("=".PadRight(50, '='));

            Log($"\n{_player.Name} has been defeated...");
            Log("\nGAME OVER");

            // Reset combat usage for abilities
            if (_player.SelectedAbility != null)
            {
                _player.SelectedAbility.ResetCombatUsage();
            }
        }

        /// <summary>
        /// Calculate XP reward for defeating a specific enemy, with randomness
        /// </summary>
        private int CalculateXPReward(Enemy enemy)
        {
            int minXP = enemy.Level * 30;
            int maxXP = enemy.Level * 70;

            int baseXP = _rngManager.Roll(minXP, maxXP);

            // 10% chance for bonus XP
            int bonusRoll = _rngManager.Roll(1, 100);
            if (bonusRoll <= 10)
            {
                int bonus = baseXP / 2;
                Log($"Bonus XP! +{bonus}");
                baseXP += bonus;
            }

            return baseXP;
        }
    }
}
