using Exiled.API.Features;
using Gamer.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xname.ImpactGrenade
{
    public class PluginHandler : Plugin<Config>
    {
        public override string Author => "Xname";
        public override string Name => "ImpactGrenade";
        public override string Prefix => "IMP";

        public override void OnEnabled()
        {
            new ImpHandler(this);
            Gamer.Diagnostics.Module.OnEnable(this);
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Gamer.Diagnostics.Module.OnDisable(this);
            base.OnDisabled();
        }
    }
}
