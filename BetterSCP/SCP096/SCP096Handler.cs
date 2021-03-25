using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Features;
using Gamer.Utilities;
using Gamer.Mistaken.BetterSCP;
using HarmonyLib;
using MEC;
using UnityEngine;
using Gamer.Diagnostics;

namespace Gamer.Mistaken.BetterSCP.SCP096
{
    class SCP096Handler : Diagnostics.Module
    {
        public SCP096Handler(PluginHandler p) : base(p)
        {
            plugin.RegisterTranslation("Info_096Target", "You are <color=yellow><b>Target</b></color> for <color=red>SCP 096</color><br>Targets: <color=yellow>{0}</color>");
            plugin.RegisterTranslation("Info_096", "You have <color=yellow>{0}</color> target{1}<br><color=yellow>{2}</color>s rage left");
        }

        public override string Name => nameof(SCP096Handler);
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.RoundStarted += this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Scp096.AddingTarget += this.Handle<Exiled.Events.EventArgs.AddingTargetEventArgs>((ev) => Scp096_AddingTarget(ev));
            Exiled.Events.Handlers.Player.ChangingRole += this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
            Exiled.Events.Handlers.Player.Hurting += this.Handle<Exiled.Events.EventArgs.HurtingEventArgs>((ev) => Player_Hurting(ev));
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Scp096.AddingTarget -= this.Handle<Exiled.Events.EventArgs.AddingTargetEventArgs>((ev) => Scp096_AddingTarget(ev));
            Exiled.Events.Handlers.Player.ChangingRole -= this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
            Exiled.Events.Handlers.Player.Hurting -= this.Handle<Exiled.Events.EventArgs.HurtingEventArgs>((ev) => Player_Hurting(ev));
        }

        private void Player_Hurting(Exiled.Events.EventArgs.HurtingEventArgs ev)
        {
            if (DmgMultiplayer)
            {
                if (ev.Target.Role == RoleType.Scp096)
                    ev.Amount *= 1.5f;
            }
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            if (ev.NewRole == RoleType.Scp096)
                Timing.RunCoroutine(AntyDuo(ev.Player));
        }

        private bool DmgMultiplayer = false;
        private IEnumerator<float> AntyDuo(Player player)
        {
            yield return Timing.WaitForSeconds(1);
            while (player?.Role == RoleType.Scp096)
            {
                DmgMultiplayer = false;
                if (player.Position.y < 900)
                {
                    foreach (var item in RealPlayers.List.Where(p => p.Role == RoleType.Scp173).ToArray())
                    {
                        if (Vector3.Distance(player.Position, item.Position) < PluginHandler.Anty173_096DuoDistance)
                        {
                            DmgMultiplayer = true;
                            player.EnableEffect<CustomPlayerEffects.Concussed>();
                            player.ShowHint("Jesteś za blisko <color=yellow>SCP 173</color>, z tego powodu będziesz dostawał <color=yellow>150</color>% obrażeń", true, 3, false);
                            break;
                        }
                    }
                }
                if (!DmgMultiplayer)
                {
                    player.DisableEffect<CustomPlayerEffects.Concussed>();
                }
                yield return Timing.WaitForSeconds(2);
            }
        }

        private void Scp096_AddingTarget(Exiled.Events.EventArgs.AddingTargetEventArgs ev)
        {
            if (Systems.Misc.SpawnProtectHandler.SpawnKillProtected.Any(i => i.Key == ev.Target.Id))
            {
                ev.AhpToAdd = 0;
                ev.EnrageTimeToAdd = 1;
            }
            else
            {
                if (ev.Target.Position.y > 900)
                    return;
                foreach (var item in RealPlayers.List.Where(p => p.Role == RoleType.Scp173))
                {
                    if(Vector3.Distance(ev.Target.Position, item.Position) < PluginHandler.Anty173_096DuoDistance / 2)
                    {
                        ev.AhpToAdd = 0;
                        ev.EnrageTimeToAdd = 0;
                        break;
                    }
                }
                var scp096 = (ev.Scp096.ReferenceHub.scpsController.CurrentScp as PlayableScps.Scp096);
                if (scp096.AddedTimeThisRage > scp096.MaximumAddedEnrageTime)
                    ev.EnrageTimeToAdd = 0;
            }
        }

        private void Server_RoundStarted()
        {
            Timing.RunCoroutine(Inform096Target());
        }

        private IEnumerator<float> Inform096Target()
        {
            yield return Timing.WaitForSeconds(1f);
            int rid = RoundPlus.RoundId; while (Round.IsStarted && rid == RoundPlus.RoundId)
            {
                foreach (var scp096 in RealPlayers.Get(RoleType.Scp096))
                {
                    if (scp096.ReferenceHub.scpsController.CurrentScp is PlayableScps.Scp096 scp096script && scp096.Role == RoleType.Scp096 && (scp096script.Enraged || scp096script.Enraging))
                    {
                        string targetMessage = plugin.ReadTranslation("Info_096Target", scp096script._targets.Count);
                        foreach (var item in scp096script._targets.ToArray())
                            Player.Get(item).ShowHint(targetMessage, true, 1, true);
                        var time = Mathf.RoundToInt(scp096script.EnrageTimeLeft).ToString();
                        if (time == "0")
                            time = "[REDACTED]";
                        scp096.ShowHint(plugin.ReadTranslation("Info_096", scp096script._targets.Count, scp096script._targets.Count == 1 ? "" : "s", time), true, 2, true);
                    }
                }
                
                yield return Timing.WaitForSeconds(1f);
            }
        }
    }
}
