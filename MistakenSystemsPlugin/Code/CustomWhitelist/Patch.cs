using System;
using HarmonyLib;

namespace Gamer.Mistaken.CustomWhitelist
{
    [HarmonyPatch(typeof(ReservedSlot), "HasReservedSlot", new Type[] { typeof(string) })]
    public static class ReservedSlotPatch
    {
        static bool Prefix(ref bool __result, string userId)
        {
            __result = true;
            return false;
        }
    }
}
