using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Exiled.API.Features;
using Gamer.Diagnostics;
using Gamer.Utilities;
using Gamer.Mistaken.Utilities.APILib;
using MEC;
using Newtonsoft.Json;
using UnidecodeSharpFork;
using UnityEngine;

namespace Gamer.Mistaken.Systems.Staff
{
    public class StaffHandler : Module
    {
        public StaffHandler(PluginHandler p) : base(p)
        {
        }

        public override string Name => "Staff";
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.RestartingRound += this.Handle(() => Server_RestartingRound(), "RoundRestart");
            Exiled.Events.Handlers.Player.ChangingGroup += this.Handle<Exiled.Events.EventArgs.ChangingGroupEventArgs>((ev) => Player_ChangingGroup(ev));
            Server_RestartingRound();
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.RestartingRound -= this.Handle(() => Server_RestartingRound(), "RoundRestart");
            Exiled.Events.Handlers.Player.ChangingGroup -= this.Handle<Exiled.Events.EventArgs.ChangingGroupEventArgs>((ev) => Player_ChangingGroup(ev));
        }

        private void Player_ChangingGroup(Exiled.Events.EventArgs.ChangingGroupEventArgs ev)
        {
            if ((ev.NewGroup?.Permissions ?? 0) == 0)
                return;
            if (Staff.Length == 0)
            {
                Log.Warn("Skipped Permission Validation, Staff List is empty");
                return;
            }
            if(ev.Player == null)
            {
                Log.Warn("Player is null");
                MEC.Timing.CallDelayed(5, () =>
                {
                    foreach (var player in RealPlayers.List.Where(p => p.Group?.Permissions != 0))
                    {
                        if (player.IsStaff() || player.AuthenticationType == Exiled.API.Enums.AuthenticationType.Northwood)
                            continue;
                        player.SendConsoleMessage("Warning, You are not staff but you have permissions, permissions revoked\nIf you think this is an error contact Gamer", "red");
                        player.Group.Permissions = 0;
                        player.Group.BadgeColor = "white";
                        player.Group.BadgeText = "";
                        player.Group.KickPower = 0;
                    }
                });
                return;
            }
            if (ev.Player.IsStaff() || ev.Player.AuthenticationType == Exiled.API.Enums.AuthenticationType.Northwood)
                return;
            //ev.IsAllowed = false;
            ev.Player.SendConsoleMessage("Warning, You are not staff but you have permissions, permissions revoked\nIf you think this is an error contact Gamer", "red");
            ev.NewGroup.Permissions = 0;
            ev.NewGroup.BadgeColor = "white";
            ev.NewGroup.BadgeText = "";
            ev.NewGroup.KickPower = 0;
        }

        private static void Server_RestartingRound()
        {
            using (var client = new WebClient())
            {
                if (!Gamer.Mistaken.Utilities.APILib.API.GetUrl(APIType.PERMISSIONS, out string url, ""))
                    return;
                try
                {
                    client.DownloadDataCompleted += RequestStaffCompleted;
                    client.DownloadDataAsync(new Uri(url));
                }
                catch
                {
                }
            }
        }
        

        public static UserInfo[] Staff { get; set; } = new UserInfo[0];
        private static void RequestStaffCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            Staff = JsonConvert.DeserializeObject<UsersInfo>(Encoding.Default.GetString(e.Result)).users;
        }


        public class UsersInfo
        {
            public UserInfo[] users;
        }
        public class UserInfo
        {
            public string nick;
            public string role;
            public string role_color;
            public string role_id;
            public string type_id;
            public string steamid;
            public string discordid;
            public int active;
        }
    }

    public static class StaffExtensions
    {
        public static bool IsStaff(this Player player) => player.UserId.IsStaff();

        public static bool IsStaff(this string UserId)
        {
            if (UserId.IsDevUserId())
                return true;
            if (StaffHandler.Staff.Any(i => i.steamid == UserId))
                return true;
            if (StaffHandler.Staff.Any(i => i.discordid + "@discord" == UserId))
                return true;
            return false;
        }
    }
}
