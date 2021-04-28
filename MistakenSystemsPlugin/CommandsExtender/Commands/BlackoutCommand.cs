using CommandSystem;
using Gamer.Utilities;

namespace Gamer.Mistaken.CommandsExtender.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))]
    class BlackoutCommand : IBetterCommand, IPermissionLocked
    {
        public string Permission => "blackout";

        public string PluginName => PluginHandler.PluginName;

        public override string Command => "blackout";

        public override string[] Aliases => new string[] { };

        public override string Description => "BLACKOUT";

        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            success = false;
            if (args.Length == 0)
                return new string[] { GetUsage() };
            else
            {
                if (bool.TryParse(args[0], out bool value))
                {
                    Systems.Utilities.API.Map.Blackout.Enabled = value;
                    success = true;
                    if (value)
                        return new string[] { "Enabled" };
                    else
                        return new string[] { "Disabled" };
                }
                else
                {
                    return new string[] { GetUsage() };
                }
            }
        }

        public string GetUsage()
        {
            return "BLACKOUT true/false";
        }
    }
}
