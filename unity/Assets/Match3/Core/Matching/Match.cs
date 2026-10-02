using System;
using System.Collections.Generic;
using System.Linq;

using Match3.Core.Grid;

namespace Match3.Core.Matching
{
    /// <summary>
    /// A group of same-colored gems that clear together: one straight run of at least
    /// <see cref="MinimumLength"/> gems, or several same-colored runs that share a cell
    /// (L, T, and cross shapes).
    /// </summary>
    public sealed class Match
    {
        /// <summary>The fewest gems in a line that form a run.</summary>
        public const int MinimumLength = 3;

        /// <summary>Creates a match. Positions are de-duplicated and sorted row-major.</summary>
        /// <param name="color">The color shared by every gem in the match.</param>
        /// <param name="positions">The matched cells, in any order, duplicates allowed.</param>
        /// <exception cref="ArgumentNullException"><paramref name="positions"/> is null.</exception>
        /// <exception cref="ArgumentException">Fewer than <see cref="MinimumLength"/> distinct positions.</exception>
        public Match(GemColor color, IEnumerable<Position> positions)
        {
            if (positions is null)
            {
                throw new ArgumentNullException(nameof(positions));
            }

            Position[] ordered = positions
                .Distinct()
                .OrderBy(position => position, Position.RowMajor)
                .ToArray();

            if (ordered.Length < MinimumLength)
            {
                throw new ArgumentException(
                    FormattableString.Invariant($"A match needs at least {MinimumLength} distinct positions; got {ordered.Length}."),
                    nameof(positions));
            }

            Color = color;
            Positions = Array.AsReadOnly(ordered);
        }

        /// <summary>The color shared by every gem in the match.</summary>
        public GemColor Color { get; }

        /// <summary>The matched cells, distinct and sorted row-major.</summary>
        public IReadOnlyList<Position> Positions { get; }

        /// <inheritdoc />
        public override string ToString() =>
            FormattableString.Invariant($"{Color} x{Positions.Count} from {Positions[0]}");
    }
}