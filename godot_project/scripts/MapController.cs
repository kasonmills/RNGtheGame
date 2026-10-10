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
            if (node.HasEnemy)
            {
                _gameManager?.TriggerCombatEncounter();
            }
            else if (node.HasLoot)
            {
                _gameManager?.TriggerLootEvent();
            }
        }
    }
}
