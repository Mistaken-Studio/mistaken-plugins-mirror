using CommandSystem;
using Exiled.API.Features;
using Gamer.Utilities;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;
using System.Text;

namespace Gamer.Mistaken.CommandsExtender.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))] 
    class HideCommand : IBetterCommand, IPermissionLocked
    {
        public override string Command => "hide";

        public string Permission => "hide";

        public string PluginName => PluginHandler.PluginName;

        public override string[] Execute(ICommandSender sender, string[] args, out bool _s)
        {
            var player = sender.GetPlayer();
            if(!LOFH.LOFH.Hidden.Contains(player.UserId)) 
                LOFH.LOFH.Hidden.Add(player.UserId);
            else
                LOFH.LOFH.Hidden.Remove(player.UserId);
            player.SessionVariables["HIDDEN"] = LOFH.LOFH.Hidden.Contains(player.UserId);
            AnnonymousEvents.Call("HIDDEN", (player, LOFH.LOFH.Hidden.Contains(player.UserId)));
            _s = true;
            return new string[] { "Done" };
        }
    }
}
