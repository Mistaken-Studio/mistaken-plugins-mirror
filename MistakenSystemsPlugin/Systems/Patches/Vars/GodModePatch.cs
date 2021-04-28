using Exiled.API.Features;
using Gamer.Utilities;

namespace Gamer.Mistaken.Systems.Patches.Vars
{
    internal static class GodModePatch
    {
        public static void Postfix(Player __instance, bool value)
        {
            AnnonymousEvents.Call("GOD_MODE", (__instance, value));
        }
    }

    internal static class BypassPatch
    {
        public static void Postfix(Player __instance, bool value)
        {
            AnnonymousEvents.Call("BYPASS", (__instance, value));
        }
    }

    internal static class OverwatchPatch
    {
        public static void Postfix(ServerRoles __instance, bool status)
        {
            AnnonymousEvents.Call("OVERWATCH", (Player.Get(__instance.gameObject), status));
        }
    }

    internal static class NicknamePatch
    {
        public static void Postfix(NicknameSync __instance, string value)
        {
            AnnonymousEvents.Call("NICKNAME", (Player.Get(__instance.gameObject), value));
        }
    }

    internal static class NoClipPatch
    {
        public static void Postfix(ServerRoles __instance, byte status)
        {
            AnnonymousEvents.Call("NOCLIP", (Player.Get(__instance.gameObject), status));
        }
    }
}
