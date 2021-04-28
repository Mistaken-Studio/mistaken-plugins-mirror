using CommandSystem;
using Gamer.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace Gamer.Mistaken.Systems.Logs.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))]
    class PlayerLogCommand : IBetterCommand
    {
        public override string Description =>
         "PlayerLog";

        public override string Command => "playerlog";

        public override string[] Aliases => new string[] { "plog" };

        public string GetUsage()
        {
            return "PlayerLog [Round Id]";
        }

        public string[] GetList()
        {
            List<string> tor = NorthwoodLib.Pools.ListPool<string>.Shared.Rent();
            tor.Add(GetUsage());
            foreach (var item in LogManager.RoundStartTime.Reverse())
                tor.Add($"RoundId: {item.Key} | Time: {item.Value}");
            var torArray = tor.ToArray();
            NorthwoodLib.Pools.ListPool<string>.Shared.Return(tor);
            return torArray;
        }

        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            success = false;
            if (args.Length == 0)
                return GetList();
            else
            {
                if (int.TryParse(args[0], out int id))
                {
                    if (!LogManager.PlayerLogs.ContainsKey(id))
                        return new string[] { "PlayerLog from that round not found" };
                    List<string> tor = NorthwoodLib.Pools.ListPool<string>.Shared.Rent();
                    tor.Add("PlayerLog for round with id " + id);
                    foreach (var item in LogManager.PlayerLogs[id])
                        tor.Add($"({item.ID}) {item.Name}\n{item.UserId} | {item.IP}\n IMute: {item.IMute} | Mute: {item.Mute}\n");
                    success = true;
                    var torArray = tor.ToArray();
                    NorthwoodLib.Pools.ListPool<string>.Shared.Return(tor);
                    return torArray;
                }
                else
                    return new string[] { "Id has to be int" };
            }
        }
    }
}
