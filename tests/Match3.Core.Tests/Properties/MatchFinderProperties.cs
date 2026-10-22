using System.Collections.Generic;
using System.Linq;

using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;

using Match3.Core.Grid;
using Match3.Core.Matching;

using Xunit;

namespace Match3.Core.Tests.Properties
{
    public class MatchFinderProperties
    {
        [Property]
        public Property MatchedCells_AreExactlyTheCellsOnARunOfThree() =>
            Prop.ForAll(GameArbitraries.Boards(), board =>
            {
                var matched = new HashSet<Position>(MatchFinder.FindMatches(board).SelectMany(match => match.Positions));
                var onARun = new HashSet<Position>(board.AllPositions().Where(cell => LiesOnRunOfThree(board, cell)));

                Assert.True(matched.SetEquals(onARun), $"MatchFinder disagrees with the brute-force definition on\n{board}");
            });

        [Property]
        public Property Matches_AreDisjointSingleColoredAndOrdered() =>
            Prop.ForAll(GameArbitraries.Boards(), board =>
            {
                IReadOnlyList<Match> matches = MatchFinder.FindMatches(board);
                List<Position> cells = matches.SelectMany(match => match.Positions).ToList();

                Assert.Equal(cells.Count, cells.Distinct().Count());
                Assert.All(matches, match => Assert.All(match.Positions, cell => Assert.Equal(match.Color, board.GemAt(cell))));
                Assert.Equal(matches.OrderBy(match => match.Positions[0], Position.RowMajor), matches);
            });

        /// <summary>
        /// The definition of a matched cell, written as plainly as possible: some window of three
        /// consecutive cells in its row or column contains it and holds a single color.
        /// </summary>
        private static bool LiesOnRunOfThree(Board board, Position cell)
        {
            for (int offset = -2; offset <= 0; offset++)
            {
                if (IsSingleColorWindow(board, new Position(cell.Row, cell.Column + offset), 0, 1) ||
                    IsSingleColorWindow(board, new Position(cell.Row + offset, cell.Column), 1, 0))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsSingleColorWindow(Board board, Position start, int rowStep, int columnStep)
        {
            var middle = new Position(start.Row + rowStep, start.Column + columnStep);
            var end = new Position(start.Row + (2 * rowStep), start.Column + (2 * columnStep));

            return board.Contains(start) && board.Contains(end) &&
                board.GemAt(start) == board.GemAt(middle) && board.GemAt(middle) == board.GemAt(end);
        }
    }
}