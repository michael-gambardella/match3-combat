using System;

namespace Match3.Core.Randomness
{
    /// <summary>
    /// A deterministic random source using the SplitMix64 algorithm: the same seed produces the
    /// same sequence on every platform and runtime.
    /// </summary>
    /// <remarks>
    /// <see cref="System.Random"/> is not used because its seeded sequence is an implementation
    /// detail that is not guaranteed to match between .NET and Unity's runtime, which would break
    /// replays and golden tests. Not suitable for security purposes.
    /// </remarks>
    public sealed class SeededRandom : IRandomSource
    {
        private ulong _state;

        /// <summary>Creates a generator whose sequence is fully determined by <paramref name="seed"/>.</summary>
        /// <param name="seed">Any value; equal seeds give equal sequences.</param>
        public SeededRandom(ulong seed) => _state = seed;

        /// <inheritdoc />
        public int NextInt(int maxExclusive)
        {
            if (maxExclusive <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxExclusive), maxExclusive, "Must be positive.");
            }

            // Modulo bias is at most maxExclusive / 2^64, which is negligible for gameplay ranges.
            return (int)(NextUInt64() % (ulong)maxExclusive);
        }

        private ulong NextUInt64()
        {
            unchecked
            {
                _state += 0x9E3779B97F4A7C15UL;
                ulong z = _state;
                z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
                z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
                return z ^ (z >> 31);
            }
        }
    }
}