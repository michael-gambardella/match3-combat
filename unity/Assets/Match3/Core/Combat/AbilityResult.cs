namespace Match3.Core.Combat
{
    /// <summary>What happened when an ability was used.</summary>
    public sealed class AbilityResult
    {
        internal AbilityResult(AbilityDefinition ability, int damageDealt, GameStatus status)
        {
            Ability = ability;
            DamageDealt = damageDealt;
            Status = status;
        }

        /// <summary>The ability that was used.</summary>
        public AbilityDefinition Ability { get; }

        /// <summary>Damage dealt to the enemy.</summary>
        public int DamageDealt { get; }

        /// <summary>The encounter's status after the ability.</summary>
        public GameStatus Status { get; }
    }
}