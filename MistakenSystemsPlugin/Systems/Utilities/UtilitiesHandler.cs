using System;
using System.Collections.Generic;
using MEC;
using Mirror;
using Exiled.API.Features;
using Gamer.Utilities;
using UnityEngine;
using Gamer.Diagnostics;

namespace Gamer.Mistaken.Systems.Utilities
{
    internal class UtilitiesHandler : Module
    {
        public override bool IsBasic => true;
        public UtilitiesHandler(PluginHandler p) : base(p)
        {
        }

        public override string Name => "Utilities";
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.RespawningTeam += this.Handle<Exiled.Events.EventArgs.RespawningTeamEventArgs>((ev) => Server_RespawningTeam(ev));
            Exiled.Events.Handlers.Scp079.InteractingTesla += this.Handle<Exiled.Events.EventArgs.InteractingTeslaEventArgs>((ev) => Scp079_InteractingTesla(ev));
            Exiled.Events.Handlers.Player.TriggeringTesla += this.Handle<Exiled.Events.EventArgs.TriggeringTeslaEventArgs>((ev) => Player_TriggeringTesla(ev));
            Exiled.Events.Handlers.Server.RestartingRound += this.Handle(() => Server_RestartingRound(), "RoundRestart");
            Exiled.Events.Handlers.Player.Hurting += this.Handle<Exiled.Events.EventArgs.HurtingEventArgs>((ev) => Player_Hurting(ev));
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.RespawningTeam -= this.Handle<Exiled.Events.EventArgs.RespawningTeamEventArgs>((ev) => Server_RespawningTeam(ev));
            Exiled.Events.Handlers.Scp079.InteractingTesla -= this.Handle<Exiled.Events.EventArgs.InteractingTeslaEventArgs>((ev) => Scp079_InteractingTesla(ev));
            Exiled.Events.Handlers.Player.TriggeringTesla -= this.Handle<Exiled.Events.EventArgs.TriggeringTeslaEventArgs>((ev) => Player_TriggeringTesla(ev));
            Exiled.Events.Handlers.Server.RestartingRound -= this.Handle(() => Server_RestartingRound(), "RoundRestart");
            Exiled.Events.Handlers.Player.Hurting -= this.Handle<Exiled.Events.EventArgs.HurtingEventArgs>((ev) => Player_Hurting(ev));
        }

        private void Player_Hurting(Exiled.Events.EventArgs.HurtingEventArgs ev)
        {
            if (ev.Target.Side == ev.Attacker.Side && !API.Map.FriendlyFire && ev.Target.UserId != ev.Attacker.UserId)
                ev.Amount = 0;
        }

        private void Server_RestartingRound()
        {
            API.Map.Restart();
        }

        private void Player_TriggeringTesla(Exiled.Events.EventArgs.TriggeringTeslaEventArgs ev)
        {
            if (API.Map.TeslaMode == API.TeslaMode.DISABLED || API.Map.TeslaMode == API.TeslaMode.DISABLED_FOR_ALL) 
                ev.IsTriggerable = false;
        }

        private void Scp079_InteractingTesla(Exiled.Events.EventArgs.InteractingTeslaEventArgs ev)
        {
            if (API.Map.TeslaMode == API.TeslaMode.DISABLED_FOR_079 || API.Map.TeslaMode == API.TeslaMode.DISABLED_FOR_ALL) 
                ev.IsAllowed = false;
        }

        private void Server_RespawningTeam(Exiled.Events.EventArgs.RespawningTeamEventArgs ev)
        {
            if (API.Map.RespawnLock)
                ev.Players.Clear();
        }
    }
}
