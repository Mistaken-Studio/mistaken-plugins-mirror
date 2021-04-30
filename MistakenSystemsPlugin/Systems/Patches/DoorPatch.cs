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
using Gamer.Mistaken.Base.Staff;
using Gamer.Utilities;
using Interactables.Interobjects.DoorUtils;

namespace Gamer.Mistaken.Systems.Patches
{
	[HarmonyPatch(typeof(CommandProcessor), "ProcessDoorQuery", typeof(CommandSender), typeof(string), typeof(string))]
	public static class DoorPatch
	{
		public static readonly HashSet<DoorVariant> IgnoredDoor = new HashSet<DoorVariant>();
		public static bool Prefix(CommandSender sender, string command, string door)
		{
			if (!CommandProcessor.CheckPermissions(sender, command.ToUpper(), PlayerPermissions.FacilityManagement, "", true))
				return false;
			if (string.IsNullOrEmpty(door))
			{
				sender.RaReply(command + "#Please select door first.", false, true, "DoorsManagement");
				return false;
			}
			if (!(door == "**" || door == "*" || door == "!*"))
				return true;
			bool flag = false;
			DoorVariant[] array = UnityEngine.Object.FindObjectsOfType<DoorVariant>();
			int i = -1;
			while (i + 1 < array.Length)
			{
				i++;
				DoorVariant doorVariant = array[i];
				if (IgnoredDoor.Contains(doorVariant))
					continue;
				if (door != "**")
				{
					if (doorVariant.TryGetComponent<DoorNametagExtension>(out _))
					{
						if (door == "!*")
							continue;
					}
					else if (door == "*")
						continue;
				}
				switch(command)
                {
					case "OPEN":
						doorVariant.NetworkTargetState = true;
						break;
					case "CLOSE":
						doorVariant.NetworkTargetState = false;
						break;
					case "LOCK":
						doorVariant.ServerChangeLock(DoorLockReason.AdminCommand, true);
						break;
					case "UNLOCK":
						doorVariant.ServerChangeLock(DoorLockReason.AdminCommand, true);
						break;
					case "UNLOCKALL":
						doorVariant.NetworkActiveLocks = 0;
						break;
					case "DESTROY":
						IDamageableDoor damageableDoor;
						if ((damageableDoor = (doorVariant as IDamageableDoor)) != null)
							damageableDoor.ServerDamage(65535f, DoorDamageType.ServerCommand);
						break;
				}
				flag = true;
			}
			bool flag2 = command.EndsWith("e", StringComparison.OrdinalIgnoreCase);
			sender.RaReply(command + "#" + (flag ? string.Concat(new string[]
			{
				"Door ",
				door,
				" ",
				command.ToLower(),
				flag2 ? "d." : "ed."
			}) : ("Can't find door " + door + ".")), flag, true, "DoorsManagement");
			if (flag)
			{
				ServerLogs.AddLog(ServerLogs.Modules.Administrative, string.Concat(new string[]
				{
					sender.Nickname,
					" ",
					(sender is PlayerCommandSender) ? ("(" + sender.SenderId + ") ") : "",
					command.ToLower(),
					flag2 ? "d" : "ed",
					" door ",
					door,
					"."
				}), ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging, false);
			}
			return false;
		}
	}

	[HarmonyPatch(typeof(DoorLockUtils), "GetMode")]
	public static class DoorLockPatch
	{
		public static bool Prefix(DoorLockReason reason, ref DoorLockMode __result)
		{
			if ((ushort)reason > 256)
			{
				__result = DoorLockMode.FullLock;
				return false;
			}
			return true;
		}
	}
}
