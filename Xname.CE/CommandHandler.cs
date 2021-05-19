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
            if (args.Length != 0)
            {
                switch (args[0])
                {
                    case "PU":
                        {

                            var player = RealPlayers.Get(int.Parse(args[1]));
                            if (player != null)
                            {
                                if (player.IsUnconscious())
                                    player.WakeUpPlayer();
                                else
                                    player.PutPlayerToSleep();
                            }
                            else
                                return new string[] { "Player not found" };
                            break;
                        }
                    default:
                        return GetUsage();
                }
                return new string[] { "Done" };
            }
            else
                return GetUsage();
        }
        private string[] GetUsage()
        {
            return new string[] 
            {
                "Usage",
                "[] - required args",
                "() - optional args",
                "CE PU [PlayerId] - makes player unconscious if concious and in reverse"
            };
        }
    }
}
