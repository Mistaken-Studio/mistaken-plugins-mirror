using HarmonyLib;

namespace Gamer.CITester
{
    [HarmonyPatch(typeof(Hints.HintDisplay), "Show")]
    internal static class HintPatch
    {
        private static bool Prefix(Hints.HintDisplay __instance)
        {
            return false;
        }
    }
}
