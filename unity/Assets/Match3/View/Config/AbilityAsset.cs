using System;

using Match3.Core.Combat;
using Match3.Core.Grid;

using UnityEngine;

namespace Match3.View.Config
{
    /// <summary>
    /// A designer-editable ability, converted to the core's <see cref="AbilityDefinition"/> when a game starts.
    /// </summary>
    /// <remarks>
    /// Each call to <see cref="ToDefinition"/> creates a new definition. The core identifies abilities by instance,
    /// so the game controller converts each asset once and uses those same objects for the loadout and the buttons.
    /// </remarks>
    [CreateAssetMenu(fileName = "Ability", menuName = "Match3/Ability")]
    public sealed class AbilityAsset : ScriptableObject
    {
        [SerializeField, Tooltip("Shown on the ability button.")]
        private string _displayName = "Fireball";

        [SerializeField, Tooltip("The AP color the ability spends.")]
        private GemColor _costColor = GemColor.Red;

        [SerializeField, Min(1), Tooltip("AP spent per use.")]
        private int _cost = 7;

        [SerializeField, Min(0), Tooltip("Damage dealt to the enemy.")]
        private int _damage = 20;

        /// <summary>Creates the core definition from the current values.</summary>
        /// <exception cref="ArgumentException">The name is blank or a value is out of range.</exception>
        public AbilityDefinition ToDefinition() => new AbilityDefinition(_displayName, _costColor, _cost, _damage);

        private void OnValidate()
        {
            try
            {
                ToDefinition();
            }
            catch (ArgumentException exception)
            {
                Debug.LogError($"{name}: {exception.Message}", this);
            }
        }
    }
}