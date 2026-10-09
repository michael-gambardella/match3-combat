using System;
using System.Collections.Generic;

using Match3.Core.Grid;
using Match3.Core.Moves;
using Match3.Core.Randomness;

namespace Match3.Core.Generation
{
    /// <summary>Creates playable boards: no matches, and at least one legal move.</summary>
    public static class BoardGenerator
    {
        /// <summary>How many boards may be generated before giving up.</summary>
        public const int MaxAttempts = 100;

        /// <summary>Generates a board with no matches and at least one legal move.</summary>
        /// <remarks>
        /// Cells are filled in row-major order. A color is forbidden for a cell if the two cells to its left
        /// both have it, or the two cells above both have it; the cell's color is
        /// <c>allowed[random.NextInt(allowed.Count)]</c>, where <c>allowed</c> lists the remaining colors in
        /// <see cref="GemColors.All"/> order. If the finished board has no legal move, a new board is generated,
        /// continuing the same random sequence. The fill order and draw rule are part of the determinism contract.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">A dimension is not positive.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="random"/> is null.</exception>
        /// <exception cref="InvalidOperationException">
        /// No board with a legal move was found within <see cref="MaxAttempts"/> attempts, for example because
        /// the board is too small to ever have one.
        /// </exception>
        public static Board Generate(int width, int height, IRandomSource random)
        {
            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width), width, "Width must be positive.");
            }

            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height), height, "Height must be positive.");
            }

            if (random is null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            for (int attempt = 0; attempt < MaxAttempts; attempt++)
            {
                GemColor[] cells = Fill(width, height, random);
                Board board = Board.Create(width, height, position => cells[(position.Row * width) + position.Column]);
                if (MoveFinder.FindLegalMoves(board).Count > 0)
                {
                    return board;
                }
            }

            throw new InvalidOperationException(
                FormattableString.Invariant($"No board with a legal move was found within {MaxAttempts} attempts."));
        }

        /// <summary>Fills one candidate in row-major order, skipping colors that would complete a run.</summary>
        private static GemColor[] Fill(int width, int height, IRandomSource random)
        {
            var cells = new GemColor[width * height];
            var allowed = new List<GemColor>(GemColors.All.Count);
            for (int row = 0; row < height; row++)
            {
                for (int column = 0; column < width; column++)
                {
                    allowed.Clear();
                    foreach (GemColor color in GemColors.All)
                    {
                        if (!IsForbidden(cells, width, row, column, color))
                        {
                            allowed.Add(color);
                        }
                    }

                    cells[(row * width) + column] = allowed[random.NextInt(allowed.Count)];
                }
            }

            return cells;
        }

        /// <summary>True when the two cells to the left, or the two cells above, already hold <paramref name="color"/>.</summary>
        private static bool IsForbidden(GemColor[] cells, int width, int row, int column, GemColor color)
        {
            if (column >= 2)
            {
                int index = (row * width) + column;
                if (cells[index - 1] == color && cells[index - 2] == color)
                {
                    return true;
                }
            }

            if (row >= 2)
            {
                int above = ((row - 1) * width) + column;
                if (cells[above] == color && cells[above - width] == color)
                {
                    return true;
                }
            }

            return false;
        }
    }
}