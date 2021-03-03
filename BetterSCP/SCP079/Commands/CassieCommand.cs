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
    public class CassieCommand : IBetterCommand
    {
        public override string Command => "cassie";

        public override string[] Aliases => new string[] { };

        public override string Description => "Stop Warhead";

        public string GetUsage()
        {
            return ".cassie [MESSAGE]";
        }

        private static DateTime Lastuse = new DateTime();
        public static float Cooldown => PluginHandler.Config.cooldownCassie;
        public static float Cost => PluginHandler.Config.apcostCassie;
        public static float ReqLvl => PluginHandler.Config.requiedlvlCassie;

        public static bool IsReady => Lastuse.AddSeconds(Cooldown).Ticks <= DateTime.Now.Ticks;
        public static long TimeLeft => Lastuse.AddSeconds(Cooldown).Ticks - DateTime.Now.Ticks;

        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            var player = sender.GetPlayer();
            success = false;
            if (player.Role != RoleType.Scp079) return new string[] { "Only SCP 079" };
            if (player.Level >= ReqLvl - 1)
            {
                if (player.Energy >= Cost)
                {
                    if (IsReady)
                    {
                        string message = string.Join(" ", args);
                        if (args.Length > 20 || message.Length > 250)
                            return new string[] { "Wiadomość nie może być dłuższa niż 20 słów i max 250 znaków licząc spację" };
                        message = message.ToLower();
                        while (message.Contains("jam_"))
                            message = message.Replace("jam_", "");
                        while(message.Contains(".g"))
                            message = message.Replace(".g", "");
                        while (message.Contains("yield_"))
                            message = message.Replace("yield_", "");
                        while (message.Contains("pitch_")) ;
                            message = message.Replace("pitch_", "");
                        Cassie.Message("PITCH_0.9 SCP 0 PITCH_0.9 7 PITCH_0.9 9 PITCH_0.9 jam_050_5 OVERRIDE PITCH_1 . . . " + message);
                        SCP079Handler.GainXP(player, Cost);
                        Lastuse = DateTime.Now;
                        RoundLogger.Log("SCP079", "CASSIE", $"{player.PlayerToString()} requested cassie \"{message}\"");

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