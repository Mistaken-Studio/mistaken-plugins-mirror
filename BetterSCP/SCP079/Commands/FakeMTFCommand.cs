using CommandSystem;
using Exiled.API.Features;
using Gamer.RoundLoggerSystem;
using Gamer.Utilities;
using System;
using System.Linq;

namespace Gamer.Mistaken.BetterSCP.SCP079.Commands
{
    /// <inheritdoc/>
    [CommandSystem.CommandHandler(typeof(CommandSystem.ClientCommandHandler))]
    public class FakeMTFCommand : IBetterCommand
    {
        /// <inheritdoc/>
        public override string Command => "fakemtf";
        /// <inheritdoc/>
        public override string[] Aliases => new string[] { };
        /// <inheritdoc/>
        public override string Description => "Fake MTF";
        /// <inheritdoc/>
        public string GetUsage()
        {
            return ".fakemtf";
        }

        private static DateTime Lastuse = new DateTime();
        internal static float Cooldown => PluginHandler.Config.cooldown;
        internal static float Cost => PluginHandler.Config.apcost;
        internal static float ReqLvl => PluginHandler.Config.requiedlvl;

        internal static bool IsReady => Lastuse.AddSeconds(Cooldown).Ticks <= DateTime.Now.Ticks;
        internal static long TimeLeft => Lastuse.AddSeconds(Cooldown).Ticks - DateTime.Now.Ticks;
        /// <inheritdoc/>
        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            var player = sender.GetPlayer();
            success = false;
            if (player.Role != RoleType.Scp079) return new string[] { "Only SCP 079" };
            if (Systems.Patches.SCP079RecontainInfoPatch.Recontaining) return new string[] { "Systems overloaded" };
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
                        //CustomAchievements.RoundEventHandler.AddProggress("Manipulator", player);

                        RoundLogger.Log("SCP079 EVENT", "FAKEMTF", $"{player.PlayerToString()} requested fakemtf");

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