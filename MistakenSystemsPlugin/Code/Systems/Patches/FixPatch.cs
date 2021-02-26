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

namespace Gamer.Mistaken.Systems.Patches
{
	//[HarmonyPatch(typeof(PlayerMovementSync), "ReceiveRotation")]
	static class Fix1Patch //Check
	{
		public static bool Prefix(NetworkConnection connection, PlayerMovementSync.RotationMessage rotation)
		{
			try
			{
				if (ReferenceHub.GetHub(connection?.identity?.gameObject)?.playerMovementSync == null) return false;
			}
			catch
			{
				return false;
			}
			return true;
		}
	}

	//[HarmonyPatch(typeof(PlayerMovementSync), "ReceivePosition")]
	static class Fix2Patch //Check
	{
		public static bool Prefix(NetworkConnection connection, PlayerMovementSync.PositionMessage position)
		{
			try
			{
				if (ReferenceHub.GetHub(connection?.identity?.gameObject)?.playerMovementSync == null) return false;
			}
			catch
            {
				return false;
            }
			return true;
		}
	}
}
