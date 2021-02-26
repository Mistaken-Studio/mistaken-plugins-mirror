using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
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
using Gamer.Diagnostics;

namespace Gamer.Mistaken.Systems.End
{
    internal class NoEndlessRoundHandler : Module
    {
        public NoEndlessRoundHandler(PluginHandler p) : base(p)
        {
        }

        public override string Name => "NoEndlessRound";
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.RoundStarted += this.Handle(() => Server_RoundStarted(), "RoundStart");
            //Exiled.Events.Handlers.Player.ChangingRole += this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
            //Exiled.Events.Handlers.Server.RespawningTeam += this.Handle<Exiled.Events.EventArgs.RespawningTeamEventArgs>((ev) => Server_RespawningTeam(ev));
            //Exiled.Events.Handlers.Map.AnnouncingNtfEntrance += this.Handle<Exiled.Events.EventArgs.AnnouncingNtfEntranceEventArgs>((ev) => Map_AnnouncingNtfEntrance(ev));
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Handle(() => Server_RoundStarted(), "RoundStart");
            //Exiled.Events.Handlers.Player.ChangingRole -= this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
            //Exiled.Events.Handlers.Server.RespawningTeam -= this.Handle<Exiled.Events.EventArgs.RespawningTeamEventArgs>((ev) => Server_RespawningTeam(ev));
            //Exiled.Events.Handlers.Map.AnnouncingNtfEntrance -= this.Handle<Exiled.Events.EventArgs.AnnouncingNtfEntranceEventArgs>((ev) => Map_AnnouncingNtfEntrance(ev));
        }

        private void Map_AnnouncingNtfEntrance(Exiled.Events.EventArgs.AnnouncingNtfEntranceEventArgs ev)
        {
            if (!SpawnSamsara)
                return;
            ev.IsAllowed = false;
            if(ev.ScpsLeft == 0)
                Cassie.GlitchyMessage($"MTFUNIT Tau 5 DESIGNATED {ev.UnitName} {ev.UnitNumber} HASENTERED ALLREMAINING NOSCPSLEFT", 0.3f, 0.1f);
            else
                Cassie.GlitchyMessage($"MTFUNIT Tau 5 DESIGNATED {ev.UnitName} {ev.UnitNumber} HASENTERED ALLREMAINING AWAITINGRECONTAINMENT {ev.ScpsLeft} SCPSUBJECT{(ev.ScpsLeft == 1 ? "" : "S")}", 0.3f, 0.1f);
        }

        private void Server_RespawningTeam(Exiled.Events.EventArgs.RespawningTeamEventArgs ev)
        {
            if (!SpawnSamsara)
                return;
            ev.NextKnownTeam = Respawning.SpawnableTeamType.NineTailedFox;
            ev.MaximumRespawnAmount = 8;
            SamstraNewUnits = ev.Players.ToHashSet();
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            if (ev.NewRole == RoleType.Spectator)
                SamstraUnits.Remove(ev.Player);
            if (!SpawnSamsara)
                return;
            if(SamstraNewUnits.Contains(ev.Player) && ev.NewRole.GetTeam() == Team.MTF)
            {
                ev.Items.RemoveAll(i => i.IsKeycard());
                ev.Items.Add(ItemType.KeycardO5);
                MEC.Timing.CallDelayed(0.5f, () =>
                {
                    Systems.Utilities.API.Map.RespawnLock = true;
                    ev.Player.Health *= 5;
                    ev.Player.AdrenalineHealth = 50;
                    ev.Player.CustomInfo = "Tau-5 Samsara";
                    ev.Player.Ammo[(int)AmmoType.Nato556] = 500;
                    ev.Player.Ammo[(int)AmmoType.Nato9] = 500;
                    ev.Player.Ammo[(int)AmmoType.Nato762] = 500;
                    ev.Player.Inventory.items.ModifyDuration(ev.Player.Inventory.items.FindIndex(i => i.id == ItemType.WeaponManagerTablet), 5000);
                    Systems.Shield.ShieldedManager.Add(new Shield.Shielded(ev.Player, 50, 0.25f, 30, 0, -1));
                    MEC.Timing.CallDelayed(8f, () =>
                    {
                        ev.Player.ShowHint("<size=200%>Jesteś <color=blue>Tau-5 Samsara</color></size><br>Twoje zadanie: <color=red>Zneutralizować wszystko poza personelem fundacji</color>", true, 10, true);
                    });
                    SamstraUnits.Add(ev.Player);
                    SamstraNewUnits.Remove(ev.Player);
                });
            }
        }

        private void Server_RoundStarted()
        {
            Timing.RunCoroutine(Execute());
        }
        private static HashSet<Player> SamstraNewUnits = new HashSet<Player>();
        private static readonly HashSet<Player> SamstraUnits = new HashSet<Player>();

        public static bool SpawnSamsara = false;
        private IEnumerator<float> Execute()
        {
            SamstraUnits.Clear();
            SpawnSamsara = false;
            int startRoundId = RoundPlus.RoundId;
            if (UnityEngine.Random.Range(1, 101) < 25 && false)
            {
                yield return Timing.WaitForSeconds(UnityEngine.Random.Range(25, 31) * 60);
                if (!Round.IsStarted || startRoundId != RoundPlus.RoundId)
                    yield break;
                SpawnSamsara = true;
                Respawning.RespawnManager.Singleton._timeForNextSequence = (float)Respawning.RespawnManager.Singleton._stopwatch.Elapsed.TotalSeconds + 30;
            }
            else
                yield return Timing.WaitForSeconds(30 * 60);
            yield return Timing.WaitForSeconds(300);
            if (!Round.IsStarted || startRoundId != RoundPlus.RoundId)
                yield break;
            Cassie.GlitchyMessage("WARHEAD OVERRIDE . ALPHA WARHEAD SEQUENCE ENGAGED", 1, 1);
            yield return Timing.WaitForSeconds(1);
            while (Cassie.IsSpeaking)
                yield return Timing.WaitForOneFrame;
            Systems.Misc.BetterWarheadHandler.Warhead.StopLock = true;
            Warhead.Start();
        }

        public static void SpawnAsSamsara(List<Player> toSpawn = null)
        {
            var players = RealPlayers.Get(RoleType.Spectator).ToArray().Shuffle();
            if(toSpawn == null) 
                toSpawn = new List<Player>();
            foreach (var player in players)
            {
                if (player.IsOverwatchEnabled)
                    continue;
                if (toSpawn.Contains(player))
                    continue;
                if (player.IsDev())
                    toSpawn.Add(player);
                else if (toSpawn.Count < 8)
                    toSpawn.Add(player);
            }
            foreach (var player in toSpawn)
                SpawnAsSamsara(player);
        }

        public static void SpawnAsSamsara(Player player)
        {
            player.Role = RoleType.NtfCommander;
            MEC.Timing.CallDelayed(0.5f, () =>
            {
                var items = player.Inventory.items;
                for (int i = 0; i < items.Count; i++)
                {
                    if(items[i].id.IsKeycard())
                        items.RemoveAt(i);
                }
                player.AddItem(ItemType.KeycardO5);
                MEC.Timing.CallDelayed(0.5f, () =>
                {
                    player.Health *= 5;
                    player.AdrenalineHealth = 50;
                    player.CustomInfo = "Tau-5 Samsara";
                    player.Ammo[(int)AmmoType.Nato556] = 500;
                    player.Ammo[(int)AmmoType.Nato9] = 500;
                    player.Ammo[(int)AmmoType.Nato762] = 500;
                    player.Inventory.items.ModifyDuration(player.Inventory.items.FindIndex(i => i.id == ItemType.WeaponManagerTablet), 5000);
                    Systems.Shield.ShieldedManager.Add(new Shield.Shielded(player, 50, 0.25f, 30, 0, -1));
                    MEC.Timing.CallDelayed(8f, () =>
                    {
                        player.ShowHint("<size=200%>Jesteś <color=blue>Tau-5 Samsara</color></size><br>Twoje zadanie: <color=red>Zneutralizować wszystko poza personelem fundacji</color>", true, 10, true);
                    });
                    SamstraUnits.Add(player);
                });
            });
        }
    }
}
