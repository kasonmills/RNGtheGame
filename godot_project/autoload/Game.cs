using Godot;
using GameLogic.Core;

namespace RNGtheGame.Godot.Autoload
{
    /// <summary>
    /// The one place a GameManager is created. Every other script reaches the game through
    /// this autoload (Game.Manager, or the shortcuts below) instead of building its own
    /// GameManager/Player/CombatManager - see "Ground rules" in docs/godot_frontend_plan.md.
    /// The one exception is scenes/debug/TestFight.cs, which deliberately bypasses this for
    /// a standalone combat sandbox.
    /// </summary>
    public partial class Game : Node
    {
        public static GameManager Manager { get; private set; } = null!;

        // Shortcuts the screens need most often (docs/godot_frontend_plan.md section 3).
        // Everything else not listed here is still reachable through Manager directly.
        public static GameLogic.Entities.Player.Player Player => Manager.Player;
        public static GameLogic.Entities.NPCs.Companions.PartyManager Party => Manager.Party;
        public static GameLogic.World.MapManager Map => Manager.Map;
        public static GameLogic.Quests.QuestManager Quests => Manager.Quests;
        public static GameLogic.Entities.NPCs.QuestGiver QuestGiver => Manager.QuestGiver;
        public static GameLogic.Entities.Enemies.Bosses.BossManager Bosses => Manager.Bosses;
        public static GameLogic.Systems.GameSettings Settings => Manager.Settings;
        public static GameLogic.Systems.StatisticsTracker Statistics => Manager.Statistics;
        public static GameLogic.Combat.CombatManager Combat => Manager.Combat;

        public override void _Ready()
        {
            Manager = new GameManager();
        }
    }
}
