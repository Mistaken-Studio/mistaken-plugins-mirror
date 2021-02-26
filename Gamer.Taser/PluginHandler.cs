using Exiled.API.Features;
using Gamer.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gamer.Taser
{
    public class PluginHandler : Plugin<Config>
    {
        public override string Author => "Gamer";
        public override string Name => "Taser";
        public override string Prefix => "TASER";

        public override void OnEnabled()
        {
            new TaserHandler(this);
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
