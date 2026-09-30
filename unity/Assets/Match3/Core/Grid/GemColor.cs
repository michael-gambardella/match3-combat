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