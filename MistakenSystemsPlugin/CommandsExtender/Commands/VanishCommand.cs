using CommandSystem;
using Gamer.Mistaken.Systems.End;
using Gamer.Utilities;
using System.Linq;

namespace Gamer.Mistaken.CommandsExtender.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))]
    public class VanisCommand : IBetterCommand, IPermissionLocked
    {
        public string Permission => "vanish";

        public string PluginName => PluginHandler.PluginName;

        public override string Command => "vanish";

        public override string[] Aliases => new string[] { "v" };

        public override string Description => "Vanish";

        public string GetUsage()
        {
            return "VANISH true/false";
        }


        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            success = true;
            var player = sender.GetPlayer();
            bool value = !VanishHandler.Vanished.ContainsKey(player.Id);
            if (args.Any(str => str.ToLower() == "true"))
                value = true;
            else if (args.Any(str => str.ToLower() == "false"))
                value = false;
            if (args.Any(str => str == "-l3"))
            {
                VanishHandler.SetGhost(player, value, 3);
                return new string[] { $"GhostMode Status: {value} | Level: 3" };
            }
            else if (args.Any(str => str == "-l2"))
            {
                VanishHandler.SetGhost(player, value, 2);
                return new string[] { $"GhostMode Status: {value} | Level: 2" };
            }
            else if (args.Any(str => str == "-l1"))
            {
                VanishHandler.SetGhost(player, value, 1);
                return new string[] { $"GhostMode Status: {value} | Level: 1" };
            }
            else
            {
                VanishHandler.SetGhost(player, value);
                return new string[] { $"GhostMode Status: {value} | Level: 1" };
            }
        }
    }
}
