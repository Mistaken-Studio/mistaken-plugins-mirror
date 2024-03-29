﻿using Gamer.Utilities;
using MistakenSocket.Client.SL;
using MistakenSocket.Shared;
using MistakenSocket.Shared.Achievements;
using MistakenSocket.Shared.API;
using System.Collections.Generic;
using System.Linq;

namespace Gamer.Mistaken.EVO
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.ClientCommandHandler))]
    internal class CommandHandler : IBetterCommand
    {
        public override string Command => "evo";

        public override string[] Aliases => new string[] { };

        public override string Description => "Evo system main command";

        public static readonly Dictionary<string, int> SetRequests = new Dictionary<string, int>();

        public override string[] Execute(CommandSystem.ICommandSender sender, string[] args, out bool success)
        {
            var player = sender.GetPlayer();
            success = false;
            if (args.Length == 0) return new string[] { "EVO get/set/refresh/info" };
            switch (args[0])
            {
                case "info":
                    {
                        SSL.Client.Send(MessageType.ACHIEVEMENT_REQUEST_INFO_ALL, new AchievementRequestAllInfo(player.UserId)).GetResponseDataCallback((result) =>
                        {
                            if (result.Type != MistakenSocket.Shared.API.ResponseType.OK)
                                return;
                            var data = result.Payload.Deserialize<AchievementResponseAllInfo>(0, 0, out _, false);
                            List<string> tor = NorthwoodLib.Pools.ListPool<string>.Shared.Rent();
                            tor.Add("EVO Ranks List: ");

                            foreach (var achievement in Handler.Achievements)
                            {
                                if (data.Info.Length == 0)
                                    break;
                                tor.Add($"{achievement.Name} - {achievement.Description}");
                                var info = data.Info.FirstOrDefault(i => i.Id == achievement.Id);
                                if (info != null)
                                    tor.Add($"Current progress - {info.Progress}");
                                else
                                    tor.Add($"Current progress - 0");
                                tor.Add($" - <color={achievement.Levels[0].Color ?? Colors.BLUE_GREEN.ToString().Replace("BLUE_GREEN", "GREEN")}>{achievement.Levels[0].Name}</color> ({achievement.Levels[0].Progress})");
                                tor.Add($" - <color={achievement.Levels[1].Color ?? Colors.CYAN.ToString()}>{achievement.Levels[1].Name}</color> ({achievement.Levels[1].Progress})");
                                tor.Add($" - <color={achievement.Levels[2].Color ?? Colors.AQUA.ToString()}>{achievement.Levels[2].Name}</color> ({achievement.Levels[2].Progress})");
                            }
                            player.SendConsoleMessage(string.Join("\n", tor), "green");
                            NorthwoodLib.Pools.ListPool<string>.Shared.Return(tor);
                        });
                        return new string[] { "Requested" };
                    }
                case "get":
                    {
                        Handler.GetAllRanks(player.UserId);
                        return new string[] { "Requested" };
                    }
                case "refresh":
                    {
                        Handler.RefreshRank(player.UserId);
                        success = true;
                        return new string[] { "Requested" };
                    }
                case "set":
                    {
                        if (args.Length > 1 && int.TryParse(args[1], out int id))
                        {
                            SetRequests.Add(player.UserId, id);
                            Handler.GetAllRanks(player.UserId);
                            Gamer.Utilities.BetterCourotines.CallDelayed(5, () =>
                            {
                                Handler.RefreshRank(player.UserId);
                            }, "EVO.RefreshPostSet");
                            success = true;
                            return new string[] { "Done" };
                        }
                        return new string[] { "evo set [id]" };
                    }
                default:
                    {
                        return new string[] { "EVO get/set/refresh/info" };
                    }
            }
        }
    }
}
