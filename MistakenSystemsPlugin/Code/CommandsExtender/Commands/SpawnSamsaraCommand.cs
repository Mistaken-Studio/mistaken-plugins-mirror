using CommandSystem;
using Gamer.Utilities;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;
using System.Text;


namespace Gamer.Mistaken.CommandsExtender.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))] 
    class SpawnSamsaraCommand : IBetterCommand, IPermissionLocked
    {
        public string Permission => "samsara";

        public string PluginName => PluginHandler.PluginName;

        public override string Command => "spawn_samsara";

        public override string[] Aliases => new string[] { "samsara" };

        public override string[] Execute(ICommandSender sender, string[] args, out bool _s)
        {
            Systems.End.NoEndlessRoundHandler.SpawnAsSamsara(new System.Collections.Generic.List<Exiled.API.Features.Player>
            {
                sender.GetPlayer()
            });
            _s = true;
            return new string[] { "Done" };
        }
    }
}
