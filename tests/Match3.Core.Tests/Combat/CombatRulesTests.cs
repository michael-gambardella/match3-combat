using System;

using Match3.Core.Combat;

using Xunit;

namespace Match3.Core.Tests.Combat
{
    public class CombatRulesTests
    {
        [Fact]
        public void Constructor_ValidValues_StoresThem()
        {
            var rules = new CombatRules(boardWidth: 8, boardHeight: 7, enemyMaxHealth: 100, damagePerGem: 2, turnLimit: 20);

            Assert.Equal(8, rules.BoardWidth);
            Assert.Equal(7, rules.BoardHeight);
            Assert.Equal(100, rules.EnemyMaxHealth);
            Assert.Equal(2, rules.DamagePerGem);
            Assert.Equal(20, rules.TurnLimit);
        }

        [Theory]
        [InlineData(2, 8, 100, 1, 20)]
        [InlineData(8, 2, 100, 1, 20)]
        [InlineData(8, 8, 0, 1, 20)]
        [InlineData(8, 8, 100, -1, 20)]
        [InlineData(8, 8, 100, 1, 0)]
        public void Constructor_ValueOutOfRange_ThrowsArgumentOutOfRange(
            int boardWidth, int boardHeight, int enemyMaxHealth, int damagePerGem, int turnLimit)
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new CombatRules(boardWidth, boardHeight, enemyMaxHealth, damagePerGem, turnLimit));
        }
    }
}