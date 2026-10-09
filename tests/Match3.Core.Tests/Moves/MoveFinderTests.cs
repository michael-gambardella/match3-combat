using System;
using System.Linq;

using Match3.Core.Grid;
using Match3.Core.Matching;
using Match3.Core.Moves;
using Match3.Core.Randomness;

using Xunit;

namespace Match3.Core.Tests.Moves
{
    public class MoveFinderTests
    {
        [Fact]
        public void IsLegal_SwapCompletesRunAtFirstCell_ReturnsTrue()
        {
            Board board = BoardNotation.Parse("""
                RRGR
                YBYB
                """);

            Assert.True(MoveFinder.IsLegal(board, new Move(P(0, 2), P(0, 3))));
        }

        [Fact]
        public void IsLegal_SwapCompletesRunAtSecondCell_ReturnsTrue()
        {
            Board board = BoardNotation.Parse("""
                RGRR
                YBYB
                """);

            Assert.True(MoveFinder.IsLegal(board, new Move(P(0, 0), P(0, 1))));
        }

        [Fact]
        public void IsLegal_SwapCompletesVerticalRun_ReturnsTrue()
        {
            Board board = BoardNotation.Parse("""
                RYB
                GRY
                RBG
                """);

            Assert.True(MoveFinder.IsLegal(board, new Move(P(1, 0), P(1, 1))));
        }

        [Fact]
        public void IsLegal_SwapCreatesNoRun_ReturnsFalse()
        {
            Board board = BoardNotation.Parse("""
                RYB
                YBR
                BRY
                """);

            Assert.False(MoveFinder.IsLegal(board, new Move(P(0, 0), P(0, 1))));
        }

        [Fact]
        public void IsLegal_SameColoredGems_ReturnsFalseEvenInsideAnExistingRun()
        {
            Board board = BoardNotation.Parse("""
                RRRY
                YBGB
                GYBP
                """);

            Assert.False(MoveFinder.IsLegal(board, new Move(P(0, 0), P(0, 1))));
        }

        [Fact]
        public void IsLegal_ExistingMatchElsewhere_DoesNotMakeUnrelatedSwapLegal()
        {
            Board board = BoardNotation.Parse("""
                RRRY
                YBGB
                GYBP
                """);

            Assert.False(MoveFinder.IsLegal(board, new Move(P(2, 2), P(2, 3))));
        }

        [Fact]
        public void IsLegal_CellOffBoard_ReturnsFalse()
        {
            Board board = BoardNotation.Parse("""
                RYB
                YBR
                BRY
                """);

            Assert.False(MoveFinder.IsLegal(board, new Move(P(2, 0), P(3, 0))));
        }

        [Fact]
        public void IsLegal_AgreesWithFullRescan_OnRandomBoards()
        {
            // The fast check only looks at the lines through the swapped cells. This verifies it against
            // the slow definition: swap, run MatchFinder on the whole board, look for a swapped cell.
            for (ulong seed = 0; seed < 200; seed++)
            {
                var random = new SeededRandom(seed);
                Board board = Board.Create(6, 6, _ => GemColors.All[random.NextInt(GemColors.All.Count)]);

                foreach (Move move in AllAdjacentMoves(board))
                {
                    bool expected = board.GemAt(move.First) != board.GemAt(move.Second) &&
                        MatchFinder.FindMatches(board.WithSwap(move.First, move.Second))
                            .Any(match => match.Positions.Contains(move.First) || match.Positions.Contains(move.Second));

                    Assert.True(
                        expected == MoveFinder.IsLegal(board, move),
                        $"Seed {seed}, move {move}: expected {expected} on board\n{board}");
                }
            }
        }

        [Fact]
        public void FindLegalMoves_ReturnsEveryLegalMoveInRowMajorOrder()
        {
            Board board = BoardNotation.Parse("""
                RYBG
                YRGB
                RBYG
                GYRB
                """);

            Assert.Equal(
                new[] { new Move(P(1, 0), P(1, 1)), new Move(P(1, 2), P(1, 3)) },
                MoveFinder.FindLegalMoves(board));
        }

        [Fact]
        public void FindLegalMoves_DeadlockedBoard_ReturnsEmpty()
        {
            Board board = BoardNotation.Parse("""
                RYB
                YBR
                BRY
                """);

            Assert.Empty(MoveFinder.FindLegalMoves(board));
        }

        [Fact]
        public void IsLegal_NullBoard_ThrowsArgumentNull()
        {
            Assert.Throws<ArgumentNullException>(() => MoveFinder.IsLegal(null!, new Move(P(0, 0), P(0, 1))));
        }

        [Fact]
        public void IsLegal_NullMove_ThrowsArgumentNull()
        {
            Assert.Throws<ArgumentNullException>(() => MoveFinder.IsLegal(BoardNotation.Parse("RYB"), null!));
        }

        [Fact]
        public void FindLegalMoves_NullBoard_ThrowsArgumentNull()
        {
            Assert.Throws<ArgumentNullException>(() => MoveFinder.FindLegalMoves(null!));
        }

        private static Position P(int row, int column) => new(row, column);

        private static Move[] AllAdjacentMoves(Board board) =>
            Enumerable.Range(0, board.Height)
                .SelectMany(row => Enumerable.Range(0, board.Width).Select(column => P(row, column)))
                .SelectMany(cell => new[] { P(cell.Row, cell.Column + 1), P(cell.Row + 1, cell.Column) }
                    .Where(board.Contains)
                    .Select(neighbor => new Move(cell, neighbor)))
                .ToArray();
    }
}