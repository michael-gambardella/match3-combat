using System;

using Match3.Core.Combat;
using Match3.Core.Grid;
using Match3.Core.Matching;
using Match3.Core.Moves;
using Match3.Core.Randomness;
using Match3.Core.Tests.Fakes;

using Xunit;

namespace Match3.Core.Tests.Combat
{
    public class GameTests
    {
        // Legal moves: (1,0)<->(1,1) and (1,2)<->(1,3). Swapping (1,0)<->(1,1) makes a red column of three.
        private const string SingleMatchBoard = """
            RYBG
            YRGB
            RBYG
            GYRB
            """;

        // Swapping (1,0)<->(2,0) clears three reds; the falls then form a black T of five, which cascades.
        private const string CascadeBoard = """
            PKRG
            RKBB
            YRRG
            PBKR
            """;

        // Its only legal move, (1,1)<->(1,2), leaves a deadlocked board when the refill is "RPP".
        private const string DeadlockAfterMoveBoard = """
            BGK
            KPG
            GGP
            """;

        private static readonly AbilityDefinition Fireball = new("Fireball", GemColor.Red, cost: 3, damage: 10);

        [Fact]
        public void Constructor_NoStartingBoard_GeneratesAPlayableBoardOfTheRulesSize()
        {
            var game = new Game(Rules(width: 8, height: 8), Array.Empty<AbilityDefinition>(), new SeededRandom(1));

            Assert.Equal(8, game.Board.Width);
            Assert.Equal(8, game.Board.Height);
            Assert.Empty(MatchFinder.FindMatches(game.Board));
            Assert.True(MoveFinder.HasLegalMove(game.Board));
            Assert.Equal(ActionPoints.Empty, game.ActionPoints);
            Assert.Equal(100, game.EnemyHealth);
            Assert.Equal(0, game.TurnsTaken);
            Assert.Equal(GameStatus.InProgress, game.Status);
        }

        [Fact]
        public void Constructor_StartingBoard_IsUsedAsGiven()
        {
            Board board = BoardNotation.Parse(SingleMatchBoard);

            var game = new Game(Rules(), Array.Empty<AbilityDefinition>(), new ScriptedRandom(), board);

            Assert.Equal(board, game.Board);
        }

        [Theory]
        [InlineData("RYB\nYBR\nBRY", 4)]
        [InlineData("RRRY\nYBGB\nGYBP\nBPGY", 4)]
        [InlineData("RYB\nYBR\nBRY", 3)]
        public void Constructor_UnplayableStartingBoard_ThrowsArgumentException(string boardText, int size)
        {
            // Wrong size; already contains a match; deadlocked.
            Assert.Throws<ArgumentException>(() => new Game(
                Rules(width: size, height: size), Array.Empty<AbilityDefinition>(), new ScriptedRandom(), BoardNotation.Parse(boardText)));
        }

        [Fact]
        public void Constructor_NullArguments_ThrowArgumentNull()
        {
            Assert.Throws<ArgumentNullException>(() => new Game(null!, Array.Empty<AbilityDefinition>(), new SeededRandom(1)));
            Assert.Throws<ArgumentNullException>(() => new Game(Rules(), null!, new SeededRandom(1)));
            Assert.Throws<ArgumentNullException>(() => new Game(Rules(), Array.Empty<AbilityDefinition>(), null!));
        }

        [Fact]
        public void PlayMove_SingleMatch_AwardsOneApAndDamagePerClearedGem()
        {
            var random = ScriptedRandom.Spawning("RRY");
            Game game = StartGame(SingleMatchBoard, Rules(damagePerGem: 2), random);

            TurnResult result = game.PlayMove(new Move(P(1, 0), P(1, 1)));

            Assert.Equal(ActionPoints.Empty.Add(GemColor.Red, 3), result.ActionPointsGained);
            Assert.Equal(result.ActionPointsGained, game.ActionPoints);
            Assert.Equal(6, result.DamageDealt);
            Assert.Equal(94, game.EnemyHealth);
            Assert.Equal(1, game.TurnsTaken);
            Assert.Equal(GameStatus.InProgress, result.Status);
            Assert.Null(result.ReshuffledBoard);
            Assert.Equal(result.Cascade.FinalBoard, game.Board);
            AssertBoard(
                """
                RYBG
                RYGB
                YBYG
                GYRB
                """,
                game.Board);
            Assert.Equal(0, random.Remaining);
        }

        [Fact]
        public void PlayMove_Cascade_AwardsApForEveryStep()
        {
            Game game = StartGame(CascadeBoard, Rules(), ScriptedRandom.Spawning("KKKYBBRK"));

            TurnResult result = game.PlayMove(new Move(P(1, 0), P(2, 0)));

            Assert.Equal(2, result.Cascade.Steps.Count);
            Assert.Equal(ActionPoints.Empty.Add(GemColor.Red, 3).Add(GemColor.Black, 5), result.ActionPointsGained);
            Assert.Equal(8, result.DamageDealt);
            AssertBoard(
                """
                YBBG
                PRRB
                YKBG
                PBKR
                """,
                game.Board);
        }

        [Fact]
        public void PlayMove_IllegalMove_ThrowsAndLeavesStateUnchanged()
        {
            Board board = BoardNotation.Parse(SingleMatchBoard);
            var game = new Game(Rules(), Array.Empty<AbilityDefinition>(), new ScriptedRandom(), board);

            Assert.Throws<ArgumentException>(() => game.PlayMove(new Move(P(0, 0), P(0, 1))));

            Assert.Equal(board, game.Board);
            Assert.Equal(0, game.TurnsTaken);
            Assert.Equal(ActionPoints.Empty, game.ActionPoints);
        }

        [Fact]
        public void PlayMove_SettledBoardDeadlocked_ReshufflesToAPlayableBoard()
        {
            var random = new ChainedRandom(ScriptedRandom.Spawning("RPP"), new SeededRandom(7));
            Game game = StartGame(DeadlockAfterMoveBoard, Rules(width: 3, height: 3), random);

            TurnResult result = game.PlayMove(new Move(P(1, 1), P(1, 2)));

            Assert.False(MoveFinder.HasLegalMove(result.Cascade.FinalBoard));
            Assert.NotNull(result.ReshuffledBoard);
            Assert.Equal(result.ReshuffledBoard, game.Board);
            Assert.Empty(MatchFinder.FindMatches(game.Board));
            Assert.True(MoveFinder.HasLegalMove(game.Board));
        }

        [Fact]
        public void PlayMove_GameEndsOnADeadlockedBoard_DoesNotReshuffle()
        {
            // The script holds only the refill; a reshuffle would draw more values and fail.
            Game game = StartGame(DeadlockAfterMoveBoard, Rules(width: 3, height: 3, turnLimit: 1), ScriptedRandom.Spawning("RPP"));

            TurnResult result = game.PlayMove(new Move(P(1, 1), P(1, 2)));

            Assert.Equal(GameStatus.Lost, result.Status);
            Assert.Null(result.ReshuffledBoard);
            Assert.Equal(result.Cascade.FinalBoard, game.Board);
        }

        [Fact]
        public void PlayMove_DefeatsEnemy_WinsWithHealthClampedAtZero()
        {
            Game game = StartGame(SingleMatchBoard, Rules(enemyMaxHealth: 2), ScriptedRandom.Spawning("RRY"));

            TurnResult result = game.PlayMove(new Move(P(1, 0), P(1, 1)));

            Assert.Equal(3, result.DamageDealt);
            Assert.Equal(0, game.EnemyHealth);
            Assert.Equal(GameStatus.Won, result.Status);
            Assert.Equal(GameStatus.Won, game.Status);
        }

        [Fact]
        public void PlayMove_AfterGameOver_ThrowsInvalidOperation()
        {
            Game game = StartGame(SingleMatchBoard, Rules(enemyMaxHealth: 2), ScriptedRandom.Spawning("RRY"));
            game.PlayMove(new Move(P(1, 0), P(1, 1)));

            Assert.Throws<InvalidOperationException>(() => game.PlayMove(new Move(P(0, 0), P(0, 1))));
        }

        [Fact]
        public void PlayMove_TurnLimitReached_Loses()
        {
            Game game = StartGame(SingleMatchBoard, Rules(turnLimit: 1), ScriptedRandom.Spawning("RRY"));

            TurnResult result = game.PlayMove(new Move(P(1, 0), P(1, 1)));

            Assert.Equal(GameStatus.Lost, result.Status);
        }

        [Fact]
        public void PlayMove_EnemyDefeatedOnTheFinalTurn_IsAWin()
        {
            Game game = StartGame(SingleMatchBoard, Rules(enemyMaxHealth: 2, turnLimit: 1), ScriptedRandom.Spawning("RRY"));

            TurnResult result = game.PlayMove(new Move(P(1, 0), P(1, 1)));

            Assert.Equal(GameStatus.Won, result.Status);
        }

        [Fact]
        public void UseAbility_Affordable_SpendsApAndDealsDamageWithoutTakingATurn()
        {
            Game game = StartGame(SingleMatchBoard, Rules(), ScriptedRandom.Spawning("RRY"), Fireball);
            game.PlayMove(new Move(P(1, 0), P(1, 1)));
            Assert.True(game.CanUse(Fireball));

            AbilityResult result = game.UseAbility(Fireball);

            Assert.Equal(10, result.DamageDealt);
            Assert.Equal(87, game.EnemyHealth);
            Assert.Equal(0, game.ActionPoints.AmountOf(GemColor.Red));
            Assert.Equal(1, game.TurnsTaken);
            Assert.False(game.CanUse(Fireball));
        }

        [Fact]
        public void UseAbility_CannotAfford_ThrowsAndLeavesStateUnchanged()
        {
            Game game = StartGame(SingleMatchBoard, Rules(), new ScriptedRandom(), Fireball);

            Assert.False(game.CanUse(Fireball));
            Assert.Throws<InvalidOperationException>(() => game.UseAbility(Fireball));
            Assert.Equal(100, game.EnemyHealth);
            Assert.Equal(ActionPoints.Empty, game.ActionPoints);
        }

        [Fact]
        public void UseAbility_NotInLoadout_ThrowsArgumentException()
        {
            Game game = StartGame(SingleMatchBoard, Rules(), new ScriptedRandom());

            Assert.Throws<ArgumentException>(() => game.UseAbility(Fireball));
        }

        [Fact]
        public void UseAbility_DefeatsEnemy_Wins()
        {
            Game game = StartGame(SingleMatchBoard, Rules(enemyMaxHealth: 13), ScriptedRandom.Spawning("RRY"), Fireball);
            game.PlayMove(new Move(P(1, 0), P(1, 1)));

            AbilityResult result = game.UseAbility(Fireball);

            Assert.Equal(0, game.EnemyHealth);
            Assert.Equal(GameStatus.Won, result.Status);
        }

        [Fact]
        public void PlayMoveAndUseAbility_NullArguments_ThrowArgumentNull()
        {
            Game game = StartGame(SingleMatchBoard, Rules(), new ScriptedRandom());

            Assert.Throws<ArgumentNullException>(() => game.PlayMove(null!));
            Assert.Throws<ArgumentNullException>(() => game.UseAbility(null!));
        }

        private static CombatRules Rules(
            int width = 4, int height = 4, int enemyMaxHealth = 100, int damagePerGem = 1, int turnLimit = 20) =>
            new(width, height, enemyMaxHealth, damagePerGem, turnLimit);

        private static Game StartGame(string boardText, CombatRules rules, IRandomSource random, params AbilityDefinition[] abilities) =>
            new(rules, abilities, random, BoardNotation.Parse(boardText));

        private static Position P(int row, int column) => new(row, column);

        private static void AssertBoard(string expected, Board actual) =>
            Assert.Equal(BoardNotation.Format(BoardNotation.Parse(expected)), actual.ToString());
    }
}