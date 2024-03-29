﻿using Exiled.API.Features;
using Gamer.Diagnostics;
using Gamer.Mistaken.Base.GUI;
using Gamer.RoundLoggerSystem;
using Gamer.Utilities;
using MEC;
using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gamer.Mistaken.Systems.Misc
{
    internal class ResurectionHandler : Module
    {
        private static new __Log Log;
        public ResurectionHandler(PluginHandler p) : base(p)
        {
            Log = base.Log;
        }

        public override string Name => "Resurection";
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.RestartingRound += this.Handle(() => Server_RestartingRound(), "RoundRestart");
            Exiled.Events.Handlers.Player.ChangingItem += this.Handle<Exiled.Events.EventArgs.ChangingItemEventArgs>((ev) => Player_ChangingItem(ev));
            Exiled.Events.Handlers.Player.UsingMedicalItem += this.Handle<Exiled.Events.EventArgs.UsingMedicalItemEventArgs>((ev) => Player_UsingMedicalItem(ev));
            Exiled.Events.Handlers.Player.ChangingRole += this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
            Exiled.Events.Handlers.Player.Died += this.Handle<Exiled.Events.EventArgs.DiedEventArgs>((ev) => Player_Died(ev));
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.RestartingRound -= this.Handle(() => Server_RestartingRound(), "RoundRestart");
            Exiled.Events.Handlers.Player.ChangingItem -= this.Handle<Exiled.Events.EventArgs.ChangingItemEventArgs>((ev) => Player_ChangingItem(ev));
            Exiled.Events.Handlers.Player.UsingMedicalItem -= this.Handle<Exiled.Events.EventArgs.UsingMedicalItemEventArgs>((ev) => Player_UsingMedicalItem(ev));
            Exiled.Events.Handlers.Player.ChangingRole -= this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
            Exiled.Events.Handlers.Player.Died -= this.Handle<Exiled.Events.EventArgs.DiedEventArgs>((ev) => Player_Died(ev));
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            ev.Player.SetGUI("u500", Mistaken.Base.GUI.PseudoGUIHandler.Position.TOP, null);
        }

        private void Player_Died(Exiled.Events.EventArgs.DiedEventArgs ev)
        {
            ev.Target.SetGUI("u500", PseudoGUIHandler.Position.TOP, null);
        }

        private void Server_RestartingRound()
        {
            Resurections.Clear();
            Resurected.Clear();
        }

        private const float MaximalDistance = 5.5f;
        private static readonly List<string> Resurections = new List<string>();
        private static readonly HashSet<string> Resurected = new HashSet<string>();

        public static bool Resurect(Player player)
        {
            if (player.CurrentItem.id != ItemType.SCP500)
                return false;
            var originalRole = player.Role;
            float nearestDistance = 999;
            Patches.RagdollManagerPatch.RagdollInfo nearest = null;
            foreach (var ragdoll in Patches.RagdollManagerPatch.Ragdolls.Where(p => (DateTime.Now - p.DeathTime).TotalSeconds < 20).ToArray())
            {
                if (ragdoll.Team == Team.SCP || ragdoll.Team == Team.TUT)
                    continue;
                var target = Player.Get(ragdoll.ragdoll.owner.PlayerId);
                if (target == null || target.IsOverwatchEnabled || target.GameObject == null || !target.IsConnected)
                {
                    player.SendConsoleMessage($"[SCP 500] {ragdoll.ragdoll.owner.Nick} nie ma na serwerze albo ma overwatcha", "red");
                    continue;
                }
                try
                {
                    var distance = Vector3.Distance(player.Position, ragdoll.ragdoll.transform.position);
                    if (distance < MaximalDistance && distance < nearestDistance)
                    {
                        nearest = ragdoll;
                        nearestDistance = distance;
                    }
                }
                catch (System.Exception ex)
                {
                    Log.Error("Failed to get ragdoll distance");
                    Log.Error(ex.Message);
                    Log.Error(ex.StackTrace);
                }
            }
            if (nearestDistance != 999)
            {
                var target = Player.Get(nearest.ragdoll.owner.PlayerId);
                if (Resurected.Contains(target.UserId))
                {
                    target.SetGUI("u500", Mistaken.Base.GUI.PseudoGUIHandler.Position.TOP, "Nie udało się wskrzesić gracza | Gracz może zostać wskrzeszony raz na rundę", 5);
                    return false;
                }
                if (Resurections.Where(i => i == target.UserId).Count() > 3)
                {
                    target.SetGUI("u500", Mistaken.Base.GUI.PseudoGUIHandler.Position.TOP, "Nie udało się wskrzesić gracza | Możesz wskrzesić 3 osoby na rundę", 5);
                    return false;
                }
                player.EnableEffect<CustomPlayerEffects.Amnesia>(15);
                player.EnableEffect<CustomPlayerEffects.Ensnared>(11);
                target.SetGUI("u500", Mistaken.Base.GUI.PseudoGUIHandler.Position.MIDDLE, $"Używam <color=yellow>SCP 500</color> na {target.Nickname}", 9);
                Gamer.Utilities.BetterCourotines.CallDelayed(10, () =>
                {
                    if (player.Role == originalRole)
                    {
                        target = Player.Get(nearest.ragdoll.owner.PlayerId);
                        if (target == null || target.GameObject == null || !target.IsConnected)
                        {
                            target.SetGUI("u500", Mistaken.Base.GUI.PseudoGUIHandler.Position.TOP, "Nie udało się wskrzesić gracza | Gracza nie ma na serwerze", 5);
                            return;
                        }
                        if (target.IsOverwatchEnabled)
                        {
                            target.SetGUI("u500", Mistaken.Base.GUI.PseudoGUIHandler.Position.TOP, "Nie udało się wskrzesić gracza | Gracz chyba nie chce być wskrzeszony", 5);
                            return;
                        }
                        if (target.IsAlive)
                        {
                            target.SetGUI("u500", Mistaken.Base.GUI.PseudoGUIHandler.Position.TOP, "Nie udało się wskrzesić gracza | Jesteś pewien że ten gracz jest martwy?", 5);
                            return;
                        }
                        Vector3 pos = nearest.ragdoll.transform.position;
                        NetworkServer.Destroy(nearest.ragdoll.gameObject);
                        Patches.RagdollManagerPatch.Ragdolls.Remove(nearest);
                        EVO.Handler.AddProgress(1010, player.UserId);
                        Resurected.Add(target.UserId);
                        Resurections.Add(player.UserId);
                        if (player.CurrentItem.durability == 1)
                            player.Inventory.items.Remove(player.CurrentItem);
                        else
                            player.Inventory.items.ModifyDuration(player.CurrentItemIndex, player.CurrentItem.durability - 1);
                        target.SetSessionVar(Main.SessionVarType.NO_SPAWN_PROTECT, true);
                        target.SetSessionVar(Main.SessionVarType.ITEM_LESS_CLSSS_CHANGE, true);
                        target.Role = nearest.Role;
                        target.SetSessionVar(Main.SessionVarType.NO_SPAWN_PROTECT, false);
                        target.SetSessionVar(Main.SessionVarType.ITEM_LESS_CLSSS_CHANGE, false);
                        target.ClearInventory();
                        Gamer.Utilities.BetterCourotines.CallDelayed(0.5f, () =>
                        {
                            Misc.SpawnProtectHandler.SpawnKillProtected.RemoveAll(i => i.Key == target.Id);
                            target.Position = pos + Vector3.up;
                            target.SetGUI("u500", Mistaken.Base.GUI.PseudoGUIHandler.Position.MIDDLE, $"Zostałeś <color=yellow>wskrzeszony</color> przez {player.Nickname}", 5);
                            target.Health = 5;
                            target.ArtificialHealth = 75;
                            target.EnableEffect<CustomPlayerEffects.Blinded>(10);
                            target.EnableEffect<CustomPlayerEffects.Deafened>(15);
                            target.EnableEffect<CustomPlayerEffects.Disabled>(30);
                            target.EnableEffect<CustomPlayerEffects.Concussed>(15);
                            target.EnableEffect<CustomPlayerEffects.Flashed>(5);
                            RoundLogger.Log("RESURECT", "RESURECT", $"Resurected {target.PlayerToString()}");
                        }, "Resurect.Respawn");
                    }
                }, "Resurection.Resurect");
                return true;
            }
            else
                player.SendConsoleMessage("[SCP 500] No targers in range", "red");
            return false;
        }


        private void Player_UsingMedicalItem(Exiled.Events.EventArgs.UsingMedicalItemEventArgs ev)
        {
            if (ev.Item == ItemType.SCP500 && ev.Player.GetEffectActive<CustomPlayerEffects.Amnesia>())
                ev.IsAllowed = false;
        }

        private void Player_ChangingItem(Exiled.Events.EventArgs.ChangingItemEventArgs ev)
        {
            if (ev.NewItem.id == ItemType.SCP500)
                this.RunCoroutine(Interface(ev.Player), "Interface");
        }

        private IEnumerator<float> Interface(Player player)
        {
            yield return Timing.WaitForSeconds(1);
            while (player?.CurrentItem.id == ItemType.SCP500)
            {
                float nearestDistance = 999;
                Patches.RagdollManagerPatch.RagdollInfo nearest = null;
                Player target = null;
                foreach (var ragdoll in Patches.RagdollManagerPatch.Ragdolls.Where(p => (DateTime.Now - p.DeathTime).TotalSeconds < 15).ToArray())
                {
                    if (ragdoll.Team == Team.SCP)
                        continue;
                    if (ragdoll.Team == Team.TUT)
                        continue;
                    target = Player.Get(ragdoll.ragdoll.owner.PlayerId);
                    if (target == null)
                        continue;
                    var distance = Vector3.Distance(player.Position, ragdoll.ragdoll.transform.position);
                    if (distance < MaximalDistance && distance < nearestDistance)
                    {
                        nearest = ragdoll;
                        nearestDistance = distance;
                    }
                }
                if (!target.GetEffectActive<CustomPlayerEffects.Amnesia>())
                {
                    if (nearestDistance != 999)
                    {
                        player.SetGUI("u500", Mistaken.Base.GUI.PseudoGUIHandler.Position.TOP, $"Wpisz '.u500' w konsoli(~) aby <color=yellow>wskrzesić</color> {target.Nickname} ({15 - Math.Floor((DateTime.Now - nearest.DeathTime).TotalSeconds)}s)");
                    }
                    else
                        player.SetGUI("u500", Mistaken.Base.GUI.PseudoGUIHandler.Position.TOP, null);
                }
                yield return Timing.WaitForSeconds(1);
            }
            player.SetGUI("u500", Mistaken.Base.GUI.PseudoGUIHandler.Position.TOP, null);
        }
    }
}
