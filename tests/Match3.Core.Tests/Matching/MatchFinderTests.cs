using System;

using Match3.Core.Grid;
using Match3.Core.Matching;

using Xunit;

namespace Match3.Core.Tests.Matching
{
    public class MatchFinderTests
    {
        [Fact]
        public void FindMatches_NoRuns_ReturnsEmpty()
        {
            MatchAssert.FindsExactly("""
                RYB
                YBR
                BRY
                """);
        }

        [Fact]
        public void FindMatches_TwoInARow_IsNotAMatch()
        {
            MatchAssert.FindsExactly("""
                RRY
                YBR
                BYB
                """);
        }

        [Fact]
        public void FindMatches_BoardSmallerThanARun_ReturnsEmpty()
        {
            MatchAssert.FindsExactly("""
                RY
                YR
                """);
        }

        [Fact]
        public void FindMatches_HorizontalThree_FindsOneMatch()
        {
            MatchAssert.FindsExactly(
                """
                RRRY
                YBGB
                GYBG
                """,
                """
                RRR.
                ....
                ....
                """);
        }

        [Fact]
        public void FindMatches_VerticalThree_FindsOneMatch()
        {
            MatchAssert.FindsExactly(
                """
                GYB
                GBY
                GYB
                """,
                """
                G..
                G..
                G..
                """);
        }

        [Fact]
        public void FindMatches_RunOfFive_IsOneMatch()
        {
            MatchAssert.FindsExactly(
                """
                BBBBB
                YGRGY
                GRYRG
                """,
                """
                BBBBB
                .....
                .....
                """);
        }

        [Fact]
        public void FindMatches_TwoRunsInSameRowSeparatedByOtherGem_AreSeparateMatches()
        {
            MatchAssert.FindsExactly(
                """
                RRRGRRR
                GYBYBYG
                """,
                """
                RRR....
                .......
                """,
                """
                ....RRR
                .......
                """);
        }

        [Fact]
        public void FindMatches_LShape_MergesIntoOneMatch()
        {
            MatchAssert.FindsExactly(
                """
                RYB
                RBY
                RRR
                """,
                """
                R..
                R..
                RRR
                """);
        }

        [Fact]
        public void FindMatches_TShape_MergesIntoOneMatch()
        {
            MatchAssert.FindsExactly(
                """
                GGGY
                BGYB
                YGBY
                """,
                """
                GGG.
                .G..
                .G..
                """);
        }

        [Fact]
        public void FindMatches_Cross_MergesIntoOneMatch()
        {
            MatchAssert.FindsExactly(
                """
                YRY
                RRR
                YRY
                """,
                """
                .R.
                RRR
                .R.
                """);
        }

        [Fact]
        public void FindMatches_ParallelRunsThatOnlyTouch_StaySeparate()
        {
            MatchAssert.FindsExactly(
                """
                RRRG
                RRRB
                GBYG
                """,
                """
                RRR.
                ....
                ....
                """,
                """
                ....
                RRR.
                ....
                """);
        }

        [Fact]
        public void FindMatches_SameColorGemTouchingRunButNotInARun_IsExcluded()
        {
            // A flood fill would wrongly include the R at (1,1).
            MatchAssert.FindsExactly(
                """
                RRRG
                BRGB
                GBYG
                """,
                """
                RRR.
                ....
                ....
                """);
        }

        [Fact]
        public void FindMatches_DifferentColors_AreSeparateMatches()
        {
            MatchAssert.FindsExactly(
                """
                RRRB
                GYGB
                YGYB
                """,
                """
                RRR.
                ....
                ....
                """,
                """
                ...B
                ...B
                ...B
                """);
        }

        [Fact]
        public void FindMatches_MultipleMatches_AreOrderedByFirstPositionRowMajor()
        {
            // The vertical run starts at (0,0), before the horizontal run at (3,0),
            // so it must come first even if rows are scanned before columns.
            MatchAssert.FindsExactly(
                """
                GYBY
                GBYB
                GYBY
                RRRB
                """,
                """
                G...
                G...
                G...
                ....
                """,
                """
                ....
                ....
                ....
                RRR.
                """);
        }

        [Fact]
        public void FindMatches_TShape_ListsSharedCellOnceInRowMajorOrder()
        {
            Board board = BoardNotation.Parse("""
                GGGY
                BGYB
                YGBY
                """);

            Match match = Assert.Single(MatchFinder.FindMatches(board));

            Assert.Equal(GemColor.Green, match.Color);
            Assert.Equal(
                new[] { new Position(0, 0), new Position(0, 1), new Position(0, 2), new Position(1, 1), new Position(2, 1) },
                match.Positions);
        }

        [Fact]
        public void FindMatches_NullBoard_ThrowsArgumentNull()
        {
            Assert.Throws<ArgumentNullException>(() => MatchFinder.FindMatches(null!));
        }
    }
}