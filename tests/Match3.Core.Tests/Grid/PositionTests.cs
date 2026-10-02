using System;

using Match3.Core.Grid;

using Xunit;

namespace Match3.Core.Tests.Grid
{
    public class PositionTests
    {
        [Fact]
        public void Equals_SameRowAndColumn_ReturnsTrue()
        {
            var a = new Position(2, 3);
            var b = new Position(2, 3);

            Assert.True(a == b);
            Assert.Equal(a, b);
        }

        [Fact]
        public void Equals_DifferentColumn_ReturnsFalse()
        {
            var a = new Position(2, 3);
            var b = new Position(2, 4);

            Assert.True(a != b);
            Assert.NotEqual(a, b);
        }

        [Fact]
        public void GetHashCode_EqualPositions_ReturnSameValue()
        {
            Assert.Equal(new Position(5, 1).GetHashCode(), new Position(5, 1).GetHashCode());
        }

        [Fact]
        public void ToString_FormatsAsRowThenColumn()
        {
            Assert.Equal("(2, 3)", new Position(2, 3).ToString());
        }

        [Fact]
        public void RowMajor_OrdersByRowThenColumn()
        {
            var positions = new[] { new Position(1, 0), new Position(0, 2), new Position(0, 1) };

            Array.Sort(positions, Position.RowMajor);

            Assert.Equal(new[] { new Position(0, 1), new Position(0, 2), new Position(1, 0) }, positions);
        }
    }
}