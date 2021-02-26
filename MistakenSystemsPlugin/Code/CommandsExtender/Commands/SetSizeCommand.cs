using Grenades;
using Mirror;

using UnityEngine;


using Gamer.Utilities;
using CommandSystem;

namespace Gamer.Mistaken.CommandsExtender.Commands
{
        [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))] class SetSizeCommand : IBetterCommand, IPermissionLocked
    {
        public string Permission => "setsize";

        public override string Description =>
        "SetSize";
        public string PluginName => PluginHandler.PluginName;

        public override string Command => "setsize";

        public override string[] Aliases => new string[] { "ssize" };

        public override string[] Execute(ICommandSender sender, string[] args, out bool _s)
        {
            _s = false;
            if (args.Length < 4)
                return new string[] { GetUsage() };
            else
            {
                if (float.TryParse(args[1], out float x))
                {
                    if (float.TryParse(args[2], out float y))
                    {
                        if (float.TryParse(args[3], out float z))
                        {
                            var output = this.ForeachPlayer(args[0], out bool success, (player) =>
                            {
                                player.Scale = new Vector3(x, y, z);
                                return new string[] { "Done" };
                            });
                            if (!success)
                                return new string[] { "Player not found", GetUsage() };
                            _s = true;
                            return output;
                        }
                        else
                            return new string[] { GetUsage() };
                    }
                    else
                        return new string[] { GetUsage() };
                }
                else
                    return new string[] { GetUsage() };
            }
        }

        public string GetUsage()
        {
            return "SETSIZE [PLAYER ID] [X] [Y] [Z]";
        }
    }
}
