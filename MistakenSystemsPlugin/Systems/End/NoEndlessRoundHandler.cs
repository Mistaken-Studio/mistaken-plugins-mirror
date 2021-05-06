using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Gamer.Diagnostics;
using Gamer.Mistaken.Base.GUI;
using Gamer.Mistaken.Base.Staff;
using Gamer.RoundLoggerSystem;
using Gamer.Utilities;
using MEC;
using System.Collections.Generic;
using System.Linq;

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
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Handle(() => Server_RoundStarted(), "RoundStart");
        }

        private void Server_RoundStarted()
        {
            this.RunCoroutine(Execute(), "Execute");
        }

        public static bool SpawnSamsara { get; private set; } = false;
        private IEnumerator<float> Execute()
        {
            SpawnSamsara = false;
            int startRoundId = RoundPlus.RoundId;
            if (UnityEngine.Random.Range(1, 101) < 25)
            {
                yield return Timing.WaitForSeconds(UnityEngine.Random.Range(25, 31) * 60);
                if (!Round.IsStarted || startRoundId != RoundPlus.RoundId)
                    yield break;
                SpawnSamsara = true;
                Systems.Utilities.API.Map.RespawnLock = true;
                Respawning.RespawnManager.Singleton._curSequence = Respawning.RespawnManager.RespawnSequencePhase.RespawnCooldown;
                Respawning.RespawnManager.Singleton._timeForNextSequence = (float)Respawning.RespawnManager.Singleton._stopwatch.Elapsed.TotalSeconds + 30;
                this.CallDelayed(30 + 17, () =>
                {
                    if (startRoundId == RoundPlus.RoundId)
                        SpawnAsSamsara();
                }, "SpawnSamsaraWithChance");
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
            RoundLogger.Log("TAU-5", "WARHEAD", $"Warhead forced");
        }

        public static void SpawnAsSamsara(List<Player> toSpawn = null)
        {
            var players = RealPlayers.Get(RoleType.Spectator).ToArray().Shuffle();
            if (toSpawn == null)
                toSpawn = new List<Player>();
            foreach (var player in players)
            {
                if (player.IsOverwatchEnabled)
                    continue;
                if (toSpawn.Contains(player))
                    continue;
                if (player.IsActiveDev())
                    toSpawn.Add(player);
                else if (toSpawn.Count < 8)
                    toSpawn.Add(player);
            }
            Respawning.NamingRules.UnitNamingRules.TryGetNamingRule(Respawning.SpawnableTeamType.NineTailedFox, out Respawning.NamingRules.UnitNamingRule unitnamingrule);
            unitnamingrule.GenerateNew(Respawning.SpawnableTeamType.NineTailedFox, out string unit);
            Map.ChangeUnitColor(Respawning.RespawnManager.Singleton.NamingManager.AllUnitNames.Count - 1, "#C00");
            foreach (var player in toSpawn)
                SpawnAsSamsara(player, unit);
            string unitnumber = unit.Split('-')[1];
            int scps = RealPlayers.List.Where(p => p.Team == Team.SCP && p.Role != RoleType.Scp0492).Count();
            Cassie.GlitchyMessage($"MTFUNIT TAU 5 DESIGNATED NATO_{unit[0]} {unitnumber} HASENTERED ALLREMAINING AWAITINGRECONTAINMENT {scps} SCPSUBJECT{(scps == 1 ? "" : "S")}", 0.3f, 0.1f);
            Systems.Utilities.API.Map.RespawnLock = true;
        }

        public static void SpawnAsSamsara(Player player, string unit)
        {
            RoundLogger.Log("TAU-5", "SPAWN", $"{player.PlayerToString()} was spawned as TAU-5 Samsara");
            player.Role = RoleType.NtfCommander;
            player.ReferenceHub.characterClassManager.NetworkCurUnitName = unit;
            Gamer.Utilities.BetterCourotines.CallDelayed(0.5f, () =>
            {
                var items = player.Inventory.items;
                for (int i = 0; i < items.Count; i++)
                {
                    if (items[i].id.IsKeycard())
                        items.RemoveAt(i);
                }
                player.AddItem(ItemType.KeycardO5);
                Gamer.Utilities.BetterCourotines.CallDelayed(0.5f, () =>
                {
                    player.Health *= 5;
                    player.ArtificialHealth = 50;
                    player.CustomInfo = "Tau-5 Samsara";
                    Base.CustomInfoHandler.Set(player, "TAU-5", "Tau-5 Samsara", false);
                    player.Ammo[(int)AmmoType.Nato556] = 500;
                    player.Ammo[(int)AmmoType.Nato9] = 500;
                    player.Ammo[(int)AmmoType.Nato762] = 500;
                    player.Inventory.items.ModifyDuration(player.Inventory.items.FindIndex(i => i.id == ItemType.WeaponManagerTablet), 5000);
                    Systems.Shield.ShieldedManager.Add(new Shield.Shielded(player, 50, 0.25f, 30, 0, -1));
                    Gamer.Utilities.BetterCourotines.CallDelayed(8f, () =>
                    {
                        player.SetGUI("tau-5", Mistaken.Base.GUI.PseudoGUIHandler.Position.MIDDLE, "<size=200%>Jesteś <color=blue>Tau-5 Samsara</color></size><br>Twoje zadanie: <color=red>Zneutralizować wszystko poza personelem fundacji</color>", 10);
                    }, "Samsara.SpawnAsGUI");
                }, "Samsara.SpawnAsLate");
            }, "Samsara.SpawnAs");
        }
    }
}
