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

            // Runs that share a cell belong to one match. Only same-colored runs can share a cell,
            // so no color check is needed.
            int[] parent = new int[runs.Count];
            for (int i = 0; i < parent.Length; i++)
            {
                parent[i] = i;
            }

            var claimedBy = new Dictionary<Position, int>();
            for (int i = 0; i < runs.Count; i++)
            {
                foreach (Position position in runs[i])
                {
                    if (claimedBy.TryGetValue(position, out int other))
                    {
                        parent[Find(parent, i)] = Find(parent, other);
                    }
                    else
                    {
                        claimedBy.Add(position, i);
                    }
                }
            }

            var groups = new Dictionary<int, List<Position>>();
            for (int i = 0; i < runs.Count; i++)
            {
                int root = Find(parent, i);
                if (!groups.TryGetValue(root, out List<Position>? group))
                {
                    group = new List<Position>();
                    groups.Add(root, group);
                }

                group.AddRange(runs[i]);
            }

            var matches = new List<Match>(groups.Count);
            foreach (List<Position> group in groups.Values)
            {
                matches.Add(new Match(board.GemAt(group[0]), group));
            }

            return matches;
        }

        /// <summary>Returns the root of <paramref name="run"/>'s group, compressing the path as it goes.</summary>
        private static int Find(int[] parent, int run)
        {
            while (parent[run] != run)
            {
                parent[run] = parent[parent[run]];
                run = parent[run];
            }

            return run;
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