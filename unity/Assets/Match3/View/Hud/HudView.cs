using System.Collections.Generic;

using Match3.Core.Combat;
using Match3.Core.Grid;
using Match3.View.Config;
using Match3.View.Controllers;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace Match3.View.Hud
{
    /// <summary>
    /// The encounter HUD. Reads the controller's state and sends commands back through it; it never touches the
    /// game directly and holds no game state of its own. Every change redraws everything, so it cannot drift.
    /// </summary>
    public sealed class HudView : MonoBehaviour
    {
        [SerializeField] private GameController _controller;
        [SerializeField] private GemPaletteAsset _palette;

        [Header("Action points")]
        [SerializeField] private ApCounterView _apCounterPrefab;
        [SerializeField] private Transform _apCounterContainer;

        [Header("Abilities")]
        [SerializeField] private AbilityButtonView _abilityButtonPrefab;
        [SerializeField] private Transform _abilityButtonContainer;

        [Header("Encounter")]
        [SerializeField] private HealthBarView _enemyHealth;
        [SerializeField] private TMP_Text _turnsRemaining;

        [Header("Game over")]
        [SerializeField] private GameObject _banner;
        [SerializeField] private TMP_Text _bannerTitle;
        [SerializeField] private TMP_Text _bannerDetail;
        [SerializeField] private Button _playAgainButton;

        private readonly List<ApCounterView> _apCounters = new List<ApCounterView>();
        private readonly List<AbilityButtonView> _abilityButtons = new List<AbilityButtonView>();

        private void Awake()
        {
            foreach (GemColor gem in GemColors.All)
            {
                ApCounterView counter = Instantiate(_apCounterPrefab, _apCounterContainer);
                counter.Bind(gem, _palette);
                _apCounters.Add(counter);
            }

            _banner.SetActive(false);
        }

        private void OnEnable()
        {
            _controller.GameStarted += OnGameStarted;
            _controller.StateChanged += Refresh;
            _playAgainButton.onClick.AddListener(_controller.StartNewGame);
        }

        private void OnDisable()
        {
            _controller.GameStarted -= OnGameStarted;
            _controller.StateChanged -= Refresh;
            _playAgainButton.onClick.RemoveListener(_controller.StartNewGame);
        }

        // Each game creates new ability instances, so the buttons are rebuilt to hold the ones this game knows.
        private void OnGameStarted()
        {
            foreach (AbilityButtonView button in _abilityButtons)
            {
                Destroy(button.gameObject);
            }

            _abilityButtons.Clear();
            foreach (AbilityDefinition ability in _controller.Abilities)
            {
                AbilityButtonView button = Instantiate(_abilityButtonPrefab, _abilityButtonContainer);
                button.Bind(ability, _palette, OnAbilityClicked);
                _abilityButtons.Add(button);
            }

            _enemyHealth.Show(_controller.EnemyHealth, _controller.EnemyMaxHealth, instant: true);
        }

        // A click can arrive in the same frame the button is disabled, so it is checked rather than trusted.
        private void OnAbilityClicked(AbilityDefinition ability)
        {
            if (_controller.CanUse(ability))
            {
                _controller.UseAbility(ability);
            }
        }

        private void Refresh()
        {
            ActionPoints points = _controller.ActionPoints;
            foreach (ApCounterView counter in _apCounters)
            {
                counter.SetAmount(points.AmountOf(counter.Gem));
            }

            foreach (AbilityButtonView button in _abilityButtons)
            {
                button.Refresh(points.AmountOf(button.Ability.CostColor), _controller.CanUse(button.Ability));
            }

            _enemyHealth.Show(_controller.EnemyHealth, _controller.EnemyMaxHealth, instant: false);
            _turnsRemaining.text = $"Turns left: {_controller.TurnsRemaining}";

            bool isOver = !_controller.IsBusy && _controller.Status != GameStatus.InProgress;
            _banner.SetActive(isOver);
            if (isOver)
            {
                bool won = _controller.Status == GameStatus.Won;
                _bannerTitle.text = won ? "Victory!" : "Defeat";
                _bannerDetail.text = won
                    ? $"Enemy defeated with {_controller.TurnsRemaining} turn(s) to spare."
                    : $"The enemy survived with {_controller.EnemyHealth} health.";
            }
        }
    }
}