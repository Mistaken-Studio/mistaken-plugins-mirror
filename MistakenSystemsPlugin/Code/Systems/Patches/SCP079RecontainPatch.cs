﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RemoteAdmin;
using HarmonyLib;
using UnityEngine;
using Exiled.API.Features;
using MEC;
using Interactables.Interobjects.DoorUtils;
using Gamer.Utilities;

namespace Gamer.Mistaken.Systems.Patches
{
	[HarmonyPatch(typeof(Recontainer079), "_Recontain")]
	public static class SCP079RecontainPatch
	{
		public static int SecondsLeft = -1;
		public static bool Waiting = false;
		public static bool Recontained = false;

		public static CoroutineHandle? Handle;

		public static bool Prefix(bool forced)
		{
			if(Handle.HasValue)
            {
				Log.Warn("Duplicate of Recontain Request");
				return false;
            }
			Handle = Timing.RunCoroutine(Recontain(forced));
			return false;
		}

		public static void Restart()
        {
			if(Handle.HasValue) 
				Timing.KillCoroutines(Handle.Value);
			Waiting = false;
			SecondsLeft = -1;
			Recontained = false;
		}

		private static IEnumerator<float> Recontain(bool forced)
		{
			int rId = RoundPlus.RoundId;
			bool skipSpeaking;
			Waiting = true;
			try
            {
				skipSpeaking = false;
				_ = Cassie.IsSpeaking;
			}
			catch (System.Exception ex)
			{
				Log.Error("Error on SCP079 Recontainment|Cassie.IsSpeaking is null");
				Log.Error(ex.Message);
				Log.Error(ex.StackTrace);
				skipSpeaking = true;
			}
			if(skipSpeaking)
				yield return Timing.WaitUntilFalse(() =>
				{
					try
					{
						return Cassie.IsSpeaking;
					}
					catch (System.Exception ex)
					{
						Log.Error("Error on SCP079 Recontainment|Cassie.IsSpeaking is null|2");
						Log.Error(ex.Message);
						Log.Error(ex.StackTrace);
						return false;
					}
				});
			if (rId != RoundPlus.RoundId)
			{
				Restart();
				yield break;
			}
			Waiting = false;
            SecondsLeft = 62;
			if (!forced)
			{
				try
				{
					Respawning.RespawnEffectsController.PlayCassieAnnouncement(string.Concat(new object[]
					{
						"JAM_",
						UnityEngine.Random.Range(0, 70).ToString("000"),
						"_",
						UnityEngine.Random.Range(2, 5),
						" SCP079RECON5"
					}), false, true);
				}
				catch(System.Exception ex)
                {
					Log.Error("Error on SCP079 Recontainment|1");
					Log.Error(ex.Message);
					Log.Error(ex.StackTrace);
                }
			}
			for (int i = 0; i < 55; i++)
			{
				SecondsLeft--;
				yield return Timing.WaitForSeconds(1);
			}
			if (SecondsLeft <= -1)
			{
				Handle = null;
				yield break;
			}
			Waiting = true;
			try
			{
				skipSpeaking = false;
				_ = Cassie.IsSpeaking;
			}
			catch (System.Exception ex)
			{
				Log.Error("Error on SCP079 Recontainment|Cassie.IsSpeaking is null");
				Log.Error(ex.Message);
				Log.Error(ex.StackTrace);
				skipSpeaking = true;
			}
			if (skipSpeaking)
				yield return Timing.WaitUntilFalse(() =>
				{
					try 
					{ 
						return Cassie.IsSpeaking; 
					} 
					catch (System.Exception ex) 
					{
						Log.Error("Error on SCP079 Recontainment|Cassie.IsSpeaking is null|2"); 
						Log.Error(ex.Message); 
						Log.Error(ex.StackTrace);
						return false;
					}
				});
			if (rId != RoundPlus.RoundId)
			{
				Restart();
				yield break;
			}
			Waiting = false;
			try
			{
				Respawning.RespawnEffectsController.PlayCassieAnnouncement(string.Concat(new object[]
				{
					"JAM_",
					UnityEngine.Random.Range(0, 70).ToString("000"),
					"_",
					UnityEngine.Random.Range(1, 4),
					" SCP079RECON6"
				}), true, true);
				Respawning.RespawnEffectsController.PlayCassieAnnouncement((Scp079PlayerScript.instances.Count > 0) ? "SCP 0 7 9 SUCCESSFULLY TERMINATED USING GENERATOR RECONTAINMENT SEQUENCE" : "FACILITY IS BACK IN OPERATIONAL MODE", false, true);
			}
			catch (System.Exception ex)
			{
				Log.Error("Error on SCP079 Recontainment|2");
				Log.Error(ex.Message);
				Log.Error(ex.StackTrace);
			}
			for (int i = 0; i < 7; i++)
			{
				SecondsLeft--;
				yield return Timing.WaitForSeconds(1);
			}
			if (SecondsLeft <= -1)
			{
				Handle = null;
				yield break;
			}
			if (rId != RoundPlus.RoundId)
			{
				Restart();
				yield break;
			}
			SecondsLeft = -1;
			Recontained = true;
			HashSet<DoorVariant> lockedDoors = new HashSet<DoorVariant>();
			try
			{
				Generator079.Generators[0].ServerOvercharge(10f, true);
				foreach (DoorVariant doorVariant in UnityEngine.Object.FindObjectsOfType<DoorVariant>())
				{
					if (doorVariant is Interactables.Interobjects.BasicDoor && doorVariant.TryGetComponent(out Scp079Interactable scp079Interactable) && scp079Interactable.currentZonesAndRooms[0].currentZone == "HeavyRooms")
					{
						lockedDoors.Add(doorVariant);
						doorVariant.NetworkTargetState = false;
						doorVariant.ServerChangeLock(DoorLockReason.NoPower, true);
					}
				}
			}
			catch (System.Exception ex)
			{
				Log.Error("Error on SCP079 Recontainment|3");
				Log.Error(ex.Message);
				Log.Error(ex.StackTrace);
			}
			Recontainer079.isLocked = true;
			try
			{
				PlayerStats ps = Server.Host.ReferenceHub.playerStats;
				foreach (Scp079PlayerScript scp079PlayerScript in Scp079PlayerScript.instances)
					ps.HurtPlayer(new PlayerStats.HitInfo(1000001f, "WORLD", DamageTypes.Recontainment, 0), scp079PlayerScript.gameObject, true, true);
			}
			catch (System.Exception ex)
			{
				Log.Error("Error on SCP079 Recontainment|4");
				Log.Error(ex.Message);
				Log.Error(ex.StackTrace);
			}
			yield return Timing.WaitForSeconds(10);
			try
			{
				foreach (DoorVariant doorVariant2 in lockedDoors)
					doorVariant2.ServerChangeLock(DoorLockReason.NoPower, false);
			}
			catch (System.Exception ex)
			{
				Log.Error("Error on SCP079 Recontainment|5");
				Log.Error(ex.Message);
				Log.Error(ex.StackTrace);
			}
			Recontainer079.isLocked = false;
			Handle = null;
			yield break;
		}
	}
}
