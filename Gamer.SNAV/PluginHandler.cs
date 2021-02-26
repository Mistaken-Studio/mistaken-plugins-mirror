using Exiled.API.Features;
using Gamer.API;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gamer.SNAV
{
    public class PluginHandler : Plugin<SNAVConfig>
    {
        public new static SNAVConfig Config;
        public override string Author => "Gamer";
        public override string Name => "SNAV-Plugin";
        public override string Prefix => "SNAV";

        public override void OnEnabled()
        {
            Config = base.Config;

            new SNavHandler(this);
            Diagnostics.Module.OnEnable(this);
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Diagnostics.Module.OnDisable(this);
            base.OnDisabled();
        }
    }

    public class SNAVConfig : Config
    {
        public string[] SNav3000Spawns { get; set; } = new string[]
        {
            "Scp079Second, 4, 2, 8.4",
            "NukeSurface, 5.5, 2, -3",
            "NukeSurface, 6.5, 2, -3",
            "HczArmory, -2, 2, -2",
            "LczArmory, 6, 2, -1",
            "LczArmory, 6, 2, 1.5",
        };
        public string[] SNavUltimateSpawns { get; set; } = new string[]
        {
            "Scp079Second, 4.5, 2, 8.4",
        };
    }
}
