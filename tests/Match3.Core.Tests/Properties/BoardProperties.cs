using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;

using Match3.Core.Grid;

using Xunit;

namespace Match3.Core.Tests.Properties
{
    public class BoardProperties
    {
        [Property]
        public Property Notation_FormatThenParse_ReturnsAnEqualBoard() =>
            Prop.ForAll(GameArbitraries.Boards(), board =>
                Assert.Equal(board, BoardNotation.Parse(BoardNotation.Format(board))));

        [Property]
        public Property WithSwap_AppliedTwice_RestoresTheBoard() =>
            Prop.ForAll(GameArbitraries.Boards(), board =>
            {
                var first = new Position(0, 0);
                var last = new Position(board.Height - 1, board.Width - 1);

                Assert.Equal(board, board.WithSwap(first, last).WithSwap(first, last));
            });
    }
}