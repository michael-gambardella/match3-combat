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

        /// <summary>Clears <paramref name="matches"/>, drops survivors, and refills the holes.</summary>
        private static CascadeStep ResolveStep(Board board, IReadOnlyList<Match> matches, IRandomSource random)
        {
            var cleared = new HashSet<Position>();
            foreach (Match match in matches)
            {
                foreach (Position position in match.Positions)
                {
                    cleared.Add(position);
                }
            }

            int width = board.Width;
            int height = board.Height;
            var cells = new GemColor[width * height];
            var occupied = new bool[cells.Length];
            var falls = new List<GemFall>();

            for (int column = 0; column < width; column++)
            {
                int destinationRow = height - 1;
                for (int row = height - 1; row >= 0; row--)
                {
                    var from = new Position(row, column);
                    if (cleared.Contains(from))
                    {
                        continue;
                    }

                    int index = (destinationRow * width) + column;
                    cells[index] = board.GemAt(from);
                    occupied[index] = true;
                    if (destinationRow != row)
                    {
                        falls.Add(new GemFall(from, new Position(destinationRow, column)));
                    }

                    destinationRow--;
                }
            }

            // Collected column by column; the step contract orders them by where they land.
            falls.Sort((left, right) => Position.RowMajor.Compare(left.To, right.To));

            var spawns = new List<GemSpawn>();
            for (int row = 0; row < height; row++)
            {
                for (int column = 0; column < width; column++)
                {
                    int index = (row * width) + column;
                    if (occupied[index])
                    {
                        continue;
                    }

                    var color = (GemColor)random.NextInt(ColorCount);
                    cells[index] = color;
                    spawns.Add(new GemSpawn(new Position(row, column), color));
                }
            }

            Board boardAfter = Board.Create(width, height, position => cells[(position.Row * width) + position.Column]);
            return new CascadeStep(matches, falls, spawns, boardAfter);
        }
    }
}