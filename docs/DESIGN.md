CI covers the core, not Unity. Running Unity in CI requires a license and adds several minutes per run. Because Match3.Core has no engine references (enforced by noEngineReferences), all game rules are fully tested without Unity. The Unity layer is verified manually.


Immutable board. Operations return a new Board. That makes boards safe to share, easy to compare in tests, and free to snapshot for replays. An 8×8 board is 64 cells, so copying costs almost nothing.


Always full. Every cell holds a valid gem, and "empty" can't be represented. Empty cells only exist briefly inside the cascade resolver (PR 3), never on a Board.


Flat row-major array. Cells are stored in one array, row by row: index = row * Width + column. It's cache-friendly, cheap to copy, and avoids 2D arrays, which the analyzer's CA1814 rule flags.


Text notation in the core, not just in tests. The same format is used for test fixtures, debug logs and bug reports. "Paste the board" is a great bug-report workflow.


GemAt(Position) instead of an indexer. CA1043 flags indexers that take non-integer arguments, and a named method reads clearly anyway.