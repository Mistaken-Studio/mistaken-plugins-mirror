using Exiled.API.Features;
using Gamer.Diagnostics;

namespace Gamer.Mistaken.Systems.Misc
{
    public class BetterWarheadHandler : Module
    {
        public BetterWarheadHandler(PluginHandler p) : base(p)
        {
        }

        public override string Name => "BetterWarhead";
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.RoundStarted += this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Warhead.Detonated += this.Handle(() => Warhead_Detonated(), "WarheadDetonated");
            Exiled.Events.Handlers.Warhead.Starting += this.Handle<Exiled.Events.EventArgs.StartingEventArgs>((ev) => Warhead_Starting(ev));
            Exiled.Events.Handlers.Warhead.Stopping += this.Handle<Exiled.Events.EventArgs.StoppingEventArgs>((ev) => Warhead_Stopping(ev));
            Exiled.Events.Handlers.Player.ActivatingWarheadPanel += this.Handle<Exiled.Events.EventArgs.ActivatingWarheadPanelEventArgs>((ev) => Player_ActivatingWarheadPanel(ev));
            Exiled.Events.Handlers.Server.RestartingRound += this.Handle(() => Server_RestartingRound(), "RoundRestart");
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Warhead.Detonated -= this.Handle(() => Warhead_Detonated(), "WarheadDetonated");
            Exiled.Events.Handlers.Warhead.Starting -= this.Handle<Exiled.Events.EventArgs.StartingEventArgs>((ev) => Warhead_Starting(ev));
            Exiled.Events.Handlers.Warhead.Stopping -= this.Handle<Exiled.Events.EventArgs.StoppingEventArgs>((ev) => Warhead_Stopping(ev));
            Exiled.Events.Handlers.Player.ActivatingWarheadPanel -= this.Handle<Exiled.Events.EventArgs.ActivatingWarheadPanelEventArgs>((ev) => Player_ActivatingWarheadPanel(ev));
            Exiled.Events.Handlers.Server.RestartingRound -= this.Handle(() => Server_RestartingRound(), "RoundRestart");
        }

        public static WarheadStatus Warhead = new WarheadStatus
        {
            TimeLeft = -1,
            CountingDown = false,
            ButtonOpen = false,
            ButtonLock = false,
            LeverLock = false,
            StartLock = false,
            StopLock = false,
            Detonated = false,
            LastStartUser = null,
            LastStopUser = null,
            Enabled = false
        };
        public struct WarheadStatus
        {
            public bool CountingDown { get; set; }
            public float TimeLeft { get; set; }
            public bool LeverLock { get; set; }
            public bool StopLock { get; set; }
            public bool StartLock { get; set; }
            public bool ButtonOpen { get; set; }
            public bool ButtonLock { get; set; }
            public bool Detonated { get; set; }
            public bool Enabled { get; set; }
            public Player LastStartUser { get; set; }
            public Player LastStopUser { get; set; }
        }

        private void Server_RestartingRound()
        {
            Warhead.ButtonLock = false;
            Warhead.LeverLock = false;
            Warhead.StartLock = false;
            Warhead.StopLock = false;
        }

        private void Player_ActivatingWarheadPanel(Exiled.Events.EventArgs.ActivatingWarheadPanelEventArgs ev)
        {
            if (Warhead.ButtonLock)
                ev.IsAllowed = false;
            if (ev.Player.IsBypassModeEnabled)
                ev.IsAllowed = true;
            if (ev.IsAllowed)
                Warhead.ButtonOpen = true;
        }
        private void Warhead_Stopping(Exiled.Events.EventArgs.StoppingEventArgs ev)
        {
            if (ev.Player == null || ev.Player.Id == 1)
                return;
            if (Warhead.StopLock)
                ev.IsAllowed = false;
            if (ev.Player.IsBypassModeEnabled)
                ev.IsAllowed = true;

            if (ev.IsAllowed)
            {
                Warhead.TimeLeft = AlphaWarheadController.Host.NetworktimeToDetonation;
                if (ev.Player != null)
                    Warhead.LastStopUser = ev.Player;
                Warhead.CountingDown = false;
            }
        }
        private void Warhead_Starting(Exiled.Events.EventArgs.StartingEventArgs ev)
        {
            if (ev.Player == null || ev.Player.Id == 1)
                return;
            if (Warhead.StartLock)
                ev.IsAllowed = false;
            if (ev.Player.IsBypassModeEnabled)
                ev.IsAllowed = true;

            if (ev.IsAllowed)
            {
                if (ev.Player != null)
                    Warhead.LastStartUser = ev.Player;
                Warhead.CountingDown = true;
            }
        }

        private void Warhead_Detonated()
        {
            Warhead.Detonated = true;
            Warhead.CountingDown = false;
        }
        private void Server_RoundStarted()
        {
            Warhead.LastStartUser = null;
            Warhead.LastStopUser = null;
        }
    }
}
