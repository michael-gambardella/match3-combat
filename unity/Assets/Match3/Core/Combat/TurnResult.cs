using Match3.Core.Cascades;
using Match3.Core.Grid;
using Match3.Core.Moves;

namespace Match3.Core.Combat
{
    /// <summary>Everything that happened during one move: what the presentation layer animates.</summary>
    public sealed class TurnResult
    {
        internal TurnResult(
            Move move, CascadeResult cascade, ActionPoints actionPointsGained, int damageDealt, Board? reshuffledBoard, GameStatus status)
        {
            Move = move;
            Cascade = cascade;
            ActionPointsGained = actionPointsGained;
            DamageDealt = damageDealt;
            ReshuffledBoard = reshuffledBoard;
            Status = status;
        }

        /// <summary>The move that was played.</summary>
        public Move Move { get; }

        /// <summary>Every cascade step the move caused.</summary>
        public CascadeResult Cascade { get; }

        /// <summary>AP earned this turn: one per cleared gem, of that gem's color.</summary>
        public ActionPoints ActionPointsGained { get; }

        /// <summary>Damage dealt to the enemy this turn.</summary>
        public int DamageDealt { get; }

        /// <summary>The new board if the settled board was deadlocked and had to be regenerated; otherwise null.</summary>
        public Board? ReshuffledBoard { get; }

        /// <summary>The encounter's status after the move.</summary>
        public GameStatus Status { get; }
    }
}