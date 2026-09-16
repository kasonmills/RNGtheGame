using System;
using System.Collections.Generic;

namespace GameLogic.World
{
    /// <summary>
    /// Types of locations on the map
    /// </summary>
    public enum LocationType
    {
        Town,           // Safe area with shops, quests, rest
        Forest,         // Exploration area with combat
        Cave,           // Dungeon with enemies
        Ruins,          // Ancient location with treasure
        Mountain,       // Challenging area
        Crossroads,     // Junction point between areas
        BossRoom,       // Special combat encounter
        TreasureRoom,   // Guaranteed loot
        RestSite        // Heal location
    }

    /// <summary>
    /// Represents a single location on the map
    /// </summary>
    public class MapNode
    {
        // Basic Info
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public LocationType Type { get; set; }

        // Connections
        public List<int> ConnectedNodeIds { get; set; }

        // Location Properties
        public bool IsVisited { get; set; }
        public bool IsDiscovered { get; set; }      // Can see on map but haven't visited
        public bool IsLocked { get; set; }          // Requires key/quest completion

        // Events
        public List<string> AvailableEvents { get; set; }  // Combat, Loot, Quest, etc.

        // World position (used to place this node on the Godot map scene)
        public float PositionX { get; set; }
        public float PositionY { get; set; }

        // Convenience flags for movement-driven event triggers (see godot_integration/scripts/map_controller.cs)
        public bool HasEnemy => AvailableEvents.Contains("Combat") || AvailableEvents.Contains("BossCombat");
        public bool HasLoot => AvailableEvents.Contains("Loot");
        public bool HasBoss => AvailableEvents.Contains("BossCombat");

        public MapNode(int id, string name, LocationType type, float positionX = 0, float positionY = 0)
        {
            Id = id;
            Name = name;
            Type = type;
            PositionX = positionX;
            PositionY = positionY;
            ConnectedNodeIds = new List<int>();
            AvailableEvents = new List<string>();
            IsVisited = false;
            IsDiscovered = false;
            IsLocked = false;
            Description = GetDefaultDescription();
        }

        /// <summary>
        /// Get default description based on location type
        /// </summary>
        private string GetDefaultDescription()
        {
            return Type switch
            {
                LocationType.Town => "A bustling settlement with merchants and townsfolk.",
                LocationType.Forest => "A dense forest filled with wildlife and danger.",
                LocationType.Cave => "A dark cave system echoing with unknown sounds.",
                LocationType.Ruins => "Ancient ruins holding secrets of the past.",
                LocationType.Mountain => "A treacherous mountain path.",
                LocationType.Crossroads => "A fork in the road leading to multiple destinations.",
                LocationType.BossRoom => "An ominous chamber emanating powerful energy.",
                LocationType.TreasureRoom => "A glittering room filled with treasures.",
                LocationType.RestSite => "A peaceful clearing perfect for rest.",
                _ => "An unknown location."
            };
        }

        /// <summary>
        /// Connect this node to another node (bidirectional)
        /// </summary>
        public void ConnectTo(MapNode otherNode)
        {
            if (!ConnectedNodeIds.Contains(otherNode.Id))
            {
                ConnectedNodeIds.Add(otherNode.Id);
            }

            if (!otherNode.ConnectedNodeIds.Contains(Id))
            {
                otherNode.ConnectedNodeIds.Add(Id);
            }
        }

        /// <summary>
        /// Connect this node to another node (one-way)
        /// </summary>
        public void ConnectToOneWay(int targetNodeId)
        {
            if (!ConnectedNodeIds.Contains(targetNodeId))
            {
                ConnectedNodeIds.Add(targetNodeId);
            }
        }

        /// <summary>
        /// Mark this location as visited
        /// </summary>
        public void Visit()
        {
            IsVisited = true;
            IsDiscovered = true;
        }

        /// <summary>
        /// Discover this location (show on map but not visited)
        /// </summary>
        public void Discover()
        {
            IsDiscovered = true;
        }

        /// <summary>
        /// Enter this node (legacy method for compatibility)
        /// </summary>
        public void EnterNode()
        {
            Visit();
            // TODO-GODOT: arrival text below -> popup/HUD text once player sprite reaches this node's position
            Console.WriteLine($"\n=== {Name} ===");
            Console.WriteLine(Description);

            if (AvailableEvents.Count > 0)
            {
                Console.WriteLine($"Available: {string.Join(", ", AvailableEvents)}");
            }
        }

        /// <summary>
        /// Get display string for this location
        /// </summary>
        public string GetDisplayString()
        {
            string status = IsVisited ? "[VISITED]" : IsDiscovered ? "[DISCOVERED]" : "[UNKNOWN]";
            string locked = IsLocked ? " [LOCKED]" : "";
            return $"{status}{locked} {Name} ({Type})";
        }
    }
}