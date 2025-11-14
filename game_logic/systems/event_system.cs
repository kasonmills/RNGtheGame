using System;
using System.Collections.Generic;
using System.Linq;

namespace GameLogic.Systems
{
    /// <summary>
    /// Types of dynamic world events
    /// </summary>
    public enum WorldEventType
    {
        RandomEncounter,    // Random enemy spawn
        MerchantVisit,      // Traveling merchant appears
        TreasureFound,      // Hidden treasure discovered
        WeatherChange,      // Weather/environmental change
        NPCEncounter,       // Random NPC interaction
        AmbientEvent,       // Flavor/atmosphere events
        LuckyFind,          // Bonus resources/items
        Hazard              // Environmental hazard
    }

    /// <summary>
    /// Rarity/frequency of world events
    /// </summary>
    public enum EventRarity
    {
        Common,         // Happens frequently
        Uncommon,       // Happens occasionally
        Rare,           // Happens rarely
        VeryRare,       // Happens very rarely
        Legendary       // Extremely rare
    }

    /// <summary>
    /// A single world event instance
    /// </summary>
    public class WorldEvent
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public WorldEventType Type { get; set; }
        public EventRarity Rarity { get; set; }

        public int MinPlayerLevel { get; set; }      // Minimum level to trigger
        public int MaxPlayerLevel { get; set; }      // Maximum level to trigger (0 = no max)
        public float TriggerChance { get; set; }     // Base chance to trigger (0.0 - 1.0)

        public bool IsActive { get; set; }           // Currently active
        public DateTime? ActivatedAt { get; set; }   // When event was activated
        public int? DurationSeconds { get; set; }    // How long event lasts (null = instant)

        public Dictionary<string, object> EventData { get; set; } // Custom data for the event

        public WorldEvent(
            string id,
            string name,
            string description,
            WorldEventType type,
            EventRarity rarity,
            float triggerChance)
        {
            Id = id;
            Name = name;
            Description = description;
            Type = type;
            Rarity = rarity;
            TriggerChance = triggerChance;
            MinPlayerLevel = 1;
            MaxPlayerLevel = 0; // No max
            IsActive = false;
            EventData = new Dictionary<string, object>();
        }

        /// <summary>
        /// Check if event has expired based on duration
        /// </summary>
        public bool HasExpired()
        {
            if (!IsActive || !DurationSeconds.HasValue || !ActivatedAt.HasValue)
                return false;

            return (DateTime.Now - ActivatedAt.Value).TotalSeconds >= DurationSeconds.Value;
        }

        /// <summary>
        /// Get remaining time for event
        /// </summary>
        public TimeSpan? GetRemainingTime()
        {
            if (!IsActive || !DurationSeconds.HasValue || !ActivatedAt.HasValue)
                return null;

            var elapsed = DateTime.Now - ActivatedAt.Value;
            var remaining = TimeSpan.FromSeconds(DurationSeconds.Value) - elapsed;
            return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }
    }

    /// <summary>
    /// Manages dynamic world events that occur independently of quests/bosses
    /// Handles random encounters, environmental changes, merchant spawns, etc.
    /// </summary>
    public class EventSystem
    {
        private Dictionary<string, WorldEvent> _registeredEvents;
        private List<WorldEvent> _activeEvents;
        private Random _rng;

        // Event cooldowns to prevent spam
        private Dictionary<string, DateTime> _eventCooldowns;
        private const int DEFAULT_COOLDOWN_SECONDS = 300; // 5 minutes

        public IReadOnlyList<WorldEvent> ActiveEvents => _activeEvents.AsReadOnly();

        public EventSystem(Random rng = null)
        {
            _registeredEvents = new Dictionary<string, WorldEvent>();
            _activeEvents = new List<WorldEvent>();
            _eventCooldowns = new Dictionary<string, DateTime>();
            _rng = rng ?? new Random();
        }

        #region Event Registration

        /// <summary>
        /// Register a world event in the system
        /// </summary>
        public void RegisterEvent(WorldEvent worldEvent)
        {
            if (!_registeredEvents.ContainsKey(worldEvent.Id))
            {
                _registeredEvents[worldEvent.Id] = worldEvent;
            }
        }

        /// <summary>
        /// Register multiple events at once
        /// </summary>
        public void RegisterEvents(params WorldEvent[] events)
        {
            foreach (var worldEvent in events)
            {
                RegisterEvent(worldEvent);
            }
        }

        /// <summary>
        /// Unregister an event (remove from system)
        /// </summary>
        public bool UnregisterEvent(string eventId)
        {
            if (_registeredEvents.ContainsKey(eventId))
            {
                // Deactivate if active
                var worldEvent = _registeredEvents[eventId];
                if (worldEvent.IsActive)
                {
                    EndEvent(eventId);
                }

                _registeredEvents.Remove(eventId);
                return true;
            }
            return false;
        }

        #endregion

        #region Event Triggering

        /// <summary>
        /// Check for random event triggers based on player level and location
        /// Call this periodically (e.g., every game tick, location change, etc.)
        /// </summary>
        public List<WorldEvent> CheckForRandomEvents(int playerLevel, string locationId = null, float luckModifier = 1.0f)
        {
            var triggeredEvents = new List<WorldEvent>();

            // Clean up expired events first
            UpdateActiveEvents();

            // Check each registered event
            foreach (var worldEvent in _registeredEvents.Values)
            {
                // Skip if already active
                if (worldEvent.IsActive)
                    continue;

                // Check level requirements
                if (playerLevel < worldEvent.MinPlayerLevel)
                    continue;

                if (worldEvent.MaxPlayerLevel > 0 && playerLevel > worldEvent.MaxPlayerLevel)
                    continue;

                // Check cooldown
                if (IsOnCooldown(worldEvent.Id))
                    continue;

                // Roll for trigger chance
                float effectiveChance = worldEvent.TriggerChance * luckModifier;
                float roll = (float)_rng.NextDouble();

                if (roll <= effectiveChance)
                {
                    TriggerEvent(worldEvent.Id);
                    triggeredEvents.Add(worldEvent);
                }
            }

            return triggeredEvents;
        }

        /// <summary>
        /// Manually trigger a specific event
        /// </summary>
        public bool TriggerEvent(string eventId)
        {
            if (!_registeredEvents.TryGetValue(eventId, out WorldEvent worldEvent))
            {
                Console.WriteLine($"Event '{eventId}' not found.");
                return false;
            }

            if (worldEvent.IsActive)
            {
                Console.WriteLine($"Event '{worldEvent.Name}' is already active.");
                return false;
            }

            // Activate the event
            worldEvent.IsActive = true;
            worldEvent.ActivatedAt = DateTime.Now;
            _activeEvents.Add(worldEvent);

            // Set cooldown
            _eventCooldowns[eventId] = DateTime.Now;

            Console.WriteLine($"World Event: {worldEvent.Name}");
            Console.WriteLine($"  {worldEvent.Description}");

            return true;
        }

        /// <summary>
        /// Force trigger a specific event type (first matching event)
        /// </summary>
        public bool TriggerEventByType(WorldEventType type, int playerLevel)
        {
            var eligibleEvent = _registeredEvents.Values
                .Where(e => e.Type == type &&
                           !e.IsActive &&
                           playerLevel >= e.MinPlayerLevel &&
                           (e.MaxPlayerLevel == 0 || playerLevel <= e.MaxPlayerLevel) &&
                           !IsOnCooldown(e.Id))
                .OrderBy(e => _rng.Next()) // Random selection
                .FirstOrDefault();

            if (eligibleEvent != null)
            {
                return TriggerEvent(eligibleEvent.Id);
            }

            return false;
        }

        #endregion

        #region Event Management

        /// <summary>
        /// End an active event
        /// </summary>
        public bool EndEvent(string eventId)
        {
            var worldEvent = _activeEvents.FirstOrDefault(e => e.Id == eventId);
            if (worldEvent == null)
            {
                Console.WriteLine($"Event '{eventId}' is not active.");
                return false;
            }

            worldEvent.IsActive = false;
            worldEvent.ActivatedAt = null;
            _activeEvents.Remove(worldEvent);

            Console.WriteLine($"Event ended: {worldEvent.Name}");
            return true;
        }

        /// <summary>
        /// Update active events and automatically end expired ones
        /// Call this periodically
        /// </summary>
        public void UpdateActiveEvents()
        {
            var expiredEvents = _activeEvents.Where(e => e.HasExpired()).ToList();

            foreach (var worldEvent in expiredEvents)
            {
                EndEvent(worldEvent.Id);
            }
        }

        /// <summary>
        /// End all active events
        /// </summary>
        public void EndAllEvents()
        {
            var activeEventsCopy = _activeEvents.ToList();
            foreach (var worldEvent in activeEventsCopy)
            {
                EndEvent(worldEvent.Id);
            }
        }

        /// <summary>
        /// Check if an event is on cooldown
        /// </summary>
        public bool IsOnCooldown(string eventId)
        {
            if (!_eventCooldowns.TryGetValue(eventId, out DateTime lastTriggered))
                return false;

            var elapsed = DateTime.Now - lastTriggered;
            return elapsed.TotalSeconds < DEFAULT_COOLDOWN_SECONDS;
        }

        /// <summary>
        /// Get remaining cooldown time for an event
        /// </summary>
        public TimeSpan? GetCooldownRemaining(string eventId)
        {
            if (!_eventCooldowns.TryGetValue(eventId, out DateTime lastTriggered))
                return null;

            var elapsed = DateTime.Now - lastTriggered;
            var remaining = TimeSpan.FromSeconds(DEFAULT_COOLDOWN_SECONDS) - elapsed;

            return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }

        #endregion

        #region Event Queries

        /// <summary>
        /// Get an event by ID
        /// </summary>
        public WorldEvent GetEvent(string eventId)
        {
            _registeredEvents.TryGetValue(eventId, out WorldEvent worldEvent);
            return worldEvent;
        }

        /// <summary>
        /// Get all registered events of a specific type
        /// </summary>
        public List<WorldEvent> GetEventsByType(WorldEventType type)
        {
            return _registeredEvents.Values
                .Where(e => e.Type == type)
                .ToList();
        }

        /// <summary>
        /// Get all registered events of a specific rarity
        /// </summary>
        public List<WorldEvent> GetEventsByRarity(EventRarity rarity)
        {
            return _registeredEvents.Values
                .Where(e => e.Rarity == rarity)
                .ToList();
        }

        /// <summary>
        /// Check if any event of a specific type is active
        /// </summary>
        public bool IsEventTypeActive(WorldEventType type)
        {
            return _activeEvents.Any(e => e.Type == type);
        }

        /// <summary>
        /// Get count of active events
        /// </summary>
        public int GetActiveEventCount()
        {
            return _activeEvents.Count;
        }

        #endregion

        #region Display Methods

        /// <summary>
        /// Get summary of all active events
        /// </summary>
        public string GetActiveEventsSummary()
        {
            if (_activeEvents.Count == 0)
            {
                return "No active world events.";
            }

            string summary = "═══ Active World Events ═══\n";
            foreach (var worldEvent in _activeEvents)
            {
                summary += $"• [{worldEvent.Type}] {worldEvent.Name}";

                if (worldEvent.DurationSeconds.HasValue)
                {
                    var remaining = worldEvent.GetRemainingTime();
                    if (remaining.HasValue)
                    {
                        summary += $" ({remaining.Value.Minutes}m {remaining.Value.Seconds}s remaining)";
                    }
                }

                summary += "\n";
            }
            return summary;
        }

        /// <summary>
        /// Get detailed info about a specific event
        /// </summary>
        public string GetEventInfo(string eventId)
        {
            if (!_registeredEvents.TryGetValue(eventId, out WorldEvent worldEvent))
            {
                return "Event not found.";
            }

            string info = $"╔══════════════════════════════════════╗\n";
            info += $"  {worldEvent.Name}\n";
            info += $"╠══════════════════════════════════════╣\n";
            info += $"  Type: {worldEvent.Type}\n";
            info += $"  Rarity: {worldEvent.Rarity}\n";
            info += $"  Status: {(worldEvent.IsActive ? "ACTIVE" : "Inactive")}\n";
            info += $"  Trigger Chance: {worldEvent.TriggerChance * 100:F1}%\n";
            info += $"  Level Range: {worldEvent.MinPlayerLevel}";

            if (worldEvent.MaxPlayerLevel > 0)
                info += $" - {worldEvent.MaxPlayerLevel}";
            else
                info += "+";

            info += "\n";
            info += $"╠══════════════════════════════════════╣\n";
            info += $"  {worldEvent.Description}\n";
            info += $"╚══════════════════════════════════════╝\n";

            return info;
        }

        #endregion
    }

    /// <summary>
    /// Predefined world event templates for easy creation
    /// </summary>
    public static class WorldEventTemplates
    {
        /// <summary>
        /// Create a random encounter event
        /// </summary>
        public static WorldEvent CreateRandomEncounter(
            string id,
            string name,
            string description,
            int minLevel,
            int maxLevel = 0)
        {
            return new WorldEvent(id, name, description, WorldEventType.RandomEncounter, EventRarity.Common, 0.1f)
            {
                MinPlayerLevel = minLevel,
                MaxPlayerLevel = maxLevel,
                DurationSeconds = 60 // 1 minute to engage
            };
        }

        /// <summary>
        /// Create a traveling merchant event
        /// </summary>
        public static WorldEvent CreateMerchantVisit(
            string id,
            string merchantName,
            int minLevel)
        {
            return new WorldEvent(
                id,
                $"{merchantName} Visits",
                $"A traveling merchant named {merchantName} has arrived with rare goods!",
                WorldEventType.MerchantVisit,
                EventRarity.Uncommon,
                0.05f)
            {
                MinPlayerLevel = minLevel,
                DurationSeconds = 300 // 5 minutes
            };
        }

        /// <summary>
        /// Create a treasure discovery event
        /// </summary>
        public static WorldEvent CreateTreasureFind(
            string id,
            string treasureName,
            EventRarity rarity,
            int minLevel)
        {
            float triggerChance = rarity switch
            {
                EventRarity.Common => 0.15f,
                EventRarity.Uncommon => 0.08f,
                EventRarity.Rare => 0.03f,
                EventRarity.VeryRare => 0.01f,
                EventRarity.Legendary => 0.005f,
                _ => 0.05f
            };

            return new WorldEvent(
                id,
                $"Found: {treasureName}",
                $"You've discovered a hidden treasure: {treasureName}!",
                WorldEventType.TreasureFound,
                rarity,
                triggerChance)
            {
                MinPlayerLevel = minLevel,
                DurationSeconds = null // Instant event
            };
        }

        /// <summary>
        /// Create a weather/environmental event
        /// </summary>
        public static WorldEvent CreateWeatherEvent(
            string id,
            string name,
            string description,
            int durationSeconds)
        {
            return new WorldEvent(id, name, description, WorldEventType.WeatherChange, EventRarity.Common, 0.2f)
            {
                MinPlayerLevel = 1,
                DurationSeconds = durationSeconds
            };
        }
    }
}