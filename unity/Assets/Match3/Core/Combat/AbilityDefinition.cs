using System;

using Match3.Core.Grid;

namespace Match3.Core.Combat
{
    /// <summary>
    /// A designer-defined ability: spend AP of one color to deal damage. Pure data, so abilities can be
    /// authored in the editor and balanced without code changes.
    /// </summary>
    /// <remarks>Abilities are identified by instance: a game's loadout holds the exact objects it was given.</remarks>
    public sealed class AbilityDefinition
    {
        /// <summary>Creates an ability.</summary>
        /// <param name="name">Display name; must not be blank.</param>
        /// <param name="costColor">The AP color the ability spends.</param>
        /// <param name="cost">AP spent per use; must be positive.</param>
        /// <param name="damage">Damage dealt to the enemy; must not be negative.</param>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="name"/> is empty or whitespace.</exception>
        /// <exception cref="ArgumentOutOfRangeException">A numeric value or the color is out of range.</exception>
        public AbilityDefinition(string name, GemColor costColor, int cost, int damage)
        {
            if (name is null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("An ability needs a name.", nameof(name));
            }

            if (!Enum.IsDefined(typeof(GemColor), costColor))
            {
                throw new ArgumentOutOfRangeException(nameof(costColor), costColor, "Undefined gem color.");
            }

            if (cost <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(cost), cost, "Cost must be positive.");
            }

            if (damage < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(damage), damage, "Damage cannot be negative.");
            }

            Name = name;
            CostColor = costColor;
            Cost = cost;
            Damage = damage;
        }

        /// <summary>Display name.</summary>
        public string Name { get; }

        /// <summary>The AP color the ability spends.</summary>
        public GemColor CostColor { get; }

        /// <summary>AP spent per use.</summary>
        public int Cost { get; }

        /// <summary>Damage dealt to the enemy.</summary>
        public int Damage { get; }

        /// <inheritdoc />
        public override string ToString() =>
            FormattableString.Invariant($"{Name} ({Cost} {CostColor} AP, {Damage} damage)");
    }
}