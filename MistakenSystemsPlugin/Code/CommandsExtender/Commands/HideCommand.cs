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
            string id = sender.GetPlayer().UserId;
            if(!LOFH.LOFH.Hidden.Contains(id)) 
                LOFH.LOFH.Hidden.Add(id);
            else
                LOFH.LOFH.Hidden.Remove(id);
            _s = true;
            return new string[] { "Done" };
        }
    }
}
