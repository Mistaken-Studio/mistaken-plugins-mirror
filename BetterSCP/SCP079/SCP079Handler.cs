using Exiled.API.Features;
using Gamer.Diagnostics;
using Gamer.Mistaken.Base.GUI;
using Gamer.Mistaken.BetterSCP.SCP079.Commands;
using Gamer.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gamer.Mistaken.BetterSCP.SCP079
{
    class SCP079Handler : Module
    {
        public SCP079Handler(PluginHandler plugin) : base(plugin)
        {
            Translations.Ini(plugin);
            Gamer.Mistaken.Systems.InfoMessage.InfoMessageManager.WelcomeMessages[RoleType.Scp079] = plugin.ReadTranslation("scp079_start_message");
        }

        public override string Name => nameof(SCP079Handler);
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.RoundStarted += this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Scp079.InteractingDoor += this.Handle<Exiled.Events.EventArgs.InteractingDoorEventArgs>((ev) => Scp079_InteractingDoor(ev));
        }
        public override void OnDisable()
        {
            Gamer.Mistaken.Systems.InfoMessage.InfoMessageManager.WelcomeMessages.Remove(RoleType.Scp079);

            Exiled.Events.Handlers.Server.RoundStarted -= this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Scp079.InteractingDoor -= this.Handle<Exiled.Events.EventArgs.InteractingDoorEventArgs>((ev) => Scp079_InteractingDoor(ev));
        }

        public static void GainXP(Player player, float ap)
        {
            ReferenceHub rh = player.ReferenceHub;
            rh.scp079PlayerScript.NetworkcurMana -= ap;
            var id = rh.scp079PlayerScript.NetworkcurLvl;
            if (id >= rh.scp079PlayerScript.levels.Length)
                id = (byte)(rh.scp079PlayerScript.levels.Length - 1);
            else if (id < 0)
                id = 0;
            float num4 = 1f / Mathf.Clamp(rh.scp079PlayerScript.levels[id].manaPerSecond / 1.5f, 1f, 5f);
            ap = Mathf.Round(ap * num4 * 10f) / 10f;
            rh.scp079PlayerScript.AddExperience(ap);
        }

        public static class Translations
        {
            public static string trans_success = "Zrobione";
            public static string trans_failed_lvl = "Musisz mieć minimum ${lvl} poziom";
            public static string trans_failed_ap = "Musisz mieć minimum ${ap} AP";
            public static string trans_failed_cooldown = "Musicz poczekać ${time} sekund ponieważ jest cooldown";
            public static string trans_failed_cooldown_blackout = "";
            public static string trans_failed_nonumber = "Podaj numer SCP. Max ${max}";
            public static string trans_failed_nonumber_blackout = "Licza sekund ile będzie trwał blackout";
            public static string trans_failed_wrongnumber = "Podaj <b>NUMER</b> SCP. Max ${max}";
            public static string trans_scan = "Skan zakończony:|Entrance Zone:{ez}|Heavy Containment Zone:{hcz}|Light Containment Zone:{lcz}|Pomieszczenie z głowicą:{nuke}|Cela przechowawcza SCP 049:{049}|Powierzchnia:{surface}|Wymiar łzowy:{pocket}";
            public static string trans_info_pbc = "<color=red>SCP 079 ma dodatkowe funkcje na tym serwerze. Kliknij <color=blue>~</color> aby zobaczyć jakie</color>";
            public static string trans_info_console = "SCP 079 na tym serwerze może używać dwóch komend: .fakemtf oraz .fakescp abu użyć której kolwiek z komend potrzebujesz minimum ${lvl} level oraz kosztuje to ${ap} AP. Komendy należy wpisać w tej konsoli";

            public static void Ini(PluginHandler plugin)
            {
                Register(plugin);
                Set(plugin);
            }

            private static void Register(PluginHandler plugin)
            {
                plugin.RegisterTranslation("b079_success", "Done");
                plugin.RegisterTranslation("b079_failed_lvl", "You must have at least ${lvl} level");
                plugin.RegisterTranslation("b079_failed_ap", "You must have at least ${ap} AP");
                plugin.RegisterTranslation("b079_failed_cooldown", "You have to wait ${time} seconds because there's cooldown");
                plugin.RegisterTranslation("b079_failed_cooldown_blackout", "You have to wait ${time} seconds because there's cooldown, ${leftS} left");
                plugin.RegisterTranslation("b079_failed_nonumber_blackout", " Input number of seconds blackout will last");
                plugin.RegisterTranslation("b079_failed_nonumber", " Input SCP number. Max ${max}");
                plugin.RegisterTranslation("b079_failed_wrongnumber", "Input a SCP <b>NUMBER</b. Max ${max}");
                plugin.RegisterTranslation("b079_scan", "Scan completed:|Entrance Zone:{ez}|Heavy Containment Zone:{hcz}|Light Containment Zone:{lcz}|Alpha Warhead silo:{nuke}|Containment chamber of SCP 049:{049}|Surface:{surface}|Pocket:{pocket}");
                //plugin.RegisterTranslation("b079_info_pbc", "<color=red>SCP 079 has adition abilities on this server. Press <color=blue>~</color> to see them</color>");
                //plugin.RegisterTranslation("b079_info_console", "SCP 079 on this server can use additional commands: .fakemtf, .fakescp, .fullscan, .scan, .blackout to use some of them you have to have minimum ${lvl} level and for some level 2 minimum and it uses ${ap} AP. Command should be typed in this console");

                plugin.RegisterTranslation("scp079_start_message", "<color=red><b><size=500%>UWAGA</size></b></color><br><br><size=90%>Rozgrywka jako <color=red>SCP 079</color> na tym serwerze jest lekko zmodyfikowana, <color=red>SCP 079</color> posiada dodatkowe możliwość:<size=75%><br><b><color=yellow>.scan</color></b> - Pokazuje w jakiej strefie znajdują się gracze(nie licząc martwych)<br>- wymaga: <color=yellow>2</color> poziomu oraz <color=yellow>100</color> AP<br><b><color=yellow>.fullscan</color></b> - Działa jak <color=yellow>.scan</color> ale wynik podaje dodatkowo jako wiadomość CASSIE<br>- wymaga: <color=yellow>3</color> poziomu oraz <color=yellow>100</color> AP<br><b><color=yellow>.fakemtf</color></b> - Wysyła fałszywą wiadomość o przyjeździe <color=blue>MFO</color><br>- wymaga: <color=yellow>3</color> poziomu oraz <color=yellow>100</color> AP<br><b><color=yellow>.fakescp [numer SCP]</color></b> - Wysyła fałszywą wiadomość o śmierci podanego <color=red>SCP</color>, przyczyna jest losowa<br>- wymaga: <color=yellow>3</color> poziomu oraz <color=yellow>100</color> AP<br><b><color=yellow>.blackout [długość w sekundach]</color></b> - Gasi światła w placówce<br>- wymaga: <color=yellow>2</color> poziomu oraz <color=yellow>ilość sekund razy 10</color> AP</size></size>");
            }
            private static void Set(PluginHandler plugin)
            {
                trans_success = plugin.ReadTranslation("b079_success");
                trans_failed_lvl = plugin.ReadTranslation("b079_failed_lvl");
                trans_failed_ap = plugin.ReadTranslation("b079_failed_ap");
                trans_failed_cooldown = plugin.ReadTranslation("b079_failed_cooldown");
                trans_failed_cooldown_blackout = plugin.ReadTranslation("b079_failed_cooldown_blackout");
                trans_failed_nonumber = plugin.ReadTranslation("b079_failed_nonumber");
                trans_failed_nonumber_blackout = plugin.ReadTranslation("b079_failed_nonumber_blackout");
                trans_failed_wrongnumber = plugin.ReadTranslation("b079_failed_wrongnumber");
                trans_scan = plugin.ReadTranslation("b079_scan");
                //trans_info_pbc = plugin.ReadTranslation("b079_info_pbc");
                //trans_info_console = plugin.ReadTranslation("b079_info_console");
            }
        }


        private void Scp079_InteractingDoor(Exiled.Events.EventArgs.InteractingDoorEventArgs ev)
        {
            if (ev.Door.NetworkTargetState && ev.Door.NetworkActiveLocks != 0 && Gamer.Mistaken.BetterRP.RoundModifiersManager.Instance.ActiveEvents != BetterRP.RoundModifiersManager.RandomEvents.NONE)
                ev.IsAllowed = false;
        }

        private void Server_RoundStarted()
        {
            MEC.Timing.RunCoroutine(UpdateGeneratorsTimer());
        }

        private IEnumerator<float> UpdateGeneratorsTimer()
        {
            yield return MEC.Timing.WaitForSeconds(30);
            int rid = RoundPlus.RoundId;
            while (Round.IsStarted && rid == RoundPlus.RoundId)
            {
                string msg = "";
                if (Respawning.RespawnManager.Singleton.NextKnownTeam != Respawning.SpawnableTeamType.None)
                {
                    var seconds = Mathf.RoundToInt(Respawning.RespawnManager.Singleton._timeForNextSequence - (float)Respawning.RespawnManager.Singleton._stopwatch.Elapsed.TotalSeconds);
                    if (Respawning.RespawnManager.Singleton.NextKnownTeam == Respawning.SpawnableTeamType.NineTailedFox)
                        msg = $"<color=#00008f><b>Helicopter is landing</b></color> in {seconds:00}s";
                    else
                        msg = $"<color=#008f00><b>Car is arriving</b></color> in {seconds:00}s";
                }
                else
                {
                    Generator079 nearestGenerator = null;
                    int generators = 0;
                    foreach (var generator in Generator079.Generators)
                    {
                        if (generator.NetworkisTabletConnected)
                        {
                            generators++;
                            if ((nearestGenerator?.NetworkremainingPowerup ?? float.MaxValue) > generator.NetworkremainingPowerup)
                                nearestGenerator = generator;
                        }
                    }

                    if (nearestGenerator != null)
                    {
                        var seconds = nearestGenerator.remainingPowerup % 60;
                        msg = $"<color=yellow>{generators}</color> generator{(generators > 1 ? "s are" : " is")} being activated<br>Time left: <color=yellow>{((nearestGenerator.remainingPowerup - seconds) / 60):00}</color>m <color=yellow>{seconds:00}</color>s";
                    }
                    else if (Exiled.Events.Handlers.CustomEvents.SCP079.IsBeingRecontained)
                    {
                        if (Exiled.Events.Handlers.CustomEvents.SCP079.IsRecontainmentPaused)
                            msg = $"Recontainment <color=yellow>paused</color>";
                        else
                            msg = $"You have <color=yellow>{Exiled.Events.Handlers.CustomEvents.SCP079.TimeToRecontainment}</color>s untill recontainment";
                    }
                }
                foreach (var player in RealPlayers.List.Where(p => p.Role != RoleType.Scp079))
                {
                    player.SetGUI("scp079", Mistaken.Base.GUI.PseudoGUIHandler.Position.MIDDLE, null);
                    player.SetGUI("scp079_message", Mistaken.Base.GUI.PseudoGUIHandler.Position.MIDDLE, null);
                }
                foreach (var player in RealPlayers.Get(RoleType.Scp079))
                {
                    string fakeSCP = $"<color=yellow>READY</color>";
                    string fakeMTF = $"<color=yellow>READY</color>";
                    string fakeCI = $"<color=yellow>READY</color>";
                    string fakeTesla = $"<color=yellow>READY</color>";
                    string scan = $"<color=yellow>READY</color>";
                    string fullScan = $"<color=yellow>READY</color>";
                    string blackout = $"<color=yellow>READY</color>";
                    string warheadStop = $"<color=yellow>READY</color>";
                    string cassie = $"<color=yellow>READY</color>";
                    if (Systems.Patches.SCP079RecontainInfoPatch.Recontaining)
                    {
                        fakeSCP = $"<color=red>ERROR</color>";
                        fakeMTF = $"<color=red>ERROR</color>";
                        fakeCI = $"<color=red>ERROR</color>";
                        fakeTesla = $"<color=red>ERROR</color>";
                        fullScan = $"<color=red>ERROR</color>";
                    }

                    if (FakeSCPCommand.ReqLvl > player.Level + 1)
                        fakeSCP = $"<color=red>Require <color=yellow>{FakeSCPCommand.ReqLvl}</color> lvl</color>";
                    else if (!FakeSCPCommand.IsReady)
                        fakeSCP = $"<color=red>Require <color=yellow>{Math.Round(new TimeSpan(FakeSCPCommand.TimeLeft).TotalSeconds)}</color>s</color>";
                    else if (FakeSCPCommand.Cost > player.Energy)
                        fakeSCP = $"<color=red>Require <color=yellow>{FakeSCPCommand.Cost}</color> AP</color>";

                    if (FakeMTFCommand.ReqLvl > player.Level + 1)
                        fakeMTF = $"<color=red>Require <color=yellow>{FakeMTFCommand.ReqLvl}</color> lvl</color>";
                    else if (!FakeMTFCommand.IsReady)
                        fakeMTF = $"<color=red>Require <color=yellow>{Math.Round(new TimeSpan(FakeMTFCommand.TimeLeft).TotalSeconds)}</color>s</color>";
                    else if (FakeMTFCommand.Cost > player.Energy)
                        fakeMTF = $"<color=red>Require <color=yellow>{FakeMTFCommand.Cost}</color> AP</color>";

                    if (FakeCICommand.ReqLvl > player.Level + 1)
                        fakeCI = $"<color=red>Require <color=yellow>{FakeCICommand.ReqLvl}</color> lvl</color>";
                    else if (!FakeCICommand.IsReady)
                        fakeCI = $"<color=red>Require <color=yellow>{Math.Round(new TimeSpan(FakeCICommand.TimeLeft).TotalSeconds)}</color>s</color>";
                    else if (FakeCICommand.Cost > player.Energy)
                        fakeCI = $"<color=red>Require <color=yellow>{FakeCICommand.Cost}</color> AP</color>";

                    if (FakeTeslaCommand.ReqLvl > player.Level + 1)
                        fakeTesla = $"<color=red>Require <color=yellow>{FakeTeslaCommand.ReqLvl}</color> lvl</color>";
                    else if (!FakeTeslaCommand.IsReady)
                        fakeTesla = $"<color=red>Require <color=yellow>{Math.Round(new TimeSpan(FakeTeslaCommand.TimeLeft).TotalSeconds)}</color>s</color>";
                    else if (FakeTeslaCommand.Cost > player.Energy)
                        fakeTesla = $"<color=red>Require <color=yellow>{FakeTeslaCommand.Cost}</color> AP</color>";

                    if (ScanCommand.ReqLvl > player.Level + 1)
                        scan = $"<color=red>Require <color=yellow>{ScanCommand.ReqLvl}</color> lvl</color>";
                    else if (!ScanCommand.IsReady)
                        scan = $"<color=red>Require <color=yellow>{Math.Round(new TimeSpan(ScanCommand.TimeLeft).TotalSeconds)}</color>s</color>";
                    else if (ScanCommand.Cost > player.Energy)
                        scan = $"<color=red>Require <color=yellow>{ScanCommand.Cost}</color> AP</color>";

                    if (FullScanCommand.ReqLvl > player.Level + 1)
                        fullScan = $"<color=red>Require <color=yellow>{FullScanCommand.ReqLvl}</color> lvl</color>";
                    else if (!FullScanCommand.IsReady)
                        fullScan = $"<color=red>Require <color=yellow>{Math.Round(new TimeSpan(FullScanCommand.TimeLeft).TotalSeconds)}</color>s</color>";
                    else if (FullScanCommand.Cost > player.Energy)
                        fullScan = $"<color=red>Require <color=yellow>{FullScanCommand.Cost}</color> AP</color>";

                    if (BlackoutCommand.ReqLvl > player.Level + 1)
                        blackout = $"<color=red>Require <color=yellow>{BlackoutCommand.ReqLvl}</color> lvl</color>";
                    else if (!BlackoutCommand.IsReady)
                        blackout = $"<color=red>Require <color=yellow>{Math.Round(new TimeSpan(BlackoutCommand.TimeLeft).TotalSeconds)}</color>s</color>";
                    else
                        blackout = $"Max <color=yellow>{Math.Floor(player.Energy / BlackoutCommand.Cost)}</color> seconds of blackout";

                    if (StopWarheadCommand.ReqLvl > player.Level + 1)
                        warheadStop = $"<color=red>Require <color=yellow>{StopWarheadCommand.ReqLvl}</color> lvl</color>";
                    else if (!StopWarheadCommand.IsReady)
                        warheadStop = $"<color=red>Require <color=yellow>{Math.Round(new TimeSpan(StopWarheadCommand.TimeLeft).TotalSeconds)}</color>s</color>";
                    else if (StopWarheadCommand.Cost > player.Energy)
                        warheadStop = $"<color=red>Require <color=yellow>{StopWarheadCommand.Cost}</color> AP</color>";
                    else if (!Warhead.IsInProgress)
                        warheadStop = "<color=red>Warhead is not detonating</color>";

                    if (CassieCommand.ReqLvl > player.Level + 1)
                        cassie = $"<color=red>Require <color=yellow>{CassieCommand.ReqLvl}</color> lvl</color>";
                    else if (!CassieCommand.IsReady)
                        cassie = $"<color=red>Require <color=yellow>{Math.Round(new TimeSpan(CassieCommand.TimeLeft).TotalSeconds)}</color>s</color>";
                    else if (CassieCommand.Cost > player.Energy)
                        cassie = $"<color=red>Require <color=yellow>{CassieCommand.Cost}</color> AP</color>";

                    string sumMessage = $@"
<size=50%>
<align=left>Fake SCP</align><line-height=1px><br></line-height><align=right>{fakeSCP}</align>
<align=left>Fake MTF</align><line-height=1px><br></line-height><align=right>{fakeMTF}</align>
<align=left>Fake CI</align><line-height=1px><br></line-height><align=right>{fakeCI}</align>
<align=left>Fake Tesla</align><line-height=1px><br></line-height><align=right>{fakeTesla}</align>
<align=left>Scan</align><line-height=1px><br></line-height><align=right>{scan}</align>
<align=left>FullScan</align><line-height=1px><br></line-height><align=right>{fullScan}</align>
<align=left>Blackout</align><line-height=1px><br></line-height><align=right>{blackout}</align>
<align=left>Warhead Stop</align><line-height=1px><br></line-height><align=right>{warheadStop}</align>
<align=left>Cassie</align><line-height=1px><br></line-height><align=right>{cassie}</align>
</size>";
                    player.SetGUI("scp079", Mistaken.Base.GUI.PseudoGUIHandler.Position.MIDDLE, sumMessage);
                    player.SetGUI("scp079_message", Mistaken.Base.GUI.PseudoGUIHandler.Position.BOTTOM, msg);
                }
                yield return MEC.Timing.WaitForSeconds(1);
            }
        }
    }
}
