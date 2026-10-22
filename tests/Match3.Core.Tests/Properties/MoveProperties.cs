using System.Linq;

using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;

using Match3.Core.Matching;
using Match3.Core.Moves;

using Xunit;

namespace Match3.Core.Tests.Properties
{
    public class MoveProperties
    {
        [Property]
        public Property IsLegal_AgreesWithAFullRescanOfTheSwappedBoard() =>
            Prop.ForAll(GameArbitraries.Boards(), board =>
            {
                // The fast check only examines the lines through the swapped cells. This compares it with the
                // slow definition: swap, run MatchFinder on the whole board, look for a match through a swapped cell.
                foreach (Move move in board.AdjacentMoves())
                {
                    bool expected = board.GemAt(move.First) != board.GemAt(move.Second) &&
                        MatchFinder.FindMatches(board.WithSwap(move.First, move.Second))
                            .Any(match => match.Positions.Contains(move.First) || match.Positions.Contains(move.Second));

                    Assert.True(
                        expected == MoveFinder.IsLegal(board, move),
                        $"Move {move}: expected {expected} on board\n{board}");
                }
            });

        [Property]
        public Property FindLegalMoves_IsExactlyTheLegalAdjacentMovesInOrder() =>
            Prop.ForAll(GameArbitraries.Boards(), board =>
                Assert.Equal(
                    board.AdjacentMoves().Where(move => MoveFinder.IsLegal(board, move)),
                    MoveFinder.FindLegalMoves(board)));
    }
}