using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Features;
using Gamer.Utilities;
using Gamer.Mistaken.BetterSCP;
using HarmonyLib;
using MEC;
using UnityEngine;
using Gamer.Diagnostics;

namespace Gamer.Mistaken.BetterSCP.SCP939
{
    class SCP939Handler : Diagnostics.Module
    {
        public override string Name => nameof(SCP939Handler);
        public SCP939Handler(PluginHandler p) : base(p)
        {
        }

        public override void OnEnable()
        {
            Exiled.Events.Handlers.Player.Died += this.Handle<Exiled.Events.EventArgs.DiedEventArgs>((ev) => Player_Died(ev));
            Exiled.Events.Handlers.Player.Hurting += this.Handle<Exiled.Events.EventArgs.HurtingEventArgs>((ev) => Player_Hurting(ev));
            Exiled.Events.Handlers.Player.ChangingRole += this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
        }

        public override void OnDisable()
        {
            Exiled.Events.Handlers.Player.Died -= this.Handle<Exiled.Events.EventArgs.DiedEventArgs>((ev) => Player_Died(ev));
            Exiled.Events.Handlers.Player.Hurting -= this.Handle<Exiled.Events.EventArgs.HurtingEventArgs>((ev) => Player_Hurting(ev));
            Exiled.Events.Handlers.Player.ChangingRole -= this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
        }

        private static float Decay;
        private static float Ratio;
        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            if(ev.NewRole.Is939())
            {
                Decay = ev.Player.ReferenceHub.playerStats.artificialHpDecay;
                ev.Player.ReferenceHub.playerStats.artificialHpDecay = 0;
                Ratio = ev.Player.ReferenceHub.playerStats.artificialNormalRatio;
                ev.Player.ReferenceHub.playerStats.artificialNormalRatio = 1;
            }
            else if (ev.NewRole == RoleType.Spectator)
            {
                ev.Player.ReferenceHub.playerStats.artificialHpDecay = Decay;
                ev.Player.ReferenceHub.playerStats.artificialNormalRatio = Ratio;
            }
        }

        private void Player_Died(Exiled.Events.EventArgs.DiedEventArgs ev)
        {
            if(ev.Killer?.Role.Is939() ?? false)
                ev.Killer.Health += UnityEngine.Random.Range(30, 50);
        }


        private void Player_Hurting(Exiled.Events.EventArgs.HurtingEventArgs ev)
        {
            if ((ev.Attacker?.Role.Is939() ?? false) && ev.Attacker.ArtificialHealth + 5 <= ev.Attacker.MaxArtificialHealth)
                ev.Attacker.ArtificialHealth += 5;
        }
    }
}
