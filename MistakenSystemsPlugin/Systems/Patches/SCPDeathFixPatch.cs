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

namespace Gamer.Mistaken.Systems.Patches
{
    [HarmonyPatch(typeof(NineTailedFoxNamingRule), "GetCassieUnitName")]
    static class SCPDeathFixPatch
	{
		public static bool Prefix(ref string regular)
		{
			Log.Debug(regular);
			if (regular.StartsWith("<color="))
				regular = regular.Substring(12, regular.Length - 12 - 8);
			Log.Debug(regular);
			return true;
		}
	}
}
