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
using MEC;
using Gamer.Utilities;

namespace Gamer.Mistaken.Systems.Patches
{
	[HarmonyPatch(typeof(Radio), "UseBattery")]
	static class RadioPatch
	{
		public static bool Prefix(Radio __instance)
		{
			if (__instance.curPreset == 1)
				return false;
			return true;
		}
	}
}
