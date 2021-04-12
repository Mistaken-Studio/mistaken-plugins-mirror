using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Features;
using Gamer.Utilities;
using Gamer.Mistaken.BetterSCP;
using HarmonyLib;
using MEC;
using UnityEngine;
using Gamer.Diagnostics;

namespace Gamer.Mistaken.BetterSCP.SCP049
{
    class SCP049Handler : Module
    {
        public SCP049Handler(PluginHandler p) : base(p)
        {
            plugin.RegisterTranslation("scp049_start_message", "<color=red><b><size=500%>UWAGA</size></b></color><br><br><br><br><br><br><size=90%>Rozgrywka jako <color=red>SCP 049</color> na tym serwerze jest zmodyfikowana, <color=red>SCP 049</color> posiada domyślnie dodatkowe <color=yellow>60</color> ahp, każdy <color=red>SCP 049-2</color> w zasięgu <color=yellow>10</color> metrów dodaje +<color=yellow>100</color> do max ahp, ahp regeneruje się z prędkością <color=yellow>20</color> na sekundę pod warunkiem że jest <color=yellow>bezpieczny</color>(w ciągu ostatnich <color=yellow>10</color> sekund nie otrzymał obrażeń)</size>");
        }

        public override string Name => nameof(SCP049Handler);
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Player.ChangingRole += this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
            Exiled.Events.Handlers.Server.RoundStarted += this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Player.Died += this.Handle<Exiled.Events.EventArgs.DiedEventArgs>((ev) => Player_Died(ev));

            Gamer.Mistaken.Systems.InfoMessage.InfoMessageManager.WelcomeMessages[RoleType.Scp049] = plugin.ReadTranslation("scp049_start_message");
        }
        public override void OnDisable()
        {
            Gamer.Mistaken.Systems.InfoMessage.InfoMessageManager.WelcomeMessages.Remove(RoleType.Scp049);

            Exiled.Events.Handlers.Player.ChangingRole -= this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
            Exiled.Events.Handlers.Server.RoundStarted -= this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Player.Died -= this.Handle<Exiled.Events.EventArgs.DiedEventArgs>((ev) => Player_Died(ev));
        }

        private void Player_Died(Exiled.Events.EventArgs.DiedEventArgs ev)
        {
            if(ev.Killer?.Role == RoleType.Scp0492)
            {
                ev.Killer.Health += 100;
                if (ev.Killer.MaxHealth < ev.Killer.Health)
                    ev.Killer.Health = ev.Killer.MaxHealth;
            }
        }

        private void Server_RoundStarted()
        {
            Timing.RunCoroutine(UpdateInfo());
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            if (ev.NewRole == RoleType.Scp049)
                Timing.RunCoroutine(UpdateShield(ev.Player));
        }

        private IEnumerator<float> UpdateInfo()
        {
            yield return Timing.WaitForSeconds(1);
            int rid = RoundPlus.RoundId; 
            while (Round.IsStarted && rid == RoundPlus.RoundId)
            {
                try
                {
                    List<Player> zombieInRange = NorthwoodLib.Pools.ListPool<Player>.Shared.Rent();
                    foreach (var player in RealPlayers.Get(RoleType.Scp049))
                    {
                        if (player == null)
                            continue;
                        List<string> message = NorthwoodLib.Pools.ListPool<string>.Shared.Rent();
                        foreach (var item in Systems.Patches.RagdollManagerPatch.Ragdolls.ToArray())
                        {
                            try
                            {
                                try
                                {
                                    if (item.ragdoll?.gameObject == null)
                                        continue;
                                }
                                catch { continue; }
                                if (item.ragdoll.NetworkallowRecall && item.ragdoll.CurrentTime < 10f)
                                {
                                    var distance = Vector3.Distance(player.Position, item.ragdoll.transform.position);
                                    if (distance > 10)
                                        continue;
                                    message.Add($"<color=yellow>{item.OwnerNickname}</color> - <color=yellow>{Mathf.RoundToInt(distance)}</color>m away - <color=yellow>{Mathf.RoundToInt(10 - item.ragdoll.CurrentTime)}</color>s");
                                }
                            }
                            catch (System.Exception ex)
                            {
                                Log.Error("Internal");
                                Log.Error(ex.Message);
                                Log.Error(ex.StackTrace);
                            }
                        }
                        if (message.Count != 0)
                            Systems.GUI.PseudoGUIHandler.Set(player, "scp049", Systems.GUI.PseudoGUIHandler.Position.BOTTOM, $"Potential zombies:<br><br>{string.Join("<br>", message)}");
                        else
                            Systems.GUI.PseudoGUIHandler.Set(player, "scp049", Systems.GUI.PseudoGUIHandler.Position.BOTTOM, null);
                        NorthwoodLib.Pools.ListPool<string>.Shared.Return(message);
                    }
                    NorthwoodLib.Pools.ListPool<Player>.Shared.Return(zombieInRange);
                }
                catch (System.Exception ex)
                {
                    Log.Error("External");
                    Log.Error(ex.Message);
                    Log.Error(ex.StackTrace);
                }
                yield return Timing.WaitForSeconds(1);
            }
        }
        private IEnumerator<float> UpdateShield(Player player)
        {
            yield return MEC.Timing.WaitForSeconds(1);
            try
            {
                Systems.Shield.ShieldedManager.Add(new Systems.Shield.Shielded(player, 60, 20, 15, 0, 1));
            }
            catch (System.Exception ex)
            {
                Log.Error(ex.Message);
                Log.Error(ex.StackTrace);
            }
            while (player?.Role == RoleType.Scp049)
            {
                if (player == null) 
                    break;
                try
                {
                    int shield = 60;
                    foreach (var zombie in RealPlayers.Get(RoleType.Scp0492))
                    {
                        if (Vector3.Distance(player.Position, zombie.Position) <= 10)
                        {
                            shield += 100;
                            if(zombie.MaxHealth > zombie.Health)
                                zombie.Health += 0.5f;
                        }
                    }
                    if(Systems.Shield.ShieldedManager.Shieldeds.Any(i => i.player.Id == player.Id))
                        Systems.Shield.ShieldedManager.Get(player).MaxShield = shield;
                    else
                        Systems.Shield.ShieldedManager.Add(new Systems.Shield.Shielded(player, 60, 20, 15, 0, 1));
                }
                catch(System.Exception ex)
                {
                    Log.Error(ex.Message);
                    Log.Error(ex.StackTrace);
                }
                yield return Timing.WaitForSeconds(1);
            }
            Gamer.Mistaken.Systems.Shield.ShieldedManager.Remove(player);
        }
    }
}
