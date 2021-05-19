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
    internal class CTF :
        EventCreator.IEMEventClass,
        EventCreator.IInternalEvent
    {
        public override string Id => "ctf";

        public override string Description { get; set; } = "Capture The Flag";

        public override string Name { get; set; } = "CTF";

        public override Dictionary<string, string> Translations => new Dictionary<string, string>()
        {
            { "TaskMTF", "Jesteś członkiem <color=blue>MFO</color>. Waszym zadaniem jest przejęcie flagi drużyny przeciwnej (<color=green>CI</color>). Bazy odznaczają się obecnością <color=yellow>latarki</color> w pomieszczeniu." },
            { "FlagMTF", "Gracz <color=green>$player</color> przejął flagę drużyny <color=blue>MFO</color>" },
            { "TaskCI", "Jesteś członkiem <color=green>CI</color>. Waszym zadaniem jest przejęcie flagi drużyny przeciwnej (<color=blue>MFO</color>). Bazy odznaczają się obecnością <color=yellow>latarki</color> w pomieszczeniu." },
            { "FlagCI", "Gracz <color=blue>$player</color> przejął flagę drużyny <color=green>CI</color>" }
        };

        private Room ciRoom = null;
        private Room mtfRoom = null;

        public override void OnIni()
        {
            Exiled.Events.Handlers.Server.RoundStarted += Server_RoundStarted;
            Exiled.Events.Handlers.Player.PickingUpItem += Player_PickingUpItem;
            Exiled.Events.Handlers.Player.Died += Player_Died;
            Exiled.Events.Handlers.Player.ItemDropped += Player_ItemDropped;
        }

        private void Player_ItemDropped(Exiled.Events.EventArgs.ItemDroppedEventArgs ev)
        {
            if (ev.Pickup.ItemId == ItemType.Flashlight && ev.Pickup.durability == 1)
            {
                Map.ClearBroadcasts();
                Map.Broadcast(4, Translations["FlagMTF"].Replace("$player", ev.Player.Nickname));
                if (ev.Player.CurrentRoom == ciRoom) OnEnd("<color=green>CI</color> wygrywa!", true);
            }
            else if (ev.Pickup.ItemId == ItemType.Flashlight && ev.Pickup.durability == 2)
            {
                Map.ClearBroadcasts();
                Map.Broadcast(4, Translations["FlagCI"].Replace("$player", ev.Player.Nickname));
                if (ev.Player.CurrentRoom == mtfRoom) OnEnd("<color=blue>MFO</color> wygrywa!", true);
            }
        }

        public override void OnDeIni()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= Server_RoundStarted;
            Exiled.Events.Handlers.Player.PickingUpItem -= Player_PickingUpItem;
            Exiled.Events.Handlers.Player.Died -= Player_Died;
            Exiled.Events.Handlers.Player.ItemDropped -= Player_ItemDropped;
        }

        public override void Register()
        {
        }

        private void Server_RoundStarted()
        {
            #region ini
            Mistaken.Base.Utilities.API.Map.RespawnLock = true;
            Round.IsLocked = true;
            foreach (var e in Map.Lifts)
                if (e.elevatorName.StartsWith("El") || e.elevatorName.StartsWith("SCP") || e.elevatorName == "") e.Network_locked = true;
            foreach (var door in Map.Doors)
                if (door.Type() == DoorType.CheckpointEntrance)
                {
                    door.ServerChangeLock(DoorLockReason.DecontEvacuate, true);
                    door.NetworkTargetState = true;
                }
            int i = 0;
            var players = Gamer.Utilities.RealPlayers.List.ToList();
            var rooms = Map.Rooms.ToList();
            var randomroom = rooms.Where(r => r.Type == RoomType.Hcz079 || r.Type == RoomType.Hcz106 || r.Type == RoomType.HczChkpA || r.Type == RoomType.HczChkpB).ToList();
            mtfRoom = randomroom[UnityEngine.Random.Range(0, randomroom.Count)];
            #endregion
            foreach (var room in rooms)
                if (Vector3.Distance(room.Position, mtfRoom.Position) >= 90 && randomroom.Contains(room) && ciRoom == null)
                    ciRoom = room;
            if (ciRoom == null) ciRoom = rooms.First(r => Vector3.Distance(r.Position, mtfRoom.Position) >= 70 && r.Zone == ZoneType.HeavyContainment);
            ItemType.Flashlight.Spawn(1, mtfRoom.Position);
            ItemType.Flashlight.Spawn(2, ciRoom.Position);
            foreach (var player in players)
            {
                player.SessionVariables["NO_SPAWN_PROTECT"] = true;
                if (i % 2 != 0)
                {
                    player.SlowChangeRole(RoleType.NtfLieutenant, mtfRoom.Position + Vector3.up * 2);
                    player.Broadcast(10, Translations["TaskMTF"]);
                }
                else
                {
                    player.SlowChangeRole(RoleType.ChaosInsurgency, ciRoom.Position + Vector3.up * 2);
                    player.Broadcast(10, Translations["TaskCI"]);
                }
                i++;
            }
            Winner();
        }

        private void Player_Died(Exiled.Events.EventArgs.DiedEventArgs ev)
        {
            var role = ev.Target.Role;
            ev.Target.Broadcast(5, "Za chwilę się odrodzisz!");
            Timing.CallDelayed(5f, () =>
            {
                ev.Target.SlowChangeRole(role, (role == RoleType.ChaosInsurgency ? ciRoom.Position : mtfRoom.Position) + Vector3.up);
            });
        }

        private void Player_PickingUpItem(Exiled.Events.EventArgs.PickingUpItemEventArgs ev)
        {
            var player = ev.Player;
            /*
            if (ev.Pickup.ItemId == ItemType.Flashlight && ev.Pickup.durability == 1 && player.Role == RoleType.NtfLieutenant) { ev.IsAllowed = false; return; }
            else if (ev.Pickup.ItemId == ItemType.Flashlight && ev.Pickup.durability == 2 && player.Role == RoleType.ChaosInsurgency) { ev.IsAllowed = false; return; }
            if (ev.Pickup.ItemId == ItemType.Flashlight && ev.Pickup.durability == 1) Map.Broadcast(5, Translations["FlagMTF"].Replace("$player", player.Nickname));
            else if (ev.Pickup.ItemId == ItemType.Flashlight && ev.Pickup.durability == 2) Map.Broadcast(5, Translations["FlagCI"].Replace("$player", player.Nickname)); ;
            */
            if (ev.Pickup.ItemId != ItemType.Flashlight)
                return;
            switch (ev.Pickup.durability)
            {
                case 1:
                    if (player.Role == RoleType.NtfLieutenant)
                    {
                        ev.IsAllowed = false;
                        return;
                    }
                    Map.Broadcast(5, Translations["FlagMTF"].Replace("$player", player.Nickname));
                    break;
                case 2:
                    if (player.Role == RoleType.ChaosInsurgency)
                    {
                        ev.IsAllowed = false;
                        return;
                    }
                    Map.Broadcast(5, Translations["FlagCI"].Replace("$player", player.Nickname));
                    break;
            }
        }

        private void Winner()
        {
            if (!Active) return;
            foreach (var player in Gamer.Utilities.RealPlayers.List)
            {
                foreach (var item in player.Inventory.items)
                {
                    if (item.id == ItemType.Flashlight)
                    {
                        if (item.durability == 1)
                        {
                            Map.ClearBroadcasts();
                            Map.Broadcast(4, Translations["FlagMTF"].Replace("$player", player.Nickname));
                            if (player.CurrentRoom == ciRoom) OnEnd("<color=green>CI</color> wygrywa!", true);
                        }
                        else if (item.durability == 2)
                        {
                            Map.ClearBroadcasts();
                            Map.Broadcast(4, Translations["FlagCI"].Replace("$player", player.Nickname));
                            if (player.CurrentRoom == mtfRoom) OnEnd("<color=blue>MFO</color> wygrywa!", true);
                        }
                    }
                }
            }
            Timing.CallDelayed(1, () => { Winner(); });
        }
    }
}
