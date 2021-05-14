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
    public class FakeSCPCommand : IBetterCommand
    {
        /// <inheritdoc/>
        public override string Command => "fakescp";
        /// <inheritdoc/>
        public override string[] Aliases => new string[] { };
        /// <inheritdoc/>
        public override string Description => "Fake SCP";
        /// <inheritdoc/>
        public string GetUsage()
        {
            return ".fakescp [powód(liczba)] [scp]";
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
                        if (args.Length == 0 || !int.TryParse(args[0], out int reason) || reason < 1 || reason > 5)
                        {
                            return new string[] 
                            { 
                                ".fakescp [przyczyna] [scp]",
                                "Podaj przyczynę śmierci:", 
                                "1. Tesla", 
                                "2. CI", 
                                "3. Klasa D", 
                                "4. Nieznany", 
                                "5. Zrekontaminowany (Działa tylko na SCP-106)", 
                                "6. Śmierć w Dekontaminacji LCZ" 
                            };
                        }
                        args = args.Skip(1).ToArray();
                        if (args.Length == 0)
                        {
                            int max = short.MaxValue;
                            return new string[] { SCP079Handler.Translations.trans_failed_nonumber.Replace("${max}", max.ToString()) };
                        }
                        string processed = "";
                        foreach (char item in string.Join("", args).ToCharArray())
                        {
                            if (item == ' ') continue;
                            processed += item;
                        }
                        if (processed.Length <= 5)
                        {
                            if (!short.TryParse(processed, out _))
                                return new string[] { SCP079Handler.Translations.trans_failed_wrongnumber.Replace("${max}", short.MaxValue.ToString()) };
                            string processedtonumber = " ";
                            foreach (char item in processed.ToCharArray())
                            {
                                if (item != ' ')
                                {
                                    try
                                    {
                                        if (short.TryParse(item.ToString(), out short num)) processedtonumber += num + " ";
                                    }
                                    catch { }
                                }
                            }
                            switch (reason)
                            {
                                case 1:
                                    Cassie.Message("SCP " + processedtonumber + " SUCCESSFULLY TERMINATED BY AUTOMATIC SECURITY SYSTEM");
                                    break;
                                case 2:
                                    Cassie.Message("SCP " + processedtonumber + " SUCCESSFULLY TERMINATED BY CHAOSINSURGENCY");
                                    break;
                                case 3:
                                    Cassie.Message("SCP " + processedtonumber + " CONTAINEDSUCCESSFULLY BY CLASSD PERSONNEL");
                                    break;
                                case 4:
                                    Cassie.Message("SCP " + processedtonumber + " SUCCESSFULLY TERMINATED . TERMINATION CAUSE UNSPECIFIED");
                                    break;
                                case 5:
                                    Cassie.Message("SCP 1 0 6 RECONTAINED SUCCESSFULLY");
                                    break;
                                case 6:
                                    Cassie.Message("SCP " + processedtonumber + " LOST IN DECONTAMINATION SEQUENCE");
                                    break;
                            }

                            SCP079Handler.GainXP(player, Cost);
                            Lastuse = DateTime.Now;
                            //CustomAchievements.RoundEventHandler.AddProggress("SneakyFox", player);

                            RoundLogger.Log("SCP079 EVENT", "FAKESCP", $"{player.PlayerToString()} requested fakescp of SCP {processedtonumber} with reason: {reason}");

                            success = true;
                            return new string[] { SCP079Handler.Translations.trans_success };
                        }
                        else
                        {
                            int max = short.MaxValue;
                            return new string[] { SCP079Handler.Translations.trans_failed_wrongnumber.Replace("${max}", max.ToString()) };
                        }
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
