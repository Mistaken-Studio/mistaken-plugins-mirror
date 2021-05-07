#pragma warning disable

using Exiled.API.Features;
using Gamer.Utilities;
using HarmonyLib;
using UnityEngine;

namespace Gamer.Mistaken.Systems.Patches
{
    [HarmonyPatch(typeof(PlayerMovementSync), "ForcePosition", typeof(Vector3), typeof(string), typeof(bool), typeof(bool), typeof(bool))]
    static class ACSmolPatch
    {
        public static bool Prefix(PlayerMovementSync __instance, Vector3 pos, string anticheatCode, bool reset = false, bool grantSafeTime = true, bool resetPrevSafePositions = true)
        {
            switch(anticheatCode)
            {
                case "S.7":
                    {
                        Log.Debug($"[ACSmolPatch] Code: {anticheatCode}");
                        var scale = Player.Get(__instance.gameObject)?.Scale;
                        if (scale != Vector3.one)
                        {
                            Log.Debug($"[ACSmolPatch] Denied AntyCheat");
                            Log.Debug($"[ACSmolPatch] Scale: {scale}");
                            __instance._lastSafePosition = __instance.RealModelPosition;
                            return false;
                        }
                        else
                            Log.Debug($"[ACSmolPatch] Allowed AntyCheat");
                        return true;
                    }
                case "R.1":
                case "R.2":
                case "S.1":
                    {
                        Log.Debug($"[ACSmolPatch] Code: {anticheatCode}");
                        var player = Player.Get(__instance.gameObject);
                        
                        if (
                            player.GetSessionVar<float>(Main.SessionVarType.RUN_SPEED, ServerConfigSynchronizer.Singleton.NetworkHumanSprintSpeedMultiplier) 
                            != ServerConfigSynchronizer.Singleton.NetworkHumanSprintSpeedMultiplier || 
                            player.GetSessionVar<float>(Main.SessionVarType.WALK_SPEED, ServerConfigSynchronizer.Singleton.NetworkHumanWalkSpeedMultiplier) 
                            != ServerConfigSynchronizer.Singleton.NetworkHumanWalkSpeedMultiplier)
                        {
                            Log.Debug($"[ACSmolPatch] Denied AntyCheat");
                            Log.Debug($"[ACSmolPatch] Run: {player.GetSessionVar<float>(Main.SessionVarType.WALK_SPEED, ServerConfigSynchronizer.Singleton.NetworkHumanWalkSpeedMultiplier)}");
                            Log.Debug($"[ACSmolPatch] Walk: {player.GetSessionVar<float>(Main.SessionVarType.RUN_SPEED, ServerConfigSynchronizer.Singleton.NetworkHumanSprintSpeedMultiplier)}");
                            __instance._lastSafePosition = __instance.RealModelPosition;
                            return false;
                        }
                        else
                            Log.Debug($"[ACSmolPatch] Allowed AntyCheat");
                        return true;
                    }
                default:
                    return true;
            }
        }
    }
}
