using Exiled.API.Features;
using Gamer.API;

namespace Xname.CE
{
    /// <inheritdoc/>
    public class PluginHandler : Plugin<Config>
    {
        /// <inheritdoc/>
        public override string Author => "Xname";
        /// <inheritdoc/>
        public override string Name => "CombatExtended";
        /// <inheritdoc/>
        public override string Prefix => "ce";
        /// <inheritdoc/>
        public override void OnEnabled()
        {
            //new CEHandler(this);

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
