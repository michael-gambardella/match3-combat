using System;

using Match3.Core.Combat;
using Match3.View.Config;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace Match3.View.Hud
{
    /// <summary>A button for one ability: its cost color, progress toward the cost, and damage.</summary>
    public sealed class AbilityButtonView : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _label;
        [SerializeField] private Image _costIcon;

        /// <summary>The ability this button uses: the same instance the game holds.</summary>
        public AbilityDefinition Ability { get; private set; }

        /// <summary>Connects the button to an ability and to the action that uses it.</summary>
        public void Bind(AbilityDefinition ability, GemPaletteAsset palette, Action<AbilityDefinition> onClick)
        {
            Ability = ability;
            _costIcon.sprite = palette.SpriteFor(ability.CostColor);
            _costIcon.color = palette.TintFor(ability.CostColor);
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() => onClick(ability));
        }

        /// <summary>Shows progress toward the cost, and enables the button only when the ability is usable.</summary>
        public void Refresh(int pointsHeld, bool usable)
        {
            _label.text = $"{Ability.Name}  {Mathf.Min(pointsHeld, Ability.Cost)}/{Ability.Cost}\n<size=70%>{Ability.Damage} damage</size>";
            _button.interactable = usable;
        }
    }
}