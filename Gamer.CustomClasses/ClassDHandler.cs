using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using Gamer.API.CustomClass;
using Gamer.API.CustomItem;
using Gamer.Diagnostics;
using Gamer.Mistaken.Base.GUI;
using Gamer.Mistaken.Systems.Misc;
using Gamer.RoundLoggerSystem;
using Gamer.Utilities;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Gamer.CustomClasses
{
    class ClassDHandler : Diagnostics.Module
    {
        public ClassDHandler(PluginHandler p) : base(p)
        {
        }
        public override string Name => "ClassD";

        public override void OnDisable()
        {
            Exiled.Events.Handlers.Player.ChangingRole -= this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
        }
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Player.ChangingRole += this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
        }

        private void Player_ChangingRole(ChangingRoleEventArgs ev)
        {
            if(ev.NewRole != RoleType.ClassD)
            {
                ev.Player.SetSessionVar(Main.SessionVarType.RUN_SPEED, ServerConfigSynchronizer.Singleton.HumanSprintSpeedMultiplier);
                ev.Player.SetSessionVar(Main.SessionVarType.WALK_SPEED, ServerConfigSynchronizer.Singleton.HumanWalkSpeedMultiplier);
            }
        }
    }
}
