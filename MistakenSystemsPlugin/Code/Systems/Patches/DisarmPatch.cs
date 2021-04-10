using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RemoteAdmin;
using HarmonyLib;
using UnityEngine;
using Exiled.API.Features;
using Gamer.Mistaken.Utilities.APILib;
using Mirror;
using Exiled.Events.Handlers;
using Exiled.Events.EventArgs;

namespace Gamer.Mistaken.Systems.Patches
{
	[HarmonyPatch(typeof(Handcuffs), "CallCmdCuffTarget")]
	static class DisarmPatch
	{
		public static bool Prefix(Handcuffs __instance, GameObject target)
		{
			if (target == null || Vector3.Distance(target.transform.position, __instance.transform.position) > __instance.raycastDistance * 1.1f)
				return true;
			Handcuffs handcuffs = ReferenceHub.GetHub(target).handcuffs;
			if (handcuffs == null || __instance.MyReferenceHub.inventory.curItem != ItemType.Disarmer || __instance.MyReferenceHub.characterClassManager.CurClass < RoleType.Scp173)
				return true;
			if (!(handcuffs.CufferId < 0 && !__instance.ForceCuff && handcuffs.MyReferenceHub.inventory.curItem == ItemType.None))
				return true;
			if (__instance.MyReferenceHub.characterClassManager.CurRole.team == Team.CDP)
			{
				if (handcuffs.MyReferenceHub.characterClassManager.CurRole.team == Team.CDP)
				{
					__instance.ClearTarget();
					handcuffs.NetworkCufferId = __instance.MyReferenceHub.queryProcessor.PlayerId;
					return false;
				}
			}
			return true;
		}
	}
}
