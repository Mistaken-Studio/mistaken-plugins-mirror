using System;
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
	//[HarmonyPatch(typeof(Recontainer079), "BeginContainment")]
	public static class SCP079RecontainPatch
	{
		public static bool ErrorMode { get; private set; } = false;
		public static int SecondsLeft { get; private set; } = -1;
		public static bool Waiting { get; private set; } = false;
		public static bool Recontained { get; private set; } = false;

		public static CoroutineHandle? Handle;

		public static bool Prefix(bool forced)
		{
			if (Handle.HasValue)
			{
				Log.Warn("Duplicate of Recontain Request");
				return false;
			}
			Log.Debug("RECONT: -2");
			int rId = RoundPlus.RoundId;
			Handle = Timing.RunCoroutine(_Recontain(forced, rId).CancelWith(Recontainer079.singleton.gameObject).RerouteExceptions((ex) =>
			{
				Log.Error("SCP 079 Recontainment Failed");
				Log.Error(ex.Message);
				Log.Error(ex.StackTrace);
				Log.Info("Running base SCP 079 Recontainment Code");
				ErrorMode = true;
				Timing.RunCoroutine(Recontainer079.singleton._Recontain(forced).CancelWith(Recontainer079.singleton.gameObject).Append(() =>
				{
					ErrorMode = false;
					Handle = null;
					Recontained = true;
				}));
			}).Append(() =>
			{
				if(rId != RoundPlus.RoundId)
					Restart();
			}), Segment.FixedUpdate);
			Log.Debug("RECONT: -1");
			return false;
		}

		public static void Restart()
        {
			if(Handle.HasValue) 
				Timing.KillCoroutines(Handle.Value);
			Waiting = false;
			SecondsLeft = -1;
			Recontained = false;
			ErrorMode = false;
		}
		[System.Obsolete("It's broken :/")]
		private static IEnumerator<float> Recontain(bool forced)
		{
			Log.Debug("Starting SCP079 Recontainment");
			Log.Debug("RECONT: 0");
			int rId = RoundPlus.RoundId;
			Waiting = true;
			yield return Timing.WaitUntilFalse(() =>
				{
					try
					{
						return Cassie.IsSpeaking;
					}
					catch (System.Exception ex)
					{
						Log.Error("Error on SCP079 Recontainment|Cassie.IsSpeaking is null");
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
			Log.Debug("RECONT: 1");
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
			Log.Debug("RECONT: 2");
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
			Log.Debug("RECONT: 3");
			Waiting = true;
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
			Log.Debug("RECONT: 4");
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
			Log.Debug("RECONT: 5");
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
			Log.Debug("RECONT: 6");
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
			Log.Debug("RECONT: 9");
			Recontainer079.isLocked = true;
			Utilities.API.Map.TeslaMode = Utilities.API.TeslaMode.DISABLED;
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
			Log.Debug("RECONT: 10");
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
			Log.Debug("RECONT: 11");
			Utilities.API.Map.TeslaMode = Utilities.API.TeslaMode.ENABLED;
			Recontainer079.isLocked = false;
			Handle = null;
			yield break;
		}

		private static IEnumerator<float> _Recontain(bool forced, int rId)
		{
			PlayerStats ps = PlayerManager.localPlayer.GetComponent<PlayerStats>();
			SecondsLeft = 62;
            #region Wait
            Waiting = true;
			while (Cassie.IsSpeaking || Warhead.IsInProgress)
				yield return float.NegativeInfinity;
			Waiting = false;
			if (SecondsLeft <= -1 || rId != RoundPlus.RoundId)
			{
				Handle = null;
				yield break;
			}
            #endregion
            if (!forced)
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
            #region WaitS
            for (int i = 0; i < 55; i++)
			{
				SecondsLeft--;
				yield return Timing.WaitForSeconds(1);
			}
			if (SecondsLeft <= -1 || rId != RoundPlus.RoundId)
			{
				Handle = null;
				yield break;
			}
            #endregion
            #region Wait
            Waiting = true;
			while (Cassie.IsSpeaking || Warhead.IsInProgress)
				yield return float.NegativeInfinity;
			Waiting = false;
			if (SecondsLeft <= -1 || rId != RoundPlus.RoundId)
			{
				Handle = null;
				yield break;
			}
			#endregion
			Respawning.RespawnEffectsController.PlayCassieAnnouncement(string.Concat(new object[]
				{
				"JAM_",
				UnityEngine.Random.Range(0, 70).ToString("000"),
				"_",
				UnityEngine.Random.Range(1, 4),
				" SCP079RECON6"
			}), true, true);
			Respawning.RespawnEffectsController.PlayCassieAnnouncement((Scp079PlayerScript.instances.Count > 0) ? "SCP 0 7 9 SUCCESSFULLY TERMINATED USING GENERATOR RECONTAINMENT SEQUENCE" : "FACILITY IS BACK IN OPERATIONAL MODE", false, true);
			#region WaitS
			for (int i = 0; i < 7; i++)
			{
				SecondsLeft--;
				yield return Timing.WaitForSeconds(1);
			}
			if (SecondsLeft <= -1 || rId != RoundPlus.RoundId)
			{
				Handle = null;
				yield break;
			}
			#endregion
			Generator079.Generators[0].ServerOvercharge(10f, true);
			SecondsLeft = -1;
			Recontained = true;
			HashSet<DoorVariant> lockedDoors = new HashSet<DoorVariant>();
			foreach (DoorVariant doorVariant in UnityEngine.Object.FindObjectsOfType<DoorVariant>())
			{
				if (doorVariant is Interactables.Interobjects.BasicDoor && doorVariant.TryGetComponent(out Scp079Interactable scp079Interactable) && scp079Interactable.currentZonesAndRooms[0].currentZone == "HeavyRooms")
				{
					lockedDoors.Add(doorVariant);
					doorVariant.NetworkTargetState = false;
					doorVariant.ServerChangeLock(DoorLockReason.NoPower, true);
				}
			}
			Recontainer079.isLocked = true;
			Utilities.API.Map.TeslaMode = Utilities.API.TeslaMode.DISABLED;
			foreach (Scp079PlayerScript scp079PlayerScript in Scp079PlayerScript.instances)
				ps.HurtPlayer(new PlayerStats.HitInfo(1000001f, "WORLD", DamageTypes.Recontainment, 0), scp079PlayerScript.gameObject, true, true);
			yield return Timing.WaitForSeconds(10);
			if (SecondsLeft <= -1 || rId != RoundPlus.RoundId)
			{
				Handle = null;
				yield break;
			}
			foreach (DoorVariant doorVariant2 in lockedDoors)
				doorVariant2.ServerChangeLock(DoorLockReason.NoPower, false);
			Utilities.API.Map.TeslaMode = Utilities.API.TeslaMode.ENABLED;
			Recontainer079.isLocked = false;
			Handle = null;
			yield break;
		}
	}
}
