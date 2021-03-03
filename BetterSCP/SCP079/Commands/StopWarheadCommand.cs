using CommandSystem;
using Exiled.API.Features;
using Gamer.RoundLoggerSystem;
using Gamer.Utilities;
using Gamer.Utilities.RoomSystemAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Gamer.Mistaken.BetterSCP.SCP079.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.ClientCommandHandler))]
    public class StopWarheadCommand : IBetterCommand
    {
        public override string Command => "stop";

        public override string[] Aliases => new string[] { };

        public override string Description => "Stop Warhead";

        public string GetUsage()
        {
            return ".stop";
        }

        private static DateTime Lastuse = new DateTime();
        public static float Cooldown => PluginHandler.Config.cooldownStopWarhead;
        public static float Cost => PluginHandler.Config.apcostStopWarhead;
        public static float ReqLvl => PluginHandler.Config.requiedlvlStopWarhead;

        public static bool IsReady => Lastuse.AddSeconds(Cooldown).Ticks <= DateTime.Now.Ticks;
        public static long TimeLeft => Lastuse.AddSeconds(Cooldown).Ticks - DateTime.Now.Ticks;

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

                        RoundLogger.Log("SCP079", "STOPWARHEAD", $"{player.PlayerToString()} requested stopwarhead");

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