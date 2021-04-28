using CommandSystem;
using Exiled.API.Features;
using Gamer.Mistaken.Systems.End;
using Gamer.Utilities;

namespace Gamer.Mistaken.CommandsExtender.Commands
{
    [System.Obsolete]
    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))]
    internal class AutoMessageCommand : IBetterCommand, IPermissionLocked
    {
        public string GetUsage()
        {
            return "AutoMessage [TYPE]\nTypes:\n- discord\n- whitelist\n- team_scp\n- suicide_scp\n- door (Zamykanie drzwi swoim = ban)\n- teamkill \n DEPRECATED";
        }

        public override string Command => "autoMessage";

        public override string Description => "Sends Auto Message | This command will be removed soon TM";

        public string Permission => "AutoMessage";

        public string PluginName => PluginHandler.PluginName;

        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            success = false;
            if (args.Length == 0)
                return new string[] { GetUsage() };
            string message;
            switch (args[0].ToLower())
            {
                case "discord":
                    {
                        message = RandomMessagesHandler.Messages[0];
                        break;
                    }
                case "wl":
                case "whitelist":
                    {
                        message = RandomMessagesHandler.Messages[1];
                        break;
                    }
                case "team_scp":
                    {
                        message = RandomMessagesHandler.Messages[2];
                        break;
                    }
                case "suicide_scp":
                    {
                        message = RandomMessagesHandler.Messages[3];
                        break;
                    }
                case "door":
                    {
                        message = RandomMessagesHandler.Messages[4];
                        break;
                    }
                case "teamkill":
                case "tk":
                    {
                        message = RandomMessagesHandler.Messages[5];
                        break;
                    }
                default:
                    {
                        return new string[] { GetUsage() };
                    }
            }
            if (message == "")
                return new string[] { "Unknown Error" };
            success = true;
            Map.Broadcast(20, message);
            return new string[] { "Done" };
        }
    }
}
