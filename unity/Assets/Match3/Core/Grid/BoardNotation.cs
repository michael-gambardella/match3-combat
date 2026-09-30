using System;
using System.Linq;
using System.Text;

namespace Match3.Core.Grid
{
    /// <summary>
    /// Converts boards to and from compact text: one line per row, top row first,
    /// one symbol per gem (<c>R Y B G P K</c>). Used for test fixtures, logs, and bug reports.
    /// </summary>
    /// <remarks>
    /// Parsing ignores blank lines and whitespace around each row, so fixtures can be indented
    /// inside source code. Formatting always uses <c>\n</c> so output is identical on every platform.
    /// </remarks>
    public static class BoardNotation
    {
        /// <summary>Parses a board from its text form.</summary>
        /// <exception cref="ArgumentNullException"><paramref name="text"/> is null.</exception>
        /// <exception cref="FormatException">The text is empty, has rows of different lengths, or contains an unknown symbol.</exception>
        public static Board Parse(string text)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            string[] rows = text
                .Split('\n')
                .Select(line => line.Trim())
                .Where(line => line.Length > 0)
                .ToArray();

            if (rows.Length == 0)
            {
                throw new FormatException("Board text contains no rows.");
            }

            int width = rows[0].Length;
            for (int row = 1; row < rows.Length; row++)
            {
                if (rows[row].Length != width)
                {
                    throw new FormatException(FormattableString.Invariant(
                        $"Row {row} has {rows[row].Length} gems; expected {width}."));
                }
            }

            return Board.Create(width, rows.Length, position => ToGem(rows[position.Row][position.Column], position));
        }

        /// <summary>Formats a board as text that <see cref="Parse"/> reads back to an equal board.</summary>
        /// <exception cref="ArgumentNullException"><paramref name="board"/> is null.</exception>
        public static string Format(Board board)
        {
            if (board is null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            var text = new StringBuilder(board.Height * (board.Width + 1));
            for (int row = 0; row < board.Height; row++)
            {
                if (row > 0)
                {
                    text.Append('\n');
                }

                for (int column = 0; column < board.Width; column++)
                {
                    text.Append(ToSymbol(board.GemAt(new Position(row, column))));
                }
            }

            return text.ToString();
        }

        private static GemColor ToGem(char symbol, Position position) => symbol switch
        {
            'R' => GemColor.Red,
            'Y' => GemColor.Yellow,
            'B' => GemColor.Blue,
            'G' => GemColor.Green,
            'P' => GemColor.Purple,
            'K' => GemColor.Black,
            _ => throw new FormatException(FormattableString.Invariant($"Unknown gem symbol '{symbol}' at {position}.")),
        };

        private static char ToSymbol(GemColor gem) => gem switch
        {
            GemColor.Red => 'R',
            GemColor.Yellow => 'Y',
            GemColor.Blue => 'B',
            GemColor.Green => 'G',
            GemColor.Purple => 'P',
            GemColor.Black => 'K',
            _ => throw new ArgumentOutOfRangeException(nameof(gem), gem, "Undefined gem color."),
        };
    }
}