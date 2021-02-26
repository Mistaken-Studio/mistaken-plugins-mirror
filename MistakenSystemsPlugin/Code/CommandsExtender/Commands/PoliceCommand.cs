using CommandSystem;
using Exiled.API.Features;
using Gamer.Utilities;
using MEC;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;


namespace Gamer.Mistaken.CommandsExtender.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))] 
    class PoliceCommand : IBetterCommand, IPermissionLocked
    {
        public string Permission => "police";

        public override string Description =>
            "Signals :)";

        public string PluginName => PluginHandler.PluginName;

        public override string Command => "police";

        public override string[] Aliases => new string[] { };

        public string GetUsage()
        {
            return "Police (True/False)";
        }

        public static readonly Dictionary<string, float> PoliceMode = new Dictionary<string, float>();
        public override string[] Execute(ICommandSender sender, string[] args, out bool _s)
        {
            var player = sender.GetPlayer();
            if (args.Length == 0 || !bool.TryParse(args[0], out bool value))
                value = !PoliceMode.ContainsKey(player.UserId);
            if (args.Length < 2 || !float.TryParse(args[1], out float time))
                time = 1;
            bool start = value;
            if (value && PoliceMode.ContainsKey(player.UserId))
            {
                PoliceMode.Remove(player.UserId);
                start = false;
            }
            if (value)
                PoliceMode.Add(player.UserId, time);
            else
                PoliceMode.Remove(player.UserId);
            if (start)
                Timing.RunCoroutine(Execute(player));
            _s = true;
            return new string[] { "Done" };
        }

        private IEnumerator<float> Execute(Player player)
        {
            while(PoliceMode.TryGetValue(player.UserId, out float time))
            {
                if (player.RankColor != "red")
                    player.RankColor = "red";
                else
                    player.RankColor = "cyan";

                yield return Timing.WaitForSeconds(time);
            }
        }
    }
}
