using CommandSystem;
using Gamer.Utilities;
using System.Linq;


namespace Gamer.Mistaken.CommandsExtender.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))]
    internal class UnBanCommand : IBetterCommand, IPermissionLocked
    {
        public string Permission => "unban";

        public override string Description => "UNBAN USERID/IP/Both";

        public string PluginName => PluginHandler.PluginName;

        public override string Command => "unban";

        public override string[] Aliases => new string[] { };

        public string GetUsage()
        {
            return "UNBAN [ID]";
        }

        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            success = false;
            if (args.Length == 0) return new string[] { GetUsage() };
            if (args[0].ToLower().Contains("@steam") || args[0].ToLower().Contains("@discord"))
            {

                foreach (var item in Systems.Logs.LogManager.PlayerLogs.ToArray())
                {
                    foreach (var player in item.Value)
                    {
                        if (player.UserId.ToLower() == args[0].ToLower())
                        {
                            BanHandler.RemoveBan(player.UserId, BanHandler.BanType.UserId);
                            BanHandler.RemoveBan(player.IP, BanHandler.BanType.IP);
                            BanHandler.ValidateBans();
                            success = true;
                            return new string[] { "Done", "Unbanned USERID: " + player.UserId, "Unbanned IP: " + player.IP };
                        }
                    }
                }
                BanHandler.RemoveBan(args[0], BanHandler.BanType.UserId);
                BanHandler.ValidateBans();
                success = true;
                return new string[] { "Done", "Unbanned USERID: " + args[0] };
            }
            else if (int.TryParse(args[0], out int Id))
            {
                foreach (var item in Systems.Logs.LogManager.PlayerLogs.ToArray())
                {
                    foreach (var player in item.Value)
                    {
                        if (player.ID == Id)
                        {
                            BanHandler.RemoveBan(player.UserId, BanHandler.BanType.UserId);
                            BanHandler.RemoveBan(player.IP, BanHandler.BanType.IP);
                            BanHandler.ValidateBans();
                            success = true;
                            return new string[] { "Done", "Unbanned USERID: " + player.UserId, "Unbanned IP: " + player.IP };
                        }
                    }
                }
                return new string[] { "Player not found in log" };
            }
            else if (args[0].Split('.').Length == 4)
            {
                foreach (var item in Systems.Logs.LogManager.PlayerLogs.ToArray())
                {
                    foreach (var player in item.Value)
                    {
                        if (player.IP == args[0])
                        {
                            BanHandler.RemoveBan(player.UserId, BanHandler.BanType.UserId);
                            BanHandler.RemoveBan(player.IP, BanHandler.BanType.IP);
                            BanHandler.ValidateBans();
                            success = true;
                            return new string[] { "Done", "Unbanned USERID: " + player.UserId, "Unbanned IP: " + player.IP };
                        }
                    }
                }
                BanHandler.RemoveBan(args[0], BanHandler.BanType.IP);
                BanHandler.ValidateBans();
                success = true;
                return new string[] { "Done", "Unbanned IP: " + args[0] };
            }
            else
            {
                return new string[] { GetUsage() };
            }
        }
    }
}
