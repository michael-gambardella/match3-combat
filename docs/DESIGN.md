# Design Decisions

Why the code is shaped the way it is. Each entry records a decision, the reasoning, and
the alternatives that were rejected.

## Project structure

### The game rules have no engine dependency
All rules live in `Match3.Core`, whose assembly definition sets `noEngineReferences`, so the
compiler rejects any `UnityEngine` usage. Unity is purely a presentation layer that plays back
results from the core. This keeps the rules fast to test, deterministic, and portable to
another engine or a server.

### One copy of the source, compiled by Unity and by dotnet
The core's source lives in `unity/Assets/Match3/Core/`, where Unity compiles it through
`Match3.Core.asmdef`. `src/Match3.Core/Match3.Core.csproj` compiles the same files for tests
and CI. There is no copy step and no prebuilt DLL to keep in sync.
*Alternative considered:* building a DLL into `Assets/Plugins/`. Rejected because every core
change would need a manual rebuild before Unity saw it.

### C# 9, matching Unity
`Directory.Build.props` pins `LangVersion` to 9.0 and disables implicit usings, so code that
passes in dotnet is guaranteed to compile in Unity. Tests are free to use newer C# (such as
raw string literals for board fixtures) because they never run inside Unity.

### CI covers the core, not Unity
Running Unity in CI requires a license and adds several minutes per run. Because the core has
no engine references, every game rule is tested without Unity. The Unity layer is verified
manually.

## Board model

### The board is immutable
Operations such as `WithSwap` return a new `Board`. Boards can be shared without defensive
copies, compared directly in tests, and snapshotted for replays. An 8x8 board is 64 cells,
so copying is cheap.

### The board is always full
Every cell holds a valid gem; an empty cell cannot be represented. Empty cells exist only
temporarily inside the cascade resolver, never on a `Board`, so code that reads a board never
has to handle a missing gem.

### Cells are stored in a flat row-major array
Index = `row * Width + column`. One array is cache-friendly and cheap to clone, and it avoids
multidimensional arrays (flagged by analyzer rule CA1814).

### `GemAt(Position)` instead of an indexer
Analyzer rule CA1043 discourages indexers with non-integer arguments, and a named method reads
clearly at call sites.

### Board text notation lives in the core
`BoardNotation` converts boards to and from compact text (`R Y B G P K`, one row per line).
It serves test fixtures, debug logging, and bug reports: a failing board can be logged,
pasted into a test, and reproduced exactly.

## Matching

### Matches are runs that share a cell, not flood-filled regions
A gem is matched only if it is part of a straight run of three or more. Same-colored runs
that share a cell merge into one match (L, T, and cross shapes); runs that only sit side by
side stay separate.
*Alternative considered:* flood-filling same-colored neighbors. Rejected because it pulls in
gems that touch a run without being part of one, and it merges stacked runs that players see
as two separate matches.

### Runs are merged with union-find
Each row and column is scanned once to collect runs. A dictionary records which run first
claimed each cell; when a later run reaches a claimed cell, the two runs are unioned. Total
time is O(W×H), with path compression adding only a near-constant factor.
*Alternative considered:* repeatedly comparing every pair of runs until nothing changes.
Simpler, but quadratic in the number of runs, and harder to reason about when chains of runs
connect.

### Output order is deterministic
Positions within a match, and matches within a result, are sorted row-major. The same board
always produces the same result, which keeps tests stable, makes replays reproducible, and
gives the presentation layer a consistent animation order. Row-major ordering is defined once,
as `Position.RowMajor`, and exposed as a comparer instead of `IComparable` because positions
have no single natural order.