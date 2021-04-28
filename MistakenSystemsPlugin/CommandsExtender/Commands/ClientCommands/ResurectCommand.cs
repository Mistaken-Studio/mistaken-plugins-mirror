using CommandSystem;
using Exiled.API.Features;
using Gamer.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace Gamer.Mistaken.CommandsExtender.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.ClientCommandHandler))] 
    class ResurectCommand : IBetterCommand
    {       
        public override string Description => "Resurection";

        public override string Command => "u500";

        public override string[] Aliases => new string[] { };

        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            success = false;
            var player = sender.GetPlayer();
            if (player.CurrentItem.id != ItemType.SCP500)
                return new string[] { "Nie masz SCP 500 w ręce" };
            if(!Systems.Misc.ResurectionHandler.Resurect(sender.GetPlayer()))
                return new string[] { "Nie udało się nikogo wskrzsić" };
            success = true;
            return new string[] { "Rozpoczynam" };
        }
    }
}
