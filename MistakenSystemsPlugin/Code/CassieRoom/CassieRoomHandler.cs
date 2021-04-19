using System.Collections.Generic;
using UnityEngine;
using MEC;
using System;
using System.Linq;
using Gamer.Mistaken.BetterRP.Ambients;
using Exiled.API.Features;
using Exiled.API.Extensions;
using Mirror;
using Gamer.Utilities;
using Gamer.Diagnostics;

namespace Gamer.Mistaken.CassieRoom
{
    public class CassieRoomHandler : Module
    {
        public CassieRoomHandler(PluginHandler plugin) : base(plugin)
        {
        }

        public override string Name => "CassieRoom";
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.WaitingForPlayers += this.Handle(() => Server_WaitingForPlayers(), "WaitingForPlayers");
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.WaitingForPlayers -= this.Handle(() => Server_WaitingForPlayers(), "WaitingForPlayers");
        }
       
        private void Server_WaitingForPlayers()
        {
            
        }
    }
}
