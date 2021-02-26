using CommandSystem;
using Exiled.API.Features;
using Gamer.Utilities;
using Gamer.Utilities.RoomSystemAPI;
using System;
using UnityEngine;

namespace Gamer.Mistaken.BetterSCP.SCP079.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.ClientCommandHandler))]
    public class FullScanCommand : IBetterCommand
    {
        public override string Command => "fullscan";

        public override string[] Aliases => new string[] { };

        public override string Description => "Full Scanning";

        public string GetUsage()
        {
            return ".fullscan";
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
                        ScanOutput scan = new ScanOutput();
                        string message = "Full facility scan initiated";
                        if (scan.SURFACE != 0)
                            message += string.Format(" . {0} SURFACE", scan.SURFACE);
                        if (scan.EZ != 0)
                            message += string.Format(" . {0} Entrance Zone", scan.EZ);
                        if (scan.HCZ != 0)
                            message += string.Format(" . {0} Heavy containment zone", scan.HCZ);
                        if (scan.LCZ != 0)
                            message += string.Format(" . {0} Light containment zone", scan.LCZ);
                        if (scan.NUKE != 0)
                            message += string.Format(" . {0} Alpha Warhead", scan.NUKE);
                        if (scan.CCH049 != 0)
                            message += string.Format(" . {0} SCP 0 4 9 containment chamber", scan.CCH049);
                        if (scan.POCKET != 0)
                            message += string.Format(" . {0} Unknown", scan.POCKET);
                        if (message != "Full facility scan initiated")
                            Cassie.Message(message);
                        else
                            Cassie.Message("DETECTED UNKNOWN SECURITY SYSTEM ERROR . FAILED TO SCAN");

                        SCP079Handler.GainXP(player, Cost);
                        Lastuse = DateTime.Now;
                        CustomAchievements.RoundEventHandler.AddProggress("Informant", player);
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