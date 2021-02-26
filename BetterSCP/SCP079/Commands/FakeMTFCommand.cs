using CommandSystem;
using Exiled.API.Features;
using Gamer.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Gamer.Mistaken.BetterSCP.SCP079.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.ClientCommandHandler))]
    public class FakeMTFCommand : IBetterCommand
    {
        public override string Command => "fakemtf";

        public override string[] Aliases => new string[] { };

        public override string Description => "Fake MTF";

        public string GetUsage()
        {
            return ".fakemtf";
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
            if (player.Role != RoleType.Scp079) return new string[] { "Only SCP 079" };
            if (player.Level >= ReqLvl - 1)
            {
                if (player.Energy >= Cost)
                {
                    if (IsReady)
                    {
                        int number = UnityEngine.Random.Range(0, 20);
                        char letter = RandomChar();
                        int scps = RealPlayers.List.Where(p => p.Team == Team.SCP && p.Role != RoleType.Scp0492).Count();
                        //cassie_sl MTFUNIT EPSILON 11 DESIGNATED NATO_A 10 HASENTERED ALLREMAINING AWAITINGRECONTAINMENT 10 SCPSUBJECTS
                        Cassie.Message($"MTFUNIT EPSILON 11 DESIGNATED NATO_{letter} {number} HASENTERED ALLREMAINING AWAITINGRECONTAINMENT {scps} SCPSUBJECT{(scps == 1 ? "" : "S")}");
                        SCP079Handler.GainXP(player, Cost);
                        Lastuse = DateTime.Now;
                        CustomAchievements.RoundEventHandler.AddProggress("Manipulator", player);
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

        private char RandomChar()
        {
            string alpha = "ABCDEFGHIJKLMNOPQRSTWYXZ";
            return alpha[UnityEngine.Random.Range(0, alpha.Length)];
        }
    }
}