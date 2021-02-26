﻿using CommandSystem;
using Gamer.Utilities;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;
using System.Text;

namespace Gamer.Mistaken.CommandsExtender.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))] 
    class FakeNickCommand : IBetterCommand, IPermissionLocked
    {
        public string Permission => "fakenick";

        public override string Description =>
            "Fakenick";

        public string PluginName => PluginHandler.PluginName;

        public override string Command => "fakenick";

        public override string[] Aliases => new string[] { "fname", "fnick" };

        public string GetUsage()
        {
            return "fakenick [Id] [nick]";
        }

        public override string[] Execute(ICommandSender sender, string[] args, out bool _s)
        {
            _s = false;
            if (args.Length < 2) return new string[] { GetUsage() };
            var players = this.GetPlayers(args[0]);
            if(players.Count > 1)
                return new string[] { "<b><size=200%>1 PLAYER</size></b>" };
            if (players.Count == 0)
                return new string[] { "Player not found" };
            players[0].DisplayNickname = string.Join(" ", args.Skip(1));
            _s = true;
            return new string[] { "Done" };
        }
    }
}
