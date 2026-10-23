using System.Collections.Generic;
using System.Linq;
using Godot;
using GameLogic.Combat;
using GameLogic.Entities;
using GameLogic.Entities.Enemies;
using GameLogic.Entities.Enemies.EnemyTypes;
using GameLogic.Entities.NPCs.Companions;
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

        private GridContainer _partyRow = null!;
        private HFlowContainer _enemyRow = null!;
        private RichTextLabel _log = null!;
        private Button _attackButton = null!;
        private Button _defendButton = null!;
        private Button _fleeButton = null!;
        private Button _resetButton = null!;

        private RNGManager _rng = null!;
        private CombatManager _combatManager = null!;
        private readonly Dictionary<Entity, EntitySlot> _slots = new();
        private bool _lastLogLineBlank = true;

        private EnemyBase? _selectedTarget;

        public override void _Ready()
        {
            _partyRow = GetNode<GridContainer>("%PartyRow");
            _enemyRow = GetNode<HFlowContainer>("%EnemyRow");
            _log = GetNode<RichTextLabel>("%CombatLog");
            _attackButton = GetNode<Button>("%AttackButton");
            _defendButton = GetNode<Button>("%DefendButton");
            _fleeButton = GetNode<Button>("%FleeButton");
            _resetButton = GetNode<Button>("%ResetButton");

            _attackButton.Pressed += OnAttackPressed;
            _defendButton.Pressed += OnDefendPressed;
            _fleeButton.Pressed += OnFleePressed;
            _resetButton.Pressed += StartNewFight;

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
            _lastLogLineBlank = true;

            var player = new Player("Hero", PlayerClass.Warrior);
            player.EquipWeapon(ItemDatabase.GetWeapon("Rusty Sword", 1));
            AddSlot(player, _partyRow, new Color(0.3f, 0.5f, 0.9f));

            // Testing companions so far: Chance (aggressive, unarmed), Felicity (supportive
            // healer), Risk (defensive tank) - covers the three AI styles currently in use.
            var companions = new List<CompanionBase> { new CompanionChance(1), new CompanionFelicity(1), new CompanionRisk(1) };
            foreach (var companion in companions)
            {
                companion.JoinParty();
                AddSlot(companion, _partyRow, new Color(0.4f, 0.7f, 0.6f));
            }

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
            _combatManager.StartCombat(player, enemies, companions.Cast<Entity>().ToList());
            RefreshAllSlots();
            UpdateActionAvailability();
        }

        private EntitySlot AddSlot(Entity entity, Container row, Color color)
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
            var actor = _combatManager.CurrentActor;
            var target = ResolveTarget();
            if (actor == null || target == null) return;
            Submit(GameLogic.Combat.CombatAction.Attack(actor, target));
        }

        private void OnDefendPressed()
        {
            var actor = _combatManager.CurrentActor;
            if (actor == null) return;
            Submit(GameLogic.Combat.CombatAction.Defend(actor));
        }

        private void OnFleePressed()
        {
            var actor = _combatManager.CurrentActor;
            if (actor == null) return;
            Submit(GameLogic.Combat.CombatAction.Flee(actor));
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

        // CombatManager.Log() also feeds the console text game, so it includes lines the
        // Godot log doesn't want (round banners, per-turn HP summaries - hovering an
        // EntitySlot already shows HP). Filtered here, not in CombatManager, so the
        // console/test experience stays unchanged.
        private void AppendLog(string bbcodeLine)
        {
            var keptLines = new List<string>();
            foreach (var rawLine in bbcodeLine.Split('\n'))
            {
                string trimmed = rawLine.Trim();
                bool isRoundBanner = trimmed.Length > 0 && trimmed.All(c => c == '=');
                bool isRoundNumber = trimmed.StartsWith("ROUND ");
                bool isTurnOrderHeader = trimmed == "Turn Order (by Speed):";
                // The turn order itself already shows relative speed (fastest to slowest),
                // so the numeric listing and per-turn speed-change lines are redundant.
                bool isSpeedListing = trimmed.Contains("(Speed:") || trimmed.Contains("'s speed ");
                bool isHpSummary = trimmed.Contains("'s HP:") || trimmed.StartsWith("Enemies: ");
                bool isRoundFooter = trimmed == "--- End of Round ---" || trimmed == "--- Speed Adjustments for Next Round ---";
                // "X attacks Y!" - who-attacked-who will be shown visually (animation/effect)
                // in a later pass instead of as log text.
                bool isAttackAnnouncement = trimmed.Contains(" attacks ") && trimmed.EndsWith("!");
                // "X took Y damage!" - the exact number will be a floating damage number as
                // part of battle animations later; hovering an EntitySlot shows current HP now.
                bool isDamageAmount = trimmed.Contains(" took ") && trimmed.EndsWith(" damage!");
                // Miss/crit will be shown visually later (0 damage = miss; a planned
                // damage-reduction display will show the full roll then subtract what was
                // blocked, on screen) instead of as log text.
                bool isMissOrCrit = trimmed == "The attack missed!" || trimmed == "CRITICAL HIT!";

                if (isRoundBanner || isRoundNumber || isTurnOrderHeader || isSpeedListing || isHpSummary
                    || isRoundFooter || isAttackAnnouncement || isDamageAmount || isMissOrCrit)
                    continue;

                keptLines.Add(rawLine);
            }

            if (keptLines.Count == 0) return;

            // Collapse any run of blank lines (within this batch or carried over from the
            // previous AppendLog call) down to at most one, so removing lines above doesn't
            // leave stacks of blank lines behind.
            var finalLines = new List<string>();
            foreach (var line in keptLines)
            {
                bool isBlank = line.Trim().Length == 0;
                if (isBlank && _lastLogLineBlank) continue;
                finalLines.Add(line);
                _lastLogLineBlank = isBlank;
            }

            if (finalLines.Count == 0) return;
            _log.AppendText(string.Join("\n", finalLines) + "\n");
        }
    }
}
