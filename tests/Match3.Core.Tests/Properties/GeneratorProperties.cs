using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;

using Match3.Core.Generation;
using Match3.Core.Grid;
using Match3.Core.Matching;
using Match3.Core.Moves;
using Match3.Core.Randomness;

using Xunit;

namespace Match3.Core.Tests.Properties
{
    public class GeneratorProperties
    {
        [Property]
        public Property Generate_ProducesAPlayableBoardOfTheRequestedSize() =>
            Prop.ForAll(GameArbitraries.PlayableSizes(), GameArbitraries.Seeds(), (size, seed) =>
            {
                Board board = BoardGenerator.Generate(size.Width, size.Height, new SeededRandom(seed));

                Assert.Equal(size.Width, board.Width);
                Assert.Equal(size.Height, board.Height);
                Assert.Empty(MatchFinder.FindMatches(board));
                Assert.NotEmpty(MoveFinder.FindLegalMoves(board));
            });

        [Property]
        public Property Generate_SameSeed_ProducesTheSameBoard() =>
            Prop.ForAll(GameArbitraries.PlayableSizes(), GameArbitraries.Seeds(), (size, seed) =>
                Assert.Equal(
                    BoardGenerator.Generate(size.Width, size.Height, new SeededRandom(seed)),
                    BoardGenerator.Generate(size.Width, size.Height, new SeededRandom(seed))));
    }
}