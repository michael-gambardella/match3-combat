using System;

using Match3.Core.Grid;

using Xunit;

namespace Match3.Core.Tests.Grid
{
    public class BoardTests
    {
        [Theory]
        [InlineData(0, 3)]
        [InlineData(3, 0)]
        [InlineData(-1, 3)]
        public void Create_NonPositiveDimension_ThrowsArgumentOutOfRange(int width, int height)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Board.Create(width, height, _ => GemColor.Red));
        }

        [Fact]
        public void Create_UndefinedColor_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => Board.Create(2, 2, _ => (GemColor)99));
        }

        [Theory]
        [InlineData(0, 0, true)]
        [InlineData(1, 2, true)]
        [InlineData(-1, 0, false)]
        [InlineData(0, -1, false)]
        [InlineData(2, 0, false)]
        [InlineData(0, 3, false)]
        public void Contains_ReportsWhetherPositionIsOnBoard(int row, int column, bool expected)
        {
            Board board = BoardNotation.Parse("RYB\nGPK");

            Assert.Equal(expected, board.Contains(new Position(row, column)));
        }

        [Fact]
        public void GemAt_OffBoard_ThrowsArgumentOutOfRange()
        {
            Board board = BoardNotation.Parse("RY\nBG");

            Assert.Throws<ArgumentOutOfRangeException>(() => board.GemAt(new Position(2, 0)));
        }

        [Fact]
        public void WithSwap_ExchangesTheTwoGems()
        {
            Board board = BoardNotation.Parse("RY\nBG");

            Board swapped = board.WithSwap(new Position(0, 0), new Position(0, 1));

            Assert.Equal("YR\nBG", swapped.ToString());
        }

        [Fact]
        public void WithSwap_LeavesOriginalBoardUnchanged()
        {
            Board board = BoardNotation.Parse("RY\nBG");

            _ = board.WithSwap(new Position(0, 0), new Position(1, 1));

            Assert.Equal("RY\nBG", board.ToString());
        }

        [Fact]
        public void WithSwap_OffBoardPosition_ThrowsArgumentOutOfRange()
        {
            Board board = BoardNotation.Parse("RY\nBG");

            Assert.Throws<ArgumentOutOfRangeException>(() => board.WithSwap(new Position(0, 0), new Position(0, 5)));
        }

        [Fact]
        public void Equals_SameDimensionsAndGems_AreEqualWithSameHashCode()
        {
            Board a = BoardNotation.Parse("RY\nBG");
            Board b = BoardNotation.Parse("RY\nBG");

            Assert.Equal(a, b);
            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }

        [Fact]
        public void Equals_DifferentGems_AreNotEqual()
        {
            Assert.NotEqual(BoardNotation.Parse("RY\nBG"), BoardNotation.Parse("RY\nBK"));
        }

        [Fact]
        public void Equals_SameGemsDifferentShape_AreNotEqual()
        {
            Assert.NotEqual(BoardNotation.Parse("RYBG"), BoardNotation.Parse("RY\nBG"));
        }
    }
}