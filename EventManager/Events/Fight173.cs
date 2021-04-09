using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using Exiled.API.Enums;
using UnityEngine;
using System.Reflection;
using MEC;

namespace Gamer.EventManager.Events
{
    internal class Fight173 :
        EventCreator.IEMEventClass,
        EventCreator.InternalEvent
    {
        public override string Id => "f173";

        public override string Description { get; set; } = "Fight between SCP 173 and Class D";

        public override string Name { get; set; } = "Fight173";

        public override EventCreator.Version Version => new EventCreator.Version(4, 0, 0);

        public override Dictionary<string, string> Translations => new Dictionary<string, string>()
        {
            //{ "", "" }
        };

        public override void OnIni()
        {
            Exiled.Events.Handlers.Server.RoundStarted += Server_RoundStarted;
            Exiled.Events.Handlers.Player.Died += Player_Died;
        }

        public override void OnDeIni()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= Server_RoundStarted;
            Exiled.Events.Handlers.Player.Died -= Player_Died;
        }

        public override void Register()
        {
        }

        private void Server_RoundStarted()
        {
            #region ini
            Mistaken.Systems.Utilities.API.Map.RespawnLock = true;
            Round.IsLocked = true;
            #endregion
            foreach (var player in Gamer.Utilities.RealPlayers.List)
            {
                if (player.Team != Team.SCP) player.SlowChangeRole(RoleType.ClassD, Exiled.API.Extensions.Role.GetRandomSpawnPoint(RoleType.Scp106));
                else player.SlowChangeRole(RoleType.Scp173, Exiled.API.Extensions.Role.GetRandomSpawnPoint(RoleType.Scp106));
            }
        }

        private void Player_Died(Exiled.Events.EventArgs.DiedEventArgs ev)
        {
            EndOnOneAliveOf();
        }
    }
}

