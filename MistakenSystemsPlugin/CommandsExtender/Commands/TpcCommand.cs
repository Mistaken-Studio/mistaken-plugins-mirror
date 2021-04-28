using CommandSystem;
using Gamer.Utilities;
using UnityEngine;

namespace Gamer.Mistaken.CommandsExtender.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))]
    class TpcCommand : IBetterCommand, IPermissionLocked
    {
        public string Permission => "tpc";

        public override string Description => "TPC";

        public string PluginName => PluginHandler.PluginName;

        public override string Command => "tpc";

        public override string[] Aliases => new string[] { };

        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            success = false;
            if (args.Length < 3) return new string[] { GetUsage() };

            var player = sender.GetPlayer();
            if (float.TryParse(args[0].Replace("~", ""), out float x))
            {
                if (float.TryParse(args[1].Replace("~", ""), out float y))
                {
                    if (float.TryParse(args[2].Replace("~", ""), out float z))
                    {
                        if (args[0].Contains("~"))
                            x += player.Position.x;
                        if (args[1].Contains("~"))
                            y += player.Position.y;
                        if (args[2].Contains("~"))
                            z += player.Position.z;
                        player.Position = new Vector3(x, y, z);
                        success = true;
                        return new string[] { "Done" };
                    }
                }
            }
            return new string[] { "Wrong Args" };
        }

        public string GetUsage()
        {
            return "TPC [X] [Y] [Z]";
        }
    }
}
