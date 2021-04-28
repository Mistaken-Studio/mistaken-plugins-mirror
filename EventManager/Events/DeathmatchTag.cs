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
    internal class DeathmatchTag :
        EventCreator.IEMEventClass,
        EventCreator.InternalEvent
    {
        public override string Id => "dmt";

        public override string Description { get; set; } = "Blank event";

        public override string Name { get; set; } = "DeathmatchTag";

        public override EventCreator.Version Version => new EventCreator.Version(4, 0, 0);

        public override Dictionary<string, string> Translations => new Dictionary<string, string>()
        {
            { "CI", "Twoim zadanie jest zabicie członków <color=blue>MFO</color>, znajdziesz ich w <color=yellow>Heavy Containment Zone</color>" },
            { "MTF", "Twoim zadanie jest zabicie członków <color=green>CI</color>, znajdziesz ich w <color=yellow>Entrance Zone</color>" }
        };

        public override void OnIni()
        {
            Exiled.Events.Handlers.Server.RoundStarted += Server_RoundStarted;
            Exiled.Events.Handlers.Player.Died += Player_Died;
            Exiled.Events.Handlers.Player.ChangingRole += Player_ChangingRole;
        }

        public override void OnDeIni()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= Server_RoundStarted;
            Exiled.Events.Handlers.Player.Died -= Player_Died;
            Exiled.Events.Handlers.Player.ChangingRole -= Player_ChangingRole;
        }

        public override void Register()
        {
        }

        private void Server_RoundStarted()
        {
            #region ini
            Mistaken.Systems.Utilities.API.Map.RespawnLock = true;
            Round.IsLocked = true;
            Pickup.Instances.ForEach(x => x.Delete());
            var doors = Map.Doors;
            foreach (var door in doors)
            {
                var doorType = door.Type();
                if (doorType == DoorType.GateA || doorType == DoorType.GateB)
                    door.ServerChangeLock(DoorLockReason.AdminCommand, true);
            }
            foreach (var e in Map.Lifts)
            {
                var elevatorType = e.Type();
                if (elevatorType == ElevatorType.LczA || elevatorType == ElevatorType.LczB)
                    e.Network_locked = true;
            }
            #endregion
            var players = Gamer.Utilities.RealPlayers.List;
            int i = 0;
            foreach (var player in players)
            {
                if (i % 2 != 0)
                {
                    switch (UnityEngine.Random.Range(0, 3))
                    {
                        case 0:
                            player.SlowChangeRole(RoleType.NtfLieutenant, Exiled.API.Extensions.Role.GetRandomSpawnPoint(RoleType.Scp93953));
                            player.Broadcast(10, Translations["MTF"]);
                            break;
                        case 1:
                            player.SlowChangeRole(RoleType.NtfLieutenant, Exiled.API.Extensions.Role.GetRandomSpawnPoint(RoleType.Scp096));
                            player.Broadcast(10, Translations["MTF"]);
                            break;
                        case 2:
                            player.SlowChangeRole(RoleType.NtfLieutenant, doors.First(d => d.Type() == DoorType.HczArmory).transform.position + Vector3.up * 2);
                            player.Broadcast(10, Translations["MTF"]);
                            break;
                    }
                }
                else
                {
                    player.SlowChangeRole(RoleType.ChaosInsurgency, Exiled.API.Extensions.Role.GetRandomSpawnPoint(RoleType.FacilityGuard));
                    player.Broadcast(10, Translations["CI"]);
                }
                i++;
            }
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            if (ev.Player.Role == RoleType.NtfLieutenant)
                ev.Player.SlowChangeRole(RoleType.ChaosInsurgency, Exiled.API.Extensions.Role.GetRandomSpawnPoint(RoleType.FacilityGuard));
            else if (ev.Player.Role == RoleType.ChaosInsurgency)
            {
                switch (UnityEngine.Random.Range(0, 3))
                {
                    case 0:
                        ev.Player.SlowChangeRole(RoleType.NtfLieutenant, Exiled.API.Extensions.Role.GetRandomSpawnPoint(RoleType.Scp93953));
                        break;
                    case 1:
                        ev.Player.SlowChangeRole(RoleType.NtfLieutenant, Exiled.API.Extensions.Role.GetRandomSpawnPoint(RoleType.Scp096));
                        break;
                    case 2:
                        ev.Player.SlowChangeRole(RoleType.NtfLieutenant, Map.Doors.First(d => d.Type() == DoorType.HczArmory).transform.position + Vector3.up * 2);
                        break;
                }
            }
        }

        private void Player_Died(Exiled.Events.EventArgs.DiedEventArgs ev)
        {
            var players = Gamer.Utilities.RealPlayers.List;
            if (players.Count(x => x.Role == RoleType.ChaosInsurgency && x.Id != ev.Target.Id) == 0)
                OnEnd("<color=blue>MFO</color> wygrywa!", true);
            else if (players.Count(x => x.Role == RoleType.NtfLieutenant && x.Id != ev.Target.Id) == 0)
                OnEnd("<color=green>CI</color> wygrywa!", true);

            var role = ev.Killer.Role;
            ev.Target.Broadcast(5, "Za chwilę się odrodzisz!");
            Timing.CallDelayed(5f, () =>
            {
                Vector3 respPoint;
                if (role == RoleType.ChaosInsurgency)
                    respPoint = Exiled.API.Extensions.Role.GetRandomSpawnPoint(RoleType.FacilityGuard);
                else
                {
                    switch (UnityEngine.Random.Range(0, 3))
                    {
                        case 0:
                            respPoint = Exiled.API.Extensions.Role.GetRandomSpawnPoint(RoleType.Scp93953);
                            break;
                        case 1:
                            respPoint = Exiled.API.Extensions.Role.GetRandomSpawnPoint(RoleType.Scp096);
                            break;
                        default:
                            respPoint = Map.Doors.First(d => d.Type() == DoorType.HczArmory).transform.position + Vector3.up * 2;
                            break;
                    }
                }
                ev.Target.SlowChangeRole(role, respPoint);
            });
        }
    }
}
