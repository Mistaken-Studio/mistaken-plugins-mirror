using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Exiled.API.Features;
using Gamer.Utilities;
using Gamer.Mistaken.Utilities.APILib;
using Gamer.Diagnostics;
using MEC;
using Gamer.RoundLoggerSystem;
using UnityEngine;
using Grenades;
using Exiled.API.Extensions;
using UnityEngine.Assertions.Must;

namespace Gamer.Mistaken.Base
{
    public class AntyTeamKillHandler : Diagnostics.Module
    {
        //public override bool IsBasic => true;
        public static int CurrentRoundId = 0;
        public static readonly Dictionary<int, List<TeamKill>> TeamKills = new Dictionary<int, List<TeamKill>>();
        public static List<TeamKill> TeamKillsList
        {
            get
            {
                var tor = new List<TeamKill>();
                foreach (var item in TeamKills.Values)
                {
                    tor.AddRange(item);
                }
                return tor;
            }
        }
        public class TeamKill
        {
            public int RoundId;
            public Player Killer;
            public Player Victim;
            public PlayerStats.HitInfo Info;

            private TeamKill(Exiled.Events.EventArgs.DyingEventArgs ev, string killerUserId)
            {
                RoundId = CurrentRoundId;
                Killer = ev.Killer;
                Victim = ev.Target;
                Info = ev.HitInformation;

                TeamKills[RoundId].Add(this);

                try
                {
                    Instance.OnTeamKill(killerUserId ?? Killer.UserId, Victim, Info.GetDamageType(), Info.Amount);
                }
                catch (System.Exception ex)
                {
                    Log.Error(ex.Message);
                    Log.Error(ex.StackTrace);
                }
                if (killerUserId != null)
                {
                    Instance.SendTK(killerUserId, Team.RIP, Victim);
                }
                else
                {
                    Instance.SendTK(Killer, Victim);
                }
            }

            public static void Create(Exiled.Events.EventArgs.DyingEventArgs ev, string killerUserId)
            {
                new TeamKill(ev, killerUserId);
            }
        }

        public static readonly Dictionary<int, List<TeamAttack>> TeamAttacks = new Dictionary<int, List<TeamAttack>>();
        public static List<TeamAttack> TeamAttacksList
        {
            get
            {
                var tor = new List<TeamAttack>();
                foreach (var item in TeamAttacks.Values)
                {
                    tor.AddRange(item);
                }
                return tor;
            }
        }

        public class TeamAttack
        {
            public int RoundId;
            public Player Killer;
            public Player Victim;
            public PlayerStats.HitInfo Info;

            private TeamAttack(Exiled.Events.EventArgs.HurtingEventArgs ev, string attackerUserId)
            {
                RoundId = CurrentRoundId;
                Killer = ev.Attacker;
                Victim = ev.Target;
                Info = ev.HitInformations;

                TeamAttacks[RoundId].Add(this);

                Instance.OnTeamAttack(attackerUserId ?? Killer.UserId, Victim, Info.GetDamageType(), Info.Amount);
            }

            public static void Create(Exiled.Events.EventArgs.HurtingEventArgs ev, string attackerUserId)
            {
                if (!Round.IsStarted)
                    return;
                new TeamAttack(ev, attackerUserId);
            }
        }

        public static readonly HashSet<KeyValuePair<Team, Team>> TeamKillTeams = new HashSet<KeyValuePair<Team, Team>>()
        {
            new KeyValuePair<Team, Team>(Team.CHI, Team.CHI),
            new KeyValuePair<Team, Team>(Team.CHI, Team.CDP),
            new KeyValuePair<Team, Team>(Team.CDP, Team.CHI),
            new KeyValuePair<Team, Team>(Team.MTF, Team.MTF),
            new KeyValuePair<Team, Team>(Team.MTF, Team.RSC),
            new KeyValuePair<Team, Team>(Team.RSC, Team.MTF),
            new KeyValuePair<Team, Team>(Team.RSC, Team.RSC),
            new KeyValuePair<Team, Team>(Team.SCP, Team.SCP),
        };

        public static readonly Dictionary<string, int> TeamKillsCounter = new Dictionary<string, int>();

        private static AntyTeamKillHandler Instance;

        public override string Name => nameof(AntyTeamKillHandler);
        private new static __Log Log;
        public AntyTeamKillHandler(PluginHandler p) : base(p)
        {
            Instance = this;
            Log = base.Log;
        }

        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.RestartingRound += this.Handle(() => Server_RestartingRound(), "RoundRestart");
            Exiled.Events.Handlers.Player.Dying += this.Handle<Exiled.Events.EventArgs.DyingEventArgs>((ev) => Player_Dying(ev));
            Exiled.Events.Handlers.Player.Hurting += this.Handle<Exiled.Events.EventArgs.HurtingEventArgs>((ev) => Player_Hurting(ev));
            Exiled.Events.Handlers.Map.ExplodingGrenade += this.Handle<Exiled.Events.EventArgs.ExplodingGrenadeEventArgs>((ev) => Map_ExplodingGrenade(ev));
        
            Server_RestartingRound();
        }

        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.RestartingRound -= this.Handle(() => Server_RestartingRound(), "RoundRestart");
            Exiled.Events.Handlers.Player.Dying -= this.Handle<Exiled.Events.EventArgs.DyingEventArgs>((ev) => Player_Dying(ev));
            Exiled.Events.Handlers.Player.Hurting -= this.Handle<Exiled.Events.EventArgs.HurtingEventArgs>((ev) => Player_Hurting(ev));
            Exiled.Events.Handlers.Map.ExplodingGrenade -= this.Handle<Exiled.Events.EventArgs.ExplodingGrenadeEventArgs>((ev) => Map_ExplodingGrenade(ev));
        }
        public const bool Debug = true;

        public readonly static Dictionary<string, Player[]> TKGreneadedPlayers = new Dictionary<string, Player[]>();
        private void Map_ExplodingGrenade(Exiled.Events.EventArgs.ExplodingGrenadeEventArgs ev)
        {
            if (!ev.IsAllowed)
            {
                Log.Debug("ATK Skip Code: 2", Debug);
                return;
            }
            if (!ev.IsFrag)
            {
                Log.Debug("ATK Skip Code: 3", Debug);
                return;
            }
            var frag = ev.Grenade.GetComponent<FragGrenade>();
            string userId = null;
            string name = null;
            string nick = null;
            if (frag.thrower == null)
            {
                try
                {
                    userId = frag._throwerName.Split('(')[1].Split(')')[0];
                    name = frag._throwerName;
                    nick = frag._throwerName.Split('(')[0];
                }
                catch (System.Exception ex)
                {
                    Log.Error(ex.Message);
                    Log.Error(ex.StackTrace);
                    Log.Error(frag._throwerName);
                    return;
                }
            }
            else
            {
                if (Server.Host == ev.Thrower)
                    return;
                if (ev.Thrower == null)
                {
                    Log.Debug("ATK Skip Code: 1", Debug);
                    return;
                }
                userId = ev.Thrower.UserId;
                name = ev.Thrower.PlayerToString();
                nick = ev.Thrower.GetDisplayName();
            }
            if (ev.TargetToDamages.Count == 0 || (ev.TargetToDamages.Count == 1 && ev.Thrower != null && ev.TargetToDamages.ContainsKey(ev.Thrower)))
            {
                Log.Debug("ATK Skip Code: 4", Debug);
                return;
            }
            if (ev.TargetToDamages.Any(i => i.Key.Side != frag.TeamWhenThrown.GetSide() || (i.Key.Role == RoleType.ClassD && frag.TeamWhenThrown == Team.CDP)))
            {
                Log.Debug("ATK Skip Code: 5", Debug);
                return;
            }
            var tkTargets = new List<Player>();
            foreach (var item in ev.TargetToDamages.ToArray())
            {
                if (IsTeamkill(frag.TeamWhenThrown, item.Key.Team))
                    tkTargets.Add(item.Key);
            }
            if (TKGreneadedPlayers.ContainsKey(userId))
            {
                Log.Debug("ATK Skip Code: 6", Debug);
                return;
            }
            if (tkTargets.Count == 0 || (tkTargets.Count == 1 && ev.Thrower != null && tkTargets.Contains(ev.Thrower)))
            {
                Log.Debug("ATK Skip Code: 7", Debug);
                return;
            }
            Log.Debug("ATK Execute Code: 1", Debug);
            TKGreneadedPlayers.Add(userId, tkTargets.ToArray());
            MEC.Timing.CallDelayed(5, () => TKGreneadedPlayers.Remove(userId));
        }

        private void Player_Hurting(Exiled.Events.EventArgs.HurtingEventArgs ev)
        {
            //if (!ev.Attacker.IsReadyPlayer())
            //    return;
            if (!ev.Target.IsReadyPlayer())
                return;
            if (IsTeamkill(ev.Attacker, ev.Target))
                TeamAttack.Create(ev, null);
            else
            {
                if (TKGreneadedPlayers.Any(i => i.Value.Contains(ev.Target)))
                    TeamAttack.Create(ev, TKGreneadedPlayers.First(i => i.Value.Contains(ev.Target)).Key);
            }
        }

        private void Player_Dying(Exiled.Events.EventArgs.DyingEventArgs ev)
        {
            //if (!ev.Killer.IsReadyPlayer())
            //    return;
            if (!ev.Target.IsReadyPlayer())
                return;
            if (IsTeamkill(ev.Killer, ev.Target))
            {
                if (AntiDuplicateTK.Contains(ev.Target.UserId))
                    Log.Debug($"Duplicate TK for {ev.Killer.ToString(true)} killing {ev.Target.ToString(true)} BUT I DO NOT CARE");
                else
                    TeamKill.Create(ev, null);
            }
            else
            {
                if (TKGreneadedPlayers.Any(i => i.Value.Contains(ev.Target)))
                {
                    var killer = TKGreneadedPlayers.First(i => i.Value.Contains(ev.Target)).Key;
                    if (AntiDuplicateTK.Contains(ev.Target.UserId))
                        Log.Debug($"Duplicate TK for {killer} killing {ev.Target.ToString(true)} BUT I DO NOT CARE");
                    else
                        TeamKill.Create(ev, killer);
                }
            }
            if (!LastDead.ContainsKey(ev.Target.UserId))
            {
                LastDead.Add(ev.Target.UserId, ev.Target.Team);
                MEC.Timing.CallDelayed(10, () =>
                {
                    LastDead.Remove(ev.Target.UserId);
                });
            }
            if (!AntiDuplicateTK.Contains(ev.Target.UserId))
            {
                AntiDuplicateTK.Add(ev.Target.UserId);
                MEC.Timing.CallDelayed(2, () =>
                {
                    AntiDuplicateTK.Remove(ev.Target.UserId);
                });
            }
        }
        public static readonly HashSet<string> AntiDuplicateTK = new HashSet<string>();
        public static readonly Dictionary<string, Team> LastDead = new Dictionary<string, Team>();

        public static bool IsTeamkill(Player killer, Player victim)
        {
            if (!Round.IsStarted) 
                return false;
            if (killer == null) 
                return false;
            if (killer.UserId == victim.UserId)
                return false;
            if (killer.IsAlive || !LastDead.TryGetValue(killer.UserId, out Team killerTeam))
                killerTeam = killer.Team;
            return IsTeamkill(killerTeam, victim.Team);
        }

        public static bool IsTeamkill(Team killer, Team victim)
        {
            if (!Round.IsStarted)
                return false;
            foreach (var info in TeamKillTeams)
            {
                if (info.Key == killer && info.Value == victim)
                    return true;
            }

            return false;
        }

        private readonly static HashSet<string> Punishing = new HashSet<string>();

        private void OnTeamKill(string killerUserId, Player victim, DamageTypes.DamageType type, float amount)
        {
            if (!TeamKillsCounter.ContainsKey(killerUserId))
                TeamKillsCounter.Add(killerUserId, 1);
            else
                TeamKillsCounter[killerUserId]++;

            var tks = TeamKillsCounter[killerUserId];
            var killer = Player.Get(killerUserId);
            if(killer != null)
            {
                killer.Broadcast(5, $"<color=red>You have teamKilled {victim} using {type.name}\n - Total TeamKills: {tks}\n <b>This will not be tolerated</b></color>");
                victim.Broadcast(5, $"<color=red>You have been teamKilled by {killer.Nickname} using {type.name}</color>");

                RoundLogger.Log("TK", "KILL", $"{killer.PlayerToString()} teamkilled {victim.PlayerToString()} using {type.name}, it's killers {tks} teamkill");

                victim.SendConsoleMessage(
                    $"You have been TeamKilled:" +
                    $"\n- TeamKiller: {killer.ToString(false)}" +
                    $"\n- You: {victim.ToString(false)}" +
                    $"\n- Tool: {type.name}" +
                    $"\n- Server: {Server.Port}",
                    "red");
            }
            else
            {
                victim.Broadcast(5, $"<color=red>You have been teamKilled by [<color=red>PLAYER LEFT</color>] using {type.name}</color>");

                RoundLogger.Log("TK", "KILL", $"{killerUserId} teamkilled {victim.PlayerToString()} using {type.name}, it's killers {tks} teamkill but was not found");

                victim.SendConsoleMessage(
                    $"You have been TeamKilled:" +
                    $"\n- TeamKiller: [PLAYER LEFT (Ask admin to check round logs)]" +
                    $"\n- You: {victim.ToString(false)}" +
                    $"\n- Tool: {type.name}" +
                    $"\n- Server: {Server.Port}",
                    "red");
            }
            
            

            if (!Punishing.Contains(killerUserId))
            {
                Punishing.Add(killerUserId);
                MEC.Timing.CallDelayed(5, () =>
                {
                    PunishPlayer(killerUserId);
                    Punishing.Remove(killerUserId);
                });
            }
        }

        private void PunishPlayer(string userId)
        {
            int tks = TeamKillsCounter[userId];
            RoundLogger.Log("TK", "PUNISH", $"{userId} was punished for {tks} teamkills");
            switch (tks)
            {
                case 1:
                case 2:
                    break;
                case 3:
                    Log.Info("Player " + userId + " has been kicked for teamkilling " + tks + " times.");
                    Ban(userId, 0, "TK: Zostałeś automatycznie wyrzucony za zabicie za dużej ilości Sojuszników");
                    break;
                case 4:
                    Log.Info("Player " + userId + " has been banned for " + 60 + " minutes after teamkilling " + tks + " players.");
                    Ban(userId, 60, "TK: Zostałeś automatycznie zbanowany na 1h za zabicie za dużej ilości Sojuszników");
                    break;
                case 5:
                    Log.Info("Player " + userId + " has been banned for " + 600 + " minutes after teamkilling " + tks + " players.");
                    Ban(userId, 600, "TK: Zostałeś automatycznie zbanowany na 10h za zabicie za dużej ilości Sojuszników");
                    break;
                case 6:
                    Log.Info("Player " + userId + " has been banned for " + 1440 + " minutes after teamkilling " + tks + " players.");
                    Ban(userId, 1440, "TK: Zostałeś automatycznie zbanowany na 1 dzień za zabicie za dużej ilości Sojuszników");
                    break;
                case 7:
                case 8:
                case 9:
                    Log.Info("Player " + userId + " has been banned for " + 2880 + " minutes after teamkilling " + tks + " players.");
                    Ban(userId, 2880, "TK: Zostałeś automatycznie zbanowany na 2 dni za zabicie za dużej ilości Sojuszników");
                    break;
                case int i when i >= 10:
                    Log.Info("Player " + userId + " has been banned for " + 43200 + " minutes after teamkilling " + tks + " players.");
                    Ban(userId, 43200, "TK: Zostałeś automatycznie zbanowany na 1 miesiąc za zabicie za dużej ilości Sojuszników");
                    break;
            }    
        }

        private void OnTeamAttack(string attackerUserId, Player victim, DamageTypes.DamageType type, float amount)
        {
            var attacker = Player.Get(attackerUserId);
            if (attacker != null)
            {
                victim.Broadcast(2, $"<color=yellow>You have been attacked by team mate {attacker.Nickname} using {type.name} and he done {amount} damage</color>");
                attacker.Broadcast(5, $"<color=yellow>You have attacked teammate {victim} using {type.name} and done {amount} damage\n <b>This will not be tolerated</b></color>");

                RoundLogger.Log("TK", "DAMAGE", $"{attacker.PlayerToString()} teamattacked {victim.PlayerToString()} using {type.name} and done {amount} damage");

                victim.SendConsoleMessage(
                    $"You have been team attacked:" +
                    $"\n- TeamAttacker: {attacker.ToString(true)}" +
                    $"\n- You: {victim.ToString(true)}" +
                    $"\n- Tool: {type.name}" +
                    $"\n- Amount: {amount}" +
                    $"\n- Server: {Server.Port}",
                    "yellow");
            }
            else
            {
                victim.Broadcast(2, $"<color=yellow>You have been attacked by team mate [<color=red>PLAYER LEFT</color>] using {type.name} and he done {amount} damage</color>");
                
                RoundLogger.Log("TK", "DAMAGE", $"{attackerUserId} teamattacked {victim.PlayerToString()} using {type.name} and done {amount} damage but was not found");

                victim.SendConsoleMessage(
                    $"You have been team attacked:" +
                    $"\n- TeamAttacker: [PLAYER LEFT (Ask admin to check round logs)]" +
                    $"\n- You: {victim.ToString(true)}" +
                    $"\n- Tool: {type.name}" +
                    $"\n- Amount: {amount}" +
                    $"\n- Server: {Server.Port}",
                    "yellow");
            }
        }

        private void Server_RestartingRound()
        {
            TeamKillsCounter.Clear();
            CurrentRoundId++;
            TeamKills.Add(CurrentRoundId, new List<TeamKill>());
            TeamAttacks.Add(CurrentRoundId, new List<TeamAttack>());
        }

        private static void Ban(string attackerUserId, int duration, string reason)
        {
            var player = Player.Get(attackerUserId);
            if (player?.IsConnected ?? false)
                Server.BanPlayer.BanUser(player.GameObject, duration * 60, reason, "AntyTeamKill");
            else
            {
                Log.Debug("Player has disconnected before punishment, banning offline...");
                var banning = new Exiled.Events.EventArgs.BanningEventArgs(player, Server.Host, duration * 60, reason, reason);
                Exiled.Events.Handlers.Player.OnBanning(banning);
                if (!banning.IsAllowed)
                    return;
                var ban = new BanDetails
                {
                    Id = attackerUserId,
                    OriginalName = attackerUserId,
                    Issuer = "AntyTeamKill",
                    IssuanceTime = DateTime.Now.Ticks,
                    Expires = DateTime.Now.AddMinutes(duration).Ticks,
                    Reason = reason
                };
                BanHandler.IssueBan(ban, BanHandler.BanType.UserId);
                Exiled.Events.Handlers.Player.OnBanned(new Exiled.Events.EventArgs.BannedEventArgs(player, Server.Host, ban, BanHandler.BanType.UserId));
                /*ban.Id = player.IPAddress;
                BanHandler.IssueBan(ban, BanHandler.BanType.IP);
                Exiled.Events.Handlers.Player.OnBanned(new Exiled.Events.EventArgs.BannedEventArgs(player, Server.Host, ban, BanHandler.BanType.IP));*/
            }
        }

        public void SendTK(Player Killer, Player Target)
        {
            if (!Round.IsStarted)
            {
                Log.Debug("[ATK] Outside of round");
                return;
            }
            TeamKillsCounter.TryGetValue(Killer.UserId, out int num);
            if (!Mistaken.Utilities.APILib.API.GetUrl(APIType.TK, out string url, Target.UserId, Killer.UserId, Target.Role.ToString(), Killer.Role.ToString(), ServerConsole.Ip, Server.Port.ToString(), num.ToString()))
                return;
            using (var client = new WebClient())
            {
                //Log.Debug(url);
                client.DownloadStringAsync(new Uri(url));
            }
        }

        public void SendTK(string killerUserId, Team killerTeam, Player Target)
        {
            if (!Round.IsStarted)
            {
                Log.Debug("[ATK] Outside of round");
                return;
            }
            TeamKillsCounter.TryGetValue(killerUserId, out int num);
            if (!Mistaken.Utilities.APILib.API.GetUrl(APIType.TK, out string url, Target.UserId, killerUserId, Target.Role.ToString(), killerTeam.ToString(), ServerConsole.Ip, Server.Port.ToString(), num.ToString()))
                return;
            using (var client = new WebClient())
            {
                //Log.Debug(url);
                client.DownloadStringAsync(new Uri(url));
            }
        }
    }
}
