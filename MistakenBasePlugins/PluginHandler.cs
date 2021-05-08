using Exiled.API.Enums;
using Exiled.API.Features;
using Gamer.API;

namespace Gamer.Mistaken.Base
{
    /// <inheritdoc/>
    public class PluginHandler : Plugin<ConfigPlus>
    {
        /// <summary>
        /// Config
        /// </summary>
        public static new ConfigPlus Config;
        /// <inheritdoc/>
        public override PluginPriority Priority => PluginPriority.Highest - 2;
        /// <inheritdoc/>
        public override string Name => "Mistaken Base";
        /// <inheritdoc/>
        public override string Author => "Gamer";
        /// <inheritdoc/>
        public override string Prefix => "mbp";
        /// <inheritdoc/>
        public override void OnEnabled()
        {
            PluginHandler.Config = base.Config as ConfigPlus;
            new CustomInfoHandler(this);
            new GUI.PseudoGUIHandler(this);
            new Staff.StaffHandler(this);
            new CustomItems.CustomItemsHandler(this);
            new ExperimentalHandler(this);

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
    public class ConfigPlus : Config
    {
        /// <summary>
        /// Is Experimental Server
        /// </summary>
        public bool IsExperimentalServer { get; set; } = false;
        /// <summary>
        /// Is Public Test Beta Server
        /// </summary>
        public bool IsPTBServer { get; set; } = false;
    }
}