using System;

using Match3.Core.Grid;
using Match3.View.Config;
using Match3.View.Gems;

using UnityEngine;

namespace Match3.View.Boards
{
    /// <summary>
    /// Draws a board as gem views and converts between board positions and world space.
    /// The single place that knows the board's on-screen layout: row 0 is the top row,
    /// and the board is centered on this transform.
    /// </summary>
    public sealed class BoardView : MonoBehaviour
    {
        [SerializeField, Tooltip("The gem prefab created for each cell.")]
        private GemView _gemPrefab;

        [SerializeField, Tooltip("How each gem color looks.")]
        private GemPaletteAsset _palette;

        [SerializeField, Min(0.1f), Tooltip("World units between neighboring cell centers.")]
        private float _cellSize = 1f;

        private GemView[] _gems = Array.Empty<GemView>();

        /// <summary>Columns in the board currently shown.</summary>
        public int Width { get; private set; }

        /// <summary>Rows in the board currently shown.</summary>
        public int Height { get; private set; }

        /// <summary>Replaces every gem with views of <paramref name="board"/>. Used at game start and after a reshuffle.</summary>
        /// <exception cref="ArgumentNullException"><paramref name="board"/> is null.</exception>
        public void Show(Board board)
        {
            if (board is null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            DestroyGems();
            Width = board.Width;
            Height = board.Height;
            _gems = new GemView[Width * Height];

            for (int row = 0; row < Height; row++)
            {
                for (int column = 0; column < Width; column++)
                {
                    var position = new Position(row, column);
                    GemView gem = Instantiate(_gemPrefab, transform);
                    gem.name = $"Gem {position}";
                    gem.transform.localPosition = LocalPositionOf(position);
                    gem.Show(board.GemAt(position), _palette);
                    _gems[IndexOf(position)] = gem;
                }
            }
        }

        /// <summary>The gem view currently in <paramref name="position"/>.</summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="position"/> is off the board.</exception>
        public GemView GemAt(Position position) => _gems[IndexOf(position)];

        /// <summary>The center of a cell, relative to this transform.</summary>
        public Vector3 LocalPositionOf(Position position) =>
            new Vector3(
                (position.Column - ((Width - 1) / 2f)) * _cellSize,
                (((Height - 1) / 2f) - position.Row) * _cellSize,
                0f);

        /// <summary>Finds the cell under a world-space point, such as the pointer.</summary>
        /// <returns>True when the point lies over a cell of the board.</returns>
        public bool TryGetPosition(Vector3 worldPoint, out Position position)
        {
            Vector3 local = transform.InverseTransformPoint(worldPoint);

            // Floor(x + 0.5) rounds to the nearest cell consistently. Mathf.RoundToInt rounds halves to even,
            // which would make points exactly between cells fall to different sides depending on the cell.
            int column = Mathf.FloorToInt((local.x / _cellSize) + ((Width - 1) / 2f) + 0.5f);
            int row = Mathf.FloorToInt(((Height - 1) / 2f) - (local.y / _cellSize) + 0.5f);

            position = new Position(row, column);
            return Contains(position);
        }

        /// <summary>
        /// True when the views show exactly <paramref name="board"/>: the right color in every cell, each at its
        /// cell's center. A cheap guard against the view drifting out of sync with the game.
        /// </summary>
        public bool IsShowing(Board board)
        {
            if (board is null || board.Width != Width || board.Height != Height)
            {
                return false;
            }

            for (int row = 0; row < Height; row++)
            {
                for (int column = 0; column < Width; column++)
                {
                    var position = new Position(row, column);
                    GemView gem = _gems[IndexOf(position)];

                    // Unity's == on Vector3 compares approximately, which is what an animated position needs.
                    if (gem == null || gem.Gem != board.GemAt(position) || gem.transform.localPosition != LocalPositionOf(position))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private bool Contains(Position position) =>
            position.Row >= 0 && position.Row < Height && position.Column >= 0 && position.Column < Width;

        private int IndexOf(Position position)
        {
            if (!Contains(position))
            {
                throw new ArgumentOutOfRangeException(nameof(position), position, $"Outside the {Width}x{Height} board view.");
            }

            return (position.Row * Width) + position.Column;
        }

        private void DestroyGems()
        {
            foreach (GemView gem in _gems)
            {
                if (gem != null)
                {
                    Destroy(gem.gameObject);
                }
            }

            _gems = Array.Empty<GemView>();
        }
    }
}