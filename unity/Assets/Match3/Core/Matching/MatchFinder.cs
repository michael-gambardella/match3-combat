using System;
using System.Collections.Generic;

using Match3.Core.Grid;

namespace Match3.Core.Matching
{
    /// <summary>Finds groups of gems that should clear.</summary>
    public static class MatchFinder
    {
        /// <summary>
        /// Finds every match on the board. Same-colored runs that share a cell merge into one match;
        /// runs that only sit side by side stay separate.
        /// </summary>
        /// <param name="board">The board to scan.</param>
        /// <returns>Matches ordered by their first position (row-major); empty when there are none.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="board"/> is null.</exception>
        public static IReadOnlyList<Match> FindMatches(Board board)
        {
            if (board is null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            var runs = new List<List<Position>>();
            AddRuns(board, board.Height, board.Width, (row, column) => new Position(row, column), runs);
            AddRuns(board, board.Width, board.Height, (column, row) => new Position(row, column), runs);

            var matches = new List<Match>();
            foreach (List<Position> run in runs)
            {
                matches.Add(new Match(board.GemAt(run[0]), run));
            }

            return matches;
        }

        /// <summary>Appends every run of at least <see cref="Match.MinimumLength"/> same-colored gems to <paramref name="runs"/>.</summary>
        /// <param name="board">The board to scan.</param>
        /// <param name="lineCount">How many parallel lines to scan.</param>
        /// <param name="lineLength">How many cells each line holds.</param>
        /// <param name="positionAt">Maps (line, step along the line) to a board position.</param>
        /// <param name="runs">Receives each run found.</param>
        private static void AddRuns(
            Board board, int lineCount, int lineLength, Func<int, int, Position> positionAt, List<List<Position>> runs)
        {
            for (int line = 0; line < lineCount; line++)
            {
                int runStart = 0;
                for (int step = 1; step <= lineLength; step++)
                {
                    bool runContinues = step < lineLength &&
                        board.GemAt(positionAt(line, step)) == board.GemAt(positionAt(line, runStart));
                    if (runContinues)
                    {
                        continue;
                    }

                    if (step - runStart >= Match.MinimumLength)
                    {
                        var run = new List<Position>(step - runStart);
                        for (int i = runStart; i < step; i++)
                        {
                            run.Add(positionAt(line, i));
                        }

                        runs.Add(run);
                    }

                    runStart = step;
                }
            }
        }
    }
}