﻿using CommandSystem;
using Exiled.API.Features;
using Gamer.Mistaken.Base.GUI;
using Gamer.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace Gamer.Mistaken.CommandsExtender.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))]
    internal class MuteAllCommand : IBetterCommand, IPermissionLocked
    {
        public string Permission => "muteall";

        public override string Description =>
            "Mutes everybody except admins";

        public string PluginName => PluginHandler.PluginName;

        public override string Command => "muteall";

        public override string[] Aliases => new string[] { "gmute", "mall" };

        public string GetUsage()
        {
            return "MUTEALL";
        }

        public static List<string> Muted = new List<string>();
        public static bool GlobalMuteActive = false;

        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            success = true;
            GlobalMuteActive = true;
            foreach (var player in RealPlayers.List.Where(p => p.IsMuted == false && !p.CheckPermissions(PlayerPermissions.AdminChat)))
            {
                Muted.Add(player.UserId);
                player.IsMuted = true;
                Gamer.Utilities.BetterCourotines.RunCoroutine(InformGlobalMute(player), "MuteAllCommand.InformGlobalMute");
            }
            MapPlus.Broadcast("GLOBAL MUTE", 10, "Activated Global Mute", Broadcast.BroadcastFlags.AdminChat);
            return new string[] { "Done" };
        }

        private IEnumerator<float> InformGlobalMute(Player player)
        {
            while (GlobalMuteActive)
            {
                player.SetGUI("globalMute", Base.GUI.PseudoGUIHandler.Position.TOP, "<color=green>[<color=orange>GLOBAL MUTE</color>]</color> Everyone except admins are muted");
                yield return MEC.Timing.WaitForSeconds(1);
            }
            player.SetGUI("globalMute", Base.GUI.PseudoGUIHandler.Position.TOP, null);
        }
    }
}
