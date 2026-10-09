using System;
using System.Collections.Generic;

using Match3.Core.Grid;
using Match3.Core.Matching;
using Match3.Core.Randomness;

namespace Match3.Core.Cascades
{
    /// <summary>Resolves a board into a settled state by clearing matches, applying gravity, and refilling.</summary>
    public static class CascadeResolver
    {
        /// <summary>
        /// The most steps a resolution may take. Real boards settle in a handful of steps; reaching this
        /// limit means the random source is broken, so resolution fails instead of looping forever.
        /// </summary>
        public const int MaxSteps = 50;

        // Spawned colors are drawn as (GemColor)random.NextInt(ColorCount), which assumes GemColor's
        // values are contiguous from 0. Adding a color with an explicit, non-contiguous value breaks this.
        private static readonly int ColorCount = Enum.GetValues(typeof(GemColor)).Length;

        /// <summary>
        /// Repeatedly clears every match, lets surviving gems fall, and fills empty cells with new gems,
        /// until the board contains no matches.
        /// </summary>
        /// <param name="board">The board to resolve, typically right after a player's swap.</param>
        /// <param name="random">Chooses each new gem's color; colors are drawn in row-major order of the empty cells.</param>
        /// <returns>One step per round of clearing, plus the settled board. No steps if the board has no matches.</returns>
        /// <exception cref="ArgumentNullException">An argument is null.</exception>
        /// <exception cref="InvalidOperationException">The board has not settled after <see cref="MaxSteps"/> steps.</exception>
        public static CascadeResult Resolve(Board board, IRandomSource random)
        {
            if (board is null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            if (random is null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            var steps = new List<CascadeStep>();
            Board current = board;
            while (true)
            {
                IReadOnlyList<Match> matches = MatchFinder.FindMatches(current);
                if (matches.Count == 0)
                {
                    return new CascadeResult(steps, current);
                }

                if (steps.Count == MaxSteps)
                {
                    throw new InvalidOperationException(
                        FormattableString.Invariant($"The board has not settled after {MaxSteps} steps."));
                }

                CascadeStep step = ResolveStep(current, matches, random);
                steps.Add(step);
                current = step.BoardAfter;
            }
        }

        /// <summary>Clears <paramref name="matches"/>, drops the surviving gems, and refills the empty cells.</summary>
        private static CascadeStep ResolveStep(Board board, IReadOnlyList<Match> matches, IRandomSource random)
        {
            var cleared = new HashSet<Position>();
            foreach (Match match in matches)
            {
                cleared.UnionWith(match.Positions);
            }

            var grid = new WorkingGrid(board.Width, board.Height);
            List<GemFall> falls = ApplyGravity(board, cleared, grid);
            List<GemSpawn> spawns = Refill(grid, random);
            return new CascadeStep(matches, falls, spawns, grid.ToBoard());
        }

        /// <summary>
        /// Places every surviving gem in <paramref name="grid"/> at the lowest free cell of its column,
        /// preserving each column's order. Uses one bottom-up pass per column.
        /// </summary>
        /// <returns>The gems that moved, ordered by destination (row-major).</returns>
        private static List<GemFall> ApplyGravity(Board board, HashSet<Position> cleared, WorkingGrid grid)
        {
            var falls = new List<GemFall>();
            for (int column = 0; column < board.Width; column++)
            {
                int destinationRow = board.Height - 1;
                for (int row = board.Height - 1; row >= 0; row--)
                {
                    var from = new Position(row, column);
                    if (cleared.Contains(from))
                    {
                        continue;
                    }

                    var to = new Position(destinationRow, column);
                    grid.Place(to, board.GemAt(from));
                    if (to != from)
                    {
                        falls.Add(new GemFall(from, to));
                    }

                    destinationRow--;
                }
            }

            // Collected column by column; the step contract orders them by where they land.
            falls.Sort((left, right) => Position.RowMajor.Compare(left.To, right.To));
            return falls;
        }

        /// <summary>Fills every empty cell with a random gem, drawing colors in row-major order.</summary>
        /// <returns>The new gems, in the order their colors were drawn.</returns>
        private static List<GemSpawn> Refill(WorkingGrid grid, IRandomSource random)
        {
            var spawns = new List<GemSpawn>();
            for (int row = 0; row < grid.Height; row++)
            {
                for (int column = 0; column < grid.Width; column++)
                {
                    var position = new Position(row, column);
                    if (grid.IsFilled(position))
                    {
                        continue;
                    }

                    GemColor gem = GemColors.All[random.NextInt(GemColors.All.Count)];
                    grid.Place(position, gem);
                    spawns.Add(new GemSpawn(position, gem));
                }
            }

            return spawns;
        }

        /// <summary>
        /// A board under construction whose cells may be empty. Empty cells exist only here, inside a
        /// single step; <see cref="ToBoard"/> enforces that the result is full.
        /// </summary>
        private sealed class WorkingGrid
        {
            private readonly GemColor[] _gems;
            private readonly bool[] _filled;

            public WorkingGrid(int width, int height)
            {
                Width = width;
                Height = height;
                _gems = new GemColor[width * height];
                _filled = new bool[width * height];
            }

            public int Width { get; }

            public int Height { get; }

            public bool IsFilled(Position position) => _filled[IndexOf(position)];

            public void Place(Position position, GemColor gem)
            {
                int index = IndexOf(position);
                _gems[index] = gem;
                _filled[index] = true;
            }

            /// <summary>Converts to an immutable <see cref="Board"/>.</summary>
            /// <exception cref="InvalidOperationException">A cell is still empty, which indicates a resolver bug.</exception>
            public Board ToBoard() => Board.Create(Width, Height, position =>
            {
                int index = IndexOf(position);
                return _filled[index]
                    ? _gems[index]
                    : throw new InvalidOperationException(FormattableString.Invariant($"Cell {position} was never filled."));
            });

            private int IndexOf(Position position) => (position.Row * Width) + position.Column;
        }
    }
}