

using System.Linq;
using UnityEngine;

using Gamer.Utilities;
using CommandSystem;
using Exiled.API.Features;

namespace Gamer.Mistaken.CommandsExtender.Commands
{
        [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))] class RespawnLockCommand : IBetterCommand
    {
       
        public override string Description =>
      "RESPAWNLOCK";

        public override string Command => "respawnlock";

        public override string[] Aliases => new string[] { "disresp" };

        public string GetUsage()
        {
            return "RESPAWNLOCK true/false";
        }

        public static bool RespawnLock { get; set; } = false;

        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            success = false;
            if (args.Length == 0) return new string[] { GetUsage() };
            if (bool.TryParse(args[0], out bool value))
            {
                RespawnLock = value;
                success = true;
                return new string[] { "RespawnLock:" + value };
            }
            else
            {
                return new string[] { GetUsage() };
            }
        }
    }
}
