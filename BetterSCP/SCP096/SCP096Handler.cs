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
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Scp096.AddingTarget -= this.Handle<Exiled.Events.EventArgs.AddingTargetEventArgs>((ev) => Scp096_AddingTarget(ev));
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
                var scp096 = ev.Scp096.ReferenceHub.scpsController.CurrentScp as PlayableScps.Scp096;
                if (scp096.AddedTimeThisRage > scp096.MaximumAddedEnrageTime)
                    ev.EnrageTimeToAdd = 0;
                if (ev.Target.Position.y > 900)
                    return;
                if (Global.GlobalHandler.DmgMultiplayer)
                {
                    ev.AhpToAdd = 0;
                    ev.EnrageTimeToAdd = 0;
                }
            }
        }

        private void Server_RoundStarted()
        {
            Timing.RunCoroutine(Inform096Target());
        }

        private IEnumerator<float> Inform096Target()
        {
            yield return Timing.WaitForSeconds(1f);
            int rid = RoundPlus.RoundId;
            List<Player> added = new List<Player>();
            Player[] lastAdded;
            while (Round.IsStarted && rid == RoundPlus.RoundId)
            {
                lastAdded = added.ToArray();
                added.Clear();
                foreach (var scp096 in RealPlayers.Get(RoleType.Scp096))
                {
                    if (scp096.Role == RoleType.Scp096 && scp096.ReferenceHub.scpsController.CurrentScp is PlayableScps.Scp096 scp096script && (scp096script.Enraged || scp096script.Enraging))
                    {
                        string targetMessage = plugin.ReadTranslation("Info_096Target", scp096script._targets.Count);
                        foreach (var item in scp096script._targets.ToArray())
                        {
                            var p = Player.Get(item);
                            Mistaken.Base.GUI.PseudoGUIHandler.Set(p, "scp096", Mistaken.Base.GUI.PseudoGUIHandler.Position.TOP, targetMessage);
                            added.Add(p);
                        }
                        var time = Mathf.RoundToInt(scp096script.EnrageTimeLeft).ToString();
                        if (time == "0")
                            time = "[REDACTED]";
                        Mistaken.Base.GUI.PseudoGUIHandler.Set(scp096, "scp096", Mistaken.Base.GUI.PseudoGUIHandler.Position.TOP, plugin.ReadTranslation("Info_096", scp096script._targets.Count, scp096script._targets.Count == 1 ? "" : "s", time));
                        added.Add(scp096);
                    }
                }
                foreach (var player in lastAdded.Where(i => !added.Contains(i)))
                    Mistaken.Base.GUI.PseudoGUIHandler.Set(player, "scp096", Mistaken.Base.GUI.PseudoGUIHandler.Position.TOP, null);

                yield return Timing.WaitForSeconds(1f);
            }
        }
    }
}
