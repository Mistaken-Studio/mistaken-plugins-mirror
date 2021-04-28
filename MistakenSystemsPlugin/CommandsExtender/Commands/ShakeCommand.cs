


using CommandSystem;
using Exiled.API.Features;
using Gamer.Utilities;


namespace Gamer.Mistaken.CommandsExtender.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))]
    internal class ShakeCommand : IBetterCommand, IPermissionLocked
    {


        public string Permission => "shake";

        public override string Description =>
        "SHAKE";

        public string PluginName => PluginHandler.PluginName;

        public override string Command => "shake";

        public override string[] Aliases => new string[] { };

        public string GetUsage()
        {
            return "SHAKE";
        }

        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            success = true;
            Warhead.Shake();
            return new string[] { "Done" };
        }
    }
}
