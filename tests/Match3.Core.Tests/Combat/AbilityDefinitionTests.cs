using System;

using Match3.Core.Combat;
using Match3.Core.Grid;

using Xunit;

namespace Match3.Core.Tests.Combat
{
    public class AbilityDefinitionTests
    {
        [Fact]
        public void Constructor_ValidValues_StoresThem()
        {
            var ability = new AbilityDefinition("Fireball", GemColor.Red, cost: 7, damage: 20);

            Assert.Equal("Fireball", ability.Name);
            Assert.Equal(GemColor.Red, ability.CostColor);
            Assert.Equal(7, ability.Cost);
            Assert.Equal(20, ability.Damage);
        }

        [Fact]
        public void Constructor_NullName_ThrowsArgumentNull()
        {
            Assert.Throws<ArgumentNullException>(() => new AbilityDefinition(null!, GemColor.Red, cost: 1, damage: 1));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_BlankName_ThrowsArgumentException(string name)
        {
            Assert.Throws<ArgumentException>(() => new AbilityDefinition(name, GemColor.Red, cost: 1, damage: 1));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Constructor_NonPositiveCost_ThrowsArgumentOutOfRange(int cost)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new AbilityDefinition("Fireball", GemColor.Red, cost, damage: 1));
        }

        [Fact]
        public void Constructor_NegativeDamage_ThrowsArgumentOutOfRange()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new AbilityDefinition("Fireball", GemColor.Red, cost: 1, damage: -1));
        }
    }
}