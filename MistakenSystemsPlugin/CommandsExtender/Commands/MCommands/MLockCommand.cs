﻿


using CommandSystem;
using Gamer.Utilities;
using System.Collections.Generic;


namespace Gamer.Mistaken.CommandsExtender.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))]
    internal class MLockCommand : IBetterCommand, IPermissionLocked
    {


        public string Permission => "m.lock";

        public override string Description =>
         "MLock";

        public string PluginName => PluginHandler.PluginName;

        public override string Command => "mlock";

        public override string[] Aliases => new string[] { };

        public string GetUsage()
        {
            return "MLock";
        }
        public static HashSet<int> Active = new HashSet<int>();

        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            success = true;
            var player = sender.GetPlayer();
            if (!Active.Contains(player.Id))
                Active.Add(player.Id);
            else
                Active.Remove(player.Id);
            if (Active.Contains(player.Id))
                return new string[] { "Activated" };
            else
                return new string[] { "Deactivated" };
        }
    }
}
