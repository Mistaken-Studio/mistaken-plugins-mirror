﻿using Exiled.API.Enums;
using Exiled.API.Features;
using Gamer.Diagnostics;
using Gamer.RoundLoggerSystem;
using Gamer.Utilities;
using Grenades;
using MEC;
using Mirror;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Gamer.Mistaken.Systems.Misc
{
    public class SpawnProtectHandler : Module
    {
        public SpawnProtectHandler(PluginHandler p) : base(p)
        {
        }

        public override string Name => "SpawnProtect";
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Player.Hurting += this.Handle<Exiled.Events.EventArgs.HurtingEventArgs>((ev) => Player_Hurting(ev));
            Exiled.Events.Handlers.Player.ChangingRole += this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
            Exiled.Events.Handlers.Server.RestartingRound += this.Handle(() => Server_RestartingRound(), "RoundRestart");
            Exiled.Events.Handlers.Player.Shooting += this.Handle<Exiled.Events.EventArgs.ShootingEventArgs>((ev) => Player_Shooting(ev));
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Player.Hurting -= this.Handle<Exiled.Events.EventArgs.HurtingEventArgs>((ev) => Player_Hurting(ev));
            Exiled.Events.Handlers.Player.ChangingRole -= this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
            Exiled.Events.Handlers.Server.RestartingRound -= this.Handle(() => Server_RestartingRound(), "RoundRestart");
            Exiled.Events.Handlers.Player.Shooting -= this.Handle<Exiled.Events.EventArgs.ShootingEventArgs>((ev) => Player_Shooting(ev));
        }

        private void Player_Shooting(Exiled.Events.EventArgs.ShootingEventArgs ev)
        {
            if (SpawnKillProtected.Any(i => i.Key == ev.Shooter.Id))
            {
                ev.IsAllowed = false;
                ev.Shooter.ShowHintPulsating("You can't shoot if your spawn protected", 2, true, false);
                RoundLogger.Log("SPAWN PROTECT", "BLOCK", $"Blocked shooting because of spawn protect for {ev.Shooter.PlayerToString()}");
            }
        }

        private void Server_RestartingRound()
        {
            SpawnKillProtected.Clear();
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            if (ev.Player == null) return;
            if (ev.Player.SessionVariables.TryGetValue("NO_SPAWN_PROTECT", out object disallow) && (bool)disallow)
                return;
            if (ev.NewRole == RoleType.NtfCadet || ev.NewRole == RoleType.NtfLieutenant || ev.NewRole == RoleType.NtfCommander || ev.NewRole == RoleType.NtfScientist || ev.NewRole == RoleType.ChaosInsurgency)
            {
                var pid = ev.Player.Id;
                var isEscape = IsEscape(ev.Player);
                SpawnKillProtected.Add(new KeyValuePair<int, bool>(pid, !isEscape));
                Timing.RunCoroutine(RemoveFromSpawnKillDetection(ev.Player, isEscape ? 4 : 8));
                MEC.Timing.CallDelayed(8, () => SpawnKillProtected.RemoveAll(i => i.Key == pid));
            }
        }

        private bool IsEscape(Player p)
        {
            switch(p.Role)
            {
                case RoleType.ClassD:
                case RoleType.Scientist:
                    return true;
                case RoleType.NtfScientist:
                case RoleType.NtfCommander:
                case RoleType.NtfCadet:
                case RoleType.NtfLieutenant:
                case RoleType.ChaosInsurgency:
                case RoleType.FacilityGuard:
                    return p.IsCuffed;
                default:
                    return false;
            }
        }

        private void Player_Hurting(Exiled.Events.EventArgs.HurtingEventArgs ev)
        {
            if (ev.Attacker != null && ev.Attacker.Id != ev.Target.Id && SpawnKillProtected.Any(i => i.Key == ev.Target.Id))
            {
                ev.Amount = 0;
                if (ev.Attacker.Side == ev.Target.Side)
                    ev.Attacker.ShowHint("<size=300%>Watch <color=yellow>your</color> fire, <br>you'r hiting <color=yellow>friendlies</color></size>", true, 5, true);
                else if (SpawnKillProtected.Any(i => i.Key == ev.Target.Id && i.Value == true))
                {
                    ev.Attacker.Kill("You have tried to spawn kill");
                    RoundLogger.Log("SPAWN PROTECT", "KILL", $"Killed {ev.Attacker.PlayerToString()} for attacking spawn protected player");
                }
                else
                {
                    ev.Attacker.ShowHint("<size=150%>Player you're <color=yellow>attaking</color> has spawn protect</size>", true, 5, true);
                    RoundLogger.Log("SPAWN PROTECT", "WARN", $"Warned {ev.Attacker.PlayerToString()} about spawn protect");
                }
            }
        }

        public static readonly List<KeyValuePair<int, bool>> SpawnKillProtected = new List<KeyValuePair<int, bool>>();


        private IEnumerator<float> RemoveFromSpawnKillDetection(Player player, int time)
        {
            int id = player?.Id ?? -1;
            for (int i = time; i > 0; i--)
            {
                player?.ShowHint($"<br><br><br><br><br><color=green>[<color=orange>Spawn Protect</color>]</color> You are spawn protected for next <color=yellow>{i}</color> second{(i == 1 ? "" : "s")}", false, 1, false);
                yield return Timing.WaitForSeconds(1);
            }

            SpawnKillProtected.RemoveAll(i => i.Key == id);
        }
    }
}
