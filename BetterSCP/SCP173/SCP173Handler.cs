using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exiled.API.Features;
using Gamer.Utilities;
using MEC;
using UnityEngine;
using Gamer.Diagnostics;

namespace Gamer.Mistaken.BetterSCP.SCP173
{
    class SCP173Handler : Module
    {
        public override string Name => nameof(SCP173Handler);
        public SCP173Handler(PluginHandler p) : base(p)
        {  
        }

        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.RoundStarted += this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Player.InteractingDoor += this.Handle<Exiled.Events.EventArgs.InteractingDoorEventArgs>((ev) => Player_InteractingDoor(ev));
            Exiled.Events.Handlers.Player.ChangingRole += this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
            Exiled.Events.Handlers.Player.Hurting += this.Handle<Exiled.Events.EventArgs.HurtingEventArgs>((ev) => Player_Hurting(ev));
        }

        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Player.InteractingDoor -= this.Handle<Exiled.Events.EventArgs.InteractingDoorEventArgs>((ev) => Player_InteractingDoor(ev));
            Exiled.Events.Handlers.Player.ChangingRole -= this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
            Exiled.Events.Handlers.Player.Hurting -= this.Handle<Exiled.Events.EventArgs.HurtingEventArgs>((ev) => Player_Hurting(ev));
        }

        private void Player_Hurting(Exiled.Events.EventArgs.HurtingEventArgs ev)
        {
            if(DmgMultiplayer)
            {
                if (ev.Target.Role == RoleType.Scp173)
                    ev.Amount *= 1.5f;
            }
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            if(ev.NewRole == RoleType.Scp173)
                Timing.RunCoroutine(AntyDuo(ev.Player));
        }

        private void Player_InteractingDoor(Exiled.Events.EventArgs.InteractingDoorEventArgs ev)
        {
            if(Round.ElapsedTime.TotalSeconds < 25 && ev.Player.Role == RoleType.Scp173)
                ev.IsAllowed = false;
        }

        private void Server_RoundStarted()
        {
            Timing.RunCoroutine(LockStart());
        }

        private bool DmgMultiplayer = false;
        private IEnumerator<float> AntyDuo(Player player)
        {
            yield return Timing.WaitForSeconds(1);
            while(player?.Role == RoleType.Scp173)
            {
                DmgMultiplayer = false;
                if (player.Position.y < 900)
                {
                    foreach (var item in RealPlayers.Get(RoleType.Scp096))
                    {
                        if (Vector3.Distance(player.Position, item.Position) < PluginHandler.Anty173_096DuoDistance)
                        {
                            DmgMultiplayer = true;
                            player.EnableEffect<CustomPlayerEffects.Concussed>();
                            player.ShowHint("Jesteś za blisko <color=yellow>SCP 096</color>, z tego powodu będziesz dostawał <color=yellow>150</color>% obrażeń", true, 3, false);
                            break;
                        }
                    }
                }
                if(!DmgMultiplayer)
                {
                    player.DisableEffect<CustomPlayerEffects.Concussed>();
                }
                yield return Timing.WaitForSeconds(2);
            }
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
