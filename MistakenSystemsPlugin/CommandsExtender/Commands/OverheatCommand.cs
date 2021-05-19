using CommandSystem;
using Gamer.Utilities;

namespace Gamer.Mistaken.CommandsExtender.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))]
    internal class OverheatCommand : IBetterCommand, IPermissionLocked
    {
        public string Permission => "overheat";

        public override string Description =>
         "OVERHEAT";

        public string PluginName => PluginHandler.PluginName;

        public override string Command => "overheat";

        public override string[] Aliases => new string[] { "oheat" };

        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            success = false;
            if (args.Length == 0 || !int.TryParse(args[0], out int proggressLevel))
                return new string[] { "Proggress level has to be an int" };
            Base.Utilities.API.Map.Overheat.OverheatLevel = proggressLevel;
            success = true;
            switch (proggressLevel)
            {
                case 0:
                    return new string[] { "Overheat in T-30m" };
                case 1:
                    return new string[] { "Overheat in T-25m" };
                case 2:
                    return new string[] { "Overheat in T-20m" };
                case 3:
                    return new string[] { "Overheat in T-15m" };
                case 4:
                    return new string[] { "Overheat in T-10m" };
                case 5:
                    return new string[] { "Overheat in T-05m" };
                case 6:
                    return new string[] { "Overheat in T-03m" };
                case 7:
                    return new string[] { "Overheat in T-90s" };
                case 8:
                case 9:
                case 10:
                case 11:
                case 12:
                case 13:
                case 14:
                case 15:
                    return new string[] { "You can't use that, use 7 (T-90s) or 16 (T-00s)" };
                case 16:
                    return new string[] { "Overheat in T-00s" };
                default:
                    return new string[] { "Out of range" };
            }
        }
    }
}
