using System;

using Match3.Core.Grid;
using Match3.Core.Moves;

using Xunit;

namespace Match3.Core.Tests.Moves
{
    public class MoveTests
    {
        [Fact]
        public void Constructor_CellsInEitherOrder_StoresThemRowMajor()
        {
            var move = new Move(P(1, 2), P(1, 1));

            Assert.Equal(P(1, 1), move.First);
            Assert.Equal(P(1, 2), move.Second);
        }

        [Fact]
        public void Equals_SameCellsInEitherOrder_AreEqualWithSameHashCode()
        {
            var a = new Move(P(0, 0), P(1, 0));
            var b = new Move(P(1, 0), P(0, 0));

            Assert.Equal(a, b);
            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }

        [Theory]
        [InlineData(0, 0, 0, 0)]
        [InlineData(0, 0, 1, 1)]
        [InlineData(0, 0, 0, 2)]
        public void Constructor_CellsNotOrthogonallyAdjacent_ThrowsArgumentException(int row1, int column1, int row2, int column2)
        {
            Assert.Throws<ArgumentException>(() => new Move(P(row1, column1), P(row2, column2)));
        }

        [Theory]
        [InlineData(0, 1, true)]
        [InlineData(1, 0, false)]
        public void IsHorizontal_ReportsWhetherCellsShareARow(int row, int column, bool expected)
        {
            Assert.Equal(expected, new Move(P(0, 0), P(row, column)).IsHorizontal);
        }

        private static Position P(int row, int column) => new(row, column);
    }
}