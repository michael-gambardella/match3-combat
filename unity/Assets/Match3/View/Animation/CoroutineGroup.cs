using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Match3.View.Animation
{
    /// <summary>Runs animations in parallel.</summary>
    internal static class CoroutineGroup
    {
        /// <summary>Starts every routine at once on <paramref name="host"/>, then waits until all have finished.</summary>
        public static IEnumerator All(MonoBehaviour host, IEnumerable<IEnumerator> routines)
        {
            // ToList starts every routine before the first wait; waiting on each handle in turn
            // then finishes when the slowest one does.
            List<Coroutine> running = routines.Select(routine => host.StartCoroutine(routine)).ToList();
            foreach (Coroutine coroutine in running)
            {
                yield return coroutine;
            }
        }
    }
}