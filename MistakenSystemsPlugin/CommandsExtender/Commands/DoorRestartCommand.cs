using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;
using System.Text;
using UnityEngine;
using Gamer.Utilities;
using CommandSystem;
using Gamer.Mistaken.Systems.Utilities.API;

namespace Gamer.Mistaken.CommandsExtender.Commands
{
        [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))] class DoorRestartCommand : IBetterCommand, IPermissionLocked
        { 
        public string Permission => "doorrestart";

        public override string Description =>
        "Restart Facility Door System";

        public string PluginName => PluginHandler.PluginName;

        public override string Command => "doorrestart";

        public override string[] Aliases => new string[] { "drestart" };

        public string GetUsage()
        {
            return "DoorRestart";
        }

        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            Map.RestartDoors();
            success = true;
            return new string[] { "Done" };
        }
    }
}
