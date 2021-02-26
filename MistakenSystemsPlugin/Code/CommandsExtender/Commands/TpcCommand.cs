


using CommandSystem;
using Gamer.Utilities;
using System.Linq;
using UnityEngine;


namespace Gamer.Mistaken.CommandsExtender.Commands
{
        [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))] class TpcCommand : IBetterCommand, IPermissionLocked
    {
       

        public string Permission => "tpc";

        public override string Description =>
         "TPC";

        public string PluginName => PluginHandler.PluginName;

        public override string Command => "tpc";

        public override string[] Aliases => new string[] { };

        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            success = false;
            if (args.Length < 3) return new string[] { GetUsage() };

            var player = sender.GetPlayer();
            float x, y, z;
            if (float.TryParse(args[0].Replace("~", ""), out x))
            {
                if (float.TryParse(args[1].Replace("~", ""), out y))
                {
                    if (float.TryParse(args[2].Replace("~", ""), out z))
                    {
                        if (args[0].Contains("~"))
                        {
                            x += player.Position.x;
                        }
                        if (args[1].Contains("~"))
                        {
                            y += player.Position.y;
                        }
                        if (args[2].Contains("~"))
                        {
                            z += player.Position.z;
                        }
                        var target = new Vector3(x, y, z);
                        if (args.Length > 2 && bool.TryParse(args[3], out bool portal) && portal)
                        {
                            player.Do106Teleport(target);
                        }
                        else
                            player.Position = target;
                        success = true;
                        return new string[] { "Done" };
                    }
                }
            }
            return new string[] { "Wrong Args" };
        }

        public string GetUsage()
        {
            return "TPC [X] [Y] [Z] (Portal true/false (default false))";
        }
    }
}
