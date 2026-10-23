using System;
using System.Collections.Generic;
using System.Linq;

using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;

using Match3.Core.Combat;
using Match3.Core.Grid;
using Match3.Core.Matching;
using Match3.Core.Moves;
using Match3.Core.Randomness;

using Xunit;

namespace Match3.Core.Tests.Properties
{
    public class GameProperties
    {
        private static readonly AbilityDefinition Strike = new("Strike", GemColor.Red, cost: 5, damage: 8);

        [Property]
        public Property RandomPlaythrough_KeepsEveryInvariantUntilTheGameEnds() =>
            Prop.ForAll(GameArbitraries.Seeds(), seed =>
            {
                var rules = new CombatRules(boardWidth: 6, boardHeight: 6, enemyMaxHealth: 80, damagePerGem: 1, turnLimit: 15);
                var game = new Game(rules, new[] { Strike }, new SeededRandom(seed));

                // The player's choices come from a separate source, so they do not disturb the game's own sequence.
                var player = new SeededRandom(seed + 1);

                while (game.Status == GameStatus.InProgress)
                {
                    Assert.Empty(MatchFinder.FindMatches(game.Board));
                    IReadOnlyList<Move> moves = MoveFinder.FindLegalMoves(game.Board);
                    Assert.NotEmpty(moves);

                    if (game.CanUse(Strike))
                    {
                        int healthBeforeAbility = game.EnemyHealth;
                        game.UseAbility(Strike);
                        Assert.Equal(Math.Max(0, healthBeforeAbility - Strike.Damage), game.EnemyHealth);
                        continue;
                    }

                    int healthBefore = game.EnemyHealth;
                    ActionPoints pointsBefore = game.ActionPoints;
                    int turnsBefore = game.TurnsTaken;

                    TurnResult result = game.PlayMove(moves[player.NextInt(moves.Count)]);

                    int gemsCleared = result.Cascade.Steps.Sum(step => step.Matches.Sum(match => match.Positions.Count));
                    Assert.True(gemsCleared >= Match.MinimumLength, "A legal move must clear at least one run.");
                    Assert.Equal(gemsCleared, result.ActionPointsGained.Total);
                    Assert.Equal(pointsBefore.Add(result.ActionPointsGained), game.ActionPoints);
                    Assert.Equal(Math.Max(0, healthBefore - result.DamageDealt), game.EnemyHealth);
                    Assert.Equal(turnsBefore + 1, game.TurnsTaken);
                }

                Assert.Equal(game.EnemyHealth == 0, game.Status == GameStatus.Won);
                Assert.True(game.Status == GameStatus.Won || game.TurnsTaken == rules.TurnLimit);
            });
    }
}