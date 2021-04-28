using CommandSystem;
using Gamer.Utilities;

namespace Gamer.Mistaken.CommandsExtender.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))]
    internal class TestCommand : IBetterCommand, IPermissionLocked
    {
        public override string Command => "test";

        public string Permission => "locked";

        public string PluginName => PluginHandler.PluginName;

        public override string[] Execute(ICommandSender sender, string[] args, out bool _s)
        {
            _s = true;
            return new string[] { "Done" };
        }
    }
}
