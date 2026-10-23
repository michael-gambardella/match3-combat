namespace Match3.Core.Combat
{
    /// <summary>Where an encounter stands.</summary>
    public enum GameStatus
    {
        /// <summary>The player can still move.</summary>
        InProgress,

        /// <summary>The enemy was defeated.</summary>
        Won,

        /// <summary>The turn limit was reached with the enemy still standing.</summary>
        Lost,
    }
}