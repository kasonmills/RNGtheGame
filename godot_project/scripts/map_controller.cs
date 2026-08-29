using Godot;
using GameLogic.Core;
using GameLogic.World;

namespace RNGtheGame.Godot.Scripts
{
    public partial class MapController : Node
    {
        // TODO: wire this to the active GameManager instance (e.g. via an autoload/singleton)
        private GameManager? _gameManager;

        private void OnPlayerMovedToNode(MapNode node)
        {
            // MapNode exposes AvailableEvents rather than HasEnemy/HasLoot flags -- check
            // that list once the encounter trigger flow is designed.
            if (node.AvailableEvents.Contains("Combat"))
            {
                _gameManager?.TriggerCombatEncounter();
            }
            else if (node.AvailableEvents.Contains("Loot"))
            {
                _gameManager?.TriggerLootEvent();
            }
        }
    }
}
