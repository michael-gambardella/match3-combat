using Match3.Core.Randomness;

namespace Match3.Core.Tests.Fakes
{
    /// <summary>Always returns the same value: a deliberately broken source for testing safety limits.</summary>
    internal sealed class ConstantRandom : IRandomSource
    {
        private readonly int _value;

        public ConstantRandom(int value) => _value = value;

        public int NextInt(int maxExclusive) => _value;
    }
}