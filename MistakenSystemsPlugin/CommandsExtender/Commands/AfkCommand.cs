
using CommandSystem;
using Gamer.Utilities;


namespace Gamer.Mistaken.CommandsExtender.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))]
    class AfkCommand : IBetterCommand, IPermissionLocked
    {
        public string Permission => "afk";

        public override string Description =>
            "Disconnect for AFK";

        public string PluginName => PluginHandler.PluginName;

        public override string Command => "afk";

        public override string[] Aliases => new string[] { };

        public string GetUsage()
        {
            return "Afk [Id]";
        }

        public override string[] Execute(ICommandSender sender, string[] args, out bool _s)
        {
            _s = false;
            if (args.Length == 0) return new string[] { GetUsage() };
            var players = this.GetPlayers(args[0]);
            if (players.Count > 1)
                return new string[] { "<b><size=200%>1 PLAYER</size></b>" };
            if (players.Count == 0)
                return new string[] { "Player not found" };
            players[0].Disconnect("You were kicked for being AFK");
            _s = true;
            return new string[] { "Done" };
        }
    }
}
