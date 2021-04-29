using Exiled.API.Features;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;

namespace Gamer.Mistaken.Systems.ThreatLevel
{
    public class ThreadLevelData
    {
        public string UserId;
        public int Level;
        public int Num;
        public string[] Response;
    }

    public static class ThreatLevelManager
    {
        public static void GetThreatLevel(Player player, Action<ThreadLevelData> Callback)
        {
            GetThreatLevel(player.UserId, Callback, false, "", "NULL");
        }
        public static void GetThreatLevel(string UserId, Action<ThreadLevelData> Callback, bool warningFlag, string warningFlagReason, string CountryCode)
        {
            int num = 0;
            int level = 0;
            List<string> tor = NorthwoodLib.Pools.ListPool<string>.Shared.Rent();

            if (warningFlag)
            {
                tor.Add($"<color=black>Active Warning Flag: {warningFlagReason}</color>");
                if (level < 4)
                    level = 4;
                num++;
            }

            /*if (CheckBannedIp(player, out string ban))
            {
                tor.Add($"<color=yellow>Simmilar IP to ban:\n {ban}</color>");
                if (level < 1) level = 1;
                num++;
            }*/

            if (CountryCode.ToUpper() != "PL")
            {
                tor.Add($"<color=yellow>Auth is not PL</color>");
                if (level < 1) level = 1;
                num++;
            }

            DonwloadPlayerInfo(UserId, (PlayerInfo info) =>
            {
                if (info != null)
                {
                    if (info.NoCommunityProfile())
                    {
                        tor.Add($"<color=red>Player didn't fill community profile</color>");
                        if (level < 3) level = 3;
                        num++;
                    }

                    if (info.IsNewAcount())
                    {
                        tor.Add($"<color=orange>Profile is new (less than 14 days old)</color>");
                        if (level < 2) level = 2;
                        num++;
                    }

                    if (info.NoGames() && false)
                    {
                        tor.Add($"<color=orange>Profile does not contain any games</color>");
                        if (level < 2) level = 2;
                        num++;
                    }

                    if (info.NotPL())
                    {
                        tor.Add($"<color=white>Profile is not PL</color>");
                        num++;
                    }

                    if (info.IsProfilePrivate)
                    {
                        tor.Add($"<color=orange>Profile is private</color>");
                        if (level < 1) level = 1;
                        num++;
                    }

                    if (info.Banned(out int lvl, out int amount))
                    {
                        tor.Add("<color=" + (lvl == 3 ? "red" : (lvl == 2 ? "orange" : (lvl == 1 ? "yellow" : "white"))) + ">Profile has active <color=red>VAC</color>/<color=red>Game</color>/<color=orange>Economy</color>/<color=orange>Community</color> ban</color>");
                        if (level < lvl) level = lvl;
                        num += amount;
                    }
                }
                var bans = Bans.BansManager.GetBans(UserId);
                if (bans.Length > 20)
                {
                    tor.Add($"<color=red>Has more than 20 bans</color>");
                    if (level < 3) level = 3;
                    num++;
                }
                else if (bans.Length > 10)
                {
                    tor.Add($"<color=orange>Has more than 10 bans</color>");
                    if (level < 2) level = 2;
                    num++;
                }
                else if (bans.Length > 5)
                {
                    tor.Add($"<color=yellow>Has more than 5 bans</color>");
                    if (level < 1) level = 1;
                    num++;
                }
                //Logger.Debug("TLM.Bans", $"{UserId}: {bans.Length}");
                Callback.Invoke(new ThreadLevelData() { UserId = UserId, Level = level, Num = num, Response = tor.ToArray() });
                NorthwoodLib.Pools.ListPool<string>.Shared.Return(tor);
            });
        }

        private static void DonwloadPlayerInfo(string UserId, Action<PlayerInfo> CallBack)
        {
            if (!UserId.EndsWith("@steam"))
            {
                CallBack.Invoke(null);
                return;
            }
            if (!Gamer.Mistaken.Utilities.APILib.API.GetSteamAPIKey(out string steamKey))
                return;
            using (var client = new WebClient())
            {
                try
                {
                    client.DownloadDataCompleted += (object _, DownloadDataCompletedEventArgs data) =>
                    {
                        try
                        {
                            var msg = System.Text.Encoding.UTF8.GetString(data.Result);
                            var pinfo = new PlayerInfo();
                            pinfo.AddInfo(PlayerInfo.DataType.NORMAL, msg);
                            using (var clientv2 = new WebClient())
                            {
                                clientv2.DownloadDataCompleted += (object __, DownloadDataCompletedEventArgs datav2) =>
                                {
                                    var msgv2 = System.Text.Encoding.UTF8.GetString(datav2.Result);
                                    pinfo.AddInfo(PlayerInfo.DataType.GAMES, msgv2);
                                    using (var clientv4 = new WebClient())
                                    {
                                        clientv4.DownloadDataCompleted += (object ____, DownloadDataCompletedEventArgs datav4) =>
                                        {
                                            var msgv4 = System.Text.Encoding.UTF8.GetString(datav4.Result);
                                            pinfo.AddInfo(PlayerInfo.DataType.BANS, msgv4);
                                            CallBack.Invoke(pinfo);
                                        };
                                        clientv4.DownloadDataAsync(new Uri($"http://api.steampowered.com/ISteamUser/GetPlayerBans/v0001/?key=$key&steamids={UserId.Split('@')[0]}".Replace("$key", steamKey)));
                                    }
                                };
                                clientv2.DownloadDataAsync(new Uri($"http://api.steampowered.com/IPlayerService/GetOwnedGames/v0001/?key=$key&steamid={UserId.Split('@')[0]}".Replace("$key", steamKey)));
                            }
                        }
                        catch (System.Exception e)
                        {
                            Log.Error(e.Message);
                            Log.Error(e.StackTrace);
                        }
                    };
                    client.DownloadDataAsync(new Uri($"https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key=$key&steamids={UserId.Split('@')[0]}".Replace("$key", steamKey)));
                }
                catch (System.Exception e)
                {
                    Log.Error(e.Message);
                    Log.Error(e.StackTrace);
                }
            }
        }

        private class PlayerInfo
        {
            public bool IsNewAcount()
            {
                if (IsProfilePrivate) return false;
                return (DateTime.Now - new DateTime(Created)).TotalDays < 13;
            }

            public bool NoCommunityProfile()
            {
                return !CommunityProfile;
            }

            public bool NoGames()
            {
                if (IsProfilePrivate) return false;
                return Games == 0;
            }

            public bool NotPL()
            {
                if (IsProfilePrivate) return false;
                if (CountryCode == null) return false;
                return CountryCode.ToLower() != "pl";
            }

            public bool Banned(out int lvl, out int num)
            {
                num = 0;
                lvl = 0;
                num += NumberOfGameBans;
                num += NumberOfVACBans;
                if (VACBanned)
                    lvl = 3;
                if (NumberOfGameBans > 0)
                    lvl = 3;
                if (EconomyBan != "none")
                {
                    lvl = 2;
                    num++;
                }
                if (CommunityBanned)
                {
                    lvl = 2;
                    num++;
                }
                if (lvl != 0 && DaysSinceLastBan > 100)
                    lvl = 2;
                if (lvl != 0) return true;
                else return false;
            }

            internal enum DataType
            {
                NORMAL,
                GAMES,
                FRIENDS,
                BANS
            }

            public void AddInfo(DataType type, string data)
            {
                switch (type)
                {
                    case DataType.NORMAL:
                        {
                            var tmp = JsonConvert.DeserializeObject<NormalInfo>(data);
                            if (tmp.response.players.Length == 0)
                                break;
                            CountryCode = tmp.response.players[0].loccountrycode;
                            Created = tmp.response.players[0].timecreated;
                            CommunityProfile = tmp.response.players[0].profilestate == 1;
                            IsProfilePrivate = tmp.response.players[0].communityvisibilitystate != 3;
                            break;
                        }
                    case DataType.GAMES:
                        {
                            var tmp = JsonConvert.DeserializeObject<Data<GameInfo>>(data);
                            Games = tmp.response.game_count;
                            break;
                        }
                    case DataType.BANS:
                        {
                            var tmp = JsonConvert.DeserializeObject<BansInfo>(data);
                            if (tmp.players.Length == 0)
                                break;
                            CommunityBanned = tmp.players[0].CommunityBanned;
                            VACBanned = tmp.players[0].VACBanned;
                            EconomyBan = tmp.players[0].EconomyBan;
                            NumberOfGameBans = tmp.players[0].NumberOfGameBans;
                            NumberOfVACBans = tmp.players[0].NumberOfVACBans;
                            DaysSinceLastBan = tmp.players[0].DaysSinceLastBan;
                            break;
                        }
                }
            }

            [System.Serializable]
            private class NormalInfo
            {
                [System.Serializable]
                public class Yes
                {
                    [System.Serializable]
                    public class Tmp
                    {
                        [JsonProperty]
                        public string steamid = default;
                        [JsonProperty]
                        public string loccountrycode = default;
                        [JsonProperty]
                        public long timecreated = default;
                        [JsonProperty]
                        public short profilestate = default;
                        [JsonProperty]
                        public short communityvisibilitystate = default;
                        [JsonProperty]
                        public string personaname = default;
                        [JsonProperty]
                        public short personastate = default;
                        [JsonProperty]
                        public short personastateflags = default;
                    }
                    [JsonProperty]
                    public Tmp[] players = default;

                }
                [JsonProperty]
                public Yes response = default;
            }

            public string CountryCode = "";
            public long Created = 0;
            public bool CommunityProfile = false;

            [System.Serializable]
            private class GameInfo
            {
                [System.Serializable]
                public class Tmp
                {
                    [JsonProperty]
                    public int appid = default;
                    [JsonProperty]
                    public int playtime_forever = default;
                    [JsonProperty]
                    public int playtime_windows_forever = default;
                    [JsonProperty]
                    public int playtime_mac_forever = default;
                    [JsonProperty]
                    public int playtime_linux_forever = default;
                }

                [JsonProperty]
                public int game_count = default;
                [JsonProperty]
                public Tmp[] games = default;
            }

            public int Games = 0;

            [System.Serializable]
            private class BansInfo
            {
                [JsonProperty]
                public Tmp[] players = default;

                [System.Serializable]
                public class Tmp
                {
                    [JsonProperty]
                    public bool CommunityBanned = default;
                    [JsonProperty]
                    public bool VACBanned = default;
                    [JsonProperty]
                    public int NumberOfVACBans = default;
                    [JsonProperty]
                    public int DaysSinceLastBan = default;
                    [JsonProperty]
                    public int NumberOfGameBans = default;
                    [JsonProperty]
                    public string EconomyBan = default;
                    [JsonProperty]
                    public string SteamId = default;
                }
            }

            public bool CommunityBanned;
            public bool VACBanned;
            public int NumberOfVACBans;
            public int DaysSinceLastBan;
            public int NumberOfGameBans;
            public string EconomyBan;
            public bool IsProfilePrivate;

            [System.Serializable]
            private class Data<T>
            {
                [JsonProperty]
                public T response = default;
            }
        }
    }
}
