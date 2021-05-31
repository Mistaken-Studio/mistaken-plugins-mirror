using Exiled.API.Features;
using Gamer.API;

namespace Xname.Radio
{
    /// <inheritdoc/>
    public class PluginHandler : Plugin<Config>
    {
        /// <inheritdoc/>
        public override string Author => "Xname";
        /// <inheritdoc/>
        public override string Name => "Radio";
        /// <inheritdoc/>
        public override string Prefix => "radio";
        /// <inheritdoc/>
        public override void OnEnabled()
        {
            new RadioHandler(this);
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
