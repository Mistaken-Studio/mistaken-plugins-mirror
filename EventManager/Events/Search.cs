using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Exiled.API.Features;
using MEC;
using Exiled.API.Extensions;
using Exiled.Events.EventArgs;
using System.ComponentModel.Design;
using Interactables.Interobjects.DoorUtils;
using Exiled.API.Enums;

namespace Gamer.EventManager.Events
{
    internal class Search : 
        EventCreator.IEMEventClass,
        EventCreator.InternalEvent,
        EventCreator.ISpawnRandomItems,
        EventCreator.IWinOnLastAlive
    {
        public override string Id => "search";

        public override string Description { get; set; } = "Search event";

        public override string Name { get; set; } = "Search";

        public override EventCreator.Version Version => new EventCreator.Version(4, 0, 0);

        public override Dictionary<string, string> Translations => new Dictionary<string, string>()
        {
            { "D", "Musisz znaleźć MicroHID'a w HCZ i uciec z nim" }
        };

        public override void OnIni()
        {
            Exiled.Events.Handlers.Player.Escaping += Player_Escaping;
            Exiled.Events.Handlers.Server.RoundStarted += Server_RoundStarted;
        }

        public override void OnDeIni()
        {
            Exiled.Events.Handlers.Player.Escaping -= Player_Escaping;
            Exiled.Events.Handlers.Server.RoundStarted -= Server_RoundStarted;
        }

        public override void Register()
        {
        }

        private void Server_RoundStarted()
        {
            #region ini
            LightContainmentZoneDecontamination.DecontaminationController.Singleton.disableDecontamination = true;
            Mistaken.Systems.Utilities.API.Map.RespawnLock = true;
            Round.IsLocked = true;
            var doors = Map.Doors;
            foreach (var door in doors)
            {
                var doorType = door.Type();
                if (door.name == "") door.NetworkTargetState = true;
                else if (doorType != DoorType.HID)
                {
                    door.NetworkTargetState = true;
                    door.ServerChangeLock(DoorLockReason.Warhead, true);
                }
                else
                {
                    door.NetworkTargetState = false;
                    door.ServerChangeLock(DoorLockReason.Warhead, true);
                }
            }


            foreach (var e in Map.Lifts)
            {
                var elevatorType = e.Type();
                if (elevatorType == ElevatorType.LczA || elevatorType == ElevatorType.LczB) e.Network_locked = true;
            }
            #endregion
            Map.Broadcast(10, EventManager.EMLB + Translations["D"]);
            foreach (var player in Gamer.Utilities.RealPlayers.List)
                player.SlowChangeRole(RoleType.ClassD, Exiled.API.Extensions.Role.GetRandomSpawnPoint(RoleType.NtfCommander));
            int rand = UnityEngine.Random.Range(0, 7);
            switch (rand)
            {
                case 0:
                    {
                        ItemType.MicroHID.Spawn(float.MaxValue, doors.First(d => d.Type() == DoorType.Scp079First).transform.position);
                        break;
                    }
                case 1:
                    {
                        ItemType.MicroHID.Spawn(float.MaxValue, doors.First(d => d.Type() == DoorType.Scp079Second).transform.position);
                        break;
                    }
                case 2:
                    {
                        ItemType.MicroHID.Spawn(float.MaxValue, doors.First(d => d.Type() == DoorType.Scp049Armory).transform.position);
                        break;
                    }
                case 3:
                    {
                        ItemType.MicroHID.Spawn(float.MaxValue, doors.First(d => d.Type() == DoorType.Scp096).transform.position);
                        break;
                    }
                case 4:
                    {
                        ItemType.MicroHID.Spawn(float.MaxValue, doors.First(d => d.Type() == DoorType.Scp106Primary).transform.position);
                        break;
                    }
                case 5:
                    {
                        ItemType.MicroHID.Spawn(float.MaxValue, doors.First(d => d.Type() == DoorType.Scp106Secondary).transform.position);
                        break;
                    }
                case 6:
                    {
                        ItemType.MicroHID.Spawn(float.MaxValue, doors.First(d => d.Type() == DoorType.NukeArmory).transform.position);
                        break;
                    }
            }

            Turnofflights();
        }

        private void Player_Escaping(EscapingEventArgs ev)
        {
            Log.Debug(ev.Player.Id);
            foreach (var item in ev.Player.Inventory.items)
            {
                Log.Debug(item.id);
            }
            Log.Debug(ev.Player.Inventory.items.Any(i => i.id == ItemType.MicroHID).ToString());
            if (ev.Player.Inventory.items.Any(i => i.id == ItemType.MicroHID))
                OnEnd(ev.Player.Nickname);
            else
                ev.IsAllowed = false;
        }

        private void Turnofflights()
        {
            if (!Active)
                return;
            Map.TurnOffAllLights(30, true);
            Timing.CallDelayed(60, () =>
            {
                Turnofflights();
            });
        }
    }
}
