using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Interactables.Interobjects.DoorUtils;
using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Gamer.EventManager.Events
{
    internal class OpositeDay :
        EventCreator.IEMEventClass,
        EventCreator.InternalEvent
    {
        public override string Id => "oday";

        public override string Description { get; set; } = "OpositeDay event";

        public override string Name { get; set; } = "OpositeDay";

        public override Dictionary<string, string> Translations => new Dictionary<string, string>()
        {
            //{ "", "" }
        };

        public override void OnIni()
        {
            Exiled.Events.Handlers.Server.RoundStarted += Server_RoundStarted;
            Exiled.Events.Handlers.Player.ChangingRole += Player_ChangingRole;
            Exiled.Events.Handlers.Server.RoundEnded += Server_RoundEnded;
        }

        public override void OnDeIni()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= Server_RoundStarted;
            Exiled.Events.Handlers.Player.ChangingRole -= Player_ChangingRole;
            Exiled.Events.Handlers.Server.RoundEnded -= Server_RoundEnded;
        }

        public override void Register()
        {
        }

        private void Server_RoundStarted()
        {
            #region ini
            LightContainmentZoneDecontamination.DecontaminationController.Singleton.disableDecontamination = true;
            var doors = Map.Doors;
            foreach (var door in doors)
            {
                var doorType = door.Type();
                if (doorType == DoorType.GateA || doorType == DoorType.GateB)
                {
                    door.NetworkTargetState = true;
                    door.ServerChangeLock(DoorLockReason.AdminCommand, true);
                }
            }
            #endregion
            Timing.CallDelayed(1, () =>
            {
                foreach (Player player in Gamer.Utilities.RealPlayers.List)
                {
                    if (player.Role == RoleType.ClassD)
                        player.Position = Exiled.API.Extensions.Role.GetRandomSpawnPoint(RoleType.Scp049);
                    else if (player.Role == RoleType.Scientist)
                        player.Position = Exiled.API.Extensions.Role.GetRandomSpawnPoint(RoleType.Scp93989);
                    else if (player.Role == RoleType.FacilityGuard)
                    {
                        foreach (var door in doors)
                        {
                            if (door.Type() == DoorType.Scp079First)
                            {
                                player.Position = door.transform.position + Vector3.up * 2;
                            }
                        }
                    }
                    else if (player.Role == RoleType.Scp93953 || player.Role == RoleType.Scp93989)
                        player.Position = Exiled.API.Extensions.Role.GetRandomSpawnPoint(RoleType.Scientist);
                    else if (player.Role == RoleType.Scp173)
                        player.Position = Exiled.API.Extensions.Role.GetRandomSpawnPoint(RoleType.NtfCadet);
                    else if (player.Role == RoleType.Scp049)
                        player.Position = Exiled.API.Extensions.Role.GetRandomSpawnPoint(RoleType.ChaosInsurgency);
                    else if (player.Role == RoleType.Scp106)
                        player.Position = Exiled.API.Extensions.Role.GetRandomSpawnPoint(RoleType.NtfCommander);
                    else if (player.Role == RoleType.Scp096)
                        player.Position = Exiled.API.Extensions.Role.GetRandomSpawnPoint(RoleType.NtfLieutenant);
                }
            });
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            if (ev.Player.Role == RoleType.NtfCadet)
                ev.Player.Position = Exiled.API.Extensions.Role.GetRandomSpawnPoint(RoleType.Scp173);
            else if (ev.Player.Role == RoleType.NtfLieutenant)
                ev.Player.Position = Exiled.API.Extensions.Role.GetRandomSpawnPoint(RoleType.Scp096);
            else if (ev.Player.Role == RoleType.NtfCommander)
                ev.Player.Position = Exiled.API.Extensions.Role.GetRandomSpawnPoint(RoleType.Scp096);
            else if (ev.Player.Role == RoleType.ChaosInsurgency)
                ev.Player.Position = Exiled.API.Extensions.Role.GetRandomSpawnPoint(RoleType.Scp049);
            else if (ev.Player.Role == RoleType.NtfScientist)
                ev.Player.Position = Exiled.API.Extensions.Role.GetRandomSpawnPoint(RoleType.Scp93989);
            else if (ev.Player.Role == RoleType.Scp0492)
                ev.Player.Position = Exiled.API.Extensions.Role.GetRandomSpawnPoint(RoleType.ClassD);
        }

        private void Server_RoundEnded(Exiled.Events.EventArgs.RoundEndedEventArgs ev)
        {
            DeInitiate();
        }
    }
}
