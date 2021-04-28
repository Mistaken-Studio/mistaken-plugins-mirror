using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Interactables.Interobjects.DoorUtils;
using MEC;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gamer.EventManager.Events
{
    internal class DBoiBattleRoyalMicroHid :
        EventCreator.IEMEventClass,
        EventCreator.IInternalEvent,
        EventCreator.IAnnouncPlayersAlive,
        EventCreator.IWinOnLastAlive
    {
        public override string Id => "dbbrmhid";

        public override string Description { get; set; } = "D Boi Battle Royal Micro Hid";

        public override string Name { get; set; } = "DBoiBattleRoyalMicroHid";

        public override Dictionary<string, string> Translations => new Dictionary<string, string>()
        {
            { "D_KILL", "You have been decontaminated" }
        };

        public bool ClearPrevious => true;
        private int Decontaminated = 0;

        public override void OnIni()
        {
            Exiled.Events.Handlers.Server.RoundStarted += Server_RoundStarted;
        }

        public override void OnDeIni()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= Server_RoundStarted;
        }

        public override void Register()
        {
        }

        private void Server_RoundStarted()
        {
            #region ini
            Pickup.Instances.ForEach(x => x.Delete());
            Decontaminated = 0;
            LightContainmentZoneDecontamination.DecontaminationController.Singleton.disableDecontamination = true;
            Mistaken.Systems.Utilities.API.Map.RespawnLock = true;
            Round.IsLocked = true;
            Mistaken.Systems.Utilities.API.Map.TeslaMode = Mistaken.Systems.Utilities.API.TeslaMode.DISABLED_FOR_ALL;
            #endregion
            foreach (var e in Map.Lifts)
            {
                if (!e.elevatorName.StartsWith("El"))
                    e.Network_locked = true;
            }
            foreach (var door in Map.Doors)
            {
                var doorType = door.Type();
                if (doorType == DoorType.CheckpointEntrance || doorType == DoorType.CheckpointLczA || doorType == DoorType.CheckpointLczB)
                {
                    door.ServerChangeLock(DoorLockReason.DecontEvacuate, true);
                    door.NetworkTargetState = true;
                }
                else if (doorType != DoorType.Scp914)
                    door.NetworkTargetState = true;
            }
            var rooms = Map.Rooms.Where(room => room.Type != RoomType.EzShelter && room.Type != RoomType.HczTesla && room.Type != RoomType.HczNuke && room.Type != RoomType.Surface && room.Type != RoomType.Hcz049 && room.Type != RoomType.Pocket && room.Type != RoomType.Hcz106 && room.Type != RoomType.HczHid && room.Type != RoomType.Lcz914 && room.Type != RoomType.Lcz173).ToList();
            foreach (var player in Gamer.Utilities.RealPlayers.List)
            {
                int random = UnityEngine.Random.Range(0, rooms.Count());
                var room = rooms[random];
                player.SlowChangeRole(RoleType.ClassD, room.Position + Vector3.up * 2);
            }
            Decontaminate();
            GiveHid();
        }

        public void Decontaminate()
        {
            if (!Active)
                return;
            if (Decontaminated < 2)
            {
                var rand = UnityEngine.Random.Range(0, 3);
                if (rand == 0)
                {
                    if (!Round.IsStarted)
                        return;
                    DecontaminateLCZ();
                }
                else if (rand == 1)
                {
                    if (!Round.IsStarted)
                        return;
                    DecontaminateHCZ();
                }
                else if (rand == 2)
                {
                    if (!Round.IsStarted)
                        return;
                    DecontaminateEZ();
                }
                Decontaminated++;
                WaitAndExecute(300, () =>
                {
                    if (!Round.IsStarted)
                        return;
                    Decontaminate();
                });
            }
            else
            {
                var rand = UnityEngine.Random.Range(0, 3);
                if (rand == 0)
                {
                    if (!Round.IsStarted)
                        return;
                    DecontaminateEZ_LCZ();
                }
                else if (rand == 1)
                {
                    if (!Round.IsStarted)
                        return;
                    DecontaminateEZ_HCZ();
                }
                else if (rand == 2)
                {
                    if (!Round.IsStarted)
                        return;
                    DecontaminateLCZ_HCZ();
                }
                WaitAndExecute(300, () =>
                {
                    if (!Round.IsStarted)
                        return;
                    DecontaminateFacility();
                });
            }
        }

        public void DecontaminateLCZ()
        {
            if (!Active)
                return;
            WaitAndExecute(224, () =>
            {
                if (!Active)
                    return;
                Cassie.Message("Pitch_0.9 Light Containment Zone Decontamination in t minus 1 minute", false, true);
                WaitAndExecute(38, () =>
                {
                    if (!Active)
                        return;
                    Cassie.Message("Pitch_0.9 Light Containment Zone Decontamination in t minus 30 seconds PITCH_1.1 . . . . . . Evacuate IMMEDIATELY . 20 . 19 . 18 . 17 . 16 . 15 . 14 . 13 . 12 . 11 . 10 seconds . 9 . 8 . 7 . 6 . 5 . 4 . 3 . 2 . 1 . . . Pitch_0.9 Light Containment Zone is under lockdown . All alive in it are now terminated", false, true);
                    WaitAndExecute(38, () =>
                    {
                        if (!Active)
                            return;
                        var elevators = Map.Lifts;
                        foreach (var elevator in elevators)
                            elevator.Network_locked = true;
                        foreach (var player in Player.Get(RoleType.ClassD))
                        {
                            if (player.CurrentRoom?.Zone == ZoneType.LightContainment)
                            {
                                player.Kill(DamageTypes.Contain);
                                player.Broadcast(10, EventManager.EMLB + Translations["D_KILL"]);
                            }
                        }

                        WaitAndExecute(60, () =>
                        {
                            foreach (var e in elevators)
                            {
                                if (e.elevatorName.StartsWith("El"))
                                    e.Network_locked = false;
                            }
                        });
                    });
                });
            });
        }

        public void DecontaminateHCZ()
        {
            if (!Active)
                return;
            WaitAndExecute(224, () =>
            {
                if (!Active)
                    return;
                Cassie.Message("Pitch_0.9 Heavy Containment Zone Decontamination in t minus 1 minute", false, true);
                WaitAndExecute(38, () =>
                {
                    if (!Active)
                        return;
                    Cassie.Message("Pitch_0.9 Heavy Containment Zone Decontamination in t minus 30 seconds PITCH_1.1 . . . . . . Evacuate IMMEDIATELY . 20 . 19 . 18 . 17 . 16 . 15 . 14 . 13 . 12 . 11 . 10 seconds . 9 . 8 . 7 . 6 . 5 . 4 . 3 . 2 . 1 . . . Pitch_0.9 Heavy Containment Zone is under lockdown . All alive in it are now terminated", false, true);
                    WaitAndExecute(38, () =>
                    {
                        if (!Active)
                            return;
                        var elevators = Map.Lifts;
                        var doors = Map.Doors;
                        foreach (var elevator in elevators)
                            elevator.Network_locked = true;
                        foreach (var door in doors)
                        {
                            if (door.Type() == DoorType.CheckpointEntrance)
                            {
                                door.NetworkTargetState = false;
                                door.ServerChangeLock(DoorLockReason.DecontEvacuate, false);
                                door.ServerChangeLock(DoorLockReason.DecontLockdown, true);
                            }
                        }
                        foreach (var player in Player.Get(RoleType.ClassD))
                        {
                            if (player.CurrentRoom?.Zone == ZoneType.HeavyContainment)
                            {
                                player.Kill(DamageTypes.Contain);
                                player.Broadcast(10, EventManager.EMLB + Translations["D_KILL"]);
                            }
                        }
                        WaitAndExecute(60, () =>
                        {
                            foreach (var e in elevators)
                            {
                                if (e.elevatorName.StartsWith("El"))
                                    e.Network_locked = false;
                            }
                            foreach (var door in doors)
                            {
                                if (door.Type() == DoorType.CheckpointEntrance)
                                {
                                    door.NetworkTargetState = true;
                                    door.ServerChangeLock(DoorLockReason.DecontEvacuate, true);
                                    door.ServerChangeLock(DoorLockReason.DecontLockdown, false);
                                }
                            }
                        });
                    });
                });
            });
        }

        public void DecontaminateEZ()
        {
            if (!Active)
                return;
            WaitAndExecute(224, () =>
            {
                if (!Active)
                    return;
                Cassie.Message("Pitch_0.9 Entrance Zone Decontamination in t minus 1 minute", false, true);
                WaitAndExecute(38, () =>
                {
                    if (!Active)
                        return;
                    Cassie.Message("Pitch_0.9 Entrance Zone Decontamination in t minus 30 seconds PITCH_1.1 . . . . . . Evacuate IMMEDIATELY . 20 . 19 . 18 . 17 . 16 . 15 . 14 . 13 . 12 . 11 . 10 seconds . 9 . 8 . 7 . 6 . 5 . 4 . 3 . 2 . 1 . . . Pitch_0.9 Entrance Zone is under lockdown . All alive in it are now terminated", false, true);
                    WaitAndExecute(38, () =>
                    {
                        if (!Active)
                            return;
                        var doors = Map.Doors;
                        foreach (var door in doors)
                        {
                            if (door.Type() == DoorType.CheckpointEntrance)
                            {
                                door.NetworkTargetState = false;
                                door.ServerChangeLock(DoorLockReason.DecontEvacuate, false);
                                door.ServerChangeLock(DoorLockReason.DecontLockdown, true);
                            }
                        }
                        foreach (var player in Player.Get(RoleType.ClassD))
                        {
                            if (player.CurrentRoom?.Zone == ZoneType.Entrance)
                            {
                                player.Kill(DamageTypes.Contain);
                                player.Broadcast(10, EventManager.EMLB + Translations["D_KILL"]);
                            }
                        }
                        WaitAndExecute(60, () =>
                        {
                            foreach (var door in doors)
                            {
                                if (door.Type() == DoorType.CheckpointEntrance)
                                {
                                    door.NetworkTargetState = true;
                                    door.ServerChangeLock(DoorLockReason.DecontEvacuate, true);
                                    door.ServerChangeLock(DoorLockReason.DecontLockdown, false);
                                }
                            }
                        });
                    });
                });
            });
        }

        public void DecontaminateEZ_HCZ()
        {
            if (!Active)
                return;
            WaitAndExecute(224, () =>
            {
                if (!Active)
                    return;
                Cassie.Message("Pitch_0.9 HEAVY Containment Zone AND ENTRANCE ZONE Decontamination in t minus 1 minute", false, true);
                WaitAndExecute(38, () =>
                {
                    if (!Active)
                        return;
                    Cassie.Message("Pitch_0.9 HEAVY Containment Zone AND ENTRANCE ZONE Decontamination in t minus 30 seconds PITCH_1.1 . . . . . . Evacuate IMMEDIATELY . 20 . 19 . 18 . 17 . 16 . 15 . 14 . 13 . 12 . 11 . 10 seconds . 9 . 8 . 7 . 6 . 5 . 4 . 3 . 2 . 1 . . . Pitch_0.9 HEAVY Containment Zone AND ENTRANCE ZONE are under lockdown . All alive in them are now terminated", false, true);
                    WaitAndExecute(40, () =>
                    {
                        if (!Active)
                            return;
                        foreach (var elevator in Map.Lifts)
                            elevator.Network_locked = true;
                        foreach (var player in Player.Get(RoleType.ClassD))
                        {
                            if (player.CurrentRoom?.Zone == ZoneType.HeavyContainment || player.CurrentRoom?.Zone == ZoneType.Entrance)
                            {
                                player.Kill(DamageTypes.Contain);
                                player.Broadcast(10, EventManager.EMLB + Translations["D_KILL"]);
                            }
                        }
                    });
                });
            });
        }

        public void DecontaminateEZ_LCZ()
        {
            if (!Active)
                return;
            WaitAndExecute(224, () =>
            {
                if (!Active)
                    return;
                Cassie.Message("Pitch_0.9 Light Containment Zone AND ENTRANCE ZONE Decontamination in t minus 1 minute", false, true);
                WaitAndExecute(38, () =>
                {
                    if (!Active)
                        return;
                    Cassie.Message("Pitch_0.9 Light Containment Zone AND ENTRANCE ZONE Decontamination in t minus 30 seconds PITCH_1.1 . . . . . . Evacuate IMMEDIATELY . 20 . 19 . 18 . 17 . 16 . 15 . 14 . 13 . 12 . 11 . 10 seconds . 9 . 8 . 7 . 6 . 5 . 4 . 3 . 2 . 1 . . . Pitch_0.9 Light Containment Zone AND ENTRANCE ZONE are under lockdown . All alive in them are now terminated", false, true);
                    WaitAndExecute(40, () =>
                    {
                        if (!Active)
                            return;
                        foreach (var elevator in Map.Lifts)
                            elevator.Network_locked = true;
                        foreach (var door in Map.Doors)
                        {
                            if (door.Type() == DoorType.CheckpointEntrance)
                            {
                                door.NetworkTargetState = false;
                                door.ServerChangeLock(DoorLockReason.DecontEvacuate, false);
                                door.ServerChangeLock(DoorLockReason.DecontLockdown, true);
                            }
                        }
                        foreach (var player in Player.Get(RoleType.ClassD))
                        {
                            if (player.CurrentRoom?.Zone == ZoneType.LightContainment || player.CurrentRoom?.Zone == ZoneType.Entrance)
                            {
                                player.Kill(DamageTypes.Contain);
                                player.Broadcast(10, EventManager.EMLB + Translations["D_KILL"]);
                            }
                        }
                    });
                });
            });
        }

        public void DecontaminateLCZ_HCZ()
        {
            if (!Active)
                return;
            WaitAndExecute(224, () =>
            {
                if (!Active)
                    return;
                Cassie.Message("Pitch_0.9 Light Containment Zone AND Heavy Containment ZONE Decontamination in t minus 1 minute", false, true);
                WaitAndExecute(38, () =>
                {
                    if (!Active)
                        return;
                    Cassie.Message("Pitch_0.9 Light Containment Zone AND Heavy Containment ZONE Decontamination in t minus 30 seconds PITCH_1.1 . . . . . . Evacuate IMMEDIATELY . 20 . 19 . 18 . 17 . 16 . 15 . 14 . 13 . 12 . 11 . 10 seconds . 9 . 8 . 7 . 6 . 5 . 4 . 3 . 2 . 1 . . . Pitch_0.9 Light Containment Zone AND Heavy Containment ZONE are under lockdown . All alive in them are now terminated", false, true);
                    WaitAndExecute(40, () =>
                    {
                        if (!Active)
                            return;
                        foreach (var door in Map.Doors)
                        {
                            if (door.Type() == DoorType.CheckpointEntrance)
                            {
                                door.NetworkTargetState = false;
                                door.ServerChangeLock(DoorLockReason.DecontEvacuate, false);
                                door.ServerChangeLock(DoorLockReason.DecontLockdown, true);
                            }
                        }
                        foreach (var player in Player.Get(RoleType.ClassD))
                        {
                            if (player.CurrentRoom?.Zone == ZoneType.LightContainment || player.CurrentRoom?.Zone == ZoneType.HeavyContainment)
                            {
                                player.Kill(DamageTypes.Contain);
                                player.Broadcast(10, EventManager.EMLB + Translations["D_KILL"]);
                            }
                        }
                    });
                });
            });
        }

        public void DecontaminateFacility()
        {
            if (!Active)
                return;
            WaitAndExecute(202, () =>
            {
                if (!Active)
                    return;
                Cassie.Message("Pitch_0.9 Initiating nato_a warhead in 3 . 2 . 1 . ", false, true);
                WaitAndExecute(8, () =>
                {
                    if (!Active)
                        return;
                    Warhead.Start();
                    Warhead.IsLocked = true;
                });
            });
        }

        private void GiveHid()
        {
            if (!Active)
                return;
            foreach (Player p in Gamer.Utilities.RealPlayers.List)
                p.AddItem(ItemType.MicroHID);
            Timing.CallDelayed(3, () =>
            {
                GiveHid();
            });
        }
    }
}
