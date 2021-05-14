using Exiled.API.Features;
using Gamer.API;
using Gamer.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace Gamer.SNAV
{
    [HarmonyLib.HarmonyPatch(typeof(Generator079), nameof(Generator079.EjectTablet))]
    internal static class GeneratorPatch
    {
        internal static readonly Dictionary<Generator079, Inventory.SyncItemInfo> Generators = new Dictionary<Generator079, Inventory.SyncItemInfo>();
        static bool Prefix(Generator079 __instance)
        {
            if (Generators.TryGetValue(__instance, out var snav))
            {
                MapPlus.Spawn(snav, __instance.tabletEjectionPoint.position, __instance.tabletEjectionPoint.rotation, Vector3.one);
                Generators.Remove(__instance);
                __instance.NetworkisTabletConnected = false;
                return false;
            }
            return true;
        }
    }
}
