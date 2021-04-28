using CommandSystem;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Gamer.Utilities;
using System.Collections.Generic;

namespace Gamer.Mistaken.Systems.Logs.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))]
    class DoorLogCommand : IBetterCommand
    {
        public static HashSet<int> Active = new HashSet<int>();

        public override string Command => "doorlog";

        public override string[] Aliases => new string[] { "dlog" };

        public override string Description => "Access door logs";

        public static void Execute(Player p, Interactables.Interobjects.DoorUtils.DoorVariant d, List<DoorLog> data)
        {
            Active.Remove(p.Id);
            string toWrite = "DoorLog for door " + (d.name == "" ? "without name" : d.name) + " | " + d.Type();
            if (data != null)
            {
                foreach (var item in data)
                {
                    toWrite += $"\n[{item.Time.ToString("HH:mm:ss")}] {(item.Open ? "OPEN" : "CLOSE")} ({item.Player.Id}) {item.Player.Nickname} | {item.Player.UserId}";
                }
            }

            p.Broadcast("DoorLog", 5, "Printed in console (~)", Broadcast.BroadcastFlags.AdminChat);
            Systems.Patches.RAPatch.LogCommand(p.Sender, $"dlog {(d.Type().ToString() == "" ? "NO_NAME" : d.Type().ToString())} {d.transform.position}", toWrite);
            p.SendConsoleMessage(toWrite, "green");
        }

        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            //success = false;
            //return new string[] { "How about <b><color=red>no</color></b>" };
            success = true;
            var player = sender.GetPlayer();
            if (!Active.Contains(player.Id)) Active.Add(player.Id);
            else Active.Remove(player.Id);
            if (Active.Contains(player.Id)) return new string[] { "Activated" };
            else return new string[] { "Deactivated" };
        }
    }
}
