using Exiled.API.Enums;
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
using Exiled.API.Extensions;
using Gamer.Mistaken.Utilities.APILib;
using MistakenSocket.Client.SL;
using MistakenSocket.Shared.ClientToCentral;
using MistakenSocket.Shared;
using Gamer.Diagnostics;
using MistakenSocket.Shared.API;

namespace Gamer.Mistaken.Systems.GUI
{
    internal class InformerHandler : Module
    {
        private string WelcomeMessage;

        public InformerHandler(PluginHandler p) : base(p)
        {
            plugin.RegisterTranslation("Info_WarheadDenied", "You <b>can't</b> <color=yellow>activate</color> Alpha Warhead as <color=red>SCP</color>");
            plugin.RegisterTranslation("Info_Intercom", "You are <color=yellow>using</color> intercom");
            plugin.RegisterTranslation("Info_268", "You have <color=yellow>{0}</color>s <color=red>SCP 268</color> left");

            plugin.RegisterTranslation("WelcomeMessage", "Hej <color=#6B9ADF>{nickname}</color>!|_nAby grać na serwerze, zapoznaj sie z regulaminem, oraz listą pluginów na <color=#6B9ADF>discord.mistaken.pl</color>");
        }

        public override string Name => "Informer";
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.WaitingForPlayers += this.Handle(() => Server_WaitingForPlayers(), "WaitingForPlayers");
            Exiled.Events.Handlers.Server.RoundStarted += this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.CustomEvents.OnFirstTimeJoined += this.Handle<Exiled.Events.EventArgs.FirstTimeJoinedEventArgs>((ev) => CustomEvents_OnFirstTimeJoined(ev));
            Exiled.Events.Handlers.Player.IntercomSpeaking += this.Handle<Exiled.Events.EventArgs.IntercomSpeakingEventArgs>((ev) => Player_IntercomSpeaking(ev));
            Exiled.Events.Handlers.Warhead.Starting += this.Handle<Exiled.Events.EventArgs.StartingEventArgs>((ev) => Warhead_Starting(ev));
            Exiled.Events.Handlers.Player.Escaping += this.Handle<Exiled.Events.EventArgs.EscapingEventArgs>((ev) => Player_Escaping(ev));

            WelcomeMessage = plugin.ReadTranslation("WelcomeMessage");
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.WaitingForPlayers -= this.Handle(() => Server_WaitingForPlayers(), "WaitingForPlayers");
            Exiled.Events.Handlers.Server.RoundStarted -= this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.CustomEvents.OnFirstTimeJoined -= this.Handle<Exiled.Events.EventArgs.FirstTimeJoinedEventArgs>((ev) => CustomEvents_OnFirstTimeJoined(ev));
            Exiled.Events.Handlers.Player.IntercomSpeaking -= this.Handle<Exiled.Events.EventArgs.IntercomSpeakingEventArgs>((ev) => Player_IntercomSpeaking(ev));
            Exiled.Events.Handlers.Warhead.Starting -= this.Handle<Exiled.Events.EventArgs.StartingEventArgs>((ev) => Warhead_Starting(ev));
            Exiled.Events.Handlers.Player.Escaping -= this.Handle<Exiled.Events.EventArgs.EscapingEventArgs>((ev) => Player_Escaping(ev));
        }

        private void Player_Escaping(Exiled.Events.EventArgs.EscapingEventArgs ev)
        {
            if (PluginHandler.Config.IsRP())
                Timing.RunCoroutine(EscapeMessage(ev.Player, ev.NewRole == RoleType.ChaosInsurgency, ev.NewRole == RoleType.NtfScientist));
        }

        private readonly static string[] CIMessage = new string[] 
        {
            "Przeprowadzacie atak na placówkę Fundacji. Nie zmieściłeś się jednak w pierwszym aucie wysłanym tu, więc przyjechałeś później.",
            "Dowództwo Delta zdecydowało o przysłaniu dodatkowych jednostek do tej placówki w celu przeprowadzenia ataku.",
            "Nie otrzymałeś informacji o planowanym ataku i przyjechałeś bez swoich towarzyszy.",
            "Nie planowano twojego przyjazdu do placówki, jednak Dowództwo Delta zdecydowało o spontanicznej ofensywie na placówkę Fundacji.",
            "Otrzymaliście informacje o wyłomie w placówce Fundacji. Postanawiacie to wykorzystać i zdobyć informacje oraz obiekty SCP.",
        };
        private readonly static string[] MTFMessage = new string[] 
        {
            "Dyrektor [REDACTED] otrzymał niepokojące informacje o sytuacji w placówce. Zostałeś więc przysłany jako wsparcie dla jednostki $team.",
            "Zaspałeś i przegapiłeś wezwanie na operację. Przywieziono cię drugim helikopterem lecącym do placówki.",
            "Sytuacja w placówce jest krytyczna, dowództwo otrzymało prośbę o natychmiastowe wsparcie. Jednostka MFO Epsilon-11 nie jest jednak gotowa do przylotu z powodów technicznych. Zmobilizowano więc lokalne rezerwy Fundacji, w tym Ciebie.",
            "W bazie otrzymaliście wezwania do dwóch placówek jednocześnie. Podzielono was na grupy i wysłano do akcji. Ty i twój oddział opanowaliście sytuację na swoim terenie, więc przywieziono was tu jako wsparcie oddziału $team.",
            "Wyłom zabezpieczeń jest gorszy niż się spodziewano, podstawowe siły obecne w placówce nie dają sobie rady. Jesteś tu, by im pomóc.",
            "Wracasz do placówki po 2-dniowym urlopie, gdzie dowiadujesz się o trwającym wyłomie. Musisz teraz pomóc swoim kolegom i Fundacji opanować sytuację.",
            "Podczas przygotowań do transferu do [REDACTED] otrzymałeś raport o sytuacji w placówce. Transfer jest przełożony przez dowództwo i przylatujesz, aby wesprzeć obecne na miejscu jednostki.",
        };
        private IEnumerator<float> EscapeMessage(Player player, bool ci, bool sci)
        {
            yield return Timing.WaitForSeconds(2);
            string message = ci ? CIMessage[UnityEngine.Random.Range(0, CIMessage.Length)] : MTFMessage[UnityEngine.Random.Range(0, MTFMessage.Length)].Replace("$team", "");
            if (sci && UnityEngine.Random.Range(0, 5) == 0) 
                message = "Ochrona placówki zgłosiła, że potrzebna jest osoba z wyższymi uprawnieniami do wykonania pewnych zadań. Przysłano więc Ciebie.";
            player.SendConsoleMessage(message, "grey");
            GUI.PseudoGUIHandler.Set(player, "escape", PseudoGUIHandler.Position.MIDDLE, message, 15);
        }

        private void Warhead_Starting(Exiled.Events.EventArgs.StartingEventArgs ev)
        {
            if(ev.Player.Team == Team.SCP)
            {
                ev.IsAllowed = false;
                GUI.PseudoGUIHandler.Set(ev.Player, "warhead", PseudoGUIHandler.Position.MIDDLE, plugin.ReadTranslation("Info_WarheadDenied"), 10);
            }
        }
        private void Player_IntercomSpeaking(Exiled.Events.EventArgs.IntercomSpeakingEventArgs ev)
        {
            if (ev.Player == null)
                return;
            if (!ev.IsAllowed)
                return;
            Timing.RunCoroutine(InformSpeaker(ev.Player));
        }

        IEnumerator<float> InformSpeaker(Player player)
        {
            string message = plugin.ReadTranslation("Info_Intercom");
            GUI.PseudoGUIHandler.Set(player, "intercom", PseudoGUIHandler.Position.BOTTOM, message);
            yield return Timing.WaitForSeconds(1);
            while(Intercom.host?.speaker != null)
                yield return Timing.WaitForSeconds(1);
            GUI.PseudoGUIHandler.Set(player, "intercom", PseudoGUIHandler.Position.BOTTOM, null);
        }

        private void CustomEvents_OnFirstTimeJoined(Exiled.Events.EventArgs.FirstTimeJoinedEventArgs ev)
        {
            if (ev.Player == null)
            {
                Log.Warn("Joined player is null");
                return;
            }
            GUI.PseudoGUIHandler.Set(ev.Player, "welcome", PseudoGUIHandler.Position.BOTTOM, WelcomeMessage.Replace("{nickname}", ev.Player.Nickname), 20);
        }

        private void Server_WaitingForPlayers()
        {
            DonwloadWelcomeMessage();
        }

        private void Server_RoundStarted()
        {
            Timing.RunCoroutine(Update());
            Timing.RunCoroutine(Update268());
        }

        public int NextUpdate = 5;
        IEnumerator<float> Update()
        {
            yield return Timing.WaitForSeconds(1);
            int rid = RoundPlus.RoundId; 
            while (Round.IsStarted && rid == RoundPlus.RoundId)
            {
                yield return Timing.WaitForSeconds(5);
                var start = DateTime.Now;
                if (Round.ElapsedTime.TotalMinutes >= NextUpdate)
                {
                    MapPlus.Broadcast("TIMER", 10, $"RoundTime: {NextUpdate} minutes", Broadcast.BroadcastFlags.AdminChat);
                    NextUpdate += 5;
                }
                foreach (var player in RealPlayers.List)
                {
                    string customInfoMessage = "";
                    string infoMessage = "";
                    if (player.IsGodModeEnabled)
                    {
                        infoMessage += "<br>GodMode: <color=yellow>Active</color>";
                        customInfoMessage += "GOD MODE |";
                    }
                    if (player.IsBypassModeEnabled)
                    {
                        infoMessage += "<br>Bypass: <color=yellow>Active</color>";
                        customInfoMessage += " BYPASS |";
                    }
                    if (player.NoClipEnabled)
                    {
                        customInfoMessage += " NOCLIP |";
                        infoMessage += "<br>Noclip: <color=yellow>Active</color>";
                    }
                    if (Systems.End.VanishHandler.Vanished.TryGetValue(player.Id, out int vanishLevel))
                    {
                        customInfoMessage += $" Vanish ({vanishLevel})";
                        infoMessage += $"<br>Vanish: <color=yellow>Active ({vanishLevel})</color>";
                    }
                    GUI.PseudoGUIHandler.Set(player, "admin", PseudoGUIHandler.Position.BOTTOM, $"<size=50%>{infoMessage}</size>");
                    
                    CustomInfoHandler.Set(player, "FLAGS", customInfoMessage == "" ? null : $"<color=red>{customInfoMessage.Trim('|').Trim()}</color>", false);
                }
                Diagnostics.MasterHandler.LogTime("InformerHandler", "Update", start, DateTime.Now);
            }
        }
        IEnumerator<float> Update268()
        {
            yield return Timing.WaitForSeconds(1);
            int rid = RoundPlus.RoundId; 
            while (Round.IsStarted && rid == RoundPlus.RoundId)
            {
                var start = DateTime.Now;
                foreach (var player in RealPlayers.List)
                {
                    var effect = player.ReferenceHub.playerEffectsController.GetEffect<CustomPlayerEffects.Scp268>();
                    if (effect.Intensity > 0)
                        GUI.PseudoGUIHandler.Set(player, "268", PseudoGUIHandler.Position.BOTTOM, plugin.ReadTranslation("Info_268", Mathf.RoundToInt(CustomPlayerEffects.Scp268.maxTime - effect.curTime)));
                    else
                        GUI.PseudoGUIHandler.Set(player, "268", PseudoGUIHandler.Position.BOTTOM, null);
                }
                Diagnostics.MasterHandler.LogTime("InformerHandler", "Update268", start, DateTime.Now);
                yield return Timing.WaitForSeconds(1);
            }
        }

        private void DonwloadWelcomeMessage()
        {
            if (PluginHandler.IsSSLSleepMode)
            {
                using (var client = new WebClient())
                {
                    if (!Gamer.Mistaken.Utilities.APILib.API.GetUrl(APIType.AUTO_MESSAGE, out string url, Server.Port.ToString(), ServerConsole.Ip)) return;
                    client.DownloadDataCompleted += OnDataDownloadCompleted;
                    try
                    {
                        client.DownloadDataAsync(new Uri(url));
                    }
                    catch (WebException ex)
                    {
                        Log.Error("Failed to get Auto Message");
                        Log.Error(ex.Status);
                        Log.Error(ex.Message);
                    }
                }
            }
            else
            {
                SSL.Client.Send(MessageType.CMD_REQUEST_DATA, new RequestData
                {
                    Type = MistakenSocket.Shared.API.DataType.SL_AUTO_MESSAGES,
                    argument = null
                }).GetResponseDataCallback((data) =>
                {
                    if (data.Type != MistakenSocket.Shared.API.ResponseType.OK)
                        return;
                    var info = data.Payload.Deserialize<(string Type, string Message, int ServerId)[]>(0, 0, out _, false);
                    foreach (var item in info)
                    {
                        if (item.Type == "welcome" && item.ServerId == Server.Port - 7776)
                            WelcomeMessage = item.Message.Replace("|_n", "<br>").Replace("\n", "<br>").Replace("\\n", "<br>");
                    }
                });
            }
        }
        private void OnDataDownloadCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            if(e.Error != null)
            {
                Log.Error("Failed to get Auto Message");
                if (e.Error is WebException ex)
                    Log.Error(ex.Status);
                Log.Error(e.Error.Message);
                return;
            }
            var msg = System.Text.Encoding.UTF8.GetString(e.Result);
            WelcomeMessage = msg.Replace("|_n", "<br>").Replace("\n", "<br>").Replace("\\n", "<br>");
        }
    }
}
