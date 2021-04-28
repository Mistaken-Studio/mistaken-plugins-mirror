using Exiled.API.Features;
using Gamer.API;

namespace Xname.ColorfullEZ
{
    /// <inheritdoc/>
    public class PluginHandler : Plugin<Config>
    {
        /// <inheritdoc/>
        public override string Author => "Xname";
        /// <inheritdoc/>
        public override string Name => "ColorfullEZ";
        /// <inheritdoc/>
        public override string Prefix => "color_ez";
        /// <inheritdoc/>
        public override void OnEnabled()
        {
            new ColorfullEZHandler(this);

            Gamer.Diagnostics.Module.OnEnable(this);
            base.OnEnabled();
        }
        /// <inheritdoc/>
        public override void OnDisabled()
        {
            Gamer.Diagnostics.Module.OnDisable(this);
            base.OnDisabled();
        }
    }
}
