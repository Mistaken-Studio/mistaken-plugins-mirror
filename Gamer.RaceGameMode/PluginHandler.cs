using Exiled.API.Features;
using Exiled.API.Interfaces;
using Gamer.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gamer.RaceGameMode
{
    public class PluginHandler : Plugin<Config>
    {
        public override string Author => "Gamer";
        public override string Name => "RaceGameMode";
        public override void OnDisabled()
        {
            base.OnDisabled();
        }
        public override void OnEnabled()
        {
            new RaceModule(this);

            Diagnostics.Module.OnEnable(this);
            base.OnEnabled();
        }
    }

    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = false;
    }
}
