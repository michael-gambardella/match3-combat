using Match3.Core.Randomness;

namespace Match3.Core.Tests.Fakes
{
    /// <summary>
    /// Draws scripted values first, then falls back to another source. Lets a test control the first
    /// few draws (for example, refill colors) and leave later ones (for example, a reshuffle) to a seed.
    /// </summary>
    internal sealed class ChainedRandom : IRandomSource
    {
        private readonly ScriptedRandom _script;
        private readonly IRandomSource _then;

        public ChainedRandom(ScriptedRandom script, IRandomSource then)
        {
            _script = script;
            _then = then;
        }

        public int NextInt(int maxExclusive) =>
            _script.Remaining > 0 ? _script.NextInt(maxExclusive) : _then.NextInt(maxExclusive);
    }
}