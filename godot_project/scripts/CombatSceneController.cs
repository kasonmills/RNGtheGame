using System.Collections.Generic;
using Godot;
using GameLogic.Combat;
using GameLogic.Entities;
using GameLogic.Entities.Enemies;
using GameLogic.Entities.Enemies.EnemyTypes;
using GameLogic.Entities.Player;
using GameLogic.Items;
using GameLogic.Systems;

namespace RNGtheGame.Godot.Scripts
{
    // First real combat scene, driven by the actual (non-blocking, event-driven)
    // CombatManager -- no bypassing it this time. See CombatManager's own
    // TODO-GODOT comments for the integration contract this follows.
    public partial class CombatSceneController : Control
    {
        private static readonly PackedScene EntitySlotScene =
            GD.Load<PackedScene>("res://scenes/entity_slot.tscn");

        private HFlowContainer _partyRow = null!;
        private HFlowContainer _enemyRow = null!;
        private RichTextLabel _log = null!;
        private Button _attackButton = null!;
        private Button _defendButton = null!;
        private Button _fleeButton = null!;

        private RNGManager _rng = null!;
        private CombatManager _combatManager = null!;
        private readonly Dictionary<Entity, EntitySlot> _slots = new();

        private EnemyBase? _selectedTarget;

        public override void _Ready()
        {
            _partyRow = GetNode<HFlowContainer>("%PartyRow");
            _enemyRow = GetNode<HFlowContainer>("%EnemyRow");
            _log = GetNode<RichTextLabel>("%CombatLog");
            _attackButton = GetNode<Button>("%AttackButton");
            _defendButton = GetNode<Button>("%DefendButton");
            _fleeButton = GetNode<Button>("%FleeButton");

            _attackButton.Pressed += OnAttackPressed;
            _defendButton.Pressed += OnDefendPressed;
            _fleeButton.Pressed += OnFleePressed;

            StartNewFight();
        }

        private void StartNewFight()
        {
            _rng = new RNGManager();
            _slots.Clear();
            foreach (Node child in _partyRow.GetChildren()) child.QueueFree();
            foreach (Node child in _enemyRow.GetChildren()) child.QueueFree();
            _selectedTarget = null;
            _log.Clear();

            var player = new Player("Hero", PlayerClass.Warrior);
            player.EquipWeapon(ItemDatabase.GetWeapon("Rusty Sword", 1));
            AddSlot(player, _partyRow, new Color(0.3f, 0.5f, 0.9f));

            var enemies = new List<EnemyBase> { new Goblin(1), new Goblin(1) };
            foreach (var enemy in enemies)
            {
                var slot = AddSlot(enemy, _enemyRow, new Color(0.85f, 0.3f, 0.3f));
                slot.Selected += OnEnemySlotSelected;
            }

            _combatManager = new CombatManager(_rng);
            _combatManager.CombatMessage += AppendLog;
            _combatManager.CombatEnded += OnCombatEnded;

            SetActionButtonsEnabled(true);
            _combatManager.StartCombat(player, enemies);
            RefreshAllSlots();
            UpdateActionAvailability();
        }

        private EntitySlot AddSlot(Entity entity, HFlowContainer row, Color color)
        {
            var slot = EntitySlotScene.Instantiate<EntitySlot>();
            slot.HitboxColor = color;
            row.AddChild(slot);
            slot.Bind(entity);
            _slots[entity] = slot;
            return slot;
        }

        private void OnEnemySlotSelected(EntitySlot slot)
        {
            if (slot.BoundEntity is not EnemyBase enemy || !enemy.IsAlive())
                return;

            foreach (var enemy2 in _combatManager.Enemies)
            {
                if (_slots.TryGetValue(enemy2, out var enemySlot))
                    enemySlot.SetSelected(enemy2 == enemy);
            }

            _selectedTarget = enemy;
        }

        private EnemyBase? ResolveTarget()
        {
            if (_selectedTarget != null && _selectedTarget.IsAlive())
                return _selectedTarget;

            foreach (var enemy in _combatManager.Enemies)
            {
                if (enemy.IsAlive())
                    return enemy;
            }
            return null;
        }

        private void OnAttackPressed()
        {
            var target = ResolveTarget();
            if (target == null) return;
            Submit(GameLogic.Combat.CombatAction.Attack(_combatManager.Player, target));
        }

        private void OnDefendPressed()
        {
            Submit(GameLogic.Combat.CombatAction.Defend(_combatManager.Player));
        }

        private void OnFleePressed()
        {
            Submit(GameLogic.Combat.CombatAction.Flee(_combatManager.Player));
        }

        private void Submit(GameLogic.Combat.CombatAction action)
        {
            _combatManager.SubmitPlayerAction(action);
            RefreshAllSlots();
            UpdateActionAvailability();
        }

        private void UpdateActionAvailability()
        {
            SetActionButtonsEnabled(_combatManager.IsWaitingForPlayerAction);
        }

        private void SetActionButtonsEnabled(bool enabled)
        {
            _attackButton.Disabled = !enabled;
            _defendButton.Disabled = !enabled;
            _fleeButton.Disabled = !enabled;
        }

        private void OnCombatEnded(bool victory)
        {
            AppendLog(victory
                ? "[color=lime][b]Victory![/b][/color]"
                : "[color=red][b]Defeat...[/b][/color]");
            SetActionButtonsEnabled(false);
        }

        private void RefreshAllSlots()
        {
            foreach (var entity in _combatManager.Combatants)
            {
                if (_slots.TryGetValue(entity, out var slot))
                    slot.Refresh();
            }
        }

        private void AppendLog(string bbcodeLine)
        {
            _log.AppendText(bbcodeLine + "\n");
        }
    }
}
