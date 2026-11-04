using System.Globalization;

using Match3.Core.Grid;
using Match3.View.Config;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace Match3.View.Hud
{
    /// <summary>Shows the AP held for one gem color, using that gem's own shape and tint.</summary>
    public sealed class ApCounterView : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _amount;

        /// <summary>The color this counter shows.</summary>
        public GemColor Gem { get; private set; }

        /// <summary>Sets which color this counter shows.</summary>
        public void Bind(GemColor gem, GemPaletteAsset palette)
        {
            Gem = gem;
            _icon.sprite = palette.SpriteFor(gem);
            _icon.color = palette.TintFor(gem);
        }

        /// <summary>Shows the amount held.</summary>
        public void SetAmount(int amount) => _amount.text = amount.ToString(CultureInfo.InvariantCulture);
    }
}