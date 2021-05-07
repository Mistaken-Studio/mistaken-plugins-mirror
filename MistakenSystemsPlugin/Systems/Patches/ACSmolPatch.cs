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
        public const bool DEBUG = false;
        public static bool Prefix(PlayerMovementSync __instance, Vector3 pos, string anticheatCode, bool reset = false, bool grantSafeTime = true, bool resetPrevSafePositions = true)
        {
            switch(anticheatCode)
            {
                case "S.7":
                    {
                        Log.Debug($"[ACSmolPatch] Code: {anticheatCode}", DEBUG);
                        var scale = Player.Get(__instance.gameObject)?.Scale;
                        if (scale != Vector3.one)
                        {
                            Log.Debug($"[ACSmolPatch] Denied AntyCheat", DEBUG);
                            Log.Debug($"[ACSmolPatch] Scale: {scale}", DEBUG);
                            __instance._realModelPosition = __instance.transform.position;
                            __instance._lastSafePosition = __instance.RealModelPosition;
                            return false;
                        }
                        else
                            Log.Debug($"[ACSmolPatch] Allowed AntyCheat", DEBUG);
                        return true;
                    }
                case "R.1":
                case "R.2":
                case "S.1":
                    {
                        Log.Debug($"[ACSmolPatch] Code: {anticheatCode}", DEBUG);
                        var player = Player.Get(__instance.gameObject);
                        if (player == null)
                            return true;
                        if (
                            player.GetSessionVar<float>(Main.SessionVarType.RUN_SPEED, ServerConfigSynchronizer.Singleton.NetworkHumanSprintSpeedMultiplier) 
                            != ServerConfigSynchronizer.Singleton.NetworkHumanSprintSpeedMultiplier || 
                            player.GetSessionVar<float>(Main.SessionVarType.WALK_SPEED, ServerConfigSynchronizer.Singleton.NetworkHumanWalkSpeedMultiplier) 
                            != ServerConfigSynchronizer.Singleton.NetworkHumanWalkSpeedMultiplier)
                        {
                            Log.Debug($"[ACSmolPatch] Denied AntyCheat", DEBUG);
                            Log.Debug($"[ACSmolPatch] Run: {player.GetSessionVar<float>(Main.SessionVarType.WALK_SPEED, ServerConfigSynchronizer.Singleton.NetworkHumanWalkSpeedMultiplier)}", DEBUG);
                            Log.Debug($"[ACSmolPatch] Walk: {player.GetSessionVar<float>(Main.SessionVarType.RUN_SPEED, ServerConfigSynchronizer.Singleton.NetworkHumanSprintSpeedMultiplier)}", DEBUG);
                            __instance._realModelPosition = __instance.transform.position;
                            __instance._lastSafePosition = __instance.RealModelPosition;
                            return false;
                        }
                        else
                            Log.Debug($"[ACSmolPatch] Allowed AntyCheat", DEBUG);
                        return true;
                    }
                default:
                    return true;
            }
        }
    }
}
