using System;
using System.Collections.Generic;
using System.Linq;
using GameLogic.Systems;

namespace GameLogic.World
{
    /// <summary>
    /// Manages the game map - node-based world with procedural generation
    /// </summary>
    public class MapManager
    {
        private Dictionary<int, MapNode> _nodes;
        private int _currentNodeId;
        private int _mapSeed;
        private RNGManager _rng;

        public MapManager()
        {
            _nodes = new Dictionary<int, MapNode>();
            _currentNodeId = 0;
            _rng = new RNGManager();
        }

        /// <summary>
        /// Generate a new map for a new game
        /// </summary>
        public void GenerateNewMap()
        {
            _mapSeed = _rng.Roll(1, 1000000);
            GenerateMapFromSeed(_mapSeed);
        }

        /// <summary>
        /// Generate a map from a specific seed (for save/load)
        /// </summary>
        public void GenerateMapFromSeed(int seed)
        {
            Console.WriteLine($"Generating map from seed: {seed}");
            _mapSeed = seed;
            _rng = new RNGManager(seed);
            _nodes.Clear();

            // Generate the map structure
            GenerateLinearMap();

            // Set starting position
            _currentNodeId = 0;
            GetCurrentNode().Visit();
            GetCurrentNode().Discover();

            // Discover adjacent nodes
            DiscoverAdjacentNodes();

            Console.WriteLine($"Map generated! Starting at: {GetCurrentLocationName()}");
        }

        /// <summary>
        /// Generate a linear map with branches (like Slay the Spire)
        /// </summary>
        private void GenerateLinearMap()
        {
            int nodeIdCounter = 0;

            // Layer 1: Starting Town
            var startTown = new MapNode(nodeIdCounter++, "Haven Village", LocationType.Town, dangerLevel: 0);
            startTown.AvailableEvents.Add("Shop");
            startTown.AvailableEvents.Add("Rest");
            _nodes.Add(startTown.Id, startTown);

            // Layer 2: Early game paths (3 nodes)
            List<MapNode> layer2 = new List<MapNode>();
            layer2.Add(new MapNode(nodeIdCounter++, "Whispering Woods", LocationType.Forest, dangerLevel: 1));
            layer2.Add(new MapNode(nodeIdCounter++, "Old Crossroads", LocationType.Crossroads, dangerLevel: 1));
            layer2.Add(new MapNode(nodeIdCounter++, "Abandoned Camp", LocationType.RestSite, dangerLevel: 1));

            foreach (var node in layer2)
            {
                node.AvailableEvents.Add("Combat");
                if (node.Type == LocationType.RestSite) node.AvailableEvents.Add("Rest");
                _nodes.Add(node.Id, node);
                node.ConnectTo(startTown);
            }

            // Layer 3: Mid-game areas (4 nodes)
            List<MapNode> layer3 = new List<MapNode>();
            layer3.Add(new MapNode(nodeIdCounter++, "Dark Cave", LocationType.Cave, dangerLevel: 3));
            layer3.Add(new MapNode(nodeIdCounter++, "Ancient Ruins", LocationType.Ruins, dangerLevel: 3));
            layer3.Add(new MapNode(nodeIdCounter++, "Treasure Vault", LocationType.TreasureRoom, dangerLevel: 2));
            layer3.Add(new MapNode(nodeIdCounter++, "Mountain Pass", LocationType.Mountain, dangerLevel: 4));

            foreach (var node in layer3)
            {
                if (node.Type == LocationType.TreasureRoom)
                {
                    node.AvailableEvents.Add("Loot");
                }
                else
                {
                    node.AvailableEvents.Add("Combat");
                    if (_rng.Roll(1, 100) <= 30) node.AvailableEvents.Add("Loot");
                }

                _nodes.Add(node.Id, node);

                // Connect to 1-2 random nodes from previous layer
                int connections = _rng.Roll(1, 2);
                for (int i = 0; i < connections; i++)
                {
                    var prevNode = layer2[_rng.Roll(0, layer2.Count - 1)];
                    node.ConnectTo(prevNode);
                }
            }

            // Layer 4: Rest area before boss
            var restArea = new MapNode(nodeIdCounter++, "Shrine of Heroes", LocationType.RestSite, dangerLevel: 0);
            restArea.AvailableEvents.Add("Rest");
            restArea.AvailableEvents.Add("Shop");
            _nodes.Add(restArea.Id, restArea);

            foreach (var node in layer3)
            {
                restArea.ConnectTo(node);
            }

            // Layer 5: Boss
            var bossRoom = new MapNode(nodeIdCounter++, "The Tyrant's Lair", LocationType.BossRoom, dangerLevel: 10);
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
        /// Travel to a specific node
        /// </summary>
        public bool TravelTo(int nodeId)
        {
            var currentNode = GetCurrentNode();
            if (currentNode == null) return false;

            // Check if destination is connected
            if (!currentNode.ConnectedNodeIds.Contains(nodeId))
            {
                Console.WriteLine("That location is not reachable from here!");
                return false;
            }

            // Check if locked
            if (_nodes[nodeId].IsLocked)
            {
                Console.WriteLine("That location is locked!");
                return false;
            }

            // Travel
            _currentNodeId = nodeId;
            var newNode = GetCurrentNode();
            newNode.Visit();

            // Discover adjacent nodes
            DiscoverAdjacentNodes();

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