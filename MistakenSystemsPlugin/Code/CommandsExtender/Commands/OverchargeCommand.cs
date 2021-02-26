using CommandSystem;
using Gamer.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Gamer.Mistaken.CommandsExtender.Commands
{
        [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))] class OverchargeCommand : IBetterCommand, IPermissionLocked
    {
        public string Permission => "overcharge";

        public override string Description =>
        "Teminate 079";

        public string PluginName => PluginHandler.PluginName;

        public override string Command => "KILL079";

        public override string[] Aliases => new string[] { "k079" };

        public string GetUsage()
        {
            return "KILL079";
        }

        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            Recontainer079.BeginContainment(true);
            success = true;
            return new string[] { "Starting" };
        }
    }
}
