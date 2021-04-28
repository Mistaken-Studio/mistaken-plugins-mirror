using Exiled.API.Features;
using Exiled.API.Interfaces;
using Exiled.Events.EventArgs;
using Gamer.Diagnostics;
using Gamer.Mistaken.Base.Staff;
using Gamer.Mistaken.Utilities.APILib;
using Gamer.Utilities;
using System;
using System.Net;

namespace Gamer.Mistaken.Ban2
{
    public class BansHandler : Diagnostics.Module
    {
        public override bool IsBasic => true;
        public override string Name => nameof(BansHandler);

        public BansHandler(IPlugin<IConfig> plugin) : base(plugin)
        {
        }

        public override void OnEnable()
        {
            Exiled.Events.Handlers.Player.Banning += this.Handle<Exiled.Events.EventArgs.BanningEventArgs>((ev) => Player_Banning(ev));
            Exiled.Events.Handlers.Player.Kicking += this.Handle<Exiled.Events.EventArgs.KickingEventArgs>((ev) => Player_Kicking(ev));
        }

        public override void OnDisable()
        {
            Exiled.Events.Handlers.Player.Banning -= this.Handle<Exiled.Events.EventArgs.BanningEventArgs>((ev) => Player_Banning(ev));
            Exiled.Events.Handlers.Player.Kicking -= this.Handle<Exiled.Events.EventArgs.KickingEventArgs>((ev) => Player_Kicking(ev));
        }

        private void Player_Kicking(KickingEventArgs ev)
        {
            var input = new BanningEventArgs(ev.Target, ev.Issuer, 0, ev.Reason, ev.FullMessage, ev.IsAllowed);
            Player_Banning(input);
            ev.IsAllowed = input.IsAllowed;
        }

        private void Player_Banning(BanningEventArgs ev)
        {
            var duration = ev.Duration;
            duration /= 60;
            Log.Debug("Banning");
            var reason = ev.Reason == "" ? "removeme" : ev.Reason.Trim();

            string textDuration = "KICK";
            if (duration != 0)
            {
                int displayDuration = duration;
                string displayDurationType = "minute";
                if (displayDuration % 60 == 0)
                {
                    displayDuration /= 60;
                    if (displayDuration % 24 == 0)
                    {
                        displayDuration /= 24;
                        if (displayDuration % 365 == 0)
                        {
                            displayDuration /= 365;
                            displayDurationType = "year";

                        }
                        else if (displayDuration % 30 == 0)
                        {
                            displayDuration /= 30;
                            displayDurationType = "month";
                        }
                        else
                            displayDurationType = "day";
                    }
                    else
                        displayDurationType = "hour";
                }
                textDuration = $"{displayDuration} {displayDurationType}" + (displayDuration == 1 ? "" : "s");
            }

            if (ev.Target.IsActiveDev())
                ev.IsAllowed = false;

            if (reason.ToUpper().StartsWith("W:") || reason.ToUpper().StartsWith("R:"))
            {
                string issuer = reason.ToUpper().StartsWith("W:") ? "Wanted Bans System" : "Remote Bans System";
                string message = $"({ev.Target.Id}) {ev.Target.Nickname} has been banned for \"{reason}\" for {textDuration} by (?) {issuer}";
                Log.Info(message);
                MapPlus.Broadcast("BAN", 10, message, Broadcast.BroadcastFlags.AdminChat);
                return;
            }

            try
            {
                string[] tmp = reason.Split(']');
                if (tmp.Length > 1)
                    reason = tmp[1].Trim();
            }
            catch (Exception e)
            {
                Log.Warn(e.Message);
                Log.Warn(e.StackTrace);
            }
            if (reason.ToUpper().StartsWith("TK:"))
            {
                if (duration == 0)
                    MapPlus.Broadcast("BAN", 10, $"{ev.Target.Nickname} został wyrzucony za Zabijanie Sojuszników przez Anty TeamKill System", Broadcast.BroadcastFlags.Normal);
                else
                    MapPlus.Broadcast("BAN", 10, $"{ev.Target.Nickname} został zbanowany za Zabijanie Sojuszników na {textDuration} przez Anty TeamKill System", Broadcast.BroadcastFlags.Normal);
            }
            else
                MapPlus.Broadcast("BAN", 10, $"({ev.Target.Id}) {ev.Target.Nickname} has been banned for \"{reason}\" for {textDuration} by ({ev.Issuer.Id}) {ev.Issuer.Nickname}", Broadcast.BroadcastFlags.AdminChat);


            SendBans(reason, ev);
        }
        public void SendBans(string reason, BanningEventArgs ev)
        {
            string adminUid = ev.Issuer?.UserId ?? "0";
            if (!Utilities.APILib.API.GetUrl(APIType.SEND_BAN, out string url, ev.Target.UserId, adminUid, reason, (ev.Duration).ToString(), ServerConsole.Ip, Server.Port.ToString()))
                return;
            using (var client = new WebClient())
            {
                //Log.Debug(url);
                client.DownloadStringAsync(new Uri(url));
            }
        }
    }
}
