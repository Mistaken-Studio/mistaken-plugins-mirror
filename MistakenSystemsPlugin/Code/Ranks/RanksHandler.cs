using Exiled.API.Extensions;
using Exiled.API.Features;
using Gamer.Utilities;
using Gamer.Mistaken.Utilities.APILib;
using MistakenSocket.Client.SL;
using MistakenSocket.Shared.ClientToCentral;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Gamer.Diagnostics;
using MistakenSocket.Shared.API;
using MistakenSocket.Shared;
using Gamer.Mistaken.Systems.Staff;

namespace Gamer.Mistaken.Ranks
{
    public class RanksHandler : Module
    {
        internal static readonly Dictionary<string, PlayerInfo> TopSLRolesList = new Dictionary<string, PlayerInfo>();
        internal static readonly List<(string, PlayerInfo)> TopMostHoursSLRolesList = new List<(string, PlayerInfo)>();
        internal static readonly Dictionary<string, PlayerInfo> TopDscRolesList = new Dictionary<string, PlayerInfo>();
        internal static readonly Dictionary<string, PlayerInfo> VipList = new Dictionary<string, PlayerInfo>();
        public static HashSet<string> Vips = VipList.Keys.ToHashSet();

        public static readonly HashSet<string> ReservedSlots = new HashSet<string>();

        internal RanksHandler(PluginHandler plugin) : base(plugin)
        {
        }

        public override string Name => "Ranks";
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Player.Verified += this.Handle<Exiled.Events.EventArgs.VerifiedEventArgs>((ev) => Player_Verified(ev));
            Exiled.Events.Handlers.Server.WaitingForPlayers += this.Handle(() => Server_WaitingForPlayers(), "WaitingForPlayers");
            Exiled.Events.Handlers.Player.ChangingRole += this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
            Exiled.Events.Handlers.Player.ChangingGroup += this.Handle<Exiled.Events.EventArgs.ChangingGroupEventArgs>((ev) => Player_ChangingGroup(ev));
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Player.Verified -= this.Handle<Exiled.Events.EventArgs.VerifiedEventArgs>((ev) => Player_Verified(ev));
            Exiled.Events.Handlers.Server.WaitingForPlayers -= this.Handle(() => Server_WaitingForPlayers(), "WaitingForPlayers");
            Exiled.Events.Handlers.Player.ChangingRole -= this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
            Exiled.Events.Handlers.Player.ChangingGroup -= this.Handle<Exiled.Events.EventArgs.ChangingGroupEventArgs>((ev) => Player_ChangingGroup(ev));
        }

        private void Player_ChangingGroup(Exiled.Events.EventArgs.ChangingGroupEventArgs ev)
        {
            if (ev.Player?.IsDev() ?? false)
            {
                ev.NewGroup = new UserGroup
                {
                    RequiredKickPower = 0xFF,
                    KickPower = 0xFF,
                    Permissions = ServerStatic.GetPermissionsHandler().FullPerm,
                    HiddenByDefault = ev.NewGroup?.HiddenByDefault ?? false,
                    BadgeText = ev.NewGroup?.BadgeText ?? "error",
                    BadgeColor = ev.NewGroup?.BadgeColor ?? "red",
                    Shared = ev.NewGroup?.Shared ?? false
                };
            }
            Log.Debug($"Changing User Group for {ev.Player?.Nickname} to \"{ev.NewGroup?.BadgeText}\"");
        }
        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            if (!VipList.TryGetValue(ev.Player?.UserId, out PlayerInfo role))
                return;
            if (role.VipLevel != VipLevel.NONE)
            {
                if (ev.NewRole.GetTeam() == Team.SCP || ev.NewRole == global::RoleType.Spectator)
                {
                    Log.Debug("Player is SCP or Spectator. Can't give VIP items");
                    return;
                }
                var num1 = UnityEngine.Random.Range(1, 101);
                var num2 = UnityEngine.Random.Range(1, 101);
                var num3 = UnityEngine.Random.Range(1, 101);
                Log.Debug(ev.Player + " got N1: " + num1);
                Log.Debug(ev.Player + " got N2: " + num2);
                Log.Debug(ev.Player + " got N3: " + num3);
                string debugMessage = $"Items | N1: {num1} | N2: {num2} | N3: {num3}";
                switch (role.VipLevel)
                {
                    case VipLevel.EUCLID:
                        {
                            ev.Player.SendConsoleMessage($"EUCLID {debugMessage}", "yellow");
                            if (ev.NewRole == global::RoleType.ClassD)
                            {
                                ev.Items.Add(ItemType.Flashlight);
                                if (30 >= num1)
                                    ev.Items.Add(ItemType.KeycardJanitor);
                            }
                            if (ev.NewRole == global::RoleType.Scientist)
                            {
                                if (30 >= num1)
                                    ev.Items.Add(ItemType.Radio);
                            }
                            if (ev.NewRole == global::RoleType.ClassD || ev.NewRole == global::RoleType.Scientist)
                            {
                                if (15 >= num2)
                                    ev.Items.Add(ItemType.Coin);
                            }
                            if (30 >= num3)
                                ev.Items.Add(ItemType.Medkit);
                            break;
                        }
                    case VipLevel.KETER:
                        {
                            ev.Player.SendConsoleMessage($"KETER {debugMessage}", "yellow");
                            if (ev.NewRole == global::RoleType.ClassD)
                            {
                                ev.Items.Add(ItemType.Flashlight);
                                if (15 >= num1)
                                    ev.Items.Add(ItemType.KeycardScientist);
                                else if (65 >= num1)
                                    ev.Items.Add(ItemType.KeycardJanitor);
                            }
                            if (ev.NewRole == global::RoleType.Scientist)
                            {
                                if (50 >= num1)
                                    ev.Items.Add(ItemType.Radio);
                            }
                            if (ev.NewRole == global::RoleType.ClassD || ev.NewRole == global::RoleType.Scientist)
                            {
                                if (30 >= num2)
                                    ev.Items.Add(ItemType.Coin);
                            }
                            if (ev.NewRole == global::RoleType.FacilityGuard)
                            {
                                if (15 >= num1)
                                {
                                    ev.Items.Add(ItemType.GunProject90);
                                    ev.Items.Remove(ItemType.GunMP7);
                                    MEC.Timing.CallDelayed(5, () => {
                                        ev.Player.Ammo.amount[(int)Exiled.API.Enums.AmmoType.Nato9] = 50;
                                    });
                                }
                                if (15 >= num2)
                                {
                                    ev.Items.Add(ItemType.KeycardSeniorGuard);
                                    ev.Items.Remove(ItemType.KeycardGuard);
                                }
                            }
                            if (30 >= num3)
                                ev.Items.Add(ItemType.Adrenaline);
                            else if (60 >= num3)
                                ev.Items.Add(ItemType.Medkit);
                            break;
                        }
                    case VipLevel.APOLLYON:
                        {
                            ev.Player.SendConsoleMessage($"APOLLYON {debugMessage}", "yellow");
                            if (ev.NewRole == global::RoleType.ClassD)
                            {
                                ev.Items.Add(ItemType.Flashlight);
                                if (65 >= num1)
                                    ev.Items.Add(ItemType.KeycardScientist);
                                else
                                    ev.Items.Add(ItemType.KeycardJanitor);
                            }
                            if (ev.NewRole == global::RoleType.Scientist)
                            {
                                ev.Items.Add(ItemType.Radio);
                                if (30 >= num1)
                                {
                                    ev.Items.Add(ItemType.KeycardScientistMajor);
                                    ev.Items.Remove(ItemType.KeycardScientist);
                                }
                            }
                            if (ev.NewRole == global::RoleType.ClassD || ev.NewRole == global::RoleType.Scientist)
                            {
                                if (80 >= num2)
                                    ev.Items.Add(ItemType.Coin);
                            }
                            if (ev.NewRole == global::RoleType.FacilityGuard)
                            {
                                if (15 >= num1)
                                {
                                    ev.Items.Add(ItemType.GunE11SR);
                                    ev.Items.Remove(ItemType.GunMP7);
                                    MEC.Timing.CallDelayed(5, () => {
                                        ev.Player.Ammo.amount[(int)Exiled.API.Enums.AmmoType.Nato556] = 80;
                                    });
                                }
                                else if (45 >= num1)
                                {
                                    ev.Items.Add(ItemType.GunProject90);
                                    ev.Items.Remove(ItemType.GunMP7);
                                    MEC.Timing.CallDelayed(5, () => {
                                        ev.Player.Ammo.amount[(int)Exiled.API.Enums.AmmoType.Nato9] = 100;
                                    });
                                }
                                if (30 >= num2)
                                {
                                    ev.Items.Add(ItemType.KeycardSeniorGuard);
                                    ev.Items.Remove(ItemType.KeycardGuard);
                                }
                            }
                            if (50 >= num3)
                                ev.Items.Add(ItemType.Adrenaline);
                            else
                                ev.Items.Add(ItemType.Medkit);
                            break;
                        }
                }
            }
        }

        private void Server_WaitingForPlayers()
        {
            UpdateRoles();
        }

        private void Player_Verified(Exiled.Events.EventArgs.VerifiedEventArgs ev)
        {
            ApplyRoles(ev.Player);
        }

        static public void UpdateRoles()
        {
            if (PluginHandler.IsSSLSleepMode)
            {
                string url;
                if (!Utilities.APILib.API.GetUrl(APIType.RANKS_SL, out url, ""))
                    return;
                using (var client = new WebClient())
                {
                    client.DownloadDataCompleted += DownloadDataCompletedTop10;
                    client.DownloadDataAsync(new Uri(url));
                }
                if (!Utilities.APILib.API.GetUrl(APIType.RANKS_DISCORD, out url, ""))
                    return;
                using (var client = new WebClient())
                {
                    client.DownloadDataCompleted += DownloadDataCompletedTopDiscord;
                    client.DownloadDataAsync(new Uri(url));
                }
                if (!Utilities.APILib.API.GetUrl(APIType.RANKS_PERMIUM, out url, ""))
                    return;
                using (var client = new WebClient())
                {
                    client.DownloadDataCompleted += DownloadDataCompletedVip;
                    client.DownloadDataAsync(new Uri(url));
                }
            }
            else
            {
                SSL.Client.Send(MessageType.CMD_REQUEST_DATA, new RequestData
                {
                    Type = MistakenSocket.Shared.API.DataType.SL_RANK_PREMIUM,
                    argument = null
                }).GetResponseDataCallback((data) =>
                {
                    if (data.Type != ResponseType.OK)
                        return;
                    var ranks = data.Payload.Deserialize<KeyValuePair<string, (string Name, string Color)>[]>(0, 0, out _, false);
                    VipList.Clear();
                    ReservedSlots.Clear();
                    foreach (var rank in ranks)
                    {
                        PlayerInfo playerInfo = new PlayerInfo
                        {
                            UserId = rank.Key,
                            RoleName = rank.Value.Name,
                            RoleColor = rank.Value.Color.ToLower(),
                            RoleType = RoleType.VIP
                        };
                        switch (rank.Value.Name.ToUpper())
                        {
                            case "SAFE":
                                playerInfo.VipLevel = VipLevel.SAFE;
                                break;
                            case "EUCLID":
                                playerInfo.VipLevel = VipLevel.EUCLID;
                                break;
                            case "KETER":
                                playerInfo.VipLevel = VipLevel.KETER;
                                break;
                            case "APOLLYON":
                                playerInfo.VipLevel = VipLevel.APOLLYON;
                                break;
                        }
                        if (HasReservedSlot(playerInfo.VipLevel))
                            ReservedSlots.Add(playerInfo.UserId);
                        if (!VipList.ContainsKey(playerInfo.UserId))
                            VipList.Add(playerInfo.UserId, playerInfo);
                        else if (VipList[playerInfo.UserId].VipLevel < playerInfo.VipLevel)
                            VipList[playerInfo.UserId] = playerInfo;
                    }
                    //return;
#pragma warning disable CS0162 // Wykryto nieosiągalny kod
                    VipList["76561198134629649@steam"] = new PlayerInfo()
                    {
                        UserId = "76561198134629649@steam",
                        RoleColor = "carmine",
                        RoleName = "Apollyon",
                        RoleType = RoleType.VIP,
                        VipLevel = VipLevel.APOLLYON
                    };
#pragma warning restore CS0162 // Wykryto nieosiągalny kod
                });

                SSL.Client.Send(MessageType.CMD_REQUEST_DATA, new RequestData
                {
                    Type = MistakenSocket.Shared.API.DataType.SL_RANK_ACIVE_SL,
                    argument = null
                }).GetResponseDataCallback((data) =>
                {
                    if (data.Type != ResponseType.OK)
                        return;
                    var rank = data.Payload.Deserialize<(string Name, string Color, string[] UserIds)>(0, 0, out _, false);
                    TopSLRolesList.Clear();
                    foreach (var userId in rank.UserIds)
                    {
                        PlayerInfo playerInfo = new PlayerInfo
                        {
                            UserId = userId,
                            RoleName = rank.Name,
                            RoleColor = rank.Color.ToLower(),
                            RoleType = RoleType.TOPSCP
                        };
                        TopSLRolesList.Add(playerInfo.UserId, playerInfo);
                    }
                });

                SSL.Client.Send(MessageType.CMD_REQUEST_DATA, new RequestData
                {
                    Type = MistakenSocket.Shared.API.DataType.SL_RANK_ACIVE_SL_MOST_HOURS,
                    argument = null
                }).GetResponseDataCallback((data) =>
                {
                    if (data.Type != ResponseType.OK)
                        return;
                    var ranks = data.Payload.Deserialize<(string userId, (string Name, string Color) Rank)[]>(0, 0, out _, false);
                    TopMostHoursSLRolesList.Clear();
                    foreach (var rank in ranks)
                    {
                        PlayerInfo playerInfo = new PlayerInfo
                        {
                            UserId = rank.userId,
                            RoleName = rank.Rank.Name,
                            RoleColor = rank.Rank.Color.ToLower(),
                            RoleType = RoleType.TOPHOURSSCP
                        };
                        TopMostHoursSLRolesList.Add((playerInfo.UserId, playerInfo));
                    }
                });

                SSL.Client.Send(MessageType.CMD_REQUEST_DATA, new RequestData
                {
                    Type = MistakenSocket.Shared.API.DataType.SL_RANK_ACIVE_DISCORD,
                    argument = null
                }).GetResponseDataCallback((data) =>
                {
                    if (data.Type != ResponseType.OK)
                        return;
                    var rank = data.Payload.Deserialize<(string Name, string Color, string[] UserIds)>(0, 0, out _, false);
                    TopDscRolesList.Clear();
                    foreach (var userId in rank.UserIds)
                    {
                        PlayerInfo playerInfo = new PlayerInfo
                        {
                            UserId = userId,
                            RoleName = rank.Name,
                            RoleColor = rank.Color.ToLower(),
                            RoleType = RoleType.TOPDISCORD
                        };
                        TopDscRolesList.Add(playerInfo.UserId, playerInfo);
                    }
                });
            }
        }

        private static string GetUserId(string input)
        {
            string uId = input;
            if (!uId.Contains("@"))
            {
                if (uId.Length == 17)
                    uId += "@steam";
                if (uId.Length == 18)
                    uId += "@discord";
            }

            return uId;
        }

        static void DownloadDataCompletedTop10(object sender, DownloadDataCompletedEventArgs e)
        {
            byte[] raw = e.Result;
            string body = Encoding.Default.GetString(raw);
            if (body == "")
                Log.Debug("Top 10 list is empty!");
            else
            {
                string[] poses = body.Split(';');
                TopSLRolesList.Clear();
                foreach (string item in poses)
                {
                    if (item != "")
                    {
                        string[] pos = item.Split(':');
                        PlayerInfo playerInfo = new PlayerInfo
                        {
                            UserId = GetUserId(pos[0]),
                            RoleName = pos[1],
                            RoleColor = pos[2],
                            RoleType = RoleType.TOPSCP,
                            VipLevel = VipLevel.NONE
                        };
                        if (!TopSLRolesList.ContainsKey(playerInfo.UserId))
                            TopSLRolesList.Add(playerInfo.UserId, playerInfo);
                    }
                }
            }
        }
        static void DownloadDataCompletedTopDiscord(object sender, DownloadDataCompletedEventArgs e)
        {
            byte[] raw = e.Result;
            string body = Encoding.Default.GetString(raw);
            if (body == "")
                Log.Debug("Top discord list is empty!");
            else
            {
                string[] poses = body.Split(';');
                TopDscRolesList.Clear();
                foreach (string item in poses)
                {
                    if (item != "")
                    {
                        string[] pos = item.Split(':');
                        PlayerInfo playerInfo = new PlayerInfo
                        {
                            UserId = GetUserId(pos[0]),
                            RoleName = pos[1],
                            RoleColor = pos[2],
                            RoleType = RoleType.TOPDISCORD,
                            VipLevel = VipLevel.NONE
                        };
                        if(!TopDscRolesList.ContainsKey(playerInfo.UserId)) 
                            TopDscRolesList.Add(playerInfo.UserId, playerInfo);
                    }
                }
            }
        }
        static void DownloadDataCompletedVip(object sender, DownloadDataCompletedEventArgs e)
        {
            byte[] raw = e.Result;
            string body = Encoding.Default.GetString(raw);
            VipInfo[] ranks = Newtonsoft.Json.JsonConvert.DeserializeObject<VipInfo[]>(body);
            VipList.Clear();
            ReservedSlots.Clear();
            foreach (var item in ranks)
            {
                PlayerInfo playerInfo = new PlayerInfo
                {
                    UserId = item.userid,
                    RoleName = item.Rank,
                    RoleColor = item.Color.ToLower(),
                    RoleType = RoleType.VIP
                };
                switch (item.Rank.ToUpper())
                {
                    case "SAFE":
                        playerInfo.VipLevel = VipLevel.SAFE;
                        break;
                    case "EUCLID":
                        playerInfo.VipLevel = VipLevel.EUCLID;
                        break;
                    case "KETER":
                        playerInfo.VipLevel = VipLevel.KETER;
                        break;
                    case "APOLLYON":
                        playerInfo.VipLevel = VipLevel.APOLLYON;
                        break;
                }
                if (HasReservedSlot(playerInfo.VipLevel))
                    ReservedSlots.Add(playerInfo.UserId);
                if (!VipList.ContainsKey(playerInfo.UserId))
                    VipList.Add(playerInfo.UserId, playerInfo);
                else if (VipList[playerInfo.UserId].VipLevel < playerInfo.VipLevel)
                    VipList[playerInfo.UserId] = playerInfo;
            }
            /*VipList["76561198134629649@steam"] = new PlayerInfo()
            {
                UserId = "76561198134629649@steam",
                RoleColor = "carmine",
                RoleName = "Apollyon",
                RoleType = RoleType.VIP,
                VipLevel = VipLevel.APOLLYON
            };
            if (HasReservedSlot(VipList["76561198134629649@steam"].VipLevel))
                ReservedSlots.Add("76561198134629649@steam");*/
        }
        
        private static bool HasReservedSlot(VipLevel level)
        {
            switch(level)
            {
                case VipLevel.SAFE:
                case VipLevel.KETER:
                case VipLevel.APOLLYON:
                    return true;
                default:
                    return false;
            }
        }
        internal static void ApplyRoles(Player player, RoleType toshow = RoleType.UNKNOWN)
        {
            PlayerInfo role;
            if (VipList.TryGetValue(player.UserId, out role) && (toshow == RoleType.UNKNOWN || toshow == RoleType.VIP))
            {
                if (role.VipLevel != VipLevel.NONE)
                {
                    player.SendConsoleMessage("You have item " + role.VipLevel, "yellow");
                }
                if (toshow != RoleType.UNKNOWN)
                {
                    player.SendConsoleMessage($"Your role \"{role.RoleName}\" with color {role.RoleColor} has been granted to you.", "cyan");
                    SetRank(player, role.RoleColor, role.RoleName, null);
                    return;
                }
                else
                {
                    player.SendConsoleMessage($"Your role \"{role.RoleName}\" with color {role.RoleColor} has been granted to you.", "cyan");
                    SetRank(player, role.RoleColor, role.RoleName, null);
                }
            }
            else if (TopSLRolesList.TryGetValue(player.UserId, out role) && (toshow == RoleType.UNKNOWN || toshow == RoleType.TOPSCP))
            {
                if (toshow != RoleType.UNKNOWN)
                {
                    player.SendConsoleMessage($"Your role \"{role.RoleName}\" with color {role.RoleColor} has been granted to you.", "cyan");
                    SetRank(player, role.RoleColor, role.RoleName, null);
                    return;
                }
                else
                {
                    player.SendConsoleMessage($"Your role \"{role.RoleName}\" with color {role.RoleColor} has been granted to you.", "cyan");
                    SetRank(player, role.RoleColor, role.RoleName, null);
                }
            }
            else if (TopMostHoursSLRolesList.Any(p => p.Item1 == player.UserId) && (toshow == RoleType.UNKNOWN || toshow == RoleType.TOPHOURSSCP))
            {
                role = TopMostHoursSLRolesList.Find(p => p.Item1 == player.UserId).Item2;
                if (toshow != RoleType.UNKNOWN)
                {
                    player.SendConsoleMessage($"Your role \"{role.RoleName}\" with color {role.RoleColor} has been granted to you.", "cyan");
                    SetRank(player, role.RoleColor, role.RoleName, null);
                    return;
                }
                else
                {
                    player.SendConsoleMessage($"Your role \"{role.RoleName}\" with color {role.RoleColor} has been granted to you.", "cyan");
                    SetRank(player, role.RoleColor, role.RoleName, null);
                }
            }
            else if (TopDscRolesList.TryGetValue(player.UserId, out role) && (toshow == RoleType.UNKNOWN || toshow == RoleType.TOPDISCORD))
            {
                if (toshow != RoleType.UNKNOWN)
                {
                    player.SendConsoleMessage($"Your role \"{role.RoleName}\" with color {role.RoleColor} has been granted to you.", "cyan");
                    SetRank(player, role.RoleColor, role.RoleName, null);
                    return;
                }
                else
                {
                    player.SendConsoleMessage($"Your role \"{role.RoleName}\" with color {role.RoleColor} has been granted to you.", "cyan");
                    SetRank(player, role.RoleColor, role.RoleName, null);
                }
            }

            ApplyDA(player);
        }
        internal static void ApplyStaffRoles(Player player)
        {
            if (!StaffHandler.Staff.Any(i => i.discordid + "@discord" == player.UserId || i.steamid == player.UserId))
                return;
            var rank = StaffHandler.Staff.FirstOrDefault(i => i.discordid + "@discord" == player.UserId || i.steamid == player.UserId);
            SetRank(player, rank.role_color, rank.role, null);
        }
        public static void SetRank(Player player, string Color = null, string Name = null, string Group = null)
        {
            if (Color != null)
                player.RankColor = Color;
            if (Name != null)
                player.RankName = Name;
            if (Group != null)
                player.GroupName = Group;
        }
        internal static void ApplyDA(Player player)
        {
            if (player.IsDev())
            {
                var roles = player.ReferenceHub.serverRoles;

                UserGroup group = new UserGroup
                {
                    RequiredKickPower = 0xFF,
                    KickPower = 0xFF,
                    Permissions = ServerStatic.GetPermissionsHandler().FullPerm,
                    HiddenByDefault = true,
                    BadgeText = roles.MyText,
                    BadgeColor = roles.MyColor,
                    Shared = false
                };
                player.SetRank("dev", group);
                roles.SetGroup(group, false, false, true);

                player.Broadcast("Ranks", 5, "Developer Access Granted", Broadcast.BroadcastFlags.AdminChat);
            }
        }

        public class PlayerInfo
        {
            public string UserId { get; set; }
            public string RoleName { get; set; }
            public string RoleColor { get; set; }
            public RoleType RoleType { get; set; }
            public VipLevel VipLevel { get; set; }
        }
        public enum RoleType
        {
            UNKNOWN = -1,
            TOPSCP = 1,
            TOPDISCORD = 2,
            VIP = 3,
            TOPHOURSSCP = 4,
        }
        public enum VipLevel
        {
            NONE,
            SAFE,
            EUCLID,
            KETER,
            APOLLYON
        }
        public struct VipInfo
        {
            public string userid;
            public string Rank;
            public string Color;
        }
    }
}
