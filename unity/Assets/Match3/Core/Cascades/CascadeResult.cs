using System;
using System.Collections.Generic;
using System.Linq;

using Match3.Core.Grid;

namespace Match3.Core.Cascades
{
    /// <summary>The outcome of resolving a board: every cascade step, and the settled board.</summary>
    public sealed class CascadeResult
    {
        /// <summary>Creates a result. The step list is copied, so the result is immutable.</summary>
        /// <exception cref="ArgumentNullException">Any argument is null.</exception>
        public CascadeResult(IEnumerable<CascadeStep> steps, Board finalBoard)
        {
            if (steps is null)
            {
                throw new ArgumentNullException(nameof(steps));
            }

            Steps = Array.AsReadOnly(steps.ToArray());
            FinalBoard = finalBoard ?? throw new ArgumentNullException(nameof(finalBoard));
        }

        /// <summary>The steps in the order they happened; empty when the board had no matches.</summary>
        public IReadOnlyList<CascadeStep> Steps { get; }

        /// <summary>The settled board, which contains no matches.</summary>
        public Board FinalBoard { get; }
    }
}