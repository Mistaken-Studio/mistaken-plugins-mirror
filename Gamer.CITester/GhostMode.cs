using HarmonyLib;

namespace Gamer.CITester
{
    [HarmonyPatch(typeof(PlayerPositionManager), "TransmitData")]
    internal static class GhostMode
    {
        private static bool Prefix(PlayerPositionManager __instance)
        {
            return false;
        }
    }
}
