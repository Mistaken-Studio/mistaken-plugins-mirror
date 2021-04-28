#pragma warning disable CS0649

using Exiled.API.Features;
using Gamer.Diagnostics;
using Gamer.Mistaken.Utilities.APILib;
using Gamer.RoundLoggerSystem;
using Gamer.Utilities;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Text;

namespace Gamer.Mistaken.Base.Staff
{
    /// <inheritdoc/>
    public class StaffHandler : Module
    {
        /// <inheritdoc/>
        public override bool IsBasic => true;
        /// <inheritdoc/>
        public StaffHandler(PluginHandler p) : base(p)
        {
        }
        /// <inheritdoc/>
        public override string Name => "Staff";
        /// <inheritdoc/>
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.RestartingRound += this.Handle(() => Server_RestartingRound(), "RoundRestart");
            Exiled.Events.Handlers.Player.ChangingGroup += this.Handle<Exiled.Events.EventArgs.ChangingGroupEventArgs>((ev) => Player_ChangingGroup(ev));
            Exiled.Events.Handlers.Player.Verified += this.Handle<Exiled.Events.EventArgs.VerifiedEventArgs>((ev) => Player_Verified(ev));

            Server_RestartingRound();
        }
        /// <inheritdoc/>
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
            if (ev.Player == null)
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
                if (info == null)
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

        /// <summary>
        /// Staff List
        /// </summary>
        public static UserInfo[] Staff { get; set; } = new UserInfo[0];
        private static void RequestStaffCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            Staff = JsonConvert.DeserializeObject<UsersInfo>(Encoding.Default.GetString(e.Result)).users;
            Main.IgnoredUIDs = Staff.Where(i => i.ignoreDNT == 1).Select(i => i.steamid).ToArray();
        }

        private class UsersInfo
        {
            public UserInfo[] users;
        }
        /// <summary>
        /// User Info Class
        /// </summary>
        public class UserInfo
        {
            /// <summary>
            /// Staff Nick
            /// </summary>
            public string nick;
            /// <summary>
            /// Staff Role
            /// </summary>
            public string role;
            /// <summary>
            /// Staff Role Color
            /// </summary>
            public string role_color;
            /// <summary>
            /// Staff Role Id
            /// </summary>
            public string role_id;
            /// <summary>
            /// Staff Role Type
            /// </summary>
            public string type_id;
            /// <summary>
            /// Staff UserId
            /// </summary>
            public string steamid;
            /// <summary>
            /// Staff DiscordId
            /// </summary>
            public string discordid;
            /// <summary>
            /// If is active staff
            /// </summary>
            public int active;
            /// <summary>
            /// SL Perms
            /// </summary>
            public ulong slperms;
            /// <summary>
            /// If rank should be shown
            /// </summary>
            public bool show_rank;
            /// <summary>
            /// If DNT should be ignored
            /// </summary>
            public int ignoreDNT;
        }
    }
    /// <summary>
    /// Extensions
    /// </summary>
    public static class StaffExtensions
    {
        /// <summary>
        /// If is active dev
        /// </summary>
        /// <param name="player">player</param>
        /// <returns><see langword="true"/> if both <see cref="Main.IsDevUserId(string)"/> and <see cref="IsStaff(string)"/> are <see langword="true"/></returns>
        public static bool IsActiveDev(this Player player) => player.UserId.IsDevUserId() && player.UserId.IsStaff();

        /// <summary>
        /// If is staff
        /// </summary>
        /// <param name="player">player</param>
        /// <returns>Result of <see cref="IsStaff(string)"/></returns>
        public static bool IsStaff(this Player player) => player.UserId.IsStaff();
        /// <summary>
        /// If is staff
        /// </summary>
        /// <param name="UserId">UserId</param>
        /// <returns>If userId is staff's userId</returns>
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
        /// <summary>
        /// Returns Staff Info
        /// </summary>
        /// <param name="player">player</param>
        /// <returns>Staff Info</returns>
        public static StaffHandler.UserInfo GetStaff(this Player player) => player.UserId.GetStaff();
        /// <summary>
        /// Returns Staff Info
        /// </summary>
        /// <param name="UserId">userId</param>
        /// <returns>Staff Info</returns>
        public static StaffHandler.UserInfo GetStaff(this string UserId)
        {
            if (UserId.IsStaff())
                return StaffHandler.Staff.FirstOrDefault(i => i.steamid == UserId || i.discordid + "@discord" == UserId);
            return null;
        }
    }
}
