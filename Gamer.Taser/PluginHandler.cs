using Exiled.API.Features;
using Gamer.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gamer.Taser
{
    /// <inheritdoc/>
    public class PluginHandler : Plugin<Config>
    {
        /// <inheritdoc/>
        public override string Author => "Gamer";
        /// <inheritdoc/>
        public override string Name => "Taser";
        /// <inheritdoc/>
        public override string Prefix => "TASER";
        /// <inheritdoc/>
        public override void OnEnabled()
        {
            new TaserHandler(this);
            Diagnostics.Module.OnEnable(this);
            base.OnEnabled();
        }
        /// <inheritdoc/>
        public override void OnDisabled()
        {
            Diagnostics.Module.OnDisable(this);
            base.OnDisabled();
        }
    }
}
