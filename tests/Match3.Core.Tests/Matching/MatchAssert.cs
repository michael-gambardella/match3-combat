using System.Collections.Generic;
using System.Linq;

using Match3.Core.Grid;
using Match3.Core.Matching;

using Xunit;

namespace Match3.Core.Tests.Matching
{
    /// <summary>Asserts match results as text masks: matched gems keep their symbol, all other cells become '.'.</summary>
    internal static class MatchAssert
    {
        public static void FindsExactly(string boardText, params string[] expectedMasks)
        {
            Board board = BoardNotation.Parse(boardText);

            string[] actual = MatchFinder.FindMatches(board).Select(match => ToMask(board, match)).ToArray();
            string[] expected = expectedMasks.Select(Normalize).ToArray();

            Assert.Equal(expected, actual);
        }

        private static string ToMask(Board board, Match match)
        {
            var matched = new HashSet<Position>(match.Positions);
            string[] rows = board.ToString().Split('\n');

            return string.Join("\n", rows.Select((line, row) => new string(
                line.Select((symbol, column) => matched.Contains(new Position(row, column)) ? symbol : '.').ToArray())));
        }

        private static string Normalize(string mask) =>
            string.Join("\n", mask.Split('\n').Select(line => line.Trim()).Where(line => line.Length > 0));
    }
}