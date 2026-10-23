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

## Cascades

### A cascade is structured steps, not an event stream
`Resolve` returns a `CascadeResult`: one `CascadeStep` per round, then the settled board.
Each step records the matches that cleared, the gems that fell, the gems that spawned, and
the board afterward. Presentation animates a step by clearing the matches, then playing the
falls and spawns together.
*Alternative considered:* a flat stream of clear, fall, and spawn events. Rejected because a
round is played as a group, and the next round is computed from that group's board. A flat
stream would make the presentation layer reconstruct which events belong together.

### Randomness is injected, and the generator is SplitMix64
`Resolve` takes an `IRandomSource`. Tests pass a scripted source and assert every gem; the
game passes `SeededRandom`. `SeededRandom` is SplitMix64, so one seed produces the same
sequence on every platform and runtime.
*Alternative considered:* `System.Random`. Rejected because its seeded sequence is an
implementation detail that is not guaranteed to match between .NET and Unity, which would
break replays and golden tests.

### Row-major draw order is part of the determinism contract
After gravity, each empty cell draws one color, left to right and then top to bottom.
`Spawns` is stored in that same order. The same board and the same sequence then always
place the same colors in the same cells. Walking the holes column by column would also be
deterministic, but it would spend the sequence on different cells, so the contract names
this order.

### Cascades from new gems are allowed
A step can leave a new match, whether gems that were already on the board fell into line or
the refill itself matched. That match is the next step. Resolution stops only when the
board has no matches.

### The settle limit fails loudly
`MaxSteps` is 50. Real boards settle in a handful of steps. If matches remain after that
many steps, `Resolve` throws `InvalidOperationException`.
*Alternative considered:* looping until the board is clear. Rejected because a broken
random source (for example, one that refills every gem with the same color) would hang.

## Moves and generation

### `Move` is a class
A move swaps two orthogonally adjacent cells. The constructor rejects any other pair, and
stores the cells in row-major order, so swapping A with B is the same move as swapping B
with A.
*Alternative considered:* a struct. Rejected because a struct's `default` value bypasses the
constructor and would be a move from a cell to itself.

### Legality is a local constant-time check
`IsLegal` looks only at the row and column through each swapped cell, reading each gem as
it would appear after the swap. Two matching neighbors on a line already complete a run of
three, so each direction stops after two cells. A test checks that this agrees with a full
`MatchFinder` rescan on 200 random boards.
*Alternative considered:* calling `WithSwap` and scanning the whole board. Rejected for
hints and validation, because copying the board makes every check O(W×H).

### Same-colored swaps are never legal
When both cells hold the same color, the swap leaves the board unchanged, so `IsLegal`
returns false. That includes a swap of two gems that already sit inside a run.

### Fills are match-free by construction
`Generate` fills cells in row-major order and skips a color when the two cells to the left,
or the two cells above, already have it, so no run of three can form. The chosen color is
`allowed[random.NextInt(allowed.Count)]`, with `allowed` in `GemColors.All` order. A board
with no legal move is discarded and the same random sequence continues, up to `MaxAttempts`
(100); then `Generate` throws. The fill order and the draw rule are part of the
determinism contract, pinned by golden tests.

### Deadlock reshuffles by regenerating
A board with no legal move is replaced by generating a new one.
*Alternative considered:* a shuffle that preserves the count of each color. Considered and
deferred.

### Random draws index `GemColors.All`
`GemColors.All` lists every `GemColor` in declaration order, and a draw is an index into
that list. Each color occupies one slot regardless of its underlying value, so the values
do not have to be contiguous. The list's order is part of the determinism contract.

## Combat

### `Game` is the only mutable type in the core
Everything a `Game` holds is immutable: the `Board`, the `ActionPoints`, and the results it
returns. Its state changes only by replacing whole values, so all mutation in the core lives
in one class. A result handed to the presentation layer never changes after the fact.

### Two kinds of error, deliberately
A bad argument throws `ArgumentException`: an illegal move, or an ability that is not in the
loadout. A bad time throws `InvalidOperationException`: the game is over, or the player
cannot afford the ability. Callers can tell "you passed the wrong thing" from "you can't do
that right now." Every check runs before any work, so a failed call changes no state and
draws no random numbers.

### `DamageDealt` is the move's full damage
`TurnResult.DamageDealt` is not reduced by the enemy's remaining health, so the UI can show
a big hit even when it overkills. `EnemyHealth` itself is clamped at 0.

### Rules and abilities are plain data
`CombatRules` and `AbilityDefinition` hold only validated numbers and names, with no
behavior. The Unity layer can author them as ScriptableObjects in the Inspector and convert
them to these types, so designers balance encounters without code changes.

### `ActionPoints` is immutable, like `Board`
`Add` and `Spend` return a new pool. Pools can be compared directly in tests and reported
in a `TurnResult` without defensive copies. Because `Spend` throws before `Game` assigns
the result, an unaffordable ability leaves the player's AP untouched.

## Testing strategy

### Properties compare the code with a brute-force oracle
A rule property checks the optimized implementation against the simplest statement of the
rule. A cell is matched when some window of three consecutive cells in its row or column is
one color. A swap is legal when `WithSwap` and a full `MatchFinder` scan find a match
through one of the swapped cells. `FindLegalMoves` must be exactly those adjacent swaps, in
row-major order. The production code stays fast; the tests keep the obvious definition.

### Boards are drawn from two to six colors
A generated board is filled from the first N colors in `GemColors.All`, with N chosen from
2 through 6. Fewer colors make runs, legal moves, and cascades common, so most cases
exercise those paths. A palette of all six colors rarely forms a run, and those boards
seldom reach the matching and cascade code.

### Counterexamples shrink by cropping edges
On failure, smaller boards are made by dropping an edge row or an edge column, and any
smaller board that still fails is kept. The result stays rectangular and full, which is the
only shape a `Board` can have. A planted bug first failed on a 6×7 board; cropping reduced
that counterexample to `YYY`, the smallest board that still broke the rule.

### A cascade step must explain the board it produces
The step-explanation property is the contract with the Unity layer. Playing a step's
clears, then its falls and spawns, must reproduce `BoardAfter` exactly. Each cell of the
result holds the gem that fell into it from the same column, the gem that spawned there, or
the gem that never moved.

### A golden replay guards recorded games
One test plays a seeded game for thirty turns: generate a board, take a random legal move,
resolve the cascade, and regenerate when the board has no legal move. It pins how many
cascade steps ran, how many gems cleared, how many reshuffles happened, and the final
board. The expected values come from an independent implementation of the rules. A failure
means outcomes changed, so a replay recorded with an earlier build would play out
differently. Update the expected values only for an intentional rule change, and say so in
the pull request.

### Each run draws fresh inputs, and a failure prints its replay
No property pins a seed. Every run draws new boards, so a case one run misses can fail on
the next. A failure prints FsCheck's replay value; rerunning with that value regenerates
the same boards, including the shrunk counterexample.
*Alternative considered:* fixing every property to one seed. Rejected because the suite
would stop searching after the first run.