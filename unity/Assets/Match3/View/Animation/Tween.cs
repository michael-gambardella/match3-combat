using System;
using System.Collections;

using UnityEngine;

namespace Match3.View.Animation
{
    /// <summary>Minimal coroutine tweening, so the project needs no third-party animation package.</summary>
    internal static class Tween
    {
        /// <summary>
        /// Calls <paramref name="apply"/> once per frame with eased progress from 0 to 1 over
        /// <paramref name="duration"/> seconds, and always finishes with exactly 1 so animations land precisely.
        /// </summary>
        public static IEnumerator Run(float duration, Func<float, float> ease, Action<float> apply)
        {
            for (float elapsed = 0f; elapsed < duration; elapsed += Time.deltaTime)
            {
                apply(ease(elapsed / duration));
                yield return null;
            }

            apply(1f);
        }
    }
}