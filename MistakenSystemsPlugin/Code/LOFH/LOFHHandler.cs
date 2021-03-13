using System;
using System.Linq;
using System.Net;
using System.Text;
using Exiled.API.Features;
using Gamer.Utilities;
using Gamer.Mistaken.Utilities.APILib;
using Gamer.Mistaken.CommandsExtender.Commands;
using MEC;
using Newtonsoft.Json;
using MistakenSocket.Shared;
using System.Threading;
using System.Threading.Tasks;
using Gamer.Diagnostics;
using MistakenSocket.Shared.API;
using MistakenSocket.Client.SL;

namespace Gamer.Mistaken.LOFH
{
    public class Handler : Module
    {
        public Handler(PluginHandler plugin) : base(plugin)
        {
        }

        public override string Name => "LOFH";
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Player.PreAuthenticating += this.Handle<Exiled.Events.EventArgs.PreAuthenticatingEventArgs>((ev) => Player_PreAuthenticating(ev));
            Exiled.Events.Handlers.Server.RestartingRound += this.Handle(() => Server_RestartingRound(), "RoundRestart");

            MEC.Timing.CallDelayed(10, () =>
            {
                SSL.Client.Send(MessageType.BLACKLIST_GET_REQUEST, null).GetResponseDataCallback((response) =>
                {
                    if (response.Type == MistakenSocket.Shared.API.ResponseType.OK)
                    {
                        var data = response.Payload.Deserialize<MistakenSocket.Shared.Blacklist.BlacklistEntry[]>(0, 0, out _, false);

                        LOFH.WarningFlags.Clear();
                        foreach (var item in data)
                            LOFH.WarningFlags.Add(item.UserId, item);
                    }
                });
            });
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Player.PreAuthenticating -= this.Handle<Exiled.Events.EventArgs.PreAuthenticatingEventArgs>((ev) => Player_PreAuthenticating(ev));
            Exiled.Events.Handlers.Server.RestartingRound -= this.Handle(() => Server_RestartingRound(), "RoundRestart");
        }

        private void Server_RestartingRound()
        {
            MenuSystem.CurrentMenus.Clear();
            MenuSystem.PlayerLogSelectedRound.Clear();
            MenuSystem.InMuteMode.Clear();
            MenuSystem.AutoRepeat.Clear();
            MenuSystem.ReportSelectedReport.Clear();
        }

        private void Player_PreAuthenticating(Exiled.Events.EventArgs.PreAuthenticatingEventArgs ev)
        {
            if (((CentralAuthPreauthFlags)ev.Flags).HasFlagFast(CentralAuthPreauthFlags.NorthwoodStaff) && !ev.UserId.IsDevUserId())
                RemoteAdmin.LOFHPatch.DisabledFor.Add(ev.UserId);
            if (!LOFH.Country.ContainsKey(ev.UserId))
                LOFH.Country.Add(ev.UserId, ev.Country);
            else
                LOFH.Country[ev.UserId] = ev.Country;

            MEC.Timing.CallDelayed(1, () =>
            {
                if (!LOFH.ThreatLevelDatas.ContainsKey(ev.UserId))
                {
                    bool wFlag = false;
                    string reason = "";
                    if (LOFH.WarningFlags.TryGetValue(ev.UserId, out MistakenSocket.Shared.Blacklist.BlacklistEntry flag))
                    {
                        wFlag = true;
                        reason = flag.Reason;
                    }

                    Systems.ThreatLevel.ThreatLevelManager.GetThreatLevel(ev.UserId, ev.Request?.RemoteEndPoint?.Address?.ToString(), (Systems.ThreatLevel.ThreadLevelData data) =>
                    {
                        LOFH.ThreatLevelDatas.Add(ev.UserId, data);
                    }, wFlag, reason, ev.Country);
                }
            });
        }
    }
}
