using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Gamer.Utilities;
using Interactables.Interobjects.DoorUtils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gamer.EventManager.Events
{
    internal class BlackDeath :
        EventCreator.IEMEventClass,
        EventCreator.InternalEvent
    {
        public override string Id => "bdeath";

        public override string Description { get; set; } = "BlackDeath event";

        public override string Name { get; set; } = "BlackDeath";

        public override Dictionary<string, string> Translations => new Dictionary<string, string>()
        {
            { "scp", "" },
            { "d", "" }
        };

        public override void OnIni()
        {
            Exiled.Events.Handlers.Map.GeneratorActivated += Map_GeneratorActivated;
            Exiled.Events.Handlers.Map.ExplodingGrenade += Map_ExplodingGrenade;
            Exiled.Events.Handlers.Server.RoundStarted += Server_RoundStarted;
            Exiled.Events.Handlers.Player.ChangingRole += Player_ChangingRole;
            Exiled.Events.Handlers.Player.EnteringPocketDimension += Player_EnteringPocketDimension;
            Exiled.Events.Handlers.Player.Died += Player_Died;
            Exiled.Events.Handlers.Player.InsertingGeneratorTablet += Player_InsertingGeneratorTablet;
        }

        public override void OnDeIni()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= Server_RoundStarted;
            Exiled.Events.Handlers.Player.ChangingRole -= Player_ChangingRole;
            Exiled.Events.Handlers.Player.EnteringPocketDimension -= Player_EnteringPocketDimension;
            Exiled.Events.Handlers.Player.Died -= Player_Died;
            Exiled.Events.Handlers.Map.ExplodingGrenade -= Map_ExplodingGrenade;
            Exiled.Events.Handlers.Map.GeneratorActivated -= Map_GeneratorActivated;
            Exiled.Events.Handlers.Player.InsertingGeneratorTablet -= Player_InsertingGeneratorTablet;
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
            #endregion
            foreach (var door in Map.Doors)
            {
                var doorType = door.Type();
                if (doorType == DoorType.HczArmory)
                {
                    door.ServerChangeLock(DoorLockReason.AdminCommand, true);
                    door.NetworkTargetState = true;
                }
                else if (door.name != "" && !(doorType == DoorType.HIDLeft || doorType == DoorType.HIDRight || doorType == DoorType.Scp106Primary || doorType == DoorType.Scp106Secondary || doorType == DoorType.Scp106Bottom))
                    door.ServerChangeLock(DoorLockReason.AdminCommand, true);
            }

            foreach (var elevator in Map.Lifts)
                elevator.Network_locked = true;

            var rooms = Map.Rooms.ToList();
            rooms.RemoveAll(r => r.Zone != ZoneType.HeavyContainment);

            for (int i = 0; i < 8; i++)
            {
                if (rooms.Count == 0)
                    break;
                var room = rooms[UnityEngine.Random.Range(0, rooms.Count)];
                ItemType.WeaponManagerTablet.Spawn(float.MaxValue, room.Position + Vector3.up * 2);
                rooms.Remove(room);
            }

            for (int i = 0; i < 5; i++)
            {
                if (rooms.Count == 0)
                    break;
                var room = rooms[UnityEngine.Random.Range(0, rooms.Count)];
                ItemType.KeycardNTFCommander.Spawn(float.MaxValue, room.Position + Vector3.up * 2);
                rooms.Remove(room);
            }

            for (int i = 0; i < 10; i++)
            {
                if (rooms.Count == 0)
                    break;
                var room = rooms[UnityEngine.Random.Range(0, rooms.Count)];
                ItemType.GrenadeFlash.Spawn(float.MaxValue, room.Position + Vector3.up * 2);
                rooms.Remove(room);
            }

            for (int i = 0; i < 15; i++)
            {
                if (rooms.Count == 0)
                    break;
                var room = rooms[UnityEngine.Random.Range(0, rooms.Count)];
                ItemType.Flashlight.Spawn(float.MaxValue, room.Position + Vector3.up * 2);
                rooms.Remove(room);
            }
            Map.TurnOffAllLights(float.MaxValue, true);
            var players = Gamer.Utilities.RealPlayers.List.ToList();
            var scp = players[UnityEngine.Random.Range(0, players.Count())];
            scp.SlowChangeRole(RoleType.Scp106);
            scp.Broadcast(10, EventManager.EMLB + Translations["scp"]);
            players.Remove(scp);
            foreach (var player in players)
            {
                player.SlowChangeRole(RoleType.ClassD);
                player.Broadcast(10, EventManager.EMLB + Translations["d"]);
            }
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            var players = Gamer.Utilities.RealPlayers.List.Where(p => p.Role != RoleType.Scp106 && p.Role != RoleType.Spectator && p.Id != ev.Player.Id).Count();
            if (ev.Player.Role == RoleType.ClassD)
                ev.Player.Position = Map.Doors.First(d => d.Type() == DoorType.HczArmory).transform.position + Vector3.up;
            else if (ev.Player.Role == RoleType.Spectator && players == 0)
                OnEnd("<color=red>SCP 106</color>");
        }

        private void Player_InsertingGeneratorTablet(Exiled.Events.EventArgs.InsertingGeneratorTabletEventArgs ev)
        {
            ev.Generator.NetworkremainingPowerup = 180;
        }

        private void Player_EnteringPocketDimension(Exiled.Events.EventArgs.EnteringPocketDimensionEventArgs ev)
        {
            ev.Player.Kill(DamageTypes.Scp106);
        }

        private void Player_Died(Exiled.Events.EventArgs.DiedEventArgs ev)
        {
            if (ev.Target.Role == RoleType.Scp106)
                OnEnd("<color=orange>Class D</color>");
        }

        private void Map_GeneratorActivated(Exiled.Events.EventArgs.GeneratorActivatedEventArgs ev)
        {
            if (Map.ActivatedGenerators > 4)
            {
                Cassie.Message("ALL GENERATORS HAS BEEN SUCCESSFULLY ENGAGED . SCP 1 0 6 RECONTAINMENT SEQUENCE COMMENCING IN T MINUS 1 MINUTE", false, true);
                WaitAndExecute(60, () =>
                {
                    Cassie.Message("SCP 1 0 6 RECONTAINMENT SEQUENCE COMMENCING IN 3 . 2 . 1 . ", false, true);
                    WaitAndExecute(8, () =>
                    {
                        var rh = ReferenceHub.GetHub(PlayerManager.localPlayer);
                        foreach (var player in RealPlayers.Get(RoleType.Scp106))
                            player.ReferenceHub.scp106PlayerScript.Contain(rh);
                        rh.playerInteract.RpcContain106(rh.gameObject); //Yes
                        WaitAndExecute(10, () =>
                        {
                            OnEnd("<color=orange>Class D</color>");
                        });
                    });
                });
            }
        }

        private void Map_ExplodingGrenade(Exiled.Events.EventArgs.ExplodingGrenadeEventArgs ev)
        {
            Log.Debug(ev.Grenade.name);
            if (ev.Grenade.name == "FLASHBANG")
            {
                foreach (var player in Gamer.Utilities.RealPlayers.List.Where(p => p.Role == RoleType.Scp106))
                {
                    if (Vector3.Distance(player.Position, ev.Grenade.transform.position) < 5)
                        player.Position = Exiled.API.Extensions.Role.GetRandomSpawnPoint(RoleType.Scp106);
                }
            }
        }
    }
}
