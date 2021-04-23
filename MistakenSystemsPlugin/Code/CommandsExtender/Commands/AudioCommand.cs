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
    class AudioCommand : IBetterCommand, IPermissionLocked
    {
        public override string Command => "audio";

        public string Permission => "audio";
        public string PluginName => PluginHandler.PluginName;

        public override string[] Execute(ICommandSender sender, string[] args, out bool _s)
        {
            _s = false;
            return new string[] { "Does not work" };
            /*if (args.Length == 0) return new string[] { "Wrong args" };
            if (args.Length < 2 || !float.TryParse(args[1], out float volume))
                volume = 1;
            CommsHack.AudioAPI.API.PlayFileRaw(CommsHack.AudioAPI.BASE_PATH + args[0], volume);
            _s = true;
            return new string[] { "Done" };*/
        }
    }
}
