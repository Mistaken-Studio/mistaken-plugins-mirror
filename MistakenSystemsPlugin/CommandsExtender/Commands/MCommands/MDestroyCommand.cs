


using CommandSystem;
using Gamer.Utilities;
using System.Collections.Generic;


namespace Gamer.Mistaken.CommandsExtender.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))]
    internal class MDestroyCommand : IBetterCommand, IPermissionLocked
    {

        public string Permission => "m.destroy";

        public override string Description =>
        "MDestroy";

        public override string Command => "mdestroy";

        public override string[] Aliases => new string[] { };

        public string PluginName => PluginHandler.PluginName;

        public string GetUsage()
        {
            return "MDestroy";
        }
        public static HashSet<int> Active = new HashSet<int>();
        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            success = true;
            var player = sender.GetPlayer();
            if (!Active.Contains(player.Id))
                Active.Add(player.Id);
            else
                Active.Remove(player.Id);
            if (Active.Contains(player.Id))
                return new string[] { "Activated" };
            else
                return new string[] { "Deactivated" };
        }
    }
}
