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
using Gamer.RoundLoggerSystem;
using Exiled.API.Extensions;

namespace Gamer.Mistaken.Systems.Staff
{
    public class StaffHandler : Module
    {
        public override bool IsBasic => true;
        public StaffHandler(PluginHandler p) : base(p)
        {
        }

        public override string Name => "Staff";
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.RestartingRound += this.Handle(() => Server_RestartingRound(), "RoundRestart");
            Exiled.Events.Handlers.Player.ChangingGroup += this.Handle<Exiled.Events.EventArgs.ChangingGroupEventArgs>((ev) => Player_ChangingGroup(ev));
            Exiled.Events.Handlers.Player.Verified += this.Handle<Exiled.Events.EventArgs.VerifiedEventArgs>((ev) => Player_Verified(ev));

            Server_RestartingRound();
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.RestartingRound -= this.Handle(() => Server_RestartingRound(), "RoundRestart");
            Exiled.Events.Handlers.Player.ChangingGroup -= this.Handle<Exiled.Events.EventArgs.ChangingGroupEventArgs>((ev) => Player_ChangingGroup(ev));
            Exiled.Events.Handlers.Player.Verified -= this.Handle<Exiled.Events.EventArgs.VerifiedEventArgs>((ev) => Player_Verified(ev));
        }

        private void Player_Verified(Exiled.Events.EventArgs.VerifiedEventArgs ev)
        {
            if (ev.Player.IsStaff())
            {
                var cgea = new Exiled.Events.EventArgs.ChangingGroupEventArgs(ev.Player, ev.Player.Group);
                Player_ChangingGroup(cgea);
                if (cgea.IsAllowed && ev.Player.Group != cgea.NewGroup)
                    ev.Player.Group = cgea.NewGroup;
            }
            CustomInfoHandler.Set(ev.Player, "STAFF", $"Player Id: <b>{ev.Player.Id}</b>", true);
        }

        private void Player_ChangingGroup(Exiled.Events.EventArgs.ChangingGroupEventArgs ev)
        {
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
                        RoundLogger.Log("STAFF", "REVOKE", $"Revoked staff permissions for {ev.Player.PlayerToString()}");
                    }
                });
                return;
            }
            if (ev.Player.AuthenticationType == Exiled.API.Enums.AuthenticationType.Northwood)
                return;
            if (ev.Player.IsStaff())
            {
                var info = ev.Player.GetStaff();
                if(info == null)
                    Log.Error($"Is staff decided that player is staff but GetStaff had other idea");
                else
                {
                    if (info.slperms != 0)
                    {
                        if (ev.NewGroup == null)
                        {
                            ev.Player.Group = new UserGroup
                            {
                                Permissions = info.slperms,
                                HiddenByDefault = true,
                                BadgeText = info.role,
                                BadgeColor = info.role_color,
                                KickPower = 2,
                                RequiredKickPower = 3,
                                Shared = true,
                                Cover = false
                            };
                            ev.IsAllowed = false;
                            return;
                        }
                        else
                            ev.NewGroup.Permissions |= info.slperms;
                        //ev.Player.ReferenceHub.serverRoles.RemoteAdmin = true;
                        //ev.Player.ReferenceHub.serverRoles.RemoteAdminMode = ServerRoles.AccessMode.LocalAccess;
                        RoundLogger.Log("STAFF", "GRANT", $"Granted staff permissions for {ev.Player.PlayerToString()}");
                        ev.Player.SendConsoleMessage("You have been granted additional permissions", "red");
                        Log.Debug("Giving " + info.slperms);
                    }
                    else
                        Log.Debug("No perms");
                    return;
                }
               
            }
            else
                Log.Debug("Not staff");
            if ((ev.NewGroup?.Permissions ?? 0) == 0)
                return;
            //ev.IsAllowed = false;
            ev.Player.SendConsoleMessage("Warning, You are not staff but you have permissions, permissions revoked\nIf you think this is an error contact Gamer", "red");
            ev.NewGroup.Permissions = 0;
            ev.NewGroup.BadgeColor = "white";
            ev.NewGroup.BadgeText = "";
            ev.NewGroup.KickPower = 0;
            RoundLogger.Log("STAFF", "REVOKE", $"Revoked staff permissions for {ev.Player.PlayerToString()}");
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
            public ulong slperms;
            public bool show_rank;
        }
    }

    public static class StaffExtensions
    {
        public static bool IsActiveDev(this Player player) => player.UserId.IsDevUserId() && player.UserId.IsStaff();

        public static bool IsStaff(this Player player) => player.UserId.IsStaff();

        public static bool IsStaff(this string UserId)
        {
            if (UserId.IsDevUserId() && StaffHandler.Staff.Length == 0)
                return true;
            if (StaffHandler.Staff.Any(i => i.steamid == UserId))
                return true;
            if (StaffHandler.Staff.Any(i => i.discordid + "@discord" == UserId))
                return true;
            return false;
        }

        public static StaffHandler.UserInfo GetStaff(this Player player) => player.UserId.GetStaff();
        public static StaffHandler.UserInfo GetStaff(this string UserId)
        {
            if(UserId.IsStaff())
                return StaffHandler.Staff.FirstOrDefault(i => i.steamid == UserId || i.discordid + "@discord" == UserId);
            return null;
        }
    }
}
