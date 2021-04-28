using CommandSystem;
using Gamer.RoundLoggerSystem;
using Gamer.Utilities;
using System;

namespace Gamer.Mistaken.BetterSCP.SCP079.Commands
{
    /// <inheritdoc/>
    [CommandSystem.CommandHandler(typeof(CommandSystem.ClientCommandHandler))]
    public class BlackoutCommand : IBetterCommand
    {
        /// <inheritdoc/>
        public override string Command => "blackout";
        /// <inheritdoc/>
        public override string[] Aliases => new string[] { };
        /// <inheritdoc/>
        public override string Description => "Do blackout";
        /// <inheritdoc/>
        public string GetUsage()
        {
            return ".blackout [duration]";
        }

        internal static DateTime Lastuse = new DateTime();
        internal static float Cooldown => PluginHandler.Config.cooldownBlackout;
        internal static float Cost => PluginHandler.Config.apcostBlackout;
        internal static float ReqLvl => PluginHandler.Config.requiedlvlBlackout;

        internal static bool IsReady => Lastuse.Ticks <= DateTime.Now.Ticks;
        internal static long TimeLeft => Lastuse.Ticks - DateTime.Now.Ticks;

        private float LastCooldown;
        /// <inheritdoc/>
        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            var player = sender.GetPlayer();
            success = false;
            if (player.Role != RoleType.Scp079) return new string[] { "Only SCP 079" };
            if (player.Level >= ReqLvl - 1)
            {
                if (IsReady)
                {
                    if (args.Length == 0)
                        return new string[] { "Usage: " + GetUsage() };
                    else
                    {
                        if (float.TryParse(args[0], out float duration))
                        {
                            var ToDrain = duration * Cost;
                            float Cooldown = duration * BlackoutCommand.Cooldown;

                            if (player.Energy >= ToDrain)
                            {
                                Generator079.mainGenerator.ServerOvercharge(duration, false);

                                SCP079Handler.GainXP(player, ToDrain);
                                Lastuse = DateTime.Now.AddSeconds(Cooldown);
                                LastCooldown = Cooldown;
                                //CustomAchievements.RoundEventHandler.AddProggress("LightsOut", player);
                                //if(ToDrain == 200) 
                                //    CustomAchievements.RoundEventHandler.AddProggress("Darkness", player);
                                RoundLogger.Log("SCP079 EVENT", "BLACKOUT", $"{player.PlayerToString()} requested blackout for {duration}s");
                                success = true;
                                return new string[] { SCP079Handler.Translations.trans_success };
                            }
                            else
                            {
                                return new string[] { SCP079Handler.Translations.trans_failed_ap.Replace("${ap}", ToDrain.ToString()) };
                            }
                        }
                        else
                        {
                            float max = float.MaxValue;
                            string toreturn = SCP079Handler.Translations.trans_failed_nonumber_blackout.Replace("${max}", max.ToString());
                            return new string[] { toreturn };
                        }
                    }
                }
                else
                {
                    return new string[] { SCP079Handler.Translations.trans_failed_cooldown_blackout.Replace("${time}", LastCooldown.ToString()).Replace("${leftS}", (Lastuse - DateTime.Now).TotalSeconds.ToString()) };
                }
            }
            else
            {
                return new string[] { SCP079Handler.Translations.trans_failed_lvl.Replace("${lvl}", ReqLvl.ToString()) };
            }
        }
    }
}
