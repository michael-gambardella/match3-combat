using System;

using Match3.Core.Combat;

using UnityEngine;

namespace Match3.View.Config
{
    /// <summary>
    /// Designer-editable combat rules, converted to the core's immutable <see cref="CombatRules"/> when a game starts.
    /// </summary>
    /// <remarks>
    /// Validation is not duplicated here: <see cref="OnValidate"/> builds the core rules and reports the core's own
    /// error, so the editor and the game always agree on what is valid.
    /// </remarks>
    [CreateAssetMenu(fileName = "CombatRules", menuName = "Match3/Combat Rules")]
    public sealed class CombatRulesAsset : ScriptableObject
    {
        [SerializeField, Min(CombatRules.MinimumBoardSize), Tooltip("Columns on the board.")]
        private int _boardWidth = 8;

        [SerializeField, Min(CombatRules.MinimumBoardSize), Tooltip("Rows on the board.")]
        private int _boardHeight = 8;

        [SerializeField, Min(1), Tooltip("The enemy's starting health.")]
        private int _enemyMaxHealth = 150;

        [SerializeField, Min(0), Tooltip("Damage dealt for every gem cleared, including gems cleared by cascades.")]
        private int _damagePerGem = 1;

        [SerializeField, Min(1), Tooltip("Moves allowed before the encounter is lost.")]
        private int _turnLimit = 25;

        /// <summary>Creates the core rules from the current values.</summary>
        /// <exception cref="ArgumentOutOfRangeException">A value is out of range.</exception>
        public CombatRules ToRules() =>
            new CombatRules(_boardWidth, _boardHeight, _enemyMaxHealth, _damagePerGem, _turnLimit);

        private void OnValidate()
        {
            try
            {
                ToRules();
            }
            catch (ArgumentException exception)
            {
                Debug.LogError($"{name}: {exception.Message}", this);
            }
        }
    }
}