using System;

using Match3.Core.Grid;

namespace Match3.Core.Moves
{
    /// <summary>
    /// A player's swap of two orthogonally adjacent cells. Swapping A with B is the same move as
    /// swapping B with A, so the cells are stored in row-major order as <see cref="First"/> and <see cref="Second"/>.
    /// </summary>
    /// <remarks>
    /// A class rather than a struct: a struct's <c>default</c> value bypasses the constructor and would
    /// represent a "move" from a cell to itself, which must be impossible to construct.
    /// </remarks>
    public sealed class Move : IEquatable<Move>
    {
        /// <summary>Creates a move swapping <paramref name="a"/> and <paramref name="b"/>, in either order.</summary>
        /// <exception cref="ArgumentException">The cells are not orthogonally adjacent.</exception>
        public Move(Position a, Position b)
        {
            int distance = Math.Abs(a.Row - b.Row) + Math.Abs(a.Column - b.Column);
            if (distance != 1)
            {
                throw new ArgumentException(
                    FormattableString.Invariant($"{a} and {b} are not orthogonally adjacent."), nameof(b));
            }

            bool aComesFirst = Position.RowMajor.Compare(a, b) < 0;
            First = aComesFirst ? a : b;
            Second = aComesFirst ? b : a;
        }

        /// <summary>The swapped cell that comes first in row-major order.</summary>
        public Position First { get; }

        /// <summary>The swapped cell that comes second in row-major order.</summary>
        public Position Second { get; }

        /// <summary>True when the two cells share a row.</summary>
        public bool IsHorizontal => First.Row == Second.Row;

        /// <inheritdoc />
        public bool Equals(Move? other) => other is not null && First == other.First && Second == other.Second;

        /// <inheritdoc />
        public override bool Equals(object? obj) => Equals(obj as Move);

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(First, Second);

        /// <inheritdoc />
        public override string ToString() => FormattableString.Invariant($"{First} <-> {Second}");
    }
}