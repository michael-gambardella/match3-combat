using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Match3.Core.Grid;
using Match3.Core.Matching;

namespace Match3.Core.Cascades
{
    /// <summary>
    /// One round of a cascade: matched gems clear, surviving gems fall, and new gems fill the
    /// empty cells. A presentation layer animates a step as: clear <see cref="Matches"/>, then
    /// play <see cref="Falls"/> and <see cref="Spawns"/> together.
    /// </summary>
    public sealed class CascadeStep
    {
        /// <summary>Creates a step. The collections are copied, so the step is immutable.</summary>
        /// <exception cref="ArgumentNullException">Any argument is null.</exception>
        public CascadeStep(
            IEnumerable<Match> matches, IEnumerable<GemFall> falls, IEnumerable<GemSpawn> spawns, Board boardAfter)
        {
            Matches = Freeze(matches, nameof(matches));
            Falls = Freeze(falls, nameof(falls));
            Spawns = Freeze(spawns, nameof(spawns));
            BoardAfter = boardAfter ?? throw new ArgumentNullException(nameof(boardAfter));
        }

        /// <summary>The matches cleared at the start of this step, in <see cref="MatchFinder"/> order.</summary>
        public IReadOnlyList<Match> Matches { get; }

        /// <summary>Gems that moved down, ordered by destination (row-major). Gems that did not move are omitted.</summary>
        public IReadOnlyList<GemFall> Falls { get; }

        /// <summary>New gems, ordered by position (row-major), which is also the order their colors were drawn.</summary>
        public IReadOnlyList<GemSpawn> Spawns { get; }

        /// <summary>The board after this step's falls and spawns.</summary>
        public Board BoardAfter { get; }

        private static ReadOnlyCollection<T> Freeze<T>(IEnumerable<T> items, string parameterName) =>
            items is null ? throw new ArgumentNullException(parameterName) : Array.AsReadOnly(items.ToArray());
    }
}