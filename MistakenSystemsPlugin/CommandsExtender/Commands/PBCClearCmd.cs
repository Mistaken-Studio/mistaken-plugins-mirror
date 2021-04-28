using CommandSystem;
using Gamer.Utilities;


namespace Gamer.Mistaken.CommandsExtender.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))]
    internal class PBCClearCmd : IBetterCommand, IPermissionLocked
    {
        public string Permission => "pbcc";

        public override string Description =>
         "PBCCLEAR";

        public string PluginName => PluginHandler.PluginName;

        public override string Command => "pbcclear";

        public override string[] Aliases => new string[] { "pbcc" };

        public string GetUsage()
        {
            return "PBCCLEAR [Id]";
        }

        public override string[] Execute(ICommandSender sender, string[] args, out bool _s)
        {
            _s = false;
            if (args.Length == 0) return new string[] { GetUsage() };
            var output = ForeachPlayer(args[0], out bool success, (player) =>
            {
                player.ClearBroadcasts();
                return new string[] { "Done" };
            });
            if (!success)
                return new string[] { "Player not found", GetUsage() };
            _s = true;
            return output;
        }
    }
}
