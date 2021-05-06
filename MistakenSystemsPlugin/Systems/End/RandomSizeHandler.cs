using Gamer.Diagnostics;
using Gamer.Utilities;
using UnityEngine;

namespace Gamer.Mistaken.Systems.End
{
    internal class RandomSizeHandler : Module
    {
        public RandomSizeHandler(PluginHandler p) : base(p)
        {
        }

        public override bool Enabled => false;
        public override string Name => "RandomSize";
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Player.ChangingRole += this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Player.ChangingRole -= this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            if (ev.Player == null) return;
            this.CallDelayed(5, () =>
            {
                if (ev.Player.Team == Team.SCP || ev.Player.Team == Team.TUT || ev.Player.Team == Team.RIP)
                {
                    ev.Player.Scale = Vector3.one;
                    return;
                }

                float diff = UnityEngine.Random.Range(0, 0.06f);
                if (diff != 0)
                    ev.Player.Scale = new Vector3(1 + (diff / 2), 1 - diff, 1 + (diff / 2));
            }, "ChangeRole");
        }
    }
}
