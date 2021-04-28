using CommandSystem;
using Gamer.Utilities;


namespace Gamer.Mistaken.CommandsExtender.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))]
    internal class DropAllCommand : IBetterCommand, IPermissionLocked
    {
        public string Permission => "dropall";

        public override string Description =>
       "DROPS EVERYTHING FROM INVENTORY";

        public string PluginName => PluginHandler.PluginName;

        public override string Command => "dropall";

        public override string[] Aliases => new string[] { "dall" };

        public string GetUsage()
        {
            return "DROPALL [Id]";
        }

        public override string[] Execute(ICommandSender sender, string[] args, out bool _s)
        {
            _s = false;
            if (args.Length == 0) return new string[] { GetUsage() };
            var output = ForeachPlayer(args[0], out bool success, (player) =>
            {
                player.ReferenceHub.inventory.ServerDropAll();

                return new string[] { "Done" };
            });
            if (!success) return new string[] { "Player not found", GetUsage() };
            _s = true;
            return output;
        }
    }
}
