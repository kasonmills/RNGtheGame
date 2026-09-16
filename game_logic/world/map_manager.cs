using System;
using System.Collections.Generic;
using System.Linq;

namespace GameLogic.World
{
    /// <summary>
    /// Manages the game map - a fixed, hand-authored node graph
    /// </summary>
    public class MapManager
    {
        private Dictionary<int, MapNode> _nodes;
        private int _currentNodeId;

        // Kept only so existing save files (which persist a MapSeed) keep loading
        // without a save-format change; the layout below is fixed, not randomized.
        private const int FixedMapId = 1;
        private int _mapSeed;

        public MapManager()
        {
            _nodes = new Dictionary<int, MapNode>();
            _currentNodeId = 0;
        }

        /// <summary>
        /// Generate the map for a new game
        /// </summary>
        public void GenerateNewMap()
        {
            GenerateMapFromSeed(FixedMapId);
        }

        /// <summary>
        /// Generate the map (for save/load compatibility - the layout is fixed regardless of seed)
        /// </summary>
        public void GenerateMapFromSeed(int seed)
        {
            _mapSeed = FixedMapId;
            _nodes.Clear();

            // Build the map structure
            GenerateFixedMap();

            // Set starting position
            _currentNodeId = 0;
            GetCurrentNode().Visit();
            GetCurrentNode().Discover();

            // Discover adjacent nodes
            DiscoverAdjacentNodes();

            Console.WriteLine($"Map loaded! Starting at: {GetCurrentLocationName()}");
        }

        /// <summary>
        /// Build the fixed story map (linear path with branches, like Slay the Spire)
        /// </summary>
        private void GenerateFixedMap()
        {
            int nodeIdCounter = 0;

            // Layer 1: Starting Town
            var startTown = new MapNode(nodeIdCounter++, "Haven Village", LocationType.Town, positionX: 0, positionY: 0);
            startTown.AvailableEvents.Add("Shop");
            startTown.AvailableEvents.Add("Rest");
            _nodes.Add(startTown.Id, startTown);

            // Layer 2: Early game paths (3 nodes)
            var whisperingWoods = new MapNode(nodeIdCounter++, "Whispering Woods", LocationType.Forest, positionX: 1, positionY: -1);
            var oldCrossroads = new MapNode(nodeIdCounter++, "Old Crossroads", LocationType.Crossroads, positionX: 1, positionY: 0);
            var abandonedCamp = new MapNode(nodeIdCounter++, "Abandoned Camp", LocationType.RestSite, positionX: 1, positionY: 1);
            var layer2 = new List<MapNode> { whisperingWoods, oldCrossroads, abandonedCamp };

            foreach (var node in layer2)
            {
                node.AvailableEvents.Add("Combat");
                if (node.Type == LocationType.RestSite) node.AvailableEvents.Add("Rest");
                _nodes.Add(node.Id, node);
                node.ConnectTo(startTown);
            }

            // Layer 3: Mid-game areas (4 nodes), each with fixed connections back to layer 2
            var darkCave = new MapNode(nodeIdCounter++, "Dark Cave", LocationType.Cave, positionX: 2, positionY: -1.5f);
            var ancientRuins = new MapNode(nodeIdCounter++, "Ancient Ruins", LocationType.Ruins, positionX: 2, positionY: -0.5f);
            var treasureVault = new MapNode(nodeIdCounter++, "Treasure Vault", LocationType.TreasureRoom, positionX: 2, positionY: 0.5f);
            var mountainPass = new MapNode(nodeIdCounter++, "Mountain Pass", LocationType.Mountain, positionX: 2, positionY: 1.5f);
            var layer3 = new List<MapNode> { darkCave, ancientRuins, treasureVault, mountainPass };

            foreach (var node in layer3)
            {
                if (node.Type == LocationType.TreasureRoom)
                {
                    node.AvailableEvents.Add("Loot");
                }
                else
                {
                    node.AvailableEvents.Add("Combat");
                }

                _nodes.Add(node.Id, node);
            }

            // Ancient Ruins gets a bonus Loot event to fit its lore (fixed, not randomized)
            ancientRuins.AvailableEvents.Add("Loot");

            darkCave.ConnectTo(whisperingWoods);
            ancientRuins.ConnectTo(whisperingWoods);
            ancientRuins.ConnectTo(oldCrossroads);
            treasureVault.ConnectTo(oldCrossroads);
            mountainPass.ConnectTo(oldCrossroads);
            mountainPass.ConnectTo(abandonedCamp);

            // Layer 4: Rest area before boss
            var restArea = new MapNode(nodeIdCounter++, "Shrine of Heroes", LocationType.RestSite, positionX: 3, positionY: 0);
            restArea.AvailableEvents.Add("Rest");
            restArea.AvailableEvents.Add("Shop");
            _nodes.Add(restArea.Id, restArea);

            foreach (var node in layer3)
            {
                restArea.ConnectTo(node);
            }

            // Layer 5: Boss
            var bossRoom = new MapNode(nodeIdCounter++, "The Tyrant's Lair", LocationType.BossRoom, positionX: 4, positionY: 0);
            bossRoom.AvailableEvents.Add("BossCombat");
            _nodes.Add(bossRoom.Id, bossRoom);
            bossRoom.ConnectTo(restArea);
        }

        /// <summary>
        /// Get the current location name
        /// </summary>
        public string GetCurrentLocationName()
        {
            return GetCurrentNode()?.Name ?? "Unknown";
        }

        /// <summary>
        /// Get the current map node
        /// </summary>
        public MapNode GetCurrentNode()
        {
            return _nodes.ContainsKey(_currentNodeId) ? _nodes[_currentNodeId] : null;
        }

        /// <summary>
        /// Get a node by id (e.g. for placing/looking up nodes on the Godot map scene)
        /// </summary>
        public MapNode GetNodeById(int nodeId)
        {
            return _nodes.ContainsKey(nodeId) ? _nodes[nodeId] : null;
        }

        /// <summary>
        /// Get every node in the map (e.g. for laying out the Godot map scene)
        /// </summary>
        public IReadOnlyCollection<MapNode> GetAllNodes()
        {
            return _nodes.Values;
        }

        /// <summary>
        /// Get available travel destinations from current location
        /// </summary>
        public List<MapNode> GetAvailableDestinations()
        {
            var currentNode = GetCurrentNode();
            if (currentNode == null) return new List<MapNode>();

            var destinations = new List<MapNode>();
            foreach (var nodeId in currentNode.ConnectedNodeIds)
            {
                if (_nodes.ContainsKey(nodeId) && !_nodes[nodeId].IsLocked)
                {
                    destinations.Add(_nodes[nodeId]);
                }
            }

            return destinations;
        }

        /// <summary>
        /// Check whether the player can currently move to the given node
        /// (e.g. gate physical movement input on the Godot map scene)
        /// </summary>
        public bool CanTravelTo(int nodeId)
        {
            var currentNode = GetCurrentNode();
            if (currentNode == null) return false;
            if (!currentNode.ConnectedNodeIds.Contains(nodeId)) return false;
            if (!_nodes.ContainsKey(nodeId)) return false;
            if (_nodes[nodeId].IsLocked) return false;

            return true;
        }

        /// <summary>
        /// Travel to a specific node
        /// </summary>
        public bool TravelTo(int nodeId)
        {
            if (!CanTravelTo(nodeId))
            {
                // TODO-GODOT: feedback below -> blocked-movement/locked UI cue instead of console text
                if (GetCurrentNode() != null && !GetCurrentNode().ConnectedNodeIds.Contains(nodeId))
                {
                    Console.WriteLine("That location is not reachable from here!");
                }
                else if (_nodes.ContainsKey(nodeId) && _nodes[nodeId].IsLocked)
                {
                    Console.WriteLine("That location is locked!");
                }

                return false;
            }

            // Travel
            _currentNodeId = nodeId;
            var newNode = GetCurrentNode();
            newNode.Visit();

            // Discover adjacent nodes
            DiscoverAdjacentNodes();

            // TODO-GODOT: text below -> player sprite moves to newNode's position; newNode.EnterNode() becomes arrival popup/HUD text
            Console.WriteLine($"\nTraveled to: {newNode.Name}");
            newNode.EnterNode();

            return true;
        }

        /// <summary>
        /// Discover all nodes adjacent to current position
        /// </summary>
        private void DiscoverAdjacentNodes()
        {
            var currentNode = GetCurrentNode();
            if (currentNode == null) return;

            foreach (var nodeId in currentNode.ConnectedNodeIds)
            {
                if (_nodes.ContainsKey(nodeId))
                {
                    _nodes[nodeId].Discover();
                }
            }
        }

        /// <summary>
        /// Get map seed for saving
        /// </summary>
        public int GetMapSeed()
        {
            return _mapSeed;
        }

        /// <summary>
        /// Get current node ID for saving
        /// </summary>
        public int GetCurrentNodeId()
        {
            return _currentNodeId;
        }

        /// <summary>
        /// Set current node (for loading saves)
        /// </summary>
        public void SetCurrentNode(int nodeId)
        {
            if (_nodes.ContainsKey(nodeId))
            {
                _currentNodeId = nodeId;
            }
        }

        /// <summary>
        /// Display the map (show discovered locations)
        /// </summary>
        public void DisplayMap()
        {
            // TODO-GODOT: text map below -> visual map scene using each node's PositionX/PositionY
            Console.WriteLine("\n=== MAP ===");
            Console.WriteLine($"Current Location: {GetCurrentLocationName()}\n");

            var discoveredNodes = _nodes.Values.Where(n => n.IsDiscovered).OrderBy(n => n.Id);

            foreach (var node in discoveredNodes)
            {
                string marker = node.Id == _currentNodeId ? " >>> " : "     ";
                Console.WriteLine($"{marker}{node.GetDisplayString()}");
            }

            Console.WriteLine($"\nTotal Discovered: {discoveredNodes.Count()}/{_nodes.Count}");
        }

        /// <summary>
        /// Load map from a saved map name (for future implementation)
        /// </summary>
        public void LoadMap(string mapName)
        {
            Console.WriteLine($"Loading map: {mapName}");
            // Future: Load custom/handcrafted maps from files
            GenerateNewMap();
        }
    }
}
