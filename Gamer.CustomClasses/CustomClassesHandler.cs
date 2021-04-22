using System;
using System.Linq;
using System.Net;
using System.Text;
using Exiled.API.Features;
using Gamer.Utilities;
using System.Threading;
using System.Threading.Tasks;
using Gamer.Diagnostics;
using System.Collections.Generic;
using Exiled.API.Enums;
using Gamer.Mistaken.Systems.Misc;
using Gamer.Mistaken.Systems;
using Gamer.RoundLoggerSystem;
using UnityEngine;
using Exiled.API.Extensions;

namespace Gamer.CustomClasses
{
    public class CustomClassesHandler : Module
    {
        public CustomClassesHandler(PluginHandler plugin) : base(plugin)
        {
        }

        public override string Name => "CustomClasses";
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Player.Died += this.Handle<Exiled.Events.EventArgs.DiedEventArgs>((ev) => Player_Died(ev));
            Exiled.Events.Handlers.Player.ChangingRole += this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Player.Died -= this.Handle<Exiled.Events.EventArgs.DiedEventArgs>((ev) => Player_Died(ev));
            Exiled.Events.Handlers.Player.ChangingRole -= this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
        }

        
        private void Player_Died(Exiled.Events.EventArgs.DiedEventArgs ev)
        {
            foreach (var item in API.CustomClass.CustomClass.CustomClasses)
            {
                if (item.PlayingAsClass.Contains(ev.Target))
                {
                    item.OnDie(ev.Target);
                    break;
                }
            }
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            foreach (var item in API.CustomClass.CustomClass.CustomClasses)
            {
                if (ev.NewRole == item.Role)
                    continue;
                if (item.PlayingAsClass.Contains(ev.Player))
                {
                    item.OnDie(ev.Player);
                    break;
                }
            }
        }
    }
}
