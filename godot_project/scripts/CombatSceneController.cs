using System.Collections.Generic;
using Godot;
using GameLogic.Combat;
using GameLogic.Entities.Enemies;
using GameLogic.Entities.Enemies.EnemyTypes;
using GameLogic.Entities.Player;
using GameLogic.Items;
using GameLogic.Systems;

namespace RNGtheGame.Godot.Scripts
{
    // Sandbox for the visual combat layout: an arbitrary-size party row and enemy row
    // (parties can be 1-8 entities, enemy counts vary by encounter), each entity a
    // reusable EntitySlot (mini HP bar + hitbox + hover/select detail popup).
    //
    // Deliberately bypasses CombatManager, which drives its loop with
    // Console.ReadLine()/ReadKey() and can't be driven by UI button presses without a
    // rework -- see godot_split_todo memory. "Attacker" here is just "first alive party
    // member" and enemies retaliate against a random alive party member; there's no real
    // speed-based turn order yet -- that still lives behind the CombatManager refactor.
    public partial class CombatSceneController : Control
    {
        private static readonly PackedScene EntitySlotScene =
            GD.Load<PackedScene>("res://scenes/entity_slot.tscn");

        private HFlowContainer _partyRow = null!;
        private HFlowContainer _enemyRow = null!;
        private RichTextLabel _log = null!;
        private Button _attackButton = null!;
        private Button _resetButton = null!;

        private RNGManager _rng = new();
        private DamageCalculator _damageCalculator = null!;

        private readonly List<Player> _party = new();
        private readonly List<EnemyBase> _enemies = new();
        private readonly Dictionary<EnemyBase, EntitySlot> _enemySlots = new();
        private readonly Dictionary<Player, EntitySlot> _partySlots = new();

        private EnemyBase? _selectedTarget;

        public override void _Ready()
        {
            _partyRow = GetNode<HFlowContainer>("%PartyRow");
            _enemyRow = GetNode<HFlowContainer>("%EnemyRow");
            _log = GetNode<RichTextLabel>("%CombatLog");
            _attackButton = GetNode<Button>("%AttackButton");
            _resetButton = GetNode<Button>("%ResetButton");

            _attackButton.Pressed += OnAttackPressed;
            _resetButton.Pressed += StartNewFight;

            StartNewFight();
        }

        private void StartNewFight()
        {
            _rng = new RNGManager();
            _damageCalculator = new DamageCalculator(_rng);
            _selectedTarget = null;

            ClearRow(_partyRow, _partySlots);
            ClearRow(_enemyRow, _enemySlots);
            _party.Clear();
            _enemies.Clear();

            // Placeholder roster -- 3 vs 3 here just to prove the layout scales past 1v1.
            // Real rosters (1-8 party members, variable enemy counts) will come from
            // whatever assembles the encounter once that system exists.
            string[] allyNames = { "Hero", "Ally 1", "Ally 2" };
            foreach (string name in allyNames)
            {
                var member = new Player(name, PlayerClass.Warrior);
                member.EquipWeapon(ItemDatabase.GetWeapon("Rusty Sword", 1));
                _party.Add(member);

                var slot = EntitySlotScene.Instantiate<EntitySlot>();
                slot.HitboxColor = new Color(0.3f, 0.5f, 0.9f);
                _partyRow.AddChild(slot);
                slot.Bind(member);
                _partySlots[member] = slot;
            }

            for (int i = 1; i <= 3; i++)
            {
                var goblin = new Goblin(i);
                _enemies.Add(goblin);

                var slot = EntitySlotScene.Instantiate<EntitySlot>();
                slot.HitboxColor = new Color(0.85f, 0.3f, 0.3f);
                slot.Selected += OnEnemySlotSelected;
                _enemyRow.AddChild(slot);
                slot.Bind(goblin);
                _enemySlots[goblin] = slot;
            }

            _log.Clear();
            AppendLog($"Your party of {_party.Count} meets {_enemies.Count} goblins! Click one to target it, then Attack.");

            _attackButton.Disabled = false;
        }

        private void ClearRow(HFlowContainer row, Dictionary<Player, EntitySlot> slots)
        {
            foreach (Node child in row.GetChildren())
                child.QueueFree();
            slots.Clear();
        }

        private void ClearRow(HFlowContainer row, Dictionary<EnemyBase, EntitySlot> slots)
        {
            foreach (Node child in row.GetChildren())
                child.QueueFree();
            slots.Clear();
        }

        private void OnEnemySlotSelected(EntitySlot slot)
        {
            if (slot.BoundEntity is not EnemyBase enemy || !enemy.IsAlive())
                return;

            foreach (var kvp in _enemySlots)
                kvp.Value.SetSelected(kvp.Key == enemy);

            _selectedTarget = enemy;
        }

        private void OnAttackPressed()
        {
            Player? attacker = _party.Find(p => p.IsAlive());
            if (attacker == null)
                return;

            EnemyBase? target = (_selectedTarget != null && _selectedTarget.IsAlive())
                ? _selectedTarget
                : _enemies.Find(e => e.IsAlive());

            if (target == null)
                return;

            DamageResult playerResult = _damageCalculator.CalculatePlayerAttackDamage(attacker, target);
            ReportAttack(attacker.Name, target.Name, playerResult);
            if (playerResult.FinalDamage > 0)
                target.TakeDamage(playerResult.FinalDamage);
            _enemySlots[target].Refresh();

            if (!target.IsAlive())
            {
                AppendLog($"[color=lime]{target.Name} defeated![/color]");
                if (target == _selectedTarget)
                    _selectedTarget = null;
            }

            if (_enemies.TrueForAll(e => !e.IsAlive()))
            {
                AppendLog("[color=lime][b]Victory! All enemies defeated.[/b][/color]");
                _attackButton.Disabled = true;
                return;
            }

            RunEnemyPhase();
        }

        private void RunEnemyPhase()
        {
            var aliveParty = _party.FindAll(p => p.IsAlive());
            if (aliveParty.Count == 0)
                return;

            foreach (var enemy in _enemies)
            {
                if (!enemy.IsAlive())
                    continue;

                var survivors = _party.FindAll(p => p.IsAlive());
                if (survivors.Count == 0)
                    break;

                Player victim = survivors[_rng.Roll(0, survivors.Count - 1)];

                DamageResult enemyResult = _damageCalculator.CalculateEnemyAttackDamage(enemy, victim);
                ReportAttack(enemy.Name, victim.Name, enemyResult);
                if (enemyResult.FinalDamage > 0)
                    victim.TakeDamage(enemyResult.FinalDamage);
                _partySlots[victim].Refresh();

                if (!victim.IsAlive())
                    AppendLog($"[color=red]{victim.Name} was defeated![/color]");
            }

            if (_party.TrueForAll(p => !p.IsAlive()))
            {
                AppendLog("[color=red][b]Defeat... the whole party has fallen.[/b][/color]");
                _attackButton.Disabled = true;
            }
        }

        private void ReportAttack(string attacker, string target, DamageResult result)
        {
            if (result.Missed)
            {
                AppendLog($"{attacker} attacks {target} and misses!");
                return;
            }

            string crit = result.IsCritical ? " [color=yellow]CRITICAL![/color]" : "";
            AppendLog($"{attacker} hits {target} for {result.FinalDamage} damage (raw {result.RawDamage}, blocked {result.DamageReduced}).{crit}");
        }

        private void AppendLog(string bbcodeLine)
        {
            _log.AppendText(bbcodeLine + "\n");
        }
    }
}
