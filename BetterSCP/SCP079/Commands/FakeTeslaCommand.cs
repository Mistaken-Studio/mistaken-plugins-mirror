using CommandSystem;
using Exiled.API.Features;
using Gamer.RoundLoggerSystem;
using Gamer.Utilities;
using Respawning.NamingRules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Gamer.Mistaken.BetterSCP.SCP079.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.ClientCommandHandler))]
    public class FakeTeslaCommand : IBetterCommand
    {
        public override string Command => "faketesla";

        public override string[] Aliases => new string[] { };

        public override string Description => "Fake Tesla";

        public string GetUsage()
        {
            return ".faketesla";
        }

        private static DateTime Lastuse = new DateTime();
        public static float Cooldown => PluginHandler.Config.cooldown;
        public static float Cost => PluginHandler.Config.apcost;
        public static float ReqLvl => PluginHandler.Config.requiedlvl;

        public static bool IsReady => Lastuse.AddSeconds(Cooldown).Ticks <= DateTime.Now.Ticks;
        public static long TimeLeft => Lastuse.AddSeconds(Cooldown).Ticks - DateTime.Now.Ticks;

        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            var player = sender.GetPlayer();
            success = false;
            if (player.Role != RoleType.Scp079) 
                return new string[] { "Only SCP 079" };
            if (player.Level >= ReqLvl - 1)
            {
                if (player.Energy >= Cost)
                {
                    if (IsReady)
                    {
                        Cassie.Message("Tesla gates deactivated by order of NINETAILEDFOX COMMANDER");
                        SCP079Handler.GainXP(player, Cost);
                        Lastuse = DateTime.Now;
                        //CustomAchievements.RoundEventHandler.AddProggress("Manipulator", player);

                        RoundLogger.Log("SCP079 EVENT", "FAKETESLA", $"{player.PlayerToString()} requested faketesla");

                        success = true;
                        return new string[] { SCP079Handler.Translations.trans_success };
                    }
                    else
                        return new string[] { SCP079Handler.Translations.trans_failed_cooldown.Replace("${time}", Cooldown.ToString()) };
                }
                else
                    return new string[] { SCP079Handler.Translations.trans_failed_ap.Replace("${ap}", Cost.ToString()) };
            }
            else
                return new string[] { SCP079Handler.Translations.trans_failed_lvl.Replace("${lvl}", ReqLvl.ToString()) };
        }
    }
}