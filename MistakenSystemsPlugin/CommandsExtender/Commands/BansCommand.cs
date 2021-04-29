using CommandSystem;
using Exiled.API.Features;
using Gamer.Utilities;
using System;
using System.Linq;


namespace Gamer.Mistaken.CommandsExtender.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))]
    internal class BansCommand : IBetterCommand, IPermissionLocked
    {
        public string GetUsage()
        {
            return "Bans [Id]";
        }

        private static Player Admin;
        private static Player player;

        public string Permission => "bans";

        public string PluginName => PluginHandler.PluginName;

        public override string Command => "bans";

        public override string[] Aliases => new string[] { };

        public static void GetBans(string userId)
        {
            var bans = Systems.Bans.BansManager.GetBans(userId);

            string message = "<color=blue>Bans output:</color>";
            message += $"\n<color=red></color>";
            message += $"\n<color=red>Nickname: {player.Nickname} ({player.UserId})</color>";
            message += $"\n<color=red>Bans: {bans.Length}</color>";
            message += $"\n<color=red></color>";
            int i = 1;
            foreach (var (AdminId, Reason, Duration, Time) in bans)
            {
                int dur = Duration;
                string type = "s";
                string fulldur = "KICK";
                if (dur != 0)
                {
                    if (dur % 60 == 0)
                    {
                        dur /= 60;
                        type = "m";
                        if (dur % 60 == 0)
                        {
                            dur /= 60;
                            type = "h";
                            if (dur % 24 == 0)
                            {
                                dur /= 24;
                                type = "d";
                                if (dur % 7 == 0)
                                {
                                    dur /= 7;
                                    type = "w";
                                }
                                if (dur % 30 == 0)
                                {
                                    dur /= 30;
                                    type = "mo";
                                    if (dur % 12 == 0)
                                    {
                                        dur /= 12;
                                        type = "y";
                                    }
                                }
                            }
                        }
                    }
                    fulldur = dur + type;
                }

                string adminUid = AdminId;
                if (!adminUid.Contains("@"))
                    adminUid += "@steam";
                if (adminUid == Admin.UserId)
                    adminUid = "You";
                if (adminUid == "0@steam")
                {
                    adminUid = "Console";
                    if (Reason.StartsWith("TK:"))
                        adminUid = "Anty Team Kill System";
                    if (Reason.StartsWith("R:"))
                        adminUid = "Remote Ban System";
                    if (Reason.StartsWith("W:"))
                        adminUid = "Wanted System";
                    if (Reason.StartsWith("AutoBan:"))
                        adminUid = "AutoBan System";
                }
                if (adminUid == "@steam")
                    adminUid = "Unknown";
                var admin = Base.Staff.StaffHandler.Staff.FirstOrDefault(_item => _item.steamid + "@steam" == adminUid || _item.discordid + "@discord" == adminUid);
                if (admin != null)
                    adminUid = $"{admin.nick} ({adminUid})";
                message += $"\n<color=blue>#{i}|{adminUid}|<color=red>{fulldur}</color>|{Time}|Reason:</color>";
                message += $"\n<color=green>{Reason.Trim()}</color>";
                i++;
            }

            Admin.SendConsoleMessage(message, "green");
        }

        public static void AnalizeBans(string userId, Systems.Bans.BansAnalizer.BanCategory category)
        {
            var bans = Systems.Bans.BansManager.GetBans(userId);
            var result = Systems.Bans.BansAnalizer.GuessBanDuration(bans, category);

            Admin.SendConsoleMessage($"- {category}: {result} => {Systems.Bans.BansAnalizer.GetDurationCategory(result)}", "green");
        }

        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            success = false;
            if (args.Length == 0) return new string[] { GetUsage() };
            Admin = sender.GetPlayer();
            if (int.TryParse(args[0], out int pid))
            {
                player = RealPlayers.Get(pid);

                if (player != null)
                {
                    GetBans(player.UserId);

                    Admin.SendConsoleMessage($"Next ban suggestions: ", "green");

                    foreach (var item in Enum.GetValues(typeof(Systems.Bans.BansAnalizer.BanCategory)))
                        AnalizeBans(player.UserId, (Systems.Bans.BansAnalizer.BanCategory)item);

                    success = true;
                    return new string[] { "Done" };
                }
                else
                    return new string[] { "Player not found" };
            }
            else
                return new string[] { GetUsage() };
        }
    }
}
