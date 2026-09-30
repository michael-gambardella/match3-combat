using System;
using System.Linq;

namespace Match3.Core.Grid
{
    /// <summary>
    /// An immutable rectangular grid of gems. Every cell always holds a valid gem.
    /// </summary>
    /// <remarks>
    /// Operations that change the board return a new instance, so boards are safe to share,
    /// simple to compare in tests, and free to snapshot for replays. Boards are small
    /// (an 8x8 board is 64 cells), so copying is cheap. Cells are stored row-major in a flat array.
    /// </remarks>
    public sealed class Board : IEquatable<Board>
    {
        private readonly GemColor[] _cells;

        private Board(int width, int height, GemColor[] cells)
        {
            Width = width;
            Height = height;
            _cells = cells;
        }

        /// <summary>Number of columns.</summary>
        public int Width { get; }

        /// <summary>Number of rows.</summary>
        public int Height { get; }

        /// <summary>Creates a board by asking <paramref name="gemAt"/> for the gem in each cell.</summary>
        /// <param name="width">Number of columns; must be positive.</param>
        /// <param name="height">Number of rows; must be positive.</param>
        /// <param name="gemAt">Returns the gem for a given cell.</param>
        /// <exception cref="ArgumentOutOfRangeException">A dimension is not positive.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="gemAt"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="gemAt"/> returned an undefined color.</exception>
        public static Board Create(int width, int height, Func<Position, GemColor> gemAt)
        {
            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width), width, "Width must be positive.");
            }

            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height), height, "Height must be positive.");
            }

            if (gemAt is null)
            {
                throw new ArgumentNullException(nameof(gemAt));
            }

            var cells = new GemColor[width * height];
            for (int row = 0; row < height; row++)
            {
                for (int column = 0; column < width; column++)
                {
                    var position = new Position(row, column);
                    GemColor gem = gemAt(position);
                    if (!Enum.IsDefined(typeof(GemColor), gem))
                    {
                        throw new ArgumentException(
                            FormattableString.Invariant($"Undefined gem color {(int)gem} at {position}."),
                            nameof(gemAt));
                    }

                    cells[(row * width) + column] = gem;
                }
            }

            return new Board(width, height, cells);
        }

        /// <summary>Returns true when <paramref name="position"/> lies on the board.</summary>
        public bool Contains(Position position) =>
            position.Row >= 0 && position.Row < Height &&
            position.Column >= 0 && position.Column < Width;

        /// <summary>Returns the gem at <paramref name="position"/>.</summary>
        /// <exception cref="ArgumentOutOfRangeException">The position is off the board.</exception>
        public GemColor GemAt(Position position) => _cells[IndexOf(position)];

        /// <summary>Returns a new board with the gems at <paramref name="a"/> and <paramref name="b"/> exchanged.</summary>
        /// <remarks>
        /// Does not check adjacency or whether the swap creates a match; those are game rules,
        /// enforced by move validation, not properties of the grid.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">Either position is off the board.</exception>
        public Board WithSwap(Position a, Position b)
        {
            int indexA = IndexOf(a);
            int indexB = IndexOf(b);

            var cells = (GemColor[])_cells.Clone();
            cells[indexA] = _cells[indexB];
            cells[indexB] = _cells[indexA];
            return new Board(Width, Height, cells);
        }

        /// <inheritdoc />
        public bool Equals(Board? other) =>
            other is not null &&
            Width == other.Width &&
            Height == other.Height &&
            _cells.SequenceEqual(other._cells);

        /// <inheritdoc />
        public override bool Equals(object? obj) => Equals(obj as Board);

        /// <inheritdoc />
        public override int GetHashCode()
        {
            var hash = default(HashCode);
            hash.Add(Width);
            hash.Add(Height);
            foreach (GemColor gem in _cells)
            {
                hash.Add(gem);
            }

            return hash.ToHashCode();
        }

        /// <summary>Returns the board in <see cref="BoardNotation"/> form.</summary>
        public override string ToString() => BoardNotation.Format(this);

        private int IndexOf(Position position)
        {
            if (!Contains(position))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(position),
                    FormattableString.Invariant($"{position} is outside the {Width}x{Height} board."));
            }

            return (position.Row * Width) + position.Column;
        }
    }
}