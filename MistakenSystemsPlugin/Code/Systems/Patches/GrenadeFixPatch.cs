using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RemoteAdmin;
using HarmonyLib;
using Assets._Scripts.Dissonance;
using Respawning.NamingRules;
using System.Text.RegularExpressions;
using Exiled.API.Features;
using Grenades;
using UnityEngine;
using Mirror;

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
			if(velocity.normalized == Vector3.zero)
				linear = new Vector3(UnityEngine.Random.Range(-5f, 5f), UnityEngine.Random.Range(0.1f, 0.6f), UnityEngine.Random.Range(-5f, 5f));
			else
				linear = velocity + original.chainSpeed * (position + Quaternion.LookRotation(velocity.normalized, Vector3.up) * original.chainMovement);
			__instance.FullInitData(original.thrower, position, transform.rotation, linear, item.Rb.angularVelocity, original.TeamWhenThrown);
			__instance.currentChainLength = original.currentChainLength + 1;
			return false;
		}
	}
}
