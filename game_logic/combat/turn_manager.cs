using System;
using System.Collections.Generic;
using GameLogic.Entities.Player;
using GameLogic.Entities.Enemies;

namespace GameLogic.Combat
{
    /// <summary>
    /// Manages turn order in combat
    /// Determines who acts when during battle
    /// </summary>
    public class TurnManager
    {
        private int _currentTurnNumber;
        private Queue<CombatantTurn> _turnQueue;
        private Player _player;
        private Enemy _enemy;

        public int CurrentTurnNumber => _currentTurnNumber;

        public TurnManager()
        {
            _currentTurnNumber = 0;
            _turnQueue = new Queue<CombatantTurn>();
        }

        /// <summary>
        /// Initialize combat with player and enemy
        /// </summary>
        public void InitializeCombat(Player player, Enemy enemy)
        {
            _player = player;
            _enemy = enemy;
            _currentTurnNumber = 0;
            _turnQueue.Clear();

            // Set up initial turn order
            SetupTurnOrder();
        }

        /// <summary>
        /// Setup turn order - for now, simple alternating turns
        /// TODO: Could be expanded to use speed stats for turn order
        /// </summary>
        private void SetupTurnOrder()
        {
            // Simple alternating: Player always goes first
            // In future, you could roll initiative or use speed stats
            _turnQueue.Enqueue(new CombatantTurn(TurnOwner.Player));
            _turnQueue.Enqueue(new CombatantTurn(TurnOwner.Enemy));
        }

        /// <summary>
        /// Get whose turn it is
        /// </summary>
        public TurnOwner GetCurrentTurnOwner()
        {
            if (_turnQueue.Count == 0)
            {
                // Refill queue for next round
                SetupTurnOrder();
            }

            return _turnQueue.Peek().Owner;
        }

        /// <summary>
        /// Advance to the next turn
        /// </summary>
        public void NextTurn()
        {
            if (_turnQueue.Count > 0)
            {
                _turnQueue.Dequeue();
            }

            // If queue is empty, we've completed a full round
            if (_turnQueue.Count == 0)
            {
                _currentTurnNumber++;
                SetupTurnOrder();
            }
        }

        /// <summary>
        /// Check if it's the player's turn
        /// </summary>
        public bool IsPlayerTurn()
        {
            return GetCurrentTurnOwner() == TurnOwner.Player;
        }

        /// <summary>
        /// Check if it's the enemy's turn
        /// </summary>
        public bool IsEnemyTurn()
        {
            return GetCurrentTurnOwner() == TurnOwner.Enemy;
        }

        /// <summary>
        /// Reset turn manager for new combat
        /// </summary>
        public void Reset()
        {
            _currentTurnNumber = 0;
            _turnQueue.Clear();
            _player = null;
            _enemy = null;
        }

        /// <summary>
        /// Get turn number for display
        /// </summary>
        public string GetTurnStatus()
        {
            TurnOwner currentOwner = GetCurrentTurnOwner();
            string ownerName = currentOwner == TurnOwner.Player ? _player?.Name : _enemy?.Name;
            
            return $"Turn {_currentTurnNumber + 1} - {ownerName}'s turn";
        }

        // === FUTURE ENHANCEMENTS ===
        // Uncomment and expand these for more complex turn systems

        /*
        /// <summary>
        /// Initialize combat with speed-based turn order
        /// Faster combatants go first
        /// </summary>
        private void SetupSpeedBasedTurnOrder()
        {
            // Compare speed stats
            int playerSpeed = _player.Speed; // You'd need to add Speed stat
            int enemySpeed = _enemy.Speed;
            
            if (playerSpeed >= enemySpeed)
            {
                _turnQueue.Enqueue(new CombatantTurn(TurnOwner.Player));
                _turnQueue.Enqueue(new CombatantTurn(TurnOwner.Enemy));
            }
            else
            {
                _turnQueue.Enqueue(new CombatantTurn(TurnOwner.Enemy));
                _turnQueue.Enqueue(new CombatantTurn(TurnOwner.Player));
            }
        }
        */

        /*
        /// <summary>
        /// Handle multi-enemy combat (future feature)
        /// </summary>
        private void SetupMultiEnemyTurnOrder(List<Enemy> enemies)
        {
            // Player turn
            _turnQueue.Enqueue(new CombatantTurn(TurnOwner.Player));
            
            // Each enemy gets a turn
            foreach (var enemy in enemies)
            {
                _turnQueue.Enqueue(new CombatantTurn(TurnOwner.Enemy, enemy.Name));
            }
        }
        */
    }

    /// <summary>
    /// Represents who owns a turn
    /// </summary>
    public enum TurnOwner
    {
        Player,
        Enemy
    }

    /// <summary>
    /// Represents a single turn in the queue
    /// </summary>
    public class CombatantTurn
    {
        public TurnOwner Owner { get; set; }
        public string SpecificCombatantName { get; set; } // For multi-enemy fights

        public CombatantTurn(TurnOwner owner, string specificName = null)
        {
            Owner = owner;
            SpecificCombatantName = specificName;
        }
    }
}
