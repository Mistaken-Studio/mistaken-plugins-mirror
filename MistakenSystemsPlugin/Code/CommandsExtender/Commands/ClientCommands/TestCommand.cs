using CommandSystem;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Gamer.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gamer.Mistaken.CommandsExtender.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.ClientCommandHandler))] 
    class DevTestCommand : IBetterCommand
    {       
        public override string Description => "DEV STUFF";
        public override string Command => "test";
        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            success = false;
            var player = sender.GetPlayer();
            if (!player.IsDev())
                return new string[] { "This command is used for testing, allowed only for Mistaken Devs" };
            switch(args[0])
            {
                case "sound":
                    GameObject.FindObjectOfType<AmbientSoundPlayer>().RpcPlaySound(int.Parse(args[1]));
                    break;
                case "tfc":
                    player.ChangeAppearance(Player.Get(args[1]), (RoleType)sbyte.Parse(args[2]));
                    break;
                case "fc":
                    player.ChangeAppearance((RoleType)sbyte.Parse(args[1]));
                    break;
                case "nick":
                    player.TargetSetNickname(Player.Get(args[1]), args[2]);
                    break;
                case "badge":
                    player.TargetSetBadge(Player.Get(args[1]), args[2], args[3]);
                    break;
            }
            success = true;
            return new string[] { "HMM" };
        }
    }
}
