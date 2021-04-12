using CommandSystem;
using Exiled.API.Features;
using Gamer.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace Gamer.Mistaken.CommandsExtender.Commands
{
        [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))] class UnMuteAllCommand : IBetterCommand, IPermissionLocked
    {
        public string Permission => "muteall";

        public override string Description =>
            "Disabled muteall";
        public override string Command => "unmuteall";

        public override string[] Aliases => new string[] { "gunmute", "unmall" };

        public string PluginName => PluginHandler.PluginName;

        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            success = false;
            if (!MuteAllCommand.GlobalMuteActive) return new string[] { "Global Mute is not active" };
            success = true;
            MuteAllCommand.GlobalMuteActive = false;
            foreach (var uId in MuteAllCommand.Muted.ToArray())
            {
                if (RealPlayers.List.Any(p => p.UserId == uId))
                {
                    var player = RealPlayers.List.First(p => p.UserId == uId);
                    player.IsMuted = false;
                    Systems.GUI.PseudoGUIHandler.Set(player, "globalMute", Systems.GUI.PseudoGUIHandler.Position.TOP, "<color=green>[<color=orange>GLOBAL MUTE</color>]</color> Everyone was unmuted", 5);
                    MuteAllCommand.Muted.Remove(uId);
                    //player.Broadcast("GLOBAL MUTE", 10, "Everyone was unmuted");
                }
                else
                    MapPlus.Broadcast("GLOBAL MUTE", 10, $"Failed to unmute {uId} because plugin failed to find him, report this incident", Broadcast.BroadcastFlags.AdminChat);
            }
            MapPlus.Broadcast("GLOBAL MUTE", 10, "Deactivated Global Mute", Broadcast.BroadcastFlags.AdminChat);
            return new string[] { "Done" };
        }

        public string GetUsage()
        {
            return "UNMUTEALL";
        }
    }
}
