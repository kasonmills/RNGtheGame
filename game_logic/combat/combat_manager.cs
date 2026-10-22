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
    /// Orchestrates combat encounters between the party (the player plus up to three
    /// companions) and one or more enemies.
    ///
    /// Rounds: every round has 8 turn slots - 4 for the party (one each) and 4 for the
    /// opponents (more than 4 enemies share slots, the slowest paired up first). Slots act
    /// in speed order, worked out fresh at the start of every round. See TurnOrder.
    ///
    /// Non-blocking/resumable: StartCombat() sets up the fight and runs computer-controlled
    /// turns automatically, pausing (IsWaitingForPlayerAction) whenever a party member the
    /// player controls is up - the player, or a companion switched to direct control. The
    /// caller (e.g. a Godot combat scene) reads CurrentActor and calls SubmitPlayerAction().
    ///
    /// For a screen:
    ///   Party / Enemies / Combatants - who is in the fight; read names, levels, health and
    ///                                  effects from them at any time during the fight
    ///   CombatEventOccurred - one event per thing that happens (hit, miss, damage, heal...)
    ///   CombatMessage       - the same fight as readable log lines
    ///   CombatFinished      - the end-of-fight result (outcome, XP, gold, items)
    /// </summary>
    public class CombatManager
    {
        /// <summary>
        /// The player plus at most this many companions fight (decided 2026-10-13).
        /// </summary>
        public const int MaxFightingCompanions = 3;

        private const int SlotsPerSide = 4;

        private RNGManager _rngManager;
        private TurnManager _turnManager;
        private DamageCalculator _damageCalculator;
        private BossManager _bossManager;

        private Player _player;
        private List<Enemy> _enemies;
        private readonly List<Entity> _party;
        private readonly List<Entity> _allCombatants;
        private bool _combatActive;
        private bool _partyFled;

        // Round-based combat tracking
        private int _currentRound;
        private List<TurnSlot> _turnOrder;
        private readonly Queue<Entity> _turnQueue;

        // Who is defending (until their next turn), which enemies fled, and which
        // companions the player controls directly
        private readonly HashSet<Entity> _defendingEntities;
        private readonly HashSet<Enemy> _fledEnemies;
        private readonly HashSet<Entity> _directlyControlled;

        // Speed penalties from being revived, waiting to be added to the next round's speed
        private readonly Dictionary<Entity, int> _pendingRevivePenalties;

        /// <summary>
        /// Fired for narrative combat-log lines (attacks, abilities, items, round events, rewards).
        /// TODO-GODOT: subscribe to this to drive a combat-log UI.
        /// </summary>
        public event Action<string> CombatMessage;

        /// <summary>
        /// Fired once for each thing that happens in the fight, in order - use it to show damage
        /// numbers, "Miss", "Critical hit", health bars moving and so on at the right moment.
        /// TODO-GODOT: subscribe to this to drive the combat scene's visual feedback.
        /// </summary>
        public event Action<CombatEvent> CombatEventOccurred;

        /// <summary>
        /// Fired exactly once per combat with the result (how it ended, XP, gold, items).
        /// TODO-GODOT: subscribe to this to show the results screen and leave the combat scene.
        /// </summary>
        public event Action<CombatResult> CombatFinished;

        /// <summary>
        /// Older yes/no version of CombatFinished, kept so existing callers keep working: fired
        /// exactly once per combat, true when the party won or the opponents fled, false when
        /// the party lost or fled. New code should use CombatFinished.
        /// </summary>
        public event Action<bool> CombatEnded;

        /// <summary>
        /// True whenever combat is paused waiting for SubmitPlayerAction() - for the party
        /// member in CurrentActor.
        /// </summary>
        public bool IsWaitingForPlayerAction { get; private set; }

        /// <summary>
        /// Whose turn it is (null between turns and after the fight).
        /// </summary>
        public Entity CurrentActor { get; private set; }

        public bool IsCombatActive => _combatActive;
        public int CurrentRound => _currentRound;

        /// <summary>
        /// The result of the most recent fight (null until a fight has ended).
        /// </summary>
        public CombatResult LastResult { get; private set; }

        public Player Player => _player;
        public IReadOnlyList<Enemy> Enemies => _enemies;
        public IReadOnlyList<Entity> Party => _party;
        public IReadOnlyList<Entity> Combatants => _allCombatants;

        /// <summary>
        /// This round's 8 turn slots in the order they act. Empty slots come last.
        /// </summary>
        public IReadOnlyList<TurnSlot> TurnOrder => _turnOrder;

        /// <summary>
        /// The save's difficulty (harder difficulties lower the party's hit chance). Set by
        /// GameManager from the game settings.
        /// </summary>
        public DifficultyLevel Difficulty
        {
            get => _damageCalculator.Difficulty;
            set => _damageCalculator.Difficulty = value;
        }

        public CombatManager(RNGManager rngManager)
        {
            _rngManager = rngManager;
            _turnManager = new TurnManager();
            _damageCalculator = new DamageCalculator(rngManager);
            _defendingEntities = new HashSet<Entity>();
            _fledEnemies = new HashSet<Enemy>();
            _directlyControlled = new HashSet<Entity>();
            _pendingRevivePenalties = new Dictionary<Entity, int>();
            _party = new List<Entity>();
            _allCombatants = new List<Entity>();
            _turnQueue = new Queue<Entity>();
            _turnOrder = new List<TurnSlot>();
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
        /// Messages that items, abilities and effects send through GameLog during the fight -
        /// passed on into the combat log so the combat screen sees them too.
        /// </summary>
        private void OnGameLogMessage(string message) => Log(message);

        private void Raise(CombatEvent combatEvent)
        {
            combatEvent.Round = _currentRound;
            CombatEventOccurred?.Invoke(combatEvent);
        }

        private void Raise(CombatEventType type, Entity actor = null, Entity target = null, int amount = 0, string message = null)
        {
            Raise(new CombatEvent { Type = type, Actor = actor, Target = target, Amount = amount, Message = message });
        }

        /// <summary>
        /// Start a combat encounter against one or more enemies. Runs computer-controlled
        /// turns automatically and pauses (IsWaitingForPlayerAction = true) whenever a
        /// player-controlled party member is up - call SubmitPlayerAction() to resume.
        /// Subscribe to CombatFinished for the outcome.
        /// </summary>
        /// <param name="player">The player character</param>
        /// <param name="enemies">The enemy or enemies to fight - built by the caller (e.g. the
        /// overworld spawn that was touched, or the boss being challenged)</param>
        /// <param name="companions">The party's companions (can be null or empty) - the first
        /// three that are in the party and standing fight</param>
        /// <param name="bossManager">Optional boss manager for boss encounters</param>
        public void StartCombat(Player player, List<Enemy> enemies, List<Entity> companions = null, BossManager bossManager = null)
        {
            _player = player;
            _enemies = enemies ?? new List<Enemy>(); // Fallback only - the caller passes the enemies in
            _bossManager = bossManager;
            _combatActive = true;
            _partyFled = false;

            // Listen to GameLog for the length of the fight (removed again in EndCombat)
            GameLog.MessageLogged -= OnGameLogMessage;
            GameLog.MessageLogged += OnGameLogMessage;
            LastResult = null;
            IsWaitingForPlayerAction = false;
            CurrentActor = null;

            foreach (var enemy in _enemies)
            {
                if (enemy is BossEnemy boss)
                {
                    // MVP (decided 2026-10-14): the old boss strength multipliers (+15% per boss
                    // beaten, +50% per repeat fight) are switched off - boss strength is to come
                    // from the party's level through the difficulty scaler (Bucket A #11).
                    // Kept for a possible update/DLC:
                    // _bossManager?.ApplyBossScaling(boss);

                    // A boss is the same object every attempt - start it at full health, and
                    // reset its mechanic so a new attempt doesn't carry over grapple/charge state.
                    boss.Health = boss.MaxHealth;
                    boss.Mechanic?.Reset();
                }
            }

            // The party: the player plus up to three companions who are in the party and standing
            _party.Clear();
            _party.Add(_player);

            if (companions != null)
            {
                foreach (var companion in companions)
                {
                    if (_party.Count >= MaxFightingCompanions + 1) break;

                    if (companion is NPCBase npc && npc.InParty && companion.IsAlive() && !_party.Contains(companion))
                    {
                        _party.Add(companion);
                    }
                }
            }

            _allCombatants.Clear();
            _allCombatants.AddRange(_party);
            _allCombatants.AddRange(_enemies);

            // Nobody carries a speed change or last action over from a previous fight
            foreach (var combatant in _allCombatants)
            {
                combatant.ResetSpeedModifier();
                combatant.LastAction = Entities.CombatAction.None;
            }

            // Clear tracking structures
            _defendingEntities.Clear();
            _fledEnemies.Clear();
            _pendingRevivePenalties.Clear();
            _turnQueue.Clear();
            _turnOrder = new List<TurnSlot>();
            _currentRound = 1;

            // Initialize turn manager (vestigial - kept for compatibility, not used to drive turns)
            _turnManager.InitializeCombat(player, _enemies.FirstOrDefault());

            Raise(CombatEventType.CombatStarted);

            if (CheckForCombatEnd()) return;

            BeginRound();
            AdvanceTurns();
        }

        /// <summary>
        /// Submit the chosen action for CurrentActor (built via CombatAction.Attack/UseAbility/
        /// UseItem/Defend/Flee) while IsWaitingForPlayerAction is true. The action is checked
        /// first (see ValidateAction) - a refused action changes nothing, raises ActionRejected
        /// and returns false, and combat keeps waiting. An accepted action is resolved, then
        /// computer-controlled turns run until the next player decision or the end of the fight.
        /// TODO-GODOT: call this from the combat scene when the player confirms an action.
        /// </summary>
        public bool SubmitPlayerAction(CombatAction action)
        {
            if (!_combatActive || !IsWaitingForPlayerAction)
            {
                RejectAction("Combat is not waiting for an action right now.");
                return false;
            }

            if (!ValidateAction(action, out string reason))
            {
                RejectAction(reason);
                return false;
            }

            var actor = CurrentActor;
            IsWaitingForPlayerAction = false;

            RunAction(action);

            if (FinishTurn(actor)) return true;

            AdvanceTurns();
            return true;
        }

        private void RejectAction(string reason)
        {
            Log(reason);
            Raise(CombatEventType.ActionRejected, CurrentActor, message: reason);
        }

        /// <summary>
        /// Check an action before it is used: it must be the actor's turn, the target must be
        /// valid for the action, an ability must be off cooldown and activatable, an item must
        /// be in the party bag, and fleeing is refused in boss fights. Fills in an obvious
        /// missing target (yourself, for a self-only ability or a potion). Returns false with
        /// the reason when the action can't be used.
        /// </summary>
        public bool ValidateAction(CombatAction action, out string reason)
        {
            reason = null;

            if (action == null || action.Actor == null)
            {
                reason = "No action was chosen.";
                return false;
            }

            if (action.Actor != CurrentActor)
            {
                reason = $"It is not {action.Actor.Name}'s turn.";
                return false;
            }

            switch (action.Type)
            {
                case ActionType.Attack:
                    if (!GetValidTargets(action.Actor, ActionType.Attack).Contains(action.Target))
                    {
                        reason = "Choose an enemy that is still in the fight to attack.";
                        return false;
                    }
                    return true;

                case ActionType.UseAbility:
                    var abilities = GetAbilities(action.Actor);
                    if (abilities.Count == 0)
                    {
                        reason = $"{action.Actor.Name} has no ability to use.";
                        return false;
                    }
                    if (action.Ability != null && !abilities.Contains(action.Ability))
                    {
                        reason = $"{action.Actor.Name} does not have {action.Ability.Name}.";
                        return false;
                    }
                    var ability = action.Ability ?? GetAbility(action.Actor) ?? abilities[0];
                    if (ability.IsPassive)
                    {
                        reason = $"{ability.Name} is a passive ability - it is always on and can't be used as an action.";
                        return false;
                    }
                    if (ability.IsOnCooldown())
                    {
                        reason = $"{ability.Name} is on cooldown ({ability.CurrentCooldown} more round{(ability.CurrentCooldown == 1 ? "" : "s")}).";
                        return false;
                    }

                    action.Ability = ability;
                    if (action.Target == null && ability.TargetType == AbilityTarget.Self)
                    {
                        action.Target = action.Actor;
                    }
                    if (action.Target == null && ability.TargetType == AbilityTarget.AllAllies)
                    {
                        action.Target = action.Actor;
                    }
                    if (!GetValidTargets(action.Actor, ActionType.UseAbility, ability: ability).Contains(action.Target))
                    {
                        reason = $"Choose a valid target for {ability.Name}.";
                        return false;
                    }
                    return true;

                case ActionType.UseItem:
                    if (action.Item is not Items.Consumable consumable)
                    {
                        reason = $"{action.Item?.Name ?? "That item"} can't be used in a fight.";
                        return false;
                    }
                    if (consumable.Quantity <= 0 || !_player.Inventory.Items.Contains(consumable))
                    {
                        reason = $"The party has no {consumable.Name} left.";
                        return false;
                    }

                    // Potions default to the user; a revive needs a chosen fallen party member
                    if (action.Target == null && consumable.Type != Items.ConsumableType.RevivePotion)
                    {
                        action.Target = action.Actor;
                    }
                    if (!GetValidTargets(action.Actor, ActionType.UseItem, consumable).Contains(action.Target))
                    {
                        reason = consumable.Type == Items.ConsumableType.RevivePotion
                            ? "Choose a fallen party member to revive."
                            : $"Choose a party member who is still standing to use {consumable.Name} on.";
                        return false;
                    }
                    return true;

                case ActionType.Flee:
                    if (IsBossFight())
                    {
                        reason = "There's no running from this fight!";
                        return false;
                    }
                    return true;

                case ActionType.Defend:
                    return true;

                default:
                    reason = "Choose an action.";
                    return false;
            }
        }

        // ------------------------------------------------------------------
        // What a combat screen can ask
        // ------------------------------------------------------------------

        /// <summary>
        /// Who the actor may target with this action - opponents for attacks, standing party
        /// members for potions and support abilities, fallen party members for revives.
        /// An empty list means the action can't be used right now (grey out the button).
        /// For abilities, pass the ability when the combatant has more than one (otherwise
        /// their main ability is used).
        /// </summary>
        public List<Entity> GetValidTargets(Entity actor, ActionType actionType, Items.Item item = null, Ability ability = null)
        {
            var targets = new List<Entity>();
            if (actor == null) return targets;

            switch (actionType)
            {
                case ActionType.Attack:
                    targets.AddRange(GetOpponents(actor));
                    break;

                case ActionType.UseAbility:
                    ability ??= GetAbility(actor);
                    if (ability == null) break;

                    switch (ability.TargetType)
                    {
                        case AbilityTarget.Self:
                            targets.Add(actor);
                            break;
                        case AbilityTarget.SingleAlly:
                        case AbilityTarget.Ally:
                        case AbilityTarget.AllAllies:
                            targets.AddRange(GetAllies(actor));
                            break;
                        default:
                            targets.AddRange(GetOpponents(actor));
                            break;
                    }
                    break;

                case ActionType.UseItem:
                    if (item is Items.Consumable consumable)
                    {
                        if (consumable.Type == Items.ConsumableType.RevivePotion)
                            targets.AddRange(GetFallenAllies(actor));
                        else
                            targets.AddRange(GetAllies(actor));
                    }
                    break;
            }

            return targets;
        }

        /// <summary>
        /// The ability this combatant can use in a fight (the player's chosen ability, a
        /// companion's unique ability, an enemy's special ability), or null.
        /// </summary>
        public Ability GetAbility(Entity combatant)
        {
            return combatant switch
            {
                Entities.Player.Player player => player.SelectedAbility,
                CompanionBase companion => companion.UniqueAbility,
                Enemy enemy => enemy.SpecialAbility,
                _ => null
            };
        }

        /// <summary>
        /// Every ability this combatant can use in a fight. Most have one (GetAbility); a
        /// companion can have more (Felicity learns a party heal at level 20).
        /// </summary>
        public List<Ability> GetAbilities(Entity combatant)
        {
            if (combatant is CompanionBase companion)
            {
                return companion.GetCombatAbilities();
            }

            var ability = GetAbility(combatant);
            return ability == null ? new List<Ability>() : new List<Ability> { ability };
        }

        /// <summary>
        /// True when the combatant's ability (their main one, or the one given) can be used
        /// as an action right now.
        /// </summary>
        public bool CanUseAbility(Entity combatant, Ability ability = null)
        {
            ability ??= GetAbility(combatant);
            return ability != null
                && GetAbilities(combatant).Contains(ability)
                && !ability.IsPassive
                && !ability.IsOnCooldown()
                && GetValidTargets(combatant, ActionType.UseAbility, ability: ability).Count > 0;
        }

        /// <summary>
        /// The consumables in the shared party bag that can be used in a fight.
        /// </summary>
        public List<Items.Consumable> GetUsableItems()
        {
            if (_player == null) return new List<Items.Consumable>();

            return _player.Inventory.Items
                .OfType<Items.Consumable>()
                .Where(item => item.Quantity > 0)
                .ToList();
        }

        public bool IsPartyMember(Entity combatant) => _party.Contains(combatant);

        /// <summary>
        /// True while the combatant is defending (until their next turn comes).
        /// </summary>
        public bool IsDefending(Entity combatant) => _defendingEntities.Contains(combatant);

        /// <summary>
        /// True when an enemy has fled the fight.
        /// </summary>
        public bool HasFled(Entity combatant) => combatant is Enemy enemy && _fledEnemies.Contains(enemy);

        /// <summary>
        /// True when the combatant is down or has fled.
        /// </summary>
        public bool IsOutOfFight(Entity combatant) => !combatant.IsAlive() || HasFled(combatant);

        /// <summary>
        /// True when a boss is among the enemies - the party can't flee (decided 2026-10-14).
        /// </summary>
        public bool IsBossFight() => _enemies.Any(e => e is BossEnemy);

        /// <summary>
        /// The party's chance (1-100) to escape right now: the lowest flee chance among the
        /// enemies still standing - the hardest enemy to get away from decides (decided
        /// 2026-10-14). 0 in a boss fight.
        /// </summary>
        public int GetPartyFleeChance()
        {
            if (IsBossFight()) return 0;

            var activeEnemies = GetActiveEnemies();
            return activeEnemies.Count == 0 ? 100 : activeEnemies.Min(e => e.FleeChance);
        }

        /// <summary>
        /// Switch a companion between direct control (the player chooses their actions, and
        /// combat waits for them like it does for the player) and computer control. Stays set
        /// between fights. Takes effect from the companion's next turn.
        /// TODO-GODOT: call this from the companion control toggle in the combat scene.
        /// </summary>
        public void SetDirectControl(Entity companion, bool playerControls)
        {
            if (companion is not CompanionBase) return;

            if (playerControls)
                _directlyControlled.Add(companion);
            else
                _directlyControlled.Remove(companion);
        }

        public bool IsDirectlyControlled(Entity companion) => _directlyControlled.Contains(companion);

        // ------------------------------------------------------------------
        // Rounds and turns
        // ------------------------------------------------------------------

        /// <summary>
        /// The speed an entity acts at this round. Worked out fresh at the start of every round:
        /// base speed, plus the change from their last action, plus Swift Tactics for companions,
        /// minus any slow that is still active (e.g. Skarn's grapple - gone once the effect ends).
        /// </summary>
        private int GetEntityEffectiveSpeed(Entity entity)
        {
            int speed;

            // Check if entity is a companion
            if (entity is CompanionBase companion)
            {
                // Use companion's version that applies Swift Tactics
                speed = companion.GetEffectiveSpeed(_player);
            }
            else
            {
                // For player and enemies, use base GetEffectiveSpeed()
                speed = entity.GetEffectiveSpeed();
            }

            // A multi-round slow (e.g. Skarn's grapple) is tracked as an ongoing effect
            // rather than the per-round SpeedModifier (which gets recalculated every
            // round based on LastAction and would otherwise overwrite it).
            var grapple = entity.GetEffect<Abilities.EnemyAbilities.GrappledEffect>();
            if (grapple != null)
            {
                speed -= grapple.Potency;
            }

            // Same for Boon's pinning bolt
            var pinned = entity.GetEffect<PinnedEffect>();
            if (pinned != null)
            {
                speed -= pinned.Potency;
            }

            return Math.Max(1, speed);
        }

        /// <summary>
        /// Begin a new round - build the 8 turn slots and fill the turn queue.
        /// </summary>
        private void BeginRound()
        {
            Log("\n" + "=".PadRight(50, '='));
            Log($"           ROUND {_currentRound}");
            Log("=".PadRight(50, '='));

            _turnOrder = BuildTurnOrder();

            // Display turn order for this round
            Log("\nTurn Order (by Speed):");
            foreach (var slot in _turnOrder)
            {
                if (slot.IsEmpty) continue;

                string names = string.Join(" + ", slot.Members.Select(m => m.Name));
                string downText = slot.Members.All(m => !m.IsAlive()) ? " - down" : "";
                Log($"  {names} (Speed: {slot.Speed}){downText}");
            }
            Log("");

            // Fill the turn queue slot by slot. Fallen party members are queued too: if they
            // are revived before their slot comes up, they act this round.
            _turnQueue.Clear();
            foreach (var slot in _turnOrder)
            {
                foreach (var member in slot.Members)
                {
                    _turnQueue.Enqueue(member);
                }
            }

            Raise(CombatEventType.RoundStarted);
        }

        /// <summary>
        /// Build this round's 8 turn slots (decided 2026-10-12):
        /// - The party has 4 slots, one per member (player + up to 3 companions).
        /// - The opponents have 4 slots, however many enemies there are. With more than 4,
        ///   enemies are sorted by speed and spread evenly; the fastest keep a slot to
        ///   themselves and the extras go to the slowest slots first (5 enemies: 1,1,1,2;
        ///   8: 2,2,2,2). A shared slot acts at its members' average speed, rounded up.
        /// - Slots act fastest first. Ties are broken at random (my default - not decided).
        /// - Unused slots are empty, placed last, and skipped.
        /// </summary>
        private List<TurnSlot> BuildTurnOrder()
        {
            var slots = new List<TurnSlot>();

            foreach (var member in _party)
            {
                slots.Add(new TurnSlot(true, new List<Entity> { member }, GetEntityEffectiveSpeed(member)));
            }

            var activeEnemies = GetActiveEnemies().Cast<Entity>().ToList();
            var enemyTieBreak = activeEnemies.ToDictionary(e => e, e => _rngManager.Roll(1, 1000000));
            var sortedEnemies = activeEnemies
                .OrderByDescending(e => GetEntityEffectiveSpeed(e))
                .ThenBy(e => enemyTieBreak[e])
                .ToList();

            int enemyCount = sortedEnemies.Count;
            int baseSize = enemyCount / SlotsPerSide;
            int extras = enemyCount % SlotsPerSide;
            int enemySlotCount = 0;
            int index = 0;

            for (int i = 0; i < SlotsPerSide && index < enemyCount; i++)
            {
                int size = enemyCount <= SlotsPerSide
                    ? 1
                    : baseSize + (i >= SlotsPerSide - extras ? 1 : 0);

                var members = sortedEnemies.GetRange(index, size);
                index += size;

                int speed = (int)Math.Ceiling(members.Average(m => (double)GetEntityEffectiveSpeed(m)));
                slots.Add(new TurnSlot(false, members, speed));
                enemySlotCount++;
            }

            var slotTieBreak = slots.ToDictionary(s => s, s => _rngManager.Roll(1, 1000000));
            var ordered = slots
                .OrderByDescending(s => s.Speed)
                .ThenBy(s => slotTieBreak[s])
                .ToList();

            for (int i = _party.Count; i < SlotsPerSide; i++)
            {
                ordered.Add(new TurnSlot(true, new List<Entity>(), 0));
            }
            for (int i = enemySlotCount; i < SlotsPerSide; i++)
            {
                ordered.Add(new TurnSlot(false, new List<Entity>(), 0));
            }

            return ordered;
        }

        /// <summary>
        /// Resolve turns until a player-controlled party member is up (returns and waits for
        /// SubmitPlayerAction()) or the fight ends. Starts the next round when this one is done.
        /// </summary>
        private void AdvanceTurns()
        {
            while (_combatActive)
            {
                if (_turnQueue.Count == 0)
                {
                    EndRound();
                    if (CheckForCombatEnd()) return;

                    _currentRound++;
                    BeginRound();
                    continue;
                }

                var entity = _turnQueue.Dequeue();

                // Defend lasts until the defender's next turn - it ends when that turn comes,
                // even if they can't act on it (decided 2026-10-12, skip rule 2026-10-14)
                EndDefend(entity);

                if (IsOutOfFight(entity))
                {
                    if (IsPartyMember(entity))
                    {
                        Log($"\n{entity.Name} is down and can't act.");
                        Raise(CombatEventType.TurnSkipped, entity);
                    }
                    continue;
                }

                CurrentActor = entity;
                Raise(CombatEventType.TurnStarted, entity);

                if (entity == _player || _directlyControlled.Contains(entity))
                {
                    Log($"\n{entity.Name}'s Turn!");
                    Log($"{entity.Name}'s HP: {entity.Health}/{entity.MaxHealth}");
                    Log($"Enemies: {string.Join(", ", GetActiveEnemies().Select(e => $"{e.Name} ({e.Health}/{e.MaxHealth})"))}");

                    IsWaitingForPlayerAction = true;
                    Raise(CombatEventType.WaitingForInput, entity);
                    return;
                }

                ExecuteEntityTurn(entity);

                if (FinishTurn(entity)) return;
            }
        }

        private void EndDefend(Entity entity)
        {
            if (_defendingEntities.Remove(entity))
            {
                Raise(CombatEventType.DefendEnded, entity);
            }
        }

        /// <summary>
        /// Apply an entity's end-of-turn status effects (poison/bleed/etc. tick here),
        /// then check whether combat ended. Returns true if it did (stop advancing turns).
        /// </summary>
        private bool FinishTurn(Entity entity)
        {
            CurrentActor = null;

            if (entity.IsAlive() && !HasFled(entity))
            {
                var before = TakeSnapshot();
                entity.ProcessEffects(_rngManager);
                ReportChanges(before);

                if (!entity.IsAlive())
                {
                    Log($"{entity.Name} succumbed to a status effect!");
                }
            }

            return CheckForCombatEnd();
        }

        /// <summary>
        /// End the fight if one side is out: every enemy down or fled (won / opponent fled),
        /// the whole party down (lost), or the party escaped. Returns true if the fight ended.
        /// The fight goes on while anyone in the party is standing (decided 2026-10-14).
        /// </summary>
        private bool CheckForCombatEnd()
        {
            if (!_combatActive) return true;

            if (_partyFled)
            {
                EndCombat(CombatOutcome.PartyFled);
                return true;
            }

            if (GetActiveEnemies().Count == 0)
            {
                bool anyDefeated = _enemies.Count == 0 || _enemies.Any(e => !_fledEnemies.Contains(e));
                EndCombat(anyDefeated ? CombatOutcome.Won : CombatOutcome.OpponentFled);
                return true;
            }

            if (_party.All(member => !member.IsAlive()))
            {
                EndCombat(CombatOutcome.Lost);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Execute a turn for a computer-controlled entity (an enemy, or a companion on auto)
        /// </summary>
        private void ExecuteEntityTurn(Entity entity)
        {
            Log($"\n{entity.Name}'s Turn!");

            CombatAction action = entity switch
            {
                Enemy enemy => DetermineEnemyAction(enemy),
                CompanionBase companion => DetermineCompanionAction(companion),
                _ => CombatAction.None(entity)
            };

            RunAction(action);
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
                if (IsOutOfFight(entity))
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

                // Display speed change
                if (modifier != 0)
                {
                    string changeText = modifier > 0 ? $"+{modifier}" : modifier.ToString();
                    string actionText = entity.LastAction.ToString();
                    Log($"{entity.Name}'s speed {changeText} from {actionText}");
                }

                // A revive this round slows them down next round, on top of the action change
                if (_pendingRevivePenalties.TryGetValue(entity, out int revivePenalty))
                {
                    modifier -= revivePenalty;
                    Log($"{entity.Name}'s speed -{revivePenalty} from being revived");
                }

                entity.SpeedModifier = modifier;
            }

            _pendingRevivePenalties.Clear();
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

            // Tick down effect durations (duration only - effects were already applied at
            // each entity's own end-of-turn this round)
            var before = TakeSnapshot();
            foreach (var entity in _allCombatants)
            {
                if (entity.IsAlive())
                {
                    entity.TickEffectDurations();
                }
            }

            // Check for Iron Will passive ability cleanse (after effects tick, before next round)
            if (_player.IsAlive() && _player.SelectedAbility is IronWillAbility ironWill)
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
            ReportChanges(before);

            // Reduce ability cooldowns for everyone at end of round
            foreach (var combatant in _allCombatants)
            {
                foreach (var ability in GetAbilities(combatant))
                {
                    if (ability.CurrentCooldown > 0)
                    {
                        ability.CurrentCooldown--;
                    }
                }
            }
        }

        // ------------------------------------------------------------------
        // Computer-controlled decisions
        // ------------------------------------------------------------------

        /// <summary>
        /// Decide a computer-controlled companion's action. Companions on auto never flee -
        /// they fight until the party wins or they go down (decided 2026-10-14). Only a
        /// companion under the player's direct control can choose Flee.
        /// </summary>
        private CombatAction DetermineCompanionAction(CompanionBase companion)
        {
            Log($"{companion.Name}'s HP: {companion.Health}/{companion.MaxHealth}");

            // Pick which enemy this companion engages (lowest-HP living enemy)
            Enemy target = SelectCompanionAttackTarget();
            if (target == null)
            {
                return CombatAction.None(companion);
            }

            // Use companion AI to decide action: 0 = Attack, 1 = Ability, 2 = Defend
            // Boon fires a special bolt when one in the party bag fits this target
            if (companion is CompanionBoon boon)
            {
                var bolt = boon.ChooseBolt(target, GetBoltCounts().Keys);
                if (bolt != null)
                {
                    var shot = CombatAction.UseAbility(boon, boon.SpecialShot, target);
                    shot.Item = new Items.Ammunition(bolt.Value);
                    return shot;
                }
            }

            var allies = GetAllies(companion);
            int actionDecision = companion.DecideCombatAction(target, allies, _rngManager);

            switch (actionDecision)
            {
                case 1: // Use Ability (falls back to attacking if it can't be used)
                    var ability = companion.ChooseCombatAbility(allies);
                    if (ability != null && !ability.IsPassive && !ability.IsOnCooldown())
                    {
                        return CombatAction.UseAbility(companion, ability, ResolveAbilityTarget(companion, ability, target));
                    }
                    return CombatAction.Attack(companion, target);

                case 2: // Defend
                    return CombatAction.Defend(companion);

                case 0: // Attack
                default:
                    return CombatAction.Attack(companion, target);
            }
        }

        /// <summary>
        /// Pick which enemy a companion should attack/target this turn - the lowest-HP
        /// living enemy (mirrors SelectEnemyTarget()'s "prioritize wounded targets" heuristic).
        /// </summary>
        private Enemy SelectCompanionAttackTarget()
        {
            return MostWounded(GetActiveEnemies().Cast<Entity>().ToList()) as Enemy;
        }

        /// <summary>
        /// Who an ability is aimed at: the user for self abilities, the most wounded ally for
        /// support abilities (e.g. Divine Light heals the party, not the enemy), otherwise the
        /// chosen opponent.
        /// </summary>
        private Entity ResolveAbilityTarget(Entity user, Ability ability, Entity opponentTarget)
        {
            switch (ability.TargetType)
            {
                case AbilityTarget.Self:
                    return user;
                case AbilityTarget.SingleAlly:
                case AbilityTarget.Ally:
                case AbilityTarget.AllAllies:
                    return MostWounded(GetAllies(user)) ?? user;
                default:
                    return opponentTarget;
            }
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

            // Select target - enemies can target the player or companions
            Entity target = SelectEnemyTarget();
            if (target == null)
            {
                return CombatAction.None(enemy);
            }

            // A boss's own unique mechanic (if any) gets first say - falls back to the
            // generic Behavior-based logic below if it returns null.
            if (enemy is BossEnemy boss && boss.Mechanic != null)
            {
                var mechanicAction = boss.Mechanic.DecideAction(boss, target, _rngManager, out string mechanicMessage);
                if (!string.IsNullOrEmpty(mechanicMessage))
                {
                    Log(mechanicMessage);
                }
                if (mechanicAction != null)
                {
                    return mechanicAction;
                }
            }

            // Decision logic based on behavior type
            switch (enemy.Behavior)
            {
                case EnemyBehavior.Aggressive:
                    // Always attack, use ability when available
                    if (canUseAbility && _rngManager.Roll(1, 100) <= 70)
                    {
                        return EnemyAbilityAction(enemy, target);
                    }
                    return CombatAction.Attack(enemy, target);

                case EnemyBehavior.Defensive:
                    // Defend when low on health
                    if (healthPercent < 0.3f && _rngManager.Roll(1, 100) <= 60)
                    {
                        return CombatAction.Defend(enemy);
                    }
                    if (canUseAbility && _rngManager.Roll(1, 100) <= 40)
                    {
                        return EnemyAbilityAction(enemy, target);
                    }
                    return CombatAction.Attack(enemy, target);

                case EnemyBehavior.Tactical:
                    // Use abilities strategically
                    if (canUseAbility && _rngManager.Roll(1, 100) <= 80)
                    {
                        return EnemyAbilityAction(enemy, target);
                    }
                    if (healthPercent < 0.25f && _rngManager.Roll(1, 100) <= 50)
                    {
                        return CombatAction.Defend(enemy);
                    }
                    return CombatAction.Attack(enemy, target);

                case EnemyBehavior.Berserker:
                    // Always attack, rarely defend, use abilities aggressively
                    if (canUseAbility && _rngManager.Roll(1, 100) <= 90)
                    {
                        return EnemyAbilityAction(enemy, target);
                    }
                    return CombatAction.Attack(enemy, target);

                case EnemyBehavior.Cautious:
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
                        return EnemyAbilityAction(enemy, target);
                    }
                    return CombatAction.Attack(enemy, target);

                case EnemyBehavior.Balanced:
                default:
                    // Mix of all actions
                    if (healthPercent < 0.3f && _rngManager.Roll(1, 100) <= 40)
                    {
                        return CombatAction.Defend(enemy);
                    }
                    if (canUseAbility && _rngManager.Roll(1, 100) <= 60)
                    {
                        return EnemyAbilityAction(enemy, target);
                    }
                    return CombatAction.Attack(enemy, target);
            }
        }

        private CombatAction EnemyAbilityAction(Enemy enemy, Entity target)
        {
            return CombatAction.UseAbility(enemy, enemy.SpecialAbility, ResolveAbilityTarget(enemy, enemy.SpecialAbility, target));
        }

        /// <summary>
        /// Select a target for an enemy - 70% the most wounded standing party member,
        /// 30% a random standing party member.
        /// </summary>
        private Entity SelectEnemyTarget()
        {
            var livingParty = GetLivingParty();

            if (livingParty.Count == 0) return null;
            if (livingParty.Count == 1) return livingParty[0];

            if (_rngManager.Roll(1, 100) <= 70)
            {
                return MostWounded(livingParty);
            }

            int randomIndex = _rngManager.Roll(0, livingParty.Count - 1);
            return livingParty[randomIndex];
        }

        // ------------------------------------------------------------------
        // Resolving actions
        // ------------------------------------------------------------------

        /// <summary>
        /// Resolve an action and report every change it caused (damage, healing, effects,
        /// defeats, revives) as combat events.
        /// </summary>
        private void RunAction(CombatAction action)
        {
            var before = TakeSnapshot();
            ProcessAction(action);
            ReportChanges(before);
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
                case ActionType.None:
                    // No offensive action this turn (e.g. a boss holding a grapple) -
                    // any narration was already logged by whatever decided this action.
                    action.Actor.LastAction = Entities.CombatAction.None;
                    break;
            }
        }

        /// <summary>
        /// Process an attack - the same rules for every attacker and target (DamageCalculator
        /// .CalculateAttack): hit roll, damage, crit, the target's defense, then defending and
        /// the Evasion ability.
        /// </summary>
        private void ProcessAttack(CombatAction action)
        {
            var attacker = action.Actor;
            var target = action.Target;

            // Track action for speed modifier calculation
            attacker.LastAction = Entities.CombatAction.Attack;

            if (target == null || IsOutOfFight(target))
            {
                Log($"\n{attacker.Name} has no one to attack!");
                return;
            }

            Log($"\n{attacker.Name} attacks {target.Name}!");

            if (attacker == _player && _player.EquippedWeapon == null)
            {
                Log("(Fighting unarmed - find a weapon!)");
            }

            DamageResult result = _damageCalculator.CalculateAttack(attacker, target, _player, action.IgnoresDefense);

            if (result.Missed)
            {
                Log("The attack missed!");
                Raise(CombatEventType.AttackMissed, attacker, target);
                return;
            }

            // Defending halves every hit until the defender's next turn
            bool isDefending = IsDefending(target);
            if (isDefending)
            {
                int damageReduction = result.FinalDamage / 2;
                result.FinalDamage -= damageReduction;
                Log($"{target.Name}'s defensive stance reduced damage by {damageReduction}!");
            }

            // Evasion (the player's passive ability) - only when not defending
            if (target == _player && !isDefending && _player.SelectedAbility is EvasionAbility evasion)
            {
                if (evasion.ShouldEvade(_rngManager, isDefending))
                {
                    Log($"{_player.Name} evaded the attack! No damage taken!");
                    Raise(CombatEventType.AttackEvaded, attacker, target);
                    return;
                }
            }

            // Boss mechanics (e.g. a charge/stoop attack) can boost the damage of this
            // specific hit. A flat multiplier commutes with the defend-reduction already
            // applied above, so applying it here (after) is equivalent to applying it before.
            if (action.DamageMultiplier != 1.0)
            {
                result.FinalDamage = (int)(result.FinalDamage * action.DamageMultiplier);
            }

            // Risk's armor: a chance a weapon or other non-magical hit lands but does no damage
            // (doubled while defending). Only the damage is stopped - a status effect the attack
            // carries still applies. Magic attacks go through as normal.
            if (target is CompanionRisk risk && !action.IsMagical && risk.RollNoDamage(_rngManager, isDefending))
            {
                Log($"The hit glances off {risk.Name}'s armor! No damage taken!");
                Raise(CombatEventType.AttackBlocked, attacker, target);

                if (action.AppliesEffect != null)
                {
                    target.AddEffect(action.AppliesEffect);
                }
                return;
            }

            if (result.IsCritical)
            {
                Log("CRITICAL HIT!");
            }

            if (result.DamageReduced > 0)
            {
                Log(target is Enemy
                    ? $"Defense blocked {result.DamageReduced} damage!"
                    : $"Armor blocked {result.DamageReduced} damage!");
            }

            Raise(new CombatEvent
            {
                Type = CombatEventType.AttackLanded,
                Actor = attacker,
                Target = target,
                Amount = result.FinalDamage,
                IsCritical = result.IsCritical
            });

            if (result.FinalDamage > 0)
            {
                target.TakeDamage(result.FinalDamage);
                Log($"{target.Name} took {result.FinalDamage} damage!");

                // Some attacks also inflict a status effect on a successful hit (e.g. a grapple's slow)
                if (action.AppliesEffect != null)
                {
                    target.AddEffect(action.AppliesEffect);
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

            if (action.Ability == null)
            {
                Log("Error: No ability provided!");
                return;
            }

            if (action.Ability is SpecialShotAbility specialShot)
            {
                ProcessSpecialShot(action, specialShot);
                return;
            }

            var target = action.Target ?? action.Actor;
            bool wholeParty = action.Ability.TargetType == AbilityTarget.AllAllies;
            Log(wholeParty
                ? $"\n{action.Actor.Name} uses {action.Ability.Name} on the whole party!"
                : target == action.Actor
                    ? $"\n{action.Actor.Name} uses {action.Ability.Name}!"
                    : $"\n{action.Actor.Name} uses {action.Ability.Name} on {target.Name}!");

            Raise(new CombatEvent
            {
                Type = CombatEventType.AbilityUsed,
                Actor = action.Actor,
                Target = target,
                Ability = action.Ability
            });

            if (wholeParty)
            {
                action.Ability.ExecuteOnGroup(action.Actor, GetAllies(action.Actor), _rngManager);
            }
            else
            {
                action.Ability.Execute(action.Actor, target, _rngManager);
            }
        }

        /// <summary>
        /// Boon's Special Shot: spend one bolt of the chosen type (action.Item) from the party
        /// bag and fire it as a normal crossbow attack with the bolt's effect added. If that
        /// type has run out, he throws an axe instead (a weaker normal attack).
        /// </summary>
        private void ProcessSpecialShot(CombatAction action, SpecialShotAbility shot)
        {
            var shooter = action.Actor;
            var target = action.Target;
            var boltType = (action.Item as Items.Ammunition)?.Type;

            Raise(new CombatEvent
            {
                Type = CombatEventType.AbilityUsed,
                Actor = shooter,
                Target = target,
                Ability = shot
            });

            var stack = boltType == null ? null : _player?.Inventory.Items
                .OfType<Items.Ammunition>()
                .FirstOrDefault(ammo => ammo.Type == boltType.Value && ammo.Quantity > 0);

            var attack = new CombatAction(ActionType.Attack, shooter, target);

            if (stack == null)
            {
                string boltName = boltType == null ? "special bolts" : $"{Items.Ammunition.GetName(boltType.Value)}s";
                Log($"\n{shooter.Name} is out of {boltName} - he throws an axe instead!");
                attack.DamageMultiplier = SpecialShotAbility.AxeDamageShare;
                ProcessAttack(attack);
                shooter.LastAction = Entities.CombatAction.UseAbility;
                return;
            }

            _player.Inventory.RemoveQuantity(stack, 1);
            int left = stack.Quantity;
            Log($"\n{shooter.Name} loads a {stack.Name}! ({left} left)");

            switch (stack.Type)
            {
                case Items.BoltType.Piercing:
                    attack.IgnoresDefense = true;
                    break;
                case Items.BoltType.Pinning:
                    attack.AppliesEffect = new PinnedEffect(SpecialShotAbility.PinDuration, SpecialShotAbility.PinSpeedReduction);
                    break;
                case Items.BoltType.Fire:
                    attack.AppliesEffect = new BurnEffect(shot.BurnPotency, SpecialShotAbility.BurnDuration);
                    break;
                // Ice and Acid: effects not designed yet - they hit like an ordinary bolt
            }

            ProcessAttack(attack);
            shooter.LastAction = Entities.CombatAction.UseAbility;
            shot.GainCombatExperience();
        }

        /// <summary>
        /// Special bolts in the party bag, by type (only types with at least one left).
        /// A combat screen uses this to offer Boon's bolt choices.
        /// </summary>
        public Dictionary<Items.BoltType, int> GetBoltCounts()
        {
            var counts = new Dictionary<Items.BoltType, int>();
            if (_player == null) return counts;

            foreach (var ammo in _player.Inventory.Items.OfType<Items.Ammunition>())
            {
                if (ammo.Quantity <= 0) continue;
                counts.TryGetValue(ammo.Type, out int count);
                counts[ammo.Type] = count + ammo.Quantity;
            }
            return counts;
        }

        /// <summary>
        /// Process an item action - items come from the shared party bag and can be used by
        /// any party member on any valid target in the party.
        /// </summary>
        private void ProcessItem(CombatAction action)
        {
            // Track action for speed modifier calculation
            action.Actor.LastAction = Entities.CombatAction.UseItem;

            if (action.Item is not Items.Consumable consumable)
            {
                Log(action.Item == null ? "Error: No item provided!" : $"{action.Item.Name} can't be used in a fight.");
                return;
            }

            var target = action.Target ?? action.Actor;

            Raise(new CombatEvent
            {
                Type = CombatEventType.ItemUsed,
                Actor = action.Actor,
                Target = target,
                Item = consumable
            });

            // The item writes its own "used ... on ..." and result lines (through GameLog)
            if (consumable.Type == Items.ConsumableType.RevivePotion)
            {
                if (consumable.UseRevivePotion(action.Actor, target))
                {
                    // Revival penalty: revived entities are disoriented and slower next round.
                    // Stored until EndRound, which resets SpeedModifier before working out the
                    // next round's speeds. Whatever they did before falling no longer counts.
                    int speedPenalty = _rngManager.Roll(2, 4);
                    _pendingRevivePenalties[target] = speedPenalty;
                    target.LastAction = Entities.CombatAction.None;
                    Log($"{target.Name} is disoriented from revival! Speed -{speedPenalty} next round.");
                }
            }
            else
            {
                consumable.UseOn(target, action.Actor);
            }

            // Remove from the party bag if the stack is used up
            if (consumable.Quantity <= 0)
            {
                _player.Inventory.RemoveItem(consumable);
            }
        }

        /// <summary>
        /// Process a defend action - halves every hit on the defender until their next turn
        /// </summary>
        private void ProcessDefend(CombatAction action)
        {
            Log($"\n{action.Actor.Name} takes a defensive stance!");

            // Track action for speed modifier calculation
            action.Actor.LastAction = Entities.CombatAction.Defend;

            _defendingEntities.Add(action.Actor);
            Raise(CombatEventType.DefendStarted, action.Actor);

            Log(IsPartyMember(action.Actor)
                ? "Incoming attacks deal half damage until their next turn!"
                : $"{action.Actor.Name} braces for impact!");
        }

        /// <summary>
        /// Process a flee action (decided 2026-10-14). Every enemy type has one flee chance:
        /// - An enemy fleeing uses its own flee chance and leaves the fight on its own.
        /// - A party member fleeing takes the whole party; the chance is the lowest flee chance
        ///   among the enemies still standing (the hardest one to get away from decides).
        /// The party can't flee from boss fights (refused in ValidateAction).
        /// </summary>
        private void ProcessFlee(CombatAction action)
        {
            var actor = action.Actor;
            actor.LastAction = Entities.CombatAction.None;

            if (actor is Enemy enemy)
            {
                if (_rngManager.Roll(1, 100) <= enemy.FleeChance)
                {
                    _fledEnemies.Add(enemy);
                    _defendingEntities.Remove(enemy);
                    Log($"\n{enemy.Name} fled from the fight!");
                    Raise(CombatEventType.FleeSucceeded, enemy);
                }
                else
                {
                    Log($"\n{enemy.Name} tried to flee but couldn't get away!");
                    Raise(CombatEventType.FleeFailed, enemy);
                }
                return;
            }

            if (_rngManager.Roll(1, 100) <= GetPartyFleeChance())
            {
                Log($"\n{actor.Name} leads the party away - you escaped!");
                _partyFled = true;
                Raise(CombatEventType.FleeSucceeded, actor);
            }
            else
            {
                Log($"\n{actor.Name} failed to escape!");
                Log("Turn wasted!");
                Raise(CombatEventType.FleeFailed, actor);
            }
        }

        // ------------------------------------------------------------------
        // Reporting changes as events
        // ------------------------------------------------------------------

        private class EntitySnapshot
        {
            public int Health;
            public bool Alive;
            public List<AbilityEffect> Effects;
        }

        private Dictionary<Entity, EntitySnapshot> TakeSnapshot()
        {
            var snapshot = new Dictionary<Entity, EntitySnapshot>();
            foreach (var entity in _allCombatants)
            {
                snapshot[entity] = new EntitySnapshot
                {
                    Health = entity.Health,
                    Alive = entity.IsAlive(),
                    Effects = new List<AbilityEffect>(entity.ActiveEffects)
                };
            }
            return snapshot;
        }

        /// <summary>
        /// Compare everyone against a snapshot and raise an event for each change - this
        /// catches damage, healing and effects from any source (attacks, abilities, items,
        /// poison ticks) without each of those needing to report itself.
        /// </summary>
        private void ReportChanges(Dictionary<Entity, EntitySnapshot> before)
        {
            foreach (var pair in before)
            {
                var entity = pair.Key;
                var old = pair.Value;

                if (!old.Alive && entity.IsAlive())
                {
                    Raise(CombatEventType.Revived, target: entity, amount: entity.Health);
                }
                else if (entity.Health < old.Health)
                {
                    Raise(CombatEventType.Damaged, target: entity, amount: old.Health - entity.Health);
                }
                else if (entity.Health > old.Health)
                {
                    Raise(CombatEventType.Healed, target: entity, amount: entity.Health - old.Health);
                }

                foreach (var effect in entity.ActiveEffects)
                {
                    if (!old.Effects.Contains(effect))
                    {
                        Raise(new CombatEvent { Type = CombatEventType.EffectApplied, Target = entity, EffectName = effect.Name });
                    }
                }
                foreach (var effect in old.Effects)
                {
                    if (!entity.ActiveEffects.Contains(effect))
                    {
                        Raise(new CombatEvent { Type = CombatEventType.EffectRemoved, Target = entity, EffectName = effect.Name });
                    }
                }

                if (old.Alive && !entity.IsAlive())
                {
                    _defendingEntities.Remove(entity);
                    Log($"{entity.Name} is down!");
                    Raise(CombatEventType.Defeated, target: entity);
                }
            }
        }

        // ------------------------------------------------------------------
        // Who is where
        // ------------------------------------------------------------------

        private List<Enemy> GetActiveEnemies()
        {
            return _enemies.Where(e => e.IsAlive() && !_fledEnemies.Contains(e)).ToList();
        }

        private List<Entity> GetLivingParty()
        {
            return _party.Where(member => member.IsAlive()).ToList();
        }

        private List<Entity> GetOpponents(Entity actor)
        {
            return IsPartyMember(actor)
                ? GetActiveEnemies().Cast<Entity>().ToList()
                : GetLivingParty();
        }

        private List<Entity> GetAllies(Entity actor)
        {
            return IsPartyMember(actor)
                ? GetLivingParty()
                : GetActiveEnemies().Cast<Entity>().ToList();
        }

        private List<Entity> GetFallenAllies(Entity actor)
        {
            return IsPartyMember(actor)
                ? _party.Where(member => !member.IsAlive()).ToList()
                : new List<Entity>();
        }

        /// <summary>
        /// The combatant with the lowest health percentage (the first one on a tie), or null.
        /// </summary>
        private static Entity MostWounded(List<Entity> candidates)
        {
            Entity mostWounded = null;
            float lowestHealthPercent = float.MaxValue;

            foreach (var candidate in candidates)
            {
                float healthPercent = (float)candidate.Health / candidate.MaxHealth;
                if (healthPercent < lowestHealthPercent)
                {
                    lowestHealthPercent = healthPercent;
                    mostWounded = candidate;
                }
            }

            return mostWounded;
        }

        // ------------------------------------------------------------------
        // End of the fight
        // ------------------------------------------------------------------

        /// <summary>
        /// Finish the fight: hand out rewards for the outcome, then fire CombatFinished and
        /// CombatEnded (once each).
        /// </summary>
        private void EndCombat(CombatOutcome outcome)
        {
            _combatActive = false;
            IsWaitingForPlayerAction = false;
            CurrentActor = null;
            _turnQueue.Clear();
            _defendingEntities.Clear();

            var result = new CombatResult
            {
                Outcome = outcome,
                RoundsFought = _currentRound
            };

            switch (outcome)
            {
                case CombatOutcome.Won:
                case CombatOutcome.OpponentFled:
                    HandleVictory(result);
                    break;
                case CombatOutcome.Lost:
                    HandleDefeat();
                    break;
                case CombatOutcome.PartyFled:
                    HandlePartyFled();
                    break;
            }

            ResetAbilityCombatUsage();
            LastResult = result;

            // Rewards are logged - stop passing GameLog messages into the combat log
            GameLog.MessageLogged -= OnGameLogMessage;

            Raise(new CombatEvent { Type = CombatEventType.CombatEnded, Message = outcome.ToString() });
            CombatFinished?.Invoke(result);
            CombatEnded?.Invoke(outcome == CombatOutcome.Won || outcome == CombatOutcome.OpponentFled);
        }

        /// <summary>
        /// Rewards for a won fight or one the opponents fled from: full XP and loot for each
        /// defeated enemy, 50% for each enemy that fled (decided 2026-10-12, per-enemy 2026-10-14).
        /// </summary>
        private void HandleVictory(CombatResult result)
        {
            Log("\n" + "=".PadRight(50, '='));
            Log(result.Outcome == CombatOutcome.Won ? "           VICTORY!" : "           THE ENEMY FLED!");
            Log("=".PadRight(50, '='));

            int totalXP = 0;
            int totalGold = 0;
            var allLootItems = new List<Items.Item>();
            var lootGenerator = new Progression.LootGenerator(new RNGManagerRandomAdapter(_rngManager));

            foreach (var enemy in _enemies)
            {
                bool fled = _fledEnemies.Contains(enemy);
                if (!fled && enemy.IsAlive()) continue; // Still standing - can't happen once the fight is won

                int xp = CalculateXPReward(enemy);

                // Roll for loot (gold + items) from the unified loot table system
                var lootTable = Progression.LootTableTemplates.CreateForEnemyType(enemy.Type);
                var loot = lootGenerator.GenerateLoot(lootTable, _player.Level, areaLevel: enemy.Level);

                if (fled)
                {
                    // Half the XP and gold; each item has a 50% chance of being dropped while fleeing
                    Log($"\n{enemy.Name} got away - half rewards.");
                    totalXP += xp / 2;
                    totalGold += loot.Gold / 2;
                    foreach (var item in loot.Items)
                    {
                        if (_rngManager.Roll(1, 100) <= 50)
                        {
                            allLootItems.Add(item);
                        }
                    }
                    result.FledEnemies.Add(enemy);
                }
                else
                {
                    Log($"\n{enemy.Name} has been defeated!");
                    totalXP += xp;
                    totalGold += loot.Gold;
                    allLootItems.AddRange(loot.Items);
                    result.DefeatedEnemies.Add(enemy);

                    // Handle boss defeats
                    if (enemy is BossEnemy defeatedBoss && _bossManager != null)
                    {
                        _bossManager.DefeatBoss(defeatedBoss.BossId);
                    }
                }
            }

            // The party won while the player was down - the player gets back up with 1 HP
            // (my default, not decided - see the TODO)
            if (!_player.IsAlive())
            {
                _player.Health = 1;
                Log($"\n{_player.Name} gets back on their feet.");
            }

            int playerLevelBefore = _player.Level;
            Log($"\nYou gained {totalXP} XP!");
            _player.AddExperience(totalXP);
            result.PlayerLevelsGained = _player.Level - playerLevelBefore;

            // Share XP with companions who are still standing
            foreach (var member in _party)
            {
                if (member is CompanionBase companion && companion.IsAlive())
                {
                    int companionLevelBefore = companion.Level;
                    companion.GainExperience(totalXP);
                    if (companion.Level > companionLevelBefore)
                    {
                        result.CompanionLevelsGained[companion] = companion.Level - companionLevelBefore;
                    }
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

            result.ExperienceGained = totalXP;
            result.GoldGained = totalGold;
            result.ItemsGained.AddRange(allLootItems);

            // Notify all shops that a combat encounter occurred (for restock tracking)
            Entities.NPCs.ShopKeeper.NotifyAllShopsOfCombat();
        }

        /// <summary>
        /// The whole party is down
        /// </summary>
        private void HandleDefeat()
        {
            Log("\n" + "=".PadRight(50, '='));
            Log("           DEFEAT");
            Log("=".PadRight(50, '='));

            Log($"\nThe whole party has fallen...");
            Log("\nGAME OVER");
        }

        /// <summary>
        /// The party escaped - no rewards (decided 2026-10-14)
        /// </summary>
        private void HandlePartyFled()
        {
            Log("You escaped, but gained no rewards...");
            Log($"Remaining HP: {_player.Health}/{_player.MaxHealth}");

            // Notify all shops about flee (may apply penalties for consecutive flees)
            Entities.NPCs.ShopKeeper.NotifyAllShopsOfFlee();
        }

        /// <summary>
        /// Reset every party member's per-fight ability use count (ability XP scales with
        /// uses within one fight)
        /// </summary>
        private void ResetAbilityCombatUsage()
        {
            foreach (var member in _party)
            {
                foreach (var ability in GetAbilities(member))
                {
                    ability.ResetCombatUsage();
                }
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
