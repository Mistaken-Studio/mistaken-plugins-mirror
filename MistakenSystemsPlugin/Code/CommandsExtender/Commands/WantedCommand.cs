using CommandSystem;
using Gamer.Mistaken.Systems.Misc;
using Gamer.Utilities;
using System.Linq;
using System.Net;
using UnityEngine;


namespace Gamer.Mistaken.CommandsExtender.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))] 
    class WantedCommand : IBetterCommand, IPermissionLocked
    {
        public string Permission => "wanted";

        public override string Description => "wanted [USERID]";

        public string PluginName => PluginHandler.PluginName;

        public override string Command => "wanted";

        public override string[] Aliases => new string[] { };

        public string GetUsage()
        {
            return "Wanted [User ID] [Time] [Reason]";
        }

        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            success = false;
            if (args.Length == 0) return new string[] { GetUsage() };
            string steamId = args[0];
            string reason = string.Join(" ", args.Skip(2));
            if (!int.TryParse(args[1].ToLower().Replace("mo", "").Trim(new char[] { 'm', 'h', 'd', 'w', 'y' }), out int duration))
                return new string[] { "Detected error with number convertion", "Failed to convert |" + args[1].ToLower().Replace("mo", "").Trim(new char[] { 'm', 'h', 'd', 'w', 'y' }) + "| to int" };

            if (args[1].Contains("m") && !args[1].Contains("mo")) { args[1] = args[1].Replace("m", null); duration *= 1; }
            if (args[1].Contains("h")) { args[1].Replace("h", null); duration *= 60; }
            if (args[1].Contains("d")) { args[1].Replace("d", null); duration *= 1440;  }
            if (args[1].Contains("w")) { args[1].Replace("w", null); duration *= 10080; }
            if (args[1].Contains("mo")) { args[1].Replace("mo", null); duration *= 43200; }
            if (args[1].Contains("y")) { args[1].Replace("y", null); duration *= 518400; }

            if (!Gamer.Mistaken.Utilities.APILib.API.GetAPIKey(out string key))
            {
                success = false;
                return new string[] { "Server doesn't have API Key | Not Real ApiLib" };
            }
            using(var client = new WebClient())
                client.DownloadStringAsync(new System.Uri($"https://mistaken.pl/admin/api/wanteds.php?key={key}&mode=add&usersid={steamId}&duration={duration}&reason={reason}&adminsid={sender.GetPlayer().UserId}"));

            success = true;
            return new string[] { "Done" };
        }
    }
}
