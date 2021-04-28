using CommandSystem;
using Exiled.API.Enums;
using Exiled.API.Features;
using Gamer.Utilities;
using System.Collections.Generic;

namespace Gamer.Mistaken.Systems.Logs.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))]
    internal class ElevatorLogCommand : IBetterCommand
    {
        public static HashSet<int> Active = new HashSet<int>();

        public override string Command => "elevatorlog";

        public override string[] Aliases => new string[] { "elog" };

        public override string Description => "Access elevator logs";

        public static void Execute(Player p, ElevatorType d, List<ElevatorLog> data)
        {
            Active.Remove(p.Id);
            string toWrite = $"ElevatorLog for elevator {d}";
            if (data != null)
            {
                foreach (var item in data)
                {
                    toWrite += $"\n[{item.Time:HH:mm:ss)}] {item.Status} ({item.Player.Id}) {item.Player.Nickname} | {item.Player.UserId}";
                }
            }

            p.Broadcast("ElevatorLog", 5, "Printed in console (~)", Broadcast.BroadcastFlags.AdminChat);
            Systems.Patches.RAPatch.LogCommand(p.Sender, $"elog {d}", toWrite);
            p.SendConsoleMessage(toWrite, "green");
        }

        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            success = true;
            var player = sender.GetPlayer();
            if (!Active.Contains(player.Id))
                Active.Add(player.Id);
            else
                Active.Remove(player.Id);
            if (Active.Contains(player.Id))
                return new string[] { "Activated" };
            else
                return new string[] { "Deactivated" };
        }
    }
}
