using System;
using System.Collections.Generic;
using System.Linq;

using Match3.Core.Grid;

using UnityEngine;

namespace Match3.View.Config
{
    /// <summary>How each gem color looks. Artists can restyle every gem here without touching code.</summary>
    [CreateAssetMenu(fileName = "GemPalette", menuName = "Match3/Gem Palette")]
    public sealed class GemPaletteAsset : ScriptableObject
    {
        [SerializeField, Tooltip("One entry per gem color. Give each color a distinct shape, not just a tint, so the board is readable for colorblind players.")]
        private Entry[] _entries = Array.Empty<Entry>();

        /// <summary>The sprite for <paramref name="gem"/>.</summary>
        /// <exception cref="KeyNotFoundException">The palette has no entry for that color.</exception>
        public Sprite SpriteFor(GemColor gem) => Find(gem).Sprite;

        /// <summary>The tint for <paramref name="gem"/>.</summary>
        /// <exception cref="KeyNotFoundException">The palette has no entry for that color.</exception>
        public Color TintFor(GemColor gem) => Find(gem).Tint;

        private Entry Find(GemColor gem)
        {
            foreach (Entry entry in _entries)
            {
                if (entry.Gem == gem)
                {
                    return entry;
                }
            }

            throw new KeyNotFoundException($"The gem palette '{name}' has no entry for {gem}.");
        }

                private void OnValidate()
        {
            foreach (GemColor gem in GemColors.All.Where(gem => _entries.All(entry => entry.Gem != gem)))
            {
                Debug.LogWarning($"{name}: no entry for {gem}.", this);
            }

            // New array entries start fully transparent, and the color picker leaves alpha untouched,
            // so an invisible gem is an easy mistake to make in the Inspector.
            foreach (Entry entry in _entries.Where(entry => entry.Tint.a <= 0f))
            {
                Debug.LogWarning($"{name}: the tint for {entry.Gem} is fully transparent, so those gems will be invisible.", this);
            }
        }

        // Unity cannot serialize a Dictionary, so the palette is a small array searched linearly (six entries).
        [Serializable]
        private struct Entry
        {
            public GemColor Gem;
            public Sprite Sprite;
            public Color Tint;
        }
    }
}