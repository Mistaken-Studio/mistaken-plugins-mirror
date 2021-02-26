using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exiled.API.Features;
using Gamer.Utilities;
using MistakenSocket.Client;
using MistakenSocket.Shared;
using MistakenSocket.Shared.AIRS;
using MistakenSocket.Shared.API;

namespace Gamer.Mistaken.AIRS
{
    public class ReportStatusUpdateHandler : MistakenSocket.Client.Handlers.Handler<ReportStatusUpdateData>
    {
        protected override MessageType Type => MessageType.CMD_RESPOND_REPORT_STATUS;
        public override ResponseData? Execute()
        {
            if (!ReportHandler.Reports.TryGetValue(Data.ReportId, out ReportHandler.ExtendedReportData reportInfo))
                return new ResponseData(ResponseType.SERVER_ERROR, "Unknown report");
            reportInfo.SetStatus((ReportHandler.ReportStatus)Data.Status, Data.Message);

            if (!Player.UserIdsCache.TryGetValue(Data.ReporterUserId, out Player target))
                Exiled.API.Features.Log.Info($"Report({Data.ReportId} || {Data.ReporterUserId}) Status Updated | Status: {Data.Status} | Message: {Data.Message}");
            else
            {
                string tmpMsg = $"\nMessage: {Data.Message}";
                string message = $"Report Id: {Data.ReportId} | Reported: {reportInfo.Data.ReportedName}\nReport status was updated to {Data.Status}{(string.IsNullOrWhiteSpace(Data.Message) ? "" : tmpMsg)}";
                target.SendConsoleMessage(message, GetColorByStatus(Data.Status));
            }
            LOFH.MenuSystem.RefreshReports();

            return new ResponseData(ResponseType.OK, "Understood");
        }

        public static string GetColorByStatus(ReportStatusType status)
        {
            switch(status)
            {
                case ReportStatusType.FAILED:
                    return "red";
                case ReportStatusType.PROCCEDING:
                    return "blue";
                case ReportStatusType.NONE:
                    return "white";
                case ReportStatusType.DONE:
                    return "green";
                default:
                    return "magenta";
            }
        }
    }
}
