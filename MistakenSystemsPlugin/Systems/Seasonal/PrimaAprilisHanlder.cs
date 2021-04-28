using Gamer.Diagnostics;
using System;

namespace Gamer.Mistaken.Systems.Seasonal
{
    internal class PrimaAprilisHanlder : Module
    {
        public PrimaAprilisHanlder(PluginHandler p) : base(p) { }

        public override string Name => "PrimaAprilis";
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.WaitingForPlayers += this.Handle(() => Server_WaitingForPlayers(), "WaitingForPlayers");
            Exiled.Events.Handlers.Player.ChangingRole += this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.WaitingForPlayers -= this.Handle(() => Server_WaitingForPlayers(), "WaitingForPlayers");
            Exiled.Events.Handlers.Player.ChangingRole -= this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            if (!active)
                return;
            if (ev.NewRole == RoleType.Spectator)
                return;
            MEC.Timing.CallDelayed(0.25f, () =>
            {
                ev.Player.Scale = new UnityEngine.Vector3(UnityEngine.Random.Range(0.25f, 1.1f), UnityEngine.Random.Range(0.5f, 1f), UnityEngine.Random.Range(0.25f, 1.1f));
            });
        }

        private const string EnableDate = "01.04";
        private bool active = false;
        private void Server_WaitingForPlayers()
        {
            active = false;
            //if (Server.Port == 7791)
            //    active = true;
            if (EnableDate == DateTime.Now.ToString("dd.MM"))
                active = true;
        }
    }
}
