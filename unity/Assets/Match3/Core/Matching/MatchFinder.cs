using System;
using System.Collections.Generic;

using Match3.Core.Grid;

namespace Match3.Core.Matching
{
    /// <summary>Finds groups of gems that should clear.</summary>
    public static class MatchFinder
    {
        /// <summary>
        /// Finds every match on the board. Same-colored runs that share a cell merge into one match;
        /// runs that only sit side by side stay separate.
        /// </summary>
        /// <param name="board">The board to scan.</param>
        /// <returns>Matches ordered by their first position (row-major); empty when there are none.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="board"/> is null.</exception>
        public static IReadOnlyList<Match> FindMatches(Board board)
        {
            throw new NotImplementedException();
        }
    }
}