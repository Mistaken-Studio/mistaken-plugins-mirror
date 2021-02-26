using Mirror;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Gamer.Utilities;
using CommandSystem;
using Exiled.API.Features;

namespace Gamer.Mistaken.Systems.Logs.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))] 
    class FlashLogCommand : IBetterCommand
    {
        public override string Command => "flashlog";

        public override string[] Aliases => new string[] { "flog" };

        public string GetUsage()
        {
            return "FlashLog [Player Id]";
        }

        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            success = false;
            if (args.Length == 0)
                return new string[] { GetUsage() };
            if (!int.TryParse(args[0], out int id))
                return new string[] { "Id has to be int" };
            if (!LogManager.FlashLog.TryGetValue(id, out Player player))
                return new string[] { "Flashlog not found" };
            return new string[] { $"Player: ({player.Id}) {player.Nickname}", $"UserId: {player.UserId}" };
        }
    }
}
