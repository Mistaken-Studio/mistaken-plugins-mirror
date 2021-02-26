using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;
using System.Text;
using UnityEngine;

using Gamer.Utilities;
using CommandSystem;
using Exiled.API.Features;

namespace Gamer.Mistaken.CommandsExtender.Commands
{
        [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))] class LockElevatorCommand : IBetterCommand, IPermissionLocked
    {
        public string Permission => "lockelevator";

        public override string Description =>
        "Allows to lock elevators";

        public string PluginName => PluginHandler.PluginName;

        public override string Command => "lockelevator";

        public override string[] Aliases => new string[] { "lelevator" };

        public string GetUsage()
        {
            return "LOCKELEVATOR [ELEVATOR] TRUE/FALSE \n Elevators:\n049\nNuke\nGateA\nGateB\nLczA\nLczB";
        }

        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            success = false;
            if (args.Length < 2) return new string[] { GetUsage() };
            if (!bool.TryParse(args[1], out bool value)) return new string[] { GetUsage() };
            success = true;
            var elevators = Map.Lifts;
            switch (args[0].ToLower())
            {
                case "049":
                    {
                        var elev = elevators.FirstOrDefault(e => e.elevatorName == "SCP-049");
                        if (elev != null)
                            elev.Network_locked = value;
                        else
                            return new string[] { "Server Error, elevator not found" };
                        return new string[] { "Done" };
                    }
                case "nuke":
                    {
                        var elev = elevators.FirstOrDefault(e => e.elevatorName == "");
                        if (elev != null)
                            elev.Network_locked = value;
                        else
                            return new string[] { "Server Error, elevator not found" };
                        return new string[] { "Done" };
                    }
                case "gatea":
                    {
                        var elev = elevators.FirstOrDefault(e => e.elevatorName == "GateA");
                        if (elev != null)
                            elev.Network_locked = value;
                        else
                            return new string[] { "Server Error, elevator not found" };
                        return new string[] { "Done" };
                    }
                case "gateb":
                    {
                        var elev = elevators.FirstOrDefault(e => e.elevatorName == "GateB");
                        if (elev != null)
                            elev.Network_locked = value;
                        else
                            return new string[] { "Server Error, elevator not found" };
                        return new string[] { "Done" };
                    }
                case "lcza":
                    {
                        var elev = elevators.FirstOrDefault(e => e.elevatorName == "ElA");
                        if (elev != null)
                            elev.Network_locked = value;
                        else
                            return new string[] { "Server Error, elevator not found" };
                        elevators.First(e => e.elevatorName == "ElA2").Network_locked = value;

                        return new string[] { "Done" };
                    }
                case "lczb":
                    {
                        var elev = elevators.FirstOrDefault(e => e.elevatorName == "ElB");
                        if (elev != null)
                            elev.Network_locked = value;
                        else
                            return new string[] { "Server Error, elevator not found" };
                        elevators.First(e => e.elevatorName == "ElB2").Network_locked = value;
                        if (elev != null)
                            elev.Network_locked = value;
                        else
                            return new string[] { "Server Error, elevator not found" };
                        return new string[] { "Done" };
                    }
                default:
                    {
                        success = false;
                        return new string[] { GetUsage() };
                    }
            }
        }
    }
}
