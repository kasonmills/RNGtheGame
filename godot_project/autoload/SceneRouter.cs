using System.Collections.Generic;
using Godot;
using GameLogic.Core;

namespace RNGtheGame.Godot.Autoload
{
    /// <summary>
    /// The one table that says "this GameState shows this scene" (docs/godot_frontend_plan.md
    /// section 3). Reacts to GameManager.StateChanged - no screen changes scenes by itself,
    /// it calls the backend (OpenX(), PushState, ChangeState) and this router does the rest.
    ///
    /// Fixed states replace whatever is in ScreenLayer. Overlay states are pushed on top of
    /// OverlayLayer and popped back off - the fixed screen underneath stays loaded the whole
    /// time, so the player returns to exactly where they were.
    /// </summary>
    public partial class SceneRouter : Node
    {
        // GameState -> scene table (docs/godot_frontend_plan.md section 3's first draft).
        // Most of these scenes don't exist yet - ShowState() checks before loading and logs
        // a clear one-line message instead of crashing, so screens can be dropped in here
        // one at a time with no router changes needed.
        private static readonly Dictionary<GameState, string> FixedScenes = new()
        {
            { GameState.MainMenu, "res://scenes/main_menu/main_menu.tscn" },
            { GameState.Cutscene, "res://scenes/tutorial/intro_story.tscn" },
            { GameState.Playing, "res://scenes/world/world.tscn" },
            { GameState.Combat, "res://scenes/combat/combat_scene.tscn" },
            { GameState.GameOver, "res://scenes/endings/game_over.tscn" },
            { GameState.Ending, "res://scenes/endings/ending.tscn" },
            { GameState.Credits, "res://scenes/endings/credits.tscn" },
        };

        // Which states are overlays at all (GameState's own grouping) - kept separate from
        // OverlayScenes below, which only has entries once a state's scene file is decided.
        // Tutorial is overlay-kind per GameState's comments, but the plan (section 3) hasn't
        // named its scene file yet, so it has no entry in OverlayScenes until that's decided.
        private static readonly HashSet<GameState> OverlayStates = new()
        {
            GameState.Tutorial, GameState.Paused, GameState.PartyMenu, GameState.QuestLog,
            GameState.Statistics, GameState.Settings, GameState.SaveScreen, GameState.Dialogue,
            GameState.Shopping, GameState.Blacksmith, GameState.QuestGiver,
        };

        private static readonly Dictionary<GameState, string> OverlayScenes = new()
        {
            { GameState.Paused, "res://scenes/overlays/pause/pause_menu.tscn" },
            { GameState.PartyMenu, "res://scenes/overlays/party/party_menu.tscn" },
            { GameState.QuestLog, "res://scenes/overlays/quests/quest_log.tscn" },
            { GameState.Statistics, "res://scenes/overlays/statistics/statistics_screen.tscn" },
            { GameState.Settings, "res://scenes/overlays/settings/settings_menu.tscn" },
            { GameState.SaveScreen, "res://scenes/overlays/save/save_screen.tscn" },
            { GameState.Dialogue, "res://scenes/overlays/dialogue/dialogue_box.tscn" },
            { GameState.Shopping, "res://scenes/overlays/shop/shop_screen.tscn" },
            { GameState.Blacksmith, "res://scenes/overlays/blacksmith/blacksmith_screen.tscn" },
            { GameState.QuestGiver, "res://scenes/overlays/quests/quest_giver_screen.tscn" },
        };

        public static SceneRouter Instance { get; private set; } = null!;

        private Control _screenLayer = null!;
        private Control _overlayLayer = null!;
        private GameState? _currentFixedState;

        // Mirrors the overlay side of GameManager's own state stack, bottom to top, so this
        // router can tell a push (open one more overlay) from a pop back to a shallower one
        // without needing to see the backend's private stack.
        private readonly List<GameState> _overlayStack = new();
        private readonly List<Node> _overlayNodes = new();

        public override void _Ready()
        {
            Instance = this;
        }

        /// <summary>
        /// Called once by Boot.cs after it finds its own ScreenLayer/OverlayLayer. Subscribes
        /// to the backend and immediately shows whatever state the game is already in - this
        /// is what makes the router "run on start-up and after a load" (the plan's wording):
        /// a load calls GameManager.ChangeState(Playing), which fires StateChanged like any
        /// other transition, so no separate load hook is needed here.
        /// </summary>
        public void Initialize(Control screenLayer, Control overlayLayer)
        {
            _screenLayer = screenLayer;
            _overlayLayer = overlayLayer;

            Game.Manager.StateChanged += OnStateChanged;
            ShowFixedState(Game.Manager.CurrentState);
        }

        private void OnStateChanged(GameState oldState, GameState newState)
        {
            if (OverlayStates.Contains(newState))
            {
                // A pop back to a shallower overlay lands on whatever is just below the
                // current top of our mirrored stack; anything else opening an overlay state
                // is a push, including the very first overlay over a fixed screen.
                bool isPopToShallowerOverlay =
                    _overlayStack.Count > 0 &&
                    _overlayStack[^1] == oldState &&
                    _overlayStack.Count >= 2 &&
                    _overlayStack[^2] == newState;

                if (isPopToShallowerOverlay)
                {
                    PopOverlay();
                }
                else
                {
                    PushOverlay(newState);
                }
            }
            else
            {
                // Reaching a fixed state always means back to the base - every
                // backend path that ends here (PopState unwinding to the bottom, or a
                // one-way ChangeState) leaves nothing stacked above it.
                while (_overlayStack.Count > 0)
                {
                    PopOverlay();
                }

                ShowFixedState(newState);
            }
        }

        private void ShowFixedState(GameState state)
        {
            if (_currentFixedState == state)
            {
                return;
            }

            foreach (Node child in _screenLayer.GetChildren())
            {
                child.QueueFree();
            }

            if (!FixedScenes.TryGetValue(state, out var path))
            {
                GD.Print($"SceneRouter: no scene registered for fixed state {state}.");
                _currentFixedState = state;
                return;
            }

            if (!ResourceLoader.Exists(path))
            {
                GD.Print($"SceneRouter: {state} would show {path}, but that scene doesn't exist yet.");
                _currentFixedState = state;
                return;
            }

            var scene = GD.Load<PackedScene>(path).Instantiate();
            _screenLayer.AddChild(scene);
            _currentFixedState = state;
        }

        private void PushOverlay(GameState state)
        {
            _overlayStack.Add(state);

            if (!OverlayScenes.TryGetValue(state, out var path) || !ResourceLoader.Exists(path))
            {
                GD.Print($"SceneRouter: overlay {state} has no scene yet.");
                _overlayNodes.Add(null!);
                return;
            }

            var scene = GD.Load<PackedScene>(path).Instantiate();
            _overlayLayer.AddChild(scene);
            _overlayNodes.Add(scene);
        }

        private void PopOverlay()
        {
            if (_overlayStack.Count == 0)
            {
                return;
            }

            int top = _overlayStack.Count - 1;
            _overlayNodes[top]?.QueueFree();
            _overlayNodes.RemoveAt(top);
            _overlayStack.RemoveAt(top);
        }
    }
}
