using HarmonyLib;
using System;

namespace Gamer.Mistaken.Systems.Patches
{
    [HarmonyPatch(typeof(Inventory.SyncItemInfo), "Equals", typeof(Inventory.SyncItemInfo))]
    static class ComparePatchFix
    {
        public static bool Prefix(Inventory.SyncItemInfo __instance, Inventory.SyncItemInfo other, ref bool __result)
        {
            __result = __instance.id == other.id && Math.Abs(__instance.durability - other.durability) < 0.000001f && __instance.uniq == other.uniq && __instance.modSight == other.modSight && __instance.modBarrel == other.modBarrel && __instance.modOther == other.modOther;
            return false;
        }
    }
}
