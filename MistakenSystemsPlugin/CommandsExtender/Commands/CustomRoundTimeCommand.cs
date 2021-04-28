
using CommandSystem;
using GameCore;
using Gamer.Utilities;
using System.Globalization;


namespace Gamer.Mistaken.CommandsExtender.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))]
    internal class CustomRoundTimeCommand : IBetterCommand
    {
        public override string Description =>
        "CRT";

        public override string Command => "crt";

        public override string[] Aliases => new string[] { };

        public string GetUsage()
        {
            return "CRT";
        }

        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            success = true;
            return new string[] { RoundStart.RoundLenght.ToString("hh\\:mm\\:ss\\.fff", CultureInfo.InvariantCulture) };
        }
    }
}
