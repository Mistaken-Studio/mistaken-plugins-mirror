using CommandSystem;
using Gamer.Utilities;
using System.Linq;


namespace Gamer.Mistaken.CommandsExtender.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))]
    internal class HintCommand : IBetterCommand, IPermissionLocked
    {
        public string Permission => "hint";

        public override string Description =>
        "Hints";

        public string PluginName => PluginHandler.PluginName;

        public override string Command => "hint";

        public override string[] Aliases => new string[] { };

        public string GetUsage()
        {
            return "hint [playerId] [duration] [message]";
        }

        public override string[] Execute(ICommandSender sender, string[] args, out bool _s)
        {
            _s = false;
            if (args.Length == 0) return new string[] { GetUsage() };

            if (float.TryParse(args[1], out float duration))
            {
                var result = this.ForeachPlayer(args[0], out bool success, p =>
                {
                    p.ShowHint(string.Join(" ", args.Skip(2)), duration);
                    return new string[] { "Done" };
                });
                if (!success) return new string[] { "Player not found" };
                _s = true;
                return result;
            }
            else
                return new string[] { "Wrong duration" };
        }
    }
}
