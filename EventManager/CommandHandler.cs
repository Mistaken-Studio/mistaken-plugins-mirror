using CommandSystem;
using Exiled.API.Features;
using Gamer.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gamer.EventManager
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))]
    internal class CommandHandler : IBetterCommand
    {
        public static CommandHandler Singleton { get; set; }

        public override string Command => "eventManager";

        public override string[] Aliases => new string[] { "em" };

        public override string Description => "Event Manager :)";

        private Dictionary<string, Func<Player, string[], (bool isSuccess, string[] message)>> subcommands = new Dictionary<string, Func<Player, string[], (bool, string[])>>()
        {
            {"q", (ply,args) => QueueCommand(ply, args) },
            {"queue", (ply,args) => QueueCommand(ply, args) },
            {"rwe", (ply,args) =>{ return (true, new string[] { "Rounds Without Event:" + EventManager.rounds_without_event }); } },
            {"g", (ply,args) => { return (true, new string[] { "Current event: " + EventManager.ActiveEvent?.Name ?? "None" }); } },
            {"get", (ply,args) => { return (true, new string[] { "Current event: " + EventManager.ActiveEvent?.Name ?? "None" }); } },
            {"l", (ply, args) => ListCommand(ply, args) },
            {"list", (ply, args) => ListCommand(ply, args) },
            {"f", (ply, args) => ForceCommand(ply, args) },
            {"force", (ply, args) => ForceCommand(ply, args) },
            {"forceend", (ply, args) => ForceEndCommand(ply, args) },
            {"setrwevent", (ply, args) => SetRWEventCommand(ply,args) }
        };
        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            success = false;
            Player admin = sender.GetPlayer();
            subcommands.TryGetValue("q", out var k);
            k.Invoke(admin, args.Skip(1).ToArray());
            if (args.Length == 0)
            {
                return new string[] { GetUsage() };
            }
            string cmd = args[0].ToLower();
            if (subcommands.TryGetValue(cmd, out var commandHandler))
            {
                var retuned = commandHandler.Invoke(admin, args.Skip(1).ToArray());
                success = retuned.isSuccess;
                return retuned.message;
            }

            /*
                if (cmd == "list" || cmd == "l") //done
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
                else if (cmd == "get" || cmd == "g") //done
                {
                    success = true;
                    return new string[] { "Current event: " + EventManager.ActiveEvent?.Name ?? "None" };
                }
                else if (cmd == "force" || cmd == "f") //done
                {
                    if (!admin.CheckPermission(EventManager.singleton.Name + ".force")) return new string[] { "You can't use this command. No permission!" };
                    else if (Gamer.Utilities.RealPlayers.List.Count() < 4 && !EventManager.DNPN) return new string[] { "You can't use this command. Not enough players!" };
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
                else if(cmd == "queue" || cmd == "q") //done
                {
                    if (!admin.CheckPermission(EventManager.singleton.Name + ".force")) return new string[] { "You can't use this command. No permission!" };
                    var name = string.Join(" ", args.Skip(1)).ToLower();

                    foreach (var item in EventManager.Events.ToArray())
                    {
                        if (item.Value.Name.ToLower() == name || item.Value.Id.ToLower() == name)
                        {
                            EventManager.singleton.EventQueue.Enqueue(item.Value);
                            success = true;
                            return new string[] { $"<color=green>Enqueued</color> {item.Value.Name}", item.Value.Description };
                        }
                    }

                    return new string[] { "Event not found" };
                }
                else if (cmd == "forceend") //done
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
                else if (cmd == "setrwevent") //done
                {
                    if (!admin.CheckPermission(EventManager.singleton.Name + ".setrwevent")) return new string[] { "You can't use this command. No permission!" };
                    if (args.Length == 1 || args[1] == "") return new string[] { "Wrong args", "EventManager setrwevent [amount]" };
                    if (int.Parse(args[1]) < 0) return new string[] { "Number has to be non-negative int" };
                    EventManager.rounds_without_event = int.Parse(args[1]);
                    success = true;
                    return new string[] { "Done" };
                }
                else if (cmd == "rwe") //done
                {
                    success = true;
                    return new string[] { "Rounds Without Event:" + EventManager.rounds_without_event };
                }
            */
            else
            {
                return new string[] { "Unknown Command", GetUsage() };
            }
        }
        public static (bool, string[]) SetRWEventCommand(Player admin, string[] args)
        {
            if (!admin.CheckPermission(EventManager.singleton.Name + ".setrwevent")) return (false, new string[] { "You can't use this command. No permission!" });
            if (args.Length == 1 || args[1] == "") return (false, new string[] { "Wrong args", "EventManager setrwevent [amount]" });
            if (int.Parse(args[1]) < 0) return (false, new string[] { "Number has to be non-negative int" });
            EventManager.rounds_without_event = int.Parse(args[1]);
            return (true, new string[] { "Done" });
        }
        public static (bool, string[]) ForceEndCommand(Player admin, string[] args)
        {
            if (!admin.CheckPermission(EventManager.singleton.Name + ".forceend")) return (false, new string[] { "You can't use this command. No permission!" });
            if (EventManager.ActiveEvent == null) return (false, new string[] { "No event is on going" });
            EventManager.ActiveEvent.OnEnd($"Anulowano event: <color=#6B9ADF>{EventManager.ActiveEvent.Name}</color>", true);
            EventManager.ActiveEvent = null;
            //Utilities.API.Map.Blackout.Enabled = false;
            Round.IsLocked = false;
            RoundSummary.singleton.ForceEnd();
            ////todsc.ForceEnd(plugin, admin, cevent);
            return (true, new string[] { "Done" });
        }
        public static (bool, string[]) ForceCommand(Player admin, string[] args)
        {
            if (!admin.CheckPermission(EventManager.singleton.Name + ".force")) return (false, new string[] { "You can't use this command. No permission!" });
            else if (Gamer.Utilities.RealPlayers.List.Count() < 4 && !EventManager.DNPN) return (false, new string[] { "You can't use this command. Not enough players!" });
            else if (EventManager.ActiveEvent != null) return (false, new string[] { "You can't forcestack events" });
            var name = string.Join(" ", args).ToLower();

            foreach (var item in EventManager.Events.ToArray())
            {
                if (item.Value.Name.ToLower() == name || item.Value.Id.ToLower() == name)
                {
                    item.Value.Initiate();

                    return (true, new string[] { $"<color=green>Activated</color> {item.Value.Name}", item.Value.Description });
                }
            }

            return (false, new string[] { "Event not found" });
        }
        public static (bool, string[]) ListCommand(Player admin, string[] args)
        {
            List<string> tor = new List<string>
                {
                    "Events:"
                };

            foreach (var item in EventManager.Events.ToArray())
                tor.Add($"<color=green>{item.Value.Id}</color>: <color=yellow>{item.Value.Name}</color> <color=red>|</color> {item.Value.Description}");


            return (true, tor.ToArray());
        }
        public static (bool, string[]) QueueCommand(Player admin, string[] args)
        {
            if (!admin.CheckPermission(EventManager.singleton.Name + ".force")) return (false, new string[] { "You can't use this command. No permission!" });
            if (args.Length == 0)
            {
                var t = new List<string>();
                foreach (var ev in EventManager.singleton.EventQueue)
                {
                    t.Add(ev.Name);
                }
                return (true, t.ToArray());
            }
            else
            {
                var name = string.Join(" ", args).ToLower();

                foreach (var item in EventManager.Events.ToArray())
                {
                    if (item.Value.Name.ToLower() == name || item.Value.Id.ToLower() == name)
                    {
                        EventManager.singleton.EventQueue.Enqueue(item.Value);
                        return (true, new string[] { $"<color=green>Enqueued</color> {item.Value.Name}", item.Value.Description });
                    }
                }

                return (false, new string[] { "Event not found" });
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
