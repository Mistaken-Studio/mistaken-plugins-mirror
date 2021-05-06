using Exiled.API.Features;
using Gamer.Diagnostics;
using Gamer.Mistaken.Base.GUI;
using Gamer.Mistaken.Systems.End;
using Gamer.Utilities;
using MEC;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gamer.Mistaken.Systems.GUI
{
    internal class SpecInfoHandler : Module
    {
        //public override bool Enabled => false;
        public SpecInfoHandler(PluginHandler p) : base(p)
        {
            plugin.RegisterTranslation("time_info", "Current time: <color=yellow>{0}</color>");
            plugin.RegisterTranslation("lcz_info", "Light Containment Zone decontamination in <color=yellow>{0}</color>m <color=yellow>{1}</color>s");
            plugin.RegisterTranslation("lcz_info_decontcaminated", "Light Containment Zone <color=yellow>DECONTAMINATED</color>");
            plugin.RegisterTranslation("warhead_info", "Warhead detonation in proggress <color=yellow>{0}</color>s");
            plugin.RegisterTranslation("warhead_info_detonated", "Warhead <color=yellow>detonated</color>");
            plugin.RegisterTranslation("round_info", "Round is <color=yellow>{0}</color>m <color=yellow>{1}</color>s long");
            plugin.RegisterTranslation("respawn_info", "<size=150%>Respawn in <color=yellow>{0}</color>m <color=yellow>{1}</color>s</size>");
            plugin.RegisterTranslation("spectator_info", "You are spectator with <color=yellow>{0}</color> other players");
            plugin.RegisterTranslation("players_info", "Server has <color=yellow>{0}</color> for max <color=yellow>{1}</color> players");
            plugin.RegisterTranslation("dead_time_info", "You are dead for <color=yellow>{0}</color>m <color=yellow>{1}</color>s");
            plugin.RegisterTranslation("generator_info", "Generatory: <color=yellow>{0}</color>/<color=yellow>5</color>");
            plugin.RegisterTranslation("overcharge_info", "Rekontaminacja SCP 079 za <color=yellow>{0}</color>s");
            plugin.RegisterTranslation("admin_warhead_info", "Warhead status: {0}   |   Warhead Button Open: <color=yellow>{1}</color><br>Warhead Last Starter: <color=yellow>({2}) {3}</color>   |   <color=yellow>({4}) {5}</color> :Warhead Last Stoper<br>Warhead Lock Start: <color=yellow>{6}</color>   |   <color=yellow>{7}</color> :Warhead Lock Stop");
            plugin.RegisterTranslation("admin_info", "{0}<br><size=50%>MTF Tickets: <color=yellow>{1}</color>   |   <color=yellow>{2}</color> :CI Tickets</size><br><size=50%>{3}</size>");

            plugin.RegisterTranslation("recontainment_not_ready", "SCP 106 <color=yellow><b>nie</b> gotowy</color> do rekontaminacji");
            plugin.RegisterTranslation("recontainment_ready", "SCP 106 <color=yellow>gotowy</color> do rekontaminacji");
            plugin.RegisterTranslation("recontainment_contained", "SCP 106 <color=yellow>zabezpieczony</color>");

            plugin.RegisterTranslation("classd_decontamination", "Dekontaminacja cel Klas D za <color=yellow>{0}</color>m <color=yellow>{1}</color>s");
            plugin.RegisterTranslation("classd_decontaminated", "Cele Klas D <color=yellow>zdekontaminowane</color>");

            plugin.RegisterTranslation("respawn_none", "<color=#8f0000><b>None? will respawn</b></color> in <color=yellow>{0}</color>s");
            plugin.RegisterTranslation("respawn_ci_will_respawn", "You <color=yellow>will</color> respawn as <color=#1d6f00>Chaos Insurgent</color>");
            plugin.RegisterTranslation("respawn_ci_will_not_respawn", "You <color=yellow>will <b>not</b></color> respawn");
            plugin.RegisterTranslation("respawn_ci_respawn", "<color=#1d6f00><size=200%><b>Car is arriving</b></color> in <color=yellow>{0}</size>s</color><br><color=yellow>{1}</color> Insurgents will respawn<br><size=50%><color=yellow>{2}</color> players will not respawn</size><br>{3}");
            plugin.RegisterTranslation("respawn_mtf_will_respawn_cadet", "<color=#61beff>Ninetailedfox Cadet</color><br>Your <color=#1200ff>Commander</color> <color=yellow>will</color> be {0}");
            plugin.RegisterTranslation("respawn_mtf_will_respawn_lieutenant", "<color=#0096ff>Ninetailedfox Lieutenant</color><br>Your <color=#1200ff>Commander</color> <color=yellow>will</color> be {0}");
            plugin.RegisterTranslation("respawn_mtf_will_respawn_commander", "<color=#1200ff>Ninetailedfox Commander</color>");
            plugin.RegisterTranslation("respawn_mtf_will_respawn", "You <color=yellow>will</color> respawn as ");
            plugin.RegisterTranslation("respawn_mtf_will_not_respawn", "You <color=yellow>will <b>not</b></color> respawn<br><color=#1200ff>Commander</color> <color=yellow>will</color> be {0}");
            plugin.RegisterTranslation("respawn_mtf_respawn", "<color=#0096ff><size=200%><b>Helicopter is landing</b></color> <br>in <color=yellow>{0}</color>s</size><br><color=yellow>{1}</color> Ninetailefox will respawn<br><size=50%><color=yellow>{2}</color> players will not respawn</size><br>{3}");
        }

        public override string Name => "SpecInfo";
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.RoundStarted += this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Server.RestartingRound += this.Handle(() => Server_RestartingRound(), "RoundRestart");
            Exiled.Events.Handlers.Server.RespawningTeam += this.Handle<Exiled.Events.EventArgs.RespawningTeamEventArgs>((ev) => Server_RespawningTeam(ev));
            Exiled.Events.Handlers.Player.ChangingRole += this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Server.RestartingRound -= this.Handle(() => Server_RestartingRound(), "RoundRestart");
            Exiled.Events.Handlers.Server.RespawningTeam -= this.Handle<Exiled.Events.EventArgs.RespawningTeamEventArgs>((ev) => Server_RespawningTeam(ev));
            Exiled.Events.Handlers.Player.ChangingRole -= this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            if (ev.NewRole != RoleType.Spectator)
                ev.Player.SetGUI("specInfo", Base.GUI.PseudoGUIHandler.Position.MIDDLE, null);
        }

        //private List<Player> SpawnQueue = new List<Player>();
        private int RespawnQueueSeed = -1;
        private void Server_RespawningTeam(Exiled.Events.EventArgs.RespawningTeamEventArgs ev)
        {
            if (Respawning.RespawnWaveGenerator.SpawnableTeams.TryGetValue(ev.NextKnownTeam, out Respawning.SpawnableTeam spawnableTeam) || ev.NextKnownTeam == Respawning.SpawnableTeamType.None)
            {
                int num = Respawning.RespawnTickets.Singleton.GetAvailableTickets(ev.NextKnownTeam);
                if (num == 0)
                {
                    num = 5;
                    Respawning.RespawnTickets.Singleton.GrantTickets(Respawning.SpawnableTeamType.ChaosInsurgency, 5, true);
                }
                ev.MaximumRespawnAmount = Mathf.Min(num, spawnableTeam.MaxWaveSize);
            }
            ev.MaximumRespawnAmount = Mathf.Max(ev.MaximumRespawnAmount, 0);
            while (ev.Players.Count > ev.MaximumRespawnAmount)
                ev.Players.RemoveAt(ev.Players.Count - 1);
            ev.Players.Shuffle(RespawnQueueSeed);
            //ev.Players.Clear();
            //foreach (var item in SpawnQueue)
            //    ev.Players.Add(item);
            this.CallDelayed(20, () =>
            {
                RespawnQueueSeed = -1;
            }, "RespawningTeam");
        }
        private void Server_RestartingRound()
        {
            RespawnQueueSeed = -1;
        }
        private void Server_RoundStarted()
        {
            cache_maxCI = Respawning.RespawnWaveGenerator.SpawnableTeams[Respawning.SpawnableTeamType.ChaosInsurgency].MaxWaveSize;
            cache_maxMTF = Respawning.RespawnWaveGenerator.SpawnableTeams[Respawning.SpawnableTeamType.NineTailedFox].MaxWaveSize;

            this.RunCoroutine(TTRUpdate(), "TTRUpdate");
            this.RunCoroutine(UpdateCache(), "UpdateCache");
            this.CallDelayed(45, () =>
            {
                Is106 = RealPlayers.List.Any(p => p.Role == RoleType.Scp106);
            }, "Update106Info");
        }
        private static bool Is106 = false;
        private readonly Dictionary<int, (Player Player, RoleType Role)> spawnQueue = new Dictionary<int, (Player Player, RoleType Role)>();
        private static readonly Dictionary<int, string> DeathMessages = new Dictionary<int, string>();
        public static void AddDeathMessage(Player player, string message)
        {
            if (DeathMessages.ContainsKey(player.Id))
                DeathMessages.Remove(player.Id);
            DeathMessages.Add(player.Id, message);
            Gamer.Utilities.BetterCourotines.CallDelayed(15, () => DeathMessages.Remove(player.Id), "SpecInfo.AddDeathMessage");
        }

        public static int cache_ticketsCI;
        public static int cache_ticketsMTF;
        public static int cache_maxCI;
        public static int cache_maxMTF;

        public static Generator079 cache_nearestGenerator;

        public static int Dynamic_maxRespawnCI => Math.Min(cache_maxCI, cache_ticketsCI == 0 ? 5 : cache_ticketsCI);
        public static int Dynamic_maxRespawnMTF => Math.Min(cache_maxMTF, cache_ticketsMTF);

        private IEnumerator<float> UpdateCache()
        {
            yield return Timing.WaitForSeconds(1);
            int rid = RoundPlus.RoundId;
            while (Round.IsStarted && rid == RoundPlus.RoundId)
            {
                yield return Timing.WaitForSeconds(5);
                cache_ticketsCI = Respawning.RespawnTickets.Singleton.GetAvailableTickets(Respawning.SpawnableTeamType.ChaosInsurgency);
                cache_ticketsMTF = Respawning.RespawnTickets.Singleton.GetAvailableTickets(Respawning.SpawnableTeamType.NineTailedFox);

                cache_nearestGenerator = null;
                foreach (var generator in Generator079.Generators)
                {
                    if (generator.NetworkisTabletConnected)
                    {
                        if ((cache_nearestGenerator?.NetworkremainingPowerup ?? float.MaxValue) > generator.NetworkremainingPowerup)
                            cache_nearestGenerator = generator;
                    }
                }
            }
        }

        private IEnumerator<float> TTRUpdate()
        {
            yield return Timing.WaitForSeconds(1);
            int rid = RoundPlus.RoundId;
            while (Round.IsStarted && rid == RoundPlus.RoundId)
            {
                yield return Timing.WaitForSeconds(1);
                if (!RealPlayers.Any(RoleType.Spectator))
                    continue;
                try
                {
                    var start = DateTime.Now;
                    var respawnManager = Respawning.RespawnManager.Singleton;

                    var toRespawn = RealPlayers.List.Where(p => p.IsDead && !p.IsOverwatchEnabled).Count();

                    var respawningCI = Math.Min(Dynamic_maxRespawnCI, toRespawn);
                    var notrespawningCI = toRespawn - respawningCI;

                    var respawningMTF = Math.Min(Dynamic_maxRespawnMTF, toRespawn);
                    var notrespawningMTF = toRespawn - respawningMTF;


                    var ttr = Mathf.RoundToInt(respawnManager._timeForNextSequence - (float)respawnManager._stopwatch.Elapsed.TotalSeconds);

                    var spectators = RealPlayers.Get(RoleType.Spectator).Count();

                    spawnQueue.Clear();
                    if (respawnManager._curSequence == Respawning.RespawnManager.RespawnSequencePhase.PlayingEntryAnimations)
                    {
                        if (Respawning.RespawnWaveGenerator.SpawnableTeams.TryGetValue(respawnManager.NextKnownTeam, out Respawning.SpawnableTeam spawnableTeam))
                        {
                            List<Player> list = NorthwoodLib.Pools.ListPool<Player>.Shared.Rent(RealPlayers.List.Where(p => p.IsDead && !p.IsOverwatchEnabled).OrderBy(rh => rh.ReferenceHub.characterClassManager.DeathTime));
                            int maxRespawnablePlayers = respawnManager.NextKnownTeam == Respawning.SpawnableTeamType.ChaosInsurgency ? Dynamic_maxRespawnCI : Dynamic_maxRespawnMTF;
                            maxRespawnablePlayers = Math.Max(maxRespawnablePlayers, 0);
                            while (list.Count > maxRespawnablePlayers)
                                list.RemoveAt(list.Count - 1);
                            if (RespawnQueueSeed == -1)
                                RespawnQueueSeed = UnityEngine.Random.Range(0, 10000);
                            list.Shuffle(RespawnQueueSeed);
                            RoleType classid;
                            foreach (var player in list)
                            {
                                try
                                {
                                    classid = spawnableTeam.ClassQueue[Mathf.Min(spawnQueue.Count, spawnableTeam.ClassQueue.Length - 1)];
                                    spawnQueue.Add(player.Id, (player, classid));
                                }
                                catch (Exception)
                                {
                                }
                            }
                            NorthwoodLib.Pools.ListPool<Player>.Shared.Return(list);
                        }
                    }

                    string ttrPlayer = InformTTR(ttr, spectators, false, "");
                    foreach (var player in RealPlayers.List.Where(p => p.IsDead && !Systems.Handler.NewPlayers.Contains(p.Id)))
                    {
                        if (!DeathMessages.TryGetValue(player.Id, out string message))
                            message = "";
                        if (respawnManager._curSequence == Respawning.RespawnManager.RespawnSequencePhase.PlayingEntryAnimations)
                        {
                            if (NoEndlessRoundHandler.SpawnSamsara)
                                InformRespawnSamsara(ttr, respawningMTF, notrespawningMTF, spawnQueue.ContainsKey(player.Id));
                            else if (respawnManager.NextKnownTeam == Respawning.SpawnableTeamType.ChaosInsurgency)
                                message += InformRespawnCI(ttr, respawningCI, notrespawningCI, spawnQueue.ContainsKey(player.Id));
                            else if (respawnManager.NextKnownTeam == Respawning.SpawnableTeamType.NineTailedFox)
                                message += InformRespawnMTF(ttr, respawningMTF, notrespawningMTF, spawnQueue.ContainsKey(player.Id) ? spawnQueue[player.Id].Role : RoleType.None, spawnQueue.FirstOrDefault(i => i.Value.Role == RoleType.NtfCommander).Value.Player?.GetDisplayName() ?? "UNKNOWN");
                            else
                                message += InformRespawnNone(ttr);
                        }
                        if (player.CheckPermissions(PlayerPermissions.AdminChat))
                        {
                            string adminMsg = "";
                            if (End.OverwatchHandler.InLongOverwatch.Contains(player.UserId))
                                adminMsg = "[<color=red>LONG OVERWATCH <b><color=yellow>ACTIVE</color></b></color>]";
                            else if (End.OverwatchHandler.InOverwatch.TryGetValue(player.UserId, out DateTime checkTime))
                            {
                                var diff = checkTime.AddMinutes(5) - DateTime.Now;
                                adminMsg = $"[<color=yellow>OVERWATCH <b>ACTIVE</b> | {diff.Minutes:00}<color=yellow>:</color>{diff.Seconds:00}</color>]";
                            }
                            message += InformTTR(ttr, spectators, true, adminMsg);
                        }
                        else
                            message += ttrPlayer;
                        //player.ShowHint(message, 2);
                        player.SetGUI("specInfo", Base.GUI.PseudoGUIHandler.Position.MIDDLE, "<br><br><br><br><br>" + message);
                    }
                    Diagnostics.MasterHandler.LogTime("SpecInfoHandler", "TTRUpdate", start, DateTime.Now);
                }
                catch (System.Exception ex)
                {
                    Log.Error(ex.Message);
                    Log.Error(ex.StackTrace);
                }
            }
        }

        private string InformTTR(/*Player player, */float ttr, int spectators, bool admin, string masterAdminMessage)
        {
            //string masterAdminMessage = "";//"[<color=yellow>No warning active</color>]";
            if (Round.IsLocked)
                masterAdminMessage = "[<color=yellow>ROUND LOCK <b>ACTIVE</b></color>]";
            else if (Utilities.API.Map.RespawnLock)
                masterAdminMessage = "[<color=yellow>RESPAWN LOCK <b>ACTIVE</b></color>]";
            else if (RealPlayers.List.Count() < 4)
                masterAdminMessage = "[<color=yellow>LESS THAN 4 PLAYERS | <b>NOT SAVING</b> ACTIVITY</color>]";

            var systemTimeString = plugin.ReadTranslation("time_info", DateTime.Now.ToString("HH:mm:ss").Replace(":", "</color>:<color=yellow>"));
            //var deadTime = DateTime.Now - (new DateTime(player.ReferenceHub.characterClassManager.DeathTime));
            var lczTime = (float)LightContainmentZoneDecontamination.DecontaminationController.Singleton.DecontaminationPhases.First(d => d.Function == LightContainmentZoneDecontamination.DecontaminationController.DecontaminationPhase.PhaseFunction.Final).TimeTrigger - (float)LightContainmentZoneDecontamination.DecontaminationController.GetServerTime;
            var lczString = plugin.ReadTranslation("lcz_info", ((lczTime - (lczTime % 60)) / 60).ToString("00"), Mathf.RoundToInt(lczTime % 60).ToString("00"));
            if (lczTime < 0)
                lczString = plugin.ReadTranslation("lcz_info_decontcaminated");
            if (Warhead.IsInProgress)
                lczString = plugin.ReadTranslation("warhead_info", Warhead.DetonationTimer.ToString("00"));
            if (Warhead.IsDetonated)
                lczString = plugin.ReadTranslation("lcz_info_decontcaminated");
            var roundTimeString = plugin.ReadTranslation("round_info", Round.ElapsedTime.Minutes.ToString("00"), Round.ElapsedTime.Seconds.ToString("00"));
            var repsawnString = plugin.ReadTranslation("respawn_info", ((ttr - (ttr % 60)) / 60).ToString("00"), (ttr % 60).ToString("00"));
            var specatorString = spectators < 2 ? "Jesteś <color=yellow>jedynym</color> martwym graczem" : plugin.ReadTranslation("spectator_info", spectators - 1);
            var playersString = plugin.ReadTranslation("players_info", PlayerManager.players.Count, CustomNetworkManager.slots);
            //var deadTimeString = plugin.ReadTranslation("dead_time_info", deadTime.Minutes.ToString("00"), deadTime.Seconds.ToString("00"));
            var generatorString = plugin.ReadTranslation("generator_info", Patches.SCP079RecontainPatch.Recontained ? "5" : Map.ActivatedGenerators.ToString()) + (cache_nearestGenerator == null ? "" : $"(<color=yellow>{Math.Round(cache_nearestGenerator.remainingPowerup % 80)}</color>s)");
            var overchargeString = plugin.ReadTranslation("overcharge_info", Patches.SCP079RecontainPatch.ErrorMode ? $"[<color=red><b>ERROR</b></color>|Code: {(Patches.SCP079RecontainPatch.Recontained ? 1 : 0)}{(Patches.SCP079RecontainPatch.Waiting ? 1 : 0)}{Patches.SCP079RecontainPatch.SecondsLeft}]" : (Exiled.Events.Handlers.CustomEvents.SCP079.IsRecontainmentPaused ? $"<color=red>{Exiled.Events.Handlers.CustomEvents.SCP079.TimeToRecontainment}</color>" : $"<color=yellow>{Exiled.Events.Handlers.CustomEvents.SCP079.TimeToRecontainment}</color>"));
            var genString = Exiled.Events.Handlers.CustomEvents.SCP079.IsBeingRecontained ? overchargeString : generatorString;
            var recontainmentReadyString = plugin.ReadTranslation("recontainment_ready");
            var recontainmentNotReadyString = plugin.ReadTranslation("recontainment_not_ready");
            var recontainmentContainedyString = plugin.ReadTranslation("recontainment_contained");
            var recontainmentString = MapPlus.FemurBreaked ? recontainmentContainedyString : (MapPlus.Lured ? recontainmentReadyString : recontainmentNotReadyString);
            var classDTime = Misc.ClassDCellsDecontaminationHandler.DecontaminatedIn;
            var classDTime_s = classDTime % 60;
            var classDString = "[<color=yellow>REDACTED</color>]";//Misc.ClassDCellsDecontaminationHandler.Decontaminated ? plugin.ReadTranslation("classd_decontaminated") : plugin.ReadTranslation("classd_decontamination", (classDTime - classDTime_s) / 60, (classDTime_s < 10 ? "0" : "") + classDTime_s);
            var miscString = Is106 ? recontainmentString : classDString;
            var adminWarheadString = plugin.ReadTranslation("admin_warhead_info", (Warhead.LeverStatus ? (Warhead.CanBeStarted ? "<color=green>Ready</color>" : "<color=blue>Cooldown</color>") : "<color=red>Disabled</color>"), Warhead.IsKeycardActivated, Misc.BetterWarheadHandler.Warhead.LastStartUser?.Id.ToString() ?? "?", Misc.BetterWarheadHandler.Warhead.LastStartUser?.Nickname ?? "UNKNOWN", Misc.BetterWarheadHandler.Warhead.LastStopUser?.Id.ToString() ?? "?", Misc.BetterWarheadHandler.Warhead.LastStopUser?.Nickname ?? "UNKNOWN", Misc.BetterWarheadHandler.Warhead.StartLock, Misc.BetterWarheadHandler.Warhead.StopLock);
            var adminString = plugin.ReadTranslation("admin_info", masterAdminMessage, cache_ticketsMTF, cache_ticketsCI, adminWarheadString);
            return $"<br><br><br>{repsawnString}<br>{specatorString}<br><size=50%>{roundTimeString}   |   [<color=yellow>REDACTED</color>]   |   {playersString}<br>{lczString}   |   {systemTimeString}<br>{genString}   |   {miscString}</size>" + (admin ? $"<br>{adminString}" : "");
        }

        private string InformRespawnNone(float ttr)
        {
            return plugin.ReadTranslation("respawn_none", (ttr % 60).ToString("00"));
        }
        private string InformRespawnSamsara(float ttr, int respawningSamsara, int notrespawningSamsara, bool willRespawn)
        {
            string roleString = willRespawn ? "<color=yellow>Przylecisz</color> jako <color=#1200ff>Jednostka Samsary</color>" : "<color=yellow><b>Nie</b> przylecisz</color>";
            return $"<color=#0096ff><size=200%><b>Helikoper Samsary łąduje</b></color> <br>za <color=yellow>{(ttr % 60):00}</color>s</size><br><color=yellow>{respawningSamsara}</color> jednostek Samsary przyleci<br><size=50%><color=yellow>{notrespawningSamsara}</color> graczy nie przyleci</size><br>{roleString}";
        }
        private string InformRespawnMTF(float ttr, int respawningMTF, int notrespawningMTF, RoleType expectedRole, string Commander)
        {
            string roleString = (expectedRole == RoleType.None ? plugin.ReadTranslation("respawn_mtf_will_not_respawn", Commander) : plugin.ReadTranslation("respawn_mtf_will_respawn"));
            switch (expectedRole)
            {
                case RoleType.NtfCadet:
                    roleString += plugin.ReadTranslation("respawn_mtf_will_respawn_cadet", Commander);
                    break;
                case RoleType.NtfLieutenant:
                    roleString += plugin.ReadTranslation("respawn_mtf_will_respawn_lieutenant", Commander);
                    break;
                case RoleType.NtfCommander:
                    roleString += plugin.ReadTranslation("respawn_mtf_will_respawn_commander");
                    break;
            }

            return plugin.ReadTranslation("respawn_mtf_respawn", (ttr % 60).ToString("00"), respawningMTF, notrespawningMTF, roleString);
        }
        private string InformRespawnCI(float ttr, int respawningCI, int notrespawningCI, bool willRespawn)
        {
            string roleString = willRespawn ? plugin.ReadTranslation("respawn_ci_will_respawn") : plugin.ReadTranslation("respawn_ci_will_not_respawn");
            return plugin.ReadTranslation("respawn_ci_respawn", (ttr % 60).ToString("00"), respawningCI, notrespawningCI, roleString);
        }
    }
}
