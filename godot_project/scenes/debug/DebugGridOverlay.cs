using Godot;

namespace RNGtheGame.Godot.Scripts
{
    // Dev-only coordinate reference grid, drawn on top of a scene so layout changes
    // can be described by x/y pixel coordinates instead of guesswork. Press F3 to
    // toggle. Not part of the shipped game - add as the last child of a scene's root.
    public partial class DebugGridOverlay : Control
    {
        [Export] public int MinorSpacingPx = 50;
        [Export] public int MajorSpacingPx = 100;

        public override void _Ready()
        {
            MouseFilter = MouseFilterEnum.Ignore;
            ZIndex = 4096;
        }

        public override void _UnhandledKeyInput(InputEvent @event)
        {
            if (@event is InputEventKey { Pressed: true, Keycode: Key.F3 })
            {
                Visible = !Visible;
                if (Visible) QueueRedraw();
                GetViewport().SetInputAsHandled();
            }
        }

        public override void _Draw()
        {
            var size = GetRect().Size;
            var minorColor = new Color(1f, 1f, 1f, 0.08f);
            var majorColor = new Color(1f, 1f, 0f, 0.35f);
            var font = ThemeDB.FallbackFont;

            for (int x = 0; x <= size.X; x += MinorSpacingPx)
            {
                bool major = x % MajorSpacingPx == 0;
                DrawLine(new Vector2(x, 0), new Vector2(x, size.Y), major ? majorColor : minorColor, major ? 1.5f : 1f);
                if (major)
                    DrawString(font, new Vector2(x + 2, 14), x.ToString(), HorizontalAlignment.Left, -1, 13, majorColor);
            }

            for (int y = 0; y <= size.Y; y += MinorSpacingPx)
            {
                bool major = y % MajorSpacingPx == 0;
                DrawLine(new Vector2(0, y), new Vector2(size.X, y), major ? majorColor : minorColor, major ? 1.5f : 1f);
                if (major)
                    DrawString(font, new Vector2(2, y - 2), y.ToString(), HorizontalAlignment.Left, -1, 13, majorColor);
            }
        }
    }
}
