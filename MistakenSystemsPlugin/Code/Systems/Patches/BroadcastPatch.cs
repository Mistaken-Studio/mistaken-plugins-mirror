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
	[HarmonyPatch(typeof(RemoteAdmin.QueryProcessor), "TargetReply")] //Check
	static class BroadcastTargetReplyPatch
	{
		public static readonly Dictionary<string, List<string>> Targets = new Dictionary<string, List<string>>();
		public static bool Prefix(NetworkConnection conn, string content)
		{
			if (!content.StartsWith("@")) return true;
			content = content.Substring(1);
			if (!Targets.ContainsKey(content))
            {
				Targets.Add(content, NorthwoodLib.Pools.ListPool<string>.Shared.Rent());
				MEC.Timing.CallDelayed(2, () => {
					var tmp = content.Split('~');
					var admin = "SYSTEM"; //tmp.Length == 1 || !tmp.Contains(" ~") || !tmp.Contains("'~'") ? "SYSTEM" : tmp.Last();
					if (tmp.Length > 1 && !content.Contains("'~'"))
						admin = tmp.Last();
					var contentWithoutAdmin = tmp.Length == 1 ? content : content.Substring(0, content.Length - (admin.Length + 1));
					var ev = new BroadcastEventArgs(Broadcast.BroadcastFlags.AdminChat, contentWithoutAdmin, admin, Targets[content].ToArray());
					CustomEvents.InvokeOnBroadcast(ref ev);

					NorthwoodLib.Pools.ListPool<string>.Shared.Return(Targets[content]);
					Targets.Remove(content);
				});
			}
			var userId = ReferenceHub.GetHub(conn?.identity?.gameObject)?.characterClassManager?.UserId;
			if(!string.IsNullOrWhiteSpace(userId))
				Targets[content].Add(userId);
			return true;
		}
	}

	[HarmonyPatch(typeof(Broadcast), "TargetAddElement")] //Check
	static class BroadcastTargetAddElementPatch
	{
		public static readonly Dictionary<string, List<string>> Targets = new Dictionary<string, List<string>>();
		public static bool Prefix(NetworkConnection conn, string data, ushort time, Broadcast.BroadcastFlags flags)
		{
			if (!Targets.ContainsKey(data))
			{
				Targets.Add(data, NorthwoodLib.Pools.ListPool<string>.Shared.Rent());
				MEC.Timing.CallDelayed(2, () => {
					var tmp = data.Split('~');
					var admin = "SYSTEM"; //tmp.Length == 1 || !tmp.Contains(" ~") || !tmp.Contains("'~'") ? "SYSTEM" : tmp.Last();
					if (tmp.Length > 1 && !data.Contains("'~'"))
						admin = tmp.Last();
					var contentWithoutAdmin = tmp.Length == 1 ? data : data.Substring(0, data.Length - (admin.Length + 1));
					var ev = new BroadcastEventArgs(flags, contentWithoutAdmin, admin, Targets[data].ToArray());
					CustomEvents.InvokeOnBroadcast(ref ev);

					NorthwoodLib.Pools.ListPool<string>.Shared.Return(Targets[data]);
					Targets.Remove(data);
				});
			}
			if (conn?.identity?.gameObject == null)
				return true;
			var user = ReferenceHub.GetHub(conn.identity.gameObject);
			string uid = user?.characterClassManager?.UserId;
			if (uid == null)
				return true;
			Targets[data].Add(uid);
			return true;
		}
	}

	[HarmonyPatch(typeof(Broadcast), "RpcAddElement")] //Check
	static class BroadcastRpcAddElementPatch
	{
		public static bool Prefix(string data, ushort time, Broadcast.BroadcastFlags flags)
		{
			var tmp = data.Split('~');
			var admin = "SYSTEM"; //tmp.Length == 1 || !tmp.Contains(" ~") || !tmp.Contains("'~'") ? "SYSTEM" : tmp.Last();
			if (tmp.Length > 1 && !data.Contains("'~'"))
				admin = tmp.Last();
			var contentWithoutAdmin = tmp.Length == 1 ? data : data.Substring(0, data.Length - (admin.Length + 1));
			var ev = new BroadcastEventArgs(flags, contentWithoutAdmin, admin, Exiled.API.Features.Player.UserIdsCache.Keys.ToArray());
			CustomEvents.InvokeOnBroadcast(ref ev);
			return true;
		}
	}
}
