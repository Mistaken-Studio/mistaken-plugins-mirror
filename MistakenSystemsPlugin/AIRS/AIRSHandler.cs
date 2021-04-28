using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exiled.API.Features;
using Gamer.Diagnostics;
using Gamer.Mistaken.Systems.GUI;
using Gamer.Mistaken.Systems.Staff;
using Gamer.RoundLoggerSystem;
using Gamer.Utilities;
using MEC;
using MistakenSocket.Client.SL;
using UnityEngine;

namespace Gamer.Mistaken.AIRS
{
    public class Handler : Module
    {
        public override bool IsBasic => true;
        public override string Name => "AIRS";
        public Handler(PluginHandler plugin) : base(plugin)
        {
        }
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.LocalReporting += this.Handle<Exiled.Events.EventArgs.LocalReportingEventArgs>((ev) => Server_LocalReporting(ev));
            Exiled.Events.Handlers.Server.RestartingRound += this.Handle(() => Server_RestartingRound(), "RoundRestart");
            Exiled.Events.Handlers.Player.ChangingRole += this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
            Exiled.Events.Handlers.Player.Died += this.Handle<Exiled.Events.EventArgs.DiedEventArgs>((ev) => Player_Died(ev));
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.LocalReporting -= this.Handle<Exiled.Events.EventArgs.LocalReportingEventArgs>((ev) => Server_LocalReporting(ev));
            Exiled.Events.Handlers.Server.RestartingRound -= this.Handle(() => Server_RestartingRound(), "RoundRestart");
            Exiled.Events.Handlers.Player.ChangingRole -= this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
            Exiled.Events.Handlers.Player.Died -= this.Handle<Exiled.Events.EventArgs.DiedEventArgs>((ev) => Player_Died(ev));
        }
        public static int Reports = 0;
        public static int ReportsOnThisServer = 0;
        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            if (!ev.Player.IsStaff())
                return;
            if (ev.NewRole == RoleType.Spectator || ev.NewRole == RoleType.Tutorial)
                Update(ev.Player, false);
            else
                Update(ev.Player, true);
        }

        private void Player_Died(Exiled.Events.EventArgs.DiedEventArgs ev)
        {
            if (!ev.Target.IsStaff())
                return;
            Update(ev.Target, false);
        }
        public static void UpdateAll()
        {
            foreach (var player in RealPlayers.List)
            {
                if (!player.IsStaff())
                    continue;
                if(player.Role == RoleType.Tutorial || player.Role == RoleType.Spectator)
                    Update(player, false);
                else
                    Update(player, true);
            }
        }
        private static void Update(Player p, bool hide)
        {
            Base.GUI.PseudoGUIHandler.Set(p, "AIRS", Base.GUI.PseudoGUIHandler.Position.TOP, hide ? null : $"Reports: <color=yellow>{Reports}</color> | Reports on #<color=yellow>{Server.Port - 7776}</color>: <color=yellow>{ReportsOnThisServer}</color>");
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
            RoundLogger.Log("AIRS", "REPORT", $"{ev.Target.PlayerToString()} was reportd by {ev.Issuer.PlayerToString()} for \"{ev.Reason}\"");
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
