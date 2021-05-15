using CommandSystem;
using Gamer.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xname.CE
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))]
    internal class CommandHandler : IBetterCommand
    {
        /// <inheritdoc/>
        public override string Command => "CE";
        /// <inheritdoc/>
        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            success = true;
            switch (args[0])
            {
                case "PU":
                    {
                        try
                        {
                            var player = RealPlayers.Get(int.Parse(args[1]));
                            if (bool.Parse(args[2]))
                                CEHandler.PutPlayerToSleep(player);
                            else
                                CEHandler.WakeUpPlayer(player);
                            break;
                        }
                        catch
                        {
                            return new string[] { "Argument must be true/false or time must be a valid number" };
                        }
                    }
                default:
                    return GetUsage();
            }
            return new string[] { "Done" };
        }
        private string[] GetUsage()
        {
            return new string[] 
            {
                "Usage",
                "[] - required args",
                "() - optional args",
                "CE PU [PlayerId] [true/false] - if true makes player unconscious else concious"
            };
        }
    }
}
