using System;
using System.Collections.Generic;
using System.Linq;

using Match3.Core.Grid;

namespace Match3.Core.Combat
{
    /// <summary>An immutable amount of action points (AP) for each gem color.</summary>
    /// <remarks>Operations return a new instance, like <see cref="Board"/>, so a pool can be shared and compared safely.</remarks>
    public sealed class ActionPoints : IEquatable<ActionPoints>
    {
        private readonly int[] _amounts;

        private ActionPoints(int[] amounts) => _amounts = amounts;

        /// <summary>A pool with no AP of any color.</summary>
        public static ActionPoints Empty { get; } = new ActionPoints(new int[GemColors.All.Count]);

        /// <summary>The total AP across all colors.</summary>
        public int Total => _amounts.Sum();

        /// <summary>Returns the AP held for <paramref name="color"/>.</summary>
        public int AmountOf(GemColor color) => _amounts[IndexOf(color)];

        /// <summary>Returns true when the pool holds at least <paramref name="cost"/> AP of <paramref name="color"/>.</summary>
        public bool CanAfford(GemColor color, int cost) => AmountOf(color) >= cost;

        /// <summary>Returns a pool with <paramref name="amount"/> more AP of <paramref name="color"/>.</summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="amount"/> is negative.</exception>
        public ActionPoints Add(GemColor color, int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Cannot add a negative amount.");
            }

            return With(color, AmountOf(color) + amount);
        }

        /// <summary>Returns a pool holding the sum of this pool and <paramref name="other"/>.</summary>
        /// <exception cref="ArgumentNullException"><paramref name="other"/> is null.</exception>
        public ActionPoints Add(ActionPoints other)
        {
            if (other is null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            return new ActionPoints(_amounts.Zip(other._amounts, (mine, theirs) => mine + theirs).ToArray());
        }

        /// <summary>Returns a pool with <paramref name="amount"/> less AP of <paramref name="color"/>.</summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="amount"/> is negative.</exception>
        /// <exception cref="InvalidOperationException">The pool holds less than <paramref name="amount"/> of that color.</exception>
        public ActionPoints Spend(GemColor color, int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Cannot spend a negative amount.");
            }

            int available = AmountOf(color);
            if (available < amount)
            {
                throw new InvalidOperationException(
                    FormattableString.Invariant($"Cannot spend {amount} {color} AP; only {available} available."));
            }

            return With(color, available - amount);
        }

        /// <inheritdoc />
        public bool Equals(ActionPoints? other) => other is not null && _amounts.SequenceEqual(other._amounts);

        /// <inheritdoc />
        public override bool Equals(object? obj) => Equals(obj as ActionPoints);

        /// <inheritdoc />
        public override int GetHashCode()
        {
            var hash = default(HashCode);
            foreach (int amount in _amounts)
            {
                hash.Add(amount);
            }

            return hash.ToHashCode();
        }

        /// <summary>Lists the colors with AP, for example "Red 3, Black 5", or "none".</summary>
        public override string ToString()
        {
            IEnumerable<string> held = GemColors.All
                .Where(color => AmountOf(color) > 0)
                .Select(color => FormattableString.Invariant($"{color} {AmountOf(color)}"));
            string text = string.Join(", ", held);
            return text.Length == 0 ? "none" : text;
        }

        private static int IndexOf(GemColor color)
        {
            for (int index = 0; index < GemColors.All.Count; index++)
            {
                if (GemColors.All[index] == color)
                {
                    return index;
                }
            }

            throw new ArgumentOutOfRangeException(nameof(color), color, "Undefined gem color.");
        }

        private ActionPoints With(GemColor color, int amount)
        {
            var amounts = (int[])_amounts.Clone();
            amounts[IndexOf(color)] = amount;
            return new ActionPoints(amounts);
        }
    }
}