﻿using Gamer.Utilities;
using MistakenSocket.Client.SL;
using MistakenSocket.Shared.AIRS;
using MistakenSocket.Shared.API;
using MistakenSocket.Shared.Events;

namespace Gamer.Mistaken.AIRS
{
    public class ReportReciveEventHandler : MistakenSocket.Client.Handlers.Handler<ReportReciveEvent>
    {
        protected override MessageType Type => MessageType.REPORT_RECIVE_EVENT;
        public override ResponseData? Execute()
        {
            LOFH.MenuSystem.RefreshReports();
            if (Data.ServerId == (byte)SSL.Client.MyType)
                return null;
            MapPlus.Broadcast("AIRS", 10, $"[{Data.Server}] ({Data.Suspect.Id}) {Data.Suspect.Nickname} was reported by {Data.ReporterName} for {Data.Reason}", Broadcast.BroadcastFlags.AdminChat);
            return null;
        }

        public static string GetColorByStatus(ReportStatusType status)
        {
            switch (status)
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
