using System;

using Match3.Core.Grid;

namespace Match3.Core.Cascades
{
    /// <summary>A new gem that fills an empty cell after gravity.</summary>
    /// <remarks>
    /// New gems enter from above the board. To animate one, start it k rows above its final row,
    /// where k is the number of gems spawned in its column during the same step.
    /// </remarks>
    public sealed class GemSpawn
    {
        /// <summary>Creates a spawn of <paramref name="color"/> at <paramref name="position"/>.</summary>
        /// <param name="position">The cell the new gem fills.</param>
        /// <param name="color">The new gem's color.</param>
        public GemSpawn(Position position, GemColor color)
        {
            Position = position;
            Color = color;
        }

        /// <summary>The cell the new gem fills.</summary>
        public Position Position { get; }

        /// <summary>The new gem's color.</summary>
        public GemColor Color { get; }

        /// <inheritdoc />
        public override string ToString() => FormattableString.Invariant($"{Color} at {Position}");
    }
}