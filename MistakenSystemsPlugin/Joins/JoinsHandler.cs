using Exiled.API.Features;
using Exiled.API.Interfaces;
using Gamer.Diagnostics;
using Gamer.Mistaken.Utilities.APILib;
using Gamer.RoundLoggerSystem;
using Gamer.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Gamer.Mistaken.Joins
{
    public class JoinsHandler : Diagnostics.Module
    {
        public override bool IsBasic => true;
        public static WantedData[] Wanteds = new WantedData[0];
        public readonly static HashSet<string> WantedUserIds = new HashSet<string>();

        private new static __Log Log;
        public JoinsHandler(IPlugin<IConfig> plugin) : base(plugin)
        {
            Log = base.Log;
        }

        public override string Name => nameof(JoinsHandler);

        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.RestartingRound += this.Handle(() => Server_RestartingRound(), "RoundRestart");
            Exiled.Events.Handlers.Player.Verified += this.Handle<Exiled.Events.EventArgs.VerifiedEventArgs>((ev) => Player_Verified(ev));
        }

        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.RestartingRound -= this.Handle(() => Server_RestartingRound(), "RoundRestart");
            Exiled.Events.Handlers.Player.Verified -= this.Handle<Exiled.Events.EventArgs.VerifiedEventArgs>((ev) => Player_Verified(ev));
        }

        private void Server_RestartingRound()
        {
            GetWanteds();
        }

        private void Player_Verified(Exiled.Events.EventArgs.VerifiedEventArgs ev)
        {
            LogPlayer(ev.Player);
            CheckForWanted(ev.Player);
        }

        internal static void CheckForWanted()
        {
            foreach (var item in RealPlayers.List)
            {
                CheckForWanted(item);
            }
        }

        internal static void CheckForWanted(Player player)
        {
            if (player == null)
            {
                Log.Error("Player is null");
                return;
            }
            if (Wanteds.Length == 0)
                GetWanteds();
            if (WantedUserIds.Contains(player.UserId))
            {
                var wanted = Wanteds.First(i => i.PlayerUserId == player.UserId);
                Log.Info("Detected Player With Active Wanted Flag");
                try
                {
                    string textTime = "KICK";
                    if (wanted.Duration != 0)
                    {
                        string durType = "minute";
                        int duration = wanted.Duration;
                        if (duration % 60 == 0)
                        {
                            duration /= 60;
                            if (duration % 24 == 0)
                            {
                                duration /= 24;
                                if (duration % 365 == 0)
                                {
                                    duration /= 365;
                                    durType = "year";

                                }
                                else if (duration % 30 == 0)
                                {
                                    duration /= 30;
                                    durType = "month";
                                }
                                else
                                    durType = "day";
                            }
                            else
                                durType = "hour";
                        }
                        textTime = $"BAN: {duration} {durType}" + (duration == 1 ? "" : "s");
                    }
                    Server.BanPlayer.BanUser(player.GameObject, wanted.Duration * 60, $"W: [{textTime}] {wanted.Reason}", $"Wanted: {wanted.AdminUserId}", false);
                    BanHandler.ValidateBans();
                    var banData = BanHandler.QueryBan(player.UserId, player.IPAddress);
                    if (banData.Key == null)
                        Log.Error("FAILED TO BAN PLAYER ON USERID");
                    if (banData.Value == null)
                        Log.Error("FAILED TO BAN PLAYER ON IP");

                    SendBans("W: " + wanted.Reason, wanted.PlayerUserId, wanted.AdminUserId, wanted.Duration);
                    RoundLogger.Log("WANTED", "BAN", $"{player.PlayerToString()} was banned because of wanted");
                    wanted.Remove();
                }
                catch (Exception e)
                {
                    Log.Error(e.Message);
                    Log.Error(e.StackTrace);
                }
            }
            //else
            //    Log.Info("Player has no Active Wanted Flag");
        }

        internal static void SendBans(string reason, string playeruid, string adminuid, int duration)
        {
            using (var client = new WebClient())
            {
                if (!Utilities.APILib.API.GetUrl(APIType.SEND_BAN, out string url, playeruid, adminuid, reason, (duration * 60).ToString(), ServerConsole.Ip, Server.Port.ToString()))
                    return;
                client.DownloadStringAsync(new System.Uri(url));
            }
        }

        public void LogPlayer(Player player)
        {
            using (var client = new WebClient())
            {
                int serverId = Server.Port - 7776;
                if (!Utilities.APILib.API.GetUrl(APIType.LOG_PLAYER, out string url, serverId.ToString(), player.UserId, ServerConsole.Ip))
                    return;
                try
                {
                    client.DownloadDataAsync(new System.Uri(url));
                }
                catch (System.UriFormatException)
                {
                    Log.Error("ERROR WITH URI FORMAT: \n" + url);
                }
            }
        }

        public static void GetWanteds()
        {
            if (!Utilities.APILib.API.GetUrl(APIType.WANTED_GET, out string url, ""))
                return;
            using (var client = new WebClient())
            {
                client.DownloadDataCompleted += GetWantedsCompleted;
                client.DownloadDataAsync(new System.Uri(url));
            }
        }

        private static void GetWantedsCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Log.Error("Error donwloading bans");
                Log.Error(e.Error.Message);
                Log.Error(e.Error.StackTrace);
                return;
            }
            string body = Encoding.Default.GetString(e.Result);
            if (string.IsNullOrWhiteSpace(body))
                return;
            WantedUserIds.Clear();
            Wanteds = Newtonsoft.Json.JsonConvert.DeserializeObject<WantedData[]>(body);
            CheckForWanted();
        }

        public class WantedData
        {
            private string pUId = "";
            [Newtonsoft.Json.JsonProperty("usersid")]
            public string PlayerUserId
            {

                get
                {
                    return pUId;
                }
                set
                {
                    pUId = value;
                    WantedUserIds.Add(pUId);
                }
            }
            [Newtonsoft.Json.JsonProperty("adminsid")]
            public string AdminUserId { get; set; }
            [Newtonsoft.Json.JsonProperty("reason")]
            public string Reason { get; set; }
            [Newtonsoft.Json.JsonProperty("duration")]
            public int Duration { get; set; }

            public void Remove()
            {
                if (!Utilities.APILib.API.GetUrl(APIType.WANTED_REMOVE, out string url, PlayerUserId))
                    return;
                using (var client = new WebClient())
                {
                    client.DownloadDataCompleted += (_, __) =>
                    {
                        GetWanteds();
                    };
                    client.DownloadDataAsync(new System.Uri(url));
                }
            }
        }
    }
}
