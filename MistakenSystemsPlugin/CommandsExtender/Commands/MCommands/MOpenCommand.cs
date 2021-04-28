


using CommandSystem;
using Gamer.Utilities;
using System.Collections.Generic;
using System.Linq;


namespace Gamer.Mistaken.CommandsExtender.Commands
{
        [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))] class MOpenCommand : IBetterCommand, IPermissionLocked
    {
        

        public string Permission => "m.open";

        public override string Description =>
        "MOpen";

        public string PluginName => PluginHandler.PluginName;

        public override string Command => "mopen";

        public override string[] Aliases => new string[] { };

        public string GetUsage()
        {
            return "MOpen";
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
