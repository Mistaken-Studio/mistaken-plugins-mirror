using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Gamer.API.CustomClass;
using Gamer.Diagnostics;
using Gamer.Mistaken.Base.GUI;
using Gamer.Mistaken.Base.Staff;
using Gamer.RoundLoggerSystem;
using Gamer.Utilities;
using MEC;
using Respawning;
using Respawning.NamingRules;
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
            Tau5CustomClass = CustomClass.CustomClasses.First(i => i.ClassSessionVarType == Main.SessionVarType.CC_TAU5);
            this.RunCoroutine(Execute(), "Execute");
        }
        public static CustomClass Tau5CustomClass { get; private set; }
        public static bool SpawnSamsara { get; private set; } = false;
        private IEnumerator<float> Execute()
        {
            SpawnSamsara = false;
            int startRoundId = RoundPlus.RoundId;
            if (UnityEngine.Random.Range(1, 101) < 25)
            {
                int rand = UnityEngine.Random.Range(25, 31);
                RoundLogger.Log("TAU-5", "DECISION", $"TAU-5 will spawn in T-{rand} minutes");
                yield return Timing.WaitForSeconds(rand * 60);
                if (!Round.IsStarted || startRoundId != RoundPlus.RoundId)
                    yield break;
                SpawnSamsara = true;
                Base.Utilities.API.Map.RespawnLock = true;
                Respawning.RespawnManager.Singleton._curSequence = Respawning.RespawnManager.RespawnSequencePhase.RespawnCooldown;
                Respawning.RespawnManager.Singleton._timeForNextSequence = (float)Respawning.RespawnManager.Singleton._stopwatch.Elapsed.TotalSeconds + 30;
                this.CallDelayed(30 + 17, () =>
                {
                    if (startRoundId == RoundPlus.RoundId)
                        Spawn();
                }, "SpawnSamsaraWithChance");
            }
            else
            {
                RoundLogger.Log("TAU-5", "DECISION", $"TAU-5 will not spawn");
                yield return Timing.WaitForSeconds(30 * 60);
            }
            yield return Timing.WaitForSeconds(300);
            if (!Round.IsStarted || startRoundId != RoundPlus.RoundId)
                yield break;
            Cassie.GlitchyMessage("WARHEAD OVERRIDE . ALPHA WARHEAD SEQUENCE ENGAGED", 1, 1);
            yield return Timing.WaitForSeconds(1);
            while (Cassie.IsSpeaking)
                yield return Timing.WaitForOneFrame;
            Base.BetterWarheadHandler.Warhead.StopLock = true;
            Warhead.Start();
            RoundLogger.Log("TAU-5", "WARHEAD", $"Warhead forced");
        }
        public static void Spawn(List<Player> toSpawn = null)
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
            string unitName = "ERROR-99";
            if (UnitNamingRules.TryGetNamingRule(SpawnableTeamType.NineTailedFox, out var rule))
            {
                rule.GenerateNew(SpawnableTeamType.NineTailedFox, out unitName);
                Map.ChangeUnitColor(Respawning.RespawnManager.Singleton.NamingManager.AllUnitNames.Count - 1, "#C00");
            }
            foreach (var player in toSpawn)
                Tau5CustomClass.Spawn(player);
            string unitnumber = unitName.Split('-')[1];
            int scps = RealPlayers.List.Where(p => p.Team == Team.SCP && p.Role != RoleType.Scp0492).Count();
            Cassie.GlitchyMessage($"MTFUNIT TAU 5 DESIGNATED NATO_{unitName[0]} {unitnumber} HASENTERED ALLREMAINING AWAITINGRECONTAINMENT {scps} SCPSUBJECT{(scps == 1 ? "" : "S")}", 0.3f, 0.1f);
            Base.Utilities.API.Map.RespawnLock = true;
        }
    }
}
