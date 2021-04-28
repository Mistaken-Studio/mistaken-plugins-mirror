using CommandSystem;
using Exiled.API.Features;
using Gamer.Mistaken.Systems.Misc;
using Gamer.Utilities;

namespace Gamer.Mistaken.CommandsExtender.Commands
{
    //[CommandSystem.CommandHandler(typeof(CommandSystem.ClientCommandHandler))]
    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))]
    internal class ChangeSCPCommand : IBetterCommand
    {
        public override string Command => "changescp";

        public override string[] Aliases => new string[] { };

        public override string Description => "Change SCP Player";

        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            Player player = sender.GetPlayer();
            RespawnPlayerHandler.RespawnSCP(player);
            success = true;
            return new string[] { "Done" };
        }

        public string GetUsage()
        {
            return "CHANGESCP";
        }
    }
}
