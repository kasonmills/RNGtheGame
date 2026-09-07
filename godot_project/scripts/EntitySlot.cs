using System;
using Godot;
using GameLogic.Entities;

namespace RNGtheGame.Godot.Scripts
{
    // One combatant's placeholder visual: a colored "hitbox" standing in for a sprite,
    // a compact always-visible HP bar above it, and a detail popup (exact HP numbers)
    // that only shows on hover or selection. Designed to be instanced N times into a
    // row per side, since a fight can have 1-8 allies and any number of enemies.
    public partial class EntitySlot : Control
    {
        [Signal]
        public delegate void SelectedEventHandler(EntitySlot slot);

        [Export] public Color HitboxColor { get; set; } = new Color(0.3f, 0.5f, 0.9f);

        private ProgressBar _miniHpBar = null!;
        private Button _hitbox = null!;
        private PanelContainer _detailPopup = null!;
        private Label _detailLabel = null!;

        private Entity? _entity;
        private bool _hovering;
        private bool _selected;

        public Entity? BoundEntity => _entity;
        public bool IsSelected => _selected;

        public override void _Ready()
        {
            _miniHpBar = GetNode<ProgressBar>("%MiniHPBar");
            _hitbox = GetNode<Button>("%Hitbox");
            _detailPopup = GetNode<PanelContainer>("%DetailPopup");
            _detailLabel = GetNode<Label>("%DetailLabel");

            _hitbox.SelfModulate = HitboxColor;
            _hitbox.MouseEntered += () => { _hovering = true; UpdatePopupVisibility(); };
            _hitbox.MouseExited += () => { _hovering = false; UpdatePopupVisibility(); };
            _hitbox.Pressed += () => EmitSignal(SignalName.Selected, this);

            _detailPopup.Visible = false;
        }

        public void Bind(Entity entity)
        {
            _entity = entity;
            _hitbox.Text = entity.Name;
            Refresh();
        }

        // Call after the bound entity's HP changes (attack landed, revive, etc.)
        public void Refresh()
        {
            if (_entity == null)
                return;

            int hp = Mathf.Max(0, _entity.Health);
            _miniHpBar.MaxValue = _entity.MaxHealth;
            _miniHpBar.Value = hp;
            _detailLabel.Text = $"{_entity.Name}\nHP: {hp}/{_entity.MaxHealth}";

            bool alive = _entity.IsAlive();
            _hitbox.Disabled = !alive;
            _hitbox.Modulate = alive ? Colors.White : new Color(1, 1, 1, 0.4f);
        }

        public void SetSelected(bool selected)
        {
            _selected = selected;
            _hitbox.SelfModulate = selected ? HitboxColor.Lightened(0.35f) : HitboxColor;
            UpdatePopupVisibility();
        }

        private void UpdatePopupVisibility()
        {
            _detailPopup.Visible = _hovering || _selected;
        }
    }
}
