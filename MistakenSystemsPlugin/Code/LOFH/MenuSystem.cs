using CommandSystem;
using Exiled.API.Features;
using Gamer.Mistaken.Systems.Misc;
using Gamer.Utilities;
using MistakenSocket.Client.SL;
using MistakenSocket.Shared;
using MistakenSocket.Shared.AIRS;
using MistakenSocket.Shared.API;
using MistakenSocket.Shared.ClientToCentral;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Gamer.Mistaken.LOFH
{
    public static class MenuSystem
    {
        public static string ProccessPress(Player player, int id, int duration, string reason)
        {
            duration /= 60;
            switch(CurrentMenus[player.Id])
            {
                case 10:
                    return Execute(player, $"ban2 {id} {duration} {reason}");
                case 11:
                    if (duration == 26280000)
                        duration = -1;
                    if (InMuteMode.Contains(player.Id))
                    {
                        if (duration != 0)
                            return Execute(player, $"mute2 {id} {duration} {reason}");
                        else
                            return Execute(player, $"unmute2 {id}");
                    }
                    else
                    {
                        if (duration != 0)
                            return Execute(player, $"imute2 {id} {duration} {reason}");
                        else
                            return Execute(player, $"iunmute2 {id}");
                    }
                default:
                    return "This button does nothing in this menu";
            }
        }
        /// <summary>
        /// ALWAYS RETURN AFTER ENABLING IT, DO NOT LET IT DO LOOP ProccessPress -> ForceRefreshPlayerList -> ProccessPress -> ..., DON'T MAKE SAME MISTAKE TWICE
        /// </summary>
        public static readonly Dictionary<int, int> AutoRepeat = new Dictionary<int, int>();
        public static string ProccessPress(Player player, int id, string rawId, int mode = 0, bool autoRepeat = false)
        {
            if (!AutoRepeat.ContainsKey(player.Id))
                AutoRepeat.Add(player.Id, 0);
            if(!autoRepeat)
                AutoRepeat[player.Id] = 0;
            string tor = "Changed Menu";
            if (id < 8000)
            {
                switch (CurrentMenus[player.Id])
                {
                    case 5:
                        return Execute(player, $"ga {id}");
                    case 6:
                        return Execute(player, $"la {id}");
                    case 7:
                        return Execute(player, $"ptkd {id}");
                    case 8:
                        if (!PlayerLogSelectedRound.ContainsKey(player.Id))
                        {
                            PlayerLogSelectedRound.Add(player.Id, id);
                            ForceRefreshPlayerList(player);
                            return "Selected Round";
                        }
                        break;
                    case 13:
                        if(!ReportSelectedReport.ContainsKey(player.Id) && id < 0)
                        {
                            ReportSelectedReport.Add(player.Id, -id);
                            return $"Selected Report: {-id}";
                        }
                        break;
                    case 14:
                        return Execute(player, $"talk {rawId}");
                }
            }         

            switch (id)
            {
                #region MENUS
                case 8000: //BACK/MENU
                    var menu = GetMenu(CurrentMenus[player.Id]);
                    CurrentMenus[player.Id] = menu.Id == 0 ? 1 : menu.Parent;
                    break;
                case 8100:
                    CurrentMenus[player.Id] = 2; //Commands
                    break;
                case 8150:
                    CurrentMenus[player.Id] = 3; //Overheat
                    break;
                case 8200:
                    CurrentMenus[player.Id] = 4; //NPCs
                    break;
                case 8951:
                    CurrentMenus[player.Id] = 5; //GA
                    break;
                case 8952:
                    CurrentMenus[player.Id] = 6; //LA
                    break;
                case 8953:
                    CurrentMenus[player.Id] = 7; //PTKD
                    break;
                case 8954:
                    CurrentMenus[player.Id] = 8; //PLOG
                    if (PlayerLogSelectedRound.ContainsKey(player.Id))
                    {
                        PlayerLogSelectedRound.Remove(player.Id);
                        tor = "Deselected Round";
                        break;
                    }
                    break;
                case 8955:
                    CurrentMenus[player.Id] = 10; //BAN
                    break;
                case 8956:
                    CurrentMenus[player.Id] = 11; //MUTE
                    break;
                case 8957:
                    if (InMuteMode.Contains(player.Id))
                        InMuteMode.Remove(player.Id);
                    else
                        InMuteMode.Add(player.Id);
                    tor = "Toggled Mode";
                    break;
                case 8250:
                    CurrentMenus[player.Id] = 9; //Warhead
                    break;
                case 8300:
                    RefreshPlayers();
                    CurrentMenus[player.Id] = 12; //PlayerList
                    break;
                case 8350:
                    CurrentMenus[player.Id] = 13; //Reports
                    if (ReportSelectedReport.ContainsKey(player.Id))
                    {
                        ReportSelectedReport.Remove(player.Id);
                        tor = "Deselected Report";
                        break;
                    }
                    RefreshReports();
                    break;
                case 8958:
                    tor = "Refreshing";
                    RefreshReports();
                    break;
                case 8960:
                    CurrentMenus[player.Id] = 14; //Talk
                    break;
                #endregion
                #region MENU
                case 8051:
                    GameCore.Console.singleton.TypeCommand("restartnextround");
                    CurrentMenus[player.Id] = 0;
                    tor = "Requested server restart";
                    break;
                case 8052:
                    AutoRepeat[player.Id] = id;
                    return GenerateServerInfo();
                #endregion
                #region Commands
                case 8101:
                    CurrentMenus[player.Id] = 0;
                    return Execute(player, $"hide");
                case 8102:
                    return Execute(player, $"vanish false");
                case 8103:
                    return Execute(player, $"vanish true -l1");
                case 8104:
                    return Execute(player, $"vanish true -l2");
                case 8105:
                    return Execute(player, $"vanish true -l3");
                case 8106:
                    return Execute(player, $"tut");
                case 8107:
                    return Execute(player, $"tut -v");
                case 8108:
                    return Execute(player, $"tut -n");
                case 8109:
                    return Execute(player, $"tut -v -n");
                #endregion
                #region Overheat
                case 8151:
                    CurrentMenus[player.Id] = 0;
                    return Execute(player, $"overheat 0");
                case 8152:
                    CurrentMenus[player.Id] = 0;
                    return Execute(player, $"overheat 1");
                case 8153:
                    CurrentMenus[player.Id] = 0;
                    return Execute(player, $"overheat 2");
                case 8154:
                    CurrentMenus[player.Id] = 0;
                    return Execute(player, $"overheat 3");
                case 8155:
                    CurrentMenus[player.Id] = 0;
                    return Execute(player, $"overheat 4");
                case 8156:
                    CurrentMenus[player.Id] = 0;
                    return Execute(player, $"overheat 5");
                case 8157:
                    CurrentMenus[player.Id] = 0;
                    return Execute(player, $"overheat 6");
                case 8158:
                    CurrentMenus[player.Id] = 0;
                    return Execute(player, $"overheat 7");
                case 8159:
                    return Execute(player, $"overheat 16");
                #endregion
                #region Warhead
                case 8251:
                    AutoRepeat[player.Id] = id;
                    return GenerateWarheadInfo();
                case 8252:
                    return Execute(player, $"warhead {(!Warhead.IsInProgress ? "start" : "stop")}");
                case 8253:
                    return Execute(player, $"warhead {(!Warhead.LeverStatus ? "on" : "off")}");
                case 8254:
                    return Execute(player, $"warhead {(!Warhead.IsKeycardActivated ? "open" : "close")}");
                case 8255:
                    return Execute(player, $"warhead lockstart {(!BetterWarheadHandler.Warhead.StartLock ? "true" : "false")}");
                case 8256:
                    return Execute(player, $"warhead lockstop {(!BetterWarheadHandler.Warhead.StopLock ? "true" : "false")}");
                case 8257:
                    return Execute(player, $"warhead buttonlock {(!BetterWarheadHandler.Warhead.ButtonLock ? "true" : "false")}");
                case 8258:
                    return Execute(player, $"warhead leverlock {(!BetterWarheadHandler.Warhead.LeverLock ? "true" : "false")}");
                #endregion
                #region PlayerList
                case 8301:
                    if (!PlayerListSelectedServer.ContainsKey(player.Id))
                        PlayerListSelectedServer.Add(player.Id, 2);
                    PlayerListSelectedServer[player.Id] = 1;
                    RefreshPlayers();
                    tor = "Toggled Server";
                    break;
                case 8302:
                    if (!PlayerListSelectedServer.ContainsKey(player.Id))
                        PlayerListSelectedServer.Add(player.Id, 2);
                    PlayerListSelectedServer[player.Id] = 2;
                    RefreshPlayers();
                    tor = "Toggled Server";
                    break;
                case 8303:
                    if (!PlayerListSelectedServer.ContainsKey(player.Id))
                        PlayerListSelectedServer.Add(player.Id, 2);
                    PlayerListSelectedServer[player.Id] = 3;
                    RefreshPlayers();
                    tor = "Toggled Server";
                    break;
                case 8304:
                    if (!PlayerListSelectedServer.ContainsKey(player.Id))
                        PlayerListSelectedServer.Add(player.Id, 2);
                    PlayerListSelectedServer[player.Id] = 4;
                    RefreshPlayers();
                    tor = "Toggled Server";
                    break;
                #endregion
                #region Reports
                case 8351:
                    var report = Reports.First(r => r.Report.ReportId == ReportSelectedReport[player.Id]);
                    AutoRepeat[player.Id] = id;
                    var reported = report.Report.ReportedData == null ? report.Report.ReportedName : $"\n- Nickname: {report.Report.ReportedData.Nickname}\n- UserId: {report.Report.ReportedData.UserId}\n- Role: {report.Report.ReportedData.Role}";
                    return $"<color=white>Server: {report.Report.Type}\nReportId: {report.Report.ReportId}\nStatus: <color={AIRS.ReportUpdateEventHandler.GetColorByStatus(report.Status)}>{report.Status}</color>\nReport Time: {report.Timestamp}\nSuspect: {reported}\nReporter: {report.Report.ReporterName} | {report.Report.ReporterUserId}\nReason: <b>{report.Report.Reason}</b></color>";
                case 8352:
                    report = Reports.First(r => r.Report.ReportId == ReportSelectedReport[player.Id]);
                    SSL.Client.Send(MessageType.CMD_RESPOND_REPORT_STATUS, new ReportStatusUpdateData
                    {
                        Message = "",
                        ReportId = report.Report.ReportId,
                        Status = ReportStatusType.DONE
                    });
                    return $"Updating ReportStatus from {report.Status} to {ReportStatusType.DONE}";
                case 8353:
                    report = Reports.First(r => r.Report.ReportId == ReportSelectedReport[player.Id]);
                    SSL.Client.Send(MessageType.CMD_RESPOND_REPORT_STATUS, new ReportStatusUpdateData
                    {
                        Message = "",
                        ReportId = report.Report.ReportId,
                        Status = ReportStatusType.PROCCEDING
                    });
                    return $"Updating ReportStatus from {report.Status} to {ReportStatusType.PROCCEDING}";
                case 8354:
                    report = Reports.First(r => r.Report.ReportId == ReportSelectedReport[player.Id]);
                    SSL.Client.Send(MessageType.CMD_RESPOND_REPORT_STATUS, new ReportStatusUpdateData
                    {
                        Message = "",
                        ReportId = report.Report.ReportId,
                        Status = ReportStatusType.FAILED
                    });
                    return $"Updating ReportStatus from {report.Status} to {ReportStatusType.FAILED}";
                #endregion
                default:
                    tor = $"UNKNOWN MENU {id}";
                    break;
            }
            ForceRefreshPlayerList(player);
            return tor;
        }

        public static string Execute(Player player, string query)
        {
            string[] args = query.Split(' ');
            Base.LoggerHandler.SendCommand(args[0], string.Join(" ", args.Skip(1)), new Exiled.Events.EventArgs.SendingRemoteAdminCommandEventArgs(player.Sender, player, player.Nickname, args.ToList()), player.UserId);
            if (RemoteAdmin.CommandProcessor.RemoteAdminCommandHandler.TryGetCommand(args[0], out ICommand cmd))
            {
                if (!cmd.Execute(new ArraySegment<string>(args), player.Sender, out string response))
                    response = $"<color=red>{response}</color>";
                return response;
            }
            else
                return $"!! COMMAND NOT FOUND !!\n {query}";
        }

        private static bool AltColor = true;
        public static string GetColor(string text)
        {
            AltColor = !AltColor;
            string[] split = text.Split('\n');
            for (int i = 0; i < split.Length; i++)
            {
                split[i] = $"<color={(AltColor ? "#76DF0C" : "#4C9900")}>{split[i]}</color>";
            }
            return string.Join("\n", split);
        }

        public static readonly Dictionary<int, int> CurrentMenus = new Dictionary<int, int>();
        public static readonly Dictionary<int, int> PlayerLogSelectedRound = new Dictionary<int, int>();
        public static readonly HashSet<int> InMuteMode = new HashSet<int>();
        public static readonly Dictionary<int, int> PlayerListSelectedServer = new Dictionary<int, int>();
        public static readonly Dictionary<int, int> ReportSelectedReport = new Dictionary<int, int>();

        public static (ReportData Report, ReportStatusType Status, DateTime Timestamp)[] Reports = new (ReportData Report, ReportStatusType Status, DateTime Timestamp)[0];
        public static readonly Dictionary<int, string> PlayerLists = new Dictionary<int, string>();
        public static string GetMenu(Player player, out bool GeneratePlayerList)
        {
            if (AutoRepeat.TryGetValue(player.Id, out int autoRepeat) && autoRepeat != 0)
                player.Sender.RaReply($"SHORT-PLAYER:PLAYER#<size=100%>{ProccessPress(player, autoRepeat, autoRepeat.ToString(), 0, true)}</size>", true, false, "PlayerInfo");
            AltColor = true;
            if (!CurrentMenus.TryGetValue(player.Id, out int selected))
                CurrentMenus.Add(player.Id, 0);
            var menu = GetMenu(selected);
            if(menu == null)
            {
                GeneratePlayerList = false;
                return $"{Generate("BACK", 8000)}ERROR UNKNOWN CATEGORY\n";
            }
            string tor;
            switch (selected)
            {
                case 2:
                    tor = menu.Get(out GeneratePlayerList);
                    if (!Systems.End.VanishHandler.Vanished.TryGetValue(player.Id, out int vanishLevel))
                        vanishLevel = 0;
                    if(vanishLevel == 0)
                        tor = tor.Replace("$v0","green");
                    else if (vanishLevel == 1)
                        tor = tor.Replace("$v1", "green");
                    else if (vanishLevel == 2)
                        tor = tor.Replace("$v2", "green");
                    else if (vanishLevel == 3)
                        tor = tor.Replace("$v3", "green");
                    tor = tor.Replace("$v0", "red");
                    tor = tor.Replace("$v1", "red");
                    tor = tor.Replace("$v2", "red");
                    tor = tor.Replace("$v3", "red");
                    tor = tor.Replace("$hide", LOFH.Hidden.Contains(player.UserId) ? "green" : "red");
                    return tor;
                case 4:
                    tor = menu.Get(out GeneratePlayerList);
                    foreach (var npc in NPCS.Npc.List.ToArray())
                        tor += $"<color=red>[<color=red>NPC</color>]</color> ({npc.NPCPlayer.Id}) {npc.Name}\n";
                    return tor;
                case 8:
                    if (PlayerLogSelectedRound.TryGetValue(player.Id, out int roundId))
                    {
                        tor = menu.Get(out GeneratePlayerList);
                        if(!Systems.Logs.LogManager.PlayerLogs.ContainsKey(roundId))
                        {
                            tor = $"(0) ERROR | WRONG ROUND ID\n{Generate("BACK", 8954)}";
                            return tor;
                        }
                        tor = $"{Generate("BACK", 8954)}";
                        foreach (var p in Systems.Logs.LogManager.PlayerLogs[roundId].ToArray())
                            tor += GetColor($"({p.ID}) {p.Name}\n{p.UserId} | {p.IP}\n");
                    }
                    else
                    {
                        tor = menu.Get(out GeneratePlayerList);
                        foreach (var round in Systems.Logs.LogManager.PlayerLogs.ToArray())
                            tor += GetColor($"<color=red>[<color=red>RoundLog</color>]</color> ({round.Key}) {Systems.Logs.LogManager.RoundStartTime[round.Key]}\n");
                    }
                    return tor;
                case 9:
                    tor = menu.Get(out GeneratePlayerList);
                    tor = tor.Replace("START/STOP", Warhead.IsInProgress ? "  STOP" : "START");
                    tor = tor.Replace("ON/OFF", Warhead.LeverStatus ? "OFF" : " ON");
                    tor = tor.Replace("OPEN/CLOSE", Warhead.IsKeycardActivated ? "CLOSE" : "  OPEN");
                    tor = tor.Replace("$lstart", Systems.Misc.BetterWarheadHandler.Warhead.StartLock ? "green" : "red");
                    tor = tor.Replace("$lstop", Systems.Misc.BetterWarheadHandler.Warhead.StopLock ? "green" : "red");
                    tor = tor.Replace("$lbutton", Systems.Misc.BetterWarheadHandler.Warhead.ButtonLock ? "green" : "red");
                    tor = tor.Replace("$llever", Systems.Misc.BetterWarheadHandler.Warhead.LeverLock ? "green" : "red");
                    string time = Math.Round(Warhead.DetonationTimer).ToString();
                    if (time.Length < 3) 
                        time = "  " + time;
                    tor = tor.Replace("TIME_LEFT", Warhead.IsDetonated ? "DETONATED" : Warhead.IsInProgress ? $"               {time}" : "      PAUSED");
                    return tor;
                case 11:
                    tor = menu.Get(out GeneratePlayerList);
                    tor = tor.Replace("IMUTE/MUTE", InMuteMode.Contains(player.Id) ? " MUTE" : "IMUTE");
                    return tor;
                case 12:
                    tor = menu.Get(out GeneratePlayerList);
                    if (!PlayerListSelectedServer.TryGetValue(player.Id, out int s))
                        s = 2;
                    tor = tor.Replace("$s1", s == 1 ? "green" : "red");
                    tor = tor.Replace("$s2", s == 2 ? "green" : "red");
                    tor = tor.Replace("$s3", s == 3 ? "green" : "red");
                    tor = tor.Replace("$s4", s == 4 ? "green" : "red");

                    if (PlayerLists.TryGetValue(s, out string value))
                        tor += value;
                    return tor;
                case 13:
                    if (ReportSelectedReport.TryGetValue(player.Id, out int reportId))
                    {
                        tor = menu.Get(out GeneratePlayerList);
                        if (!Reports.Any(r => r.Report.ReportId == reportId))
                        {
                            tor = $"(0) ERROR | WRONG REPORT ID\n{Generate("BACK", 8350)}";
                            return tor;
                        }
                        tor = $"<size=1><color=#00000000>(-1)</color></size>[Report: {reportId}]\n{Generate("                      BACK", 8350)}\n{Generate("       Description", 8351)}\n{Generate("                      Done", 8352)}\n{Generate("       Procceding", 8353)}\n{Generate("                       Fail", 8354)}";
                    }
                    else
                    {
                        tor = menu.Get(out GeneratePlayerList);
                        foreach (var report in Reports.Take(20))
                            tor += $"<color={AIRS.ReportUpdateEventHandler.GetColorByStatus(report.Status)}>[#{report.Report.Type}]<size=1><color=#00000000>(-{report.Report.ReportId})</color></size>{report.Report.ReportedName}</color>\n";
                    }       
                    return tor;
            }
            return menu.Get(out GeneratePlayerList);
        }

        private static void RefreshPlayers()
        {
            SSL.Client.Send(MessageType.CMD_REQUEST_DATA, new RequestData
            {
                Type = MistakenSocket.Shared.API.DataType.SL_PLAYERLIST,
                argument = "0".Serialize(false)
            }).GetResponseDataCallback((data) =>
            {
                if (data.Type != MistakenSocket.Shared.API.ResponseType.OK)
                    return;
                var lists = data.Payload.Deserialize<KeyValuePair<int, SL_Player[]>[]>(0, 0, out _, false);
                foreach (var item in lists)
                {
                    if (!PlayerLists.ContainsKey(item.Key))
                        PlayerLists.Add(item.Key, "");
                    List<string> tmp = new List<string>();
                    foreach (var element in item.Value)
                    {
                        string type = $"<color=white>[<color=red>{element.Type}</color>]</color>";
                        string threat = $"<color=red><<color=white>{element.ThreatLevel}</color>></color>";
                        tmp.Add($"<color={RemoteAdmin.LOFHPatch.RoleToColor((RoleType)element.Role, false)}>{(element.Type == SL_PlayerType.USER ? "" : type)} <color=red>|<color=white>{element.Bans}</color>|</color>{(element.ThreatLevel == 0 ? "" : threat)} ({element.Id}) {element.Nickname} | {element.UserId}</color>");
                    }
                    PlayerLists[item.Key] = string.Join("\n", tmp.ToArray());
                }
                ForceRefreshPlayerList(12);
            });
        }

        public static void RefreshReports()
        {
            SSL.Client.Send(MessageType.CMD_REQUEST_DATA, new RequestData
            {
                Type = DataType.CMD_REPORTS,
                argument = "".Serialize(false)
            }).GetResponseDataCallback((data) =>
            {
                if (data.Type != ResponseType.OK)
                    return;
                var tmp = data.Payload.Deserialize<(ReportData Report, ReportStatusType Status, DateTime Timestamp)[]>(0, 0, out _, false).ToList();
                Reports = tmp.Where(i => (DateTime.Now - i.Timestamp).TotalMinutes < 30).OrderBy(i => -i.Timestamp.Ticks).ToArray();
                ForceRefreshPlayerList(13);
            });
        }

        public static void ForceRefreshPlayerList(int menuId) => CurrentMenus.Where(i => i.Value == menuId).ToList().ForEach(p => ForceRefreshPlayerList(RealPlayers.Get(p.Key)));
        private static void ForceRefreshPlayerList(Player p) => RemoteAdmin.LOFHPatch.Prefix("REQUEST_DATA PLAYER_LIST SILENT", p.Sender);
        
        private static string Generate(string name, int id) =>
            $"<color=green>[<color=orange>MENU</color>]<size=1><color=#00000000>({id})</color></size>{name}</color>\n";

        public static Menu GetMenu(int id) => Menus.FirstOrDefault(m => m.Id == id);


        public static string GenerateWarheadInfo()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("<size=150%><color=#00AA00><b>Warhead Info</b></color></size>");

            stringBuilder.Append($"\n<color=#00AA00>Status</color>: <color=orange>{(Warhead.IsDetonated ? "Detonated" : (Warhead.IsInProgress ? "Counting Down" : (Warhead.LeverStatus ? (Warhead.CanBeStarted ? "Enabled" : "On Cooldown") : "Disabled")))}</color>");
            stringBuilder.Append($"\n<color=#00AA00>Time Left</color>: <color=orange>{Math.Round(Mathf.Clamp(Warhead.RealDetonationTimer - Warhead.DetonationTimer, 0, 999))}</color>s");
            stringBuilder.Append($"\n<color=#00AA00>Cooldown</color>: <color=orange>{Math.Round(Mathf.Clamp(Warhead.DetonationTimer - Warhead.RealDetonationTimer, 0, 999))}</color>s");

            stringBuilder.Append($"\n<color=#00AA00>Last Start</color>: (<color=orange>{(Systems.Misc.BetterWarheadHandler.Warhead.LastStartUser?.Id.ToString() ?? "?")}</color>) <color=orange>{(Systems.Misc.BetterWarheadHandler.Warhead.LastStartUser?.Nickname ?? "NOBODY")}</color>");
            stringBuilder.Append($"\n<color=#00AA00>Last Stop </color>: (<color=orange>{(Systems.Misc.BetterWarheadHandler.Warhead.LastStopUser?.Id.ToString() ?? "?")}</color>) <color=orange>{(Systems.Misc.BetterWarheadHandler.Warhead.LastStopUser?.Nickname ?? "NOBODY")}</color>");
            return stringBuilder.ToString();
        }

        public static string GenerateServerInfo()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append("<size=150%><color=#00AA00>Server Info</color></size>");
            stringBuilder.Append($"\n<color=#00AA00>Players</color>: <color=orange>{RealPlayers.List.Count()}</color>/<color=orange>25</color>");
            stringBuilder.Append($"\n<color=#00AA00>Admins</color>: <color=orange>{RealPlayers.List.Where(p => p.CheckPermissions(PlayerPermissions.AdminChat)).Count()}</color>");
            var decontamination = (float)MapPlus.DecontaminationEndTime - (float)LightContainmentZoneDecontamination.DecontaminationController.GetServerTime;
            var decontamination_s = decontamination % 60;
            stringBuilder.Append($"\n<color=#00AA00>Decontamination</color>: " + (MapPlus.IsLCZDecontaminated() ? "<color=orange>DECONTAMINATED</color>" : $"<color=orange>{((decontamination - decontamination_s) / 60)}</color>m <color=orange>{(decontamination_s < 10 ? "0" : "") + Math.Round(decontamination_s)}</color>s"));
            stringBuilder.Append($"\n<color=#00AA00>SCP 106 Recontamination</color>: <color=orange>{(MapPlus.FemurBreaked ? "Used" : (MapPlus.Lured ? "Ready" : "Not Ready"))}</color>");
            stringBuilder.Append($"\n<color=#00AA00>Generators</color>: <color=orange>{Map.ActivatedGenerators}</color>/<color=orange>5</color>");
            stringBuilder.Append($"\n<color=#00AA00>Tickets MTF</color>: <color=orange>{Respawning.RespawnTickets.Singleton.GetAvailableTickets(Respawning.SpawnableTeamType.NineTailedFox)}</color>");
            stringBuilder.Append($"\n<color=#00AA00>Tickets CI</color>: <color=orange>{Respawning.RespawnTickets.Singleton.GetAvailableTickets(Respawning.SpawnableTeamType.ChaosInsurgency)}</color>");
            var ttr = Mathf.RoundToInt(Respawning.RespawnManager.Singleton._timeForNextSequence - (float)Respawning.RespawnManager.Singleton._stopwatch.Elapsed.TotalSeconds);
            var ttr_s = ttr % 60;
            stringBuilder.Append($"\n<color=#00AA00>Respawn Time</color>: <color=orange>{(ttr - ttr_s) / 60}</color>m <color=orange>{(ttr_s < 10 ? "0" : "") + ttr_s}</color>s");
            var classDTime = ClassDCellsDecontaminationHandler.DecontaminatedIn;
            var classDTime_s = classDTime % 60;
            stringBuilder.Append($"\n<color=#00AA00>Class D Decontamination</color>:" + (ClassDCellsDecontaminationHandler.Decontaminated ? "<color=orange>Decontaminated</color>" : $"<color=orange>{(classDTime - classDTime_s) / 60}</color>m <color=orange>{(classDTime_s < 10 ? "0" : "") + classDTime_s}</color>s"));
            
            return stringBuilder.ToString();
        }

        public static readonly List<Menu> Menus = new List<Menu>()
        {
            new Menu(0, "DEFAULT", true, -1
            ),
            new Menu(1, "                 MENU                     ", false, 0,
                (8350, "                 REPORTS"),
                (8955, "                        BAN"),
                (8956, "       IMUTE/MUTE"),
                (8100, "           Commands"), 
                (8200, "                      NPCs"),
                (8250, "               Warhead"),
                (8052, "                    Server"),
                (8051, "  Request Restart")
            ), 
            new Menu(2, "           COMMANDS                 ", false, 1,
                (8951, "        Get Attacker"), 
                (8952, "        Last Attacker"), 
                (8953, "     Player TK Data"), 
                (8954, "            Player Log"),
                (8960, "                      TALK"),
                (8150, "               Overheat"),
                (8101, "                      <color=$hide>HIDE</color>"),
                (8102, "          <color=$v0>VANISH | 0</color>"),
                (8103, "          <color=$v1>VANISH | 1</color>"),
                (8104, "          <color=$v2>VANISH | 2</color>"),
                (8105, "          <color=$v3>VANISH | 3</color>"),
                (8106, "                       TUT"),
                (8107, "                 V | TUT"),
                (8108, "     NOCLIP | TUT"),
                (8109, "V, NOCLIP | TUT")
            ),
            new Menu(3, "           OVERHEAT                 ", false, 2,
                (8160, "                 CANCEL"),
                (8151, "            30 Minutes"), 
                (8152, "            25 Minutes"), 
                (8153, "            20 Minutes"), 
                (8154, "            15 Minutes"), 
                (8155, "            10 Minutes"), 
                (8156, "            05 Minutes"), 
                (8157, "            03 Minutes"), 
                (8158, "            90 Seconds"), 
                (8159, "            00 Seconds")
            ),
            new Menu(4, "                 NPCs                      ", false, 1
            ),
            new Menu(5, "                    GA                        ", true, 2
            ),
            new Menu(6, "                    LA                        ", true, 2
            ),
            new Menu(7, "                 PTKD                     ", true, 2
            ),
            new Menu(8, "          ROUND LOG                ", false, 2
            ),
            new Menu(9, "            WARHEAD                  ", false, 1,
                (8251, "                        INFO"),
                (8252, "                     START/STOP"),
                (8253, "                          ON/OFF"),
                (8254, "                      OPEN/CLOSE"),
                (8255, "           <color=$lstart>LOCK START</color>"),
                (8256, "             <color=$lstop>LOCK STOP</color>"),
                (8257, "         <color=$lbutton>LOCK BUTTON</color>"),
                (8258, "           <color=$llever>LOCK LEVER</color>"),
                (0   , "               TIME_LEFT")
            ),
            new Menu(10, "                  BAN                       ", true, 1
            ),
            new Menu(11, "                 MUTE                     ", true, 1,
                (8957, "         MODE: IMUTE/MUTE")
            ),
            new Menu(12, "        PLAYER LIST             ", false, 1,
                (8301, "                   <color=$s1>SERVER #1</color>"),
                (8302, "                   <color=$s2>SERVER #2</color>"),
                (8303, "                   <color=$s3>SERVER #3</color>"),
                (8304, "                   <color=$s4>SERVER #4</color>")
            ),
            new Menu(13, "            REPORTS                 ", false, 1,
                (8958, "                  REFRESH")
            ),
            new Menu(14, "               TALK                    ", true, 2
            ),
        };
        public class Menu
        {
            public int Id;
            public string Name;

            public bool GeneratePlayerList;
            public (int, string)[] Options;
            public int Parent;

            public Menu(int id, string name, bool generatePlayerList, int parent, params (int, string)[] options)
            {
                this.Id = id;
                this.Name = name;
                this.GeneratePlayerList = generatePlayerList;
                this.Options = options;
                this.Parent = parent;
            }

            public string Get(out bool GeneratePlayerList)
            {
                GeneratePlayerList = this.GeneratePlayerList;
                if(this.Id == 0)
                    return $"{Generate("                      MENU", 8000)}{string.Concat(Options.Select(i => Generate(i.Item2, i.Item1)))}";
                else
                    return $"<color=red>[<color=green><size=1><color=#00000000>({-1})</color></size>{Name}</color>]</color>\n{Generate("                      BACK", 8000)}{string.Concat(Options.Select(i => Generate(i.Item2, i.Item1)))}";
            }
        }
    }
}
