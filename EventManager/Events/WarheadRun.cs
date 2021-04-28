using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Interactables.Interobjects.DoorUtils;
using System.Collections.Generic;
using System.Linq;

namespace Gamer.EventManager.Events
{
    internal class WarheadRun :
        EventCreator.IEMEventClass,
        EventCreator.IEndOnNoAlive,
        EventCreator.IWinOnEscape,
        EventCreator.InternalEvent
    {
        public override string Id => "whr";

        public override string Description { get; set; } = "WarheadRun";

        public override string Name { get; set; } = "WarheadRun";

        public override EventCreator.Version Version => new EventCreator.Version(4, 0, 0);

        public override Dictionary<string, string> Translations => new Dictionary<string, string>()
        {
            { "D", "Jesteś klasą D. Musisz uciec z placówki. Pierwszy gracz który ucieknie wygrywa" }
        };

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
            Mistaken.Systems.Utilities.API.Map.TeslaMode = Mistaken.Systems.Utilities.API.TeslaMode.DISABLED_FOR_ALL;
            LightContainmentZoneDecontamination.DecontaminationController.Singleton.disableDecontamination = true;
            Mistaken.Systems.Utilities.API.Map.RespawnLock = true;
            Round.IsLocked = true;
            Map.Lifts.First(e => e.Type() == ElevatorType.Nuke).Network_locked = true;
            foreach (var door in Map.Doors)
            {
                door.NetworkTargetState = true;
                var type = door.Type();
                if (type == DoorType.CheckpointEntrance || type == DoorType.CheckpointLczA || type == DoorType.CheckpointLczB)
                    door.ServerChangeLock(DoorLockReason.DecontEvacuate, true);
                else if (door.name != "")
                    door.ServerChangeLock(DoorLockReason.AdminCommand, true);
            }
            #endregion

            foreach (var player in Gamer.Utilities.RealPlayers.List)
                player.SlowChangeRole(RoleType.ClassD);
            Map.Broadcast(10, EventManager.EMLB + Translations["D"]);
            Cassie.Message("nato_a warhead will be initiated in t minus 1 minute", false, true);
            WaitAndExecute(68, () =>
            {
                if (!Active)
                    return;
                Warhead.Start();
                Warhead.IsLocked = true;
                foreach (var p in Player.Get(RoleType.ClassD))
                {
                    p.AddItem(ItemType.SCP207);
                    p.AddItem(ItemType.SCP500);
                }
            });
        }
    }
}
