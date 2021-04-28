using CommandSystem;
using Gamer.Utilities;
using LightContainmentZoneDecontamination;
using System.Linq;


namespace Gamer.Mistaken.CommandsExtender.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))]
    class DLCZCommand : IBetterCommand, IPermissionLocked
    {
        public string Permission => "dlcz";

        public override string Description =>
        "LCZ Decontamination";

        public string PluginName => PluginHandler.PluginName;

        public override string Command => "dlcz";

        public override string[] Aliases => new string[] { };

        public string GetUsage()
        {
            return "dlcz get/play (id) (global: true/false)";
        }

        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            success = false;
            if (args.Length == 0)
                return new string[] { GetUsage() };

            var dlcz = LightContainmentZoneDecontamination.DecontaminationController.Singleton;
            if (dlcz == null)
                return new string[] { "DecontaminationLCZ not found" };

            switch (args[0].ToLower())
            {
                case "g":
                case "get":
                    return new string[] { "Current Id: " + (dlcz._nextPhase - 1) };

                case "st":
                case "settime":
                    {
                        if (args.Length == 1)
                            return new string[] { GetUsage() };
                        if (float.TryParse(args[1], out float time))
                        {
                            double last = dlcz.DecontaminationPhases.First(i => i.Function == DecontaminationController.DecontaminationPhase.PhaseFunction.Final).TimeTrigger - DecontaminationController.GetServerTime;
                            double toSet = Mirror.NetworkTime.time -
                            DecontaminationController.Singleton.DecontaminationPhases.First(i =>
                            i.Function == DecontaminationController.DecontaminationPhase.PhaseFunction.Final).TimeTrigger +
                            time;
                            if (toSet <= 0)
                            {
                                success = false;
                                return new string[] { $"NetworkRoundStartTime can't be negative | it whould be {toSet}" };
                            }
                            dlcz.NetworkRoundStartTime = toSet;
                            success = true;
                            return new string[] { $"Time set to {time}, it was {last}" };
                        }
                        else
                            return new string[] { GetUsage() };
                    }
                case "ss":
                case "setstatus":
                    {
                        if (args.Length == 1)
                            return new string[] { "LCZ Decontamination Status: " + dlcz.enabled };
                        if (!bool.TryParse(args[1], out bool value))
                            return new string[] { GetUsage() };
                        dlcz.disableDecontamination = value;
                        success = true;
                        if (value)
                            return new string[] { $"Resumed Decontamination" };
                        else
                            return new string[] { $"Paused Decontamination" };
                    }
                default:
                    return new string[] { GetUsage() };
            }
        }
    }
}
