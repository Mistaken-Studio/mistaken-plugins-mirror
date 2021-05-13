using Exiled.API.Enums;
using Exiled.API.Features;
using Gamer.API;

namespace Gamer.CustomClasses
{
    /// <inheritdoc/>
    public class PluginHandler : Plugin<Config>
    {
        /// <inheritdoc/>
        public override string Author => "Gamer";
        /// <inheritdoc/>
        public override string Name => "CustomClasses";
        /// <inheritdoc/>
        public override string Prefix => "cc";
        /// <inheritdoc/>
        public override PluginPriority Priority => PluginPriority.Highest;
        /// <inheritdoc/>
        public static string PluginName => Instance.Name;
        /// <summary>
        /// PluginHandler Instance
        /// </summary>
        public static PluginHandler Instance { get; private set; }
        /// <inheritdoc/>
        public override void OnEnabled()
        {
            new GuardCommanderHandler(this);
            new ZoneManagerHandler(this);
            new DeputyFacalityManagerHandler(this);
            new TAU5Handler(this);
            new CustomClassesHandler(this);

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
