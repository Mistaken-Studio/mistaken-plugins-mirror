using Exiled.API.Enums;
using Exiled.API.Features;
using Gamer.API;

namespace Gamer.Mistaken.BetterSCP
{
    /// <inheritdoc/>
    public class PluginHandler : Plugin<BSConfig>
    {
        /// <summary>
        /// Config
        /// </summary>
        public static new BSConfig Config;
        /// <inheritdoc/>
        public override string Author => "Gamer";
        /// <inheritdoc/>
        public override string Name => "Better SCP";
        /// <inheritdoc/>
        public override PluginPriority Priority => PluginPriority.Highest - 3;
        /// <inheritdoc/>
        public override string Prefix => "bscp";

        /// <summary>
        /// AntyDuo Trigger distance
        /// </summary>
        public const float Anty173_096DuoDistance = 20;
        /// <inheritdoc/>
        public override void OnEnabled()
        {
            Config = base.Config;
            new Global.GlobalHandler(this);
            new SCP106.SCP106Handler(this);
            new SCP079.SCP079Handler(this);
            new SCP049.SCP049Handler(this);
            new SCP173.SCP173Handler(this);
            new SCP096.SCP096Handler(this);
            new SCP914.SCP914Handler(this);
            new Pocket.PocketHandler(this);
            new SCP939.SCP939Handler(this);
            new SCP012.SCP012Handler(this);
            new SCP1499.SCP1499Handler(this);

            new SCP0492.SCP0492Handler(this);

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
    public class BSConfig : Config
    {
#pragma warning disable CS1591
#pragma warning disable IDE1006
        #region SCP079
        public int requiedlvl { get; set; } = 3;
        public int apcost { get; set; } = 100;
        public int cooldown { get; set; } = 180;

        public int requiedlvlScan { get; set; } = 2;
        public int apcostScan { get; set; } = 100;
        public int cooldownScan { get; set; } = 60;

        public int requiedlvlBlackout { get; set; } = 2;
        public int apcostBlackout { get; set; } = 10;
        public int cooldownBlackout { get; set; } = 5;

        public int requiedlvlStopWarhead { get; set; } = 5;
        public int apcostStopWarhead { get; set; } = 200;
        public int cooldownStopWarhead { get; set; } = 600;

        public int requiedlvlCassie { get; set; } = 5;
        public int apcostCassie { get; set; } = 200;
        public int cooldownCassie { get; set; } = 300;
        #endregion
#pragma warning restore IDE1006
    }
}
