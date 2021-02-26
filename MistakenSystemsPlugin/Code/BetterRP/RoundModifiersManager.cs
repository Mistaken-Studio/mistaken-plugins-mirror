using Exiled.API.Extensions;
using Exiled.API.Features;
using Gamer.Utilities;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using LightContainmentZoneDecontamination;
using MEC;
using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Gamer.Mistaken.BetterRP
{
    public class RoundModifiersManager
    {
        public static RoundModifiersManager Instance { get; private set; }
        public static void SetInstance() => Instance = new RoundModifiersManager();
        [Flags]
        public enum RandomEvents : uint
        {
            NONE = 0,
            GATE_A_PERNAMENT_CLOSE = 1,
            GATE_B_PERNAMENT_CLOSE = 2,
            GATE_A_ELEVATOR_LOCKDOWN = 4,
            GATE_B_ELEVATOR_LOCKDOWN = 8,
            LCZ_DECONTAMINATION_REAL_20_MINUTES = 16,
            LCZ_DECONTAMINATION_REAL_17_MINUTES = 32,
            LCZ_DECONTAMINATION_REAL_15_MINUTES = 64,
            LCZ_DECONTAMINATION_REAL_10_MINUTES = 128,
            LCZ_DECONTAMINATION_REAL_5_MINUTES = 256,
            LCZ_DECONTAMINATION_PAUSED = 512,
            SURFACE_GATE_LOCKDOWN = 1024,
            LIFT_A_LOCKDOWN = 2048,
            LIFT_B_LOCKDOWN = 4096,
            CHECKPOINT_A_PERNAMENT_OPEN = 8192,
            CHECKPOINT_B_PERNAMENT_OPEN = 16384,
            CHECKPOINT_A_PERNAMENT_CLOSE = 32768,
            CHECKPOINT_B_PERNAMENT_CLOSE = 65536,
            BLACKOUT_FOR_5_SECONDS_EVERY_1_MINUTE = 131072,
            BLACKOUT_FOR_5_SECONDS_EVERY_2_MINUTE = 262144,
            BLACKOUT_FOR_20_SECONDS_EVERY_5_MINUTE = 524288,
            BLACKOUT_FOR_1_SECONDS_EVERY_30_SECONDS = 1048576,
            CHECKPOINT_EZ_PERNAMENT_OPEN = 2097152,
            CASSIE_AUTO_SCAN_5_MINUTES = 4194304,
            CASSIE_AUTO_SCAN_10_MINUTES = 8388608,
            CASSIE_AUTO_SCAN_15_MINUTES = 16777216,
            ALL_DOORS_OPEN_AT_BEGINING = 33554432,
            GATE_A_PERNAMENT_OPEN = 67108864,
            GATE_B_PERNAMENT_OPEN = 134217728,
            TESLA_GATES_DISABLED = 268435456,
        }
        public RandomEvents ActiveEvents
        {
            get
            {
                return _ae;
            }
        }
        private RandomEvents _ae;
        public static int RandomEventsLength
        {
            get
            {
                return Enum.GetValues(typeof(RandomEvents)).Length - 1;
            }
        }

        private const short RESetTries = 1000;
        public bool SetActiveEvents(int events = -1)
        {
            if (events == -1) 
                events = UnityEngine.Random.Range(1, 12);
            return SetActiveEvents(events, 0);
        }
        private bool SetActiveEvents(int events, int check)
        {
            _ae = 0;
            check++;
            bool broken = false;

            for (int i = 0; i < events; i++)
            {
                bool success = false;
                int tries = 0;
                while (!success && !broken)
                {
                    tries++;
                    var num = UnityEngine.Random.Range(0, RandomEventsLength);
                    RandomEvents re = (RandomEvents)Math.Pow(2, num);
                    if (!_ae.HasFlag(re))
                    {
                        _ae |= re;
                        success = true;
                    }
                    if (tries > 100)
                    {
                        broken = true;
                        break;
                    }
                }
                if (broken) 
                    break;
            }
            if(check < (RESetTries ^ Mathf.RoundToInt(events/2)))
            {
                if (!ValidateRandomEvents(_ae) || broken)
                    return SetActiveEvents(events, check);
                else
                    return true;
            }      
            else
            {
                Log.Warn($"Failed to generate ActiveEvents in {RESetTries} tries !");
                Log.Warn("Target Events Amount: " + events);
                _ae = 0;
                return false;
            }
        }

        private bool ValidateRandomEvents(RandomEvents events)
        {
            if (events.HasFlag(RandomEvents.LIFT_A_LOCKDOWN) && events.HasFlag(RandomEvents.LIFT_B_LOCKDOWN)) return false;
            if ((events.HasFlag(RandomEvents.GATE_A_ELEVATOR_LOCKDOWN) || events.HasFlag(RandomEvents.GATE_A_PERNAMENT_CLOSE)) && (events.HasFlag(RandomEvents.GATE_B_ELEVATOR_LOCKDOWN) || events.HasFlag(RandomEvents.GATE_B_PERNAMENT_CLOSE))) return false;
            if (events.HasFlag(RandomEvents.GATE_A_PERNAMENT_CLOSE) && events.HasFlag(RandomEvents.GATE_A_PERNAMENT_OPEN)) return false;
            if (events.HasFlag(RandomEvents.GATE_B_PERNAMENT_CLOSE) && events.HasFlag(RandomEvents.GATE_B_PERNAMENT_OPEN)) return false;
            if (events.HasFlag(RandomEvents.SURFACE_GATE_LOCKDOWN) && (events.HasFlag(RandomEvents.GATE_A_ELEVATOR_LOCKDOWN) || events.HasFlag(RandomEvents.GATE_A_PERNAMENT_CLOSE) || events.HasFlag(RandomEvents.GATE_B_ELEVATOR_LOCKDOWN) || events.HasFlag(RandomEvents.GATE_B_PERNAMENT_CLOSE))) return false;
            if (events.HasFlag(RandomEvents.CHECKPOINT_A_PERNAMENT_CLOSE) && events.HasFlag(RandomEvents.CHECKPOINT_A_PERNAMENT_OPEN)) return false;
            if (events.HasFlag(RandomEvents.CHECKPOINT_B_PERNAMENT_CLOSE) && events.HasFlag(RandomEvents.CHECKPOINT_B_PERNAMENT_OPEN)) return false;
            if ((events.HasFlag(RandomEvents.CHECKPOINT_A_PERNAMENT_CLOSE) || events.HasFlag(RandomEvents.LIFT_A_LOCKDOWN)) && (events.HasFlag(RandomEvents.CHECKPOINT_B_PERNAMENT_CLOSE) || events.HasFlag(RandomEvents.LIFT_B_LOCKDOWN))) return false;
            if (events.HasFlag(RandomEvents.CHECKPOINT_B_PERNAMENT_CLOSE) && events.HasFlag(RandomEvents.LIFT_A_LOCKDOWN)) return false;
            if (events.HasFlag(RandomEvents.LCZ_DECONTAMINATION_PAUSED) && (events.HasFlag(RandomEvents.LCZ_DECONTAMINATION_REAL_10_MINUTES) || events.HasFlag(RandomEvents.LCZ_DECONTAMINATION_REAL_15_MINUTES) || events.HasFlag(RandomEvents.LCZ_DECONTAMINATION_REAL_5_MINUTES) || events.HasFlag(RandomEvents.LCZ_DECONTAMINATION_REAL_20_MINUTES) || events.HasFlag(RandomEvents.LCZ_DECONTAMINATION_REAL_17_MINUTES))) return false;
            if (events.HasFlag(RandomEvents.LCZ_DECONTAMINATION_REAL_10_MINUTES) && (events.HasFlag(RandomEvents.LCZ_DECONTAMINATION_PAUSED) || events.HasFlag(RandomEvents.LCZ_DECONTAMINATION_REAL_15_MINUTES) || events.HasFlag(RandomEvents.LCZ_DECONTAMINATION_REAL_5_MINUTES) || events.HasFlag(RandomEvents.LCZ_DECONTAMINATION_REAL_20_MINUTES) || events.HasFlag(RandomEvents.LCZ_DECONTAMINATION_REAL_17_MINUTES))) return false;
            if (events.HasFlag(RandomEvents.LCZ_DECONTAMINATION_REAL_15_MINUTES) && (events.HasFlag(RandomEvents.LCZ_DECONTAMINATION_REAL_10_MINUTES) || events.HasFlag(RandomEvents.LCZ_DECONTAMINATION_PAUSED) || events.HasFlag(RandomEvents.LCZ_DECONTAMINATION_REAL_5_MINUTES) || events.HasFlag(RandomEvents.LCZ_DECONTAMINATION_REAL_20_MINUTES) || events.HasFlag(RandomEvents.LCZ_DECONTAMINATION_REAL_17_MINUTES))) return false;
            if (events.HasFlag(RandomEvents.LCZ_DECONTAMINATION_REAL_5_MINUTES) && (events.HasFlag(RandomEvents.LCZ_DECONTAMINATION_REAL_15_MINUTES) || events.HasFlag(RandomEvents.LCZ_DECONTAMINATION_REAL_10_MINUTES) || events.HasFlag(RandomEvents.LCZ_DECONTAMINATION_PAUSED) || events.HasFlag(RandomEvents.LCZ_DECONTAMINATION_REAL_20_MINUTES) || events.HasFlag(RandomEvents.LCZ_DECONTAMINATION_REAL_17_MINUTES))) return false;
            if (events.HasFlag(RandomEvents.LCZ_DECONTAMINATION_REAL_20_MINUTES) && (events.HasFlag(RandomEvents.LCZ_DECONTAMINATION_REAL_5_MINUTES) || events.HasFlag(RandomEvents.LCZ_DECONTAMINATION_REAL_15_MINUTES) || events.HasFlag(RandomEvents.LCZ_DECONTAMINATION_REAL_10_MINUTES) || events.HasFlag(RandomEvents.LCZ_DECONTAMINATION_PAUSED) || events.HasFlag(RandomEvents.LCZ_DECONTAMINATION_REAL_17_MINUTES))) return false;
            if (events.HasFlag(RandomEvents.LCZ_DECONTAMINATION_REAL_17_MINUTES) && (events.HasFlag(RandomEvents.LCZ_DECONTAMINATION_REAL_20_MINUTES) || events.HasFlag(RandomEvents.LCZ_DECONTAMINATION_REAL_5_MINUTES) || events.HasFlag(RandomEvents.LCZ_DECONTAMINATION_REAL_15_MINUTES) || events.HasFlag(RandomEvents.LCZ_DECONTAMINATION_REAL_10_MINUTES) || events.HasFlag(RandomEvents.LCZ_DECONTAMINATION_PAUSED))) return false;
            if (events.HasFlag(RandomEvents.BLACKOUT_FOR_1_SECONDS_EVERY_30_SECONDS) && (events.HasFlag(RandomEvents.BLACKOUT_FOR_20_SECONDS_EVERY_5_MINUTE) || events.HasFlag(RandomEvents.BLACKOUT_FOR_5_SECONDS_EVERY_1_MINUTE) || events.HasFlag(RandomEvents.BLACKOUT_FOR_5_SECONDS_EVERY_2_MINUTE))) return false;
            if (events.HasFlag(RandomEvents.BLACKOUT_FOR_20_SECONDS_EVERY_5_MINUTE) && (events.HasFlag(RandomEvents.BLACKOUT_FOR_1_SECONDS_EVERY_30_SECONDS) || events.HasFlag(RandomEvents.BLACKOUT_FOR_5_SECONDS_EVERY_1_MINUTE) || events.HasFlag(RandomEvents.BLACKOUT_FOR_5_SECONDS_EVERY_2_MINUTE))) return false;
            if (events.HasFlag(RandomEvents.BLACKOUT_FOR_5_SECONDS_EVERY_1_MINUTE) && (events.HasFlag(RandomEvents.BLACKOUT_FOR_20_SECONDS_EVERY_5_MINUTE) || events.HasFlag(RandomEvents.BLACKOUT_FOR_1_SECONDS_EVERY_30_SECONDS) || events.HasFlag(RandomEvents.BLACKOUT_FOR_5_SECONDS_EVERY_2_MINUTE))) return false;
            if (events.HasFlag(RandomEvents.BLACKOUT_FOR_5_SECONDS_EVERY_2_MINUTE) && (events.HasFlag(RandomEvents.BLACKOUT_FOR_20_SECONDS_EVERY_5_MINUTE) || events.HasFlag(RandomEvents.BLACKOUT_FOR_5_SECONDS_EVERY_1_MINUTE) || events.HasFlag(RandomEvents.BLACKOUT_FOR_1_SECONDS_EVERY_30_SECONDS))) return false;
            if (events.HasFlag(RandomEvents.CASSIE_AUTO_SCAN_5_MINUTES) && (events.HasFlag(RandomEvents.CASSIE_AUTO_SCAN_10_MINUTES) || events.HasFlag(RandomEvents.CASSIE_AUTO_SCAN_15_MINUTES))) return false;
            if (events.HasFlag(RandomEvents.CASSIE_AUTO_SCAN_10_MINUTES) && (events.HasFlag(RandomEvents.CASSIE_AUTO_SCAN_5_MINUTES) || events.HasFlag(RandomEvents.CASSIE_AUTO_SCAN_15_MINUTES))) return false;
            if (events.HasFlag(RandomEvents.CASSIE_AUTO_SCAN_15_MINUTES) && (events.HasFlag(RandomEvents.CASSIE_AUTO_SCAN_10_MINUTES) || events.HasFlag(RandomEvents.CASSIE_AUTO_SCAN_5_MINUTES))) return false;
            return true;
        }

        public void ExecuteFags()
        {
            foreach (var item in Map.Lifts)
                item.Network_locked = false;

            foreach (var item in Map.Doors)
            {
                item.NetworkTargetState = false;
                item.NetworkActiveLocks = 0;
            }
            Systems.Utilities.API.Map.TeslaMode = Systems.Utilities.API.TeslaMode.ENABLED;
            Systems.Utilities.API.Map.Blackout.Enabled = false;

            if (_ae == 0)
            {
                Log.Debug("No Acitve Random Events");
                return;
            }
            List<string> locked = NorthwoodLib.Pools.ListPool<string>.Shared.Rent(0);
            List<string> open = NorthwoodLib.Pools.ListPool<string>.Shared.Rent(0);
            bool teslaOff = false;
            for (int i = 0; i < RandomEventsLength; i++)
            {
                RandomEvents re = (RandomEvents)Math.Pow(2, i);
                if (_ae.HasFlag(re))
                {
                    Log.Debug("Executing: " + re);
                    switch (re)
                    {
                        case RandomEvents.LCZ_DECONTAMINATION_PAUSED:
                            {
                                DecontaminationController.Singleton.disableDecontamination = true;
                                break;
                            }
                        case RandomEvents.LCZ_DECONTAMINATION_REAL_20_MINUTES:
                            {
                                DecontaminationController.Singleton.NetworkRoundStartTime = GetNewLCZTime(1200);
                                break;
                            }
                        case RandomEvents.LCZ_DECONTAMINATION_REAL_17_MINUTES:
                            {
                                DecontaminationController.Singleton.NetworkRoundStartTime = GetNewLCZTime(1020);
                                break;
                            }
                        case RandomEvents.LCZ_DECONTAMINATION_REAL_15_MINUTES:
                            {
                                DecontaminationController.Singleton.NetworkRoundStartTime = GetNewLCZTime(900);
                                break;
                            }
                        case RandomEvents.LCZ_DECONTAMINATION_REAL_10_MINUTES:
                            {
                                Timing.CallDelayed(45, () => {
                                    DecontaminationController.Singleton.NetworkRoundStartTime = GetNewLCZTime(600);
                                });
                                break;
                            }
                        case RandomEvents.LCZ_DECONTAMINATION_REAL_5_MINUTES:
                            {
                                Timing.CallDelayed(45, () => {
                                    DecontaminationController.Singleton.NetworkRoundStartTime = GetNewLCZTime(300);
                                });
                                break;
                            }
                        case RandomEvents.BLACKOUT_FOR_1_SECONDS_EVERY_30_SECONDS:
                            {
                                Systems.Utilities.API.Map.Blackout.Delay = 30;
                                Systems.Utilities.API.Map.Blackout.Length = 1;
                                Systems.Utilities.API.Map.Blackout.Enabled = true;
                                break;
                            }
                        case RandomEvents.BLACKOUT_FOR_20_SECONDS_EVERY_5_MINUTE:
                            {
                                Systems.Utilities.API.Map.Blackout.Delay = 300;
                                Systems.Utilities.API.Map.Blackout.Length = 20;
                                Systems.Utilities.API.Map.Blackout.Enabled = true;
                                break;
                            }
                        case RandomEvents.BLACKOUT_FOR_5_SECONDS_EVERY_1_MINUTE:
                            {
                                Systems.Utilities.API.Map.Blackout.Delay = 60;
                                Systems.Utilities.API.Map.Blackout.Length = 5;
                                Systems.Utilities.API.Map.Blackout.Enabled = true;
                                break;
                            }
                        case RandomEvents.BLACKOUT_FOR_5_SECONDS_EVERY_2_MINUTE:
                            {
                                Systems.Utilities.API.Map.Blackout.Delay = 120;
                                Systems.Utilities.API.Map.Blackout.Length = 5;
                                Systems.Utilities.API.Map.Blackout.Enabled = true;
                                break;
                            }
                        case RandomEvents.CASSIE_AUTO_SCAN_10_MINUTES:
                            {
                                Timing.RunCoroutine(ToScan(600));
                                break;
                            }
                        case RandomEvents.CASSIE_AUTO_SCAN_5_MINUTES:
                            {
                                Timing.RunCoroutine(ToScan(300));
                                break;
                            }
                        case RandomEvents.CASSIE_AUTO_SCAN_15_MINUTES:
                            {
                                Timing.RunCoroutine(ToScan(900));
                                break;
                            }
                        case RandomEvents.LIFT_A_LOCKDOWN:
                            {
                                locked.Add("LIGHT CONTAINMENT ZONE ELEVATOR A");
                                foreach (var item in Map.Lifts.Where(e => e.Type() == Exiled.API.Enums.ElevatorType.LczA))
                                    item.Network_locked = true;
                                break;
                            }
                        case RandomEvents.LIFT_B_LOCKDOWN:
                            {
                                locked.Add("LIGHT CONTAINMENT ZONE ELEVATOR B");
                                foreach (var item in Map.Lifts.Where(e => e.Type() == Exiled.API.Enums.ElevatorType.LczB))
                                    item.Network_locked = true;
                                break;
                            }
                        case RandomEvents.GATE_A_ELEVATOR_LOCKDOWN:
                            {
                                foreach (var item in Map.Lifts.Where(e => e.Type() == Exiled.API.Enums.ElevatorType.GateA))
                                    item.Network_locked = true;
                                break;
                            }
                        case RandomEvents.GATE_B_ELEVATOR_LOCKDOWN:
                            {
                                foreach (var item in Map.Lifts.Where(e => e.Type() == Exiled.API.Enums.ElevatorType.GateB))
                                    item.Network_locked = true;
                                break;
                            }
                        case RandomEvents.SURFACE_GATE_LOCKDOWN:
                            {
                                if (!Map.Doors.Any(d => d.Type() == Exiled.API.Enums.DoorType.SurfaceGate))
                                    break;
                                locked.Add("SURFACE GATE");
                                Map.Doors.First(d => d.Type() == Exiled.API.Enums.DoorType.SurfaceGate).NetworkActiveLocks = (byte)DoorLockReason.AdminCommand;
                                break;
                            }
                        case RandomEvents.GATE_A_PERNAMENT_OPEN:
                            {
                                if (!Map.Doors.Any(d => d.Type() == Exiled.API.Enums.DoorType.GateA))
                                    break;
                                open.Add("GATE A");
                                var door = Map.Doors.First(d => d.Type() == Exiled.API.Enums.DoorType.GateA);
                                door.NetworkActiveLocks = (byte)DoorLockReason.AdminCommand;
                                door.NetworkTargetState = true;
                                break;
                            }
                        case RandomEvents.GATE_B_PERNAMENT_OPEN:
                            {
                                if (!Map.Doors.Any(d => d.Type() == Exiled.API.Enums.DoorType.GateB))
                                    break;
                                open.Add("GATE B");
                                var door = Map.Doors.First(d => d.Type() == Exiled.API.Enums.DoorType.GateB);
                                door.NetworkActiveLocks = (byte)DoorLockReason.AdminCommand;
                                door.NetworkTargetState = true;
                                break;
                            }
                        case RandomEvents.GATE_A_PERNAMENT_CLOSE:
                            {
                                if (!Map.Doors.Any(d => d.Type() == Exiled.API.Enums.DoorType.GateA))
                                    break;
                                locked.Add("GATE A");
                                var door = Map.Doors.First(d => d.Type() == Exiled.API.Enums.DoorType.GateA);
                                door.NetworkActiveLocks = (byte)DoorLockReason.AdminCommand;
                                door.NetworkTargetState = false;
                                break;
                            }
                        case RandomEvents.GATE_B_PERNAMENT_CLOSE:
                            {
                                if (!Map.Doors.Any(d => d.Type() == Exiled.API.Enums.DoorType.GateB))
                                    break;
                                locked.Add("GATE B");
                                var door = Map.Doors.First(d => d.Type() == Exiled.API.Enums.DoorType.GateB);
                                door.NetworkActiveLocks = (byte)DoorLockReason.AdminCommand;
                                door.NetworkTargetState = false;
                                break;
                            }
                        case RandomEvents.CHECKPOINT_EZ_PERNAMENT_OPEN:
                            {
                                if (!Map.Doors.Any(d => d.Type() == Exiled.API.Enums.DoorType.CheckpointEntrance))
                                    break;
                                open.Add("CHECKPOINT ENTRANCE ZONE");
                                var door = Map.Doors.First(d => d.Type() == Exiled.API.Enums.DoorType.CheckpointEntrance);
                                door.BreakDoor();
                                break;
                            }
                        case RandomEvents.ALL_DOORS_OPEN_AT_BEGINING:
                            {
                                Map.Doors.Where(d => d.Type() == Exiled.API.Enums.DoorType.EntranceDoor || d.Type() == Exiled.API.Enums.DoorType.HeavyContainmentDoor || d.Type() == Exiled.API.Enums.DoorType.LightContainmentDoor).Select(door => door.NetworkActiveLocks = (byte)DoorLockReason.AdminCommand);
                                break;
                            }
                        case RandomEvents.CHECKPOINT_A_PERNAMENT_CLOSE:
                            {
                                if (!Map.Doors.Any(d => d.Type() == Exiled.API.Enums.DoorType.CheckpointLczA))
                                    break;
                                locked.Add("CHECKPOINT A");
                                var door = Map.Doors.First(d => d.Type() == Exiled.API.Enums.DoorType.CheckpointLczA);
                                door.NetworkActiveLocks = (byte)DoorLockReason.AdminCommand;
                                door.NetworkTargetState = false;
                                break;
                            }
                        case RandomEvents.CHECKPOINT_B_PERNAMENT_CLOSE:
                            {
                                if (!Map.Doors.Any(d => d.Type() == Exiled.API.Enums.DoorType.CheckpointLczB))
                                    break;
                                locked.Add("CHECKPOINT B");
                                var door = Map.Doors.First(d => d.Type() == Exiled.API.Enums.DoorType.CheckpointLczB);
                                door.NetworkActiveLocks = (byte)DoorLockReason.AdminCommand;
                                door.NetworkTargetState = false;
                                break;
                            }
                        case RandomEvents.CHECKPOINT_A_PERNAMENT_OPEN:
                            {
                                if (!Map.Doors.Any(d => d.Type() == Exiled.API.Enums.DoorType.CheckpointLczA))
                                    break;
                                open.Add("CHECKPOINT A");
                                var door = Map.Doors.First(d => d.Type() == Exiled.API.Enums.DoorType.CheckpointLczA);
                                door.BreakDoor();
                                break;
                            }
                        case RandomEvents.CHECKPOINT_B_PERNAMENT_OPEN:
                            {
                                if (!Map.Doors.Any(d => d.Type() == Exiled.API.Enums.DoorType.CheckpointLczB))
                                    break;
                                open.Add("CHECKPOINT B");
                                var door = Map.Doors.First(d => d.Type() == Exiled.API.Enums.DoorType.CheckpointLczB);
                                door.BreakDoor();
                                break;
                            }
                        case RandomEvents.TESLA_GATES_DISABLED:
                            {
                                Systems.Utilities.API.Map.TeslaMode = Systems.Utilities.API.TeslaMode.DISABLED;
                                teslaOff = true;
                                break;
                            }
                    }
                }
            }

            string cassie = "ATTENTION ALL PERSONNEL . DETECTED CASSIESYSTEM CRITICAL ERROR . . " + (locked.Count != 0 ? "INITIATED LOCKDOWN ON " + string.Join(" . ", locked) : "") + " . " + (open.Count != 0 ? " SYSTEM OPEN LOCKDOWN " + string.Join(" . ", open) : "") + " . " + (teslaOff ? "TESLA GATES DISABLED" : "");

            if (locked.Count != 0 || open.Count != 0)
                Cassie.DelayedMessage(cassie, 60);

            NorthwoodLib.Pools.ListPool<string>.Shared.Return(locked);
            NorthwoodLib.Pools.ListPool<string>.Shared.Return(open);

            string toWrite = "[RPE] Active: ";
            for (int i = 1; i <= ((uint[])Enum.GetValues(typeof(RandomEvents))).Max(); i *= 2)
            {
                if(_ae.HasFlag((RandomEvents)i))
                {
                    toWrite += $"\n- {(RandomEvents)i}";
                }
            }
            foreach (var item in Player.List.Where(p => p.RemoteAdminAccess))
                item.SendConsoleMessage(toWrite, "green");
            MapPlus.Broadcast("RPE", 10, "RandomRPEvents activated, check console for more info", Broadcast.BroadcastFlags.AdminChat);
        }

        public IEnumerator<float> ToScan(float interval)
        {
            yield return Timing.WaitForSeconds(1);
            int rid = RoundPlus.RoundId; 
            while (Round.IsStarted && rid == RoundPlus.RoundId)
            {
                yield return Timing.WaitForSeconds(interval);
                int SCP = Player.Get(Team.SCP).Count();
                int CDP = Player.Get(Team.CDP).Count();
                int RSC = Player.Get(Team.RSC).Count();
                int MTF = Player.Get(Team.MTF).Count();
                int CHI = Player.Get(Team.CHI).Count();
                while (Cassie.IsSpeaking)
                    yield return Timing.WaitForOneFrame;
                Cassie.Message($"FACILITY SCAN RESULT . {(SCP == 0 ? "NO" : SCP.ToString())} SCPSUBJECT{(SCP == 1 ? "" : "S")} . {(CDP == 0 ? "NO" : CDP.ToString())} CLASSD . {(RSC == 0 ? "NO" : RSC.ToString())} scientist{(RSC == 1 ? "" : "S")} . {(MTF == 0 ? "NO" : MTF.ToString())} FoUNDATION FORCES . {(CHI == 0 ? "NO" : CHI.ToString())} CHAOSINSURGENCY");
            }
        }

        public static void Restart()
        {
            Systems.Utilities.API.Map.TeslaMode = Systems.Utilities.API.TeslaMode.ENABLED;
            RoundModifiersManager.Instance._ae = RandomEvents.NONE;
        }

        private double GetNewLCZTime(double fullTime)
        {
            if (fullTime < 1) throw new Exception("Full Time can't be smaller than 1");
            Log.Debug("GNLCZT| START");
            Log.Debug("GNLCZT| " + NetworkTime.time);
            Log.Debug("GNLCZT| " + DecontaminationController.Singleton.DecontaminationPhases.First(i => i.Function == DecontaminationController.DecontaminationPhase.PhaseFunction.Final).TimeTrigger);
            Log.Debug("GNLCZT| " + fullTime);
            var tor = (NetworkTime.time - DecontaminationController.Singleton.DecontaminationPhases.First(i =>
              i.Function == DecontaminationController.DecontaminationPhase.PhaseFunction.Final).TimeTrigger) +
              fullTime;
            tor = tor < 1 ? 1 : tor;
            Log.Debug("GNLCZT| " + tor);
            Log.Debug("GNLCZT| END");
            return tor;
        }
    }
}
