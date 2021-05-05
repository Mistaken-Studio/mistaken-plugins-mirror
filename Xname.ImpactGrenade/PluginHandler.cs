using Exiled.API.Features;
using Gamer.API;

namespace Xname.ImpactGrenade
{
    /// <inheritdoc/>
    public class PluginHandler : Plugin<Config>
    {
        /// <inheritdoc/>
        public override string Author => "Xname";
        /// <inheritdoc/>
        public override string Name => "ImpactGrenade";
        /// <inheritdoc/>
        public override string Prefix => "IMP";
        /// <inheritdoc/>
        public override void OnEnabled()
        {
            new ImpHandler(this);
            new FlashImpHandler(this);
            new BallFix(this);
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
