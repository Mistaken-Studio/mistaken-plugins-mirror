using HarmonyLib;

namespace Gamer.Mistaken.Systems.Patches
{
    [HarmonyPatch(typeof(Radio), "UseBattery")]
    internal static class RadioPatch
    {
        public static bool Prefix(Radio __instance)
        {
            if (__instance.curPreset == 1)
                return false;
            return true;
        }
    }
}
