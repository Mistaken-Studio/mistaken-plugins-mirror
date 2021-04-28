using CommandSystem;
using Gamer.Utilities;


namespace Gamer.Mistaken.CommandsExtender.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))]
    internal class Controll079Command : IBetterCommand, IPermissionLocked
    {
        public string Permission => "controll079";

        public override string Description =>
        "Edit SCP 079 data";

        public string PluginName => PluginHandler.PluginName;

        public override string Command => "controll079";

        public override string[] Aliases => new string[] { "c079" };

        public string GetUsage()
        {
            return "CONTROLL079 [Id] LVL/XP/AP/MAX_AP [VALUE]";
        }

        public override string[] Execute(ICommandSender sender, string[] args, out bool _s)
        {
            _s = false;
            if (args.Length < 3) return new string[] { GetUsage() };
            if (!byte.TryParse(args[2], out byte value)) return new string[] { GetUsage() };
            var output = ForeachPlayer(args[0], out bool success, (player) =>
            {
                switch (args[1].ToLower())
                {
                    case "lvl":
                        {
                            player.Level = value;
                            return new string[] { "Done" };
                        }
                    case "xp":
                        {
                            player.Experience = value;
                            return new string[] { "Done" };
                        }
                    case "ap":
                        {
                            player.Energy = value;
                            return new string[] { "Done" };
                        }
                    case "max_ap":
                        {
                            player.MaxEnergy = value;
                            return new string[] { "Done" };
                        }
                    default:
                        {
                            return new string[] { GetUsage() };
                        }
                }
            });
            if (!success) return new string[] { "Plyaer not found", GetUsage() };
            _s = true;
            return output;
        }
    }
}
