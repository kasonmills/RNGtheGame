using Godot;
using RNGtheGame.Godot.Autoload;

namespace RNGtheGame.Godot.Scenes.Boot
{
    /// <summary>
    /// The project's main scene - never swapped out. Holds ScreenLayer (fixed screens) and
    /// OverlayLayer (pause, party menu, shop, ...) and hands both to SceneRouter once, which
    /// then shows whatever state the game is already in (docs/godot_frontend_plan.md section 2/3).
    /// Game and SceneRouter are autoloads, so by the time this runs Game.Manager already exists.
    /// </summary>
    public partial class Boot : Control
    {
        public override void _Ready()
        {
            var screenLayer = GetNode<Control>("ScreenLayer");
            var overlayLayer = GetNode<Control>("OverlayLayer");

            SceneRouter.Instance.Initialize(screenLayer, overlayLayer);
        }
    }
}
