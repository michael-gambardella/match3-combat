using System.Collections.Generic;
using System.Linq;

using Match3.Core.Grid;
using Match3.Core.Moves;

namespace Match3.Core.Tests.Properties
{
    /// <summary>Enumerations over a board used by property tests to compute expected results by brute force.</summary>
    internal static class BoardTestExtensions
    {
        /// <summary>Every position on the board, in row-major order.</summary>
        public static IEnumerable<Position> AllPositions(this Board board) =>
            Enumerable.Range(0, board.Height)
                .SelectMany(row => Enumerable.Range(0, board.Width).Select(column => new Position(row, column)));

        /// <summary>Every swap of orthogonal neighbors, in the order <see cref="MoveFinder.FindLegalMoves"/> reports moves.</summary>
        public static IEnumerable<Move> AdjacentMoves(this Board board) =>
            board.AllPositions().SelectMany(cell =>
                new[] { new Position(cell.Row, cell.Column + 1), new Position(cell.Row + 1, cell.Column) }
                    .Where(board.Contains)
                    .Select(neighbor => new Move(cell, neighbor)));
    }
}