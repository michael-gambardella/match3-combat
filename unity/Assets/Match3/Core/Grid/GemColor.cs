using System;
using System.Collections.Generic;
using System.Linq;

namespace Match3.Core.Grid
{
    /// <summary>The set of gem colors.</summary>
    public static class GemColors
    {
        /// <summary>
        /// Every <see cref="GemColor"/>, ordered by underlying value. Random draws index into this list,
        /// so its order is part of the determinism contract.
        /// </summary>
        public static IReadOnlyList<GemColor> All { get; } =
            Array.AsReadOnly(Enum.GetValues(typeof(GemColor)).Cast<GemColor>().ToArray());
    }
}


namespace Match3.Core.Grid
{
    /// <summary>The color of a gem. Matching gems of a color earns action points of that color.</summary>
    public enum GemColor
    {
        /// <summary>Red gem. Notation symbol <c>R</c>.</summary>
        Red,

        /// <summary>Yellow gem. Notation symbol <c>Y</c>.</summary>
        Yellow,

        /// <summary>Blue gem. Notation symbol <c>B</c>.</summary>
        Blue,

        /// <summary>Green gem. Notation symbol <c>G</c>.</summary>
        Green,

        /// <summary>Purple gem. Notation symbol <c>P</c>.</summary>
        Purple,

        /// <summary>Black gem. Notation symbol <c>K</c> (as in CMYK, since <c>B</c> is Blue).</summary>
        Black,
    }
}