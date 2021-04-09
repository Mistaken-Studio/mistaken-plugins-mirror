using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using RemoteAdmin;
using System.IO;
using UnityEngine.Networking;
using Gamer.EventManager.Events;
using MEC;
using CommandSystem;
using Exiled.API.Features;
using Gamer.Utilities;

namespace Gamer.EventManager
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))]
    class CommandHandler : IBetterCommand
    {
        public static CommandHandler singleton { get; set; }

        public override string Command => "eventManager";

        public override string[] Aliases => new string[] { "em" };

        public override string Description => "Event Manager :)";

        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            success = false;
            Player admin = sender.GetPlayer();
            if (args.Length == 0)
            {
                return new string[] { GetUsage() };
            }
            string cmd = args[0].ToLower();
            if (cmd == "list" || cmd == "l")
            {
                List<string> tor = new List<string>
                {
                    "Events:"
                };

                foreach (var item in EventManager.Events.ToArray())
                    tor.Add($"<color=green>{item.Value.Id}</color>: <color=yellow>{item.Value.Name}</color> <color=red>|</color> {item.Value.Description}");

                success = true;
                return tor.ToArray();
            }
            else if (cmd == "get" || cmd == "g")
            {
                success = true;
                return new string[] { "Current event: " + EventManager.ActiveEvent?.Name ?? "None" };
            }
            else if (cmd == "force" || cmd == "f")
            {
                if (!admin.CheckPermission(EventManager.singleton.Name + ".force")) return new string[] { "You can't use this command. No permission!" };
                else if (Gamer.Utilities.RealPlayers.List.Count() < 4) return new string[] { "You can't use this command. Not enough players!" };
                //if (!(admin.UserId == "76561198123437513@steam" || admin.UserId == "76561198134629649@steam")) return new string[] { "Work In Progress" };
                else if (EventManager.ActiveEvent != null) return new string[] { "You can't forcestack events" };
                var name = string.Join(" ", args.Skip(1)).ToLower();

                foreach (var item in EventManager.Events.ToArray())
                {
                    if (item.Value.Name.ToLower() == name || item.Value.Id.ToLower() == name)
                    {
                        item.Value.Initiate();
                        success = true;
                        return new string[] { $"<color=green>Activated</color> {item.Value.Name}", item.Value.Description };
                    }
                }

                return new string[] { "Event not found" };
            }
            else if (cmd == "forceend")
            {
                if (!admin.CheckPermission(EventManager.singleton.Name + ".forceend")) return new string[] { "You can't use this command. No permission!" };
                if (EventManager.ActiveEvent == null) return new string[] { "No event is on going" };
                EventManager.ActiveEvent.OnEnd($"Anulowano event: <color=#6B9ADF>{EventManager.ActiveEvent.Name}</color>", true);
                EventManager.ActiveEvent = null;
                //Utilities.API.Map.Blackout.Enabled = false;
                Round.IsLocked = false;
                RoundSummary.singleton.ForceEnd();
                ////todsc.ForceEnd(plugin, admin, cevent);
                success = true;
                return new string[] { "Done" };
            }
            else if (cmd == "setrwevent")
            {
                if (!admin.CheckPermission(EventManager.singleton.Name + ".setrwevent")) return new string[] { "You can't use this command. No permission!" };
                if (args.Length == 1 || args[1] == "") return new string[] { "Wrong args", "EventManager setrwevent [amount]" };
                if (int.Parse(args[1]) < 0) return new string[] { "Number has to be non-negative int" };
                EventManager.rounds_without_event = int.Parse(args[1]);
                success = true;
                return new string[] { "Done" };
            }
            else if (cmd == "rwe")
            {
                success = true;
                return new string[] { "Rounds Without Event:" + EventManager.rounds_without_event };
            }
            else
            {
                return new string[] { "Unknown Command", GetUsage() };
            }
        }

        public string GetUsage()
        {
            return
                "\n () = optional argument" +
                "\n [] = required argument" +
                "\n EventManager list" +
                "\n EventManager force [event name]" +
                "\n EventManager forceend" +
                "\n EventManager get"
                ;
        }
    }
}
