using TMPro;

using UnityEngine;

namespace Match3.View.Hud
{
    /// <summary>
    /// A health bar that drains smoothly to its new value. The fill is a child whose right anchor is set to the
    /// health fraction, so no special sprite or image mode is needed.
    /// </summary>
    public sealed class HealthBarView : MonoBehaviour
    {
        [SerializeField, Tooltip("The fill rectangle, stretched to the bar with zero offsets.")]
        private RectTransform _fill;

        [SerializeField, Tooltip("Shows \"health / max\".")]
        private TMP_Text _label;

        [SerializeField, Min(0.01f), Tooltip("How much of the bar drains per second.")]
        private float _drainSpeed = 1.5f;

        private float _shownFraction = 1f;
        private float _targetFraction = 1f;

        /// <summary>Shows a health value; the bar drains toward it unless <paramref name="instant"/> is true.</summary>
        public void Show(int health, int maxHealth, bool instant)
        {
            _targetFraction = maxHealth > 0 ? Mathf.Clamp01((float)health / maxHealth) : 0f;
            if (instant)
            {
                _shownFraction = _targetFraction;
            }

            _label.text = $"{health} / {maxHealth}";
            ApplyFill();
        }

        private void Update()
        {
            if (Mathf.Approximately(_shownFraction, _targetFraction))
            {
                return;
            }

            _shownFraction = Mathf.MoveTowards(_shownFraction, _targetFraction, _drainSpeed * Time.deltaTime);
            ApplyFill();
        }

        private void ApplyFill() => _fill.anchorMax = new Vector2(_shownFraction, 1f);
    }
}