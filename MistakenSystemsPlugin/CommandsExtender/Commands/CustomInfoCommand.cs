using CommandSystem;
using Gamer.Utilities;
using System.Linq;

namespace Gamer.Mistaken.CommandsExtender.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))]
    internal class CustomInfoCommand : IBetterCommand, IPermissionLocked
    {
        public string Permission => "custominfo";

        public string PluginName => PluginHandler.PluginName;

        public override string Command => "custominfo";

        public override string[] Aliases => new string[] { "cinfo" };

        public string GetUsage()
        {
            return "custominfo [playerId] [message]";
        }

        public override string[] Execute(ICommandSender sender, string[] args, out bool _s)
        {
            _s = false;
            if (args.Length == 0) return new string[] { GetUsage() };

            var result = ForeachPlayer(args[0], out bool success, p =>
            {
                p.ReferenceHub.nicknameSync.Network_customPlayerInfoString = string.Join(" ", args.Skip(1));
                if (p.ReferenceHub.nicknameSync.Network_customPlayerInfoString == "")
                    p.ReferenceHub.nicknameSync.Network_customPlayerInfoString = null;
                return new string[] { "Done" };
            });
            if (!success)
                return new string[] { "Player not found" };
            _s = true;
            return result;
        }
    }
}
