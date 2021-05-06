using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Gamer.Diagnostics;
using Gamer.RoundLoggerSystem;
using Gamer.Utilities;
using System.Linq;
using UnityEngine;

namespace Gamer.Mistaken.Systems.End
{
    internal class CamperEscapeHandler : Module
    {
        public CamperEscapeHandler(PluginHandler p) : base(p)
        {
        }

        public override string Name => "CamperEscape";
        public override void OnEnable()
        {
            this.CallDelayed(0.1f, () =>
            {
                Exiled.Events.Handlers.Server.RoundStarted += this.Handle(() => Server_RoundStarted(), "RoundStart");
                Exiled.Events.Handlers.Player.ChangingRole += this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
            }, "LateEnable");
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Player.ChangingRole -= this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            bool hasFacilityManager = ev.Items.Any(i => i == ItemType.KeycardFacilityManager);
            int weapons = 0;
            foreach (var item in ev.Items)
            {
                if (item.IsWeapon(false))
                {
                    if (weapons > 2)
                        ev.Items.Remove(item);
                    else
                        weapons++;
                }
                else if (hasFacilityManager)
                {
                    if (item == ItemType.KeycardChaosInsurgency || item == ItemType.KeycardNTFCommander || item == ItemType.KeycardNTFLieutenant)
                    {
                        ev.Items.Remove(item);
                        ev.Items.Remove(ItemType.KeycardFacilityManager);
                        ev.Items.Add(ItemType.KeycardO5);
                        hasFacilityManager = false;
                    }
                }
            }
        }

        private void Server_RoundStarted()
        {
            Vector3 Center012 = default;
            foreach (var door in Map.Doors)
            {
                if (door.Type() == DoorType.Scp012Bottom)
                    Center012 = door.transform.position + (door.transform.right * 1.5f) + (door.transform.forward * 8.5f);
            }

            //SCP012
            Components.Killer.Spawn(
                Center012 + new Vector3(0.25f, -0, 0),
                new Vector3(1.4f, 4, 1),
                5,
                "<color=red><b>Warning!</b></color><br>If you stay here you will recive <b>damage</b>",
                false,
                (p) => p.Team == Team.SCP
            );
            //SCP106
            Components.Killer.Spawn(
                Map.Rooms.First(r => r.Type == RoomType.Hcz106).GetByRoomOffset(new Vector3(15f, -10, -7.5f)),
                new Vector3(33, 1, 33),
                0,
                "<color=red><b>Warning!</b></color>\nYou <b>died</b>",
                true,
                (p) => p.Team == Team.SCP
            );
            //Escape
            Components.Killer.Spawn(
                new Vector3(179.5f, 990, 32.5f),
                new Vector3(13, 20, 19),
                5,
                "<color=red><b>Warning!</b></color><br>If you stay here you will recive <b>damage</b>",
                false,
                (p) => p.Team == Team.CDP || p.Team == Team.RSC || p.IsCuffed
            );
            Components.Killer.Spawn(
                new Vector3(174.5f, 990, 37),
                new Vector3(21, 20, 10),
                5,
                "<color=red><b>Warning!</b></color><br>If you stay here you will recive <b>damage</b>",
                false,
                (p) => p.Team == Team.CDP || p.Team == Team.RSC || p.IsCuffed
            );
            //Escape
            Components.Escape.Spawn(
                new Vector3(179.5f, 990, 32.5f),
                new Vector3(13, 20, 19),
                (p) =>
                {
                    if (!p.IsCuffed)
                        return;
                    switch (p.Team)
                    {
                        case Team.MTF:
                            Respawning.RespawnTickets.Singleton.GrantTickets(Respawning.SpawnableTeamType.ChaosInsurgency, 2);
                            p.ReferenceHub.characterClassManager.SetPlayersClass(RoleType.ChaosInsurgency, p.GameObject, false, true);
                            break;
                        case Team.CHI:
                            Respawning.RespawnTickets.Singleton.GrantTickets(Respawning.SpawnableTeamType.NineTailedFox, 2);
                            p.ReferenceHub.characterClassManager.SetPlayersClass(RoleType.NtfLieutenant, p.GameObject, false, true);
                            break;
                        default:
                            return;
                    }
                    RoundLogger.Log("ESCAPE", "ESCAPE", $"{p.PlayerToString()} has escaped");
                }
            );
            Components.Escape.Spawn(
                new Vector3(174.5f, 990, 37),
                new Vector3(21, 20, 10),
                (p) =>
                {
                    if (!p.IsCuffed)
                        return;
                    switch (p.Team)
                    {
                        case Team.MTF:
                            Respawning.RespawnTickets.Singleton.GrantTickets(Respawning.SpawnableTeamType.ChaosInsurgency, 2);
                            p.ReferenceHub.characterClassManager.SetPlayersClass(RoleType.ChaosInsurgency, p.GameObject, false, true);
                            break;
                        case Team.CHI:
                            Respawning.RespawnTickets.Singleton.GrantTickets(Respawning.SpawnableTeamType.NineTailedFox, 2);
                            p.ReferenceHub.characterClassManager.SetPlayersClass(RoleType.NtfLieutenant, p.GameObject, false, true);
                            break;
                        default:
                            return;
                    }
                    RoundLogger.Log("ESCAPE", "ESCAPE", $"{p.PlayerToString()} has escaped");
                }
            );
        }
    }
}
