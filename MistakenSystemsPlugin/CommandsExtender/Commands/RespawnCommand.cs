using CommandSystem;
using Gamer.Utilities;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;
using System.Text;


namespace Gamer.Mistaken.CommandsExtender.Commands
{
        [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))] class RespawnCommand : IBetterCommand, IPermissionLocked
    {
        public string Permission => "respawn";

        public override string Description =>
            "Edits time to respawn";

        public string PluginName => PluginHandler.PluginName;

        public override string Command => "respawn";

        public override string[] Aliases => new string[] { };

        public string GetUsage()
        {
            return "respawn [time]";
        }

        public override string[] Execute(ICommandSender sender, string[] args, out bool _s)
        {
            _s = false;
            if (args.Length == 0 || !int.TryParse(args[0], out int value)) 
                return new string[] { GetUsage() };
            Respawning.RespawnManager.Singleton._timeForNextSequence = (float)Respawning.RespawnManager.Singleton._stopwatch.Elapsed.TotalSeconds + value;
            _s = true;
            return new string[] { "Done" };
        }
    }
}
