using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Exiled.API.Enums;
using Exiled.API.Features;
using Gamer.API;
using Gamer.Mistaken.Systems.Patches.Vars;
using Gamer.Utilities;

namespace Gamer.CustomClasses
{
    public class PluginHandler : Plugin<Config>
    {
        public override string Author => "Gamer";
        public override string Name => "CustomClasses";
        public override string Prefix => "cc";
        public static string PluginName => Instance.Name;

        public static PluginHandler Instance { get; private set; }
        public override void OnEnabled()
        {
            new GuardCommanderHandler(this);

            Diagnostics.Module.OnEnable(this);
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Diagnostics.Module.OnDisable(this);
            base.OnDisabled();
        }
    }
}
