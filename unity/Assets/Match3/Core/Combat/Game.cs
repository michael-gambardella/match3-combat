using System;
using System.Collections.Generic;
using System.Linq;

using Match3.Core.Cascades;
using Match3.Core.Generation;
using Match3.Core.Grid;
using Match3.Core.Matching;
using Match3.Core.Moves;
using Match3.Core.Randomness;

namespace Match3.Core.Combat
{
    /// <summary>
    /// One encounter: the board, the player's AP and abilities, and the enemy's health. Runs the full
    /// turn flow and reports each turn as an immutable result for the presentation layer to animate.
    /// </summary>
    /// <remarks>
    /// The only mutable type in the core. Everything it holds is immutable, so its state changes only
    /// by replacing whole values in <see cref="PlayMove"/> and <see cref="UseAbility"/>.
    /// </remarks>
    public sealed class Game
    {
        private readonly IRandomSource _random;

        /// <summary>Starts an encounter.</summary>
        /// <param name="rules">The encounter's tunable numbers.</param>
        /// <param name="abilities">The player's loadout.</param>
        /// <param name="random">Drives board generation, refills, and reshuffles.</param>
        /// <param name="startingBoard">
        /// An authored board, for example for a tutorial; when null, a board is generated. An authored board
        /// must match the rules' size, contain no matches, and have a legal move.
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="rules"/>, <paramref name="abilities"/>, or <paramref name="random"/> is null.</exception>
        /// <exception cref="ArgumentException">The loadout contains null, or <paramref name="startingBoard"/> is not playable.</exception>
        public Game(CombatRules rules, IEnumerable<AbilityDefinition> abilities, IRandomSource random, Board? startingBoard = null)
        {
            Rules = rules ?? throw new ArgumentNullException(nameof(rules));
            _random = random ?? throw new ArgumentNullException(nameof(random));
            if (abilities is null)
            {
                throw new ArgumentNullException(nameof(abilities));
            }

            Abilities = Array.AsReadOnly(abilities.ToArray());
            if (Abilities.Contains(null!))
            {
                throw new ArgumentException("The loadout cannot contain null.", nameof(abilities));
            }

            if (startingBoard is null)
            {
                Board = BoardGenerator.Generate(rules.BoardWidth, rules.BoardHeight, random);
            }
            else
            {
                EnsurePlayable(startingBoard, rules);
                Board = startingBoard;
            }

            ActionPoints = ActionPoints.Empty;
            EnemyHealth = rules.EnemyMaxHealth;
            Status = GameStatus.InProgress;
        }

        /// <summary>The encounter's tunable numbers.</summary>
        public CombatRules Rules { get; }

        /// <summary>The player's loadout.</summary>
        public IReadOnlyList<AbilityDefinition> Abilities { get; }

        /// <summary>The current board. While the game is in progress it has no matches and at least one legal move.</summary>
        public Board Board { get; private set; }

        /// <summary>The player's AP.</summary>
        public ActionPoints ActionPoints { get; private set; }

        /// <summary>The enemy's remaining health; never below zero.</summary>
        public int EnemyHealth { get; private set; }

        /// <summary>Moves played so far. Using an ability does not take a turn.</summary>
        public int TurnsTaken { get; private set; }

        /// <summary>Where the encounter stands.</summary>
        public GameStatus Status { get; private set; }

        /// <summary>Returns true when <paramref name="move"/> is legal on the current board.</summary>
        /// <exception cref="ArgumentNullException"><paramref name="move"/> is null.</exception>
        public bool IsLegal(Move move) => MoveFinder.IsLegal(Board, move);

        /// <summary>Returns true when the game is in progress, the ability is in the loadout, and the player can afford it.</summary>
        /// <exception cref="ArgumentNullException"><paramref name="ability"/> is null.</exception>
        public bool CanUse(AbilityDefinition ability)
        {
            if (ability is null)
            {
                throw new ArgumentNullException(nameof(ability));
            }

            return Status == GameStatus.InProgress &&
                Abilities.Contains(ability) &&
                ActionPoints.CanAfford(ability.CostColor, ability.Cost);
        }

        /// <summary>
        /// Plays one turn: swaps the gems, resolves every cascade, awards one AP per cleared gem (of that gem's
        /// color), deals <see cref="CombatRules.DamagePerGem"/> per cleared gem, and updates the status. If the
        /// game continues and the settled board is deadlocked, a new board is generated.
        /// </summary>
        /// <remarks>
        /// <see cref="TurnResult.DamageDealt"/> is the full damage of the move, even when it exceeds the enemy's
        /// remaining health. Defeating the enemy on the last allowed turn is a win.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="move"/> is null.</exception>
        /// <exception cref="InvalidOperationException">The game is over.</exception>
        /// <exception cref="ArgumentException"><paramref name="move"/> is not legal on the current board.</exception>
        public TurnResult PlayMove(Move move)
        {
            if (move is null)
            {
                throw new ArgumentNullException(nameof(move));
            }

            EnsureInProgress();
            if (!IsLegal(move))
            {
                throw new ArgumentException(FormattableString.Invariant($"{move} is not a legal move."), nameof(move));
            }

            CascadeResult cascade = CascadeResolver.Resolve(Board.WithSwap(move.First, move.Second), _random);
            ActionPoints gained = ActionPointsFrom(cascade);
            int damage = gained.Total * Rules.DamagePerGem;

            ActionPoints = ActionPoints.Add(gained);
            Board = cascade.FinalBoard;
            TurnsTaken++;
            DealDamage(damage);

            // Checked after the damage, so defeating the enemy on the last turn is a win.
            if (Status == GameStatus.InProgress && TurnsTaken >= Rules.TurnLimit)
            {
                Status = GameStatus.Lost;
            }

            Board? reshuffled = null;
            if (Status == GameStatus.InProgress && !MoveFinder.HasLegalMove(Board))
            {
                reshuffled = BoardGenerator.Generate(Rules.BoardWidth, Rules.BoardHeight, _random);
                Board = reshuffled;
            }

            return new TurnResult(move, cascade, gained, damage, reshuffled, Status);
        }

        /// <summary>Spends the ability's AP and deals its damage. Using an ability does not take a turn.</summary>
        /// <exception cref="ArgumentNullException"><paramref name="ability"/> is null.</exception>
        /// <exception cref="InvalidOperationException">The game is over, or the player cannot afford the ability.</exception>
        /// <exception cref="ArgumentException"><paramref name="ability"/> is not in this game's loadout.</exception>
        public AbilityResult UseAbility(AbilityDefinition ability)
        {
            if (ability is null)
            {
                throw new ArgumentNullException(nameof(ability));
            }

            EnsureInProgress();
            if (!Abilities.Contains(ability))
            {
                throw new ArgumentException(FormattableString.Invariant($"{ability} is not in this game's loadout."), nameof(ability));
            }

            // Spend throws before the assignment when the player cannot afford it, so a failed call changes nothing.
            ActionPoints = ActionPoints.Spend(ability.CostColor, ability.Cost);
            DealDamage(ability.Damage);
            return new AbilityResult(ability, ability.Damage, Status);
        }

        private static ActionPoints ActionPointsFrom(CascadeResult cascade)
        {
            ActionPoints gained = ActionPoints.Empty;
            foreach (CascadeStep step in cascade.Steps)
            {
                foreach (Match match in step.Matches)
                {
                    gained = gained.Add(match.Color, match.Positions.Count);
                }
            }

            return gained;
        }

        private void EnsureInProgress()
        {
            if (Status != GameStatus.InProgress)
            {
                throw new InvalidOperationException(FormattableString.Invariant($"The game is over ({Status})."));
            }
        }

        private void DealDamage(int damage)
        {
            EnemyHealth = Math.Max(0, EnemyHealth - damage);
            if (EnemyHealth == 0)
            {
                Status = GameStatus.Won;
            }
        }

        private static void EnsurePlayable(Board startingBoard, CombatRules rules)
        {
            if (startingBoard.Width != rules.BoardWidth || startingBoard.Height != rules.BoardHeight)
            {
                throw new ArgumentException(
                    FormattableString.Invariant(
                        $"The starting board is {startingBoard.Width}x{startingBoard.Height}; the rules require {rules.BoardWidth}x{rules.BoardHeight}."),
                    nameof(startingBoard));
            }

            if (MatchFinder.FindMatches(startingBoard).Count > 0)
            {
                throw new ArgumentException("The starting board already contains a match.", nameof(startingBoard));
            }

            if (!MoveFinder.HasLegalMove(startingBoard))
            {
                throw new ArgumentException("The starting board has no legal move.", nameof(startingBoard));
            }
        }
    }
}