using CommandSystem;
using Gamer.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace Gamer.Mistaken.CommandsExtender.Commands
{
        [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))] class CEHelpCommand : IBetterCommand, IPermissionLocked
    {
        public string Permission => "ceh";
        public override string Description =>
            "Help Command for CommandsExtender";

        public string PluginName => PluginHandler.PluginName;

        public override string Command => "cehelp";

        public override string[] Aliases => new string[] { "ceh" };

        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            success = false;
            bool showOnlyDisallowed = false;
            if (args.Length > 0 && args[0] == "-h")
                showOnlyDisallowed = true;
            if (RemoteAdmin.CommandProcessor.RemoteAdminCommandHandler?.AllCommands == null)
                return new string[] { "SOMETHING GONE WRONG :|" };
            var tor = NorthwoodLib.Pools.ListPool<string>.Shared.Rent();
            var player = sender.GetPlayer();
            foreach (var item in RemoteAdmin.CommandProcessor.RemoteAdminCommandHandler?.AllCommands)
            {
                bool hasAccess = !(item is IPermissionLocked permission) || player.CheckPermission($"{permission.PluginName}.{permission.Permission}");
                if ((!showOnlyDisallowed && hasAccess) || (showOnlyDisallowed && !hasAccess))
                {
                    string message = item?.Command ?? "ERROR, UNKNOWN COMMAND";
                    if (item.Aliases?.Length > 0) message += " | " + string.Join(" | ", item.Aliases);
                    tor.Add(message);
                    tor.Add($" - {item.Description}");
                    if (item is IPermissionLocked)
                    {
                        var plocked = item as IPermissionLocked;
                        tor.Add($" - {plocked.PluginName}.{plocked.Permission}");
                    }
                }
            }

            success = true;
            var torArray = tor.ToArray();
            NorthwoodLib.Pools.ListPool<string>.Shared.Return(tor);
            return torArray;
        }

        public string GetUsage()
        {
            return "COMMANDSEXTENDERHELP";
        }
    }
}
