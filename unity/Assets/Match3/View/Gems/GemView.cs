using System.Collections;

using Match3.Core.Grid;
using Match3.View.Animation;
using Match3.View.Config;

using UnityEngine;

namespace Match3.View.Gems
{
    /// <summary>
    /// Displays and animates one gem. It knows nothing about rules or board positions:
    /// the board view decides what it shows and where it goes.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class GemView : MonoBehaviour
    {
        [SerializeField, Min(1f), Tooltip("How much larger the gem is drawn while selected.")]
        private float _selectedScale = 1.15f;

        private SpriteRenderer _renderer;
        private Vector3 _baseScale;

        /// <summary>The color this gem currently shows.</summary>
        public GemColor Gem { get; private set; }

        /// <summary>Shows <paramref name="gem"/> using the palette's sprite and tint, at normal size.</summary>
        public void Show(GemColor gem, GemPaletteAsset palette)
        {
            Gem = gem;
            _renderer.sprite = palette.SpriteFor(gem);
            _renderer.color = palette.TintFor(gem);
            transform.localScale = _baseScale;
        }

        /// <summary>Enlarges the gem while it is selected.</summary>
        public void SetSelected(bool selected) =>
            transform.localScale = selected ? _baseScale * _selectedScale : _baseScale;

        /// <summary>Moves to <paramref name="target"/> (local position), easing into place.</summary>
        public IEnumerator MoveTo(Vector3 target, float duration)
        {
            Vector3 start = transform.localPosition;
            yield return Tween.Run(duration, Easing.OutQuad, t => transform.localPosition = Vector3.LerpUnclamped(start, target, t));
        }

        /// <summary>Shrinks to nothing. The caller destroys the gem afterwards.</summary>
        public IEnumerator Vanish(float duration)
        {
            Vector3 start = transform.localScale;
            yield return Tween.Run(duration, Easing.InQuad, t => transform.localScale = Vector3.LerpUnclamped(start, Vector3.zero, t));
        }

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _baseScale = transform.localScale;
        }
    }
}