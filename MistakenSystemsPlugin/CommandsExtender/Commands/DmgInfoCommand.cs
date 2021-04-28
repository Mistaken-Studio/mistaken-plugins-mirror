using CommandSystem;
using Gamer.Utilities;
using System.Collections.Generic;


namespace Gamer.Mistaken.CommandsExtender.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))]
    internal class DmgInfoCommand : IBetterCommand, IPermissionLocked
    {


        public string Permission => "dmginfo";

        public override string Description =>
            "Damage Info";

        public override string Command => "dmginfo";

        public override string[] Aliases => new string[] { };

        public string PluginName => PluginHandler.PluginName;

        public string GetUsage()
        {
            return "DmgInfo (true/false)";
        }
        public static HashSet<int> Active = new HashSet<int>();

        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            success = false;
            var player = sender.GetPlayer();
            bool value = false;
            if (args.Length == 0)
            {
                value = !Active.Contains(player.Id);
            }
            else if (bool.TryParse(args[0], out value))
            {
            }
            else return new string[] { GetUsage() };
            success = true;
            if (value)
            {
                if (!Active.Contains(player.Id)) Active.Add(player.Id);
                return new string[] { "Enabled" };
            }
            else
            {
                Active.Remove(player.Id);
                return new string[] { "Disabled" };
            }
        }
    }
}
