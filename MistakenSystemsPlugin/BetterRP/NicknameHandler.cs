using Exiled.API.Features;
using Gamer.Diagnostics;
using MEC;
using System.Collections.Generic;

namespace Gamer.Mistaken.BetterRP
{
    class NicknameHandler : Module
    {
        public NicknameHandler(PluginHandler plugin) : base(plugin)
        {

        }
        public override string Name => "BetterRP.Nickname";
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Player.ChangingRole += this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
            Exiled.Events.Handlers.Server.RespawningTeam += this.Handle<Exiled.Events.EventArgs.RespawningTeamEventArgs>((ev) => Server_RespawningTeam(ev));
            Exiled.Events.Handlers.Player.Dying += this.Handle<Exiled.Events.EventArgs.DyingEventArgs>((ev) => Player_Dying(ev));
            Exiled.Events.Handlers.Player.Escaping += this.Handle<Exiled.Events.EventArgs.EscapingEventArgs>((ev) => Player_Escaping(ev));
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Player.ChangingRole -= this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
            Exiled.Events.Handlers.Server.RespawningTeam -= this.Handle<Exiled.Events.EventArgs.RespawningTeamEventArgs>((ev) => Server_RespawningTeam(ev));
            Exiled.Events.Handlers.Player.Dying -= this.Handle<Exiled.Events.EventArgs.DyingEventArgs>((ev) => Player_Dying(ev));
            Exiled.Events.Handlers.Player.Escaping -= this.Handle<Exiled.Events.EventArgs.EscapingEventArgs>((ev) => Player_Escaping(ev));
        }

        private void Player_Escaping(Exiled.Events.EventArgs.EscapingEventArgs ev)
        {
            if (PluginHandler.Config.Type != MSP_ServerType.HARD_RP)
                return;
            MEC.Timing.CallDelayed(1, () =>
            {
                ev.Player.DisplayNickname = NicknameManager.GenerateNickname(ev.NewRole, "OMEGA", ev.Player.Id.ToString());
                if (ev.Player.DisplayNickname == "default")
                    ev.Player.DisplayNickname = null;
                ev.Player.ClearBroadcasts();
                if (ev.Player.DisplayNickname != null)
                    ev.Player.Broadcast(10, $"Nazywasz się: {ev.Player.DisplayNickname}");
            });
        }

        private void Player_Dying(Exiled.Events.EventArgs.DyingEventArgs ev)
        {
            if (PluginHandler.Config.Type != MSP_ServerType.HARD_RP)
                return;
            MEC.Timing.CallDelayed(1, () =>
            {
                ev.Target.DisplayNickname = null;
            });
        }

        private int ChaosRespawnId { get; set; } = 0;

        private void Server_RespawningTeam(Exiled.Events.EventArgs.RespawningTeamEventArgs ev)
        {
            if (PluginHandler.Config.Type != MSP_ServerType.HARD_RP)
                return;
            List<Player> toSpawn = ev.Players;
            while (toSpawn.Count > ev.MaximumRespawnAmount)
            {
                toSpawn.RemoveAt(toSpawn.Count - 1);
            }
            if (ev.NextKnownTeam == Respawning.SpawnableTeamType.NineTailedFox && ev.Players.Count > 0)
            {
                List<Player> TeamAlfa = NorthwoodLib.Pools.ListPool<Player>.Shared.Rent(0);
                List<Player> TeamBravo = NorthwoodLib.Pools.ListPool<Player>.Shared.Rent(0);
                List<Player> TeamCharlie = NorthwoodLib.Pools.ListPool<Player>.Shared.Rent(0);
                Player commander = ev.Players[0];
                for (int i = 1; i < ev.Players.Count; i++)
                {
                    if (i % 3 == 1)
                        TeamAlfa.Add(ev.Players[i]);
                    if (i % 3 == 2)
                        TeamBravo.Add(ev.Players[i]);
                    if (i % 3 == 0)
                        TeamCharlie.Add(ev.Players[i]);
                }

                Timing.CallDelayed(1, () =>
                {
                    var unit = commander.ReferenceHub.characterClassManager.NetworkCurUnitName;
                    commander.DisplayNickname = $"{unit}-1";
                    commander.ClearBroadcasts();
                    commander.Broadcast(10, $"Nazywasz się: {commander.DisplayNickname}");
                    for (int i = 0; i < TeamAlfa.Count; i++)
                    {
                        TeamAlfa[i].DisplayNickname = $"{unit} Alfa-{i + 1}";
                        TeamAlfa[i].ClearBroadcasts();
                        TeamAlfa[i].Broadcast(10, $"Nazywasz się: {TeamAlfa[i].DisplayNickname}");
                    }
                    for (int i = 0; i < TeamBravo.Count; i++)
                    {
                        TeamBravo[i].DisplayNickname = $"{unit} Bravo-{i + 1}";
                        TeamBravo[i].ClearBroadcasts();
                        TeamBravo[i].Broadcast(10, $"Nazywasz się: {TeamBravo[i].DisplayNickname}");
                    }
                    for (int i = 0; i < TeamCharlie.Count; i++)
                    {
                        TeamCharlie[i].DisplayNickname = $"{unit} Charlie-{i + 1}";
                        TeamCharlie[i].ClearBroadcasts();
                        TeamCharlie[i].Broadcast(10, $"Nazywasz się: {TeamCharlie[i].DisplayNickname}");
                    }
                    NorthwoodLib.Pools.ListPool<Player>.Shared.Return(TeamAlfa);
                    NorthwoodLib.Pools.ListPool<Player>.Shared.Return(TeamBravo);
                    NorthwoodLib.Pools.ListPool<Player>.Shared.Return(TeamCharlie);
                });
            }
            else if (ev.NextKnownTeam == Respawning.SpawnableTeamType.ChaosInsurgency && ev.Players.Count > 0)
            {
                for (int i = 0; i < ev.Players.Count; i++)
                {
                    ev.Players[i].DisplayNickname = $"CHAOS-{NicknameManager.NatoAlphabet[ChaosRespawnId]}-{i + 1}";
                    ev.Players[i].ClearBroadcasts();
                    ev.Players[i].Broadcast(10, $"Nazywasz się: {ev.Players[i].DisplayNickname}");
                }
                ChaosRespawnId++;
            }
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            if (ev.Player == null) return;
            if (!PluginHandler.Config.IsHardRP())
                return;
            if (ev.NewRole != RoleType.NtfCommander && ev.NewRole != RoleType.NtfLieutenant && ev.NewRole != RoleType.NtfCadet && ev.NewRole != RoleType.ChaosInsurgency)
            {
                ev.Player.DisplayNickname = NicknameManager.GenerateNickname(ev.NewRole).Replace("default", ev.Player.Nickname);
                ev.Player.ClearBroadcasts();
                ev.Player.Broadcast(10, $"Nazywasz się: {ev.Player.DisplayNickname}");
            }
            else
            {
                MEC.Timing.CallDelayed(5, () =>
                {
                    if (!ev.Player.Nickname.Contains("-") || ev.Player.Nickname.Contains("D-") || (ev.NewRole != RoleType.ChaosInsurgency && ev.Player.Nickname.Contains("CHAOS")) || (ev.NewRole == RoleType.ChaosInsurgency && !ev.Player.Nickname.Contains("CHAOS")))
                    {
                        ev.Player.DisplayNickname = NicknameManager.GenerateNickname(ev.NewRole, "OMEGA", ev.Player.Id.ToString()).Replace("default", ev.Player.ReferenceHub.nicknameSync._firstNickname);
                        ev.Player.ClearBroadcasts();
                        ev.Player.Broadcast(10, $"Nazywasz się: {ev.Player.DisplayNickname}");
                    }
                });
            }
        }
    }
}
