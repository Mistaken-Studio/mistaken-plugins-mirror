namespace Gamer.Utilities
{
    /// <summary>
    /// Used for RoundId
    /// </summary>
    public static class RoundPlus
    {
        /// <summary>
        /// Round Id
        /// </summary>
        public static int RoundId { get; private set; } = 0;
        /// <summary>
        /// Increments Round Id
        /// </summary>
        public static void IncRoundId() => RoundId++;
    }
}
