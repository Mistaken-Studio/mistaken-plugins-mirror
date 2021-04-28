using Exiled.API.Enums;
using Exiled.API.Features;
using Gamer.Utilities;
using Grenades;
using MEC;
using Mirror;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using UnityEngine;
using Exiled.API.Extensions;
using Gamer.Diagnostics;

namespace Gamer.Mistaken.Systems.Misc
{
    internal class CustomMaxHealthHandler : Module
    {
        public CustomMaxHealthHandler(PluginHandler p) : base(p)
        {
        }

        public override string Name => "CustomMaxHealth";
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Player.Hurting += this.Handle<Exiled.Events.EventArgs.HurtingEventArgs>((ev) => Player_Hurting(ev));
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Player.Hurting -= this.Handle<Exiled.Events.EventArgs.HurtingEventArgs>((ev) => Player_Hurting(ev));
        }

        private void Player_Hurting(Exiled.Events.EventArgs.HurtingEventArgs ev)
        {
            switch (ev.Target.Role)
            {
                case RoleType.Scp049:
                    ev.Amount *= (1700 / 1500);
                    break;
            }
        }
    }
}
