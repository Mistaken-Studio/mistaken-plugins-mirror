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
            Exiled.Events.Handlers.Player.ChangedRole -= this.Handle<Exiled.Events.EventArgs.ChangedRoleEventArgs>((ev) => Player_ChangedRole(ev));
        }
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Player.ChangedRole += this.Handle<Exiled.Events.EventArgs.ChangedRoleEventArgs>((ev) => Player_ChangedRole(ev));
        }

        private void Player_ChangedRole(ChangedRoleEventArgs ev)
        {
            if(ev.Player.Role != RoleType.ClassD)
            {
                ev.Player.SetSessionVar(Main.SessionVarType.RUN_SPEED, ServerConfigSynchronizer.Singleton.HumanSprintSpeedMultiplier);
                ev.Player.SetSessionVar(Main.SessionVarType.WALK_SPEED, ServerConfigSynchronizer.Singleton.HumanWalkSpeedMultiplier);
            }
            else
            {
                int rand = UnityEngine.Random.Range(-10, 10);
                Log.Debug(rand);
                ev.Player.MaxHealth -= rand * 2;
                ev.Player.Health = ev.Player.MaxHealth;
                ev.Player.SetSessionVar(Main.SessionVarType.RUN_SPEED, ServerConfigSynchronizer.Singleton.HumanSprintSpeedMultiplier * (1 + rand));
                ev.Player.SetSessionVar(Main.SessionVarType.WALK_SPEED, ServerConfigSynchronizer.Singleton.HumanWalkSpeedMultiplier * (1 + rand));
            }
        }
    }
}
