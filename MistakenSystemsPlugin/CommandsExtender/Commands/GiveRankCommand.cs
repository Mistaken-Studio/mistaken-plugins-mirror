
using CommandSystem;
using Gamer.Utilities;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;
using System.Text;


namespace Gamer.Mistaken.CommandsExtender.Commands
{
        [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))] class GiveRankCommand : IBetterCommand, IPermissionLocked
    {
        public string Permission => "giverank";

        public override string Description =>
        "Gives Rank";

        public string PluginName => PluginHandler.PluginName;

        public override string Command => "giverank";

        public override string[] Aliases => new string[] { };

        public string GetUsage()
        {
            return "GIVERANK [Id] [COLOR] [NAME]";
        }

        public override string[] Execute(ICommandSender sender, string[] args, out bool _s)
        {
            _s = false;
            if (args.Length < 3) return new string[] { GetUsage() };
            string color = args[1].ToLower();
            string txt = string.Join(" ", args.Skip(2));
            var output = this.ForeachPlayer(args[0], out bool success, (player) =>
            {
                player.RankColor = color;
                player.RankName = txt;

                return new string[] { "Done" };
            });
            if (!success)
                return new string[] { "Player not found", GetUsage() };
            _s = true;
            return output;
        }
    }
}
