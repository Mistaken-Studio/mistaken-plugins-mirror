using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Exiled.API.Enums;
using Exiled.API.Features;
using Gamer.API;
using Gamer.Mistaken.Systems.Patches.Vars;
using Gamer.Utilities;
using MistakenSocket.Client.SL;
using MistakenSocket.Shared;

namespace Gamer.Mistaken
{
    public class PluginHandler : Plugin<MSConfig>
    {
        public new static MSConfig Config;
        public override string Author => "Gamer";
        public override string Name => "MistakenSystemsPlugin";
        public override string Prefix => "msp";
        public override PluginPriority Priority => PluginPriority.Highest - 1;

        public static string PluginName => Instance.Name;

        public static bool IsSSLSleepMode
        {
            get
            {
                try
                {
                    return SSL.Client.IsSleeping;
                }
                catch
                {
                    return true;
                }
            }
        }

        public static PluginHandler Instance { get; private set; }
        internal static HarmonyLib.Harmony Harmony {get; private set; }
        public override void OnEnabled()
        {
            Instance = this;
            Config = base.Config;

            new Systems.Utilities.UtilitiesHandler(this);
            new Systems.Handler(this);

            new AIRS.Handler(this);
            new BetterMutes.Handler(this);
            new CustomWhitelist.Handler(this);
            new EVO.Handler(this);
            new LOFH.Handler(this);
            new Suicide.Handler(this);
            new CommandsExtender.CommandsHandler(this);
            new BetterRP.Handler(this);
            new Ranks.RanksHandler(this);
            new Ban2.BansHandler(this);

            CustomAchievements.CustomAchievements.OnEnabled(this);

            if (Config.WhitelistEnabled)
                Log.SendRaw("! Whitelist is enabled !", ConsoleColor.Red);
            else
                Log.SendRaw("Whitelist is NOT enabled", ConsoleColor.Green);

            Harmony = new HarmonyLib.Harmony("com.gamer.mistaken");
            Harmony.Patch(typeof(PlayerMovementSync).GetMethod(nameof(PlayerMovementSync.ReceiveRotation), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic), new HarmonyLib.HarmonyMethod(typeof(Systems.Patches.Fix1Patch).GetMethod(nameof(Systems.Patches.Fix1Patch.Prefix))), null, null, null);
            Harmony.Patch(typeof(PlayerMovementSync).GetMethod(nameof(PlayerMovementSync.ReceivePosition), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic), new HarmonyLib.HarmonyMethod(typeof(Systems.Patches.Fix2Patch).GetMethod(nameof(Systems.Patches.Fix2Patch.Prefix))), null, null, null);
            //harmony.Patch(typeof(Exiled.Permissions.Extensions.Permissions).GetMethod(nameof(Exiled.Permissions.Extensions.Permissions.CheckPermission), BindingFlags.Static | BindingFlags.Public), new HarmonyLib.HarmonyMethod(typeof(Systems.Patches.PermissionsPatch).GetMethod(nameof(Systems.Patches.PermissionsPatch.Prefix))), null, null, null);
            Harmony.PatchAll(Assembly.GetExecutingAssembly());
            EnableVarPatchs.Patch();

            MistakenSocket.Client.Handlers.HandlerBase.RegisterAll();

            Diagnostics.Module.OnEnable(this);
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Diagnostics.Module.OnDisable(this);
            base.OnDisabled();
        }
    }

    public class MSConfig : Config
    {
        public bool WhitelistEnabled { get; set; } = false;

        public MSP_ServerType Type { get; set; } = MSP_ServerType.NORMAL;
        public bool IsRP() => Type != MSP_ServerType.NORMAL;
        public bool IsHardRP() => Type == MSP_ServerType.HARD_RP;
    }

    public enum MSP_ServerType
    {
        NORMAL,
        RP,
        HARD_RP
    }
}
