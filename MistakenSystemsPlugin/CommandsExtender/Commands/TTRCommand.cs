

using CommandSystem;
using Gamer.Utilities;
using Respawning;
using System.Collections.Generic;
using UnityEngine;

namespace Gamer.Mistaken.CommandsExtender.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))] 
    class TTRCommand : IBetterCommand, IPermissionLocked
    {
        public string Permission => "ttr";

        public override string Description =>
        
             "Returns Time To Respawn";

        public string PluginName => PluginHandler.PluginName;

        public override string Command => "ttr";

        public override string[] Aliases => new string[] { };

        public string GetUsage()
        {
            return "TTR";
        }

        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            success = true;

            var respawnManager = Respawning.RespawnManager.Singleton;
            var ttr = Mathf.RoundToInt(RespawnManager.Singleton._timeForNextSequence - (float)RespawnManager.Singleton._stopwatch.Elapsed.TotalSeconds);
            return new string[] { $"TTR: {((ttr-(ttr % 60))/60):00}m {(ttr%60):00}s | {respawnManager.NextKnownTeam}" };
        }
    }
}
