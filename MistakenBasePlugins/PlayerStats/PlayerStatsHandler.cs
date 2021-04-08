using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Exiled.API.Features;
using Exiled.API.Interfaces;
using Exiled.Events.EventArgs;
using Gamer.Diagnostics;
using Gamer.Utilities;
using Gamer.Mistaken.Utilities.APILib;
using MEC;

namespace Gamer.Mistaken.Base
{
    public class PlayerStatsHandler : Diagnostics.Module
    {
        public static readonly Dictionary<string, PlayerStats> Stats = new Dictionary<string, PlayerStats>();

        public override string Name => nameof(PlayerStatsHandler);

        public PlayerStatsHandler(IPlugin<IConfig> plugin) : base(plugin)
        {
        }

        public override void OnEnable()
        {
            Exiled.Events.Handlers.Player.Dying += this.Handle<Exiled.Events.EventArgs.DyingEventArgs>((ev) => Player_Dying(ev));
            Exiled.Events.Handlers.Player.Verified += this.Handle<Exiled.Events.EventArgs.VerifiedEventArgs>((ev) => Player_Verified(ev));
            Exiled.Events.Handlers.Server.RoundEnded += this.Handle<Exiled.Events.EventArgs.RoundEndedEventArgs>((ev) => Server_RoundEnded(ev));
            Exiled.Events.Handlers.Player.Destroying += this.Handle<Exiled.Events.EventArgs.DestroyingEventArgs>((ev) => Player_Destroying(ev));
            Exiled.Events.Handlers.Player.Escaping += this.Handle<Exiled.Events.EventArgs.EscapingEventArgs>((ev) => Player_Escaping(ev));
            Exiled.Events.Handlers.Server.RestartingRound += this.Handle(() => Server_RestartingRound(), "RoundRestart");
            Exiled.Events.Handlers.Scp079.GainingExperience += this.Handle<Exiled.Events.EventArgs.GainingExperienceEventArgs>((ev) => Scp079_GainingExperience(ev));
            Exiled.Events.Handlers.Player.Hurting += this.Handle<Exiled.Events.EventArgs.HurtingEventArgs>((ev) => Player_Hurting(ev));
        }

        public override void OnDisable()
        {
            Exiled.Events.Handlers.Player.Dying -= this.Handle<Exiled.Events.EventArgs.DyingEventArgs>((ev) => Player_Dying(ev));
            Exiled.Events.Handlers.Player.Verified -= this.Handle<Exiled.Events.EventArgs.VerifiedEventArgs>((ev) => Player_Verified(ev));
            Exiled.Events.Handlers.Server.RoundEnded -= this.Handle<Exiled.Events.EventArgs.RoundEndedEventArgs>((ev) => Server_RoundEnded(ev));
            Exiled.Events.Handlers.Player.Destroying -= this.Handle<Exiled.Events.EventArgs.DestroyingEventArgs>((ev) => Player_Destroying(ev));
            Exiled.Events.Handlers.Player.Escaping -= this.Handle<Exiled.Events.EventArgs.EscapingEventArgs>((ev) => Player_Escaping(ev));
            Exiled.Events.Handlers.Server.RestartingRound -= this.Handle(() => Server_RestartingRound(), "RoundRestart");
            Exiled.Events.Handlers.Scp079.GainingExperience -= this.Handle<Exiled.Events.EventArgs.GainingExperienceEventArgs>((ev) => Scp079_GainingExperience(ev));
            Exiled.Events.Handlers.Player.Hurting -= this.Handle<Exiled.Events.EventArgs.HurtingEventArgs>((ev) => Player_Hurting(ev));
        }

        private void Player_Hurting(HurtingEventArgs ev)
        {
            if (ev.Target.Team != Team.SCP)
                return;
            if (ev.Attacker.IsHost || !ev.Attacker.IsReadyPlayer())
                return;
            if (!Stats.ContainsKey(ev.Attacker.UserId))
                return;
            try
            {
                Stats[ev.Attacker.UserId].DmgToSCP += ev.Amount;
            }
            catch (NullReferenceException e)
            {
                Log.Error("NullReferenceException handled. ps==null|" + (Stats == null));
                Log.Error("StackTrace:" + e.StackTrace);
            }
            catch (Exception e)
            {
                Log.Error("Error:" + e.Message);
                Log.Error("StackTrace:" + e.StackTrace);
            }
        }

        private void Scp079_GainingExperience(GainingExperienceEventArgs ev)
        {
            try
            {
                switch (ev.GainType)
                {
                    case ExpGainType.KillAssist:
                    case ExpGainType.DirectKill:
                    case ExpGainType.PocketAssist:
                        {
                            Stats[ev.Player.UserId].Kills++;
                            break;
                        }
                }
            }
            catch (NullReferenceException e)
            {
                Log.Error("NullReferenceException handled. ps==null|" + (Stats == null));
                Log.Error("StackTrace:" + e.StackTrace);
            }
            catch (Exception e)
            {
                Log.Error("Error:" + e.Message);
                Log.Error("StackTrace:" + e.StackTrace);
            }
        }

        private void Server_RestartingRound()
        {
            PluginHandler.EventOnGoing = false;
        }

        private void Player_Escaping(EscapingEventArgs ev)
        {
            try
            {
                if (ev.IsAllowed)
                    Stats[ev.Player.UserId].Escapes++;
            }
            catch { }
        }

        private void Player_Destroying(DestroyingEventArgs ev)
        {
            try
            {
                Stats[ev.Player.UserId].Leavetime = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                if (ev.Player.Role != RoleType.Spectator)
                    Stats[ev.Player.UserId].Deaths++;
            }
            catch { }
        }

        private void Server_RoundEnded(RoundEndedEventArgs ev)
        {
            try
            {
                if (Stats == null)
                {
                    Log.Warn("Error|ps == null");
                    return;
                }
                Log.Info("Round ended");

                foreach (PlayerStats stat in Stats.Values.ToArray())
                {
                    try
                    {
                        if (stat == null)
                        {
                            Log.Info("_ps == null");
                            return;
                        }
                        if (stat.DNT)
                        {
                            Log.Info("Player stats with DNT! Skipping");
                            Forget(stat.UserId);
                        }
                        else
                        {
                            if (stat.Leavetime == null) 
                                stat.Leavetime = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                            SendStats(stat.UserId, stat.Kills, stat.Deaths, (Int32)(stat.Leavetime - stat.Jointime), stat.Tk_kills, stat.Tk_deaths, stat.Escapes); 
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Info("Error during sending stats: " + e.Message);
                        Log.Info("Stacktrace: " + e.StackTrace);
                    }
                }
                Stats.Clear();
            }
            catch (Exception e)
            {
                Log.Warn("Error during sending: " + e.Message);
                Log.Warn("Stacktrace: " + e.StackTrace);
            }
        }

        private void Player_Verified(VerifiedEventArgs ev)
        {
            if (!Stats.TryGetValue(ev.Player.UserId, out PlayerStats stats))
            {
                Stats.Add(ev.Player.UserId, new PlayerStats
                {
                    Nickname = ev.Player.Nickname,
                    Ip = ev.Player.IPAddress,
                    Jointime = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds,
                    UserId = ev.Player.UserId,
                    Deaths = 0,
                    Kills = 0,
                    Escapes = 0,
                    Tk_deaths = 0,
                    Tk_kills = 0,
                    DNT = ev.Player.DoNotTrack,
                });
            }
            else
                stats.Leavetime = null;


            try
            {
                stats = Stats[ev.Player.UserId];
                stats.Nickname = ev.Player.Nickname;
                stats.UserId = ev.Player.UserId;
                stats.DNT = ev.Player.DoNotTrack;
                stats.Ip = ev.Player.IPAddress;
            }
            catch
            {
            }
        }

        private void Player_Dying(DyingEventArgs ev)
        {
            if (PluginHandler.EventOnGoing) 
                return;
            if (ev.Target.Role == RoleType.Spectator)
                return;
            //Log.Debug(ev.Target.Nickname + ":" + ev.Killer.Nickname);
            bool tk = AntyTeamKillHandler.IsTeamkill(ev.Killer, ev.Target);
            if (!string.IsNullOrEmpty(ev.Target?.UserId) && !string.IsNullOrEmpty(ev.Killer?.UserId))
            {
                try
                {
                    if (Stats.ContainsKey(ev.Killer.UserId))
                    {
                        if (ev.Target != ev.Killer)
                        {

                            Stats[ev.Killer.UserId].Kills++;
                            if (tk) Stats[ev.Killer.UserId].Tk_kills++;
                        }
                    }
                    if (Stats.ContainsKey(ev.Killer.UserId))
                    {
                        Stats[ev.Target.UserId].Deaths++;
                        if (tk)
                            Stats[ev.Target.UserId].Tk_deaths++;
                    }
                }
                catch 
                { 
                    Log.Error("Error"); 
                }
            }
            if (ev.HitInformation.GetDamageType() == DamageTypes.Pocket || ev.Target.Position.y < -1800)
            {
                foreach (var player in RealPlayers.Get(RoleType.Scp106))
                    if(Stats.ContainsKey(player.UserId)) Stats[player.UserId].Kills++;
            }

            Logger.Debug("DEATH", $"{ev.Target.Nickname} ({ev.Target.Role}) was killed by {ev.Killer?.Nickname ?? "WORLD"} ({ev.Killer?.Role.ToString() ?? "WORLD"}) using {ev.HitInformation.GetDamageName()}");
        }

        public void SendStats(string userid, uint kills, uint deaths, Int32 time, uint tk_kills, uint tk_deaths, uint escapes)
        {
            if (PluginHandler.EventOnGoing)
            {
                Log.Info("Cancled sending stats|EventOnGoing");
                return;
            }
            if (time > 60 * 60 * 2)
            {
                Log.Warn("To much time. Max is 2 h");
                return;
            }
            Log.Info("Sending stats");
            //StatsLog
            /*{
                string path = $"{Paths.Configs}/StatsLog_{Server.Port}.txt";
                if (!File.Exists(path))
                    File.Create(path).Close();
                File.AppendAllLines(path, new string[] { $"{userid} | {kills - tk_kills}({kills}|{tk_kills}) | {deaths - tk_deaths}({deaths}|{tk_deaths}) | {time} | {escapes}" });
            }*/
            if (deaths - tk_deaths >= 100)
            {
                Log.Error("Emergency stats cancel!! +100 deaths");
                Stats.Remove(userid);
                return;
            }
            if (kills - tk_kills >= 100)
            {
                Log.Error("Emergency stats cancel!! +100 kils");
                Stats.Remove(userid);
                return;
            }
            using (var client = new WebClient())
            {
                if (!Utilities.APILib.API.GetUrl(APIType.SEND_STATS, out string url, userid, ((byte)PluginHandler.Config.RankingType).ToString(), (kills - tk_kills).ToString(), (deaths - tk_deaths).ToString(), time.ToString(), tk_kills.ToString(), tk_deaths.ToString(), escapes.ToString())) 
                    return;
                //Log.Debug(url);
                client.DownloadStringAsync(new Uri(url));
                Stats.Remove(userid);
            }
        }

        public void Forget(string userId)
        {
            if (!Utilities.APILib.API.GetUrl(APIType.FORGET, out string url, userId)) 
                return;
            using (var client = new WebClient())
            {
                client.DownloadStringAsync(new Uri(url));
            }
        }

        public class PlayerStats
        {
            public string Nickname { get; set; }
            public string UserId { get; set; }
            public string Ip { get; set; }
            public Int32? Jointime { get; set; }
            public Int32? Leavetime { get; set; }
            public uint Kills { get; set; }
            public uint Deaths { get; set; }
            public uint Tk_kills { get; set; }
            public uint Tk_deaths { get; set; }
            public uint Escapes { get; set; }
            public float DmgToSCP { get; set; }
            public bool DNT { get; set; }
        }
    }
}
