using CommandSystem;
using Gamer.RoundLoggerSystem;
using Gamer.Utilities;
using Gamer.Utilities.RoomSystemAPI;
using System;

namespace Gamer.Mistaken.BetterSCP.SCP079.Commands
{
    /// <inheritdoc/>
    [CommandSystem.CommandHandler(typeof(CommandSystem.ClientCommandHandler))]
    public class ScanCommand : IBetterCommand
    {
        /// <inheritdoc/>
        public override string Command => "scan";
        /// <inheritdoc/>
        public override string[] Aliases => new string[] { };
        /// <inheritdoc/>
        public override string Description => "Scanning";
        /// <inheritdoc/>
        public string GetUsage()
        {
            return ".scan";
        }

        private static DateTime Lastuse = new DateTime();
        internal static float Cooldown => PluginHandler.Config.cooldownScan;
        internal static float Cost => PluginHandler.Config.apcostScan;
        internal static float ReqLvl => PluginHandler.Config.requiedlvlScan;

        internal static bool IsReady => Lastuse.AddSeconds(Cooldown).Ticks <= DateTime.Now.Ticks;
        internal static long TimeLeft => Lastuse.AddSeconds(Cooldown).Ticks - DateTime.Now.Ticks;
        /// <inheritdoc/>
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
                        string message = SCP079Handler.Translations.trans_scan;
                        message = message.Replace("{ez}", scan.EZ + "");
                        message = message.Replace("{hcz}", scan.HCZ + "");
                        message = message.Replace("{lcz}", scan.LCZ + "");
                        message = message.Replace("{nuke}", scan.NUKE + "");
                        message = message.Replace("{049}", scan.CCH049 + "");
                        message = message.Replace("{pocket}", scan.POCKET + "");
                        message = message.Replace("{surface}", scan.SURFACE + "");

                        string[] argsScan = message.Split('|');

                        for (int i = 0; i < argsScan.Length; i++)
                        {
                            player.SendConsoleMessage(argsScan[i], "red");
                        }
                        SCP079Handler.GainXP(player, Cost);
                        Lastuse = DateTime.Now;
                        //CustomAchievements.RoundEventHandler.AddProggress("Informant", player);

                        RoundLogger.Log("SCP079 EVENT", "SCAN", $"{player.PlayerToString()} requested scan");

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
