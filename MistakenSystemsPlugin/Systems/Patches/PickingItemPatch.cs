using Exiled.Events.EventArgs;
using HarmonyLib;
using Searching;

namespace Gamer.Mistaken.Systems.Patches
{
    [HarmonyPatch(typeof(ItemSearchCompletor), "ValidateAny")]
    internal static class PickingItemPatch //Check
    {
        public static bool Prefix(ItemSearchCompletor __instance)
        {
            if (__instance?.TargetPickup == null)
                return true;
            if (__instance?.TargetItem == null)
                return true;
            if (__instance?.Hub == null)
                return true;
            if (__instance?.TargetPickup.ItemId == null)
                return true;
            PickItemRequestEventArgs data = new PickItemRequestEventArgs(Exiled.API.Features.Player.Get(__instance.Hub), __instance.TargetPickup);
            Exiled.Events.Handlers.CustomEvents.InvokeOnRequestPickItem(ref data);
            if (!data.IsAllowed)
            {
                data.Pickup.InUse = false;
                return false;
            }
            return true;
        }
    }
}
