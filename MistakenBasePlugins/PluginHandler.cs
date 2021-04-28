using System;
using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using Gamer.API;

namespace Gamer.Mistaken.Base
{
    public class PluginHandler : Plugin<MBPConfig>
    {
        private static PluginHandler Instance;
        internal new static MBPConfig Config => ((Plugin<MBPConfig>)Instance).Config;
        internal static bool EventOnGoing = false;
        public override PluginPriority Priority => PluginPriority.Highest - 2;
        public override string Name => "Mistaken Base";
        public override string Author => "Gamer";
        public override string Prefix => "mbp";
        public override void OnEnabled()
        {
            Instance = this;
            
            Diagnostics.Module.OnEnable(this);
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Diagnostics.Module.OnDisable(this);
            base.OnDisabled();
        }
    }

    public class MBPConfig : Config
    {
        public bool IsRankingEnabled { get; set; } = true;

        public ServerType RankingType { get; set; } = ServerType.RANKED;
    }

    public enum ServerType
    {
        RANKED,
        RP,
        CASUAL
    }
}