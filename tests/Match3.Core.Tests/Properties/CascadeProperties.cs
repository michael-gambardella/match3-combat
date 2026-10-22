using System.Collections.Generic;
using System.Linq;

using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;

using Match3.Core.Cascades;
using Match3.Core.Grid;
using Match3.Core.Matching;
using Match3.Core.Randomness;

using Xunit;

namespace Match3.Core.Tests.Properties
{
    public class CascadeProperties
    {
        [Property]
        public Property Resolve_SettlesToAMatchFreeBoardOfTheSameSize() =>
            Prop.ForAll(GameArbitraries.Boards(), GameArbitraries.Seeds(), (board, seed) =>
            {
                CascadeResult result = CascadeResolver.Resolve(board, new SeededRandom(seed));

                Assert.Equal(board.Width, result.FinalBoard.Width);
                Assert.Equal(board.Height, result.FinalBoard.Height);
                Assert.Empty(MatchFinder.FindMatches(result.FinalBoard));
                Assert.All(result.Steps, step => Assert.NotEmpty(step.Matches));
            });

        [Property]
        public Property Resolve_EachStepIsFullyExplainedByItsFallsAndSpawns() =>
            Prop.ForAll(GameArbitraries.Boards(), GameArbitraries.Seeds(), (board, seed) =>
            {
                // This is the contract the presentation layer relies on: animating only the step's
                // clears, falls, and spawns must reproduce BoardAfter exactly.
                Board before = board;
                foreach (CascadeStep step in CascadeResolver.Resolve(board, new SeededRandom(seed)).Steps)
                {
                    AssertStepExplainsBoard(before, step);
                    before = step.BoardAfter;
                }
            });

        [Property]
        public Property Resolve_SameSeed_ProducesIdenticalSteps() =>
            Prop.ForAll(GameArbitraries.Boards(), GameArbitraries.Seeds(), (board, seed) =>
            {
                CascadeResult first = CascadeResolver.Resolve(board, new SeededRandom(seed));
                CascadeResult second = CascadeResolver.Resolve(board, new SeededRandom(seed));

                Assert.Equal(first.Steps.Select(step => step.BoardAfter), second.Steps.Select(step => step.BoardAfter));
            });

        private static void AssertStepExplainsBoard(Board before, CascadeStep step)
        {
            var cleared = new HashSet<Position>(step.Matches.SelectMany(match => match.Positions));
            Dictionary<Position, GemFall> fallsByDestination = step.Falls.ToDictionary(fall => fall.To);
            Dictionary<Position, GemSpawn> spawnsByPosition = step.Spawns.ToDictionary(spawn => spawn.Position);

            Assert.Equal(cleared.Count, step.Spawns.Count);
            Assert.All(step.Falls, fall => Assert.True(
                fallsByDestination.ContainsKey(fall.From) || spawnsByPosition.ContainsKey(fall.From),
                $"{fall.From} was vacated by a fall but never refilled"));

            foreach (Position cell in before.AllPositions())
            {
                GemColor after = step.BoardAfter.GemAt(cell);
                if (fallsByDestination.TryGetValue(cell, out GemFall? fall))
                {
                    Assert.False(spawnsByPosition.ContainsKey(cell), $"{cell} is both a fall destination and a spawn");
                    Assert.Equal(cell.Column, fall.From.Column);
                    Assert.True(fall.From.Row < cell.Row, $"The fall into {cell} does not move down");
                    Assert.False(cleared.Contains(fall.From), $"The fall into {cell} starts on a cleared cell");
                    Assert.Equal(before.GemAt(fall.From), after);
                }
                else if (spawnsByPosition.TryGetValue(cell, out GemSpawn? spawn))
                {
                    Assert.Equal(spawn.Color, after);
                }
                else
                {
                    Assert.False(cleared.Contains(cell), $"{cell} was cleared but neither refilled nor reached by a fall");
                    Assert.Equal(before.GemAt(cell), after);
                }
            }
        }
    }
}