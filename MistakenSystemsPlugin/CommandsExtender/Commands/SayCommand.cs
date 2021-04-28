using CommandSystem;
using Gamer.Utilities;
using System.Linq;


namespace Gamer.Mistaken.CommandsExtender.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))]
    class SayCommand : IBetterCommand, IPermissionLocked
    {
        public string Permission => "say";

        public override string Description =>
         "Prints text in players console";

        public string PluginName => PluginHandler.PluginName;

        public override string Command => "say";

        public override string[] Aliases => new string[] { };

        public string GetUsage()
        {
            return "SAY [Id] [COLOR] [MESSAGE]";
        }

        public override string[] Execute(ICommandSender sender, string[] args, out bool _s)
        {
            _s = false;
            if (args.Length < 3) return new string[] { GetUsage() };
            string msg = string.Join(" ", args.Skip(2));
            string color = args[1].ToLower();
            var output = this.ForeachPlayer(args[0], out bool success, (player) =>
            {
                player.SendConsoleMessage(msg, color);
                return new string[] { "Done" };
            });
            if (!success)
                return new string[] { "Player not found", GetUsage() };
            _s = true;
            return output;
        }
    }
}
