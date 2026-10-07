using System;

namespace Match3.Core.Randomness
{
    /// <summary>
    /// A source of random integers. Game rules depend on this instead of <see cref="System.Random"/>
    /// so tests can script outcomes and a seed can reproduce an entire game.
    /// </summary>
    public interface IRandomSource
    {
        /// <summary>Returns a uniformly distributed integer in [0, <paramref name="maxExclusive"/>).</summary>
        /// <param name="maxExclusive">Exclusive upper bound; must be positive.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxExclusive"/> is not positive.</exception>
        int NextInt(int maxExclusive);
    }
}