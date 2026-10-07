using System;
using System.Collections.Generic;
using System.Linq;

using Match3.Core.Grid;
using Match3.Core.Randomness;

namespace Match3.Core.Tests.Fakes
{
    /// <summary>Returns a fixed sequence of values, failing loudly if the code draws more than the test expects.</summary>
    internal sealed class ScriptedRandom : IRandomSource
    {
        private readonly Queue<int> _values;

        public ScriptedRandom(params int[] values) => _values = new Queue<int>(values);

        /// <summary>How many scripted values have not been drawn yet.</summary>
        public int Remaining => _values.Count;

        /// <summary>Scripts new gem colors in draw order using board notation symbols, e.g. "KRB".</summary>
        public static ScriptedRandom Spawning(string symbols)
        {
            Board row = BoardNotation.Parse(symbols);
            return new ScriptedRandom(
                Enumerable.Range(0, row.Width).Select(column => (int)row.GemAt(new Position(0, column))).ToArray());
        }

        public int NextInt(int maxExclusive)
        {
            if (_values.Count == 0)
            {
                throw new InvalidOperationException("ScriptedRandom ran out of values: the code drew more random numbers than the test expected.");
            }

            int value = _values.Dequeue();
            if (value < 0 || value >= maxExclusive)
            {
                throw new InvalidOperationException($"Scripted value {value} is outside [0, {maxExclusive}).");
            }

            return value;
        }
    }
}