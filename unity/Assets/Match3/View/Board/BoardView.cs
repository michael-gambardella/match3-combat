using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Match3.Core.Cascades;
using Match3.Core.Combat;
using Match3.Core.Grid;
using Match3.Core.Moves;
using Match3.View.Animation;
using Match3.View.Config;
using Match3.View.Gems;

using UnityEngine;

namespace Match3.View.Boards
{
    /// <summary>
    /// Draws a board as gem views, converts between board positions and world space, and plays back turns.
    /// The single place that knows the board's on-screen layout: row 0 is the top row, and the board is
    /// centered on this transform.
    /// </summary>
    /// <remarks>
    /// Playback follows a <see cref="TurnResult"/> exactly and derives nothing itself. The gem array is updated
    /// to final cells as each animation starts, so the bookkeeping is never between two states.
    /// </remarks>
    public sealed class BoardView : MonoBehaviour
    {
        [SerializeField, Tooltip("The gem prefab created for each cell.")]
        private GemView _gemPrefab;

        [SerializeField, Tooltip("How each gem color looks.")]
        private GemPaletteAsset _palette;

        [SerializeField, Min(0.1f), Tooltip("World units between neighboring cell centers.")]
        private float _cellSize = 1f;

        [Header("Animation")]
        [SerializeField, Min(0f), Tooltip("Seconds for two gems to trade places.")]
        private float _swapDuration = 0.15f;

        [SerializeField, Min(0f), Tooltip("Seconds for matched gems to shrink away.")]
        private float _clearDuration = 0.2f;

        [SerializeField, Min(0.01f), Tooltip("Seconds a gem takes to fall one cell. Longer falls take proportionally longer.")]
        private float _fallSecondsPerCell = 0.07f;

        private GemView[] _gems = Array.Empty<GemView>();

        /// <summary>Columns in the board currently shown.</summary>
        public int Width { get; private set; }

        /// <summary>Rows in the board currently shown.</summary>
        public int Height { get; private set; }

        /// <summary>Replaces every gem with views of <paramref name="board"/>, instantly.</summary>
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
                    Place(CreateGem(board.GemAt(position), LocalPositionOf(position)), position);
                }
            }
        }

        /// <summary>Plays a whole turn: the swap, every cascade step, then the reshuffle if there was one.</summary>
        /// <exception cref="ArgumentNullException"><paramref name="result"/> is null.</exception>
        public IEnumerator PlayTurn(TurnResult result)
        {
            if (result is null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            yield return PlaySwap(result.Move);
            foreach (CascadeStep step in result.Cascade.Steps)
            {
                yield return PlayStep(step);
            }

            if (result.ReshuffledBoard != null)
            {
                yield return PlayReshuffle(result.ReshuffledBoard);
            }
        }

        /// <summary>Swaps two gems and swaps them back, for a move the game rejected.</summary>
        public IEnumerator PlayRejectedSwap(Move move)
        {
            yield return PlaySwap(move);
            yield return PlaySwap(move);
        }

        /// <summary>The gem view currently in <paramref name="position"/>.</summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="position"/> is off the board.</exception>
        public GemView GemAt(Position position) => _gems[IndexOf(position)];

        /// <summary>The center of a cell, relative to this transform. Rows above the board (negative) are valid.</summary>
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

        /// <summary>True when <paramref name="position"/> is a cell of the board currently shown.</summary>
        public bool Contains(Position position) =>
            position.Row >= 0 && position.Row < Height && position.Column >= 0 && position.Column < Width;

        private IEnumerator PlaySwap(Move move)
        {
            GemView first = TakeGem(move.First);
            GemView second = TakeGem(move.Second);
            Place(first, move.Second);
            Place(second, move.First);

            yield return CoroutineGroup.All(this, new[]
            {
                first.MoveTo(LocalPositionOf(move.Second), _swapDuration),
                second.MoveTo(LocalPositionOf(move.First), _swapDuration),
            });
        }

        private IEnumerator PlayStep(CascadeStep step)
        {
            // 1. Clear. Matches never overlap, so each cell is taken once; a repeat would throw in TakeGem.
            List<GemView> cleared = step.Matches.SelectMany(match => match.Positions).Select(TakeGem).ToList();
            yield return CoroutineGroup.All(this, cleared.Select(gem => gem.Vanish(_clearDuration)));
            foreach (GemView gem in cleared)
            {
                Destroy(gem.gameObject);
            }

            // 2. Falls and spawns together. Take every falling gem before placing any: a gem can land in a cell
            //    that another falling gem has just left.
            var motions = new List<IEnumerator>();
            var falling = step.Falls.Select(fall => (Gem: TakeGem(fall.From), fall.From, fall.To)).ToList();
            foreach (var (gem, from, to) in falling)
            {
                Place(gem, to);
                motions.Add(gem.MoveTo(LocalPositionOf(to), (to.Row - from.Row) * _fallSecondsPerCell));
            }

            // New gems start stacked above the board, so a column's spawns drop in together like a single stream.
            foreach (IGrouping<int, GemSpawn> column in step.Spawns.GroupBy(spawn => spawn.Position.Column))
            {
                int dropRows = column.Count();
                foreach (GemSpawn spawn in column)
                {
                    var start = new Position(spawn.Position.Row - dropRows, spawn.Position.Column);
                    GemView gem = CreateGem(spawn.Color, LocalPositionOf(start));
                    Place(gem, spawn.Position);
                    motions.Add(gem.MoveTo(LocalPositionOf(spawn.Position), dropRows * _fallSecondsPerCell));
                }
            }

            yield return CoroutineGroup.All(this, motions);

            // 3. Compiled only into the editor and development builds.
            Debug.Assert(IsShowing(step.BoardAfter), "The board view does not match the cascade step's board.", this);
        }

        private IEnumerator PlayReshuffle(Board board)
        {
            yield return CoroutineGroup.All(this, AllGems().Select(gem => gem.Vanish(_clearDuration)));
            Show(board);
            yield return CoroutineGroup.All(this, AllGems().Select(gem => gem.Appear(_clearDuration)));
        }

        private GemView CreateGem(GemColor color, Vector3 localPosition)
        {
            GemView gem = Instantiate(_gemPrefab, transform);
            gem.transform.localPosition = localPosition;
            gem.Show(color, _palette);
            return gem;
        }

        /// <summary>Removes and returns the gem in a cell, leaving the cell empty.</summary>
        private GemView TakeGem(Position position)
        {
            int index = IndexOf(position);
            GemView gem = _gems[index];
            if (gem == null)
            {
                throw new InvalidOperationException($"There is no gem at {position} to take.");
            }

            _gems[index] = null;
            return gem;
        }

        /// <summary>Puts a gem in an empty cell. Throws if the cell is occupied, so bookkeeping mistakes fail loudly.</summary>
        private void Place(GemView gem, Position position)
        {
            int index = IndexOf(position);
            if (_gems[index] != null)
            {
                throw new InvalidOperationException($"Cannot place a gem at {position}: the cell is occupied.");
            }

            _gems[index] = gem;
            gem.name = $"Gem {position}";
        }

        private IEnumerable<GemView> AllGems() => _gems.Where(gem => gem != null);

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