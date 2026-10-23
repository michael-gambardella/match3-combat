using System;

namespace Match3.Core.Combat
{
    /// <summary>Designer-tunable numbers for one encounter.</summary>
    public sealed class CombatRules
    {
        /// <summary>The smallest supported board side. Smaller boards cannot reliably hold a legal move.</summary>
        public const int MinimumBoardSize = 3;

        /// <summary>Creates a rule set.</summary>
        /// <param name="boardWidth">Columns; at least <see cref="MinimumBoardSize"/>.</param>
        /// <param name="boardHeight">Rows; at least <see cref="MinimumBoardSize"/>.</param>
        /// <param name="enemyMaxHealth">The enemy's starting health; must be positive.</param>
        /// <param name="damagePerGem">Damage dealt for every gem cleared; must not be negative.</param>
        /// <param name="turnLimit">Moves allowed before the encounter is lost; must be positive.</param>
        /// <exception cref="ArgumentOutOfRangeException">A value is out of range.</exception>
        public CombatRules(int boardWidth, int boardHeight, int enemyMaxHealth, int damagePerGem, int turnLimit)
        {
            if (boardWidth < MinimumBoardSize)
            {
                throw new ArgumentOutOfRangeException(nameof(boardWidth), boardWidth, "Board is too narrow.");
            }

            if (boardHeight < MinimumBoardSize)
            {
                throw new ArgumentOutOfRangeException(nameof(boardHeight), boardHeight, "Board is too short.");
            }

            if (enemyMaxHealth <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(enemyMaxHealth), enemyMaxHealth, "Enemy health must be positive.");
            }

            if (damagePerGem < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(damagePerGem), damagePerGem, "Damage cannot be negative.");
            }

            if (turnLimit <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(turnLimit), turnLimit, "Turn limit must be positive.");
            }

            BoardWidth = boardWidth;
            BoardHeight = boardHeight;
            EnemyMaxHealth = enemyMaxHealth;
            DamagePerGem = damagePerGem;
            TurnLimit = turnLimit;
        }

        /// <summary>Columns on the board.</summary>
        public int BoardWidth { get; }

        /// <summary>Rows on the board.</summary>
        public int BoardHeight { get; }

        /// <summary>The enemy's starting health.</summary>
        public int EnemyMaxHealth { get; }

        /// <summary>Damage dealt for every gem cleared, including gems cleared by cascades.</summary>
        public int DamagePerGem { get; }

        /// <summary>Moves allowed before the encounter is lost.</summary>
        public int TurnLimit { get; }
    }
}