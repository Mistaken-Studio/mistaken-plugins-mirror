using CommandSystem;
using Gamer.Utilities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Gamer.Mistaken.CommandsExtender.Commands
{
        [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))] class CassiePrivateCommand : IBetterCommand, IPermissionLocked
    {
        public string Permission => "cassie_p";

        public override string Description =>
            "CASSIE Private";

        public string PluginName => PluginHandler.PluginName;

        public override string Command => "cassie_p";

        public override string[] Aliases => new string[] { "cassie_private" };

        public string GetUsage()
        {
            return "CASSIE_P [Target] [MESSAGE]";
        }

        public override string[] Execute(ICommandSender sender, string[] args, out bool _s)
        {
            _s = false;
            return new string[] { "Haha, cassie says <b><color=red>no</color></b>" };
            /*if (args.Length == 0) return new string[] { GetUsage() };
            var pids = args[0];
            args = args.Skip(1).ToArray();
            var tor = this.ForeachPlayer(pids, out bool success, (player) => {
                PlayerManager.localPlayer.GetComponent<Respawning.RespawnEffectsController>().CallTargetRpc("RpcCassieAnnouncement", player.Connection, string.Join(" ", args), false, false);
                return new string[] { "Done" };
            });
            if (!success)
                return new string[] { "No players found" };
            _s = true;
            return tor;*/
        }
    }
}
