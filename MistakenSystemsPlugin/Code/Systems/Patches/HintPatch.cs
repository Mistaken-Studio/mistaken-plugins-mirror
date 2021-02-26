﻿using System;
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

	[HarmonyPatch(typeof(Exiled.API.Features.Player), "ShowHint")] //Check
	static class HintPatch
	{
		private static readonly Dictionary<string, int> Tries = new Dictionary<string, int>();
		public static bool Prefix(Player __instance, ref string message, float duration = 3f)
		{
			if (!RealPlayers.List.Contains(__instance))
			{
				string msg = message;
				Timing.CallDelayed(0.5f, () => {
					__instance.ShowHint(msg, duration);
				});
				if (!Tries.ContainsKey(message))
					Tries.Add(message, 10);
				else
				{
					Tries[message]--;
					if (Tries[message] == 0)
						Tries.Remove(message);
				}
				return false;
			}
			Tries.Remove(message);
			message = message.Replace("\n", "<br>").Replace("\\n", "<br>");
			return true;
		}
	}
}
