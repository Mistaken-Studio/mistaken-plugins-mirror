#pragma warning disable

using Exiled.API.Features;
using Gamer.Mistaken.Base.Staff;
using Grenades;
using HarmonyLib;
using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gamer.Mistaken.Systems.Patches
{
    [HarmonyPatch(typeof(Grenades.Grenade), nameof(Grenades.Grenade.FullInitData))]
    static class GrenadeFixPatch
    {
        public static bool Prefix(ref GrenadeManager player, Vector3 position, Quaternion rotation, Vector3 linearVelocity, Vector3 angularVelocity, global::Team originalTeam)
        {
            if (player == null)
                player = ReferenceHub.HostHub.GetComponent<GrenadeManager>();
            return true;
        }
    }

    [HarmonyPatch(typeof(Grenades.Grenade), nameof(Grenades.Grenade.InitData), typeof(FragGrenade), typeof(Pickup))]
    static class GrenadeFixPatch2
    {
        public static bool Prefix(Grenade __instance, FragGrenade original, global::Pickup item)
        {
            if (!NetworkServer.active)
                return true;
            Transform transform = item.transform;
            Vector3 position = transform.position;
            Vector3 velocity = item.Rb.velocity;
            Vector3 linear;
            if (velocity.normalized == Vector3.zero)
                linear = new Vector3(UnityEngine.Random.Range(-5f, 5f), UnityEngine.Random.Range(0.1f, 0.6f), UnityEngine.Random.Range(-5f, 5f));
            else
                linear = velocity + original.chainSpeed * (position + Quaternion.LookRotation(velocity.normalized, Vector3.up) * original.chainMovement);
            __instance.FullInitData(original.thrower, position, transform.rotation, linear, item.Rb.angularVelocity, original.TeamWhenThrown);
            __instance.currentChainLength = original.currentChainLength + 1;
            return false;
        }
    }

    [HarmonyPatch(typeof(Grenades.FragGrenade), nameof(Grenades.FragGrenade.ServersideExplosion))]
    static class GrenadeFixPatch3
    {
        internal static readonly List<GameObject> disabledGoList = new List<GameObject>();
        public static bool Prefix(FragGrenade __instance)
        {
            foreach (var item in GameObject.FindObjectsOfType(typeof(GameObject)))
            {
                var go = item as GameObject;
                if (item.name == "LookingTarget" || item.name.Contains("InRange"))
                {
                    if (!go.activeSelf)
                        continue;
                    disabledGoList.Add(go);
                    go.SetActive(false);
                }
            }
            try
            {
                Vector3 position = __instance.transform.position;
                foreach (var keyValuePair in ReferenceHub.GetAllHubs())
                {
                    var player = Player.Get(keyValuePair.Value);
                    try
                    {
                        if (!player.IsActiveDev())
                            continue;
                    }
                    catch { continue; }
                    if (ServerConsole.FriendlyFire || !(keyValuePair.Key != __instance.thrower.gameObject) || (keyValuePair.Value.weaponManager.GetShootPermission(__instance.throwerTeam, false) && keyValuePair.Value.weaponManager.GetShootPermission(__instance.TeamWhenThrown, false)))
                    {
                        PlayerStats playerStats = keyValuePair.Value.playerStats;
                        if (playerStats != null && playerStats.ccm.InWorld)
                        {
                            float num2 = __instance.damageOverDistance.Evaluate(Vector3.Distance(position, playerStats.transform.position)) * (playerStats.ccm.IsHuman() ? GameCore.ConfigFile.ServerConfig.GetFloat("human_grenade_multiplier", 0.7f) : GameCore.ConfigFile.ServerConfig.GetFloat("scp_grenade_multiplier", 1f));
                            if (num2 > __instance.absoluteDamageFalloff)
                            {
                                foreach (Transform transform in playerStats.grenadePoints)
                                {
                                    if (Physics.Linecast(position, transform.position, out var _hit, __instance.hurtLayerMask))
                                    {
                                        if (!(_hit.collider.name == "PlyCenter"))
                                        {
                                            player.SendConsoleMessage($"[GRENADE] Blocked by {_hit.collider.name} ({_hit.collider.gameObject.layer})", "red");
                                            RoundLoggerSystem.RoundLogger.Log("GRENADE", "BLOCK", $"Grenade was blocked by {_hit.collider.name} ({_hit.collider.gameObject.layer})");
                                        }
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Log.Error(ex.Message);
                Log.Error(ex.StackTrace);
            }
            return true;
        }

        public static void Postfix(FragGrenade __instance)
        {
            try
            {
                foreach (var item in disabledGoList)
                {
                    try
                    {
                        item.SetActive(true);
                    }
                    catch (System.Exception ex)
                    {
                        Log.Error("Got them inside");
                        Log.Error(ex.Message);
                        Log.Error(ex.StackTrace);
                    }
                }
            }
            catch(System.Exception ex)
            {
                Log.Error("Got them outside");
                Log.Error(ex.Message);
                Log.Error(ex.StackTrace);
            }
        }
    }
}
