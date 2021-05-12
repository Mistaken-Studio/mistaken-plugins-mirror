using Exiled.API.Features;
using Gamer.API;

namespace Gamer.Taser
{
    /// <inheritdoc/>
    public class PluginHandler : Plugin<TaserConfig>
    {
        /// <inheritdoc cref="Plugin{TConfig}.Config"/>
        public static new TaserConfig Config;
        /// <inheritdoc/>
        public override string Author => "Gamer";
        /// <inheritdoc/>
        public override string Name => "Taser";
        /// <inheritdoc/>
        public override string Prefix => "TASER";
        /// <inheritdoc/>
        public override void OnEnabled()
        {
            Config = base.Config;
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
    /// <inheritdoc/>
    public class TaserConfig : Config
    {
        /// <summary>
        /// On hit cooldown
        /// </summary>
        public float TaserHitCooldown = 90f;
        //public float TaserMissCooldown = 60f;
    }
}
