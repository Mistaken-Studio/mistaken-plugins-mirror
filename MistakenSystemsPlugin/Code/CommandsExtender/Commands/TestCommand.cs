using CommandSystem;
using Exiled.API.Features;
using Gamer.Utilities;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;
using System.Text;
using UnityEngine;

namespace Gamer.Mistaken.CommandsExtender.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))] 
    class TestCommand : IBetterCommand, IPermissionLocked
    {
        public override string Command => "test";

        public string Permission => "locked";

        public string PluginName => PluginHandler.PluginName;

        public override string[] Execute(ICommandSender sender, string[] args, out bool _s)
        {
            _s = true;
            return new string[] { "Done" };
        }
    }
}
