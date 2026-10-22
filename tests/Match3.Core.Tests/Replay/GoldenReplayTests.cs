using System.Collections.Generic;

using Match3.Core.Cascades;
using Match3.Core.Generation;
using Match3.Core.Grid;
using Match3.Core.Moves;
using Match3.Core.Randomness;

using Xunit;

namespace Match3.Core.Tests.Replay
{
    public class GoldenReplayTests
    {
        /// <summary>
        /// Plays a full seeded game (generation, random legal moves, cascades, and reshuffles on deadlock)
        /// and pins the outcome. The expected values come from an independent implementation of the rules.
        /// </summary>
        /// <remarks>
        /// If this fails, a change altered game outcomes, and every replay recorded with an earlier build
        /// would now play out differently. Update the expected values only for an intentional rule change,
        /// and say so in the pull request.
        /// </remarks>
        [Fact]
        public void SeededGame_ThirtyTurns_PlaysOutExactlyAsRecorded()
        {
            var random = new SeededRandom(2026);
            Board board = BoardGenerator.Generate(8, 8, random);
            int cascadeSteps = 0;
            int gemsCleared = 0;
            int reshuffles = 0;

            for (int turn = 0; turn < 30; turn++)
            {
                IReadOnlyList<Move> moves = MoveFinder.FindLegalMoves(board);
                Move move = moves[random.NextInt(moves.Count)];

                CascadeResult result = CascadeResolver.Resolve(board.WithSwap(move.First, move.Second), random);
                foreach (CascadeStep step in result.Steps)
                {
                    cascadeSteps++;
                    gemsCleared += step.Spawns.Count;
                }

                board = result.FinalBoard;
                if (MoveFinder.FindLegalMoves(board).Count == 0)
                {
                    board = BoardGenerator.Generate(8, 8, random);
                    reshuffles++;
                }
            }

            Assert.Equal(37, cascadeSteps);
            Assert.Equal(122, gemsCleared);
            Assert.Equal(0, reshuffles);
            Assert.Equal(
                BoardNotation.Format(BoardNotation.Parse("""
                    BGBKGBKG
                    BRKPKPBP
                    PKGPYRKY
                    PGRKGKPR
                    KRKYRYBG
                    YGGPYPPR
                    KBYBGKYY
                    RPPBYRPG
                    """)),
                board.ToString());
        }
    }
}