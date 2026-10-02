using System;
using System.Collections.Generic;
using System.Linq;

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
        /// <remarks>
        /// Scans each row and column once to collect runs, then merges runs that share a cell using
        /// union-find. Time is O(W×H); union-find with path compression adds only a near-constant factor.
        /// Runs are merged by shared cells rather than by flood fill, so a same-colored gem that merely
        /// touches a run is not pulled into it.
        /// </remarks>
        /// <param name="board">The board to scan.</param>
        /// <returns>Matches ordered by their first position (row-major); empty when there are none.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="board"/> is null.</exception>
        public static IReadOnlyList<Match> FindMatches(Board board)
        {
            if (board is null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            List<IReadOnlyList<Position>> runs =
                FindRuns(board, board.Height, board.Width, (row, column) => new Position(row, column))
                .Concat(FindRuns(board, board.Width, board.Height, (column, row) => new Position(row, column)))
                .ToList();

            List<Match> matches = MergeRunsSharingACell(runs)
                .Select(cells => new Match(board.GemAt(cells[0]), cells))
                .ToList();

            // Matches never overlap, so their first positions are unique: the unstable sort has no ties to break.
            matches.Sort((a, b) => Position.RowMajor.Compare(a.Positions[0], b.Positions[0]));
            return matches.AsReadOnly();
        }

        /// <summary>Yields every run of at least <see cref="Match.MinimumLength"/> same-colored gems along parallel lines.</summary>
        /// <param name="board">The board to scan.</param>
        /// <param name="lineCount">How many parallel lines to scan.</param>
        /// <param name="lineLength">How many cells each line holds.</param>
        /// <param name="positionAt">Maps (line, step along the line) to a board position.</param>
        private static IEnumerable<IReadOnlyList<Position>> FindRuns(
            Board board, int lineCount, int lineLength, Func<int, int, Position> positionAt)
        {
            for (int line = 0; line < lineCount; line++)
            {
                int runStart = 0;

                // step == lineLength is a sentinel past the end that closes the final run.
                for (int step = 1; step <= lineLength; step++)
                {
                    bool runContinues = step < lineLength &&
                        board.GemAt(positionAt(line, step)) == board.GemAt(positionAt(line, runStart));
                    if (runContinues)
                    {
                        continue;
                    }

                    int length = step - runStart;
                    if (length >= Match.MinimumLength)
                    {
                        var run = new Position[length];
                        for (int i = 0; i < length; i++)
                        {
                            run[i] = positionAt(line, runStart + i);
                        }

                        yield return run;
                    }

                    runStart = step;
                }
            }
        }

        /// <summary>
        /// Groups runs that share at least one cell. Only same-colored runs can share a cell,
        /// so no color check is needed.
        /// </summary>
        /// <param name="runs">The runs to group.</param>
        /// <returns>One list of positions per group; a shared cell may appear more than once.</returns>
        private static List<List<Position>> MergeRunsSharingACell(List<IReadOnlyList<Position>> runs)
        {
            var sets = new DisjointSet(runs.Count);
            var claimedBy = new Dictionary<Position, int>();
            for (int run = 0; run < runs.Count; run++)
            {
                foreach (Position position in runs[run])
                {
                    if (claimedBy.TryGetValue(position, out int other))
                    {
                        sets.Union(run, other);
                    }
                    else
                    {
                        claimedBy.Add(position, run);
                    }
                }
            }

            var groups = new Dictionary<int, List<Position>>();
            for (int run = 0; run < runs.Count; run++)
            {
                int root = sets.Find(run);
                if (!groups.TryGetValue(root, out List<Position>? group))
                {
                    group = new List<Position>();
                    groups.Add(root, group);
                }

                group.AddRange(runs[run]);
            }

            return new List<List<Position>>(groups.Values);
        }

        /// <summary>Union-find over integer ids, with path compression.</summary>
        private sealed class DisjointSet
        {
            private readonly int[] _parent;

            public DisjointSet(int count) => _parent = Enumerable.Range(0, count).ToArray();

            /// <summary>Returns the root id of <paramref name="id"/>'s set, shortening the path as it goes.</summary>
            public int Find(int id)
            {
                while (_parent[id] != id)
                {
                    _parent[id] = _parent[_parent[id]];
                    id = _parent[id];
                }

                return id;
            }

            /// <summary>Merges the sets containing <paramref name="a"/> and <paramref name="b"/>.</summary>
            public void Union(int a, int b) => _parent[Find(a)] = Find(b);
        }
    }
}