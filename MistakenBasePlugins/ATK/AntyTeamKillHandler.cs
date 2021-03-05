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

namespace Gamer.Mistaken.Base
{
    public class AntyTeamKillHandler : Diagnostics.Module
    {
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

            private TeamKill(Exiled.Events.EventArgs.DyingEventArgs ev)
            {
                RoundId = CurrentRoundId;
                Killer = ev.Killer;
                Victim = ev.Target;
                Info = ev.HitInformation;

                TeamKills[RoundId].Add(this);

                try
                {
                    Instance.OnTeamKill(Killer, Victim, Info.GetDamageType(), Info.Amount);
                }
                catch (System.Exception ex)
                {
                    Log.Error(ex.Message);
                    Log.Error(ex.StackTrace);
                }
                Instance.SendTK(Killer, Victim);
            }

            public static void Create(Exiled.Events.EventArgs.DyingEventArgs ev)
            {
                new TeamKill(ev);
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

            private TeamAttack(Exiled.Events.EventArgs.HurtingEventArgs ev)
            {
                RoundId = CurrentRoundId;
                Killer = ev.Attacker;
                Victim = ev.Target;
                Info = ev.HitInformations;

                TeamAttacks[RoundId].Add(this);

                Instance.OnTeamAttack(Killer, Victim, Info.GetDamageType(), Info.Amount);
            }

            public static void Create(Exiled.Events.EventArgs.HurtingEventArgs ev)
            {
                if (!Round.IsStarted)
                    return;
                new TeamAttack(ev);
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
        public AntyTeamKillHandler(PluginHandler p) : base(p)
        {
            Instance = this;
        }

        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.RestartingRound += this.Handle(() => Server_RestartingRound(), "RoundRestart");
            Exiled.Events.Handlers.Player.Dying += this.Handle<Exiled.Events.EventArgs.DyingEventArgs>((ev) => Player_Dying(ev));
            Exiled.Events.Handlers.Player.Hurting += this.Handle<Exiled.Events.EventArgs.HurtingEventArgs>((ev) => Player_Hurting(ev));

            Server_RestartingRound();
        }

        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.RestartingRound -= this.Handle(() => Server_RestartingRound(), "RoundRestart");
            Exiled.Events.Handlers.Player.Dying -= this.Handle<Exiled.Events.EventArgs.DyingEventArgs>((ev) => Player_Dying(ev));
            Exiled.Events.Handlers.Player.Hurting -= this.Handle<Exiled.Events.EventArgs.HurtingEventArgs>((ev) => Player_Hurting(ev));
        }

        private void Player_Hurting(Exiled.Events.EventArgs.HurtingEventArgs ev)
        {
            if (!ev.Attacker.IsVerified)
                return;
            if (!ev.Target.IsVerified)
                return;
            if (IsTeamkill(ev.Attacker, ev.Target))
                TeamAttack.Create(ev);
        }

        private void Player_Dying(Exiled.Events.EventArgs.DyingEventArgs ev)
        {
            if (!ev.Killer.IsVerified)
                return;
            if (!ev.Target.IsVerified)
                return;
            if (IsTeamkill(ev.Killer, ev.Target))
            {
                if (AntiDuplicateTK.Contains(ev.Target.UserId))
                    Log.Debug($"Duplicate TK for {ev.Killer.ToString(true)} attacking {ev.Target.ToString(true)} BUT I DO NOT CARE");
                else
                    TeamKill.Create(ev);
            }
            if (!LastDead.ContainsKey(ev.Target.UserId))
            {
                LastDead.Add(ev.Target.UserId, ev.Target.Team);
                MEC.Timing.CallDelayed(8, () =>
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
            foreach (var info in TeamKillTeams)
            {
                if (info.Key == killerTeam && info.Value == victim.Team)
                    return true;
            }

            return false;
        }

        private readonly static HashSet<string> Punishing = new HashSet<string>();

        private void OnTeamKill(Player killer, Player victim, DamageTypes.DamageType type, float amount)
        {
            if (!TeamKillsCounter.ContainsKey(killer.UserId))
                TeamKillsCounter.Add(killer.UserId, 1);
            else
                TeamKillsCounter[killer.UserId]++;

            var tks = TeamKillsCounter[killer.UserId];
            killer.Broadcast(5, $"<color=red>You have teamKilled {victim} using {type.name}\n - Total TeamKills: {tks}\n <b>This will not be tolerated</b></color>");
            victim.Broadcast(5, $"<color=red>You have been teamKilled by {killer.Nickname} using {type.name}</color>");

            RoundLogger.Log("TK", "KILL", $"{killer.PlayerToString()} teamkilled {victim.PlayerToString()} using {type.name}, it's killers {tks} teamkill");

            victim.SendConsoleMessage(
                $"You have been TeamKilled:" +
                $"\n- TeamKiller: {killer.ToString(true)}" +
                $"\n- You: {victim.ToString(true)}" +
                $"\n- Tool: {type.name}",
                "red");

            if (!Punishing.Contains(killer.UserId))
            {
                Punishing.Add(killer.UserId);
                MEC.Timing.CallDelayed(5, () =>
                {
                    Timing.RunCoroutine(PunishPlayer(killer));
                    Punishing.Remove(killer.UserId);
                });
            }
        }

        private IEnumerator<float> PunishPlayer(Player player)
        {
            if (player == null)
            {
                Log.Warn("PLAYER IS NULL BUT WASN'T");
                yield break;
            }

            int tks = TeamKillsCounter[player.UserId];
            RoundLogger.Log("TK", "PUNISH", $"{player.PlayerToString()} was punished for {tks} teamkills");
            switch (tks)
            {
                case 1:
                case 2:
                    break;
                case 3:
                    Log.Info("Player " + player.ToString(true, false) + " has been kicked for teamkilling " + tks + " times.");
                    Ban(player, 0, "TK: Zostałeś automatycznie wyrzucony za zabicie za dużej ilości Sojuszników");
                    break;
                case 4:
                    Log.Info("Player " + player.ToString(true, false) + " has been banned for " + 60 + " minutes after teamkilling " + tks + " players.");
                    Ban(player, 60, "TK: Zostałeś automatycznie zbanowany na 1h za zabicie za dużej ilości Sojuszników");
                    break;
                case 5:
                    Log.Info("Player " + player.ToString(true, false) + " has been banned for " + 600 + " minutes after teamkilling " + tks + " players.");
                    Ban(player, 600, "TK: Zostałeś automatycznie zbanowany na 10h za zabicie za dużej ilości Sojuszników");
                    break;
                case 6:
                    Log.Info("Player " + player.ToString(true, false) + " has been banned for " + 1440 + " minutes after teamkilling " + tks + " players.");
                    Ban(player, 1440, "TK: Zostałeś automatycznie zbanowany na 1 dzień za zabicie za dużej ilości Sojuszników");
                    break;
                case 7:
                case 8:
                case 9:
                    Log.Info("Player " + player.ToString(true, false) + " has been banned for " + 2880 + " minutes after teamkilling " + tks + " players.");
                    Ban(player, 2880, "TK: Zostałeś automatycznie zbanowany na 2 dni za zabicie za dużej ilości Sojuszników");
                    break;
                case int i when i >= 10:
                    Log.Info("Player " + player.ToString(true, false) + " has been banned for " + 43200 + " minutes after teamkilling " + tks + " players.");
                    Ban(player, 43200, "TK: Zostałeś automatycznie zbanowany na 1 miesiąc za zabicie za dużej ilości Sojuszników");
                    break;
            }    
        }

        private void OnTeamAttack(Player attacker, Player victim, DamageTypes.DamageType type, float amount)
        {
            victim.Broadcast(2, $"<color=yellow>You have been attacked by team mate {attacker.Nickname} using {type.name} and he done {amount} damage</color>");
            attacker.Broadcast(5, $"<color=yellow>You have attacked teammate {victim} using {type.name} and done {amount} damage\n <b>This will not be tolerated</b></color>");

            RoundLogger.Log("TK", "DAMAGE", $"{attacker.PlayerToString()} teamattacked {victim.PlayerToString()} using {type.name} and done {amount} damage");

            victim.SendConsoleMessage(
                $"You have been team attacked:" +
                $"\n- TeamAttacker: {attacker.ToString(true)}" +
                $"\n- You: {victim.ToString(true)}" +
                $"\n- Tool: {type.name}" +
                $"\n- Amount: {amount}", 
                "yellow");
        }

        private void Server_RestartingRound()
        {
            TeamKillsCounter.Clear();
            CurrentRoundId++;
            TeamKills.Add(CurrentRoundId, new List<TeamKill>());
            TeamAttacks.Add(CurrentRoundId, new List<TeamAttack>());
        }

        private static void Ban(Player player, int duration, string reason)
        {
            if (player.IsConnected)
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
                    Id = player.UserId,
                    OriginalName = player.Nickname,
                    Issuer = "AntyTeamKill",
                    IssuanceTime = DateTime.Now.Ticks,
                    Expires = DateTime.Now.AddMinutes(duration).Ticks,
                    Reason = reason
                };
                BanHandler.IssueBan(ban, BanHandler.BanType.UserId);
                Exiled.Events.Handlers.Player.OnBanned(new Exiled.Events.EventArgs.BannedEventArgs(player, Server.Host, ban, BanHandler.BanType.UserId));
                ban.Id = player.IPAddress;
                BanHandler.IssueBan(ban, BanHandler.BanType.IP);
                Exiled.Events.Handlers.Player.OnBanned(new Exiled.Events.EventArgs.BannedEventArgs(player, Server.Host, ban, BanHandler.BanType.IP));
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
    }
}
