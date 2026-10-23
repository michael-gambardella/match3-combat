using System;

using Match3.Core.Combat;
using Match3.Core.Grid;

using Xunit;

namespace Match3.Core.Tests.Combat
{
    public class ActionPointsTests
    {
        [Fact]
        public void Empty_HoldsNoApOfAnyColor()
        {
            Assert.All(GemColors.All, color => Assert.Equal(0, ActionPoints.Empty.AmountOf(color)));
            Assert.Equal(0, ActionPoints.Empty.Total);
        }

        [Fact]
        public void Add_IncreasesOnlyThatColorAndLeavesTheOriginalUnchanged()
        {
            ActionPoints points = ActionPoints.Empty.Add(GemColor.Red, 3);

            Assert.Equal(3, points.AmountOf(GemColor.Red));
            Assert.Equal(0, points.AmountOf(GemColor.Blue));
            Assert.Equal(3, points.Total);
            Assert.Equal(0, ActionPoints.Empty.Total);
        }

        [Fact]
        public void Add_AnotherPool_SumsEachColor()
        {
            ActionPoints first = ActionPoints.Empty.Add(GemColor.Red, 2).Add(GemColor.Blue, 1);
            ActionPoints second = ActionPoints.Empty.Add(GemColor.Red, 1).Add(GemColor.Black, 4);

            ActionPoints sum = first.Add(second);

            Assert.Equal(3, sum.AmountOf(GemColor.Red));
            Assert.Equal(1, sum.AmountOf(GemColor.Blue));
            Assert.Equal(4, sum.AmountOf(GemColor.Black));
        }

        [Fact]
        public void Spend_AffordableAmount_DecreasesThatColor()
        {
            ActionPoints points = ActionPoints.Empty.Add(GemColor.Green, 5).Spend(GemColor.Green, 3);

            Assert.Equal(2, points.AmountOf(GemColor.Green));
        }

        [Fact]
        public void Spend_MoreThanAvailable_ThrowsInvalidOperation()
        {
            ActionPoints points = ActionPoints.Empty.Add(GemColor.Green, 2);

            Assert.Throws<InvalidOperationException>(() => points.Spend(GemColor.Green, 3));
        }

        [Fact]
        public void AddAndSpend_NegativeAmount_ThrowArgumentOutOfRange()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => ActionPoints.Empty.Add(GemColor.Red, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => ActionPoints.Empty.Spend(GemColor.Red, -1));
        }

        [Theory]
        [InlineData(2, true)]
        [InlineData(3, true)]
        [InlineData(4, false)]
        public void CanAfford_ComparesCostWithAmountHeld(int cost, bool expected)
        {
            ActionPoints points = ActionPoints.Empty.Add(GemColor.Red, 3);

            Assert.Equal(expected, points.CanAfford(GemColor.Red, cost));
        }

        [Fact]
        public void Equals_SameAmounts_AreEqualWithSameHashCode()
        {
            ActionPoints first = ActionPoints.Empty.Add(GemColor.Red, 1).Add(GemColor.Black, 2);
            ActionPoints second = ActionPoints.Empty.Add(GemColor.Black, 2).Add(GemColor.Red, 1);

            Assert.Equal(first, second);
            Assert.Equal(first.GetHashCode(), second.GetHashCode());
        }

        [Fact]
        public void ToString_ListsColorsWithAp()
        {
            Assert.Equal("Red 3, Black 5", ActionPoints.Empty.Add(GemColor.Black, 5).Add(GemColor.Red, 3).ToString());
            Assert.Equal("none", ActionPoints.Empty.ToString());
        }
    }
}