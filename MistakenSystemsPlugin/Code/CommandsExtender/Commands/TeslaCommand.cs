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
        [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))] class TeslaCommand : IBetterCommand, IPermissionLocked
    {
        

        public string Permission => "tesla";

        public override string Description =>
        
            "Manipulate Facility Tesla System";

        public string PluginName => PluginHandler.PluginName;

        public override string Command => "tesla";

        public override string[] Aliases => new string[] { };

        public string GetUsage()
        {
            return "TESLA ENABLE/DISABLE/RESTART";
        }

        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            success = false;
            if (args.Length == 0) return new string[] { GetUsage() };
            success = true;
            switch (args[0].ToLower())
            {
                case "enable":
                    {
                        Map.TeslaMode = TeslaMode.ENABLED;
                        return new string[] { "Tesla gates Enabled" };
                    }
                case "disable":
                    {
                        Map.TeslaMode = TeslaMode.DISABLED;
                        return new string[] { "Tesla gates Disabled for players" };
                    }

                case "disableall":
                    {
                        Map.TeslaMode = TeslaMode.DISABLED_FOR_ALL;
                        return new string[] { "Tesla gates Disabled for all" };
                    }
                case "disable079":
                    {
                        Map.TeslaMode = TeslaMode.DISABLED_FOR_079;
                        return new string[] { "Tesla gates Disabled for 079" };
                    }
                case "restart":
                    {
                        Map.RestartTeslaGates(true);
                        return new string[] { "Tesla gates Restarted" };
                    }
                default:
                    success = false;
                    return new string[] { GetUsage() };
            }
        }
    }
}
