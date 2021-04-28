using CommandSystem;
using Gamer.Mistaken.Systems.Utilities.API;
using Gamer.Utilities;

namespace Gamer.Mistaken.CommandsExtender.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))]
    class DoorRestartCommand : IBetterCommand, IPermissionLocked
    {
        public string Permission => "doorrestart";

        public override string Description =>
        "Restart Facility Door System";

        public string PluginName => PluginHandler.PluginName;

        public override string Command => "doorrestart";

        public override string[] Aliases => new string[] { "drestart" };

        public string GetUsage()
        {
            return "DoorRestart";
        }

        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            Map.RestartDoors();
            success = true;
            return new string[] { "Done" };
        }
    }
}
