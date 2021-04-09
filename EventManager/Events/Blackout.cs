using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using MEC;
using UnityEngine;

namespace Gamer.EventManager.Events
{
    internal class Blackout : 
        EventCreator.IEMEventClass,
        EventCreator.InternalEvent
    {
        public override string Id => "blackout";

        public override string Description { get; set; } = "Blackout event";

        public override string Name { get; set; } = "Blackout";

        public override EventCreator.Version Version => new EventCreator.Version(4, 0, 0);

        public override Dictionary<string, string> Translations => new Dictionary<string, string>()
        {
        };

        public override void OnIni()
        {
            Exiled.Events.Handlers.Server.RoundEnded += Server_RoundEnded;
            Exiled.Events.Handlers.Player.ChangingRole += Player_ChangingRole;
            Exiled.Events.Handlers.Server.RoundStarted += Server_RoundStarted;
            Exiled.Events.Handlers.Map.GeneratorActivated += Map_GeneratorActivated;
            Exiled.Events.Handlers.Player.InsertingGeneratorTablet += Player_InsertingGeneratorTablet;
        }

        public override void OnDeIni()
        {
            Exiled.Events.Handlers.Server.RoundEnded += Server_RoundEnded;
            Exiled.Events.Handlers.Player.ChangingRole += Player_ChangingRole;
            Exiled.Events.Handlers.Server.RoundStarted += Server_RoundStarted;
            Exiled.Events.Handlers.Map.GeneratorActivated += Map_GeneratorActivated;
            Exiled.Events.Handlers.Player.InsertingGeneratorTablet += Player_InsertingGeneratorTablet;
        }

        public override void Register()
        {
        }

        private void Server_RoundStarted()
        {
            Cassie.Message("LIGHT SYSTEM ERRROR . LIGHTS OUT", false, true);
            Mistaken.Systems.Utilities.API.Map.Blackout.Enabled = true;
            Mistaken.Systems.Utilities.API.Map.Blackout.OnlyHCZ = false;
            Mistaken.Systems.Utilities.API.Map.Blackout.Delay = 10;
            Mistaken.Systems.Utilities.API.Map.Blackout.Length = 10;
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            Timing.CallDelayed(2, () =>
            {
                ev.Player.AddItem(ItemType.Flashlight);
            });
        }

        private void Player_InsertingGeneratorTablet(Exiled.Events.EventArgs.InsertingGeneratorTabletEventArgs ev)
        {
            ev.Generator.NetworkremainingPowerup = 180;
        }

        private void Map_GeneratorActivated(Exiled.Events.EventArgs.GeneratorActivatedEventArgs ev)
        {
            if (Map.ActivatedGenerators > 4)
            {
                Timing.CallDelayed(70, () => 
                {
                    Mistaken.Systems.Utilities.API.Map.Blackout.Enabled = false;
                });
            }
        }

        private void Server_RoundEnded(Exiled.Events.EventArgs.RoundEndedEventArgs ev)
        {
            DeInitiate();
        }
    }
}
