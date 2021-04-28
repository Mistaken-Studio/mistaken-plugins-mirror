using Exiled.API.Features;

namespace Gamer.Mistaken.Systems.Patches.Vars
{
    internal static class EnableVarPatchs
    {
        public static void Patch()
        {
            PluginHandler.Harmony.Patch(typeof(Player).GetProperty(nameof(Player.IsGodModeEnabled)).GetSetMethod(), null, new HarmonyLib.HarmonyMethod(typeof(GodModePatch).GetMethod(nameof(GodModePatch.Postfix))));
            PluginHandler.Harmony.Patch(typeof(Player).GetProperty(nameof(Player.IsBypassModeEnabled)).GetSetMethod(), null, new HarmonyLib.HarmonyMethod(typeof(BypassPatch).GetMethod(nameof(BypassPatch.Postfix))));
            PluginHandler.Harmony.Patch(typeof(ServerRoles).GetMethod(nameof(ServerRoles.SetOverwatchStatus), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance), null, new HarmonyLib.HarmonyMethod(typeof(OverwatchPatch).GetMethod(nameof(OverwatchPatch.Postfix))));
            PluginHandler.Harmony.Patch(typeof(NicknameSync).GetProperty(nameof(NicknameSync.DisplayName)).GetSetMethod(), null, new HarmonyLib.HarmonyMethod(typeof(NicknamePatch).GetMethod(nameof(NicknamePatch.Postfix))));
            PluginHandler.Harmony.Patch(typeof(ServerRoles).GetMethod(nameof(ServerRoles.CallCmdSetNoclipStatus)), null, new HarmonyLib.HarmonyMethod(typeof(NoClipPatch).GetMethod(nameof(NoClipPatch.Postfix))));
            //PluginHandler.Harmony.Patch(typeof(Player).GetProperty(nameof(Player.IsGodModeEnabled)).GetSetMethod(), null, new HarmonyLib.HarmonyMethod(typeof(GodModePatch).GetMethod(nameof(GodModePatch.Postfix))));
            //PluginHandler.Harmony.Patch(typeof(Player).GetProperty(nameof(Player.IsGodModeEnabled)).GetSetMethod(), null, new HarmonyLib.HarmonyMethod(typeof(GodModePatch).GetMethod(nameof(GodModePatch.Postfix))));
            //PluginHandler.Harmony.Patch(typeof(Player).GetProperty(nameof(Player.IsGodModeEnabled)).GetSetMethod(), null, new HarmonyLib.HarmonyMethod(typeof(GodModePatch).GetMethod(nameof(GodModePatch.Postfix))));
            //PluginHandler.Harmony.Patch(typeof(Player).GetProperty(nameof(Player.IsGodModeEnabled)).GetSetMethod(), null, new HarmonyLib.HarmonyMethod(typeof(GodModePatch).GetMethod(nameof(GodModePatch.Postfix))));
            //PluginHandler.Harmony.Patch(typeof(Player).GetProperty(nameof(Player.IsGodModeEnabled)).GetSetMethod(), null, new HarmonyLib.HarmonyMethod(typeof(GodModePatch).GetMethod(nameof(GodModePatch.Postfix))));
            //PluginHandler.Harmony.Patch(typeof(Player).GetProperty(nameof(Player.IsGodModeEnabled)).GetSetMethod(), null, new HarmonyLib.HarmonyMethod(typeof(GodModePatch).GetMethod(nameof(GodModePatch.Postfix))));
            //PluginHandler.Harmony.Patch(typeof(Player).GetProperty(nameof(Player.IsGodModeEnabled)).GetSetMethod(), null, new HarmonyLib.HarmonyMethod(typeof(GodModePatch).GetMethod(nameof(GodModePatch.Postfix))));
        }
    }
}
