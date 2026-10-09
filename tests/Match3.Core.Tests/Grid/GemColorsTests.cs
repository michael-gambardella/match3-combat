using Match3.Core.Grid;

using Xunit;

namespace Match3.Core.Tests.Grid
{
    public class GemColorsTests
    {
        [Fact]
        public void All_ListsEveryColorInDeclarationOrder()
        {
            Assert.Equal(
                [GemColor.Red, GemColor.Yellow, GemColor.Blue, GemColor.Green, GemColor.Purple, GemColor.Black],
                GemColors.All);
        }
    }
}