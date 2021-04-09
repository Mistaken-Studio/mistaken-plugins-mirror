using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Interactables.Interobjects.DoorUtils;

namespace Gamer.EventManager.Events
{
    internal class TryNotToBlink : 
        EventCreator.IEMEventClass,
        EventCreator.InternalEvent
    {
        public override string Id => "trynottoblink";

        public override string Description { get; set; } = "The name explains it all :)";

        public override string Name { get; set; } = "TryNotToBlink";

        public override EventCreator.Version Version => new EventCreator.Version(4, 0, 0);

        public override Dictionary<string, string> Translations => new Dictionary<string, string>()
        {
            //{ "", "" }
        };

        public override void OnIni()
        {
            Exiled.Events.Handlers.Server.RoundStarted += Server_RoundStarted;
            Exiled.Events.Handlers.Player.ChangingRole += Player_ChangingRole;
            Exiled.Events.Handlers.Player.Died += Player_Died;
        }

        public override void OnDeIni()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= Server_RoundStarted;
            Exiled.Events.Handlers.Player.ChangingRole -= Player_ChangingRole;
            Exiled.Events.Handlers.Player.Died -= Player_Died;
        }

        public override void Register()
        {
        }

        private void Server_RoundStarted()
        {
            #region ini
            Pickup.Instances.ForEach(x => x.Delete());
            Mistaken.Systems.Utilities.API.Map.RespawnLock = true;
            Round.IsLocked = true;
            foreach (var door in Map.Doors.Where(d => d.Type() == DoorType.CheckpointLczA && d.Type() == DoorType.CheckpointLczB))
            {
                var d = door as Interactables.Interobjects.CheckpointDoor;
                d.ServerChangeLock(DoorLockReason.DecontLockdown, true);
                d.NetworkTargetState = true;
            }
            #endregion
            var players = Gamer.Utilities.RealPlayers.List;
            foreach (var player in players)
            {
                if (player.Side != Side.Scp) player.SlowChangeRole(RoleType.ClassD);
                else player.SlowChangeRole(RoleType.Scp173);
            }
                
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            if (ev.Player.Role == RoleType.ClassD) ev.Player.SlowChangeRole(RoleType.Scp173);
        }

        private void Player_Died(Exiled.Events.EventArgs.DiedEventArgs ev)
        {
            EndOnOneAliveOf();
        }
    }
}
