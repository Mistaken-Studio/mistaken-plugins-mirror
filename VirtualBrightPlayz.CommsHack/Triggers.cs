using Dissonance;
using HarmonyLib;
using Mirror;
using RemoteAdmin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommsHack
{
	[HarmonyPatch(typeof(CustomBroadcastTrigger), "IsUserActivated")]
	public class TriggerPatch
	{
		// Token: 0x06000036 RID: 54 RVA: 0x00002E74 File Offset: 0x00001074
		public static bool Prefix(CustomBroadcastTrigger __instance, ref bool __result)
		{
			bool flag = __instance.GetComponent<QueryProcessor>().PlayerId >= 9000 || __instance.GetComponent<NetworkIdentity>().connectionToClient == null;
			bool result;
			if (flag)
			{
				__result = true;
				result = false;
			}
			else
			{
				result = true;
			}
			return result;
		}
	}

	[HarmonyPatch(typeof(VoiceBroadcastTrigger), "get_CanTrigger")]
	public class TriggerPatch2
	{
		// Token: 0x06000038 RID: 56 RVA: 0x00002EC0 File Offset: 0x000010C0
		public static bool Prefix(VoiceBroadcastTrigger __instance, ref bool __result)
		{
			bool flag = __instance.GetComponent<QueryProcessor>().PlayerId >= 9000 || __instance.GetComponent<NetworkIdentity>().connectionToClient == null;
			bool result;
			if (flag)
			{
				__result = true;
				result = false;
			}
			else
			{
				result = true;
			}
			return result;
		}
	}

	[HarmonyPatch(typeof(BaseCommsTrigger), "get_TokenActivationState")]
	public class TriggerPatch3
	{
		// Token: 0x0600003A RID: 58 RVA: 0x00002F0C File Offset: 0x0000110C
		public static bool Prefix(BaseCommsTrigger __instance, ref bool __result)
		{
			bool flag = __instance.GetComponent<QueryProcessor>().PlayerId >= 9000 || __instance.GetComponent<NetworkIdentity>().connectionToClient == null;
			bool result;
			if (flag)
			{
				__result = true;
				result = false;
			}
			else
			{
				result = true;
			}
			return result;
		}
	}

	[HarmonyPatch(typeof(VoiceBroadcastTrigger), "ShouldActivate")]
	public class TriggerPatch4
	{
		// Token: 0x0600003C RID: 60 RVA: 0x00002F58 File Offset: 0x00001158
		public static bool Prefix(VoiceBroadcastTrigger __instance, ref bool __result, bool intent)
		{
			bool flag = __instance.GetComponent<QueryProcessor>().PlayerId >= 9000 || __instance.GetComponent<NetworkIdentity>().connectionToClient == null;
			bool result;
			if (flag)
			{
				__result = true;
				result = false;
			}
			else
			{
				result = true;
			}
			return result;
		}
	}

	[HarmonyPatch(typeof(Mirror.NetworkServer), "SendSpawnMessage")]
	static class SpawnPatch
	{
		public static readonly Dictionary<NetworkIdentity, HashSet<NetworkConnection>> WhitelistedSpawns = new Dictionary<NetworkIdentity, HashSet<NetworkConnection>>();
		public static bool Prefix(NetworkIdentity identity, NetworkConnection conn)
		{
			if (!WhitelistedSpawns.TryGetValue(identity, out HashSet<NetworkConnection> list))
				return true;
			if (!list.Contains(conn))
				return false;
			return true;
		}
	}

	[HarmonyPatch(typeof(VersionCheck), "Start")]
	static class VersionCheckPatch
	{
		public static bool Prefix(VersionCheck __instance)
		{
			if (__instance.connectionToClient == null) return false;
			return true;
		}
	}

	[HarmonyPatch(typeof(RemoteAdmin.QueryProcessor), "Start")]
	static class QueryProcessorPatch
	{
		public static bool Prefix(QueryProcessor __instance)
		{
			if (__instance.PlayerId > 9000) return false;
			return true;
		}
	}
}
