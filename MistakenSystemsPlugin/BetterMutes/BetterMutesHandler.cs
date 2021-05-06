using Gamer.Diagnostics;
using Gamer.Utilities;
using MEC;
using System;
using System.Collections.Generic;

namespace Gamer.Mistaken.BetterMutes
{
    public class Handler : Module
    {
        public override bool IsBasic => true;
        public Handler(PluginHandler plugin) : base(plugin)
        {
            this.RunCoroutine(AutoMuteReload(), "AutoMuteReload");
        }

        public override string Name => "BetterMutes";
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Player.Verified += this.Handle<Exiled.Events.EventArgs.VerifiedEventArgs>((ev) => Player_Verified(ev));
            Exiled.Events.Handlers.Server.RestartingRound += this.Handle(() => Server_RestartingRound(), "RoundRestart");
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Player.Verified -= this.Handle<Exiled.Events.EventArgs.VerifiedEventArgs>((ev) => Player_Verified(ev));
            Exiled.Events.Handlers.Server.RestartingRound -= this.Handle(() => Server_RestartingRound(), "RoundRestart");
        }

        private IEnumerator<float> AutoMuteReload()
        {
            while (true)
            {
                global::MuteHandler.Reload();
                yield return Timing.WaitForSeconds(60);
            }
        }

        private void Server_RestartingRound()
        {
            MuteHandler.UpdateMutes();
        }

        private void Player_Verified(Exiled.Events.EventArgs.VerifiedEventArgs ev)
        {
            var mute = MuteHandler.GetMute(ev.Player.UserId);
            if (mute == null)
                return;
            var endString = mute.EndTime == -1 ? "NEVER" : new DateTime(mute.EndTime).ToString("dd.MM.yyyy HH:mm:ss");
            var reasonString = mute.Reason == "removeme" ? "No Reason" : mute.Reason;
            var type = mute.Intercom ? "Intercom" : "Server";
            ev.Player.Broadcast("MUTE", 5, $"You are {type} muted until {endString} UTC\nReason: {reasonString}", Broadcast.BroadcastFlags.AdminChat);
            ev.Player.SendConsoleMessage($"You are {type} muted until {endString} UTC\nReason: {reasonString}", "red");
        }
    }
}
