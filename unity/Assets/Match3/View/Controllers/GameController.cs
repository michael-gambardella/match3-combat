using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Match3.Core.Combat;
using Match3.Core.Moves;
using Match3.Core.Randomness;
using Match3.View.Boards;
using Match3.View.Config;
using Match3.View.Interaction;

using UnityEngine;

namespace Match3.View.Controllers
{
    /// <summary>
    /// The composition root of an encounter: builds the <see cref="Game"/> from designer assets, is the only code
    /// that calls it, and keeps the board, input, and HUD in step with it.
    /// </summary>
    /// <remarks>
    /// The game updates instantly and the board then animates the result. Input stays disabled until the
    /// animation finishes, so the player always acts on the board they can see. The read-only properties are
    /// valid once the first game has started, which happens in <c>Start</c> and raises <see cref="StateChanged"/>.
    /// </remarks>
    public sealed class GameController : MonoBehaviour
    {
        [SerializeField, Tooltip("Draws and animates the board.")]
        private BoardView _boardView;

        [SerializeField, Tooltip("Reports the swaps the player requests.")]
        private BoardInput _input;

        [SerializeField, Tooltip("The encounter's tunable numbers.")]
        private CombatRulesAsset _rules;

        [SerializeField, Tooltip("The player's loadout.")]
        private AbilityAsset[] _abilities = Array.Empty<AbilityAsset>();

        [SerializeField, Min(0), Tooltip("0 picks a new seed every run. Any other value replays that exact game.")]
        private int _seed;

        [SerializeField, Tooltip("Log a summary of every turn and ability to the Console.")]
        private bool _logTurns = true;

        private Game _game;

        /// <summary>Raised whenever anything the HUD shows may have changed.</summary>
        public event Action StateChanged;

        /// <summary>Raised when a new game starts, after the board is shown and before the first <see cref="StateChanged"/>.</summary>
        public event Action GameStarted;

        /// <summary>The seed of the current game. Set it in the Inspector to replay the game.</summary>
        public ulong Seed { get; private set; }

        /// <summary>True while a swap is animating; actions are unavailable until it finishes.</summary>
        public bool IsBusy { get; private set; }

        /// <summary>Where the encounter stands.</summary>
        public GameStatus Status => _game.Status;

        /// <summary>The player's loadout: the instances to pass to <see cref="UseAbility"/>.</summary>
        public IReadOnlyList<AbilityDefinition> Abilities => _game.Abilities;

        /// <summary>The player's action points.</summary>
        public ActionPoints ActionPoints => _game.ActionPoints;

        /// <summary>The enemy's remaining health.</summary>
        public int EnemyHealth => _game.EnemyHealth;

        /// <summary>The enemy's starting health.</summary>
        public int EnemyMaxHealth => _game.Rules.EnemyMaxHealth;

        /// <summary>Moves left before the encounter is lost.</summary>
        public int TurnsRemaining => _game.Rules.TurnLimit - _game.TurnsTaken;

        /// <summary>True when the ability can be used right now: no animation is playing and the game allows it.</summary>
        public bool CanUse(AbilityDefinition ability) => !IsBusy && _game.CanUse(ability);

        /// <summary>Uses an ability from <see cref="Abilities"/>.</summary>
        /// <exception cref="InvalidOperationException">The ability cannot be used right now; check <see cref="CanUse"/> first.</exception>
        public void UseAbility(AbilityDefinition ability)
        {
            if (!CanUse(ability))
            {
                throw new InvalidOperationException($"{ability} cannot be used right now.");
            }

            AbilityResult result = _game.UseAbility(ability);
            Log($"Used {ability.Name}: {result.DamageDealt} damage. Enemy {EnemyHealth}/{EnemyMaxHealth}.");
            Refresh();
        }

        /// <summary>Starts a new encounter with a fresh board, or replays the configured seed.</summary>
        /// <exception cref="InvalidOperationException">A swap is still animating.</exception>
        public void StartNewGame()
        {
            if (IsBusy)
            {
                throw new InvalidOperationException("Cannot start a new game while a turn is animating.");
            }

            Seed = _seed != 0 ? (ulong)_seed : (ulong)(Environment.TickCount & int.MaxValue);
            _game = new Game(_rules.ToRules(), _abilities.Select(asset => asset.ToDefinition()), new SeededRandom(Seed));
            _boardView.Show(_game.Board);

            Debug.Log($"New game with seed {Seed}. Set Seed to {Seed} on the GameController to replay it.", this);
            GameStarted?.Invoke();
            Refresh();
        }

        private void OnEnable() => _input.SwapRequested += OnSwapRequested;

        private void OnDisable() => _input.SwapRequested -= OnSwapRequested;

        private void Start() => StartNewGame();

        private void OnSwapRequested(Move move)
        {
            // Input is disabled in both of these states; this is a safety net, not the mechanism.
            if (IsBusy || _game.Status != GameStatus.InProgress)
            {
                return;
            }

            StartCoroutine(PlaySwap(move));
        }

        private IEnumerator PlaySwap(Move move)
        {
            SetBusy(true);

            if (_game.IsLegal(move))
            {
                TurnResult result = _game.PlayMove(move);
                yield return _boardView.PlayTurn(result);

                Log(
                    $"Turn {_game.TurnsTaken}/{_game.Rules.TurnLimit}: {result.Cascade.Steps.Count} cascade step(s), " +
                    $"+{result.ActionPointsGained} AP, {result.DamageDealt} damage" +
                    $"{(result.ReshuffledBoard != null ? ", board reshuffled" : string.Empty)}. Enemy {EnemyHealth}/{EnemyMaxHealth}.");
                Debug.Assert(_boardView.IsShowing(_game.Board), "The board view does not match the game after the turn.", this);
            }
            else
            {
                yield return _boardView.PlayRejectedSwap(move);
            }

            SetBusy(false);
        }

        private void SetBusy(bool busy)
        {
            IsBusy = busy;
            Refresh();
        }

        /// <summary>Applies the current state: input is live only when idle and in progress; the HUD re-reads.</summary>
        private void Refresh()
        {
            _input.enabled = !IsBusy && _game.Status == GameStatus.InProgress;
            if (!IsBusy && _game.Status != GameStatus.InProgress)
            {
                Log($"Game over: {_game.Status}. Enemy {EnemyHealth}/{EnemyMaxHealth}, {_game.TurnsTaken} turn(s) taken.");
            }

            StateChanged?.Invoke();
        }

        private void Log(string message)
        {
            if (_logTurns)
            {
                Debug.Log(message, this);
            }
        }

        // A development aid until the HUD exists: right-click the component header in Play mode.
        [ContextMenu("Debug/Use First Affordable Ability")]
        private void UseFirstAffordableAbility()
        {
            if (_game == null)
            {
                Debug.Log("Enter Play mode first.", this);
                return;
            }

            AbilityDefinition ability = Abilities.FirstOrDefault(CanUse);
            if (ability == null)
            {
                Debug.Log($"No ability is affordable right now. AP: {ActionPoints}.", this);
                return;
            }

            UseAbility(ability);
        }
    }
}