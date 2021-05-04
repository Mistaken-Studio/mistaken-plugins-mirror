#pragma warning disable

using Exiled.API.Features;
using Gamer.Utilities;
using HarmonyLib;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using MEC;
using Respawning;
using System.Collections.Generic;

namespace Gamer.Mistaken.Systems.Patches
{
    //[HarmonyPatch(typeof(Recontainer079), "BeginContainment")]
    public static class SCP079RecontainPatch
    {
        public static bool ErrorMode { get; private set; } = false;
        private static int _secondsLeft = -1;
        public static int SecondsLeft
        {
            get => _secondsLeft;
            private set
            {
                _secondsLeft = value;
                Exiled.Events.Handlers.CustomEvents.SCP079.TimeToRecontainment = value;
            }
        }
        private static bool _waiting = false;
        public static bool Waiting
        {
            get => _waiting;
            set
            {
                _waiting = value;
                Exiled.Events.Handlers.CustomEvents.SCP079.IsRecontainmentPaused = value;
            }
        }
        public static bool Recontained { get; private set; } = false;

        public static bool Prefix(bool forced)
        {
            Timing.RunCoroutine(SCP079RecontainPatch._Recontain(forced).CancelWith(Recontainer079.singleton.gameObject), Segment.FixedUpdate);
            return false;
        }

        public static void Restart()
        {
            Waiting = false;
            SecondsLeft = -1;
            Recontained = false;
            ErrorMode = false;

            SCP079RecontainInfoPatch.Restart();
        }

        public static IEnumerator<float> _Recontain(bool forced)
        {
            Log.Debug("Recontain 79: 1");
            SecondsLeft = 62;
            PlayerStats ps = PlayerManager.localPlayer.GetComponent<PlayerStats>();
            NineTailedFoxAnnouncer annc = NineTailedFoxAnnouncer.singleton;
            Log.Debug("Recontain 79: 2");
            Waiting = true;
            while (/*!annc.Free*/annc.queue.Count != 0 || AlphaWarheadController.Host.inProgress)
                yield return float.NegativeInfinity;
            Log.Debug("Recontain 79: 3");
            Waiting = false;
            if (!forced)
            {
                RespawnEffectsController.PlayCassieAnnouncement(string.Concat(new object[]
                {
                    "JAM_",
                    UnityEngine.Random.Range(0, 70).ToString("000"),
                    "_",
                    UnityEngine.Random.Range(2, 5),
                    " SCP079RECON5"
                }), false, true);
            }
            Log.Debug("Recontain 79: 4");
            for (int i = 0; i < 55; i++)
            {
                yield return Timing.WaitForSeconds(1f);
                SecondsLeft--;
            }
            Log.Debug("Recontain 79: 5");
            Waiting = true;
            while (/*!annc.Free*/annc.queue.Count != 0 || AlphaWarheadController.Host.inProgress)
                yield return float.NegativeInfinity;
            Waiting = false;
            Log.Debug("Recontain 79: 6");
            RespawnEffectsController.PlayCassieAnnouncement(string.Concat(new object[]
            {
                "JAM_",
                UnityEngine.Random.Range(0, 70).ToString("000"),
                "_",
                UnityEngine.Random.Range(1, 4),
                " SCP079RECON6"
            }), true, true);
            Log.Debug("Recontain 79: 7");
            RespawnEffectsController.PlayCassieAnnouncement((Scp079PlayerScript.instances.Count > 0) ? "SCP 0 7 9 SUCCESSFULLY TERMINATED USING GENERATOR RECONTAINMENT SEQUENCE" : "FACILITY IS BACK IN OPERATIONAL MODE", false, true);
            Log.Debug("Recontain 79: 8");
            for (int i = 0; i < 7; i++)
            {
                yield return Timing.WaitForSeconds(1f);
                SecondsLeft--;
            }
            Log.Debug("Recontain 79: 9");
            Generator079.Generators[0].ServerOvercharge(10f, true);
            Log.Debug("Recontain 79: 10");
            HashSet<DoorVariant> lockedDoors = new HashSet<DoorVariant>();
            foreach (DoorVariant doorVariant in UnityEngine.Object.FindObjectsOfType<DoorVariant>())
            {
                Scp079Interactable scp079Interactable;
                if (doorVariant is BasicDoor && doorVariant.TryGetComponent<Scp079Interactable>(out scp079Interactable) && scp079Interactable.currentZonesAndRooms[0].currentZone == "HeavyRooms")
                {
                    lockedDoors.Add(doorVariant);
                    doorVariant.NetworkTargetState = false;
                    doorVariant.ServerChangeLock(DoorLockReason.NoPower, true);
                }
            }
            Log.Debug("Recontain 79: 11");
            Recontainer079.isLocked = true;
            Log.Debug("Recontain 79: 12");
            foreach (Scp079PlayerScript scp079PlayerScript in Scp079PlayerScript.instances)
            {
                ps.HurtPlayer(new PlayerStats.HitInfo(1000001f, "WORLD", DamageTypes.Recontainment, 0), scp079PlayerScript.gameObject, true, true);
            }
            Log.Debug("Recontain 79: 13");
            Recontained = true;
            for (int i = 0; i < 10; i++)
            {
                yield return Timing.WaitForSeconds(1f);
            }
            Log.Debug("Recontain 79: 14");
            foreach (DoorVariant doorVariant2 in lockedDoors)
            {
                doorVariant2.ServerChangeLock(DoorLockReason.NoPower, false);
            }
            Log.Debug("Recontain 79: 15");
            Recontainer079.isLocked = false;
            Log.Debug("Recontain 79: 16");
            SecondsLeft = -1;
            Waiting = false;
            yield break;
        }
    }
    [HarmonyPatch(typeof(Recontainer079), "BeginContainment")]
    public static class SCP079RecontainInfoPatch
    {
        public static bool Recontaining { get; private set; } = false;

        public static void Postfix(bool forced)
        {
            Recontaining = true;
            Log.Debug("RUN POSTFIX ON Recontainer079.BeginContainment");
        }

        public static void Restart()
        {
            Recontaining = false;
        }
    }
}
