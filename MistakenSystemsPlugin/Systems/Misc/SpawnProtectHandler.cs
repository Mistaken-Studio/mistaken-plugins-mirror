using Exiled.API.Enums;
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
                Mistaken.Base.GUI.PseudoGUIHandler.Set(ev.Shooter, "spawnProtectShoot", Mistaken.Base.GUI.PseudoGUIHandler.Position.TOP, "You can't shoot if your spawn protected", 2);
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
            if (ev.Player.GetSessionVar<bool>(Main.SessionVarType.NO_SPAWN_PROTECT))
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
                    Mistaken.Base.GUI.PseudoGUIHandler.Set(ev.Attacker, "spawnProtect", Mistaken.Base.GUI.PseudoGUIHandler.Position.MIDDLE, "<size=300%>Watch <color=yellow>your</color> fire, <br>you'r hiting <color=yellow>friendlies</color></size>", 5);
                else if (SpawnKillProtected.Any(i => i.Key == ev.Target.Id && i.Value == true))
                {
                    ev.Attacker.Kill("You have tried to spawn kill");
                    RoundLogger.Log("SPAWN PROTECT", "KILL", $"Killed {ev.Attacker.PlayerToString()} for attacking spawn protected player");
                }
                else
                {
                    Mistaken.Base.GUI.PseudoGUIHandler.Set(ev.Attacker, "spawnProtect", Mistaken.Base.GUI.PseudoGUIHandler.Position.MIDDLE, "<size=150%>Player you're <color=yellow>attaking</color> has spawn protect</size>", 5);
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
                try
                {
                    Mistaken.Base.GUI.PseudoGUIHandler.Set(player, "spawnProtect", Mistaken.Base.GUI.PseudoGUIHandler.Position.BOTTOM, $"<color=green>[<color=orange>Spawn Protect</color>]</color> You are spawn protected for next <color=yellow>{i}</color> second{(i == 1 ? "" : "s")}");
                }
                catch(System.Exception ex)
                {
                    Log.Error(ex.Message);
                    Log.Error(ex.StackTrace);
                }
                yield return Timing.WaitForSeconds(1);
            }
            try
            {
                Mistaken.Base.GUI.PseudoGUIHandler.Set(player, "spawnProtect", Mistaken.Base.GUI.PseudoGUIHandler.Position.BOTTOM, null);
                SpawnKillProtected.RemoveAll(i => i.Key == id);
            }
            catch (System.Exception ex)
            {
                Log.Error(ex.Message);
                Log.Error(ex.StackTrace);
            }
        }
    }
}
