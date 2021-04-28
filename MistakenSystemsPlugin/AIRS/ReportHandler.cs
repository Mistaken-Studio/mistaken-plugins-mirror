using Exiled.API.Features;
using Gamer.Utilities;
using MistakenSocket.Client;
using MistakenSocket.Client.SL;
using MistakenSocket.Shared;
using MistakenSocket.Shared.AIRS;
using MistakenSocket.Shared.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Log = Exiled.API.Features.Log;

namespace Gamer.Mistaken.AIRS
{
    public static class ReportHandler
    {
        public static bool ExecuteReport(ReferenceHub reporter, ReferenceHub reported, ref string Reason)
        {
            try
            {
                var Target = Player.Get(reported);
                var Issuer = Player.Get(reporter);
                if (Issuer == null)
                {
                    if (reporter == null)
                    {
                        Log.Error("Reporter is null");
                        return false;
                    }
                    Log.Warn("Reporter is not player");
                    reporter.GetComponent<GameConsoleTransmission>().SendToClient(reporter.characterClassManager.connectionToClient, "[REPORTING] Failed to Report, You are not player", "red");
                    return false;
                }

                if (Target == null)
                {
                    reporter.GetComponent<GameConsoleTransmission>().SendToClient(reporter.characterClassManager.connectionToClient, "[REPORTING] Failed to Report, Target is not player or not found", "red");
                    return false;
                }

                if (Reason.ToUpper().Contains("AFK"))
                {
                    if (!Systems.AntiAFK.Handler.AfkPosition.TryGetValue(Target.Id, out KeyValuePair<int, UnityEngine.Vector3> value))
                    {
                        int time = value.Key;
                        if (time > 3)
                        {
                            time += 12;
                            if (time > 12)
                                time = 12;
                            Systems.AntiAFK.Handler.AfkPosition[Target.Id] = new KeyValuePair<int, UnityEngine.Vector3>(time, value.Value);
                        }
                    }
                }

                int serverType = -1;
                try
                {
                    serverType = (Server.Port - 7776);
                    if (CustomNetworkManager.Ip.StartsWith("188.68.252.20"))
                        serverType = 4 + (Server.Port - 8004);
                    else if (CustomNetworkManager.Ip.StartsWith("31.11.249.2"))
                        serverType = 10;
                }
                catch (System.Exception e)
                {
                    Log.Error("ServerType Detection Error");
                    Log.Error(e.Message);
                    Log.Error(e.StackTrace);
                }

                try
                {
                    MapPlus.Broadcast("AIRS", 10, $"[LOCAL] ({Target?.Id}) {Target?.Nickname} was reported by ({Issuer?.Id}) {Issuer?.Nickname} for {Reason}", Broadcast.BroadcastFlags.AdminChat);
                }
                catch (System.Exception e)
                {
                    Log.Error("Broadcast Error");
                    Log.Error(e.Message);
                    Log.Error(e.StackTrace);
                }
                ReportData data = new ReportData();
                try
                {
                    data = new ReportData
                    {
                        ReportId = -1,
                        ReportType = (short)GuessReportType(Reason),
                        ReporterUserId = Issuer?.UserId,
                        ReporterName = Issuer?.Nickname,
                        ReportedName = Target?.Nickname,
                        ReportedData = Target?.ToPlayer(),
                        Type = (ServerType)serverType,
                        Reason = Reason
                    };
                }
                catch (System.Exception e)
                {
                    Log.Error("Report Data Creation Error");
                    Log.Error(e.Message);
                    Log.Error(e.StackTrace);
                    return false;
                }
                var report = new ExtendedReportData(data, Issuer);

                var messageId = SSL.Client.Send(MessageType.SL_REPORT, data);
                if (!messageId.HasValue)
                {
                    Issuer.SendConsoleMessage($"[AIRS] Report request was <b>NOT</b> sent\nCurrent Report Status: {ReportStatus.ERROR}", "red");
                    return false;
                }
                Issuer.SendConsoleMessage($"[AIRS] Report request was sent\nCurrent Report Status: {ReportStatus.AWAITING_CONFIRMATION}", "green");
                messageId.GetResponseDataCallback((response) =>
                {
                    if (response.Type != ResponseType.OK)
                    {
                        Issuer.SendConsoleMessage($"[AIRS] Report request failed\n{response.Response}\nCurrent Report Status: {ReportStatus.ERROR}", "red");
                        report.Issuer.ClearBroadcasts();
                        report.Issuer.Broadcast("AIRS", 10, $"Your report for {report.Data.ReportedName} <b>failed</b> to issue", Broadcast.BroadcastFlags.AdminChat);
                        report.SetStatus(ReportStatus.ERROR, response.Response);
                        return;
                    }
                    report.Issuer.ClearBroadcasts();
                    report.Issuer.Broadcast("AIRS", 10, $"Your report for {report.Data.ReportedName} was successfully issued", Broadcast.BroadcastFlags.AdminChat);
                    int reportId = int.Parse(response.Response);
                    try
                    {
                        report.SetId(reportId);
                        report.SetStatus(ReportStatus.CONFIRMED);
                        Reports.Add(reportId, report);
                    }
                    catch (System.Exception e)
                    {
                        Log.Error("Report Data Add Error");
                        Log.Error(e.Message);
                        Log.Error(e.StackTrace);
                        return;
                    }
                });
                
                return true;
            }
            catch (System.Exception e)
            {
                Log.Error("Global Error");
                Log.Error(e.Message);
                Log.Error(e.StackTrace);
                return false;
            }
        }

        public static Dictionary<int, ExtendedReportData> Reports { get; } = new Dictionary<int, ExtendedReportData>();

        public class ExtendedReportData
        {
            public ReportData Data;
            public ReportStatus Status { get; private set; }
            public Player Issuer;

            public void SetStatus(ReportStatus status, string reason = "")
            {
                Log.Debug($"Updated Report({Data.ReportId}) Status from {Status} to {status}");
                try
                {
                    if (Issuer?.IsConnected ?? false)
                    {
                        if (status > 0)
                        {
                            Issuer.Broadcast("AIRS", 10, $"Report({Data.ReportId}) for {Data.ReportedName} status was changed from {Status} to {status}, check '~' for more info", Broadcast.BroadcastFlags.AdminChat);
                            Issuer.ClearBroadcasts();
                        }
                        Issuer.SendConsoleMessage($"[AIRS] Report({Data.ReportId}) for {Data.ReportedName} status was changed from {Status} to {status}", "green");

                        if (status == ReportStatus.ERROR)
                        {
                            if (reason == "")
                                Issuer.SendConsoleMessage($"[AIRS] Unexpected Error has ocured", "red");
                            else
                                Issuer.SendConsoleMessage($"[AIRS] Error has ocured\n{reason}", "red");
                        }
                    }
                }
                catch(System.Exception e)
                {
                    Log.Error("Failed to display report info to reporter");
                    Log.Error(e.Message);
                    Log.Error(e.StackTrace);
                }
                if((int)status != -10 && (int)status < 1 && (int)status < (int)Status)
                {
                    Log.Warn("Tried to update status to older one");
                    return;
                }    
                Status = status;
            }

            public void SetId(int Id)
            {
                Data = new ReportData
                {
                    Reason = Data.Reason,
                    ReportedData = Data.ReportedData,
                    ReportedName = Data.ReportedName,
                    ReporterName = Data.ReporterName,
                    ReporterUserId = Data.ReporterUserId,
                    ReportId = Id,
                    ReportType = Data.ReportType,
                    Type = Data.Type
                };
            }

            public ExtendedReportData(ReportData data, Player issuer)
            {
                Data = data;
                Status = ReportStatus.AWAITING_CONFIRMATION;
                Issuer = issuer;
            }
        }

        [Flags]
        public enum ReportType : short
        {
            UNKNOWN = 0,
            TEAMKILL = 1,
            CHEATS = 2,
            TEAMING = 4,
        }

        public enum ReportStatus : short
        {
            ERROR = -10,
            AWAITING_CONFIRMATION = -2,
            CONFIRMED = 0,
            DONE,
            FAILED,
            PROCCEDING,
        }

        public static ReportType GuessReportType(string reason)
        {
            string tmp = reason.ToLower();
            var tor = ReportType.UNKNOWN;
            if (tmp.Contains("tk"))
                tor |= ReportType.TEAMKILL;
            else if (tmp.Contains("tkill"))
                tor |= ReportType.TEAMKILL;
            else if (tmp.Contains("teamkill"))
                tor |= ReportType.TEAMKILL;
            else if (tmp.Contains("team kill"))
                tor |= ReportType.TEAMKILL;

            if (tmp.Contains("cheats"))
                tor |= ReportType.CHEATS;
            else if (tmp.Contains("cheater"))
                tor |= ReportType.CHEATS;
            else if (tmp.Contains("hacker"))
                tor |= ReportType.CHEATS;
            else if (tmp.Contains("haker"))
                tor |= ReportType.CHEATS;
            else if (tmp.Contains("cheatuje"))
                tor |= ReportType.CHEATS;
            else if (tmp.Contains("czituje"))
                tor |= ReportType.CHEATS;
            else if (tmp.Contains("cziter"))
                tor |= ReportType.CHEATS;

            if (tmp.Contains("teaming"))
                tor |= ReportType.TEAMING;
            else if (tmp.Contains("sojusz"))
                tor |= ReportType.TEAMING;
            else if (tmp.Contains("team z"))
                tor |= ReportType.TEAMING;
            else if (tmp.Contains("team with"))
                tor |= ReportType.TEAMING;

            return tor;
        }
    }
}
