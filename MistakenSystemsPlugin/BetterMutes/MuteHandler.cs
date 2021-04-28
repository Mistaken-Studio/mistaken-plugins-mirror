using Exiled.API.Features;
using Gamer.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Gamer.Mistaken.BetterMutes
{
    public static class MuteHandler
    {
        private static string Path
        {
            get
            {
                return Paths.Configs + "/BetterMutes.txt";
            }
        }

        public static bool GetDuration(string input, out int duration)
        {
            if (input.EndsWith("mo"))
            {
                if (int.TryParse(input.Replace("mo", ""), out duration))
                    duration *= 60 * 24 * 30;
                else
                    return false;
            }
            else if (input.EndsWith("y"))
            {
                if (int.TryParse(input.Replace("y", ""), out duration))
                    duration *= 60 * 24 * 365;
                else
                    return false;
            }
            else if (input.EndsWith("w"))
            {
                if (int.TryParse(input.Replace("w", ""), out duration))
                    duration *= 60 * 24 * 7;
                else
                    return false;
            }
            else if (input.EndsWith("d"))
            {
                if (int.TryParse(input.Replace("d", ""), out duration))
                    duration *= 60 * 24;
                else
                    return false;
            }
            else if (input.EndsWith("h"))
            {
                if (int.TryParse(input.Replace("h", ""), out duration))
                    duration *= 60;
                else
                    return false;
            }
            else if (input.EndsWith("m"))
            {
                if (int.TryParse(input.Replace("m", ""), out duration))
                    duration *= 1;
                else
                    return false;
            }
            else
            {
                if (int.TryParse(input, out duration))
                    duration *= 1;
                else
                    return false;
            }
            return true;
        }

        public static bool Mute(Player player, bool intercomMute, string reason = "removeme", float duration = -1)
        {
            global::MuteHandler.IssuePersistentMute((intercomMute ? "ICOM-" : "") + player.UserId);
            var mute = GetMute(player.UserId);
            if (mute != null)
            {
                if (mute.Intercom && !intercomMute)
                {
                    RemoveMute(player.UserId, true);
                }
                else
                    return false;
            }
            if (intercomMute)
                player.IsIntercomMuted = true;
            else
                player.IsMuted = true;
            bool disconnect = reason.Contains("-dc");
            if (disconnect)
                reason = reason.Replace("-dc", "");
            File.AppendAllLines(Path, new string[]
            {
                JsonConvert.SerializeObject(new MuteData
                {
                    UserId = player.UserId,
                    Reason = reason,
                    EndTime = duration == -1 ? -1 : DateTime.UtcNow.AddMinutes(duration).Ticks,
                    Intercom = intercomMute
                })
            });
            if (!intercomMute && disconnect)
                player.Disconnect("Zostałeś wyciszony");
            return true;
        }

        public static MuteData GetMute(string UserId)
        {
            foreach (var line in File.ReadAllLines(Path))
            {
                var mute = JsonConvert.DeserializeObject<MuteData>(line);
                if (mute.UserId != UserId)
                    continue;

                return mute;
            }

            return null;
        }

        public static bool RemoveMute(string UserId, bool Intercom)
        {
            bool success = false;
            var lines = File.ReadAllLines(Path);
            List<string> toWrite = NorthwoodLib.Pools.ListPool<string>.Shared.Rent(File.ReadAllLines(Path));
            foreach (var line in lines)
            {
                var mute = JsonConvert.DeserializeObject<MuteData>(line);
                if (mute.UserId != UserId)
                    continue;
                if (mute.Intercom != Intercom)
                    continue;
                success = true;
                toWrite.Remove(line);
                if (Intercom)
                    global::MuteHandler.RevokePersistentMute($"ICOM-{mute.UserId}");
                else
                    global::MuteHandler.RevokePersistentMute(mute.UserId);

                if (RealPlayers.List.Any(p => p.UserId == mute.UserId))
                {
                    var player = RealPlayers.List.First(p => p.UserId == mute.UserId);
                    if (mute.Intercom)
                        player.IsIntercomMuted = false;
                    else
                        player.IsMuted = false;
                }
                break;
            }

            File.WriteAllLines(Path, toWrite.ToArray());
            NorthwoodLib.Pools.ListPool<string>.Shared.Return(toWrite);
            return success;
        }

        public static void UpdateMutes()
        {
            foreach (var line in File.ReadAllLines(Path))
            {
                var mute = JsonConvert.DeserializeObject<MuteData>(line);
                if (mute.EndTime == -1)
                    continue;

                Log.Debug($"Mute end check, ends: {new DateTime(mute.EndTime)}, now: {DateTime.UtcNow}");
                if ((new DateTime(mute.EndTime) - DateTime.UtcNow).TotalSeconds < 0)
                {
                    RemoveMute(mute.UserId, mute.Intercom);
                    Log.Info($"Ended mute for {mute.UserId}");
                }
            }
        }

        public static readonly List<MuteData> Mutes = new List<MuteData>();

        public class MuteData
        {
            public string UserId;
            public string Reason;
            public long EndTime;
            public bool Intercom;
        }
    }
}
