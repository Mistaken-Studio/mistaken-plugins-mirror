using CommandSystem;
using Exiled.API.Features;
using Gamer.RoundLoggerSystem;
using Gamer.Utilities;
using System;

namespace Gamer.Mistaken.BetterSCP.SCP079.Commands
{
    /// <inheritdoc/>
    [CommandSystem.CommandHandler(typeof(CommandSystem.ClientCommandHandler))]
    public class StopWarheadCommand : IBetterCommand
    {
        /// <inheritdoc/>
        public override string Command => "stop";
        /// <inheritdoc/>
        public override string[] Aliases => new string[] { "warheadstop", "stopwarhead", "stopwh", "swarhead" };
        /// <inheritdoc/>
        public override string Description => "Stop Warhead";
        /// <inheritdoc/>
        public string GetUsage()
        {
            return ".stop";
        }

        private static DateTime Lastuse = new DateTime();
        internal static float Cooldown => PluginHandler.Config.cooldownStopWarhead;
        internal static float Cost => PluginHandler.Config.apcostStopWarhead;
        internal static float ReqLvl => PluginHandler.Config.requiedlvlStopWarhead;

        internal static bool IsReady => Lastuse.AddSeconds(Cooldown).Ticks <= DateTime.Now.Ticks;
        internal static long TimeLeft => Lastuse.AddSeconds(Cooldown).Ticks - DateTime.Now.Ticks;
        /// <inheritdoc/>
        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            var player = sender.GetPlayer();
            success = false;
            if (player.Role != RoleType.Scp079) return new string[] { "Only SCP 079" };
            if (!Warhead.IsInProgress) return new string[] { "Warhead is not detonating" };
            if (Warhead.IsLocked || Systems.Misc.BetterWarheadHandler.Warhead.StopLock) return new string[] { "Warhead is locked" };
            if (player.Level >= ReqLvl - 1)
            {
                if (player.Energy >= Cost)
                {
                    if (IsReady)
                    {
                        Warhead.Stop();
                        Warhead.LeverStatus = false;
                        Cassie.Message("PITCH_0.8 You jam_070_3 will jam_050_5 .g5 no jam_040_9 detonate me", false, false);
                        SCP079Handler.GainXP(player, Cost);
                        Lastuse = DateTime.Now;

                        RoundLogger.Log("SCP079 EVENT", "STOPWARHEAD", $"{player.PlayerToString()} requested stopwarhead");

                        success = true;
                        return new string[] { SCP079Handler.Translations.trans_success };
                    }
                    else
                    {
                        return new string[] { SCP079Handler.Translations.trans_failed_cooldown.Replace("${time}", Cooldown.ToString()) };
                    }
                }
                else
                {
                    return new string[] { SCP079Handler.Translations.trans_failed_ap.Replace("${ap}", Cost.ToString()) };
                }
            }
            else
            {
                return new string[] { SCP079Handler.Translations.trans_failed_lvl.Replace("${lvl}", ReqLvl.ToString()) };
            }
        }
    }
}