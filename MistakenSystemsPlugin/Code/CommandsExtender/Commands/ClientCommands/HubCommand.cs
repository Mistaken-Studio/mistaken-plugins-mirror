using CommandSystem;
using Exiled.API.Features;
using Gamer.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace Gamer.Mistaken.CommandsExtender.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.ClientCommandHandler))] 
    class HubCommand : IBetterCommand
    {       
        public override string Command => "hub";

        public string GetUsage()
        {
            return ".hub [serwer]";
        }

        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            success = false;
            if (args.Length == 0 || !byte.TryParse(args[0], out byte serverId) || !(serverId == 1 || serverId == 2 || serverId == 3 || serverId == 4 || serverId == 14 || serverId == 15)) return new string[] {
                "Złe argumenty",
                GetUsage(),
                "Serwer:",
                "1 - Ranked",
                "2 - RP",
                "3 - Casual",
                //"4 - RP 2"
            };
            var player = sender.GetPlayer();
            Server.Host.ReferenceHub.playerStats.CallTargetRpc(nameof(PlayerStats.RpcRoundrestartRedirect), player.Connection, 0.1f, 7776 + serverId);
            success = true;
            return new string[] { "Redirecting" };
        }
    }
}
