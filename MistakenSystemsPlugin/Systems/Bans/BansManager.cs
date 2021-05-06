using Gamer.Mistaken.Utilities.APILib;
using MistakenSocket.Client.SL;
using MistakenSocket.Shared;
using MistakenSocket.Shared.API;
using MistakenSocket.Shared.ClientToCentral;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Gamer.Mistaken.Systems.Bans
{
    public static class BansManager
    {
        private static readonly Dictionary<string, (string AdminId, string Reason, int Duration, DateTime Time)[]> BansCache = new Dictionary<string, (string AdminId, string Reason, int Duration, DateTime Time)[]>();

        public static void ClearCache()
        {
            BansCache.Clear();
        }
        private static readonly HashSet<string> Requesting = new HashSet<string>();
        public static (string AdminId, string Reason, int Duration, DateTime Time)[] GetBans(string userId)
        {
            if (BansCache.TryGetValue(userId, out (string AdminId, string Reason, int Duration, DateTime Time)[] tor))
                return tor;
            GetBansFromDB(userId);
            return new (string AdminId, string Reason, int Duration, DateTime Time)[0];
        }

        private static void GetBansFromDB(string userId)
        {
            if (Requesting.Contains(userId))
                return;
            Requesting.Add(userId);
            Gamer.Utilities.BetterCourotines.CallDelayed(15, () => Requesting.Remove(userId), "BansManager.GetBansFromDB");
            if (PluginHandler.IsSSLSleepMode)
            {
                using (var client = new WebClient())
                {
                    client.DownloadDataCompleted += GetBansCompleted;
                    if (!Gamer.Mistaken.Utilities.APILib.API.GetUrl(APIType.GET_BANS, out string url, userId))
                        return;
                    client.QueryString.Add("userId", userId);
                    client.DownloadDataAsync(new Uri(url), userId);
                }
                return;
            }
            SSL.Client.Send(MessageType.CMD_REQUEST_DATA, new RequestData
            {
                Type = MistakenSocket.Shared.API.DataType.SL_BANS,
                argument = userId.Serialize(false)
            }).GetResponseDataCallback((data) =>
            {
                if (data.Type != MistakenSocket.Shared.API.ResponseType.OK)
                    return;
                if (BansCache.ContainsKey(userId))
                    BansCache.Remove(userId);
                BansCache.Add(userId, data.Payload.Deserialize<(string AdminId, string Reason, int Duration, DateTime Time)[]>(0, 0, out _, false));
                Requesting.Remove(userId);
            });
        }

        private static void GetBansCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            try
            {
                string userId = ((WebClient)sender).QueryString["userId"];
                string body = Encoding.Default.GetString(e.Result);
                var data = JsonConvert.DeserializeObject<BansObject[]>(body);

                BansCache[userId] = data.Select(i => (i.AdminUserId, i.Reason, i.Duration, i.Time)).ToArray();
                Requesting.Remove(userId);
            }
            catch
            {
                Exiled.API.Features.Log.Error("Failed to download bans");
            }
        }

        public class BansObject
        {
            [JsonProperty("id")]
            public int Id;
            [JsonProperty("userid")]
            public string UserId;
            [JsonProperty("adminsid")]
            public string AdminUserId;
            [JsonProperty("reason")]
            public string Reason;
            [JsonProperty("duration")]
            public int Duration;
            [JsonProperty("time")]
            public string TimeString;
            [JsonProperty("server")]
            public string Server;

            private DateTime _time;
            public DateTime Time
            {
                get
                {
                    if (_time != null)
                        return _time;
                    var tmp = TimeString.Split(' ');
                    var date = tmp[0].Split('-');
                    var time = tmp[1].Split(':');
                    _time = new DateTime(int.Parse(date[0]), int.Parse(date[1]), int.Parse(date[2]), int.Parse(time[0]), int.Parse(time[1]), int.Parse(time[2]));
                    return _time;
                }
            }
        }
    }
}
