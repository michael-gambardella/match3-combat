using System;
using System.Collections.Generic;

using Match3.Core.Grid;
using Match3.Core.Matching;

namespace Match3.Core.Moves
{
    /// <summary>Decides which swaps are legal, for move validation, hints, and deadlock detection.</summary>
    public static class MoveFinder
    {
        // A cell plus this many matching neighbors along one line forms a run of Match.MinimumLength.
        private const int NeighborsNeeded = Match.MinimumLength - 1;

        /// <summary>
        /// Returns true when swapping the move's gems would create a run of at least three through
        /// at least one of the two swapped cells.
        /// </summary>
        /// <remarks>
        /// A move is not legal when either cell is off the board, or when both gems have the same color
        /// (the swap would not change the board). Matches elsewhere on the board do not make a move legal.
        /// Only the row and column through each swapped cell are examined, so this runs in constant time.
        /// </remarks>
        /// <exception cref="ArgumentNullException">An argument is null.</exception>
        public static bool IsLegal(Board board, Move move)
        {
            if (board is null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            if (move is null)
            {
                throw new ArgumentNullException(nameof(move));
            }

            if (!board.Contains(move.First) || !board.Contains(move.Second))
            {
                return false;
            }

            if (board.GemAt(move.First) == board.GemAt(move.Second))
            {
                return false;
            }

            var swapped = new SwappedBoard(board, move);
            return swapped.HasRunThrough(move.First) || swapped.HasRunThrough(move.Second);
        }

        /// <summary>Returns every legal move on the board; an empty list means the board is deadlocked.</summary>
        /// <returns>Moves ordered by <see cref="Move.First"/>, then <see cref="Move.Second"/> (row-major).</returns>
        /// <remarks>Checks each cell's right and lower neighbor once, so the total cost is O(W×H).</remarks>
        /// <exception cref="ArgumentNullException"><paramref name="board"/> is null.</exception>
        public static IReadOnlyList<Move> FindLegalMoves(Board board)
        {
            if (board is null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            var moves = new List<Move>();
            for (int row = 0; row < board.Height; row++)
            {
                for (int column = 0; column < board.Width; column++)
                {
                    var cell = new Position(row, column);
                    AddIfLegal(board, moves, cell, new Position(row, column + 1));
                    AddIfLegal(board, moves, cell, new Position(row + 1, column));
                }
            }

            return moves.AsReadOnly();
        }

        private static void AddIfLegal(Board board, List<Move> moves, Position cell, Position neighbor)
        {
            if (!board.Contains(neighbor))
            {
                return;
            }

            var move = new Move(cell, neighbor);
            if (IsLegal(board, move))
            {
                moves.Add(move);
            }
        }

        /// <summary>
        /// A read-only view of a board as it would look after a move, without copying the board.
        /// A struct so the many checks made by <see cref="FindLegalMoves"/> do not allocate.
        /// </summary>
        private readonly struct SwappedBoard
        {
            private readonly Board _board;
            private readonly Move _move;

            public SwappedBoard(Board board, Move move)
            {
                _board = board;
                _move = move;
            }

            /// <summary>True when <paramref name="cell"/> lies on a horizontal or vertical run of at least <see cref="Match.MinimumLength"/>.</summary>
            public bool HasRunThrough(Position cell)
            {
                GemColor color = GemAt(cell);
                return CountMatching(cell, color, 0, -1) + CountMatching(cell, color, 0, 1) >= NeighborsNeeded
                    || CountMatching(cell, color, -1, 0) + CountMatching(cell, color, 1, 0) >= NeighborsNeeded;
            }

            private GemColor GemAt(Position position)
            {
                if (position == _move.First)
                {
                    return _board.GemAt(_move.Second);
                }

                if (position == _move.Second)
                {
                    return _board.GemAt(_move.First);
                }

                return _board.GemAt(position);
            }

            /// <summary>
            /// Counts gems of <paramref name="color"/> next to <paramref name="origin"/> in one direction,
            /// stopping at <see cref="NeighborsNeeded"/> because more cannot change the answer.
            /// </summary>
            private int CountMatching(Position origin, GemColor color, int rowStep, int columnStep)
            {
                int count = 0;
                var position = new Position(origin.Row + rowStep, origin.Column + columnStep);
                while (count < NeighborsNeeded && _board.Contains(position) && GemAt(position) == color)
                {
                    count++;
                    position = new Position(position.Row + rowStep, position.Column + columnStep);
                }

                return count;
            }
        }
    }
}