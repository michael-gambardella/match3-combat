namespace Match3.View.Animation
{
    /// <summary>Easing curves mapping linear progress (0 to 1) to eased progress (0 to 1).</summary>
    internal static class Easing
    {
        /// <summary>Starts fast and slows into place: falling gems settling.</summary>
        public static float OutQuad(float t) => 1f - ((1f - t) * (1f - t));

        /// <summary>Starts slow and speeds up: gems shrinking away.</summary>
        public static float InQuad(float t) => t * t;
    }
}