using System;

namespace Match3.Core.Grid
{
    /// <summary>
    /// An immutable cell coordinate on the board. Row 0 is the top row and column 0 is the
    /// leftmost column, matching the reading order of text-based board fixtures.
    /// </summary>
    /// <remarks>
    /// A position is not validated against any board size and may lie off the board
    /// (for example, when computing a neighbor). Bounds checking is the board's responsibility.
    /// </remarks>
    public readonly struct Position : IEquatable<Position>
    {
        /// <summary>Creates a position at the given row and column.</summary>
        /// <param name="row">Zero-based row index, counted from the top.</param>
        /// <param name="column">Zero-based column index, counted from the left.</param>
        public Position(int row, int column)
        {
            Row = row;
            Column = column;
        }

        /// <summary>Zero-based row index, counted from the top of the board.</summary>
        public int Row { get; }

        /// <summary>Zero-based column index, counted from the left of the board.</summary>
        public int Column { get; }

        /// <summary>Returns true when both positions refer to the same cell.</summary>
        public static bool operator ==(Position left, Position right) => left.Equals(right);

        /// <summary>Returns true when the positions refer to different cells.</summary>
        public static bool operator !=(Position left, Position right) => !left.Equals(right);

        /// <inheritdoc />
        public bool Equals(Position other) => Row == other.Row && Column == other.Column;

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is Position other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(Row, Column);

        /// <inheritdoc />
        public override string ToString() => FormattableString.Invariant($"({Row}, {Column})");
    }
}