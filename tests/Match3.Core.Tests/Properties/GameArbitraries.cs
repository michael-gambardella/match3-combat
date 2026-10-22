using System.Collections.Generic;
using System.Linq;

using FsCheck;
using FsCheck.Fluent;

using Match3.Core.Grid;

namespace Match3.Core.Tests.Properties
{
    /// <summary>FsCheck generators for game types.</summary>
    internal static class GameArbitraries
    {
        /// <summary>The largest width and height of a generated board.</summary>
        public const int MaxBoardSize = 8;

        /// <summary>
        /// Boards from 1x1 to 8x8. Each board uses a random subset of two to six colors: fewer colors make
        /// runs and cascades common, so most cases exercise the interesting code paths. On failure, boards
        /// shrink by dropping edge rows and columns, so the reported counterexample is as small as possible.
        /// </summary>
        public static Arbitrary<Board> Boards() => Arb.From(RandomBoards(), Shrink);

        /// <summary>Seeds for <see cref="Match3.Core.Randomness.SeededRandom"/>.</summary>
        public static Arbitrary<ulong> Seeds() =>
            Gen.Choose(0, int.MaxValue).Select(seed => (ulong)seed).ToArbitrary();

        /// <summary>Board sizes that can always hold a legal move: 3x3 to 10x10.</summary>
        public static Arbitrary<(int Width, int Height)> PlayableSizes() =>
            (from width in Gen.Choose(3, 10)
             from height in Gen.Choose(3, 10)
             select (width, height)).ToArbitrary();

        private static Gen<Board> RandomBoards() =>
            from width in Gen.Choose(1, MaxBoardSize)
            from height in Gen.Choose(1, MaxBoardSize)
            from colorCount in Gen.Choose(2, GemColors.All.Count)
            from gems in Gen.Elements(GemColors.All.Take(colorCount)).ArrayOf(width * height)
            select Board.Create(width, height, position => gems[(position.Row * width) + position.Column]);

        private static IEnumerable<Board> Shrink(Board board)
        {
            if (board.Height > 1)
            {
                yield return Crop(board, rowOffset: 1, columnOffset: 0, board.Width, board.Height - 1);
                yield return Crop(board, rowOffset: 0, columnOffset: 0, board.Width, board.Height - 1);
            }

            if (board.Width > 1)
            {
                yield return Crop(board, rowOffset: 0, columnOffset: 1, board.Width - 1, board.Height);
                yield return Crop(board, rowOffset: 0, columnOffset: 0, board.Width - 1, board.Height);
            }
        }

        private static Board Crop(Board board, int rowOffset, int columnOffset, int width, int height) =>
            Board.Create(width, height, position =>
                board.GemAt(new Position(position.Row + rowOffset, position.Column + columnOffset)));
    }
}