using Exiled.API.Features;
using Gamer.Diagnostics;
using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Gamer.Mistaken.BetterSCP.SCP173
{
    internal class SCP173Handler : Module
    {
        public override string Name => nameof(SCP173Handler);
        public SCP173Handler(PluginHandler p) : base(p)
        {
        }

        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.RoundStarted += this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Player.InteractingDoor += this.Handle<Exiled.Events.EventArgs.InteractingDoorEventArgs>((ev) => Player_InteractingDoor(ev));
        }

        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Player.InteractingDoor -= this.Handle<Exiled.Events.EventArgs.InteractingDoorEventArgs>((ev) => Player_InteractingDoor(ev));
        }

        private void Player_InteractingDoor(Exiled.Events.EventArgs.InteractingDoorEventArgs ev)
        {
            if (Round.ElapsedTime.TotalSeconds < 25 && ev.Player.Role == RoleType.Scp173)
                ev.IsAllowed = false;
        }

        private void Server_RoundStarted()
        {
            Timing.RunCoroutine(LockStart());
        }

        private IEnumerator<float> LockStart()
        {
            yield return Timing.WaitForSeconds(0.1f);
            while (Round.ElapsedTime.TotalSeconds < 25)
            {
                Gamer.Mistaken.Systems.InfoMessage.InfoMessageManager.WelcomeMessages[RoleType.Scp173] = $"Zostaniesz wypuszczony za <color=yellow>{Mathf.RoundToInt(25 - (float)Round.ElapsedTime.TotalSeconds)}</color>s";
                yield return Timing.WaitForSeconds(1);
            }
            Gamer.Mistaken.Systems.InfoMessage.InfoMessageManager.WelcomeMessages.Remove(RoleType.Scp173);
        }
    }
}
