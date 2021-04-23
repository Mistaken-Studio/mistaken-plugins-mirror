using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exiled.API.Features;
using Gamer.Utilities;
using HarmonyLib;
using System.Reflection;
using Exiled.API.Enums;
using Gamer.API;

namespace Gamer.Mistaken.BetterSCP
{
    public class PluginHandler : Plugin<BSConfig>
    {
        public new static BSConfig Config;
        public override string Author => "Gamer";
        public override string Name => "Better SCP";
        public override PluginPriority Priority => PluginPriority.Highest - 3;
        public override string Prefix => "bscp";

        public const float Anty173_096DuoDistance = 20;

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
            new SCP1499.SCP1499Handler(this);

            new SCP0492.SCP0492Handler(this);

            Diagnostics.Module.OnEnable(this);

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Diagnostics.Module.OnDisable(this);
            base.OnDisabled();
        }
    }

    public class BSConfig : Config
    {
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
