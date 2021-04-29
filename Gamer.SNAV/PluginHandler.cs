using Exiled.API.Features;
using Gamer.API;

namespace Gamer.SNAV
{
    /// <inheritdoc/>
    public class PluginHandler : Plugin<SNAVConfig>
    {
        /// <summary>
        /// Config
        /// </summary>
        public static new SNAVConfig Config;
        /// <inheritdoc/>
        public override string Author => "Gamer";
        /// <inheritdoc/>
        public override string Name => "SNAV-Plugin";
        /// <inheritdoc/>
        public override string Prefix => "SNAV";
        /// <inheritdoc/>
        public override void OnEnabled()
        {
            Config = base.Config;

            new SNavHandler(this);
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
    public class SNAVConfig : Config
    {
        /// <summary>
        /// SNav 3000 Spawns
        /// </summary>
        public string[] SNav3000Spawns { get; set; } = new string[]
        {
            "Scp079Second, 4, 2, 8.4",
            "NukeSurface, 5.5, 2, -3",
            "NukeSurface, 6.5, 2, -3",
            "HczArmory, -2, 2, -2",
            "LczArmory, 6, 2, -1",
            "LczArmory, 6, 2, 1.5",
        };
        /// <summary>
        /// SNav Ultimate Spawns
        /// </summary>
        public string[] SNavUltimateSpawns { get; set; } = new string[]
        {
            "Scp079Second, 4.5, 2, 8.4",
        };
    }
}
