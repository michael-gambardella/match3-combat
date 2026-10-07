using System;
using System.Linq;

using Match3.Core.Cascades;
using Match3.Core.Grid;
using Match3.Core.Matching;
using Match3.Core.Randomness;
using Match3.Core.Tests.Fakes;

using Xunit;

namespace Match3.Core.Tests.Cascades
{
    public class CascadeResolverTests
    {
        private static readonly int ColorCount = Enum.GetValues<GemColor>().Length;

        [Fact]
        public void Resolve_NoMatches_ReturnsNoStepsAndUnchangedBoard()
        {
            Board board = BoardNotation.Parse("""
                RYB
                YBR
                BRY
                """);

            CascadeResult result = CascadeResolver.Resolve(board, new ScriptedRandom());

            Assert.Empty(result.Steps);
            Assert.Equal(board, result.FinalBoard);
        }

        [Fact]
        public void Resolve_HorizontalMatch_ClearsFallsAndRefillsInOneStep()
        {
            Board board = BoardNotation.Parse("""
                YBGP
                BGPY
                GPYB
                RRRK
                """);
            var random = ScriptedRandom.Spawning("KRB");

            CascadeResult result = CascadeResolver.Resolve(board, random);

            CascadeStep step = Assert.Single(result.Steps);
            Assert.Single(step.Matches);
            AssertBoard(
                """
                KRBP
                YBGY
                BGPB
                GPYK
                """,
                step.BoardAfter);
            Assert.Equal(step.BoardAfter, result.FinalBoard);
            Assert.Equal(0, random.Remaining);
        }

        [Fact]
        public void Resolve_Falls_ListOnlyMovedGemsOrderedByDestinationRowMajor()
        {
            // Column 3 has no cleared cells, so none of its gems move or appear in Falls.
            Board board = BoardNotation.Parse("""
                YBGP
                BGPY
                GPYB
                RRRK
                """);

            CascadeStep step = Assert.Single(CascadeResolver.Resolve(board, ScriptedRandom.Spawning("KRB")).Steps);

            Assert.Equal(
                new[]
                {
                    (P(0, 0), P(1, 0)), (P(0, 1), P(1, 1)), (P(0, 2), P(1, 2)),
                    (P(1, 0), P(2, 0)), (P(1, 1), P(2, 1)), (P(1, 2), P(2, 2)),
                    (P(2, 0), P(3, 0)), (P(2, 1), P(3, 1)), (P(2, 2), P(3, 2)),
                },
                step.Falls.Select(fall => (fall.From, fall.To)));
        }

        [Fact]
        public void Resolve_VerticalMatch_GemAboveFallsPastAllClearedCells()
        {
            Board board = BoardNotation.Parse("""
                BRB
                YRR
                YPP
                YRB
                """);

            CascadeStep step = Assert.Single(CascadeResolver.Resolve(board, ScriptedRandom.Spawning("PYG")).Steps);

            GemFall fall = Assert.Single(step.Falls);
            Assert.Equal((P(0, 0), P(3, 0)), (fall.From, fall.To));
            Assert.Equal(
                new[] { (P(0, 0), GemColor.Purple), (P(1, 0), GemColor.Yellow), (P(2, 0), GemColor.Green) },
                step.Spawns.Select(spawn => (spawn.Position, spawn.Color)));
            AssertBoard(
                """
                PRB
                YRR
                GPP
                BRB
                """,
                step.BoardAfter);
        }

        [Fact]
        public void Resolve_Spawns_DrawColorsInRowMajorOrder()
        {
            // Two stacked matches leave two empty rows. Drawing column by column instead of
            // row by row would put PGY / BRK on top instead of PBG / RYK.
            Board board = BoardNotation.Parse("""
                YBR
                GGK
                YYY
                GGG
                """);

            CascadeStep step = Assert.Single(CascadeResolver.Resolve(board, ScriptedRandom.Spawning("PBGRYK")).Steps);

            AssertBoard(
                """
                PBG
                RYK
                YBR
                GGK
                """,
                step.BoardAfter);
        }

        [Fact]
        public void Resolve_FallingGemsFormNewMatch_CascadesASecondStep()
        {
            Board board = BoardNotation.Parse("""
                KPGP
                PKGY
                YRRR
                PRGY
                """);

            CascadeResult result = CascadeResolver.Resolve(board, ScriptedRandom.Spawning("YYRRRP"));

            Assert.Equal(2, result.Steps.Count);
            AssertBoard(
                """
                KYYR
                PPGP
                YKGY
                PRGY
                """,
                result.Steps[0].BoardAfter);
            Match cascade = Assert.Single(result.Steps[1].Matches);
            Assert.Equal(new[] { P(1, 2), P(2, 2), P(3, 2) }, cascade.Positions);
            AssertBoard(
                """
                KYRR
                PPRP
                YKPY
                PRYY
                """,
                result.FinalBoard);
        }

        [Fact]
        public void Resolve_NewGemsFormMatch_CascadesASecondStep()
        {
            Board board = BoardNotation.Parse("""
                RPBB
                YBGP
                YYYK
                PPYR
                """);

            CascadeResult result = CascadeResolver.Resolve(board, ScriptedRandom.Spawning("RRRGKP"));

            Assert.Equal(2, result.Steps.Count);
            AssertBoard(
                """
                RRRB
                RPBP
                YBGK
                PPYR
                """,
                result.Steps[0].BoardAfter);
            Match cascade = Assert.Single(result.Steps[1].Matches);
            Assert.Equal(new[] { P(0, 0), P(0, 1), P(0, 2) }, cascade.Positions);
            AssertBoard(
                """
                GKPB
                RPBP
                YBGK
                PPYR
                """,
                result.FinalBoard);
        }

        [Fact]
        public void Resolve_SameSeed_ProducesIdenticalResolution()
        {
            Board board = RandomBoard(new SeededRandom(7));

            CascadeResult first = CascadeResolver.Resolve(board, new SeededRandom(99));
            CascadeResult second = CascadeResolver.Resolve(board, new SeededRandom(99));

            Assert.NotEmpty(first.Steps);
            Assert.Equal(first.Steps.Select(step => step.BoardAfter), second.Steps.Select(step => step.BoardAfter));
            Assert.Equal(first.FinalBoard, second.FinalBoard);
        }

        [Fact]
        public void Resolve_RandomBoards_AlwaysSettleWithNoMatches()
        {
            for (ulong seed = 0; seed < 100; seed++)
            {
                var random = new SeededRandom(seed);

                CascadeResult result = CascadeResolver.Resolve(RandomBoard(random), random);

                Assert.Empty(MatchFinder.FindMatches(result.FinalBoard));
                Assert.All(result.Steps, step => Assert.NotEmpty(step.Matches));
            }
        }

        [Fact]
        public void Resolve_RandomSourceThatNeverLetsBoardSettle_ThrowsInsteadOfLooping()
        {
            // Every new gem is red, so the top row refills as RRR and matches forever.
            Board board = BoardNotation.Parse("""
                RRR
                YBG
                BGY
                """);

            Assert.Throws<InvalidOperationException>(() => CascadeResolver.Resolve(board, new ConstantRandom(0)));
        }

        [Fact]
        public void Resolve_NullBoard_ThrowsArgumentNull()
        {
            Assert.Throws<ArgumentNullException>(() => CascadeResolver.Resolve(null!, new ScriptedRandom()));
        }

        [Fact]
        public void Resolve_NullRandom_ThrowsArgumentNull()
        {
            Assert.Throws<ArgumentNullException>(() => CascadeResolver.Resolve(BoardNotation.Parse("RYB"), null!));
        }

        private static Position P(int row, int column) => new(row, column);

        private static Board RandomBoard(SeededRandom random) =>
            Board.Create(8, 8, _ => (GemColor)random.NextInt(ColorCount));

        private static void AssertBoard(string expected, Board actual) =>
            Assert.Equal(BoardNotation.Format(BoardNotation.Parse(expected)), actual.ToString());
    }
}