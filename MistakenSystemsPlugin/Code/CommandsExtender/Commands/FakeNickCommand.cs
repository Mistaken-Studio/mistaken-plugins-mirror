using CommandSystem;
using Exiled.API.Extensions;
using Gamer.Utilities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Gamer.Mistaken.CommandsExtender.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))] 
    internal class FakeNickCommand : IBetterCommand, IPermissionLocked
    {
        public string Permission => "fakenick";

        public override string Description =>
            "Fakenick";

        public string PluginName => PluginHandler.PluginName;

        public override string Command => "fakenick";

        public override string[] Aliases => new string[] { "fname", "fnick" };

        public string GetUsage()
        {
            return "fakenick [Id] [nick]";
        }
        public static readonly Dictionary<string, string> FullNicknames = new Dictionary<string, string>();
        public override string[] Execute(ICommandSender sender, string[] args, out bool _s)
        {
            _s = false;
            if (args.Length < 2) return new string[] { GetUsage() };
            var players = this.GetPlayers(args[0]);
            if(players.Count > 1)
                return new string[] { "<b><size=200%>1 PLAYER</size></b>" };
            if (players.Count == 0)
                return new string[] { "Player not found" };
            var player = players[0];
            if(args.Contains("-full"))
            {
                FullNicknames[player.UserId] = string.Join(" ", args.Skip(1).Where(i => i != "-full"));
                if (string.IsNullOrWhiteSpace(FullNicknames[player.UserId]))
                    FullNicknames.Remove(player.UserId);
                MirrorExtensions.SendFakeTargetRpc(player, player.Connection.identity, typeof(PlayerStats), nameof(PlayerStats.RpcRoundrestart), 0.1f, true);
                //MirrorExtensions.SendFakeTargetRpc(player, player.Connection.identity, typeof(PlayerStats), nameof(PlayerStats.RpcRoundrestartRedirect), new object[] { 0.1f, (ushort)(7776 + serverId) });
                return new string[] { "Reconnecting" };
            }
            //players[0].ReferenceHub.nicknameSync.MyNick = string.Join(" ", args.Skip(1));
            //players[0].ReferenceHub.nicknameSync.UpdatePlayerlistInstance(players[0].ReferenceHub.nicknameSync.MyNick);
            player.DisplayNickname = string.Join(" ", args.Skip(1));
            _s = true;
            return new string[] { "Done" };
        }
    }
}
