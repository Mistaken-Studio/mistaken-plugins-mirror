using CommandSystem;
using Gamer.Utilities;

namespace Xname.ColorfullEZ
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))]
    internal class EZColorsCommand : IBetterCommand, IPermissionLocked
    {
        public string Permission => "colors";

        public override string Description =>
            "Changes Entrance Zone color";

        public string PluginName => "colorfull_ez";

        public override string Command => "ezcolors";

        public override string[] Aliases => new string[] { };

        public string GetUsage()
        {
            return "ezcolors [Card/Empty for random]";
        }

        public override string[] Execute(ICommandSender sender, string[] args, out bool _s)
        {
            _s = true;
            if (args.Length == 0)
            {
                ColorfullEZHandler.Generate(ColorfullEZHandler.GetKeycard());
                return new string[] { "Done" };
            }
            switch (args[0])
            {
                case "none":
                    ColorfullEZHandler.Clear();
                    break;
                case "true":
                    ColorfullEZHandler.SyncFor(sender.GetPlayer());
                    break;
                case "false":
                    ColorfullEZHandler.DesyncFor(sender.GetPlayer());
                    break;
                default:
                    ColorfullEZHandler.Generate((ItemType)byte.Parse(args[0]));
                    break;
            }
            return new string[] { "Done" };
        }
    }
}
