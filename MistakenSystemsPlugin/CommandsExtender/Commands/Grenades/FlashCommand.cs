﻿//

using CommandSystem;
using Gamer.Utilities;
using System.Linq;

namespace Gamer.Mistaken.CommandsExtender.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))]
    internal class FlashCmd : IBetterCommand, IPermissionLocked
    {
        public string Permission => "flash";

        public override string Description =>
        "Flashes";

        public string PluginName => PluginHandler.PluginName;

        public override string Command => "flash";

        public override string[] Aliases => new string[] { };

        public string GetUsage()
        {
            return "FLASH [PLAYER ID/'ALL'] (AMOUNT)";
        }

        public void DropUnder(int[] pids, int times)
        {
            foreach (var item in pids)
            {
                RealPlayers.Get(item).DropGrenadeUnder(1, times);
            }
        }

        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            success = false;
            if (args.Length == 0)
                return new string[] { GetUsage() };
            else
            {
                int amount = 1;
                if (args.Length > 1)
                {
                    if (!int.TryParse(args[1], out amount))
                        return new string[] { GetUsage() };
                }
                var pids = GetPlayers(args[0]).Select(p => p.Id).ToArray();
                if (pids.Length == 0)
                    return new string[] { "Player not found", GetUsage() };
                DropUnder(pids, amount);
                success = true;
                return new string[] { "Done" };
            }
        }
    }
}
