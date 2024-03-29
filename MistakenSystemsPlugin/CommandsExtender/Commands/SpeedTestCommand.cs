﻿using CommandSystem;
using Gamer.Utilities;


namespace Gamer.Mistaken.CommandsExtender.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))]
    internal class SpeedTestCommand : IBetterCommand, IPermissionLocked
    {
        public string Permission => "speed";

        public override string Description =>
            "Test speed";

        public string PluginName => PluginHandler.PluginName;

        public override string Command => "speed";

        public override string[] Aliases => new string[] { };

        public string GetUsage()
        {
            return "Speed [Id] [Walk Speed] [Run Speed]";
        }

        public override string[] Execute(ICommandSender sender, string[] args, out bool _s)
        {
            _s = false;
            if (args.Length < 3)
                return new string[] { GetUsage() };
            if (!float.TryParse(args[1], out float walkSpeed))
            {
                if (args[1] == "~")
                    walkSpeed = 1.2f;
                else
                    return new string[] { "Wrong walk speed", GetUsage() };
            }
            if (!float.TryParse(args[2], out float runSpeed))
            {
                if (args[2] == "~")
                    runSpeed = 1.05f;
                else
                    return new string[] { "Wrong run speed", GetUsage() };
            }
            var success = ForeachPlayer(args[0], (p) =>
            {
                p.SetSpeed(walkSpeed, runSpeed);
                //Systems.Misc.CustomSpeedHandler.SetSpeeds(p, walkSpeed, runSpeed);
            });
            if (success == false)
                return new string[] { "Player not found" };
            _s = true;
            return new string[] { "Done" };
        }
    }
}
