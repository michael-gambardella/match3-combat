using System;

using Match3.Core.Grid;

namespace Match3.Core.Cascades
{
    /// <summary>A surviving gem that moved down to fill cleared cells beneath it.</summary>
    public sealed class GemFall
    {
        /// <summary>Creates a fall from <paramref name="from"/> to <paramref name="to"/>.</summary>
        /// <param name="from">Where the gem was before the step.</param>
        /// <param name="to">Where the gem is after the step.</param>
        public GemFall(Position from, Position to)
        {
            From = from;
            To = to;
        }

        /// <summary>Where the gem was before the step.</summary>
        public Position From { get; }

        /// <summary>Where the gem is after the step.</summary>
        public Position To { get; }

        /// <inheritdoc />
        public override string ToString() => FormattableString.Invariant($"{From} -> {To}");
    }
}