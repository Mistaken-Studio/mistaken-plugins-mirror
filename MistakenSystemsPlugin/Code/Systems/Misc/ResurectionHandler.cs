using Exiled.API.Enums;
using Exiled.API.Features;
using Gamer.Diagnostics;
using Gamer.Utilities;
using Grenades;
using MEC;
using Mirror;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using UnityEngine;

namespace Gamer.Mistaken.Systems.Misc
{
    internal class ResurectionHandler : Module
    {
        public ResurectionHandler(PluginHandler p) : base(p)
        {
        }

        public override string Name => "Resurection";
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.RestartingRound += this.Handle(() => Server_RestartingRound(), "RoundRestart");
            Exiled.Events.Handlers.Player.ChangingItem += this.Handle<Exiled.Events.EventArgs.ChangingItemEventArgs>((ev) => Player_ChangingItem(ev));
            Exiled.Events.Handlers.Player.UsingMedicalItem += this.Handle<Exiled.Events.EventArgs.UsingMedicalItemEventArgs>((ev) => Player_UsingMedicalItem(ev));
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.RestartingRound -= this.Handle(() => Server_RestartingRound(), "RoundRestart");
            Exiled.Events.Handlers.Player.ChangingItem -= this.Handle<Exiled.Events.EventArgs.ChangingItemEventArgs>((ev) => Player_ChangingItem(ev));
            Exiled.Events.Handlers.Player.UsingMedicalItem -= this.Handle<Exiled.Events.EventArgs.UsingMedicalItemEventArgs>((ev) => Player_UsingMedicalItem(ev));
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
                catch(System.Exception ex)
                {
                    Log.Error("Failed to get ragdoll distance");
                    Log.Error(ex.Message);
                    Log.Error(ex.StackTrace);
                }
            }
            if(nearestDistance != 999)
            {
                var target = Player.Get(nearest.ragdoll.owner.PlayerId);
                if (Resurected.Contains(target.UserId))
                {
                    player.ShowHintPulsating("Nie udało się wskrzesić gracza | Gracz może zostać wskrzeszony raz na rundę", 5, true, true);
                    return false;
                }
                if (Resurections.Where(i => i == target.UserId).Count() > 3)
                {
                    player.ShowHintPulsating("Nie udało się wskrzesić gracza | Możesz wskrzesić 3 osoby na rundę", 5, true, true);
                    return false;
                }
                player.EnableEffect<CustomPlayerEffects.Amnesia>(15);
                player.EnableEffect<CustomPlayerEffects.Ensnared>(11);
                player.ShowHintPulsating($"Używam <color=yellow>SCP 500</color> na {target.Nickname}", 10, true, true);
                Timing.CallDelayed(10, () =>
                {
                    if (player.Role == originalRole)
                    {
                        target = Player.Get(nearest.ragdoll.owner.PlayerId);
                        if (target == null || target.GameObject == null || !target.IsConnected)
                        {
                            player.ShowHintPulsating("Nie udało się wskrzesić gracza | Gracza nie ma na serwerze", 5, true, true);
                            return;
                        }
                        if (target.IsOverwatchEnabled)
                        {
                            player.ShowHintPulsating("Nie udało się wskrzesić gracza | Gracz chyba nie chce być wskrzeszony", 5, true, true);
                            return;
                        }
                        if (target.IsAlive)
                        {
                            player.ShowHintPulsating("Nie udało się wskrzesić gracza | Jesteś pewien że ten gracz jest martwy?", 5, true, true);
                            return;
                        }
                        Vector3 pos = nearest.ragdoll.transform.position;
                        NetworkServer.Destroy(nearest.ragdoll.gameObject);
                        Patches.RagdollManagerPatch.Ragdolls.Remove(nearest);
                        EVO.Handler.AddProgres(1010, player.UserId);
                        Resurected.Add(target.UserId);
                        Resurections.Add(player.UserId);
                        if (player.CurrentItem.durability == 1)
                            player.Inventory.items.Remove(player.CurrentItem);
                        else
                            player.Inventory.items.ModifyDuration(player.CurrentItemIndex, player.CurrentItem.durability - 1);
                        target.Role = nearest.Role;
                        Misc.SpawnProtectHandler.SpawnKillProtected.RemoveAll(i => i.Key == target.Id);
                        target.ClearInventory();
                        Timing.CallDelayed(0.5f, () =>
                        {
                            Misc.SpawnProtectHandler.SpawnKillProtected.RemoveAll(i => i.Key == target.Id);
                            target.Position = pos + Vector3.up;
                            target.ShowHintPulsating($"Zostałeś <color=yellow>wskrzeszony</color> przez {player.Nickname}", 5, true, true);
                            target.Health = 5;
                            target.AdrenalineHealth = 75;
                            target.EnableEffect<CustomPlayerEffects.Blinded>(10);
                            target.EnableEffect<CustomPlayerEffects.Deafened>(15);
                            target.EnableEffect<CustomPlayerEffects.Disabled>(30);
                            target.EnableEffect<CustomPlayerEffects.Concussed>(15);
                            target.EnableEffect<CustomPlayerEffects.Flashed>(5);
                        });
                    }
                });
                return true;
            }
            else
                player.SendConsoleMessage("[SCP 500] No targers in range", "red");
            return false;
        }


        private void Player_UsingMedicalItem(Exiled.Events.EventArgs.UsingMedicalItemEventArgs ev)
        {
            if (ev.Item == ItemType.SCP500 && ev.Player.ReferenceHub.playerEffectsController.GetEffect<CustomPlayerEffects.Amnesia>().Enabled)
                ev.IsAllowed = false;
        }

        private void Player_ChangingItem(Exiled.Events.EventArgs.ChangingItemEventArgs ev)
        {
            if(ev.NewItem.id == ItemType.SCP500) 
                Timing.RunCoroutine(Interface(ev.Player));   
        }

        private IEnumerator<float> Interface(Player player)
        {
            yield return Timing.WaitForSeconds(1);
            while(player?.CurrentItem.id == ItemType.SCP500)
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

                if (nearestDistance != 999)
                    player.ShowHint($"Wpisz '.u500' w konsoli(~) aby <color=yellow>wskrzesić</color> {target.Nickname} ({15 - Math.Floor((DateTime.Now - nearest.DeathTime).TotalSeconds)}s)", true, 2, true);
                yield return Timing.WaitForSeconds(1);
            }
        }
    }
}
