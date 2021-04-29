#pragma warning disable

using Exiled.API.Features;
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
        internal static readonly List<GameObject> disabledLookingList = new List<GameObject>();
        public static bool Prefix(FragGrenade __instance)
        {
            foreach (var item in GameObject.FindObjectsOfType(typeof(GameObject)))
            {
                var go = item as GameObject;
                if (item.name == "LookingTarget")
                {
                    if (!go.activeSelf)
                        continue;
                    disabledLookingList.Add(go);
                    go.SetActive(false);
                }
            }
            return true;
        }

        public static void Postfix(FragGrenade __instance)
        {
            foreach (var item in disabledLookingList)
                item.SetActive(true);
        }
    }
}
