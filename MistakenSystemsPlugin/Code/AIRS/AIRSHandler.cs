using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exiled.API.Features;
using Gamer.Diagnostics;
using Gamer.Utilities;
using MEC;
using MistakenSocket.Client.SL;
using UnityEngine;

namespace Gamer.Mistaken.AIRS
{
    public class Handler : Module
    {
        public override string Name => "AIRS";
        public Handler(PluginHandler plugin) : base(plugin)
        {
        }
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.LocalReporting += this.Handle<Exiled.Events.EventArgs.LocalReportingEventArgs>((ev) => Server_LocalReporting(ev));
            Exiled.Events.Handlers.Server.RestartingRound += this.Handle(() => Server_RestartingRound(), "RoundRestart");
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.LocalReporting -= this.Handle<Exiled.Events.EventArgs.LocalReportingEventArgs>((ev) => Server_LocalReporting(ev));
            Exiled.Events.Handlers.Server.RestartingRound -= this.Handle(() => Server_RestartingRound(), "RoundRestart");
        }

        public static readonly HashSet<int> AlreadyReported = new HashSet<int>();
        private void Server_LocalReporting(Exiled.Events.EventArgs.LocalReportingEventArgs ev)
        {
            string reason = ev.Reason;
            if(string.IsNullOrWhiteSpace(reason))
            {
                ev.Issuer.Broadcast("REPORT", 10, "!! Podaj powód zgłoszenia !!", Broadcast.BroadcastFlags.AdminChat);
                ev.IsAllowed = false;
                return;
            }
            if(AlreadyReported.Contains(ev.Target.Id))
            {
                ev.Issuer.Broadcast("REPORT", 10, "!! Ten gracz został już zgłoszony !!", Broadcast.BroadcastFlags.AdminChat);
                ev.IsAllowed = false;
                return;
            }
            int id = ev.Target.Id;
            AlreadyReported.Add(id);
            MEC.Timing.CallDelayed(60, () => AlreadyReported.Remove(id));
            ev.IsAllowed = ReportHandler.ExecuteReport(ev.Issuer.ReferenceHub, ev.Target.ReferenceHub, ref reason);
            ev.Reason = reason;
        }


        private void Server_RestartingRound()
        {
            AlreadyReported.Clear();
        }
    }
}
