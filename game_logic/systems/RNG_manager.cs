using System;
using System.Collections.Generic;
using System.Linq;

namespace GameLogic.Systems
{
    /// <summary>
    /// Interface for RNG implementations
    /// Implement this interface to add new RNG algorithms later
    /// </summary>
    public interface IRNGAlgorithm
    {
        /// <summary>
        /// Generate a random integer in the range [minValue, maxValue)
        /// </summary>
        int Next(int minValue, int maxValue);

        /// <summary>
        /// Generate a random double in the range [0.0, 1.0)
        /// </summary>
        double NextDouble();

        /// <summary>
        /// Generate random bytes
        /// </summary>
        void NextBytes(byte[] buffer);

        /// <summary>
        /// Get the name of this RNG algorithm
        /// </summary>
        string GetAlgorithmName();
    }

    /// <summary>
    /// Default RNG implementation using System.Random
    /// This is the current implementation for testing/playability
    /// </summary>
    public class SystemRandomAlgorithm : IRNGAlgorithm
    {
        private readonly Random _random;

        public SystemRandomAlgorithm(int? seed = null)
        {
            _random = seed.HasValue ? new Random(seed.Value) : new Random();
        }

        public int Next(int minValue, int maxValue)
        {
            return _random.Next(minValue, maxValue);
        }

        public double NextDouble()
        {
            return _random.NextDouble();
        }

        public void NextBytes(byte[] buffer)
        {
            _random.NextBytes(buffer);
        }

        public string GetAlgorithmName()
        {
            return "System.Random";
        }
    }

    /// <summary>
    /// Example placeholder for future Mersenne Twister implementation
    /// Uncomment and implement when you're ready to add more RNG algorithms
    /// </summary>
    /*
    public class MersenneTwisterAlgorithm : IRNGAlgorithm
    {
        // TODO: Implement Mersenne Twister algorithm
        public int Next(int minValue, int maxValue) { throw new NotImplementedException(); }
        public double NextDouble() { throw new NotImplementedException(); }
        public void NextBytes(byte[] buffer) { throw new NotImplementedException(); }
        public string GetAlgorithmName() { return "Mersenne Twister"; }
    }
    */

    /// <summary>
    /// Example placeholder for future Xorshift implementation
    /// </summary>
    /*
    public class XorshiftAlgorithm : IRNGAlgorithm
    {
        // TODO: Implement Xorshift algorithm
        public int Next(int minValue, int maxValue) { throw new NotImplementedException(); }
        public double NextDouble() { throw new NotImplementedException(); }
        public void NextBytes(byte[] buffer) { throw new NotImplementedException(); }
        public string GetAlgorithmName() { return "Xorshift"; }
    }
    */

    /// <summary>
    /// Central RNG manager for the entire game
    /// Provides unified interface for all random number generation
    /// Can swap RNG algorithms without changing game code
    /// </summary>
    public class RNGManager
    {
        private IRNGAlgorithm _algorithm;
        private readonly Dictionary<string, IRNGAlgorithm> _availableAlgorithms;

        // Statistics tracking (optional, for debugging/analysis)
        private long _totalRolls;
        private bool _trackStatistics;

        /// <summary>
        /// Get the current RNG algorithm being used
        /// </summary>
        public string CurrentAlgorithm => _algorithm.GetAlgorithmName();

        /// <summary>
        /// Get total number of rolls made (if statistics tracking is enabled)
        /// </summary>
        public long TotalRolls => _totalRolls;

        public RNGManager(int? seed = null, bool trackStatistics = false)
        {
            _availableAlgorithms = new Dictionary<string, IRNGAlgorithm>();
            _trackStatistics = trackStatistics;
            _totalRolls = 0;

            // Register System.Random as default algorithm
            var systemRandom = new SystemRandomAlgorithm(seed);
            RegisterAlgorithm("system", systemRandom);

            // Set System.Random as the active algorithm
            _algorithm = systemRandom;

            // Future algorithms can be registered here:
            // RegisterAlgorithm("mersenne", new MersenneTwisterAlgorithm());
            // RegisterAlgorithm("xorshift", new XorshiftAlgorithm());
        }

        #region Algorithm Management

        /// <summary>
        /// Register a new RNG algorithm
        /// Use this to add custom RNG implementations later
        /// </summary>
        public void RegisterAlgorithm(string name, IRNGAlgorithm algorithm)
        {
            _availableAlgorithms[name.ToLower()] = algorithm;
        }

        /// <summary>
        /// Switch to a different registered RNG algorithm
        /// </summary>
        public bool SwitchAlgorithm(string algorithmName)
        {
            if (_availableAlgorithms.TryGetValue(algorithmName.ToLower(), out IRNGAlgorithm algorithm))
            {
                _algorithm = algorithm;
                Console.WriteLine($"Switched to RNG algorithm: {_algorithm.GetAlgorithmName()}");
                return true;
            }

            Console.WriteLine($"RNG algorithm '{algorithmName}' not found.");
            return false;
        }

        /// <summary>
        /// Get list of available RNG algorithms
        /// </summary>
        public List<string> GetAvailableAlgorithms()
        {
            return _availableAlgorithms.Keys.ToList();
        }

        #endregion

        #region Core RNG Methods

        /// <summary>
        /// Generate a random integer in the range [min, max] (inclusive on both ends)
        /// This is the primary RNG method used throughout the game
        /// </summary>
        public int Roll(int min, int max)
        {
            if (_trackStatistics) _totalRolls++;
            return _algorithm.Next(min, max + 1);
        }

        /// <summary>
        /// Generate a random integer in the range [min, max) (exclusive max)
        /// </summary>
        public int Next(int min, int max)
        {
            if (_trackStatistics) _totalRolls++;
            return _algorithm.Next(min, max);
        }

        /// <summary>
        /// Generate a random integer in the range [0, max) (exclusive max)
        /// </summary>
        public int Next(int max)
        {
            if (_trackStatistics) _totalRolls++;
            return _algorithm.Next(0, max);
        }

        /// <summary>
        /// Generate a random double in the range [0.0, 1.0)
        /// </summary>
        public double NextDouble()
        {
            if (_trackStatistics) _totalRolls++;
            return _algorithm.NextDouble();
        }

        /// <summary>
        /// Generate a random float in the range [0.0, 1.0)
        /// </summary>
        public float NextFloat()
        {
            if (_trackStatistics) _totalRolls++;
            return (float)_algorithm.NextDouble();
        }

        /// <summary>
        /// Generate random bytes
        /// </summary>
        public void NextBytes(byte[] buffer)
        {
            if (_trackStatistics) _totalRolls++;
            _algorithm.NextBytes(buffer);
        }

        #endregion

        #region Specialized RNG Methods

        /// <summary>
        /// Generate a random boolean (50/50 chance)
        /// </summary>
        public bool NextBool()
        {
            if (_trackStatistics) _totalRolls++;
            return _algorithm.Next(0, 2) == 0;
        }

        /// <summary>
        /// Generate a random boolean with a specific probability
        /// </summary>
        /// <param name="probability">Probability of returning true (0.0 to 1.0)</param>
        public bool NextBool(float probability)
        {
            if (_trackStatistics) _totalRolls++;
            return _algorithm.NextDouble() < probability;
        }

        /// <summary>
        /// Roll a percentage chance (0-100). Returns true if roll succeeds.
        /// Example: RollPercentage(75) has 75% chance to return true
        /// </summary>
        public bool RollPercentage(int successChance)
        {
            if (_trackStatistics) _totalRolls++;
            return Roll(1, 100) <= successChance;
        }

        /// <summary>
        /// Roll a percentage chance with float precision (0.0-1.0)
        /// Example: RollChance(0.75f) has 75% chance to return true
        /// </summary>
        public bool RollChance(float chance)
        {
            if (_trackStatistics) _totalRolls++;
            return _algorithm.NextDouble() <= chance;
        }

        /// <summary>
        /// Roll multiple dice and return the sum
        /// Example: RollDice(3, 6) rolls 3d6
        /// </summary>
        public int RollDice(int count, int sides)
        {
            int total = 0;
            for (int i = 0; i < count; i++)
            {
                total += Roll(1, sides);
            }
            return total;
        }

        /// <summary>
        /// Roll a d20 (common in RPGs)
        /// </summary>
        public int RollD20()
        {
            return Roll(1, 20);
        }

        /// <summary>
        /// Roll a d100 (percentile dice)
        /// </summary>
        public int RollD100()
        {
            return Roll(1, 100);
        }

        /// <summary>
        /// Generate a random float in the range [min, max]
        /// </summary>
        public float Range(float min, float max)
        {
            if (_trackStatistics) _totalRolls++;
            return min + (float)_algorithm.NextDouble() * (max - min);
        }

        /// <summary>
        /// Generate a random integer in the range [min, max]
        /// </summary>
        public int Range(int min, int max)
        {
            return Roll(min, max);
        }

        #endregion

        #region Weighted Random Selection

        /// <summary>
        /// Select a random index based on weights
        /// Example: weights = [10, 20, 70] means 10% chance index 0, 20% chance index 1, 70% chance index 2
        /// </summary>
        public int SelectWeightedIndex(int[] weights)
        {
            if (weights == null || weights.Length == 0)
                throw new ArgumentException("Weights array cannot be null or empty");

            int totalWeight = weights.Sum();
            if (totalWeight <= 0)
                throw new ArgumentException("Total weight must be greater than 0");

            int roll = Roll(0, totalWeight - 1);
            int currentWeight = 0;

            for (int i = 0; i < weights.Length; i++)
            {
                currentWeight += weights[i];
                if (roll < currentWeight)
                {
                    return i;
                }
            }

            return weights.Length - 1; // Fallback
        }

        /// <summary>
        /// Select a random item from a list based on weights
        /// </summary>
        public T SelectWeightedItem<T>(List<T> items, List<int> weights)
        {
            if (items == null || weights == null)
                throw new ArgumentNullException("Items and weights cannot be null");

            if (items.Count != weights.Count)
                throw new ArgumentException("Items and weights must have the same count");

            int index = SelectWeightedIndex(weights.ToArray());
            return items[index];
        }

        #endregion

        #region Collection Shuffling and Selection

        /// <summary>
        /// Shuffle a list in-place using Fisher-Yates algorithm
        /// </summary>
        public void Shuffle<T>(List<T> list)
        {
            int n = list.Count;
            for (int i = n - 1; i > 0; i--)
            {
                int j = Next(0, i + 1);
                // Swap
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        /// <summary>
        /// Get a random item from a list
        /// </summary>
        public T SelectRandom<T>(List<T> list)
        {
            if (list == null || list.Count == 0)
                throw new ArgumentException("List cannot be null or empty");

            return list[Next(0, list.Count)];
        }

        /// <summary>
        /// Get a random item from an array
        /// </summary>
        public T SelectRandom<T>(T[] array)
        {
            if (array == null || array.Length == 0)
                throw new ArgumentException("Array cannot be null or empty");

            return array[Next(0, array.Length)];
        }

        /// <summary>
        /// Get multiple random unique items from a list (without replacement)
        /// </summary>
        public List<T> SelectRandomUnique<T>(List<T> list, int count)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            if (count > list.Count)
                throw new ArgumentException("Cannot select more items than available in list");

            // Create a copy and shuffle
            var copy = new List<T>(list);
            Shuffle(copy);

            // Take the first 'count' items
            return copy.Take(count).ToList();
        }

        #endregion

        #region Statistics and Debugging

        /// <summary>
        /// Enable or disable statistics tracking
        /// </summary>
        public void SetStatisticsTracking(bool enabled)
        {
            _trackStatistics = enabled;
        }

        /// <summary>
        /// Reset statistics counter
        /// </summary>
        public void ResetStatistics()
        {
            _totalRolls = 0;
        }

        /// <summary>
        /// Get RNG manager info for debugging
        /// </summary>
        public string GetInfo()
        {
            string info = $"═══ RNG Manager Info ═══\n";
            info += $"Current Algorithm: {_algorithm.GetAlgorithmName()}\n";
            info += $"Available Algorithms: {string.Join(", ", _availableAlgorithms.Keys)}\n";

            if (_trackStatistics)
            {
                info += $"Total Rolls: {_totalRolls:N0}\n";
            }
            else
            {
                info += "Statistics Tracking: Disabled\n";
            }

            return info;
        }

        #endregion
    }
}