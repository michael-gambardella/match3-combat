using System;

using Match3.Core.Grid;
using Match3.Core.Matching;

using Xunit;

namespace Match3.Core.Tests.Matching
{
    public class MatchTests
    {
        [Fact]
        public void Constructor_UnorderedPositionsWithDuplicates_StoresDistinctRowMajor()
        {
            var match = new Match(GemColor.Red, new[]
            {
                new Position(1, 0), new Position(0, 2), new Position(0, 0), new Position(1, 0),
            });

            Assert.Equal(new[] { new Position(0, 0), new Position(0, 2), new Position(1, 0) }, match.Positions);
        }

        [Fact]
        public void Constructor_FewerThanThreeDistinctPositions_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new Match(
                GemColor.Red,
                new[] { new Position(0, 0), new Position(0, 1), new Position(0, 1) }));
        }
    }
}