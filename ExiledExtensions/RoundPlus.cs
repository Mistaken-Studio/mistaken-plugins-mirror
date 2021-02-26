namespace Gamer.Utilities
{
    public static class RoundPlus
    {
        public static int RoundId { get; private set; } = 0;

        public static void IncRoundId() => RoundId++;
    }
}
