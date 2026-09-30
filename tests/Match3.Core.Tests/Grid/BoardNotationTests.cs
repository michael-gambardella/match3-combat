using System;

using Match3.Core.Grid;

using Xunit;

namespace Match3.Core.Tests.Grid
{
    public class BoardNotationTests
    {
        [Fact]
        public void Parse_ValidText_ReadsDimensionsAndGems()
        {
            Board board = BoardNotation.Parse("RYB\nGPK");

            Assert.Equal(3, board.Width);
            Assert.Equal(2, board.Height);
            Assert.Equal(GemColor.Red, board.GemAt(new Position(0, 0)));
            Assert.Equal(GemColor.Blue, board.GemAt(new Position(0, 2)));
            Assert.Equal(GemColor.Black, board.GemAt(new Position(1, 2)));
        }

        [Fact]
        public void Parse_IndentedFixtureWithBlankLines_IgnoresWhitespace()
        {
            Board board = BoardNotation.Parse("""

                RRG
                BBY

                """);

            Assert.Equal("RRG\nBBY", BoardNotation.Format(board));
        }

        [Fact]
        public void Parse_WindowsLineEndings_AreAccepted()
        {
            Board board = BoardNotation.Parse("RY\r\nBG");

            Assert.Equal(2, board.Height);
        }

        [Fact]
        public void Parse_RowsOfDifferentLengths_ThrowsFormatException()
        {
            Assert.Throws<FormatException>(() => BoardNotation.Parse("RYB\nGP"));
        }

        [Fact]
        public void Parse_UnknownSymbol_ThrowsFormatException()
        {
            Assert.Throws<FormatException>(() => BoardNotation.Parse("RYX"));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   \n  \n")]
        public void Parse_NoRows_ThrowsFormatException(string text)
        {
            Assert.Throws<FormatException>(() => BoardNotation.Parse(text));
        }

        [Fact]
        public void Format_ParsedText_RoundTrips()
        {
            const string text = "RYBG\nPKRY\nBGPK";

            Assert.Equal(text, BoardNotation.Format(BoardNotation.Parse(text)));
        }
    }
}