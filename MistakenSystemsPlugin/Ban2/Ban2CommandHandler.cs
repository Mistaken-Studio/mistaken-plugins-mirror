using CommandSystem;
using Exiled.API.Features;
using Gamer.Utilities;
using MEC;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gamer.Mistaken.Ban2
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class Ban2Command : IBetterCommand
    {
        public override string Command => "ban2";

        public override string[] Aliases => new string[] { };

        public override string Description => "Good old ban2";

        public static List<BanData> AwaitingBans = new List<BanData>();

        private int duration;

        public static void CompleteBan(CommandSender senderPlayer, string reason, Player target, int duration, string textTime, bool loud)
        {
            Server.BanPlayer.BanUser(target.GameObject, duration * 60, $"[{textTime}] {reason}", senderPlayer?.SenderId ?? "UNKNOWN", false);
            if (loud)
                Map.Broadcast(5, target?.Nickname + " has been <color=red>banned</color> from this server", Broadcast.BroadcastFlags.Normal);
        }

        public static void PrepareStyleBan(Player target, int baseDur, ref string type, out string nickname, out string userId, out string Ip)
        {
            if (baseDur != 1)
                type += "s";
            if (target == null)
            {
                nickname = "ERROR";
                userId = "ERROR";
                Ip = "ERROR";
            }
            else
            {
                nickname = target.Nickname;
                userId = target.UserId;
                Ip = target.IPAddress;
            }
        }

        public static string[] StyleBanConfirm(int PlayerID, Player target, int baseDur, int duration, string type, string reason, string confirmReason)
        {
            PrepareStyleBan(target, baseDur, ref type, out string nick, out string userId, out string ip);

            return new string[] {
                "",
                "<color=blue>======= BANNING FINAL STEP =======</color>",
                "ID: " + PlayerID,
                "Nickname: " + nick,
                "SteamID64: " +userId,
                "IpAddress: " + ip,
                "Time : " + baseDur + " "+type+"(RAW: "+duration+")",
                "Reason:" + reason.ToString(),
                "",
                $"<color=red>Confirmation Request Reason: {confirmReason}</color>",
                "",
                "Type \"CONFIRM\" to ban player or wait 15 seconds to cancel",
                "<color=blue>======= BANNING FINAL STEP =======</color>"
            };
        }

        public static string[] StyleBan(int PlayerID, Player target, int baseDur, int duration, string type, string reason)
        {
            PrepareStyleBan(target, baseDur, ref type, out string nick, out string userId, out string ip);

            return new string[] {
                "",
                "<color=green>======= BANNING CONFIRMATION =======</color>",
                "ID: " + PlayerID,
                "Nickname: " + nick,
                "SteamID64: " +userId,
                "IpAddress: " + ip,
                "Time : " + baseDur + " "+type+"(RAW: "+duration+")",
                "Reason:" + reason.ToString(),
                "<color=green>======= BANNING CONFIRMATION =======</color>"
            };
        }

        public string GetUsage()
        {
            return "BAN2 [ID] [DURATION] [REASON]";
        }

        public void GetDurationAndReason(int dur, string[] args, out int duration, out string type, out string reason, out string textTime, out bool loud)
        {
            type = "";
            duration = dur;
            if (args[1].Contains("m") && !args[1].Contains("mo")) { args[1] = args[1].Replace("m", null); duration *= 1; type = "minute"; }
            if (args[1].Contains("h")) { args[1].Replace("h", null); duration *= 60; type = "hour"; }
            if (args[1].Contains("d")) { args[1].Replace("d", null); duration *= 1440; type = "day"; }
            if (args[1].Contains("w")) { args[1].Replace("w", null); duration *= 10080; type = "week"; }
            if (args[1].Contains("mo")) { args[1].Replace("mo", null); duration *= 43200; type = "month"; }
            if (args[1].Contains("y")) { args[1].Replace("y", null); duration *= 518400; type = "year"; }

            if (type == "") type = "minute";
            reason = "";
            loud = false;
            foreach (string s in args.Skip(2))
            {
                if (s.ToLower() == "-loud") loud = true;
                if (s.Trim().StartsWith("-") == false)
                {
                    reason += " " + s;
                }
            }

            textTime = $"BAN: {dur} {type}";
            if (dur == 0)
                textTime = "KICK";
            else if (dur != 1)
                textTime += "s";
        }

        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            success = false;
            if (args.Length > 2)
            {
                try
                {
                    var Sender = sender as CommandSender;
                    if (!int.TryParse(args[0], out int pid))
                        return new string[] { "Failed to parse playerId to int32" };
                    var target = RealPlayers.Get(pid);
                    if (target == null)
                        return new string[] { "Player not found" };

                    if (Sender.IsPlayer())
                    {
                        var Admin = Sender.GetPlayer();

                        if (Admin.ReferenceHub.serverRoles.KickPower <= target.ReferenceHub.serverRoles.Group?.RequiredKickPower && false)
                            return new string[] { $"Access denied | Too low kickpower | {Admin.ReferenceHub.serverRoles.KickPower} <= {target.ReferenceHub.serverRoles.Group?.RequiredKickPower}" };
                        if (int.TryParse(args[1].ToLower().Replace("mo", "").Trim(new char[] { 'm', 'h', 'd', 'w', 'y' }), out int value))
                            duration = value;
                        else
                            return new string[] { "Detected error with number convertion", "Failed to convert |" + args[1].ToLower().Replace("mo", "").Trim(new char[] { 'm', 'h', 'd', 'w', 'y' }) + "| to Int32" };

                    }
                    var baseDur = duration;
                    GetDurationAndReason(duration, args, out duration, out string type, out string reason, out string textTime, out bool loud);

                    bool requireConfirmation = false;
                    string confirmationReason = "Error";
                    if (((float)duration / 1440) >= 1)
                    {
                        requireConfirmation = true;
                        confirmationReason = "Ban is longer than 1 day";
                    }
                    var suggestedLength = Systems.Bans.BansAnalizer.GuessBanDuration(Systems.Bans.BansManager.GetBans(target.UserId), Systems.Bans.BansAnalizer.GetBanCategory(reason));
                    var durrationCategory = Systems.Bans.BansAnalizer.GetDurationCategory(duration);
                    var suggestedLengthCategory = Systems.Bans.BansAnalizer.GetDurationCategory(suggestedLength);
                    if (suggestedLengthCategory != durrationCategory)
                    {
                        requireConfirmation = true;
                        confirmationReason = $"Guessed ban duration is diffrent, {durrationCategory} => {suggestedLengthCategory}";
                    }

                    success = true;
                    if (!requireConfirmation)
                    {
                        CompleteBan(Sender, reason, target, duration, textTime, loud);
                        return StyleBan(pid, target, baseDur, duration, type, reason);
                    }
                    else
                    {
                        BanData data = new BanData(Sender, reason, target, duration, baseDur, textTime, loud);
                        AwaitingBans.Add(data);
                        Timing.CallDelayed(15, () => AwaitingBans.Remove(data));

                        return StyleBanConfirm(pid, target, baseDur, duration, type, reason, confirmationReason);
                    }

                }
                catch (Exception e)
                {
                    return new string[] { e.Message, e.StackTrace, e.InnerException.ToString(), e.Source };
                }
            }
            else
                return new string[] { GetUsage() };
        }

        public class BanData
        {
            public CommandSender Admin;
            public string Reason;
            public Player Target;
            public int Duration;
            public int BaseDur;
            public string TextTime;
            public bool isLoud;

            public BanData(CommandSender admin, string reason, Player target, int duration, int baseDur, string textTime, bool loud = false)
            {
                Admin = admin;
                Reason = reason;
                Target = target;
                Duration = duration;
                BaseDur = baseDur;
                TextTime = textTime;
                isLoud = loud;
            }

            public void Execute()
            {
                CompleteBan(Admin, Reason, Target, Duration, TextTime, isLoud);
                AwaitingBans.Remove(this);
            }
        }
    }

    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class ConfirmCommand : IBetterCommand
    {
        public override string Command => "confirm";

        public override string[] Aliases => new string[] { };

        public override string Description => "Let's confirm some bans";

        public string GetUsage()
        {
            return "CONFIRM";
        }

        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            success = false;
            Log.Debug("Confirming ban");
            var Sender = sender as CommandSender;

            Player senderPlayer = sender.GetPlayer();
            bool bc = args.Length != 0 && args[0].ToLower().Trim() == "-bc";
            if (senderPlayer == null)
                bc = false;
            foreach (Ban2Command.BanData item in Ban2Command.AwaitingBans.ToArray())
            {
                if (item.Admin?.SenderId == Sender.SenderId)
                {
                    item.Execute();
                    if (bc)
                    {
                        senderPlayer.ClearBroadcasts();
                        senderPlayer.Broadcast(10, "<color=green>Ban Execution Confirmed</color>");
                    }
                    success = true;
                    return Ban2Command.StyleBan(item.Target?.Id ?? -1, item.Target, item.BaseDur, item.Duration, item.TextTime, item.Reason);
                }
            }
            if (bc)
            {
                senderPlayer.ClearBroadcasts();
                senderPlayer.Broadcast(10, "<color=red>No ban found</color>");
            }
            else
                Log.Debug(args.Length + "|" + string.Join(" ", args));
            return new string[] { "No Ban Found" };
        }
    }
}
