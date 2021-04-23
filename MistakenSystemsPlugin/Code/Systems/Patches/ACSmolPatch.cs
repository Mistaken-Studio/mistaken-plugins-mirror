#pragma warning disable

using HarmonyLib;
using UnityEngine;
using Exiled.API.Features;

namespace Gamer.Mistaken.Systems.Patches
{
	[HarmonyPatch(typeof(PlayerMovementSync), "ForcePosition", typeof(Vector3), typeof(string), typeof(bool), typeof(bool), typeof(bool))]
	static class ACSmolPatch
	{
		public static bool Prefix(PlayerMovementSync __instance, Vector3 pos, string anticheatCode, bool reset = false, bool grantSafeTime = true, bool resetPrevSafePositions = true)
		{
			if (anticheatCode != "S.7")
				return true;
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
	}
}
