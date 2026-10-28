using System;

using Match3.Core.Grid;
using Match3.Core.Moves;
using Match3.View.Boards;
using Match3.View.Gems;

using UnityEngine;
using UnityEngine.InputSystem;

namespace Match3.View.Interaction
{
    /// <summary>
    /// Turns pointer input (mouse, pen, or touch) into swap requests: tap a gem then a neighbor, or drag a gem
    /// toward a neighbor. It never calls the game; it only reports the move the player asked for.
    /// </summary>
    /// <remarks>Disable this component to ignore input, for example while a turn is animating.</remarks>
    public sealed class BoardInput : MonoBehaviour
    {
        [SerializeField, Tooltip("The board the player interacts with.")]
        private BoardView _boardView;

        [SerializeField, Tooltip("The camera rendering the board. Defaults to the main camera.")]
        private Camera _camera;

        private Position? _selected;
        private Position? _dragStart;
        private bool _deselectOnRelease;

        /// <summary>
        /// Raised when the player asks to swap two neighboring gems. The move has not been checked for legality;
        /// that is the game's decision.
        /// </summary>
        public event Action<Move> SwapRequested;

        private void Awake()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
            }
        }

        private void OnDisable()
        {
            ClearSelection();
            _dragStart = null;
            _deselectOnRelease = false;
        }

        private void Update()
        {
            Pointer pointer = Pointer.current;
            if (pointer == null)
            {
                return;
            }

            if (pointer.press.wasPressedThisFrame)
            {
                OnPress(pointer);
            }
            else if (pointer.press.wasReleasedThisFrame)
            {
                OnRelease();
            }
            else if (pointer.press.isPressed && _dragStart.HasValue)
            {
                OnDrag(pointer);
            }
        }

        private void OnPress(Pointer pointer)
        {
            if (!TryGetCellUnder(pointer, out Position cell))
            {
                ClearSelection();
                return;
            }

            if (_selected.HasValue && AreNeighbors(_selected.Value, cell))
            {
                RequestSwap(_selected.Value, cell);
                return;
            }

            // Pressing the selected gem again keeps it selected so it can still be dragged;
            // if the press ends without a drag, the release deselects it.
            _deselectOnRelease = _selected == cell;
            Select(cell);
            _dragStart = cell;
        }

        private void OnDrag(Pointer pointer)
        {
            // The out position is set even when the pointer has left the board, which is what a drag off an edge needs.
            TryGetCellUnder(pointer, out Position cell);
            Position start = _dragStart.Value;
            if (cell == start)
            {
                return;
            }

            int rowDelta = cell.Row - start.Row;
            int columnDelta = cell.Column - start.Column;
            Position target = Math.Abs(columnDelta) >= Math.Abs(rowDelta)
                ? new Position(start.Row, start.Column + Math.Sign(columnDelta))
                : new Position(start.Row + Math.Sign(rowDelta), start.Column);

            if (_boardView.Contains(target))
            {
                RequestSwap(start, target);
            }
            else
            {
                ClearSelection();
                _dragStart = null;
                _deselectOnRelease = false;
            }
        }

        private void OnRelease()
        {
            if (_deselectOnRelease)
            {
                ClearSelection();
            }

            _dragStart = null;
            _deselectOnRelease = false;
        }

        private void RequestSwap(Position from, Position to)
        {
            ClearSelection();
            _dragStart = null;
            _deselectOnRelease = false;
            SwapRequested?.Invoke(new Move(from, to));
        }

        private void Select(Position cell)
        {
            ClearSelection();
            _selected = cell;
            _boardView.GemAt(cell).SetSelected(true);
        }

        private void ClearSelection()
        {
            // The board or its gems may already be destroyed when this runs during shutdown.
            if (_selected.HasValue && _boardView != null && _boardView.Contains(_selected.Value))
            {
                GemView gem = _boardView.GemAt(_selected.Value);
                if (gem != null)
                {
                    gem.SetSelected(false);
                }
            }

            _selected = null;
        }

        private bool TryGetCellUnder(Pointer pointer, out Position cell)
        {
            Vector2 screenPoint = pointer.position.ReadValue();
            float depth = Mathf.Abs(_boardView.transform.position.z - _camera.transform.position.z);
            Vector3 worldPoint = _camera.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, depth));
            return _boardView.TryGetPosition(worldPoint, out cell);
        }

        private static bool AreNeighbors(Position a, Position b) =>
            Math.Abs(a.Row - b.Row) + Math.Abs(a.Column - b.Column) == 1;
    }
}