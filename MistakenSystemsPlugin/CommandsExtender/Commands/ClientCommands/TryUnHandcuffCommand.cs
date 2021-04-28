using CommandSystem;
using Exiled.API.Features;
using Gamer.Mistaken.Base.GUI;
using Gamer.Utilities;
using System.Collections.Generic;

namespace Gamer.Mistaken.CommandsExtender.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.ClientCommandHandler))]
    class TryUnHandcuffCommand : IBetterCommand
    {
        public override string Description => "Try your luck";

        public override string Command => "try";

        public override string[] Aliases => new string[] { };

        internal readonly static HashSet<string> Tried = new HashSet<string>();
        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            success = false;
            var player = sender.GetPlayer();
            if (!player.IsCuffed)
                return new string[] { "Nie jesteś skuty" };
            if (Tried.Contains(player.UserId))
                return new string[] { "Możesz próbować tylko raz na życie" };
            Tried.Add(player.UserId);
            if (UnityEngine.Random.Range(1, 101) < 6 && player.Position.y < 800)
            {
                player.CufferId = -1;
                player.EnableEffect<CustomPlayerEffects.Amnesia>(10);
                player.EnableEffect<CustomPlayerEffects.Disabled>(10);
                player.EnableEffect<CustomPlayerEffects.Concussed>(10);
                player.EnableEffect<CustomPlayerEffects.Bleeding>();
                success = true;
                return new string[] { "Sukces" };
            }
            else
            {
                player.EnableEffect<CustomPlayerEffects.Amnesia>(10);
                player.EnableEffect<CustomPlayerEffects.Disabled>(15);
                player.EnableEffect<CustomPlayerEffects.Concussed>(30);
                player.EnableEffect<CustomPlayerEffects.Bleeding>();
                Player cuffer = RealPlayers.Get(player.CufferId);
                cuffer.SetGUI("try", Mistaken.Base.GUI.PseudoGUIHandler.Position.BOTTOM, $"<b>!! {player.Nickname} <color=yellow>próbował</color> się rozkuć !!</b>", 10);
                success = true;
                return new string[] { "Nie udało ci się" };
            }
        }
    }
}
