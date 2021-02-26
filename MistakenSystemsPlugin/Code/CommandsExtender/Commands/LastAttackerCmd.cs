using CommandSystem;
using Exiled.API.Features;
using Gamer.Utilities;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Gamer.Mistaken.CommandsExtender.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))] 
    class LastAttackerCmd : IBetterCommand, IPermissionLocked
    {
        public string Permission => "last_attacker";

        public override string Description => "Get Last Attacker";
        public string PluginName => PluginHandler.PluginName;

        public override string Command => "lastattacker";

        public override string[] Aliases => new string[] { "la" };

        public override string[] Execute(ICommandSender sender, string[] args, out bool _s)
        {
            _s = false;
            if (args.Length == 0) return new string[] { GetUsage() };
            var output = this.ForeachPlayer(args[0], out bool success, (player) => {
                CommandsHandler.LastAttackers.TryGetValue(player.UserId, out (Player, Player) info);
                string[] tor = new string[3];
                tor[0] = $"Data for {player.UserId}";
                tor[1] = $"Attacker: {info.Item1?.ToString(true) ?? "Not found"}";
                tor[2] = $"Killer: {info.Item2?.ToString(true) ?? "Not found"}";
                return tor;
            });
            if (!success) return new string[] { "Player not found", GetUsage() };
            _s = true;
            return output;
        }

        public string GetUsage()
        {
            return "LA (Id)";
        }
    }
}
